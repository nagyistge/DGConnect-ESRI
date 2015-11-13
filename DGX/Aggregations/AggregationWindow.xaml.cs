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

namespace Dgx.Aggregations
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
    using DGXSettings;
    using DGXSettings.Properties;

    using DgxTools;

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

            this.Client = new RestClient(DgxHelper.GetEndpointBase(Settings.Default));
            string unencryptedPassword;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out unencryptedPassword);

            if (result)
            {
                this.Authenticate(Settings.Default.username, unencryptedPassword, this.Client, DgxHelper.GetAuthenticationEndpoint(Settings.Default), true);
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
            var request = new RestRequest(auth, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // Convert userpass to 64 base string.  i.e. username:password => 64 base string representation
            var passBytes = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", user, pass));
            var string64 = Convert.ToBase64String(passBytes);

            // Add the 64 base string representation to the header
            request.AddHeader("Authorization", string.Format("Basic {0}", string64));

            request.AddParameter("grant_type", "password");
            request.AddParameter("username", user);
            request.AddParameter("password", pass);

            if (runAsync)
            {
                client.ExecuteAsync<AccessToken>(
                    request,
                    resp =>
                        {
                            if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                            {
                                this.Authorization = resp.Data;
                            }
                            else
                            {
                                MessageBox.Show(DgxResources.InvalidUserPass);
                            }
                        });
            }
            else
            {
                var response = client.Execute<AccessToken>(request);
                if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                {
                    this.Authorization = response.Data;
                }
                else
                {
                    MessageBox.Show(DgxResources.InvalidUserPass);
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
                    return "avg:attributes.sentiment.positive.dbl";

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
            if (this.AoiPolygon == null)
            {
                MessageBox.Show(DgxResources.invalidBoundingBox);
                return;
            }

            if (this.QuerySelectionComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(DgxResources.noAggregationSelected);
                return;
            }

            if (this.detailGranularityComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(DgxResources.pleaseChooseDetailsGranularity);
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
                        DgxHelper.GetAuthenticationEndpoint(Settings.Default),
                        false);
                }
                else
                {
                    MessageBox.Show(DgxResources.problemDecryptingPassword);
                    return;
                }

                if (this.Authorization == null)
                {
                    MessageBox.Show(DgxResources.InvalidUserPass);
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

            this.Client.ExecuteAsync<MotherOfGodAggregations>(
                request,
                response =>
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
                                        AddLayerToArcMap(featureClassName);
                                        this.goButton.IsEnabled = true;
                                    });
                        }
                        else
                        {
                            var error = string.Format(
                                "\n{0}\n\n{1}",
                                response.ResponseUri.AbsoluteUri,
                                response.Content);
                            this.logWriter.Error(error);

                            this.Dispatcher.Invoke(
                                DispatcherPriority.Normal,
                                (MethodInvoker)delegate { this.goButton.IsEnabled = true; });

                            MessageBox.Show(DgxResources.Source_ErrorMessage);
                        }
                    });
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
            request.AddParameter("aggs", agg);

            // If the user setup a custom date range then use that otherwise assume no date range has been specified.
            if (this.specifyDateCheckbox.IsChecked != null && this.specifyDateCheckbox.IsChecked.Value)
            {
                // Make sure we don't do something illegal like trying to access a null value.
                if (this.startDatePicker.SelectedDate != null && this.endDatePicker.SelectedDate != null)
                {
                    request.AddParameter("startDate", this.startDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                    request.AddParameter("endDate", this.endDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                }
            }

            // Add the important AOI points.
            request.AddParameter("left", this.AoiPolygon.Envelope.UpperLeft.X);
            request.AddParameter("right", this.AoiPolygon.Envelope.UpperRight.X);
            request.AddParameter("upper", this.AoiPolygon.Envelope.UpperLeft.Y);
            request.AddParameter("lower", this.AoiPolygon.Envelope.LowerLeft.Y);
            request.AddParameter("count", 100000);
        }

        /// <summary>
        /// Adds a layer to arc map
        /// </summary>
        /// <param name="featureClassName">
        /// The feature class name.
        /// </param>
        private void AddLayerToArcMap(string featureClassName)
        {
            var featureWorkspace =
                                            (IFeatureWorkspace)Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
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
        private void InsertRow(IFeatureClass featureClass, string key, IFeatureCursor insertCur, Dictionary<string,Dictionary<string,double>>resultDictionary,Dictionary <string,string> uniqueFieldNames)
        {
            // get the polygon of the geohash
            var poly = this.GetGeoHashPoly(key);
            var buffer = featureClass.CreateFeatureBuffer();

            // Setup the features geometry.
            buffer.Shape = (IGeometry)poly;
            buffer.Value[featureClass.FindField("Name")] = key;
            foreach (var subKey in resultDictionary[key].Keys)
            {
                buffer.Value[featureClass.FindField(uniqueFieldNames[subKey])] =
                    resultDictionary[key][subKey];
            }

            // Feature has been created so add to the feature class.
            insertCur.InsertFeature(buffer);
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
        private void WriteToFeatureClass(IFeatureClass featureClass, Dictionary<string,Dictionary<string,double>> resultDictionary, Dictionary<string,string> uniqueFieldNames, IWorkspace workspace)
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
            // if there was already a listener established close it.
            AggregationRelay.Instance.AoiHasBeenDrawn -= this.InstanceOnAoiHasBeenDrawn;
            AggregationRelay.Instance.AoiHasBeenDrawn += this.InstanceOnAoiHasBeenDrawn;

            var commandBars = ArcMap.Application.Document.CommandBars;
            var commandId = new UIDClass() { Value = ThisAddIn.IDs.Dgx_Aggregations_AggregationSelector };
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
    }
}
