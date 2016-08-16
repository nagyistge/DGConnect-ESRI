namespace Gbdx.Aggregations
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using global::Aggregations;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using GbdxTools;

    using NetworkConnections;

    using RestSharp;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;

    /// <summary>
    ///     Designer class of the dockable window add-in. It contains user interfaces that
    ///     make up the dockable window.
    /// </summary>
    public partial class Aggregations : UserControl
    {
        #region Fields & Properties

        private const string StatusLabelPrefix = "Status: ";

        private const int MaxAttempts = 5;

        /// <summary>
        ///     GBDX Authentication Token
        /// </summary>
        private string token;

        /// <summary>
        ///     Gets or sets the AOI polygon.
        /// </summary>
        private IPolygon ShapeAoi { get; set; }

        /// <summary>
        ///     Gets or sets the search item.
        /// </summary>
        private string SearchItem { get; set; }

        /// <summary>
        ///     Element of the existing bounding box
        /// </summary>
        private IElement BoundingBoxElmement { get; set; }

        /// <summary>
        ///     Host object of the dockable window
        /// </summary>
        private object Hook { get; set; }

        /// <summary>
        ///     Gets or sets the previously selected arcmap selected tool.
        /// </summary>
        private ICommandItem PreviouslySelectedItem { get; set; }

        #endregion

        public Aggregations(object hook)
        {
            this.InitializeComponent();
            this.Hook = hook;

            // Get GBDX authentication token
            this.GetAuthenticationToken();

            try
            {
                
                this.startDatePicker.MaxDate = this.endDatePicker.Value.Date;
                
                this.endDatePicker.MinDate = this.startDatePicker.Value.Date;
                this.endDatePicker.MaxDate = DateTime.Now.Date;
                this.startDatePicker.Value = DateTime.Now.AddMonths(-1);
                this.endDatePicker.Value = DateTime.Now.Date;
            }
            catch (Exception e)
            {
                Jarvis.Logger.Error(e);
            }

            // Event handlers for when the group boxes are checked
            this.changeDetectionGroupBox.CheckedChanged += this.EventHandlerCheckBoxGroupCheckChanged;
            this.mltcGroupBox.CheckedChanged += this.EventHandlerCheckBoxGroupCheckChanged;
            this.multiChangeDetectionGroupBox.CheckedChanged += this.EventHandlerCheckBoxGroupCheckChanged;

            // change the selection type to be Draw Rectangle by default.
            this.selectionTypeComboBox.SelectedIndex = 0;

            AggregationRelay.Instance.AoiHasBeenDrawn += this.EventHandlerInstanceAoiHasBeenDrawn;
        }

        /// <summary>
        ///     Adds a layer to arc map
        /// </summary>
        /// <param name="featureClassName">
        ///     The feature class name.
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

        private void AggregationResponse(IRestResponse<MotherOfGodAggregations> response, int attempts = 0)
        {
            Jarvis.Logger.Info(response.ResponseUri.ToString());

            // If we are getting timeouts retry up to MAX ATTEMPTS and log that a timeout occurred attempting the following URL
            if (response.StatusCode == HttpStatusCode.GatewayTimeout && attempts <= MaxAttempts)
            {
                IRestClient restClient = new RestClient(Settings.Default.AuthBase);
                restClient.ExecuteAsync<MotherOfGodAggregations>(
                    response.Request,
                    resp => this.AggregationResponse(response, attempts + 1));

                Jarvis.Logger.Warning(string.Format("{0} :: {1}", response.StatusCode, response.ResponseUri));
                return;
            }

            if (response.Data != null && response.StatusCode == HttpStatusCode.OK)
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

                if (resultDictionary.Count == 0 && uniqueFieldNames.Count == 0)
                {
                    MessageBox.Show(GbdxResources.NoDataFound);
                    this.Invoke((MethodInvoker)delegate { this.goButton.Enabled = true; });

                    return;
                }

                FieldChecker checker = new FieldCheckerClass();

                // Create a unique name for the feature class based on name.
                var featureClassName = "Agg_" + Guid.NewGuid();

                checker.ValidateTableName(featureClassName, out featureClassName);

                // Function being called really says it all but ... lets CREATE A FEATURE CLASS
                var featureClass = Jarvis.CreateStandaloneFeatureClass(
                    workspace,
                    featureClassName,
                    uniqueFieldNames,
                    false,
                    0);

                this.WriteToFeatureClass(featureClass, resultDictionary, uniqueFieldNames, workspace);

                // Use the dispatcher to make sure the following calls occur on the MAIN thread.
                this.Invoke(
                    (MethodInvoker)delegate
                        {
                            this.AddLayerToArcMap(featureClassName);
                            this.goButton.Enabled = true;
                        });
            }
            else
            {
                var error = string.Format(
                    "STATUS: {0}\n{1}\n\n{2}",
                    response.StatusCode,
                    response.ResponseUri.AbsoluteUri,
                    response.Content);
                Jarvis.Logger.Error(error);

                this.Invoke((MethodInvoker)delegate { this.goButton.Enabled = true; });

                MessageBox.Show(GbdxResources.Source_ErrorMessage);
            }
        }

        private void ChangeDetection()
        {
            try
            {
                var diffs = this.includeDiffCheckbox.Checked;
                var layerA = this.aggLayerAComboBox.Text;
                var layerB = this.aggLayerBComboBox.Text;
                if (layerA == null || layerB == null)
                {
                    MessageBox.Show("No layers available");
                    return;
                }
                var layers = GetFeatureLayersFromToc(ArcMap.Document.ActiveView);
                var flayerA = this.GetLayerByName(layers, layerA);
                var flayerB = this.GetLayerByName(layers, layerB);
                this.analysisProgressBar.Value = 0;

                this.statusLabel.Text = StatusLabelPrefix + "formatting and caching Layer A";

                Application.DoEvents();

                var ignoreCols = new List<string> { "OBJECTID", "SHAPE", "SHAPE_Length", "SHAPE_Area" };

                var ptA = this.FeatureLayerToPivotTable(flayerA, "GeoHash", ignoreCols);

                this.statusLabel.Text = StatusLabelPrefix + "Preparing " + layerA;

                Application.DoEvents();

                var ptB = this.FeatureLayerToPivotTable(flayerB, "GeoHash", ignoreCols);

                this.statusLabel.Text = StatusLabelPrefix + "Preparing " + layerB;

                Application.DoEvents();
                var uniqueFieldNames = new Dictionary<string, string>();

                var analyzer = new PivotTableAnalyzer(this.UpdatePBar, this.SetPBarProperties);
                var res = analyzer.DetectChange(ptA, ptB, layerA + "," + layerB, diffs);
                foreach (var entry in res)
                {
                    foreach (var name in entry.Data.Keys)
                    {
                        if (!uniqueFieldNames.ContainsKey(name))
                        {
                            uniqueFieldNames.Add(name, name);
                        }
                    }
                    break;
                }
                var ws = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                var fcName = "change_" + layerA + "_" + layerB + "_" + DateTime.Now.Millisecond;
                var featureClass = Jarvis.CreateStandaloneFeatureClass(ws, fcName, uniqueFieldNames, false, 0);
                this.statusLabel.Text = StatusLabelPrefix + "Loading output feature class";
                Application.DoEvents();

                this.InsertPivoTableRowsToFeatureClass(featureClass, res, uniqueFieldNames);
                this.AddLayerToArcMap(fcName);
                this.analysisProgressBar.Value = 0;

                this.statusLabel.Text = StatusLabelPrefix + "Done";
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured calculating change\n" + ex.Message);
            }
        }

        /// <summary>
        ///     Create the aggregation AGGS argument
        /// </summary>
        /// <param name="request">
        ///     The request.
        /// </param>
        private void CreateAggregationArguments(ref RestRequest request)
        {
            var argument = this.CreateAggsArgument();
            this.SearchItem = argument;

            // create the aggregation that is about to be processed.
            var agg = string.Format("geohash:{0};{1}", this.detailLevelComboBox.SelectedItem, argument);
            request.AddParameter("aggs", agg, ParameterType.QueryString);

            if (this.queryTextBox != null && this.queryTextBox.Text != null && this.queryTextBox.Text != "")
            {
                request.AddParameter("query", this.CreateQueryString(), ParameterType.QueryString);
            }

            // If the user setup a custom date range then use that otherwise assume no date range has been specified.
            if (this.dateRangeCheckGroupBox.Checked)
            {
                // Make sure we don't do something illegal like trying to access a null value.
                request.AddParameter(
                    "startDate",
                    this.startDatePicker.Value.ToString("yyyy-MM-dd"),
                    ParameterType.QueryString);
                request.AddParameter(
                    "endDate",
                    this.endDatePicker.Value.ToString("yyyy-MM-dd"),
                    ParameterType.QueryString);
            }
            request.AddParameter("count", 100000, ParameterType.QueryString);
        }

        /// <summary>
        ///     The create AGGS argument.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private string CreateAggsArgument()
        {
            var selectedItem = this.QuerySelectionComboBox.SelectedItem.ToString();

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

        private string CreateQueryString()
        {
            var builder = new StringBuilder();
            foreach (var line in this.queryTextBox.Lines)
            {
                builder.Append(line);
                builder.Append(" ");
            }
            var output = builder.ToString().Trim();
            return output;
        }

        /// <summary>
        ///     Eventhandler top handle when one of the checkbox group on the analysis tab is checked to ensure that the others are
        ///     unchecked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandlerCheckBoxGroupCheckChanged(object sender, EventArgs e)
        {
            try
            {
                var checkedBox = (CheckBox)sender;
                if (!checkedBox.Checked)
                {
                    return;
                }

                if (checkedBox.Text.Equals(this.changeDetectionGroupBox.CheckBoxText))
                {
                    this.multiChangeDetectionGroupBox.Checked = false;
                    this.mltcGroupBox.Checked = false;
                }
                else if (checkedBox.Text.Equals(this.multiChangeDetectionGroupBox.CheckBoxText))
                {
                    this.changeDetectionGroupBox.Checked = false;
                    this.mltcGroupBox.Checked = false;
                }
                else if (checkedBox.Text.Equals(this.mltcGroupBox.CheckBoxText))
                {
                    this.changeDetectionGroupBox.Checked = false;
                    this.multiChangeDetectionGroupBox.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Jarvis.Logger.Error(exception);
            }
        }

        private void EventHandlerGenerateButtonClick(object sender, EventArgs e)
        {
            if (this.changeDetectionGroupBox.Checked)
            {
                this.ChangeDetection();
            }
            else if (this.multiChangeDetectionGroupBox.Checked)
            {
                this.MultiChangeDetection();
            }
            else if (this.mltcGroupBox.Checked)
            {
                this.MoreLikeThisCell();
            }
        }

        private void EventHandlerGoButtonClick(object sender, EventArgs e)
        {
            if (this.ShapeAoi == null && this.selectionTypeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show(GbdxResources.invalidBoundingBox);
                return;
            }

            if (this.QuerySelectionComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(GbdxResources.noAggregationSelected);
                return;
            }

            if (this.detailLevelComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(GbdxResources.pleaseChooseDetailsGranularity);
                return;
            }
            this.goButton.Enabled = false;
            var request = new RestRequest("insight-vector/api/aggregation", Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.CreateAggregationArguments(ref request);

            var aoi = string.Empty;
            if (this.selectionTypeComboBox.SelectedIndex == 1)
            {
                try
                {
                    aoi = Jarvis.ConvertPolygonsToGeoJson(Jarvis.GetPolygons(ArcMap.Document.FocusMap));
                }
                catch (OutOfMemoryException exception)
                {
                    Jarvis.Logger.Error("To many features selected");
                    Jarvis.Logger.Error(exception);
                    MessageBox.Show(GbdxResources.ToManyFeaturesSelected);
                    this.goButton.Enabled = true;
                    return;
                }
            }
            else if (this.selectionTypeComboBox.SelectedIndex == 0)
            {
                aoi = Jarvis.ConvertPolygonsGeoJson(this.ShapeAoi);
            }

            if (string.IsNullOrEmpty(aoi))
            {
                MessageBox.Show("No AOI selected");
                return;
            }

            request.AddParameter("application/json", aoi, ParameterType.RequestBody);

            var client = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
            client.ExecuteAsync<MotherOfGodAggregations>(request, response => this.AggregationResponse(response));
        }

        private void EventHandlerInfoButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show(
                "This tool calculates the change between two aggregations over the same area that represent different time slices. Geohash size must be consistent in order to obtain valid results from this algorithm."
                + " The algorithm performs a sparse cosine similarity based on all field values for each row, and calculates the diff between all fields individually. The output feature class will have the union of fields from both input features. Any pivot table will work.",
                "About");
        }

        private void EventHandlerInstanceAoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            // Now that the event has been heard stop listening for future events.
            AggregationRelay.Instance.AoiHasBeenDrawn -= this.EventHandlerInstanceAoiHasBeenDrawn;

            // re-select the previously enabled tool
            ArcMap.Application.CurrentTool = this.PreviouslySelectedItem;
            poly.Project(Jarvis.ProjectedCoordinateSystem);
            this.ShapeAoi = poly;
        }

        private void EventHandlerMltcInfoButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show(GbdxResources.mltcAbout, "About");
        }

        private void EventHandlerMultiChangeDetectionInfoButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show(
                "This tool calculates the change between two or more aggregation layers over the same area that represent different time slices. Geohash size must be consistent in order to obtain valid results from this algorithm."
                + " The algorithm performs a sparse cosine similarity based on all field values for each row, and produces a pivot table of pairwise cell by cell comparisons. For percent diff on a field by field basis, use the Pairwise Change Detection tool.",
                "About");
        }

        private void EventHandlerQueryTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                ((TextBox)sender).AppendText("\r\n");
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Event Handler for the refresh button to execute the proper action depending on which group box is enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandlerRefreshButtonClick(object sender, EventArgs e)
        {
            if (this.changeDetectionGroupBox.Checked)
            {
                this.RefreshChangeDetectionComboBoxes();
            }
            else if (this.multiChangeDetectionGroupBox.Checked)
            {
                this.RefreshMultiChangeDetection();
            }
            else if (this.mltcGroupBox.Checked)
            {
                this.RefreshMltc();
            }
        }

        private void EventHandlerSelectAreaButtonClick(object sender, EventArgs e)
        {
            if (this.BoundingBoxElmement != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActivatedView, this.BoundingBoxElmement);
                this.BoundingBoxElmement = null;
            }

            // if there was already a listener established close it.
            AggregationRelay.Instance.AoiHasBeenDrawn -= this.EventHandlerInstanceAoiHasBeenDrawn;
            AggregationRelay.Instance.AoiHasBeenDrawn += this.EventHandlerInstanceAoiHasBeenDrawn;

            var commandBars = ArcMap.Application.Document.CommandBars;
            var commandId = new UIDClass { Value = ThisAddIn.IDs.Gbdx_Aggregations_AggregationSelector };
            var commandItem = commandBars.Find(commandId, false, false);

            // save currently selected tool to be re-selected after the AOI has been drawn.
            if (commandItem != null)
            {
                this.PreviouslySelectedItem = ArcMap.Application.CurrentTool;
                ArcMap.Application.CurrentTool = commandItem;
            }
        }

        public PivotTable FeatureLayerToPivotTable(
            IFeatureLayer layer,
            string rowKeyColName,
            List<string> columnsToIgnore)
        {
            SendAnInt sai = this.UpdatePBar;
            this.analysisProgressBar.Minimum = 0;
            this.analysisProgressBar.Maximum = layer.FeatureClass.FeatureCount(null);
            this.analysisProgressBar.Value = 0;

            if (columnsToIgnore == null)
            {
                columnsToIgnore = new List<string>();
            }
            if (!columnsToIgnore.Contains("OBJECTID"))
            {
                columnsToIgnore.Add("OBJECTID");
            }
            var pt = new PivotTable();
            if (PivotTableCache.Cache.ContainsKey(layer.Name))
            {
                pt = PivotTableCache.Cache[layer.Name];
                return pt;
            }

            var featureCursor = layer.FeatureClass.Search(null, false);
            var feature = featureCursor.NextFeature();
            // loop through the returned features and get the value for the field
            var x = 0;
            while (feature != null)
            {
                var entry = new PivotTableEntry();
                //do something with each feature(ie update geometry or attribute)
                this.analysisProgressBar.Value++;
                sai.Invoke(x);
                x++;
                for (var i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (this.analysisProgressBar.Value == this.analysisProgressBar.Maximum)
                    {
                        this.analysisProgressBar.Maximum = this.analysisProgressBar.Maximum + 10;
                    }

                    var f = feature.Fields.get_Field(i).Name;
                    var val = feature.get_Value(i).ToString();

                    if (columnsToIgnore.Contains(f))
                    {
                        continue;
                    }

                    if (f.Equals(rowKeyColName))
                    {
                        entry.RowKey = Convert.ToString(val);
                    }
                    else
                    {
                        try
                        {
                            entry.Data.Add(f, int.Parse(val));
                        }
                        catch
                        {
                        }
                    }
                }
                pt.Add(entry);
                feature = featureCursor.NextFeature();
            }

            sai.Invoke(Convert.ToInt32(this.analysisProgressBar.Maximum));
            //add to the cache
            if (!PivotTableCache.Cache.ContainsKey(layer.Name))
            {
                PivotTableCache.Cache.Add(layer.Name, pt);
            }
            return pt;
        }

        public PivotTable FeaturesToPivotTable(
            List<IFeature> layers,
            string rowKeyColName,
            List<string> columnsToIgnore)
        {
            SendAnInt sai = this.UpdatePBar;
            this.analysisProgressBar.Minimum = 0;
            this.analysisProgressBar.Maximum = layers.Count;
            this.analysisProgressBar.Value = 0;

            if (columnsToIgnore == null)
            {
                columnsToIgnore = new List<string>();
            }
            if (!columnsToIgnore.Contains("OBJECTID"))
            {
                columnsToIgnore.Add("OBJECTID");
            }
            var pt = new PivotTable();

            // IFeature feature = featureCursor.NextFeature();
            // loop through the returned features and get the value for the field
            var x = 0;
            foreach (var feature in layers)
            {
                var entry = new PivotTableEntry();
                //do something with each feature(ie update geometry or attribute)
                //  Console.WriteLine("The {0} field contains a value of {1}", nameOfField, feature.get_Value(fieldIndexValue));
                this.analysisProgressBar.Value++;
                sai.Invoke(x);
                x++;
                for (var i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (this.analysisProgressBar.Value == this.analysisProgressBar.Maximum)
                    {
                        this.analysisProgressBar.Maximum = this.analysisProgressBar.Maximum + 10;
                    }

                    var fname = feature.Fields.get_Field(i).Name;
                    var val = feature.get_Value(i).ToString();

                    if (columnsToIgnore.Contains(fname))
                    {
                        continue;
                    }

                    if (fname.Equals(rowKeyColName))
                    {
                        entry.RowKey = Convert.ToString(val);
                    }
                    else
                    {
                        try
                        {
                            entry.Data.Add(fname, int.Parse(val));
                        }
                        catch
                        {
                        }
                    }
                }
                pt.Add(entry);
            }
            sai.Invoke(Convert.ToInt32(this.analysisProgressBar.Maximum));
            return pt;
        }

        private void GetAuthenticationToken()
        {
            IRestClient restClient = new RestClient(Settings.Default.AuthBase);
            string password;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out password);

            if (!result)
            {
                Jarvis.Logger.Warning("PASSWORD FAILED DECRYPTION");
                MessageBox.Show("Error decrypting password");
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

        public static List<IFeatureLayer> GetFeatureLayersFromToc(IActiveView activeView)
        {
            var outlist = new List<IFeatureLayer>();

            if (activeView == null)
            {
                return null;
            }
            var map = activeView.FocusMap;

            for (var i = 0; i < map.LayerCount; i++)
            {
                var item = activeView.FocusMap.Layer[i] as IFeatureLayer;
                if (item != null)
                {
                    outlist.Add(item);
                }
            }

            return outlist;
        }

        /// <summary>
        ///     The get geo hash poly.
        /// </summary>
        /// <param name="geohash">
        ///     The geo hash.
        /// </param>
        /// <returns>
        ///     The <see cref="Polygon" />.
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

        public IFeatureLayer GetLayerByName(List<IFeatureLayer> layers, string name)
        {
            foreach (var layer in layers)
            {
                if (layer.Name.Equals(name))
                {
                    return layer;
                }
            }
            return null;
        }

        public List<IFeature> GetSelectedFeatureFromLayerByName(List<IFeatureLayer> layers, string nameOfLayer)
        {
            IFeatureLayer l = null;
            foreach (var layer in layers)
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
            var featureSelection = (IFeatureSelection)featureLayer;
            var selectionSet = featureSelection.SelectionSet;

            ICursor cursor;
            selectionSet.Search(new QueryFilterClass(), false, out cursor);
            var featureCursor = cursor as IFeatureCursor;
            var features = new List<IFeature>();

            IFeature feature;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                features.Add(feature);
            }

            return features;
        }

        private void InsertPivoTableRowsToFeatureClass(
            IFeatureClass featureClass,
            PivotTable ptable,
            Dictionary<string, string> uniqueFieldNames)
        {
            // get the polygon of the geohash
            var insertCur = featureClass.Insert(true);
            this.analysisProgressBar.Maximum = ptable.Count;
            this.analysisProgressBar.Minimum = 0;
            this.analysisProgressBar.Value = 0;
            var fieldsInFc = new List<string>();
            for (var y = 0; y < featureClass.Fields.FieldCount; y++)
            {
                fieldsInFc.Add(featureClass.Fields.Field[y].Name);
            }

            var i = 0;
            foreach (var entry in ptable)
            {
                i++;
                this.UpdatePBar(i);

                var poly = this.GetGeoHashPoly(entry.RowKey);
                var buffer = featureClass.CreateFeatureBuffer();

                // Setup the features geometry.
                buffer.Shape = (IGeometry)poly;

                buffer.Value[featureClass.FindField("Geohash")] = entry.RowKey;
                foreach (var val in entry.Data.Keys)
                {
                    if (uniqueFieldNames.ContainsKey(val))
                    {
                        try
                        {
                            if (val.EndsWith("_str"))
                            {
                                var fieldName = "DG_" + uniqueFieldNames[val];
                                var field = featureClass.FindField(fieldName);
                                var value = entry.Label;

                                buffer.Value[field] = value;
                            }
                            else
                            {
                                var fieldName = "DG_" + uniqueFieldNames[val];
                                var field = featureClass.FindField(fieldName);
                                var value = entry.Data[val];

                                buffer.Value[field] = value;
                            }
                        }
                        catch (Exception error)
                        {
                            Jarvis.Logger.Error(error);
                        }
                    }
                }
                // Feature has been created so add to the feature class.
                insertCur.InsertFeature(buffer);
            }
            insertCur.Flush();
            Marshal.ReleaseComObject(insertCur);
        }

        /// <summary>
        ///     Insert a row into the feature class.
        /// </summary>
        /// <param name="featureClass">
        ///     The feature class that will have rows inserted
        /// </param>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="insertCur">
        ///     The insert cur.
        /// </param>
        /// <param name="resultDictionary">
        ///     The result dictionary.
        /// </param>
        /// <param name="uniqueFieldNames">
        ///     The unique field names.
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
                    var index = featureClass.FindField("DG_" + field);

                    if (index != -1)
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

        private void MoreLikeThisCell()
        {
            try
            {
                if (this.focusLayerComboBox.Text == null || this.focusLayerComboBox.Text == "")
                {
                    MessageBox.Show("You must select a layer");
                    return;
                }
                var layers = GetFeatureLayersFromToc(ArcMap.Document.ActiveView);
                var layerWithSelection = this.GetLayerByName(layers, this.focusLayerComboBox.Text);
                if (layerWithSelection == null)
                {
                    MessageBox.Show("Layer does not exist");
                    return;
                }
                this.analysisProgressBar.Minimum = 0;
                this.analysisProgressBar.Maximum = layerWithSelection.FeatureClass.FeatureCount(null);
                this.analysisProgressBar.Value = 0;
                Application.DoEvents();

                var outPut = this.GetSelectedFeatureFromLayerByName(layers, this.focusLayerComboBox.Text);
                var cols = new List<string>();
                var outputCols = new Dictionary<string, string>();
                if (outPut.Count == 0)
                {
                    MessageBox.Show("No features in focus layer are selected. Make a selection");
                    return;
                }

                var signature = this.FeaturesToPivotTable(outPut, "GeoHash", null);
                var ignoreCols = new List<string> { "OBJECTID", "SHAPE", "SHAPE_Length", "SHAPE_Area" };

                var analyzer = new PivotTableAnalyzer(this.UpdatePBar, this.SetPBarProperties);

                this.statusLabel.Text = StatusLabelPrefix + "Preparing and caching AOI layer";
                Application.DoEvents();

                var aoiPivotTable = this.FeatureLayerToPivotTable(layerWithSelection, "GeoHash", null);

                this.statusLabel.Text = StatusLabelPrefix + "Processing signature";

                Application.DoEvents();

                if (signature.Count > 1)
                {
                    var res =
                        MessageBox.Show(
                            "You have multiple cells selected. Would you like to use an average of the selected features as the signature? If NO is selected, a new layer will be generated for every selected cell. If YES is selected an average will be generated as the signature, one layer will be generated, and typically the resulting similarity distribution may be narrower. Also, diff columns are based on the averages.",
                            "Multiple Selected Cells",
                            MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        var graph = analyzer.GetSimilarityGraph(signature);

                        foreach (var key in graph.Keys)
                        {
                            var formattedGraph = "Graph for rowkey: " + key + "\n";
                            foreach (var innerKey in graph[key].Keys)
                            {
                                var sim = graph[key][innerKey];
                                formattedGraph += "\t" + innerKey + " :: " + sim + "\n";
                            }
                            var box = new ScrollableMessageBox();
                            box.Show(formattedGraph);
                        }
                        var resGraph = MessageBox.Show("Continue?", "Graph", MessageBoxButtons.YesNoCancel);
                        if (resGraph == DialogResult.No || resGraph == DialogResult.Cancel)
                        {
                            this.UpdatePBar(0);
                            return;
                        }
                        signature = analyzer.GenerateAverageVector(signature);
                    }
                }

                foreach (var entry in signature)
                {
                    var res = analyzer.GetSparseSimilarites(entry, aoiPivotTable, true, false);
                    foreach (var colName in res[0].Data.Keys)
                    {
                        if (!outputCols.ContainsKey(colName))
                        {
                            if (!ignoreCols.Contains(colName))
                            {
                                outputCols.Add(colName, colName);
                            }
                        }
                    }
                    var ws = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);

                    var fcName = "mlt_" + entry.RowKey + "_" + DateTime.Now.Millisecond;
                    var featureClass = Jarvis.CreateStandaloneFeatureClass(ws, fcName, outputCols, false, 0);

                    this.statusLabel.Text = StatusLabelPrefix + "Loading feature class";
                    Application.DoEvents();

                    this.InsertPivoTableRowsToFeatureClass(featureClass, res, outputCols);
                    this.AddLayerToArcMap(fcName);

                    this.statusLabel.Text = StatusLabelPrefix + "Done";
                    this.analysisProgressBar.Value = 0;
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                Jarvis.Logger.Error(ex);
                MessageBox.Show("An unhandled exception occurred");
            }
        }

        private void MultiChangeDetection()
        {
            try
            {
                var layerNames = new List<string>();
                var allFieldNames = new List<string>();
                foreach (string s in this.changeLayersListBox.SelectedItems)
                {
                    if (!layerNames.Contains(s))
                    {
                        layerNames.Add(s);
                    }
                }
                if (this.changeLayersListBox.SelectedItems.Count < 2)
                {
                    MessageBox.Show("Please select at least two layers");
                    return;
                }
                var colsMappingForMsgBox = new Dictionary<string, string>();
                var uniqueFieldNames = new Dictionary<string, string>();
                var multiResult = new PivotTable();
                var analyzer = new PivotTableAnalyzer(this.UpdatePBar, this.SetPBarProperties);

                for (var i = 0; i < layerNames.Count; i++)
                {
                    for (var x = i + 1; x < layerNames.Count; x++)
                    {
                        var layerA = layerNames[i];
                        var layerB = layerNames[x];
                        colsMappingForMsgBox.Add("L" + i + "_" + x + "L", layerA + " --> " + layerB);

                        if (layerA == null || layerB == null)
                        {
                            MessageBox.Show("no layers available");
                        }

                        var layers = GetFeatureLayersFromToc(ArcMap.Document.ActiveView);

                        var flayerA = this.GetLayerByName(layers, layerA);
                        var flayerB = this.GetLayerByName(layers, layerB);
                        this.analysisProgressBar.Value = 0;

                        this.statusLabel.Text = StatusLabelPrefix + "Preparing " + layerA;
                        Application.DoEvents();
                        var ignoreCols = new List<string> { "OBJECTID", "SHAPE", "SHAPE_Length", "SHAPE_Area" };
                        var ptA = this.FeatureLayerToPivotTable(flayerA, "GeoHash", ignoreCols);

                        this.statusLabel.Text = StatusLabelPrefix + "Preparing " + layerB;

                        Application.DoEvents();
                        var ptB = this.FeatureLayerToPivotTable(flayerB, "GeoHash", ignoreCols);

                        this.statusLabel.Text = StatusLabelPrefix + "Calculating change";
                        Application.DoEvents();

                        var res = analyzer.DetectChange(ptA, ptB, "L" + i + "_" + x + "L", false);

                        foreach (var entry in res)
                        {
                            if (!entry.Data.ContainsKey("layerAIndex"))
                            {
                                entry.Data.Add("layerAIndex", i);
                            }
                            if (!entry.Data.ContainsKey("layerBIndex"))
                            {
                                entry.Data.Add("layerBIndex", i);
                            }

                            entry.Label = "L" + i + "_" + x + "L";

                            foreach (var name in entry.Data.Keys)
                            {
                                if (!allFieldNames.Contains(name))
                                {
                                    allFieldNames.Add(name);
                                }

                                if (!uniqueFieldNames.ContainsKey(name))
                                {
                                    uniqueFieldNames.Add(name, name);
                                }
                            }
                            break;
                        }
                        multiResult.AddRange(res);
                    }
                }
                var flat = analyzer.flattenAndSimplify(multiResult, "percent_change");
                uniqueFieldNames = new Dictionary<string, string>();
                foreach (var pte in flat)
                {
                    var vals = new List<double>();
                    foreach (var field in pte.Data.Keys)
                    {
                        vals.Add(pte.Data[field]);
                    }
                    pte.Data.Add("stdev_", analyzer.stdev(vals));
                    pte.Data.Add("avg_", analyzer.avg(vals));
                    pte.Data.Add("min_", analyzer.min(vals));
                    pte.Data.Add("max_", analyzer.max(vals));

                    foreach (var field in pte.Data.Keys)
                    {
                        if (!uniqueFieldNames.ContainsKey(field))
                        {
                            uniqueFieldNames.Add(field, field);
                        }
                    }
                }
                var ws = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                var fcName = "multi_change_" + DateTime.Now.Millisecond;
                //  uniqueFieldNames.Add("context_str", "context_str");
                var featureClass = Jarvis.CreateStandaloneFeatureClass(ws, fcName, uniqueFieldNames, false, 0);

                this.statusLabel.Text = StatusLabelPrefix + "Loading output feature class";
                Application.DoEvents();

                this.InsertPivoTableRowsToFeatureClass(featureClass, flat, uniqueFieldNames);
                this.AddLayerToArcMap(fcName);
                this.analysisProgressBar.Value = 0;

                this.statusLabel.Text = StatusLabelPrefix + "Done";

                var message = "FYI:\n";
                foreach (var colName in colsMappingForMsgBox.Keys)
                {
                    message += "Column " + colName + " represents change for " + colsMappingForMsgBox[colName] + "\n";
                }
                message += "\n*This is required because of field length limitations";
                MessageBox.Show(message);

                Application.DoEvents();
                // now I need to create a feature class and feature layer from this object
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured calculating change" + ex.Message);
            }
        }

        private void RefreshChangeDetectionComboBoxes()
        {
            var aView = ArcMap.Document.ActiveView;

            var layers = GetFeatureLayersFromToc(aView);

            if (this.aggLayerAComboBox.Items.Count != 0)
            {
                this.aggLayerAComboBox.Items.Clear();
            }
            if (this.aggLayerBComboBox.Items.Count != 0)
            {
                this.aggLayerBComboBox.Items.Clear();
            }

            foreach (var layer in layers)
            {
                if (layer.Name.ToLower().Contains("agg_"))
                {
                    this.aggLayerAComboBox.Items.Add(layer.Name);
                    this.aggLayerBComboBox.Items.Add(layer.Name);
                }
            }
            if (this.aggLayerAComboBox.Items.Count < 1)
            {
                MessageBox.Show("No aggregation layers available");
            }
        }

        private void RefreshMltc()
        {
            var aView = ArcMap.Document.ActiveView;
            var layers = GetFeatureLayersFromToc(aView);

            if (this.focusLayerComboBox.Items.Count != 0)
            {
                this.focusLayerComboBox.Items.Clear();
            }

            // not sure if this if statemenbt is ever needed ....
            if (this.focusLayerComboBox.Items.Count != 0)
            {
                for (var i = 0; i < this.aggLayerAComboBox.Items.Count; i++)
                {
                    this.focusLayerComboBox.Items.RemoveAt(i);
                }
            }

            foreach (var layer in layers)
            {
                if (layer.Name.ToLower().Contains("agg_"))
                {
                    this.focusLayerComboBox.Items.Add(layer.Name);
                }
            }

            if (this.focusLayerComboBox.Items.Count < 1)
            {
                MessageBox.Show("No aggregation layers available");
            }
        }

        private void RefreshMultiChangeDetection()
        {
            var aView = ArcMap.Document.ActiveView;
            var layers = GetFeatureLayersFromToc(aView);

            if (this.changeLayersListBox.Items.Count != 0)
            {
                this.changeLayersListBox.Items.Clear();
            }
            foreach (var layer in layers)
            {
                if (layer.Name.ToLower().Contains("agg_"))
                {
                    this.changeLayersListBox.Items.Add(layer.Name);
                }
            }
            if (this.changeLayersListBox.Items.Count < 1)
            {
                MessageBox.Show("No aggregation layers available");
            }
        }

        //do not call this via a delegate from another thread
        private void SetPBarProperties(int max, int min, int val)
        {
            this.analysisProgressBar.Minimum = min;
            this.analysisProgressBar.Maximum = max;
            this.analysisProgressBar.Value = val;
            Application.DoEvents();
        }

        //do not call this via a delegate from another thread
        private void UpdatePBar(int val)
        {
            this.analysisProgressBar.Value = val;
            Application.DoEvents();
        }

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
                Jarvis.Logger.Error(error);
            }
        }

        /// <summary>
        ///     Implementation class of the dockable window add-in. It is responsible for
        ///     creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            #region Fields & Properties

            private Aggregations m_windowUI;

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (this.m_windowUI != null) this.m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

            protected override IntPtr OnCreateChild()
            {
                this.m_windowUI = new Aggregations(this.Hook);
                return this.m_windowUI.Handle;
            }
        }
    }
}