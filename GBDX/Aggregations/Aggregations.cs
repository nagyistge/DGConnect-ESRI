namespace Gbdx.Aggregations
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;
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
    using UserControl = System.Windows.Forms.UserControl;

    /// <summary>
    ///     Designer class of the dockable window add-in. It contains user interfaces that
    ///     make up the dockable window.
    /// </summary>
    public partial class Aggregations : UserControl
    {
        #region Fields & Properties

        const int MaxAttempts = 3;

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

            GetAuthenticationToken();

            this.startDatePicker.Value = DateTime.Now.AddMonths(-1);
            this.endDatePicker.Value = DateTime.Now;

            this.infoButton.Enabled = true;
            this.multiChangeDetectionInfoButton.Enabled = true;
            this.mltcInfoButton.Enabled = true;
            this.selectionTypeComboBox.SelectedIndex = 0;

            AggregationRelay.Instance.AoiHasBeenDrawn += this.EventHandlerInstanceAoiHasBeenDrawn;
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

            // 
            if (response.StatusCode == HttpStatusCode.GatewayTimeout && attempts <= MaxAttempts)
            {
                IRestClient restClient = new RestClient(Settings.Default.AuthBase);
                restClient.ExecuteAsync<MotherOfGodAggregations>(
                    response.Request,
                    resp => this.AggregationResponse(response, attempts + 1));
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
            var agg = string.Format(
                "geohash:{0};{1}",
                this.detailLevelComboBox.SelectedItem,
                argument);
            request.AddParameter("aggs", agg, ParameterType.QueryString);

            if (this.queryTextBox != null && this.queryTextBox.Text != null && this.queryTextBox.Text != "")
            {
                request.AddParameter(
                    "query",
                    this.queryTextBox.Text.Replace("\n", "").Replace("\r", ""),
                    ParameterType.QueryString);
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

            var request = new RestRequest("insight-vector/api/aggregation", Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.CreateAggregationArguments(ref request);

            var aoi = string.Empty;
            if (this.selectionTypeComboBox.SelectedIndex == 1)
            {
                aoi = Jarvis.ConvertPolygonsToGeoJson(Jarvis.GetPolygons(ArcMap.Document.FocusMap));
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

        private void EventHandlerInstanceAoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            poly.Project(Jarvis.ProjectedCoordinateSystem);
            this.ShapeAoi = poly;
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