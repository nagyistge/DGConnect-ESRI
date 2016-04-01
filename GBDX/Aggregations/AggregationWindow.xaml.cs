// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregationWindow.xaml.cs" company="DigitalGlobe">
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

namespace Gbdx.Aggregations
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using System.Windows.Threading;

    using global::Aggregations;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using GbdxTools;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Logging;

    using NetworkConnections;

    using RestSharp;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;
    using MessageBox = System.Windows.Forms.MessageBox;
    using UserControl = System.Windows.Controls.UserControl;

    public delegate void SendAnInt(int val);

    public delegate void UpdateAggWindowPbar(int max, int min, int val);

    /// <summary>
    /// Designer class of the dockable window add-in. It contains WPF user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class AggregationWindow : UserControl
    {
        /// <summary>
        /// used for writing to the log files.
        /// </summary>
        private readonly Logger logWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationWindow"/> class.
        /// </summary>
        public AggregationWindow()
        {
            this.InitializeComponent();
            this.logWriter = new Logger(Jarvis.LogFile, false);
            this.startDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            this.endDatePicker.SelectedDate = DateTime.Now;

            this.startDatePicker.DisplayDateEnd = this.endDatePicker.SelectedDate.Value;
            this.endDatePicker.DisplayDateStart = this.startDatePicker.SelectedDate;

            this.Client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
            string unencryptedPassword;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out unencryptedPassword);

            if (result)
            {
                this.Authenticate(
                    Settings.Default.username,
                    unencryptedPassword,
                    this.Client,
                    GbdxHelper.GetAuthenticationEndpoint(Settings.Default),
                    true);
            }

            AggregationRelay.Instance.AoiHasBeenDrawn += this.InstanceAoiHasBeenDrawn;
        }

        /// <summary>
        /// Gets or sets the authorization.
        /// </summary>
        private AccessToken Authorization { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        private IRestClient Client { get; set; }

        /// <summary>
        /// Gets or sets the AOI polygon.
        /// </summary>
        private IPolygon AoiPolygon { get; set; }

        /// <summary>
        /// Gets or sets the search item.
        /// </summary>
        private string SearchItem { get; set; }

        /// <summary>
        /// Element of the existing bounding box
        /// </summary>
        private IElement BoundingBoxElmement { get; set; }

        /// <summary>
        /// Gets or sets the previously selected arcmap selected tool.
        /// </summary>
        private ICommandItem PreviouslySelectedItem { get; set; }

        /// <summary>
        /// The authenticate.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="pass">
        /// The pass.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="auth">
        /// The auth.
        /// </param>
        /// <param name="runAsync">
        /// The run Async.
        /// </param>
        private void Authenticate(string user, string pass, IRestClient client, string auth, bool runAsync)
        {
            var tempClient = new RestClient(Settings.Default.AuthBase);

            var request = new RestRequest(auth, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // Add the 64 base string representation to the header
            request.AddHeader("Authorization", string.Format("Basic {0}", Settings.Default.apiKey));

            request.AddParameter("grant_type", "password");
            request.AddParameter("username", user);
            request.AddParameter("password", pass);

            if (runAsync)
            {
                tempClient.ExecuteAsync<AccessToken>(
                    request,
                    resp =>
                        {
                            if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                            {
                                this.Authorization = resp.Data;
                            }
                            else
                            {
                                MessageBox.Show(GbdxResources.InvalidUserPass);
                            }
                        });
            }
            else
            {
                var response = tempClient.Execute<AccessToken>(request);
                if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                {
                    this.Authorization = response.Data;
                }
                else
                {
                    MessageBox.Show(GbdxResources.InvalidUserPass);
                }
            }
        }

        /// <summary>
        /// The instance AOI has been drawn.
        /// </summary>
        /// <param name="poly">
        /// The poly.
        /// </param>
        /// <param name="elm">
        /// The elm.
        /// </param>
        private void InstanceAoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            poly.Project(Jarvis.ProjectedCoordinateSystem);
            this.AoiPolygon = poly;
        }

        /// <summary>
        /// The create AGGS argument.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string CreateAggsArgument()
        {
            var selectedItem = ((ComboBoxItem)this.QuerySelectionComboBox.SelectedItem).Content.ToString();

            switch (selectedItem)
            {
                case "What data is available in the region?":
                    return "terms:ingest_source";

                case "What is the twitter sentiment in the region?":
                    return "avg:attributes.sentiment_positive_dbl";

                case "What type of data is available in the region?":
                    return "terms:item_type";
            }

            // Default to return available source in AOI.
            return "terms:ingest_source";
        }

        /// <summary>
        /// The go button_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void GoButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.AoiPolygon == null && this.selectionTypeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show(GbdxResources.invalidBoundingBox);
                return;
            }

            if (this.QuerySelectionComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(GbdxResources.noAggregationSelected);
                return;
            }

            if (this.detailGranularityComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(GbdxResources.pleaseChooseDetailsGranularity);
                return;
            }

            // Create new request to the aggregation service.
            var request = new RestRequest("/insight-vector/api/aggregation");
            
            if (this.Authorization == null)
            {
                string unencryptedPassword;
                var result = Aes.Instance.Decrypt128(Settings.Default.password, out unencryptedPassword);

                if (result)
                {
                    this.Authenticate(
                        Settings.Default.username,
                        unencryptedPassword,
                        this.Client,
                        GbdxHelper.GetAuthenticationEndpoint(Settings.Default),
                        false);
                }
                else
                {
                    MessageBox.Show(GbdxResources.problemDecryptingPassword);
                    return;
                }

                if (this.Authorization == null)
                {
                    MessageBox.Show(GbdxResources.InvalidUserPass);
                    return;
                }
            }

            // Add header that says we are allowed to use the service
            request.AddHeader(
                "Authorization",
                string.Format("{0} {1}", this.Authorization.token_type, this.Authorization.access_token));

            this.CreateAggregationArguments(ref request);

            // Lets prevent aggregation calls from being spammed until the current one completes.
            this.goButton.IsEnabled = false;

            // the user selected use selected features for AOI
            if (this.selectionTypeComboBox.SelectedIndex == 1)
            {
                var aoi = Jarvis.ConvertPolygonsToGeoJson(Jarvis.GetPolygons(ArcMap.Document.FocusMap));
                request.Method = Method.POST;
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", aoi, ParameterType.RequestBody);
            }

            this.Client.ExecuteAsync<MotherOfGodAggregations>(request, response => this.HandleResponse(response));
        }

        private void HandleResponse(IRestResponse<MotherOfGodAggregations> response)
        {
            if (response.Data != null)
            {
                var workspace = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                var resultDictionary = new Dictionary<string, Dictionary<string, double>>();
                var uniqueFieldNames = new Dictionary<string, string>();

                AggregationHelper.ProcessAggregations(
                    response.Data.aggregations,
                    0,
                    ref resultDictionary,
                    string.Empty,
                    false,
                    ref uniqueFieldNames);

                // Create a unique name for the feature class based on name.
                var featureClassName = "Aggregation" + DateTime.Now.ToString("ddMMMHHmmss");

                // Function being called really says it all but ... lets CREATE A FEATURE CLASS
                var featureClass = Jarvis.CreateStandaloneFeatureClass(
                    workspace,
                    featureClassName,
                    uniqueFieldNames,
                    false,
                    0);

                this.WriteToFeatureClass(featureClass, resultDictionary, uniqueFieldNames, workspace);

                // Use the dispatcher to make sure the following calls occur on the MAIN thread.
                this.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (MethodInvoker)delegate
                        {
                            this.AddLayerToArcMap(featureClassName);
                            this.goButton.IsEnabled = true;
                        });
            }
            else
            {
                var error = string.Format("\n{0}\n\n{1}", response.ResponseUri.AbsoluteUri, response.Content);
                this.logWriter.Error(error);

                this.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (MethodInvoker)delegate { this.goButton.IsEnabled = true; });

                MessageBox.Show(GbdxResources.Source_ErrorMessage);
            }
        }

        /// <summary>
        /// Create the aggregation AGGS argument
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        private void CreateAggregationArguments(ref RestRequest request)
        {
            var argument = this.CreateAggsArgument();
            this.SearchItem = argument;

            // create the aggregation that is about to be processed.
            var agg = string.Format(
                "geohash:{0};{1}",
                ((ComboBoxItem)this.detailGranularityComboBox.SelectedItem).Content,
                argument);
            request.AddParameter("aggs", agg,ParameterType.QueryString);

            if (this.tbFilter != null && this.tbFilter.Text != null && this.tbFilter.Text != "")
            {
                request.AddParameter("query", this.tbFilter.Text, ParameterType.QueryString);
            }

            // If the user setup a custom date range then use that otherwise assume no date range has been specified.
            if (this.specifyDateCheckbox.IsChecked != null && this.specifyDateCheckbox.IsChecked.Value)
            {
                // Make sure we don't do something illegal like trying to access a null value.
                if (this.startDatePicker.SelectedDate != null && this.endDatePicker.SelectedDate != null)
                {
                    request.AddParameter("startDate", this.startDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"), ParameterType.QueryString);
                    request.AddParameter("endDate", this.endDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"), ParameterType.QueryString);
                }
            }
            request.AddParameter("count", 100000, ParameterType.QueryString);
            if (this.selectionTypeComboBox.SelectedIndex != 0)
            {
                return;
            }

            // Add the important AOI points.
            request.AddParameter("left", this.AoiPolygon.Envelope.UpperLeft.X, ParameterType.QueryString);
            request.AddParameter("right", this.AoiPolygon.Envelope.UpperRight.X, ParameterType.QueryString);
            request.AddParameter("upper", this.AoiPolygon.Envelope.UpperLeft.Y, ParameterType.QueryString);
            request.AddParameter("lower", this.AoiPolygon.Envelope.LowerLeft.Y, ParameterType.QueryString);
            
        }

        /// <summary>
        /// Adds a layer to arc map
        /// </summary>
        /// <param name="featureClassName">
        /// The feature class name.
        /// </param>
        private void AddLayerToArcMap(string featureClassName)
        {
            var featureWorkspace = (IFeatureWorkspace)Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
            var openMe = featureWorkspace.OpenFeatureClass(featureClassName);
            IFeatureLayer featureLayer = new FeatureLayerClass();
            featureLayer.FeatureClass = openMe;
            var layer = (ILayer)featureLayer;
            layer.Name = featureClassName;
            ArcMap.Document.AddLayer(layer);
        }

        /// <summary>
        /// Insert a row into the feature class.
        /// </summary>
        /// <param name="featureClass">
        /// The feature class that will have rows inserted
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="insertCur">
        /// The insert cur.
        /// </param>
        /// <param name="resultDictionary">
        /// The result dictionary.
        /// </param>
        /// <param name="uniqueFieldNames">
        /// The unique field names.
        /// </param>
        private void InsertRow(
            IFeatureClass featureClass,
            string key,
            IFeatureCursor insertCur,
            Dictionary<string, Dictionary<string, double>> resultDictionary,
            Dictionary<string, string> uniqueFieldNames)
        {
            try
            {
                // get the polygon of the geohash
                var poly = this.GetGeoHashPoly(key);
                var buffer = featureClass.CreateFeatureBuffer();

                // Setup the features geometry.
                buffer.Shape = (IGeometry)poly;
                buffer.Value[featureClass.FindField("GeoHash")] = key;
                foreach (var subKey in resultDictionary[key].Keys)
                {
                    var field = uniqueFieldNames[subKey];
                    var value = resultDictionary[key][subKey];
                    var index = featureClass.FindField(field);

                    if(index != -1)
                    {
                        buffer.Value[index] = value;
                    }
                }

                // Feature has been created so add to the feature class.
                insertCur.InsertFeature(buffer);
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        private void InsertPivoTableRowsToFeatureClass(
            IFeatureClass featureClass,
            PivotTable ptable,
            Dictionary<string, string> uniqueFieldNames)
        {
            // get the polygon of the geohash
            //uniqueFieldNames.Add("cos_sim", "cos_sim");
            IFeatureCursor insertCur = featureClass.Insert(true);
            this.pbarChangeDet.Maximum = ptable.Count;
            this.pbarChangeDet.Minimum = 0;
            this.pbarChangeDet.Value = 0;
            int i = 0;
            foreach (PivotTableEntry entry in ptable)
            {
                i++;
                this.UpdatePBar(i);

                var poly = this.GetGeoHashPoly(entry.RowKey);
                var buffer = featureClass.CreateFeatureBuffer();

                // Setup the features geometry.
                buffer.Shape = (IGeometry)poly;
                foreach (String val in entry.Data.Keys)
                {
                    if (uniqueFieldNames.ContainsKey(val))
                    {
                        buffer.Value[featureClass.FindField(uniqueFieldNames[val])] = entry.Data[val];
                    }
                }
                // Feature has been created so add to the feature class.
                insertCur.InsertFeature(buffer);
            }
            insertCur.Flush();
        }

        /// <summary>
        /// Write the feature class to disk
        /// </summary>
        /// <param name="featureClass">
        /// The feature class.
        /// </param>
        /// <param name="resultDictionary">
        /// The result dictionary.
        /// </param>
        /// <param name="uniqueFieldNames">
        /// The unique field names.
        /// </param>
        /// <param name="workspace">
        /// The workspace.
        /// </param>
        private void WriteToFeatureClass(
            IFeatureClass featureClass,
            Dictionary<string, Dictionary<string, double>> resultDictionary,
            Dictionary<string, string> uniqueFieldNames,
            IWorkspace workspace)
        {
            try
            {
                var insertCur = featureClass.Insert(true);
                foreach (var key in resultDictionary.Keys)
                {
                    this.InsertRow(featureClass, key, insertCur, resultDictionary, uniqueFieldNames);
                }

                // Flush all writes to the GBD
                insertCur.Flush();

                // Now that creation of the feature class has been completed release all the handlers used.
                Marshal.ReleaseComObject(insertCur);
                Marshal.ReleaseComObject(featureClass);
                Marshal.ReleaseComObject(workspace);
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The get geo hash poly.
        /// </summary>
        /// <param name="geohash">
        /// The geo hash.
        /// </param>
        /// <returns>
        /// The <see cref="Polygon"/>.
        /// </returns>
        private Polygon GetGeoHashPoly(string geohash)
        {
            var coords = Jarvis.DecodeBbox(geohash);
            IPoint pt1 = new PointClass();
            pt1.PutCoords(coords[1], coords[2]);
            IPoint pt2 = new PointClass();
            pt2.PutCoords(coords[3], coords[2]);
            IPoint pt3 = new PointClass();
            pt3.PutCoords(coords[3], coords[0]);
            IPoint pt4 = new PointClass();
            pt4.PutCoords(coords[1], coords[0]);

            Polygon poly = new PolygonClass();
            poly.AddPoint(pt1);
            poly.AddPoint(pt2);
            poly.AddPoint(pt3);
            poly.AddPoint(pt4);
            poly.AddPoint(pt1);

            return poly;
        }

        /// <summary>
        /// Enable/disable functionality based on if the specify date checkbox has been clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SpecifyDateCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (this.specifyDateCheckbox.IsChecked == null)
            {
                return;
            }

            this.startDatePicker.IsEnabled = this.specifyDateCheckbox.IsChecked.Value;
            this.endDatePicker.IsEnabled = this.specifyDateCheckbox.IsChecked.Value;
            this.startingDateLabel.IsEnabled = this.specifyDateCheckbox.IsChecked.Value;
            this.endDateLabel.IsEnabled = this.specifyDateCheckbox.IsChecked.Value;
        }

        /// <summary>
        /// The event handler for select area button click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SelectAreaButtonClick(object sender, RoutedEventArgs e)
        {

            if (this.BoundingBoxElmement != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActivatedView, this.BoundingBoxElmement);
                this.BoundingBoxElmement = null;
            }

            // if there was already a listener established close it.
            AggregationRelay.Instance.AoiHasBeenDrawn -= this.InstanceOnAoiHasBeenDrawn;
            AggregationRelay.Instance.AoiHasBeenDrawn += this.InstanceOnAoiHasBeenDrawn;

            var commandBars = ArcMap.Application.Document.CommandBars;
            var commandId = new UIDClass() { Value = ThisAddIn.IDs.Gbdx_Aggregations_AggregationSelector };
            var commandItem = commandBars.Find(commandId, false, false);

            // save currently selected tool to be re-selected after the AOI has been drawn.
            if (commandItem != null)
            {
                this.PreviouslySelectedItem = ArcMap.Application.CurrentTool;
                ArcMap.Application.CurrentTool = commandItem;
            }
        }

        /// <summary>
        /// The instance on AOI has been drawn event handler.
        /// </summary>
        /// <param name="poly">
        /// The poly.
        /// </param>
        /// <param name="elm">
        /// The elm.
        /// </param>
        private void InstanceOnAoiHasBeenDrawn(IPolygon poly, IElement elm)
        {

            // stop listening
            AggregationRelay.Instance.AoiHasBeenDrawn -= this.InstanceOnAoiHasBeenDrawn;

            this.BoundingBoxElmement = elm;

            // All done so return the selected tool to the same one the user had previously.
            ArcMap.Application.CurrentTool = this.PreviouslySelectedItem;
        }

        /// <summary>
        /// The end date picker calendar closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void EndDatePickerCalendarClosed(object sender, RoutedEventArgs e)
        {
            this.startDatePicker.DisplayDateEnd = this.endDatePicker.SelectedDate;
        }

        /// <summary>
        /// The start date picker calendar closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void StartDatePickerCalendarClosed(object sender, RoutedEventArgs e)
        {
            this.endDatePicker.DisplayDateStart = this.startDatePicker.SelectedDate;
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            /// <summary>
            /// The m_window UI.
            /// </summary>
            private ElementHost mWindowUi;

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
                this.mWindowUi = new ElementHost();
                this.mWindowUi.Child = new AggregationWindow();
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
                    this.mWindowUi.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        //do not call this via a delegate from another thread
        private void UpdatePBar(int val)
        {
            this.pbarChangeDet.Value = val;
            System.Windows.Forms.Application.DoEvents();
        }

        //do not call this via a delegate from another thread
        private void SetPBarProperties(int max, int min, int val)
        {
            this.pbarChangeDet.Minimum = min;
            this.pbarChangeDet.Maximum = max;
            this.pbarChangeDet.Value = val;
            System.Windows.Forms.Application.DoEvents();
        }

        private void UpdateStatusLabel(String label)
        {
            this.lblPbarStatus.Content = label;
            System.Windows.Forms.Application.DoEvents();
        }

        private void buttAnalyzeAgg_Click(object sender, RoutedEventArgs e)
        {
            ESRI.ArcGIS.ArcMapUI.IContentsView cView = this.GetContentsViewFromArcMap(ArcMap.Application, 0);
            IActiveView aView = this.GetActiveViewFromArcMap(ArcMap.Application);
            List<IFeatureLayer> layers = this.GetFeatureLayersFromToc(aView);

            if (!this.cbAggLayerA.Items.IsEmpty)
            {
                this.cbAggLayerA.Items.Clear();
                this.cbAggLayerB.Items.Clear();
            }

            if (!this.cbAggLayerA.Items.IsEmpty)
            {
                for (int i = 0; i < this.cbAggLayerA.Items.Count; i++)
                {
                    this.cbAggLayerA.Items.RemoveAt(i);
                }
            }
            if (!this.cbAggLayerB.Items.IsEmpty)
            {
                for (int i = 0; i < this.cbAggLayerB.Items.Count; i++)
                {
                    this.cbAggLayerB.Items.RemoveAt(i);
                }
            }

            foreach (IFeatureLayer layer in layers)
            {
                // MessageBox.Show(layer.Name + " -- " + layer.DataSourceType);
                if (layer.Name.ToLower().Contains("aggregation"))
                {
                    this.cbAggLayerA.Items.Add(layer.Name);
                    this.cbAggLayerB.Items.Add(layer.Name);
                }
            }
            if (this.cbAggLayerA.Items.Count < 1)
            {
                MessageBox.Show("No aggregation layers available");
            }
        }

        public PivotTable FeatureLayerToPivotTable(
            IFeatureLayer layer,
            String rowKeyColName,
            List<String> columnsToIgnore)
        {
            SendAnInt sai = new SendAnInt(this.UpdatePBar);
            this.pbarChangeDet.Minimum = 0;
            this.pbarChangeDet.Maximum = layer.FeatureClass.FeatureCount(null);
            this.pbarChangeDet.Value = 0;

            if (columnsToIgnore == null)
            {
                columnsToIgnore = new List<String>();
            }
            if (!columnsToIgnore.Contains("OBJECTID"))
            {
                columnsToIgnore.Add("OBJECTID");
            }
            PivotTable pt = new PivotTable();
            if (PivotTableCache.Cache.ContainsKey(layer.Name))
            {
                pt = PivotTableCache.Cache[layer.Name];
                return pt;
            }

            IFeatureCursor featureCursor = layer.FeatureClass.Search(null, false);
            IFeature feature = featureCursor.NextFeature();
            // loop through the returned features and get the value for the field
            int x = 0;
            while (feature != null)
            {
                PivotTableEntry entry = new PivotTableEntry();
                //do something with each feature(ie update geometry or attribute)
                //  Console.WriteLine("The {0} field contains a value of {1}", nameOfField, feature.get_Value(fieldIndexValue));
                this.pbarChangeDet.Value++;
                sai.Invoke(x);
                x++;
                for (int i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (this.pbarChangeDet.Value == this.pbarChangeDet.Maximum)
                    {
                        this.pbarChangeDet.Maximum = this.pbarChangeDet.Maximum + 10;
                    }

                    string f = feature.Fields.get_Field(i).Name;
                    string val = feature.get_Value(i).ToString();

                    if (columnsToIgnore.Contains(f))
                    {
                        continue;
                    }

                    if (f.Equals(rowKeyColName))
                    {
                        entry.RowKey = Convert.ToString(val);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            entry.Data.Add(f, int.Parse(val));
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                }
                pt.Add(entry);
                feature = featureCursor.NextFeature();
            }

            sai.Invoke(Convert.ToInt32(this.pbarChangeDet.Maximum));
            //add to the cache
            if (!PivotTableCache.Cache.ContainsKey(layer.Name))
            {
                PivotTableCache.Cache.Add(layer.Name, pt);
            }
            return pt;
        }

        public PivotTable FeaturesToPivotTable(
            List<IFeature> layers,
            String rowKeyColName,
            List<String> columnsToIgnore)
        {
            SendAnInt sai = new SendAnInt(this.UpdatePBar);
            this.pbarChangeDet.Minimum = 0;
            this.pbarChangeDet.Maximum = layers.Count;
            this.pbarChangeDet.Value = 0;

            if (columnsToIgnore == null)
            {
                columnsToIgnore = new List<String>();
            }
            if (!columnsToIgnore.Contains("OBJECTID"))
            {
                columnsToIgnore.Add("OBJECTID");
            }
            PivotTable pt = new PivotTable();

            // IFeature feature = featureCursor.NextFeature();
            // loop through the returned features and get the value for the field
            int x = 0;
            foreach (IFeature feature in layers)
            {
                PivotTableEntry entry = new PivotTableEntry();
                //do something with each feature(ie update geometry or attribute)
                //  Console.WriteLine("The {0} field contains a value of {1}", nameOfField, feature.get_Value(fieldIndexValue));
                this.pbarChangeDet.Value++;
                sai.Invoke(x);
                x++;
                for (int i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (this.pbarChangeDet.Value == this.pbarChangeDet.Maximum)
                    {
                        this.pbarChangeDet.Maximum = this.pbarChangeDet.Maximum + 10;
                    }

                    string fname = feature.Fields.get_Field(i).Name;
                    string val = feature.get_Value(i).ToString();

                    if (columnsToIgnore.Contains(fname))
                    {
                        continue;
                    }

                    if (fname.Equals(rowKeyColName))
                    {
                        entry.RowKey = Convert.ToString(val);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            entry.Data.Add(fname, int.Parse(val));
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                }
                pt.Add(entry);
            }
            sai.Invoke(Convert.ToInt32(this.pbarChangeDet.Maximum));
            return pt;
        }

        ///<summary>Get the Contents View (TOC) for ArcMap.</summary>
        ///
        ///<param name="application">An IApplication interface that is the ArcMap application.</param>
        ///<param name="index">A System.Int32 that is the tab number of the TOC. When specifying the index number: 0 = usually the Display tab, 1 = usually the Source tab.</param>
        /// 
        ///<returns>An IContentsView interface.</returns>
        /// 
        ///<remarks></remarks>
        public ESRI.ArcGIS.ArcMapUI.IContentsView GetContentsViewFromArcMap(
            IApplication application,
            Int32 index)
        {
            if (application == null || index < 0 || index > 1)
            {
                return null;
            }

            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = (ESRI.ArcGIS.ArcMapUI.IMxDocument)(application.Document);
                // Explicit Cast
            ESRI.ArcGIS.ArcMapUI.IContentsView contentsView = mxDocument.get_ContentsView(index);
                // 0 = usually the Display tab, 1 = usually the Source tab

            return contentsView;
        }

        ///<summary>Get ActiveView from ArcMap</summary>
        ///  
        ///<param name="application">An IApplication interface that is the ArcMap application.</param>
        ///   
        ///<returns>An IActiveView interface.</returns>
        ///   
        ///<remarks></remarks>
        public IActiveView GetActiveViewFromArcMap(IApplication application)
        {
            if (application == null)
            {
                return null;
            }
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument;
                // Dynamic Cast
            IActiveView activeView = mxDocument.ActiveView;

            return activeView;
        }

        public List<IFeatureLayer> GetFeatureLayersFromToc(IActiveView activeView)
        {
            List<IFeatureLayer> outlist = new List<IFeatureLayer>();

            if (activeView == null)
            {
                return null;
            }
            IMap map = activeView.FocusMap;

            for (int i = 0; i < map.LayerCount; i++)
            {
                if (activeView.FocusMap.get_Layer(i) is IFeatureLayer)
                {
                    outlist.Add((IFeatureLayer)activeView.FocusMap.get_Layer(i));
                }
            }

            return outlist;
        }

        public IFeatureLayer getLayerByName(List<IFeatureLayer> layers, String name)
        {
            foreach (IFeatureLayer layer in layers)
            {
                if (layer.Name.Equals(name))
                {
                    return layer;
                }
            }
            return null;
        }

        public List<IFeature> getSelectedFeatureFromLayerByName(
            List<IFeatureLayer> layers,
            String nameOfLayer)
        {
            IFeatureLayer l = null;
            foreach (IFeatureLayer layer in layers)
            {
                if (layer.Name.Equals(nameOfLayer))
                {
                    l = layer;
                    break;
                }
            }
            return this.GetSelectedFeatures(l);
        }

        public List<IFeature> GetSelectedFeatures(IFeatureLayer featureLayer)
        {
            IFeatureSelection featureSelection = (IFeatureSelection)featureLayer;
            var selectionSet = featureSelection.SelectionSet;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            string shapeField = featureClass.ShapeFieldName;

            ICursor cursor;
            selectionSet.Search(new QueryFilterClass(), false, out cursor);
            var featureCursor = cursor as IFeatureCursor;
            var features = new List<IFeature>();

            IFeature feature;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                features.Add((feature));
            }

            return features;
        }

        private void butRunSignature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.cbFocusLayer.Text == null || this.cbFocusLayer.Text == "")
                {
                    MessageBox.Show("You must select a layer");
                    return;
                }
                List<IFeatureLayer> layers = this.GetFeatureLayersFromToc(this.GetActiveViewFromArcMap(ArcMap.Application));
                IFeatureLayer layerWithSelection = this.getLayerByName(layers, this.cbFocusLayer.Text);
                if (layerWithSelection == null)
                {
                    MessageBox.Show("Layer does not exist");
                    return;
                }
                this.pbarChangeDet.Minimum = 0;
                this.pbarChangeDet.Maximum = layerWithSelection.FeatureClass.FeatureCount(null);
                this.pbarChangeDet.Value = 0;
                System.Windows.Forms.Application.DoEvents();

                List<IFeature> outPut = this.getSelectedFeatureFromLayerByName(layers, this.cbFocusLayer.Text);
                List<String> cols = new List<string>();
                Dictionary<string, string> outputCols = new Dictionary<string, string>();
                if (outPut.Count == 0)
                {
                    MessageBox.Show("No features in focus layer are selected. Make a selection");
                    return;
                }

                PivotTable signature = this.FeaturesToPivotTable(outPut, "GeoHash", null);
                List<String> ignoreCols = new List<String>() { "OBJECTID", "SHAPE", "SHAPE_Length", "SHAPE_Area" };

                PivotTableAnalyzer analyzer = new PivotTableAnalyzer(
                    new SendAnInt(this.UpdatePBar),
                    new UpdateAggWindowPbar(this.SetPBarProperties));
                this.UpdateStatusLabel("Preparing and caching AOI layer");
                System.Windows.Forms.Application.DoEvents();

                PivotTable aoiPivotTable = this.FeatureLayerToPivotTable(layerWithSelection, "GeoHash", null);

                this.UpdateStatusLabel("Processing Signature");

                System.Windows.Forms.Application.DoEvents();

                if (signature.Count > 1)
                {
                    DialogResult res =
                        MessageBox.Show(
                            "You have multiple cells selected. Would you like to use an average of the selected features as the signature? If NO is selected, a new layer will be generated for every selected cell. If YES is selected an average will be generated as the signature, one layer will be generated, and typically the resulting similarity distribution may be narrower. Also, diff columns are based on the averages.",
                            "Multiple Selected Cells",
                            MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        Dictionary<string, Dictionary<String, double>> Graph = analyzer.GetSimilarityGraph(signature);

                        foreach (String key in Graph.Keys)
                        {
                            String formattedGraph = "Graph for rowkey: " + key + "\n";
                            foreach (String innerKey in Graph[key].Keys)
                            {
                                double sim = Graph[key][innerKey];
                                formattedGraph += "\t" + innerKey + " :: " + sim + "\n";
                            }
                            var box = new ScrollableMessageBox();
                            box.Show(formattedGraph);
                        }
                        DialogResult resGraph = MessageBox.Show("Continue?", "Graph", MessageBoxButtons.YesNoCancel);
                        if (resGraph == DialogResult.No || resGraph == DialogResult.Cancel)
                        {
                            this.UpdatePBar(0);
                            this.UpdateStatusLabel("Status");
                            return;
                        }
                        signature = analyzer.GenerateAverageVector(signature);
                    }
                }

                foreach (PivotTableEntry entry in signature)
                {
                    PivotTable res = analyzer.GetSparseSimilarites(entry, aoiPivotTable, true, false);
                    foreach (String colName in res[0].Data.Keys)
                    {
                        if (!outputCols.ContainsKey(colName))
                        {
                            if (!ignoreCols.Contains(colName))
                            {
                                outputCols.Add(colName, colName);
                            }
                        }
                    }
                    IWorkspace ws = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);

                    String fcName = "mlt_" + entry.RowKey + "_" + DateTime.Now.Millisecond;
                    var featureClass = Jarvis.CreateStandaloneFeatureClass(ws, fcName, outputCols, false, 0);
                    IFeatureCursor insertCur = featureClass.Insert(true);
                    this.UpdateStatusLabel("Loading Feature Class");
                    System.Windows.Forms.Application.DoEvents();

                    this.InsertPivoTableRowsToFeatureClass(featureClass, res, outputCols);
                    this.AddLayerToArcMap(fcName);

                    this.lblPbarStatus.Content = "Done";
                    this.pbarChangeDet.Value = 0;
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unhandled exception occurred");
            }
        }

        private void buttAnalyzeDiff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String layerA = (String)this.cbAggLayerA.SelectedValue;
                String layerB = this.cbAggLayerB.Text;
                if (layerA == null || layerB == null)
                {
                    MessageBox.Show("no layers available");
                }
                List<IFeatureLayer> layers = this.GetFeatureLayersFromToc(this.GetActiveViewFromArcMap(ArcMap.Application));
                //  getSelectedFeatureFromLayerByName(layers, "somename");
                IFeatureLayer flayerA = this.getLayerByName(layers, layerA);
                IFeatureLayer flayerB = this.getLayerByName(layers, layerB);
                this.pbarChangeDet.Value = 0;

                this.UpdateStatusLabel("formatting and Caching Layer A");
                System.Windows.Forms.Application.DoEvents();
                List<String> ignoreCols = new List<String>() { "OBJECTID", "SHAPE", "SHAPE_Length", "SHAPE_Area" };
                PivotTable ptA = this.FeatureLayerToPivotTable(flayerA, "GeoHash", ignoreCols);
                this.UpdateStatusLabel("formatting and Caching Layer B");
                System.Windows.Forms.Application.DoEvents();
                PivotTable ptB = this.FeatureLayerToPivotTable(flayerB, "GeoHash", ignoreCols);
                this.UpdateStatusLabel("Generating Change Detection layer");
                System.Windows.Forms.Application.DoEvents();
                Dictionary<String, String> uniqueFieldNames = new Dictionary<String, String>();

                PivotTableAnalyzer analyzer = new PivotTableAnalyzer(
                    new SendAnInt(this.UpdatePBar),
                    new UpdateAggWindowPbar(this.SetPBarProperties));
                PivotTable res = analyzer.DetectChange(ptA, ptB);
                foreach (PivotTableEntry entry in res)
                {
                    foreach (String name in entry.Data.Keys)
                    {
                        if (!uniqueFieldNames.ContainsKey(name))
                        {
                            uniqueFieldNames.Add(name, name);
                        }
                    }
                    break;
                }
                IWorkspace ws = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                String fcName = "change_" + layerA + "_" + layerB + "_" + DateTime.Now.Millisecond;
                var featureClass = Jarvis.CreateStandaloneFeatureClass(ws, fcName, uniqueFieldNames, false, 0);
                IFeatureCursor insertCur = featureClass.Insert(true);
                this.UpdateStatusLabel("Loading Feature Class");
                System.Windows.Forms.Application.DoEvents();

                this.InsertPivoTableRowsToFeatureClass(featureClass, res, uniqueFieldNames);
                this.AddLayerToArcMap(fcName);
                this.pbarChangeDet.Value = 0;

                this.lblPbarStatus.Content = "Done";

                System.Windows.Forms.Application.DoEvents();
                // now I need to create a feature class and feature layer from this object
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured calculating change\n" + ex.Message.ToString());
            }
        }

        private void butChangeInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "This tool calculates the change between two aggregations over the same area that represent different time slices. Geohash size must be consistent in order to obtain valid results from this algorithm."
                + " The algorithm performs a sparse cosine similarity based on all field values for each row, and calculates the diff between all fields individually. The output feature class will have the union of fields from both input features. Any pivot table will work.",
                "info");
        }

        private void butPopFocLyrCb_Click(object sender, RoutedEventArgs e)
        {
            ESRI.ArcGIS.ArcMapUI.IContentsView cView = this.GetContentsViewFromArcMap(ArcMap.Application, 0);
            IActiveView aView = this.GetActiveViewFromArcMap(ArcMap.Application);
            List<IFeatureLayer> layers = this.GetFeatureLayersFromToc(aView);

            if (!this.cbFocusLayer.Items.IsEmpty)
            {
                this.cbFocusLayer.Items.Clear();
            }

            if (!this.cbFocusLayer.Items.IsEmpty)
            {
                for (int i = 0; i < this.cbAggLayerA.Items.Count; i++)
                {
                    this.cbFocusLayer.Items.RemoveAt(i);
                }
            }

            foreach (IFeatureLayer layer in layers)
            {
                // MessageBox.Show(layer.Name + " -- " + layer.DataSourceType);
                if (layer.Name.ToLower().Contains("aggregation"))
                {
                    this.cbFocusLayer.Items.Add(layer.Name);
                }
            }

            if (this.cbFocusLayer.Items.Count < 1)
            {
                MessageBox.Show("No aggregation layers available");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(GbdxResources.mltcAbout, "About");
        }

        private void selectionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectionTypeComboBox.SelectedIndex == 0)
            {
                this.selectAreaButton.IsEnabled = true;
            }
            else if (this.selectionTypeComboBox.SelectedIndex == 1)
            {
                this.selectAreaButton.IsEnabled = false;
            }
        }
    }
}
