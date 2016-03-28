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

using GbdxTools;

namespace Gbdx.Vector_Index.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Windows.Forms;

    using Amib.Threading;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using GbdxSettings.Properties;

    using Logging;

    using NetworkConnections;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;
    using Path = System.IO.Path;

    /// <summary>
    /// Designer class of the dock able window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class VectorIndexDockable : UserControl
    {
        /// <summary>
        /// Max number of threads allowed to communicate with GBDX services.  
        /// </summary>
        private const int MaxThreads = 4;
        
        /// <summary>
        /// When true a console will appear when exceptions occur.  Currently not functional.
        /// </summary>
        private static readonly bool ConsoleLogging = false;

        /// <summary>
        /// Random number generator meant to create a new state number for certain kinds of events.
        /// </summary>
        private readonly Random applicationStateGenerator = new Random(DateTime.UtcNow.Millisecond);

        /// <summary>
        /// HashSet containing all of the checked nodes  This acts as a check to make sure the same node isn't downloaded twice.
        /// </summary>
        private readonly HashSet<TreeNode> checkedNodes = new HashSet<TreeNode>();

        /// <summary>
        /// Smart thread pool meant to process the sections of code that require a STA apartment state
        /// </summary>
        private readonly SmartThreadPool smartThreadPool;

        /// <summary>
        /// Object to lock to synchronize threads
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// The logger variable to capture info, warnings, errors etc. 
        /// </summary>
        private readonly Logger logWriter;

        /// <summary>
        /// The comms package that implements IGbdxComms interface.  Used for communications with GBDX
        /// services.
        /// </summary>
        private readonly IGbdxComms comms = new GbdxComms(Jarvis.LogFile, ConsoleLogging);

        /// <summary>
        /// Originally selected esri item.  Once the Vector index tool has drawn it's bounding box this tool will be re-selected.
        /// </summary>
        private ICommandItem originallySelectedItem;
        
        /// <summary>
        /// Main network object that will be copied to threads as needed.
        /// </summary>
        private NetObject networkObject;

        /// <summary>
        /// Contains the coordinates of the bounding box.
        /// </summary>
        private BoundingBox bBox;

        /// <summary>
        /// The IElement containing the visual of the bounding box displayed on Arc Map
        /// </summary>
        private IElement boundingBoxGraphicElement = null;

        /// <summary>
        /// If we are using a query instead of the usual tree traversal method for the UVI
        /// </summary>
        private bool usingQuerySource = false;

        /// <summary>
        /// Current application state.  This state will change if the user changes MXDs, closes and re-opens the UVI, and other events of that nature.
        /// </summary>
        private int currentApplicationState;

        /// <summary>
        /// Local copy of the username.
        /// </summary>
        private string username = string.Empty;

        /// <summary>
        /// Local copy of the unencrypted password.
        /// </summary>
        private string password = string.Empty;

        /// <summary>
        /// Determines if the UVI is in test mode.  This variable will only be true if manually set in a developer environment.
        /// </summary>
        private bool test = false;

        /// <summary>
        /// Current number of threads in use that are communicating with GBDX services
        /// </summary>
        private int currentThreadCount = 0;

        /// <summary>
        /// The current job queue.  If there are more than the max threads number the TreeNodes will be added 
        /// to the queue.  When a job finishes the queue is checked to see if any jobs are currently waiting.
        /// </summary>
        private Queue<TreeNode> jobQueue = new Queue<TreeNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorIndexDockable"/> class. 
        /// Default constructor.
        /// </summary>
        public VectorIndexDockable()
        {
            this.smartThreadPool = new SmartThreadPool();
            this.treeView1.AfterCheck += this.TreeView1AfterCheck;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorIndexDockable"/> class.
        /// </summary>
        /// <param name="hook">
        /// The hook.
        /// </param>
        public VectorIndexDockable(object hook)
        {
            this.InitializeComponent();

            this.logWriter = new Logger(Jarvis.LogFile, ConsoleLogging);

            this.Hook = hook;
            this.textBoxSearch.Text = GbdxSettings.GbdxResources.EnterSearchTerms;
            this.textBoxSearch.ForeColor = Color.DarkGray;
            this.UserAuthenticationCheck(Settings.Default, ref this.username, ref this.password, this.comms, true);
            this.smartThreadPool = new SmartThreadPool();
            this.treeView1.AfterCheck += this.TreeView1AfterCheck;
            this.VisibleChanged += this.VectorIndexDockableVisibleChanged;
            this.textBoxSearch.LostFocus += this.TextBoxSearchLeave;
            this.textBoxSearch.GotFocus += this.TextBoxSearchEnter;

            this.textBoxSearch.KeyUp += this.TextBoxSearchKeyUp;
            this.textBoxSearch.Invalidate();

            ArcMap.Events.NewDocument += this.ResetVectorIndex;
            ArcMap.Events.OpenDocument += this.ResetVectorIndex;
            ArcMap.Events.CloseDocument += this.ResetVectorIndex;
            this.currentApplicationState = this.applicationStateGenerator.Next();

            this.aoiTypeComboBox.SelectedIndex = 0;
            this.ActiveControl = this.treeView1;
            this.comms = new GbdxComms(Jarvis.LogFile, ConsoleLogging);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorIndexDockable"/> class. 
        /// Constructor to be used in a developer setting for testing purposes.
        /// </summary>
        /// <param name="hook">
        /// hook coming from arc map
        /// </param>
        /// <param name="isTest">
        /// is test variable
        /// </param>
        public VectorIndexDockable(object hook, bool isTest)
        {
            this.InitializeComponent();
            this.Hook = hook;
            this.UserAuthenticationCheck(Settings.Default, ref this.username, ref this.password, this.comms, true);
            this.smartThreadPool = new SmartThreadPool();
            this.treeView1.AfterCheck += this.TreeView1AfterCheck;
            this.VisibleChanged += this.VectorIndexDockableVisibleChanged;
            this.textBoxSearch.LostFocus += this.TextBoxSearchLeave;
            this.textBoxSearch.GotFocus += this.TextBoxSearchEnter;
            ArcMap.Events.NewDocument += this.ResetVectorIndex;
            ArcMap.Events.OpenDocument += this.ResetVectorIndex;
            ArcMap.Events.CloseDocument += this.ResetVectorIndex;
            this.currentApplicationState = this.applicationStateGenerator.Next();
            this.test = isTest;
        }

        /// <summary>
        /// The deserialize JSON.
        /// </summary>
        /// <param name="obj">
        /// The worker object containing response results.
        /// </param>
        private delegate void DeserializeJson(WorkerObject obj);

        /// <summary>
        /// The type tree.
        /// </summary>
        /// <param name="obj">
        /// The worker object containing response results.
        /// </param>
        private delegate void TypeTree(WorkerObject obj);

        #region ESRI Addin Designer Added Code (Includes Constructor)
        /// <summary>
        /// Gets or sets the Host object of the dockable window
        /// </summary>
        private object Hook { get; set; }
        #endregion

        #region Private Methods
        #region Static
        /// <summary>
        /// When a error during stage retrieval happens there is a set mount of time that.
        /// </summary>
        /// <param name="localComms">
        /// The local Comms.
        /// </param>
        /// <param name="networkObj">
        /// The network object.
        /// </param>
        /// <param name="formParams">
        /// The form parameters for paged requests.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ItemRetrievalErrorRecovery(IGbdxComms localComms, ref NetObject networkObj, NameValueCollection formParams)
        {
            bool success = false;

            // Create a stopwatch timer and ensure it's starting at 0.
            var timer = new Stopwatch();
            timer.Stop();
            timer.Reset();
            timer.Start();

            // Try again for 60 seconds to get a successful response.
            // After 60 seconds Elastic Search will "forget" about the page id
            // Requiring a whole new calling of this function.
            while (timer.ElapsedMilliseconds < 60000)
            {
                // Sleep for 250 milliseconds before each attempt.
                Thread.Sleep(250);
                success = localComms.StagedRequest(ref networkObj, formParams);

                if (success)
                {
                    timer.Stop();

                    // the staged request returned successfully and recovered from the stall.
                    break;
                }
            }

            return success;
        }

        #endregion

        #region Button Event Handelers

        /// <summary>
        /// Event handler for when the select area button is clicked.
        /// </summary>
        /// <param name="sender">
        /// the button that is clicked
        /// </param>
        /// <param name="e">
        /// event arguments that get sent by the button
        /// </param>
        private void SelectAreaButtonClick(object sender, EventArgs e)
        {
            if (!this.UserAuthenticationCheck(Settings.Default, ref this.username, ref this.password, this.comms, false))
            {
                return;
            }

            // Draw Rectangle option
            if(this.aoiTypeComboBox.SelectedIndex ==0)
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
                this.PolygonAoi();
            }
        }

        private static string GetGeoJson()
        {
            var polys = Jarvis.GetPolygons(ArcMap.Document.FocusMap);
            return Jarvis.ConvertPolygonsToGeoJson(polys);
        }

        /// <summary>
        /// Event handler for when the clear button is clicked.
        /// </summary>
        /// <param name="sender">
        /// the button that is clicked
        /// </param>
        /// <param name="e">
        /// event arguments that get sent by the button
        /// </param>
        private void ClearButtonClick(object sender, EventArgs e)
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

        #endregion

        #region Network Object Setup Functions

        /// <summary>
        /// Sets up the network object.
        /// </summary>
        /// <param name="gbdxUsername">
        /// The gbdx Username.
        /// </param>
        /// <param name="gbdxPassword">
        /// The gbdx Password.
        /// </param>
        /// A set up net object
        /// <returns>
        /// NetObject username, password set along with the authentication endpoints that will be needed.
        /// </returns>
        private NetObject SetupNetObject(string gbdxUsername, string gbdxPassword)
        {
            string decyrptedPassword;
            var success = Aes.Instance.Decrypt128(gbdxPassword, out decyrptedPassword);

            if (!success)
            {
                return null;
            }

            var netObj = new NetObject
                             {
                                 Password = decyrptedPassword,
                                 User = gbdxUsername
                             };

            netObj.AuthUrl = Settings.Default.AuthBase;
            netObj.ApiKey = Settings.Default.apiKey;

            netObj = this.SetAuthenticationEndpoints(netObj);


            return netObj;
        }

        /// <summary>
        /// Set the NetworkObject authentication endpoints.
        /// </summary>
        /// <param name="netObj">
        /// Network object that is having the endpoints set
        /// </param>
        /// <returns>
        /// A network object with the same settings and the authentication endpoints set
        /// </returns>
        private NetObject SetAuthenticationEndpoints(NetObject netObj)
        {
            netObj.AuthEndpoint = GbdxHelper.GetAuthenticationEndpoint(Settings.Default);
            netObj.BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default);
            return netObj;
        }

        #endregion

        #region Other UI Event Handelers

        /// <summary>
        /// Event handler for the polygon has been set.  This is how the Vector index through the select area button
        /// gets kicked off.  
        /// </summary>
        /// <param name="poly">
        /// The IPolygon of the bounding box
        /// </param>
        /// <param name="elm">
        /// The element containing the graphic displayed on arc map
        /// </param>
        private void InstancePolygonHasBeenSet(IPolygon poly, IElement elm)
        {
            // We got the data we needed so unsubscribe from the event.
            VectorIndexRelay.Instance.PolygonHasBeenSet -= this.InstancePolygonHasBeenSet;

            // Now that we have what was needed from the VectorIndex tool Re-Select original tool
            ArcMap.Application.CurrentTool = this.originallySelectedItem;

            this.boundingBoxGraphicElement = elm;

            // Kick off vector index functionality
            this.VectorIndex(poly);
        }

        /// <summary>
        /// Retrieves a list of the currently checked nodes
        /// </summary>
        /// <param name="nodes">
        /// TreeNodeCollection of all nodes
        /// </param>
        /// <returns>
        /// List of checked nodes.
        /// </returns>
        private List<TreeNode> GetCheckedNodes(TreeNodeCollection nodes)
        {
            List<TreeNode> output = new List<TreeNode>();
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

        /// <summary>
        /// Process a Source Node Click 
        /// </summary>
        /// <param name="sourceNode">
        /// the source node that was clicked
        /// </param>
        private void ProcessSourceNodeClick(TreeNode sourceNode)
        {
            sourceNode.Text = sourceNode.Text.Replace(GbdxSettings.GbdxResources.Source_ErrorMessage, string.Empty);
            sourceNode.Text += GbdxSettings.GbdxResources.SearchingText;

            var node = (VectorIndexSourceNode)sourceNode;
            var work = new WorkerObject
                           {
                               BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                               BoundBox = this.bBox,
                               SourceNode = node,
                               NetworkObject = this.networkObject,
                               ApplicationState = this.currentApplicationState,
                               Logger = this.logWriter
                           };

            // differntiate between using the query source and the regular sources
            // modified to work with RestSharp
            work.NetworkObject.BaseUrl = work.BaseUrl;

            if (this.aoiTypeComboBox.SelectedIndex == 1)
            {
                if(this.usingQuerySource)
                {
                    work.NetworkObject.AddressUrl = string.Format("/insight-vector/api/shape/query/{0}/types?q={1}",work.SourceNode.Source.Name,this.textBoxSearch.Text);
                }
                else
                {
                    work.NetworkObject.AddressUrl = string.Format(
                        "/insight-vector/api/shape/{0}/geometries",
                        work.SourceNode.Source.Name);
                }
            }
            else
            {
                work.NetworkObject.AddressUrl = this.usingQuerySource
                                                    ?
                                                    VectorIndexHelper.CreateQueryUrl(
                                                        work.BoundBox,
                                                        string.Empty,
                                                        this.textBoxSearch.Text,
                                                        work.SourceNode.Source.Name)
                                                    :
                                                    VectorIndexHelper.CreateUrl(
                                                        work.BoundBox,
                                                        string.Empty,
                                                        Uri.EscapeDataString(work.SourceNode.Source.Name));
                
            }

            if (node.Source != null)
            {
                ThreadPool.QueueUserWorkItem(
                    this.GetGeometryTypes,
                    new object[] { work, this.comms });
            }
        }

        /// <summary>
        /// Process a query node item click
        /// </summary>
        /// <param name="itemNode">
        /// query source (geometry)
        /// </param>
        private void ProcessQueryItemNodeClick(TreeNode itemNode)
        {
            itemNode.Text = itemNode.Text.Replace(GbdxSettings.GbdxResources.Source_ErrorMessage, string.Empty);
            var work = new WorkerObject
                           {
                               BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                               BoundBox = this.bBox,
                               SourceNode = (VectorIndexSourceNode)itemNode.Parent,
                               GeometryNode = (VectorIndexGeometryNode)itemNode,
                               NetworkObject = this.networkObject,
                               QuerySource = true,
                               ApplicationState = this.currentApplicationState,
                               Logger = this.logWriter
                           };

            var geometry = ((VectorIndexSourceNode)itemNode.Parent).Source.Name;
            var item = ((VectorIndexGeometryNode)itemNode).GeometryType.Name;

            // Modifed to work with restsharp
            if (this.networkObject.UsingPolygonAoi)
            {
                work.OriginalPagingIdUrl = string.Format(
                    "/insight-vector/api/shape/query/{0}/{1}/paging?q={2}",
                    Uri.EscapeDataString(geometry),
                    Uri.EscapeDataString(item),
                    Uri.EscapeDataString(this.textBoxSearch.Text));
            }
            else
            {
                work.OriginalPagingIdUrl = VectorIndexHelper.CreateQueryUrl(
                    work.BoundBox,
                    string.Empty,
                    Uri.EscapeDataString(this.textBoxSearch.Text),
                    Uri.EscapeDataString(geometry),
                    Uri.EscapeDataString(item));

            }
            var para = new object[2];
            para[0] = work;
            para[1] = this.comms;

            ThreadPool.QueueUserWorkItem(this.GetVectorData, para);
        }

        /// <summary>
        /// Process a geometry node click.  (Polygon, Polyline, Point)
        /// </summary>
        /// <param name="geometryNode">
        /// The node that was checked
        /// </param>
        private void ProcessGeometryNodeClick(TreeNode geometryNode)
        {
            geometryNode.Text = geometryNode.Text.Replace(GbdxSettings.GbdxResources.Source_ErrorMessage, string.Empty);
            geometryNode.Text += GbdxSettings.GbdxResources.SearchingText;

            var node = (VectorIndexGeometryNode)geometryNode;

            // Checking to see if the node is contained within the checkednodes type prevents multiple requests for data being fired off.
            var work = new WorkerObject
                           {
                               BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                               BoundBox = this.bBox,
                               SourceNode = (VectorIndexSourceNode)geometryNode.Parent,
                               GeometryNode = node,
                               NetworkObject = this.networkObject,
                               ApplicationState = this.currentApplicationState,
                               Logger = this.logWriter
                           };
            
            // Create the url for the vector types.
            // Modified to work with RestSharp.
            work.NetworkObject.BaseUrl = work.BaseUrl;

            if (this.aoiTypeComboBox.SelectedIndex == 1)
            {
                work.NetworkObject.AddressUrl = string.Format(
                    "/insight-vector/api/shape/{0}/{1}/types",
                    work.SourceNode.Source.Name,
                    work.GeometryNode.GeometryType.Name);
            }
            else

            {
                work.NetworkObject.AddressUrl = VectorIndexHelper.CreateUrl(
                    work.BoundBox,
                    string.Empty,
                    Uri.EscapeDataString(work.SourceNode.Source.Name),
                    Uri.EscapeDataString(work.GeometryNode.GeometryType.Name));

            }
            var parms = new object[] { work, this.comms };
            
            ThreadPool.QueueUserWorkItem(this.GetVectorTypes, parms);
        }

        /// <summary>
        /// Process when a type node is clicked like a HGIS &gt; Point &gt; Towers
        /// </summary>
        /// <param name="typeNode">
        /// Node that was checked
        /// </param>
        private void ProcessTypeNodeClick(TreeNode typeNode)
        {
            typeNode.Text = typeNode.Text.Replace(GbdxSettings.GbdxResources.Source_ErrorMessage, string.Empty);

            var work = new WorkerObject
                           {
                               BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                               BoundBox = this.bBox,
                               SourceNode = (VectorIndexSourceNode)typeNode.Parent.Parent,
                               GeometryNode = (VectorIndexGeometryNode)typeNode.Parent,
                               NetworkObject = this.networkObject,
                               TypeNode = (VectorIndexTypeNode)typeNode,
                               QuerySource = false,
                               ApplicationState = this.currentApplicationState,
                               Logger = this.logWriter
                           };

            // modified to work with restsharp
            string url;
            if (this.aoiTypeComboBox.SelectedIndex == 1)
            {
                url = string.Format("/insight-vector/api/shape/{0}/{1}/{2}/paging",Uri.EscapeDataString(work.SourceNode.Source.Name),
                    Uri.EscapeDataString(work.GeometryNode.GeometryType.Name),
                    Uri.EscapeDataString(work.TypeNode.Type.Name));
            }
            else
            {
                url = VectorIndexHelper.CreateStagingIdUrl(
                    work.BoundBox,
                    string.Empty,
                    Uri.EscapeDataString(work.SourceNode.Source.Name),
                    Uri.EscapeDataString(work.GeometryNode.GeometryType.Name),
                    Uri.EscapeDataString(work.TypeNode.Type.Name),
                    1,
                    100);
                
            }
            // Create url to get the Paging ID.
            work.OriginalPagingIdUrl = url;
            var para = new object[2];
            para[0] = work;
            para[1] = this.comms;
            ThreadPool.QueueUserWorkItem(this.GetVectorData, para);
        }

        /// <summary>
        /// Event handler that fires every time a item is checked in the tree view.
        /// </summary>
        /// <param name="sender">
        /// Tree view
        /// </param>
        /// <param name="e">
        /// Tree view Arguments
        /// </param>
        private void TreeView1AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!this.UserAuthenticationCheck(Settings.Default, ref this.username, ref this.password, this.comms, false))
            {
                return;
            }

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
                    // if using query source process the node differently than normal.  There is no next level of nodes before
                    // data acquistion 
                    if (this.usingQuerySource)
                    {
                        // Check to see if the number of max threads has exceeded teh threshold
                        if (this.currentThreadCount + 1 > MaxThreads)
                        {
                            // Currently the number of threads is at it's max so lets add to the queue
                            this.jobQueue.Enqueue(item);
                            item.Text += GbdxSettings.GbdxResources.Queued;
                            return;
                        }

                        // Max number of threads has not been exceeded so kick of a thread for it
                        this.currentThreadCount += 1;
                        this.ProcessQueryItemNodeClick(item);
                    }
                    else
                    {
                        this.ProcessGeometryNodeClick(item);
                    }

                    return;
                }

                // Check to see if the node type is a type node.
                if (item.GetType() == typeof(VectorIndexTypeNode))
                {
                    // Check to see if the number of max threads has exceeded the threshold
                    if (this.currentThreadCount + 1 > MaxThreads)
                    {
                        // Currently the number of threads is at it's max so lets add to the queue
                        this.jobQueue.Enqueue(item);
                        item.Text += GbdxSettings.GbdxResources.Queued;
                        return;
                    }

                    // Max number of threads has not been exceeded so kick of a thread for it
                    this.currentThreadCount += 1;

                    this.ProcessTypeNodeClick(item);
                    return;
                }
            }
        }

        /// <summary>
        /// Event handler for when the search textbox gets focus
        /// </summary>
        /// <param name="sender">
        /// the textbox
        /// </param>
        /// <param name="e">
        /// event arguments
        /// </param>
        private void TextBoxSearchEnter(object sender, EventArgs e)
        {
            if (this.textBoxSearch.Text.Equals(GbdxSettings.GbdxResources.EnterSearchTerms))
            {
                this.textBoxSearch.Clear();
                this.textBoxSearch.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Event handler for when the search textbox loses focus
        /// </summary>
        /// <param name="sender">
        /// the textbox
        /// </param>
        /// <param name="e">
        /// Event arguments
        /// </param>
        private void TextBoxSearchLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxSearch.Text))
            {
                this.textBoxSearch.Text = GbdxSettings.GbdxResources.EnterSearchTerms;
                this.textBoxSearch.ForeColor = Color.DarkGray;
            }
        }

        #endregion

        #region Threaded Methods

        /// <summary>
        /// Call to get the sources within the boundaries of the bounding box
        /// </summary>
        /// <param name="param">
        /// working object
        /// </param>
        private void LoadSources(object param)
        {
            var parmArray = (object[])param;

            var work = (WorkerObject)parmArray[0];

            var gbdxComms = (IGbdxComms)parmArray[1];

            work.NetworkObject.Result = string.Empty;

            var netObj = work.NetworkObject;

            if (netObj.UsingPolygonAoi)
            {
                netObj = gbdxComms.PushRequest(netObj);
            }
            else
            {
                netObj = gbdxComms.Request(netObj);
            }

            work.NetworkObject = netObj;
            work.ResponseObject = VectorIndexHelper.GetSourceType(netObj.Result);
            if (work.ResponseObject == null)
            {
                netObj.ErrorOccurred = true;
            }

            // If NetworkObject is null or the result string is null or empty don't process any further
            // Perhaps a messagebox should be spawned with more details about the issue.
            if (netObj == null || netObj.ErrorOccurred)
            {
                // Update the UI with error information
                this.Invoke(new TypeTree(this.UpdateTreeViewWithSources), work);

                // Error occurred so stop procesing
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            // The null case is checked for above.
            work.Types = work.ResponseObject.Data;

            // If not a unit test call the invoke to update the UI Thread
            this.Invoke(new TypeTree(this.UpdateTreeViewWithSources), work);
        }

        /// <summary>
        /// The call to get the types for selected source.
        /// </summary>
        /// <param name="source">
        /// worker object
        /// </param>
        private void GetGeometryTypes(object source)
        {
            var parms = (object[])source;
            var work = (WorkerObject)parms[0];
            var gbdxCloudComms = (IGbdxComms)parms[1];
            work.NetworkObject.Result = string.Empty;
            var netobj = work.NetworkObject;

            if (netobj.UsingPolygonAoi)
            {
                netobj = gbdxCloudComms.PushRequest(netobj);
            }
            else
            {
                netobj = gbdxCloudComms.Request(netobj);
            }

            work.NetworkObject = netobj;

            

            work.ResponseObject = VectorIndexHelper.GetSourceType(netobj.Result);
            if (work.ResponseObject == null)
            {
                netobj.ErrorOccurred = true;
            }

            // If NetworkObject is null or the result string is null or empty don't process any further
            if (netobj == null || netobj.ErrorOccurred)
            {
                // Update the UI with error information
                this.Invoke(new TypeTree(this.UpdateTreeviewWithGeometryTypes), work);

                // Error occurred so stop procesing
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            // The null case is checked for above.
            work.GeometryTypes = work.ResponseObject.Data;

            if (work.IsUnitTest)
            {
                return;
            }

            this.Invoke(new TypeTree(this.UpdateTreeviewWithGeometryTypes), work);
        }

        /// <summary>
        /// The call to get the actual vector data for a given type and source.
        /// </summary>
        /// <param name="source">
        /// worker object
        /// </param>
        private void GetVectorTypes(object source)
        {
            var parms = (object[])source;
            var work = (WorkerObject)parms[0];
            var gbdxComms = (IGbdxComms)parms[1];
            work.NetworkObject.Result = string.Empty;

            var netobj = work.NetworkObject;

            if (netobj.UsingPolygonAoi)
            {
                netobj = gbdxComms.PushRequest(netobj);
            }
            else
            {
                netobj = gbdxComms.Request(netobj);
            }

            work.ResponseObject = VectorIndexHelper.GetSourceType(netobj.Result);
            if (work.ResponseObject == null)
            {
                netobj.ErrorOccurred = true;
            }

            if (netobj.ErrorOccurred)
            {
                this.Invoke(new TypeTree(this.UpdateTreeviewWithTypes), work);
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            // The null case is checked for above.
            work.Types = work.ResponseObject.Data;

            // If this is method is called by a unit test then dont execute the invoke line
            if (!work.IsUnitTest)
            {
                this.Invoke(new TypeTree(this.UpdateTreeviewWithTypes), work);
            }
        }

        /// <summary>
        /// The get vector data.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        private void GetVectorData(object source)
        {
            var param = (object[])source;

            var work = (WorkerObject)param[0];
            var gbdxComms = (GbdxComms)param[1];

            work.NetworkObject.Result = string.Empty;
            work.NetworkObject.AddressUrl = work.OriginalPagingIdUrl;
            work.NetworkObject.BaseUrl = work.BaseUrl;
            work.NetworkObject.AuthUrl = Settings.Default.AuthBase;
            work.NetworkObject.AuthEndpoint = Settings.Default.authenticationServer;
            var netobj = work.NetworkObject;

            // Set the result and page id back empty string. only needed when this function gets called again.  
            netobj.Result = string.Empty;
            netobj.PageId = string.Empty;
            if (netobj.UsingPolygonAoi)
            {
                netobj = gbdxComms.PushRequest(netobj);
            }
            else
            {
                netobj = gbdxComms.Request(netobj);
            }

            if (netobj == null || netobj.ErrorOccurred)
            {
                this.Invoke(new DeserializeJson(this.ArcObjectsDeserializeJson), work);
                return;
            }

            // Use the page id to get the first page of results.
            work.NetworkObject.PageId = VectorIndexHelper.GetPageId(netobj.Result);

            work.Responses = new List<string>();
            var done = false;
            var itemsReceived = 0;
            var success = true;

            work.TemporaryFilePath = Path.GetTempFileName();
            using (var file = new StreamWriter(work.TemporaryFilePath))
            {
                while (!done)
                {
                    // modifed to work with rest sharp
                    work.NetworkObject.AddressUrl = VectorIndexHelper.CreateStagedDataRequestUrl(string.Empty);

                    // Make a locally copy of the network object.
                    var networkObj = work.NetworkObject;

                    // Set the form parameters for paged requests.
                    var formParams = HttpUtility.ParseQueryString(string.Empty);
                    formParams.Add("ttl", "1m");
                    formParams.Add("fields", "attributes");
                    formParams.Add("pagingId", work.NetworkObject.PageId);

                    // Get the staged request.  If no error return true.
                    success = gbdxComms.StagedRequest(ref networkObj, formParams);

                    var pagedResult = JsonConvert.DeserializeObject<PagedData>(networkObj.Result);

                    networkObj.PageId = pagedResult.next_paging_id;
                    networkObj.PageItemCount = Convert.ToInt32( pagedResult.item_count);

                    // Download didn't start so lets try it again.  but after a 250 ms nap to give the service a chance to catch up
                    if (itemsReceived == 0 && !success)
                    {
                        if (work.NumberOfAttempts <= 5)
                        {
                            Thread.Sleep(250);
                            work.NumberOfAttempts++;
                            this.GetVectorData(source);
                            return;
                        }

                        // Attempting to restart this isn't working lets stop and let the user decide what to do
                        work.NetworkObject.ErrorOccurred = true;
                        this.Invoke(new DeserializeJson(this.ArcObjectsDeserializeJson), work);
                        return;
                    }

                    // If we started getting results but the download stalled
                    if (itemsReceived > 0 && !success)
                    {
                        success = this.ItemRetrievalErrorRecovery(gbdxComms, ref networkObj, formParams);

                        // The attempt to recover from the stalled transfer has failed so let's restart the transfer.
                        if (!success)
                        {
                            if (work.NumberOfAttempts <= 5)
                            {
                                work.NumberOfAttempts++;
                                this.GetVectorData(source);

                                return;
                            }

                            // Attempting to restart this isn't working lets stop and let the user decide what to do
                            work.NetworkObject.ErrorOccurred = true;
                            this.Invoke(new DeserializeJson(this.ArcObjectsDeserializeJson), work);
                            return;
                        }
                    }

                    // Check to see if the last page has been received otherwise process it.
                    //if (string.Equals(pagedResult.ToString(), "{}", StringComparison.OrdinalIgnoreCase))
                    if(networkObj.PageItemCount == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        // If successfull update the UI on the progress and add the page of results to the responses.
                        if (success)
                        {
                            itemsReceived += work.NetworkObject.PageItemCount;
                            try
                            {
                                this.UpdateUiPercentageComplete(work, itemsReceived);
                            }
                            catch
                            {
                                // A problem occurred in updating the progress of the transfer.  Most likely that the tree structure
                                // doesnt exist anymore.  So lets stop processing
                                return;
                            }

                            work.NumberOfLines++;
                            var outputStr = pagedResult.data.ToString().Replace("\r", "").Replace("\n","");
                            file.WriteLine(outputStr);
                           }

                        // close If(success)
                    }

                    // close else
                }

                // close the while loop
            }

            // Close using filestream

            // All data has been received Invoke the ArcObjectsDeserilizeJson function on the main UI thread.
            this.Invoke(new DeserializeJson(this.ArcObjectsDeserializeJson), work);
        }

        /// <summary>
        /// The start job.
        /// </summary>
        /// <param name="jobs">
        /// The jobs.
        /// </param>
        private void StartJob(Queue<TreeNode> jobs)
        {
            if (jobs.Count == 0)
            {
                // No jobs left so abort
                return;
            }

            var item = jobs.Dequeue();

            if (item.GetType() == typeof(VectorIndexGeometryNode))
            {
                this.ProcessQueryItemNodeClick(item);
            }
            else
            {
                this.ProcessTypeNodeClick(item);
            }
        }

        #endregion

        #region Smart ThreadPool Event Handelers

        /// <summary>
        /// Event handler that fires after the smart thread pool has completed it's work.
        /// </summary>
        /// <param name="wir">
        /// work item result brought in by the event
        /// </param>
        private void DoPostWork(IWorkItemResult wir)
        {
            // If not on the UI thread switch to it.
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { this.DoPostWork(wir); });
                return;
            }

            var postResult = wir.State as WorkerObject;

            // Add recently merged and written to File GDB to ArcMap as a feature layer.
            if (postResult != null && this.currentApplicationState == postResult.ApplicationState)
            {
                this.AddLayerToMap(postResult);
            }
        }

        /// <summary>
        /// The add layer to map.
        /// </summary>
        /// <param name="workObj">
        /// The WorkerObject.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool AddLayerToMap(WorkerObject workObj)
        {
            var success = false;
            try
            {
                lock (this.locker)
                {
                    var featureWorkspace =
                        (IFeatureWorkspace)Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                    var featureClass = featureWorkspace.OpenFeatureClass(workObj.TableName);
                    ILayer featureLayer;
                    featureLayer = VectorIndexHelper.CreateFeatureLayer(
                        featureClass,
                        workObj.QuerySource ? workObj.GeometryNode.GeometryType.Name : workObj.TypeNode.Type.Name);
                    VectorIndexHelper.AddFeatureLayerToMap(featureLayer);
                    success = true;
                }
            }
            catch (Exception error)
            {
                workObj.Logger.Error(error);
            }

            return success;
        }

        /// <summary>
        /// The write to table.
        /// </summary>
        /// <param name="workspace">
        /// The workspace.
        /// </param>
        /// <param name="work">
        /// The work.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool WriteToTable(IWorkspace workspace, WorkerObject work)
        {
            var success = true;
            if (string.IsNullOrEmpty(work.CombinedJsonData))
            {
                return false;
            }

            try
            {
                var outputTable = VectorIndexHelper.GetTable(work.CombinedJsonData);
                lock (this.locker)
                {
                    outputTable.SaveAsTable(workspace, work.TableName);
                }
            }
            catch (Exception error)
            {
                work.Logger.Error(error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// The merge JSON strings.
        /// </summary>
        /// <param name="work">
        /// The work.
        /// </param>
        /// <returns>
        /// The <see cref="JObject"/>.
        /// </returns>
        private JObject MergeJsonStrings(WorkerObject work)
        {
            int count = 0;
            var mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union };
            var sourceNodeIndex = work.SourceNode.Index;
            var geometryNodeIndex = work.GeometryNode.Index;
            int typeNodeIndex = -1;
            if (!work.QuerySource)
            {
                typeNodeIndex = work.TypeNode.Index;
            }

            double progress;

            JObject jsonObject = null;
            int max = (work.NumberOfLines * 2) + 2;
            try
            {
                string line;
                using (var file = new StreamReader(work.TemporaryFilePath))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        count++;

                        if (jsonObject != null)
                        {
                            jsonObject.Merge(JObject.Parse(line), mergeSettings);
                        }
                        else
                        {
                            jsonObject = JObject.Parse(line);
                        }

                        progress = Math.Round(((double)count / max) * 100, 1) + 50;
                        if (work.QuerySource)
                        {
                            var doneMessage = string.Format(
                                "{0} ({1}): {2}% ",
                                work.GeometryNode.GeometryType.Name,
                                work.GeometryNode.GeometryType.Count,
                                progress);
                            this.UpdateUiProgress(sourceNodeIndex, geometryNodeIndex, doneMessage);
                        }
                        else
                        {
                            var doneMessage = string.Format(
                                "{0} ({1}): {2}% ",
                                work.TypeNode.Type.Name,
                                work.TypeNode.Type.Count,
                                progress);
                            this.UpdateUiProgress(sourceNodeIndex, geometryNodeIndex, typeNodeIndex, doneMessage);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                work.Logger.Error(error);
                return null;
            }

            return jsonObject;
        }

        /// <summary>
        /// The merge properties.
        /// </summary>
        /// <param name="jsonObject">
        /// The JSON object.
        /// </param>
        /// <param name="work">
        /// The work.
        /// </param>
        /// <returns>
        /// The <see cref="JObject"/>.
        /// </returns>
        private JObject MergeProperties(JObject jsonObject, WorkerObject work)
        {
            try
            {
                var fields = new Dictionary<string, FieldDefinition>();
                if (jsonObject != null)
                {
                    var jsonFields = (JArray)jsonObject["fields"];

                    // This for loops looks through all of the fields and updtes the max length for the fields
                    foreach (JToken t in jsonFields)
                    {
                        var tempFieldDefintion = JsonConvert.DeserializeObject<FieldDefinition>(t.ToString());
                        FieldDefinition value;

                        // Attempt to get the value if one is received will evaluate to true and return the object.
                        if (fields.TryGetValue(tempFieldDefintion.Alias, out value))
                        {
                            if (value.Length < tempFieldDefintion.Length)
                            {
                                value.Length = tempFieldDefintion.Length;
                            }
                        }
                        else
                        {
                            fields.Add(tempFieldDefintion.Alias, tempFieldDefintion);
                        }
                    }

                    jsonFields.RemoveAll();
                    foreach (var defintion in fields.Values)
                    {
                        jsonFields.Add(JObject.FromObject(defintion));
                    }
                }
            }
            catch (Exception error)
            {
                work.Logger.Error(error);
                return null;
            }

            return jsonObject;
        }

        /// <summary>
        /// The process vector index response 2.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private object ProcessVectorIndexResponse2(object source)
        {
            var work = (WorkerObject)source;
            var workspace = Jarvis.OpenWorkspace(work.WorkspacePath);
            var sourceNodeIndex = work.SourceNode.Index;
            var geometryNodeIndex = work.GeometryNode.Index;
            int typeNodeIndex = -1;
            if (!work.QuerySource)
            {
                typeNodeIndex = work.TypeNode.Index;
            }

            var jsonObject = this.MergeJsonStrings(work);

            jsonObject = this.MergeProperties(jsonObject, work);

            if (jsonObject != null)
            {
                var mergedJson = jsonObject.ToString(Formatting.None);
                work.CombinedJsonData = mergedJson;
            }

            try
            {
                this.WriteToTable(workspace, work);
            }
            catch (Exception error)
            {
                work.Logger.Error(error);
            }

            if (work.QuerySource)
            {
                var doneMessage = string.Format(
                    "{0} ({1}): {2}% ",
                    work.GeometryNode.GeometryType.Name,
                    work.GeometryNode.GeometryType.Count,
                    100);
                this.UpdateUiProgress(sourceNodeIndex, geometryNodeIndex, doneMessage);
            }
            else
            {
                var doneMessage = string.Format(
                    "{0} ({1}): {2}% ",
                    work.TypeNode.Type.Name,
                    work.TypeNode.Type.Count,
                    100);
                this.UpdateUiProgress(sourceNodeIndex, geometryNodeIndex, typeNodeIndex, doneMessage);
            }

            return true; // all is good
        }

        /// <summary>
        /// The update UI progress.
        /// </summary>
        /// <param name="sourceNodeIndex">
        /// The source node index.
        /// </param>
        /// <param name="geometryNodeIndex">
        /// The geometry node index.
        /// </param>
        /// <param name="typeNodeIndex">
        /// The type node index.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void UpdateUiProgress(int sourceNodeIndex, int geometryNodeIndex, int typeNodeIndex, string message)
        {
            this.treeView1.Invoke(
                new MethodInvoker(
                    () =>
                    this.treeView1.Nodes[sourceNodeIndex].Nodes[geometryNodeIndex].Nodes[typeNodeIndex].Text = message));
        }

        /// <summary>
        /// The update UI progress.
        /// </summary>
        /// <param name="sourceNodeIndex">
        /// The source node index.
        /// </param>
        /// <param name="geometryNodeIndex">
        /// The geometry node index.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void UpdateUiProgress(int sourceNodeIndex, int geometryNodeIndex, string message)
        {
            this.treeView1.Invoke(
                new MethodInvoker(() => this.treeView1.Nodes[sourceNodeIndex].Nodes[geometryNodeIndex].Text = message));
        }

        #endregion

        #region UI Thread Update Functions

        /// <summary>
        /// Once sources have been received update the UI
        /// </summary>
        /// <param name="workObjs">
        /// Worker object
        /// </param>
        private void UpdateTreeViewWithSources(object workObjs)
        {
            var work = (WorkerObject)workObjs;

            if (work.ApplicationState != this.currentApplicationState)
            {
                return;
            }

            var sourceList = work.Types;

            // An error occurred
            if (work.NetworkObject.ErrorOccurred || sourceList == null)
            {
                var newItem = new VectorIndexSourceNode
                                  {
                                      Text = GbdxSettings.GbdxResources.Source_ErrorMessage,
                                      ResponseObject = work.ResponseObject
                                  };
                this.treeView1.Nodes.Clear();
                this.treeView1.Nodes.Add(newItem);
                this.treeView1.CheckBoxes = false;

                // A error occured so stop processing
                return;
            }

            // If no sources found.
            if (work.ResponseObject.Data.Count == 0)
            {
                var newItem = new VectorIndexSourceNode
                                  {
                                      Text =
                                          GbdxSettings.GbdxResources
                                          .VectorIndexDockable_UpdateTreeViewWithSources_No_data_found_,
                                      ResponseObject = work.ResponseObject
                                  };
                this.treeView1.Nodes.Add(newItem);
                this.treeView1.CheckBoxes = false;

                // No sources were found so stop processing
                return;
            }

            // Before populating the tree lets remove the empty node i created and turn checkboxes back on
            this.treeView1.Nodes.Clear();
            this.treeView1.CheckBoxes = true;

            foreach (var item in sourceList)
            {
                var newItem = new VectorIndexSourceNode
                                  {
                                      Text = string.Format("{0} ({1})", item.Name, item.Count),
                                      Source = item,
                                      ResponseObject = work.ResponseObject
                                  };

                this.treeView1.Nodes.Add(newItem);
            }

            this.treeView1.Sort();
        }

        /// <summary>
        /// Updates the treeview with the types for each source.  Ensures that the type is properly nested under the source node.
        /// </summary>
        /// <param name="workObjs">
        /// The WorkerObject
        /// </param>
        private void UpdateTreeviewWithGeometryTypes(object workObjs)
        {
            var worker = (WorkerObject)workObjs;
            if (worker.NetworkObject.ErrorOccurred)
            {
                // Remove the node from the list of checked nodes because an error occurred.
                this.checkedNodes.Remove(this.treeView1.Nodes[worker.SourceNode.Index]);
                this.treeView1.Nodes[worker.SourceNode.Index].Checked = false;
                this.treeView1.Nodes[worker.SourceNode.Index].Text =
                    this.treeView1.Nodes[worker.SourceNode.Index].Text.Replace(
                        GbdxSettings.GbdxResources.SearchingText,
                        GbdxSettings.GbdxResources.Source_ErrorMessage);

                // Don't allow it to process further.
                return;
            }

            // Results were found so lets get rid of the searching text.
            this.treeView1.Nodes[worker.SourceNode.Index].Text =
                this.treeView1.Nodes[worker.SourceNode.Index].Text.Replace(GbdxSettings.GbdxResources.SearchingText, string.Empty);
            foreach (var geoType in worker.GeometryTypes)
            {
                var newItem = new VectorIndexGeometryNode()
                                  {
                                      Source = worker.SourceNode.Source,
                                      GeometryType = geoType,
                                      Text =
                                          string.Format(
                                              "{0} ({1})",
                                              geoType.Name,
                                              geoType.Count),
                                      ResponseObject = worker.ResponseObject
                                  };

                // If we are using querysource then the source node needs to have the "download all" option in the context menu.
                if (this.usingQuerySource)
                {
                    this.treeView1.Nodes[worker.SourceNode.Index].ContextMenuStrip =
                        this.CreateContextMenuStrip(this.treeView1.Nodes[worker.SourceNode.Index]);
                }

                this.treeView1.Nodes[worker.SourceNode.Index].Nodes.Add(newItem);
            }

            this.treeView1.Sort();
        }

        /// <summary>
        /// Called after network thread has received all of the type data.
        /// </summary>
        /// <param name="workObjs">
        /// The WorkerObject
        /// </param>
        private void UpdateTreeviewWithTypes(object workObjs)
        {
            var worker = (WorkerObject)workObjs;

            if (worker.NetworkObject.ErrorOccurred)
            {
                // Remove the node from the list of checked nodes because an error occurred.
                this.checkedNodes.Remove(this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index]);
                this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Checked = false;
                this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Text =
                    this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Text.Replace(
                        GbdxSettings.GbdxResources.SearchingText,
                        GbdxSettings.GbdxResources.Source_ErrorMessage);

                // Don't allow it to process further.
                return;
            }

            this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Text =
                this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Text.Replace(
                    GbdxSettings.GbdxResources.SearchingText,
                    string.Empty);

            foreach (var type in worker.Types)
            {
                if (string.IsNullOrEmpty(type.Name))
                {
                    continue;
                }

                var newItem = new VectorIndexTypeNode()
                                  {
                                      Source = worker.SourceNode.Source,
                                      Geometry = worker.GeometryNode.GeometryType,
                                      Type = type,
                                      Text = string.Format("{0} ({1})", type.Name, type.Count),
                                      ResponseObject = worker.ResponseObject
                                  };
                this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].ContextMenuStrip =
                    this.CreateContextMenuStrip(
                        this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index]);
                this.treeView1.Nodes[worker.SourceNode.Index].Nodes[worker.GeometryNode.Index].Nodes.Add(newItem);
            }

            this.treeView1.Sort();
        }

        /// <summary>
        /// Called after the vector data has been received to add data to the map
        /// </summary>
        /// <param name="source">
        /// The WorkerObject
        /// </param>
        private void ArcObjectsDeserializeJson(object source)
        {
            var work = (WorkerObject)source;

            if (this.jobQueue.Count > 0)
            {
                this.StartJob(this.jobQueue);
            }
            else
            {
                // Reduce the number of working threads
                this.currentThreadCount -= 1;
            }

            if (work.NetworkObject.ErrorOccurred)
            {
                // Remove the node from the list of checked nodes because an error occurred.
                this.checkedNodes.Remove(
                    this.treeView1.Nodes[work.SourceNode.Index].Nodes[work.GeometryNode.Index].Nodes[work.TypeNode.Index
                        ]);
                this.treeView1.Nodes[work.SourceNode.Index].Nodes[work.GeometryNode.Index].Nodes[work.TypeNode.Index]
                    .Checked = false;
                this.treeView1.Nodes[work.SourceNode.Index].Nodes[work.GeometryNode.Index].Nodes[work.TypeNode.Index]
                    .Text += GbdxSettings.GbdxResources.Source_ErrorMessage;

                // Don't allow it to process further.
                return;
            }

            work.WorkspacePath = Settings.Default.geoDatabase;
            if (work.QuerySource)
            {
                work.TableName = "VI_" + work.GeometryNode.GeometryType.Name.Replace(" ", "_") + "QUERY_Search_"
                                 + this.textBoxSearch.Text + "_" + DateTime.Now.ToString("ddMMMHHmmss");
            }
            else
            {
                work.TableName = "VI_" + work.TypeNode.Type.Name.Replace(" ", "_") + work.SourceNode.Source.Name + "_"
                                 + DateTime.Now.ToString("ddMMMHHmmss");
            }

            Regex rgx = new Regex("[^a-zA-Z0-9]");
            work.TableName = rgx.Replace(work.TableName, string.Empty);
            this.smartThreadPool.QueueWorkItem(this.ProcessVectorIndexResponse2, work, this.DoPostWork);
        }

        #endregion

        private void PolygonAoi()
        {
            if (this.networkObject == null)
            {
                this.networkObject = this.SetupNetObject(
                    Settings.Default.username,
                    Settings.Default.password);
            }

            this.networkObject.AuthEndpoint = GbdxHelper.GetAuthenticationEndpoint(Settings.Default);
            this.networkObject.AddressUrl = null;
            this.networkObject.Error = null;
            this.networkObject.ErrorOccurred = false;
            this.networkObject.PageId = string.Empty;
            this.networkObject.ApiKey = Settings.Default.apiKey;
            this.networkObject.AuthUrl = Settings.Default.AuthBase;
            this.networkObject.UsingPolygonAoi = true;
            this.networkObject.PolygonAoi = GetGeoJson();

            // There was a problem in creating the network object so dont proceed.
            if (this.networkObject == null)
            {
                return;
            }

            // Setup worker object 
            var work = new WorkerObject
            {
                BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                Logger = this.logWriter
            };

            var newNetObj = this.networkObject;
            work.NetworkObject = newNetObj;

            if (this.textBoxSearch.Text.Equals(GbdxSettings.GbdxResources.EnterSearchTerms) || this.textBoxSearch.Text == string.Empty)
            {
                this.usingQuerySource = false;

                // Modified to work with Restsharp
                work.NetworkObject.BaseUrl = work.BaseUrl;
                work.NetworkObject.AddressUrl = "/insight-vector/api/shape/sources";
            }
            else
            {
                this.usingQuerySource = true;

                // Modifed to work with restsharp
                work.NetworkObject.BaseUrl = work.BaseUrl;

                work.NetworkObject.AddressUrl = "/insight-vector/api/shape/query/geometries?q=" + this.textBoxSearch.Text;
            }

            this.treeView1.CheckBoxes = false;
            this.treeView1.Nodes.Clear();
            var searchingNode = new VectorIndexSourceNode { Text = GbdxSettings.GbdxResources.SearchingText };
            this.treeView1.Nodes.Add(searchingNode);

            work.ApplicationState = this.currentApplicationState;

            // Offload network communications for getting initial source list to the threadpool for processing.
            ThreadPool.QueueUserWorkItem(
                this.LoadSources,
                new object[] { work, this.comms });
        }
        /// <summary>
        /// Once the envelope of the bounding box has been established start the UVI tree population process.
        /// </summary>
        /// <param name="poly">
        /// The polygon of the bounding box that the user selected.
        /// </param>
        private void VectorIndex(IPolygon poly)
        {
            // Create the initial networkobject
            if (this.networkObject == null)
            {
                this.networkObject = this.SetupNetObject(
                    Settings.Default.username,
                    Settings.Default.password);
            }

            this.networkObject.AuthEndpoint = GbdxHelper.GetAuthenticationEndpoint(Settings.Default);
            this.networkObject.AddressUrl = null;
            this.networkObject.Error = null;
            this.networkObject.ErrorOccurred = false;
            this.networkObject.PageId = string.Empty;
            this.networkObject.ApiKey = Settings.Default.apiKey;
            this.networkObject.AuthUrl = Settings.Default.AuthBase;

            // There was a problem in creating the network object so dont proceed.
            if (this.networkObject == null)
            {
                return;
            }

            // Project the envelope from the polygon to the correct projection
            var envelope = VectorIndexHelper.ProjectToWgs1984(poly.Envelope);

            if (!ArcUtility.ValidateRoundedWgs84Polygon(envelope))
            {
                MessageBox.Show(GbdxSettings.GbdxResources.invalidBoundingBox);
                if (this.boundingBoxGraphicElement != null)
                {
                    ArcUtility.DeleteElementFromGraphicContainer(
                        ArcMap.Document.ActiveView,
                        this.boundingBoxGraphicElement);
                    this.boundingBoxGraphicElement = null;
                }

                return;
            }

            // Setup bounding box
            this.bBox = new BoundingBox
                            {
                                Xmax = envelope.XMax,
                                Ymax = envelope.YMax,
                                Xmin = envelope.XMin,
                                Ymin = envelope.YMin
                            };

            // Setup worker object 
            var work = new WorkerObject
                           {
                               BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                               BoundBox = this.bBox,
                               Logger = this.logWriter
                           };

            var newNetObj = this.networkObject;
            work.NetworkObject = newNetObj;

            if (this.textBoxSearch.Text.Equals(GbdxSettings.GbdxResources.EnterSearchTerms) || this.textBoxSearch.Text == string.Empty)
            {
                this.usingQuerySource = false;

                // Modified to work with Restsharp
                work.NetworkObject.BaseUrl = work.BaseUrl;
                work.NetworkObject.AddressUrl = VectorIndexHelper.CreateUrl(work.BoundBox, string.Empty);
            }
            else
            {
                this.usingQuerySource = true;

                // Modifed to work with restsharp
                work.NetworkObject.BaseUrl = work.BaseUrl;
                work.NetworkObject.AddressUrl =
                    VectorIndexHelper.CreateQueryUrl(work.BoundBox, string.Empty, this.textBoxSearch.Text);
            }

            this.treeView1.CheckBoxes = false;
            var searchingNode = new VectorIndexSourceNode { Text = GbdxSettings.GbdxResources.SearchingText };
            this.treeView1.Nodes.Add(searchingNode);

            work.ApplicationState = this.currentApplicationState;

            // Offload network communications for getting initial source list to the threadpool for processing.
            ThreadPool.QueueUserWorkItem(
                this.LoadSources,
                new object[] { work, this.comms});
        }

        /// <summary>
        /// The user authentication check.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="pass">
        /// The pass.
        /// </param>
        /// <param name="cloudComms">
        /// The cloud comms.
        /// </param>
        /// <param name="suppressMessageBox">
        /// The suppress message box.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool UserAuthenticationCheck(
            Settings settings,
            ref string user,
            ref string pass,
            IGbdxComms cloudComms,
            bool suppressMessageBox)
        {
            if (string.IsNullOrEmpty(settings.username) || string.IsNullOrEmpty(settings.password))
            {
                if (!suppressMessageBox)
                {
                    MessageBox.Show(GbdxSettings.GbdxResources.InvalidUserPass);
                }

                return false;
            }

            if (!this.CheckCredentials(settings, ref user, ref pass))
            {
                // Wipe out previous network object
                this.networkObject = null;

                // Re create the network object
                this.networkObject = this.SetupNetObject(settings.username, settings.password);
                this.networkObject.AuthUrl = string.IsNullOrEmpty(Settings.Default.AuthBase)
                                                 ? Settings.Default.DefaultAuthBase
                                                 : Settings.Default.AuthBase;
                // Authorize the network object if it fails inform the user.
                if (cloudComms.AuthenticateNetworkObject(ref this.networkObject))
                {
                    // Authentication was succesful lets continue.
                    return true;
                }

                user = string.Empty;
                pass = string.Empty;
                if (!suppressMessageBox)
                {
                    MessageBox.Show(GbdxSettings.GbdxResources.InvalidUserPass);
                }

                return false;
            }

            // Everything checked out so lets continue
            return true;
        }

        /// <summary>
        /// The check credentials.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="pass">
        /// The pass.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CheckCredentials(Settings settings, ref string user, ref string pass)
        {
            if (!string.Equals(settings.username, user) || !string.Equals(settings.password, pass))
            {
                user = settings.username;

                // This will be the encrypted value. 
                pass = settings.password;
                return false;
            }

            return true;
        }

        /// <summary>
        /// The event handler for when the key up event for the search textbox is triggered.
        /// </summary>
        /// <param name="sender">
        /// textbox listening for the key up event.  
        /// </param>
        /// <param name="e">
        /// Event arguments
        /// </param>
        private void TextBoxSearchKeyUp(object sender, KeyEventArgs e)
        {
            // Only care about the enter key
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            // Check user authentication
            if (!this.UserAuthenticationCheck(Settings.Default, ref this.username, ref this.password, this.comms, false))
            {
                return;
            }

            this.currentApplicationState = this.applicationStateGenerator.Next();
            this.treeView1.Nodes.Clear();
            this.checkedNodes.Clear();
            this.treeView1.CheckBoxes = true;
            this.usingQuerySource = false;

            // if the boundingbox graphic element hasn't been pressed then
            // lets assume that we use the curren't active views extent as the envelope.
            if (this.boundingBoxGraphicElement == null)
            {
                var poly = VectorIndexHelper.DisplayRectangle(
                    ArcMap.Document.ActiveView,
                    out this.boundingBoxGraphicElement);

                // Kick off vector index functionality
                this.VectorIndex(poly);
                return;
            }

            // We already have a bounding box drawn so lets re-use that without redrawing the aoi.
            var tempPolygon = (IPolygon)this.boundingBoxGraphicElement.Geometry;
            this.VectorIndex(tempPolygon);
        }

        /// <summary>
        /// The create context menu strip.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// The <see cref="UviContextMenuStrip"/>.
        /// </returns>
        private UviContextMenuStrip CreateContextMenuStrip(TreeNode node)
        {
            var menuStrip = new UviContextMenuStrip(node);
            var downloadAllToolStrip = new ToolStripMenuItem
                                           {
                                               Text = GbdxSettings.GbdxResources.Download_all_Below_100000,
                                               Name = "DownloadAll"
                                           };
            downloadAllToolStrip.Click += this.DownloadAllToolStripOnClick;
            menuStrip.Items.Add(downloadAllToolStrip);

            return menuStrip;
        }

        /// <summary>
        /// Resets the vector index.  Deletes the element from the graphic container, clears the the treeview and search textbox
        /// and changes the current applications tate.
        /// </summary>
        private void ResetVectorIndex()
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

        /// <summary>
        /// The update UI percentage complete.
        /// </summary>
        /// <param name="work">
        /// The work.
        /// </param>
        /// <param name="itemsReceived">
        /// The items received.
        /// </param>
        private void UpdateUiPercentageComplete(WorkerObject work, int itemsReceived)
        {
            if (!work.QuerySource)
            {
                var sourceNodeIndex = work.SourceNode.Index;
                var geometryNodeIndex = work.GeometryNode.Index;
                var typeNodeIndex = work.TypeNode.Index;
                var percentComplete = (((double)(100 * itemsReceived / work.TypeNode.Type.Count)) / 200) * 100;
                var message = string.Format(
                    "{0} ({1}): {2}% ",
                    work.TypeNode.Type.Name,
                    work.TypeNode.Type.Count,
                    percentComplete);

                // Update UI on progress
                this.treeView1.Invoke(
                    new MethodInvoker(
                        () =>
                        this.treeView1.Nodes[sourceNodeIndex].Nodes[geometryNodeIndex].Nodes[typeNodeIndex].Text =
                        message));
            }
            else
            {
                // updates the query tree.
                // The query tree structure is 1 level shorter than the normal vector index tree.
                var sourceNodeIndex = work.SourceNode.Index;
                var geometryNodeIndex = work.GeometryNode.Index;
                var percentComplete = (((double)(100 * itemsReceived / work.GeometryNode.GeometryType.Count)) / 200)
                                      * 100;
                var message = string.Format(
                    "{0} ({1}): {2}% ",
                    work.GeometryNode.GeometryType.Name,
                    work.GeometryNode.GeometryType.Count,
                    percentComplete);

                // Update UI on progress
                this.treeView1.Invoke(
                    new MethodInvoker(
                        () => this.treeView1.Nodes[sourceNodeIndex].Nodes[geometryNodeIndex].Text = message));
            }
        }

        /// <summary>
        /// The download all tool strip on click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
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
        /// Event handler to handle the events of when the form's visibility changes.
        /// </summary>
        /// <param name="sender">
        /// sender of the event
        /// </param>
        /// <param name="e">
        /// event arguments
        /// </param>
        private void VectorIndexDockableVisibleChanged(object sender, EventArgs e)
        {
            this.ResetVectorIndex();
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            /// <summary>
            /// The Dockable vector index window.
            /// </summary>
            private VectorIndexDockable mWindowUi;

            /// <summary>
            /// Initializes a new instance of the <see cref="AddinImpl"/> class.
            /// </summary>
            public AddinImpl()
            {
            }

            /// <summary>
            /// The on create child.
            /// </summary>
            /// <returns>
            /// The <see cref="IntPtr"/>.
            /// </returns>
            protected override IntPtr OnCreateChild()
            {
                this.mWindowUi = new VectorIndexDockable(this.Hook);
                return this.mWindowUi.Handle;
            }

            /// <summary>
            /// The dispose.
            /// </summary>
            /// <param name="disposing">
            /// The disposing.
            /// </param>
            protected override void Dispose(bool disposing)
            {
                if (this.mWindowUi != null)
                {
                    this.mWindowUi.Dispose(disposing);
                }

                base.Dispose(disposing);
            }
        }
        #endregion
    } // closes VectorIndexDockable Class
}