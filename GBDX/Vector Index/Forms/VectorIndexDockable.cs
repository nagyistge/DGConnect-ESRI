// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndexDockable.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//   
//          http://www.apache.org/licenses/LICENSE-2.0
//   
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx.Vector_Index.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Windows.Forms;
    using System.Linq;

    using AnswerFactory;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using GbdxTools;

    using NetworkConnections;

    using RestSharp;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;
    using FileStream = System.IO.FileStream;
    using Path = System.IO.Path;

    /// <summary>
    ///     Designer class of the dock able window add-in. It contains user interfaces that
    ///     make up the dockable window.
    /// </summary>
    public partial class VectorIndexDockable : UserControl
    {
        private delegate void AddLayerToMapDelegate(string tableName, string layerName);

        private delegate void UpdateTreeGeometries(
            IRestResponse<SourceTypeResponseObject> resp,
            VectorIndexSourceNode source);

        private delegate void UpdateTreeSources(IRestResponse<SourceTypeResponseObject> resp);

        private delegate void UpdateTreeTypes(IRestResponse<SourceTypeResponseObject> resp, VectorIndexGeometryNode geom);

        private delegate void UpdateStatusText(TreeNode node, string message);

        #region Fields & Properties

        /// <summary>
        ///     Max number of attempts before an error message is displayed.
        /// </summary>
        private const int MaxAttempts = 5;

        /// <summary>
        ///     Random number generator meant to create a new state number for certain kinds of events.
        /// </summary>
        private readonly Random applicationStateGenerator = new Random(DateTime.UtcNow.Millisecond);

        /// <summary>
        ///     HashSet containing all of the checked nodes  This acts as a check to make sure the same node isn't downloaded
        ///     twice.
        /// </summary>
        private readonly HashSet<TreeNode> checkedNodes = new HashSet<TreeNode>();

        private string Aoi = string.Empty;

        /// <summary>
        ///     The IElement containing the visual of the bounding box displayed on Arc Map
        /// </summary>
        private IElement boundingBoxGraphicElement;

        /// <summary>
        ///     Current application state.  This state will change if the user changes MXDs, closes and re-opens the UVI, and other
        ///     events of that nature.
        /// </summary>
        private int currentApplicationState;

        /// <summary>
        ///     Originally selected esri item.  Once the Vector index tool has drawn it's bounding box this tool will be
        ///     re-selected.
        /// </summary>
        private ICommandItem originallySelectedItem;

        private string query = string.Empty;
        
        private string token = string.Empty;

        /// <summary>
        ///     If we are using a query instead of the usual tree traversal method for the UVI
        /// </summary>
        private bool usingQuerySource;

        /// <summary>
        ///     Gets or sets the Host object of the dockable window
        /// </summary>
        private object Hook { get; set; }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="VectorIndexDockable" /> class.
        ///     Default constructor.
        /// </summary>
        public VectorIndexDockable()
        {
            this.treeView1.AfterCheck += this.EventHandlerTreeViewAfterCheck;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VectorIndexDockable" /> class.
        /// </summary>
        /// <param name="hook">
        ///     The hook.
        /// </param>
        public VectorIndexDockable(object hook)
        {
            this.InitializeComponent();
            this.GetAuthenticationToken();

            this.Hook = hook;
            this.textBoxSearch.Text = GbdxResources.EnterSearchTerms;
            this.textBoxSearch.ForeColor = Color.DarkGray;
            this.treeView1.AfterCheck += this.EventHandlerTreeViewAfterCheck;
            this.VisibleChanged += this.EventHandlerVectorIndexDockableVisibleChanged;
            this.textBoxSearch.LostFocus += this.EventHandlerTextBoxSearchLeave;
            this.textBoxSearch.GotFocus += this.EventHandlerTextBoxSearchEnter;

            this.textBoxSearch.KeyUp += this.EventHandlerTextBoxSearchKeyUp;
            this.textBoxSearch.Invalidate();

            ArcMap.Events.NewDocument += this.ResetVectorServices;
            ArcMap.Events.OpenDocument += this.ResetVectorServices;
            ArcMap.Events.CloseDocument += this.ResetVectorServices;
            this.currentApplicationState = this.applicationStateGenerator.Next();

            this.aoiTypeComboBox.SelectedIndex = 0;
            this.ActiveControl = this.treeView1;
        }

        private static void AddLayerToMap(string tableName, string layerName)
        {
            try
            {
                lock (Jarvis.FeatureClassLockerObject)
                {
                    var featureWorkspace = (IFeatureWorkspace)Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                    var featureClass = featureWorkspace.OpenFeatureClass(tableName);
                    ILayer featureLayer;
                    featureLayer = VectorIndexHelper.CreateFeatureLayer(featureClass, layerName);
                    VectorIndexHelper.AddFeatureLayerToMap(featureLayer);
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        /// <summary>
        ///     The create context menu strip.
        /// </summary>
        /// <param name="node">
        ///     The node.
        /// </param>
        /// <returns>
        ///     The <see cref="UviContextMenuStrip" />.
        /// </returns>
        private UviContextMenuStrip CreateContextMenuStrip(TreeNode node)
        {
            var menuStrip = new UviContextMenuStrip(node);
            var downloadAllToolStrip = new ToolStripMenuItem
                                           {
                                               Text = GbdxResources.Download_all_Below_100000,
                                               Name = "DownloadAll"
                                           };
            downloadAllToolStrip.Click += this.DownloadAllToolStripOnClick;
            menuStrip.Items.Add(downloadAllToolStrip);

            return menuStrip;
        }

        /// <summary>
        ///     The download all tool strip on click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void DownloadAllToolStripOnClick(object sender, EventArgs e)
        {
            var item = (ToolStripDropDownItem)sender;
            var owner = (UviContextMenuStrip)item.Owner;
            owner.AttachedTreeNode.Nodes.GetEnumerator();
            if (this.usingQuerySource)
            {
                for (var i = 0; i <= owner.AttachedTreeNode.Nodes.Count - 1; i++)
                {
                    var node = (VectorIndexGeometryNode)owner.AttachedTreeNode.Nodes[i];
                    if (node.Source.Count <= 100000)
                    {
                        node.Checked = true;
                    }
                }
            }
            else
            {
                for (var i = 0; i <= owner.AttachedTreeNode.Nodes.Count - 1; i++)
                {
                    var node = (VectorIndexTypeNode)owner.AttachedTreeNode.Nodes[i];
                    if (node.Type.Count <= 100000)
                    {
                        node.Checked = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Event handler for when the clear button is clicked.
        /// </summary>
        /// <param name="sender">
        ///     the button that is clicked
        /// </param>
        /// <param name="e">
        ///     event arguments that get sent by the button
        /// </param>
        private void EventHandlerClearButtonClick(object sender, EventArgs e)
        {
            // Clear any current drawn images
            if (this.boundingBoxGraphicElement != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(
                    ArcMap.Document.ActivatedView,
                    this.boundingBoxGraphicElement);
                this.boundingBoxGraphicElement = null;
            }

            // Clear Treeview
            this.treeView1.Nodes.Clear();
            this.textBoxSearch.Clear();
            this.currentApplicationState = this.applicationStateGenerator.Next();
        }

        /// <summary>
        ///     Event handler for when the select area button is clicked.
        /// </summary>
        /// <param name="sender">
        ///     the button that is clicked
        /// </param>
        /// <param name="e">
        ///     event arguments that get sent by the button
        /// </param>
        private void EventHandlerSelectAreaButtonClick(object sender, EventArgs e)
        {
            // Draw Rectangle option
            if (this.aoiTypeComboBox.SelectedIndex == 0)
            {
                // Reset this variable to allow the user to differentiate between normal and query search.
                this.usingQuerySource = false;
                this.treeView1.CheckBoxes = true;

                if (ArcMap.Application.CurrentTool.Name != "DigitalGlobe_Inc_sma_VectorIndex")
                {
                    // Unsubscribe from any previous events.  Prevents awesome mode
                    VectorIndexRelay.Instance.PolygonHasBeenSet -= this.InstancePolygonHasBeenSet;

                    // Subscribe to the event
                    VectorIndexRelay.Instance.PolygonHasBeenSet += this.InstancePolygonHasBeenSet;

                    // Clear any current drawn images
                    if (this.boundingBoxGraphicElement != null)
                    {
                        ArcUtility.DeleteElementFromGraphicContainer(
                            ArcMap.Document.ActivatedView,
                            this.boundingBoxGraphicElement);
                        this.boundingBoxGraphicElement = null;
                    }

                    // Clear the treeview
                    this.treeView1.Nodes.Clear();
                    this.textBoxSearch.Clear();

                    this.currentApplicationState = this.applicationStateGenerator.Next();
                    var commandBars = ArcMap.Application.Document.CommandBars;
                    var commandId = new UIDClass { Value = "DigitalGlobe_Inc_sma_VectorIndex" };

                    var commandItem = commandBars.Find(commandId, false, false);
                    if (commandItem != null)
                    {
                        this.originallySelectedItem = ArcMap.Application.CurrentTool;
                        ArcMap.Application.CurrentTool = commandItem;
                    }
                }
            }

            // Use selected AOI 
            else if (this.aoiTypeComboBox.SelectedIndex == 1)
            {
                // Clear any current drawn images
                if (this.boundingBoxGraphicElement != null)
                {
                    ArcUtility.DeleteElementFromGraphicContainer(
                        ArcMap.Document.ActivatedView,
                        this.boundingBoxGraphicElement);
                    this.boundingBoxGraphicElement = null;
                }

                this.ShapeAoi();
            }
        }

        /// <summary>
        ///     Event handler for when the search textbox gets focus
        /// </summary>
        /// <param name="sender">
        ///     the textbox
        /// </param>
        /// <param name="e">
        ///     event arguments
        /// </param>
        private void EventHandlerTextBoxSearchEnter(object sender, EventArgs e)
        {
            if (this.textBoxSearch.Text.Equals(GbdxResources.EnterSearchTerms))
            {
                this.textBoxSearch.Clear();
                this.textBoxSearch.ForeColor = Color.Black;
            }
        }

        /// <summary>
        ///     The event handler for when the key up event for the search textbox is triggered.
        /// </summary>
        /// <param name="sender">
        ///     textbox listening for the key up event.
        /// </param>
        /// <param name="e">
        ///     Event arguments
        /// </param>
        private void EventHandlerTextBoxSearchKeyUp(object sender, KeyEventArgs e)
        {
            // Only care about the enter key
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            this.currentApplicationState = this.applicationStateGenerator.Next();
            this.treeView1.Nodes.Clear();
            this.checkedNodes.Clear();
            this.treeView1.CheckBoxes = true;
            this.usingQuerySource = false;

            if (this.aoiTypeComboBox.SelectedIndex == 0)
            {
                // if the boundingbox graphic element hasn't been pressed then
                // lets assume that we use the curren't active views extent as the envelope.
                if (this.boundingBoxGraphicElement == null)
                {
                    var poly = VectorIndexHelper.DisplayRectangle(
                        ArcMap.Document.ActiveView,
                        out this.boundingBoxGraphicElement);

                    // Kick off vector index functionality
                    this.ShapeAoi(poly);
                    return;
                }

                // We already have a bounding box drawn so lets re-use that without redrawing the aoi.
                var tempPolygon = (IPolygon)this.boundingBoxGraphicElement.Geometry;
                this.ShapeAoi(tempPolygon);
            }
            else
            {
                // Clear any current drawn images
                if (this.boundingBoxGraphicElement != null)
                {
                    ArcUtility.DeleteElementFromGraphicContainer(
                        ArcMap.Document.ActivatedView,
                        this.boundingBoxGraphicElement);
                    this.boundingBoxGraphicElement = null;
                }

                this.ShapeAoi();
            }
        }

        /// <summary>
        ///     Event handler for when the search textbox loses focus
        /// </summary>
        /// <param name="sender">
        ///     the textbox
        /// </param>
        /// <param name="e">
        ///     Event arguments
        /// </param>
        private void EventHandlerTextBoxSearchLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxSearch.Text))
            {
                this.textBoxSearch.Text = GbdxResources.EnterSearchTerms;
                this.textBoxSearch.ForeColor = Color.DarkGray;
            }
        }

        /// <summary>
        ///     Event handler that fires every time a item is checked in the tree view.
        /// </summary>
        /// <param name="sender">
        ///     Tree view
        /// </param>
        /// <param name="e">
        ///     Tree view Arguments
        /// </param>
        private void EventHandlerTreeViewAfterCheck(object sender, TreeViewEventArgs e)
        {
            var nodes = this.GetCheckedNodes(this.treeView1.Nodes);
            foreach (var item in nodes)
            {
                // if we already have the node as Checked stop processing.
                if (this.checkedNodes.Contains(item))
                {
                    continue;
                }

                // Add the item to the list of checked items
                this.checkedNodes.Add(item);

                // Check to see if the node type is a source node.
                if (item.GetType() == typeof(VectorIndexSourceNode))
                {
                    this.ProcessSourceNodeClick(item);
                    return;
                }

                // Check to see if the node type is a geometry node.
                if (item.GetType() == typeof(VectorIndexGeometryNode))
                {
                    this.ProcessGeometryNodeClick(item);
                    return;
                }

                // Check to see if the node type is a type node.
                if (item.GetType() == typeof(VectorIndexTypeNode))
                {
                    this.GetPagingId(item, this.currentApplicationState);
                    return;
                }

            }
        }

        private void UpdateTreeNodeStatus(TreeNode node, string message)
        {
            var vectorNode = (VectorIndexTypeNode)node;

            var result = this.treeView1.Nodes.Find(node.Name, true);
            var updatedMessage = string.Format("{0} ({1}) [{2}]", vectorNode.Type.Name, vectorNode.Type.Count, message);

            if (result.Length != 0)
            {
                result[0].Text = updatedMessage;
            }
        }

        /// <summary>
        ///     Event handler to handle the events of when the form's visibility changes.
        /// </summary>
        /// <param name="sender">
        ///     sender of the event
        /// </param>
        /// <param name="e">
        ///     event arguments
        /// </param>
        private void EventHandlerVectorIndexDockableVisibleChanged(object sender, EventArgs e)
        {
            this.ResetVectorServices();
        }

        private void GetAuthenticationToken()
        {
            IRestClient restClient = new RestClient(Settings.Default.AuthBase);

            string password;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out password);

            if (!result)
            {
                Jarvis.Logger.Warning("PASSWORD FAILED DECRYPTION");
                return;
            }

            var request = new RestRequest(Settings.Default.authenticationServer, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", string.Format("Basic {0}", Settings.Default.apiKey));
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", Settings.Default.username);
            request.AddParameter("password", password);

            restClient.ExecuteAsync<AccessToken>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());

                        if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                        {
                            this.token = resp.Data.access_token;
                        }
                        else
                        {
                            this.token = string.Empty;
                        }
                    });
        }

        /// <summary>
        ///     Retrieves a list of the currently checked nodes
        /// </summary>
        /// <param name="nodes">
        ///     TreeNodeCollection of all nodes
        /// </param>
        /// <returns>
        ///     List of checked nodes.
        /// </returns>
        private List<TreeNode> GetCheckedNodes(TreeNodeCollection nodes)
        {
            var output = new List<TreeNode>();
            foreach (TreeNode aNode in nodes)
            {
                if (aNode.Checked)
                {
                    output.Add(aNode);
                }

                if (aNode.Nodes.Count != 0)
                {
                    output.AddRange(this.GetCheckedNodes(aNode.Nodes));
                }
            }

            return output;
        }

        private static string GetFileNameCloseStream(StreamWriter streamWriter)
        {
            var fs = (FileStream)streamWriter.BaseStream;
            var filepath = fs.Name;
            streamWriter.Close();

            return filepath;
        }

        private void GetGeometries(VectorIndexSourceNode source, int applicationState, int attempts = 0)
        {
            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
            var addressUrl = string.Empty;

            if (string.IsNullOrEmpty(this.query))
            {
                addressUrl = string.Format("/insight-vector/api/shape/{0}/geometries", source.Source.Name);
            }
            else
            {
                addressUrl = "/insight-vector/api/shape/query/geometries?q=" + this.query;
            }

            var request = new RestRequest(addressUrl, Method.POST);
            request.AddHeader("Authorization", "Bearer  " + this.token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", this.Aoi, ParameterType.RequestBody);
            attempts++;
            client.ExecuteAsync<SourceTypeResponseObject>(
                request,
                resp => this.ProcessGeometries(resp, source, applicationState, attempts));
        }

        private void GetPages(TreeNode node, string pageId, int applicationState, int totalCount, int currentCount, string layerName, StreamWriter fileStreamWriter, int attempts = 0)
        {
            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
            var request = new RestRequest("/insight-vector/api/esri/paging", Method.POST);

            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("ttl", "5m");
            request.AddParameter("fields", "attributes");
            request.AddParameter("pagingId", pageId);

            attempts++;
            client.ExecuteAsync<PagedData2>(
                request,
                resp => this.ProcessPage(node, resp, applicationState, totalCount, currentCount, pageId, layerName, fileStreamWriter, attempts));
        }

        //private void UpdateStatus(TreeNode node, )

        private void GetPagingId(TreeNode node, int applicationState, int attempts = 0)
        {
            var sourceNode = (VectorIndexSourceNode)node.Parent.Parent;
            var geometryNode = (VectorIndexGeometryNode)node.Parent;
            var typeNode = (VectorIndexTypeNode)node;

            var addressUrl = string.Empty;

            if (string.IsNullOrEmpty(this.query))
            {
                addressUrl = string.Format(
                    "/insight-vector/api/shape/{0}/{1}/{2}/paging",
                    sourceNode.Source.Name,
                    geometryNode.GeometryType.Name,
                    typeNode.Type.Name);
            }
            else
            {
                addressUrl = string.Format(
                    "/insight-vector/api/shape/query/{0}/{1}/paging?q={2}",
                    geometryNode.GeometryType.Name,
                    typeNode.Type.Name,
                    this.query);
            }

            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));

            var request = new RestRequest(addressUrl, Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", this.Aoi, ParameterType.RequestBody);

            attempts++;

            client.ExecuteAsync<PageId>(request, resp => this.ProcessPagingId(resp, attempts, node, applicationState));
        }

        private void GetSources(int applicationState, int attempts = 0)
        {
            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));

            var addressUrl = "/insight-vector/api/shape/sources";

            var request = new RestRequest(addressUrl, Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", this.Aoi, ParameterType.RequestBody);
            attempts++;

            client.ExecuteAsync<SourceTypeResponseObject>(
                request,
                resp => this.ProcessSources(resp, applicationState, attempts));
        }

        private void GetTypes(TreeNode geometryNode, int applicationState, int attempts = 0)
        {
            geometryNode.Text = geometryNode.Text.Replace(GbdxResources.Source_ErrorMessage, string.Empty);
            geometryNode.Text += GbdxResources.SearchingText;

            var sourceNode = (VectorIndexSourceNode)geometryNode.Parent;
            var geomNode = (VectorIndexGeometryNode)geometryNode;

            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
            var addressString = string.Empty;
            if (string.IsNullOrEmpty(this.query))
            {
                addressString = string.Format(
                    "/insight-vector/api/shape/{0}/{1}/types",
                    sourceNode.Source.Name,
                    geomNode.GeometryType.Name);
            }
            else
            {
                addressString = string.Format(
                    "/insight-vector/api/shape/query/{0}/types?q={1}",
                    geomNode.GeometryType.Name,
                    this.query);
            }

            var request = new RestRequest(addressString, Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", this.Aoi, ParameterType.RequestBody);
            attempts++;
            client.ExecuteAsync<SourceTypeResponseObject>(
                request,
                resp => this.ProcessGetTypesResponse(resp, geometryNode, applicationState, attempts));
        }

        /// <summary>
        ///     Event handler for the polygon has been set.  This is how the Vector index through the select area button
        ///     gets kicked off.
        /// </summary>
        /// <param name="poly">
        ///     The IPolygon of the bounding box
        /// </param>
        /// <param name="elm">
        ///     The element containing the graphic displayed on arc map
        /// </param>
        private void InstancePolygonHasBeenSet(IPolygon poly, IElement elm)
        {
            // We got the data we needed so unsubscribe from the event.
            VectorIndexRelay.Instance.PolygonHasBeenSet -= this.InstancePolygonHasBeenSet;

            // Now that we have what was needed from the VectorIndex tool Re-Select original tool
            ArcMap.Application.CurrentTool = this.originallySelectedItem;

            this.boundingBoxGraphicElement = elm;

            // Kick off vector index functionality
            //this.VectorIndex(poly);
            this.ShapeAoi(poly);
        }

        private void ProcessGeometries(
            IRestResponse<SourceTypeResponseObject> resp,
            VectorIndexSourceNode source,
            int applicationState,
            int attempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK && attempts <= MaxAttempts)
            {
                this.GetGeometries(source, applicationState, attempts);
            }

            if (applicationState == this.currentApplicationState)
            {
                this.Invoke(new UpdateTreeGeometries(this.UpdateTreeviewWithGeometry), resp, source);
            }
        }

        /// <summary>
        ///     Process a geometry node click.  (Polygon, Polyline, Point)
        /// </summary>
        /// <param name="geometryNode">
        ///     The node that was checked
        /// </param>
        private void ProcessGeometryNodeClick(TreeNode geometryNode)
        {
            this.GetTypes(geometryNode, this.currentApplicationState);
        }

        private void ProcessGetTypesResponse(
            IRestResponse<SourceTypeResponseObject> resp,
            TreeNode geometryNode,
            int applicationState,
            int attempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK && attempts <= MaxAttempts)
            {
                this.GetTypes(geometryNode, applicationState, attempts);
                return;
            }

            if (applicationState == this.currentApplicationState)
            {
                this.Invoke(new UpdateTreeTypes(this.UpdateTreeviewWithTypes), resp, geometryNode);
            }
        }

        /// <summary>
        ///     Combine all the pages of json results into one big json collection and convert it to a feature class
        /// </summary>
        /// <param name="node"></param>
        /// <param name="resp">IRestResponse</param>
        /// <param name="applicationState"></param>
        /// <param name="totalCount"></param>
        /// <param name="currentCount"></param>
        /// <param name="pageId">page id for the next page of results</param>
        /// <param name="layerName">name of the layer that will be made</param>
        /// <param name="fileStreamWriter">streamwriter to the tempfile</param>
        /// <param name="attempts">number of attempts to make before erroring out</param>
        private void ProcessPage(TreeNode node, IRestResponse<PagedData2> resp, int applicationState, int totalCount, int currentCount, string pageId, string layerName, StreamWriter fileStreamWriter, int attempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            // If we have a problem getting the page try again up to max attempts
            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK && attempts <= MaxAttempts)
            {
                this.GetPages(node, pageId, applicationState, totalCount, currentCount, layerName, fileStreamWriter, attempts);
                return;
            }

            // there are items so write them to the temp file.
            // one page of results per line
            if (resp.Data.item_count != "0")
            {
                // Write entire page of data to one line in the temp file
                fileStreamWriter.WriteLine(resp.Data.data.Replace("\r", "").Replace("\n", ""));

                if (applicationState == this.currentApplicationState)
                {
                    int count;
                    var result = int.TryParse(resp.Data.item_count, out count);

                    if (result)
                    {
                        currentCount += count;
                    }

                    var message = string.Format("Downloading {0}%", currentCount / totalCount * 100);
                    this.Invoke(new UpdateStatusText(this.UpdateTreeNodeStatus), node, message);

                    // Continue getting the rest of the associated pages.
                    this.GetPages(node, resp.Data.next_paging_id, applicationState, totalCount, currentCount, layerName, fileStreamWriter);
                }
                else
                {
                    // application states changed mid download so stop future paging and delete the temp file.
                    var filepath = GetFileNameCloseStream(fileStreamWriter);

                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
            }
            else
            {
                var filepath = GetFileNameCloseStream(fileStreamWriter);

                // make sure that the application state matches before proceeding with creating the feature class
                if (applicationState == this.currentApplicationState)
                {
                    var message = "Processing";
                    this.Invoke(new UpdateStatusText(this.UpdateTreeNodeStatus), node, message);

                    var tableName = Jarvis.ConvertPagesToFeatureClass(filepath, layerName);

                    if (!string.IsNullOrEmpty(tableName))
                    {
                        this.Invoke(new AddLayerToMapDelegate(AddLayerToMap), tableName, layerName);
                        message = "Complete";
                        this.Invoke(new UpdateStatusText(this.UpdateTreeNodeStatus), node, message);
                    }
                }

                // delete the file after everything has finished processing
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
        }

        private void ProcessPagingId(IRestResponse<PageId> resp, int attempts, TreeNode node, int applicationState)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK && attempts <= MaxAttempts)
            {
                this.GetPagingId(node, attempts);
                return;
            }

            var typeNode = (VectorIndexTypeNode)node;
            var tempFile = Path.GetTempFileName();
            var fileStream = File.Open(tempFile, FileMode.Append);
            var fileStreamWriter = new StreamWriter(fileStream);

            if (applicationState == this.currentApplicationState)
            {
                this.GetPages(node, resp.Data.pagingId, applicationState, resp.Data.itemCount,0, typeNode.Type.Name, fileStreamWriter);
            }
        }

        /// <summary>
        ///     Process a Source Node Click
        /// </summary>
        /// <param name="sourceNode">
        ///     the source node that was clicked
        /// </param>
        private void ProcessSourceNodeClick(TreeNode sourceNode)
        {
            sourceNode.Text = sourceNode.Text.Replace(GbdxResources.Source_ErrorMessage, string.Empty);
            sourceNode.Text += GbdxResources.SearchingText;

            this.GetGeometries(sourceNode as VectorIndexSourceNode, this.currentApplicationState);
        }

        private void ProcessSources(IRestResponse<SourceTypeResponseObject> resp, int applicationState, int attempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK && attempts <= MaxAttempts)
            {
                this.GetSources(applicationState, attempts);
            }

            if (applicationState == this.currentApplicationState)
            {
                this.Invoke(new UpdateTreeSources(this.UpdateTreeViewWithSources), resp);
            }
        }

        /// <summary>
        ///     Resets the vector index.  Deletes the element from the graphic container, clears the the treeview and search
        ///     textbox
        ///     and changes the current applications tate.
        /// </summary>
        private void ResetVectorServices()
        {
            this.treeView1.Nodes.Clear();
            this.textBoxSearch.Clear();

            // Clear any current drawn images
            if (this.boundingBoxGraphicElement != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(
                    ArcMap.Document.ActivatedView,
                    this.boundingBoxGraphicElement);
                this.boundingBoxGraphicElement = null;
            }

            this.currentApplicationState = this.applicationStateGenerator.Next();
        }

        private void ShapeAoi(IPolygon poly = null)
        {
            this.query = string.Empty;

            var geometries = new List<IGeometry>();
            if (poly == null)
            {
                geometries = Jarvis.GetSelectedGeometries(ArcMap.Document.FocusMap);
            }
            else
            {
                geometries.Add(poly);
            }

            // check to see if features were selected
            if (geometries.Count == 0)
            {
                MessageBox.Show(GbdxResources.noFeaturesSelected);
                return;
            }

            this.Aoi = Jarvis.CreateGeometryCollectionGeoJson(geometries);

            this.treeView1.CheckBoxes = false;
            this.treeView1.Nodes.Clear();
            var searchingNode = new VectorIndexSourceNode { Text = GbdxResources.SearchingText };
            this.treeView1.Nodes.Add(searchingNode);

            this.currentApplicationState = this.applicationStateGenerator.Next();

            if (this.textBoxSearch.Text.Equals(GbdxResources.EnterSearchTerms)
                || this.textBoxSearch.Text == string.Empty)
            {
                this.query = string.Empty;
                this.GetSources(this.currentApplicationState);
            }
            else
            {
                this.query = this.textBoxSearch.Text;
                this.GetGeometries(searchingNode, this.currentApplicationState);
            }
        }

        private void UpdateTreeviewWithGeometry(
            IRestResponse<SourceTypeResponseObject> resp,
            VectorIndexSourceNode source)
        {
            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK)
            {
                // Remove the node from the list of checked nodes because an error occurred.
                this.checkedNodes.Remove(this.treeView1.Nodes[source.Index]);
                this.treeView1.Nodes[source.Index].Checked = false;
                this.treeView1.Nodes[source.Index].Text =
                    this.treeView1.Nodes[source.Index].Text.Replace(
                        GbdxResources.SearchingText,
                        GbdxResources.Source_ErrorMessage);

                // Don't allow it to process further.
                return;
            }

            // do this for normal tree traversal with a source - geometry - type
            if (string.IsNullOrEmpty(this.query))
            {
                // Results were found so lets get rid of the searching text.
                this.treeView1.Nodes[source.Index].Text =
                    this.treeView1.Nodes[source.Index].Text.Replace(GbdxResources.SearchingText, string.Empty);
                foreach (var geoType in resp.Data.Data)
                {
                    var newItem = new VectorIndexGeometryNode
                                      {
                                          Source = source.Source,
                                          GeometryType = geoType,
                                          Text =
                                              string.Format(
                                                  "{0} ({1})",
                                                  geoType.Name,
                                                  geoType.Count)
                                      };

                    this.treeView1.Nodes[source.Index].Nodes.Add(newItem);
                }
            }
            else // do this when there is a query whose tree structure goes geoemtry - type
            {
                // Results were found so lets get rid of the searching text.
                this.treeView1.Nodes.Remove(source);
                this.treeView1.CheckBoxes = true;
                foreach (var geoType in resp.Data.Data)
                {
                    var newItem = new VectorIndexGeometryNode
                                      {
                                          Source = null,
                                          GeometryType = geoType,
                                          Text =
                                              string.Format(
                                                  "{0} ({1})",
                                                  geoType.Name,
                                                  geoType.Count)
                                      };
                    this.treeView1.Nodes.Add(newItem);
                }
            }

            this.treeView1.Sort();
        }

        private void UpdateTreeViewWithSources(IRestResponse<SourceTypeResponseObject> resp)
        {
            // An error occurred
            if (resp.Data == null || resp.Data.Data == null)
            {
                var newItem = new VectorIndexSourceNode { Text = GbdxResources.Source_ErrorMessage };
                this.treeView1.Nodes.Clear();
                this.treeView1.Nodes.Add(newItem);
                this.treeView1.CheckBoxes = false;

                // A error occured so stop processing
                return;
            }

            // If no sources found.
            if (resp.Data.Data.Count == 0)
            {
                var newItem = new VectorIndexSourceNode { Text = GbdxResources.NoDataFound };
                this.treeView1.Nodes.Add(newItem);
                this.treeView1.CheckBoxes = false;

                // No sources were found so stop processing
                return;
            }

            // Before populating the tree lets remove the empty node i created and turn checkboxes back on
            this.treeView1.Nodes.Clear();
            this.treeView1.CheckBoxes = true;

            foreach (var item in resp.Data.Data)
            {
                var newItem = new VectorIndexSourceNode
                                  {
                                      Text = string.Format("{0} ({1})", item.Name, item.Count),
                                      Source = item
                                  };

                this.treeView1.Nodes.Add(newItem);
            }

            this.treeView1.Sort();
        }

        private void UpdateTreeviewWithTypes(
            IRestResponse<SourceTypeResponseObject> resp,
            VectorIndexGeometryNode geometryNode)
        {
            var sourceNode = (VectorIndexSourceNode)geometryNode.Parent;

            if (resp.Data == null || resp.StatusCode != HttpStatusCode.OK)
            {
                // Remove the node from the list of checked nodes because an error occurred.
                this.checkedNodes.Remove(this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index]);
                this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Checked = false;
                this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Text =
                    this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Text.Replace(
                        GbdxResources.SearchingText,
                        GbdxResources.Source_ErrorMessage);

                // Don't allow it to process further.
                return;
            }

            // if not using a query
            if (string.IsNullOrEmpty(this.query))
            {
                this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Text =
                    this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Text.Replace(
                        GbdxResources.SearchingText,
                        string.Empty);
            }
            else // if using a query
            {
                this.treeView1.Nodes[geometryNode.Index].Text =
                    this.treeView1.Nodes[geometryNode.Index].Text.Replace(GbdxResources.SearchingText, string.Empty);
            }

            foreach (var type in resp.Data.Data)
            {
                if (string.IsNullOrEmpty(type.Name))
                {
                    continue;
                }

                var newItem = new VectorIndexTypeNode
                                  {
                                      Geometry = geometryNode.GeometryType,
                                      Type = type,
                                      Text = string.Format("{0} ({1})", type.Name, type.Count),
                                      Name = Guid.NewGuid().ToString()
                                  };
                
                if (string.IsNullOrEmpty(this.query))
                {
                    // non query
                    newItem.Source = sourceNode.Source;
                    this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].ContextMenuStrip =
                        this.CreateContextMenuStrip(this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index]);

                    this.treeView1.Nodes[sourceNode.Index].Nodes[geometryNode.Index].Nodes.Add(newItem);
                }
                else
                {
                    // query
                    newItem.Source = null;

                    this.treeView1.Nodes[geometryNode.Index].ContextMenuStrip =
                        this.CreateContextMenuStrip(this.treeView1.Nodes[geometryNode.Index]);

                    this.treeView1.Nodes[geometryNode.Index].Nodes.Add(newItem);
                }
            }

            this.treeView1.Sort();
        }

        /// <summary>
        ///     Implementation class of the dockable window add-in. It is responsible for
        ///     creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            #region Fields & Properties

            /// <summary>
            ///     The Dockable vector index window.
            /// </summary>
            private VectorIndexDockable mWindowUi;

            #endregion

            /// <summary>
            ///     The dispose.
            /// </summary>
            /// <param name="disposing">
            ///     The disposing.
            /// </param>
            protected override void Dispose(bool disposing)
            {
                if (this.mWindowUi != null)
                {
                    this.mWindowUi.Dispose(disposing);
                }

                base.Dispose(disposing);
            }

            /// <summary>
            ///     The on create child.
            /// </summary>
            /// <returns>
            ///     The <see cref="IntPtr" />.
            /// </returns>
            protected override IntPtr OnCreateChild()
            {
                this.mWindowUi = new VectorIndexDockable(this.Hook);
                return this.mWindowUi.Handle;
            }
        }
    } // closes VectorIndexDockable Class
}