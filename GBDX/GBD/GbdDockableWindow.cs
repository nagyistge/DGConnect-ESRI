// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdDockableWindow.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   //   //      Licensed under the Apache License, Version 2.0 (the "License");
//   //   //      you may not use this file except in compliance with the License.
//   //   //      You may obtain a copy of the License at
//   //   //          http://www.apache.org/licenses/LICENSE-2.0
//   //   //      Unless required by applicable law or agreed to in writing, software
//   //   //      distributed under the License is distributed on an "AS IS" BASIS,
//   //   //      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   //   //      See the License for the specific language governing permissions and
//   //   //      limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Forms.VisualStyles;
using GbdxTools;

using Logging;

using Microsoft.Windows.Controls;

using RestSharp.Deserializers;

namespace Gbdx.Gbd
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geometry;

    using GBD;

    using NetworkConnections;

    using Newtonsoft.Json;

    using RestSharp;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;

    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class GbdDockableWindow : UserControl
    {
        #region Fields

        /// <summary>
        /// Regular expression for getting the catalog id out of a url address.-
        /// </summary>
        private readonly Regex catalogIdRegEx = new Regex("catalogId=(?<catId>.*?)&");

        /// <summary>
        /// The file path.
        /// </summary>
        private readonly string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                           + Settings.Default.GbdOrders;


        /// <summary>
        /// GBD Comms that will talk with the GBD services.
        /// </summary>
        private readonly IGbdxComms comms;

        /// <summary>
        /// The polygon of the drawn AOI.
        /// </summary>
        private IPolygon localPolygon;

        /// <summary>
        /// The IElement of the drawn AOI.
        /// </summary>
        private IElement localElement;

        /// <summary>
        /// The data table holding all of the records
        /// </summary>
        private DataTable localDatatable;

        /// <summary>
        /// The work queue for the threads that are getting GBD results.
        /// </summary>
        private Queue workQueue;

        /// <summary>
        /// Worker thread being used to get the meta data from GBD
        /// </summary>
        private Thread worker1Thread;

        /// <summary>
        /// Worker thread being used to get the meta data from GBD
        /// </summary>
        private Thread worker2Thread;

        /// <summary>
        /// Worker thread being used to get the meta data from GBD
        /// </summary>
        private Thread worker3Thread;

        /// <summary>
        /// Worker thread being used to get the meta data from GBD
        /// </summary>
        private Thread worker4Thread;

        /// <summary>
        /// Thread that will do status update checks.
        /// </summary>
        private Thread statusUpdateThread;

        /// <summary>
        /// Thread control variable.  When set to false the threads will exit gracefully otherwise they will run until the work queue has been depleted.
        /// </summary>
        private volatile bool okToWork = true;

        /// <summary>
        /// the data view that the data grid view is using as a data source.
        /// </summary>
        private DataView dataView;

        /// <summary>
        /// Dictionary of all results.
        /// </summary>
        private Dictionary<string, Properties> allResults;

        /// <summary>
        /// Dictionary of cached images.
        /// </summary>
        private Dictionary<string, Image> cachedImages;


        /// <summary>
        /// Hashset of the polygons that the user has selected.
        /// </summary>
        private HashSet<string> userSelectedPolygons;

        /// <summary>
        /// The async handle.
        /// </summary>
        private RestRequestAsyncHandle asyncHandle;

        /// <summary>
        /// The RestSharp client used for network communications.
        /// </summary>
        private readonly RestClient client = new RestClient("https://browse.digitalglobe.com");

        /// <summary>
        /// Determines if all polygons should be displayed
        /// </summary>
        private bool displayAllPolgons;

        /// <summary>
        /// The cb header.
        /// </summary>
        private DataGridViewCheckBoxHeaderCell cbHeader;

        /// <summary>
        /// Previously selected item in arcmap.  Useful for making whatever tool was selected before clicking select all slected once I'm done doing stuff.
        /// </summary>
        private ICommandItem previouslySelectedItem;

        /// <summary>
        /// The total number of items in the data grid view.
        /// </summary>
        private int itemsTotal = 0;

        /// <summary>
        /// The start time.
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// The end time.
        /// </summary>
        private DateTime endTime;

        /// <summary>
        /// The date time changing.
        /// </summary>
        private bool dateTimeChanging = false;

        /// <summary>
        /// The log writer.
        /// </summary>
        private Logger logWriter;

        /// <summary>
        /// The order table.
        /// </summary>
        private DataTable orderTable;

        /// <summary>
        /// The gbd order list.
        /// </summary>
        private List<GbdOrder> gbdOrderList;

        private string token;

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="GbdDockableWindow"/> class.
        /// </summary>
        /// <param name="hook">
        /// The hook.
        /// </param>
        public GbdDockableWindow(object hook)
        {
            // Check to make sure there are credentials for GBD account.
            if (string.IsNullOrEmpty(Settings.Default.username) || string.IsNullOrEmpty(Settings.Default.password))
            {
                MessageBox.Show(GbdxResources.InvalidUserPass);
                return;
            }

            string pass;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out pass);
            if (result)
            {
                this.token = GetAccessToken(Settings.Default.apiKey, Settings.Default.username, pass);
            }
            else
            {
                MessageBox.Show("Problem decrypting password");
            }

            // Initialize GBD Communications and authorize with GBD authentication.
            this.comms = new GbdxComms(Jarvis.LogFile, false);

            this.InitializeComponent();
            this.VisibleChanged += this.GbdDockableWindowVisibleChanged;
            this.Hook = hook;
            GbdRelay.Instance.AoiHasBeenDrawn += this.InstanceAoiHasBeenDrawn;

            this.localDatatable = this.CreateDataTable();

            this.dataGridView1.RowsAdded += DataGridView1_RowsAdded;

            this.workQueue = Queue.Synchronized(new Queue());
            this.dataView = new DataView(this.localDatatable);

            this.dataGridView1.DataSource = this.dataView;

            var dataGridViewColumnHeaderStyle = new DataGridViewCellStyle();
            dataGridViewColumnHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewColumnHeaderStyle;

            var dataGridViewColumn = this.dataGridView1.Columns["Pan"];
            if (dataGridViewColumn != null)
            {
                dataGridViewColumn.CellTemplate = new DataGridViewDisableCheckBoxCell();
            }

            var dataGridViewColumnMs = this.dataGridView1.Columns["MS"];
            if (dataGridViewColumnMs != null)
            {
                dataGridViewColumnMs.CellTemplate = new DataGridViewDisableCheckBoxCell();
            }
            // Set the current DateTime to last year
            this.fromDateTimePicker.Value = DateTime.Now.AddMonths(-1);
            this.startTime = DateTime.Now.AddMonths(-1);
            this.toDateTimePicker.Value = DateTime.Now;
            this.endTime = DateTime.Now;

            this.dataGridView1.CellClick += this.DataGridView1SelectionChanged;
            this.allResults = new Dictionary<string, Properties>();

            this.thumbnailPictureBox.LoadCompleted += this.ThumbnailPictureBoxLoadCompleted;
            this.cachedImages = new Dictionary<string, Image>();
            this.thumbnailPictureBox.InitialImage = new Bitmap(GbdxResources.PleaseStandBy, new Size(309, 376));

            this.displayAllPolgons = false;

            // Initialize the hashset and dictionary required for displaying mass polygons.
            this.userSelectedPolygons = new HashSet<string>();

            this.dataGridView1.CellContentClick += this.DataGridView1CellContentClick;

            try
            {
                this.cbHeader = new DataGridViewCheckBoxHeaderCell();

                if (this.dataGridView1.Columns["Selected"] != null)
                {
                    this.dataGridView1.Columns["Selected"].HeaderCell = this.cbHeader;

                    // Change the column width to something more reasonable. 
                    this.dataGridView1.Columns["Selected"].Width = 29;
                }
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }

            if (Settings.Default.baseUrl.Equals(Settings.Default.DefaultBaseUrl))
            {
                this.exportButton.Text = "Export";
            }
            else
            {
                this.exportButton.Text = "Order";
            }

            this.dataView.RowFilter = this.FilterSetup();
            this.SetupOrderStatusTable();

            // Allocate the memory for the object even if we don't have orders yet
            this.gbdOrderList = new List<GbdOrder>();

            // If we have orders saved then lets load them up
            if (File.Exists(this.filePath))
            {
                this.gbdOrderList = this.LoadGbdOrdersFromFile(this.filePath);
                UpdateOrderTable(this.gbdOrderList, ref this.orderTable);
            }
        }

        private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            var row = this.dataGridView1.Rows[e.RowIndex];

            SetImageryCheckbox("PAN ID", "PAN", row);
            SetImageryCheckbox("MS ID", "MS", row);
        }


        private void SetImageryCheckbox(string idName, string checkboxName, DataGridViewRow row)
        {
            try
            {
                var id = row.Cells[idName].Value.ToString();
                var cell = (DataGridViewDisableCheckBoxCell)row.Cells[checkboxName];

                if (!string.IsNullOrEmpty(id))
                {
                    cell.Enabled = true;
                }
                else
                {
                    cell.Enabled = false;
                }

            }
            catch (Exception e)
            {
                Jarvis.Logger.Error(e);
            }
        }

        /// <summary>
        /// Setup up the order status data table.
        /// </summary>
        private void SetupOrderStatusTable()
        {
            this.orderTable = this.CreateOrderTable();
            this.orderDataGridView.DataSource = this.orderTable;

        }

        /// <summary>
        /// The GBD dockable window visible changed event handler
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void GbdDockableWindowVisibleChanged(object sender, EventArgs e)
        {
            this.ResetGbd();
        }

        /// <summary>
        /// Delegate for when a data table has been built by one of the worker threads and add it to the main table and update the user view.
        /// </summary>
        /// <param name="dt">data table that was constructed</param>
        /// <param name="responses">dictionary of the newly complete responses.</param>
        private delegate void DataTableDone(DataTable dt, Dictionary<string, Properties> responses);

        private delegate void ExecuteAfterResponse(List<GbdOrder> orders);

        private delegate void UpdateImageryCheckbox(string id, string name, DataGridViewRow row);

        /// <summary>
        /// Callback to update the order status.  Will be fired from a background thread
        /// </summary>
        /// <param name="updateStatus">
        /// The update status.
        /// </param>
        private delegate void UpdateStatusCallback(GbdOrder updateStatus);

        /// <summary>
        /// Gets or sets the hook object of the dockable window
        /// </summary>
        private object Hook { get; set; }

        /// <summary>
        /// The create data table.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        private DataTable CreateDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("Selected", typeof(bool)) { ReadOnly = false, DefaultValue = false });
            dt.Columns.Add(new DataColumn("PAN", typeof(bool)) { ReadOnly = false, DefaultValue = false, });
            dt.Columns.Add(new DataColumn("MS", typeof(bool)) { ReadOnly = false, DefaultValue = false });
            dt.Columns.Add(new DataColumn("Catalog ID", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Sensor", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Acquired", typeof(DateTime)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Cloud Cover", typeof(double)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Off Nadir Angle", typeof(double)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Sun Elevation", typeof(double)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Pan Resolution", typeof(double)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("PAN ID", typeof(string)) { ReadOnly = true, DefaultValue = string.Empty });
            dt.Columns.Add(new DataColumn("MS ID", typeof(string)) { ReadOnly = true, DefaultValue = string.Empty });

            var primary = new DataColumn[1];
            primary[0] = dt.Columns["Catalog ID"];
            dt.PrimaryKey = primary;
            return dt;
        }

        /// <summary>
        /// Create Order Data Table
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        private DataTable CreateOrderTable()
        {
            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("Order ID", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Order Date", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Service Provider", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Order Status", typeof(string)) { ReadOnly = false });
            var primary = new DataColumn[1];
            primary[0] = dt.Columns["Order ID"];
            dt.PrimaryKey = primary;
            return dt;
        }

        /// <summary>
        /// The update data table.
        /// </summary>
        /// <param name="dataTobeAdded">
        /// The data to be added.
        /// </param>
        /// <param name="responses">
        /// The responses.
        /// </param>
        private void UpdateDataTable(DataTable dataTobeAdded, Dictionary<string, Properties> responses)
        {
            try
            {
                // Only update the table if we are still allowed to do work.
                if (!this.okToWork)
                {
                    return;
                }
                // need to check the newly added data for duplicates and remove them
                var index = this.dataGridView1.FirstDisplayedScrollingRowIndex;
                var horizIndex = this.dataGridView1.FirstDisplayedScrollingColumnIndex;

                this.localDatatable.Merge(dataTobeAdded, true);

                // Add the new responses to the others.
                this.allResults = this.allResults.Concat(responses)
                    .GroupBy(d => d.Key)
                    .ToDictionary(d => d.Key, d => d.First().Value);

                if (this.displayAllPolgons)
                {
                    this.SetAllCheckBoxes(this.displayAllPolgons, this.dataGridView1);
                }

                this.DrawViewablePolygons();

                this.dataGridView1.FirstDisplayedScrollingRowIndex = index;
                this.dataGridView1.FirstDisplayedScrollingColumnIndex = horizIndex;

                var dataGridViewColumn = this.dataGridView1.Columns["Selected"];
                if (dataGridViewColumn != null)
                {
                    var chkBox = (DataGridViewCheckBoxHeaderCell)dataGridViewColumn.HeaderCell;

                    if (chkBox.isChecked)
                    {
                        this.SetAllCheckBoxes(chkBox.isChecked, this.dataGridView1);
                        this.DrawCheckedPolygons();
                    }
                }

                this.UpdateSelectedAndTotalLabels();
            }
            catch (Exception e)
            {
                Jarvis.Logger.Error(e);
            }
        }

        /// <summary>
        /// Reset GBD window
        /// </summary>
        private void ResetGbd()
        {
            this.okToWork = false;
            this.allResults = new Dictionary<string, Properties>();
            this.workQueue.Clear();

            //// Check the threads.  if they are running lets shut them down.
            //this.ThreadLifeCheck(this.worker1Thread);
            //this.ThreadLifeCheck(this.worker2Thread);
            //this.ThreadLifeCheck(this.worker3Thread);
            //this.ThreadLifeCheck(this.worker4Thread);
            //this.ThreadLifeCheck(this.statusUpdateThread);

            this.localDatatable.Clear();
            this.allResults.Clear();
            this.ClearPolygons();
            this.catalogIdSearchTextBox.Clear();

            // Check the domain to see which label should be on the export button
            if (Settings.Default.baseUrl.Equals(Settings.Default.DefaultBaseUrl))
            {
                this.exportButton.Text = "Export";
            }
            else
            {
                this.exportButton.Text = "Order";
            }
        }

        #region Filtering

        /// <summary>
        /// The filter setup.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string FilterSetup()
        {
            var newFilter = string.Empty;
            newFilter += this.SensorFilterSetup(newFilter);
            newFilter += this.CloudCoverFilterSetup(newFilter);
            newFilter += this.NadirAngleFilterSetup(newFilter);
            newFilter += this.SunElevationFilterSetup(newFilter);
            newFilter += this.PanResolutionFilterSetup(newFilter);
            newFilter += this.AcquiredDateFilterSetup(newFilter, this.fromDateTimePicker, this.toDateTimePicker);
            newFilter = CatalogIdFilter(newFilter, this.catalogIdSearchTextBox.Text);
            return newFilter;
        }

        /// <summary>
        /// The cloud cover filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string CloudCoverFilterSetup(string filter)
        {
            string output;
            var selectedIndex = this.cloudCoverageComboBox.SelectedIndex;
            if (selectedIndex <= 0)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(filter))
            {
                output = "[Cloud Cover]<='" + this.cloudCoverageComboBox.Items[selectedIndex] + "'";
            }
            else
            {
                output = " AND [Cloud Cover]<='" + this.cloudCoverageComboBox.Items[selectedIndex] + "'";
            }

            return output;
        }

        /// <summary>
        /// The nadir angle filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string NadirAngleFilterSetup(string filter)
        {
            string output;
            var selectedIndex = this.nadirAngleComboBox.SelectedIndex;
            if (selectedIndex <= 0)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(filter))
            {
                output = "[Off Nadir Angle]<='" + this.nadirAngleComboBox.Items[selectedIndex] + "'";
            }
            else
            {
                output = "AND [Off Nadir Angle]<='" + this.nadirAngleComboBox.Items[selectedIndex] + "'";
            }

            return output;
        }

        /// <summary>
        /// The sun elevation filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string SunElevationFilterSetup(string filter)
        {
            string output;
            var selectedIndex = this.sunElevationComboBox.SelectedIndex;
            if (selectedIndex <= 0)
            {
                return string.Empty;
            }

            output = "[Sun Elevation]<='" + this.sunElevationComboBox.Items[selectedIndex] + "'";
            if (!string.IsNullOrEmpty(filter))
            {
                output = "AND " + output;
            }

            return output;
        }

        /// <summary>
        /// The pan resolution filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string PanResolutionFilterSetup(string filter)
        {
            string output;
            var selectedIndex = this.panResolutionComboBox.SelectedIndex;
            if (selectedIndex <= 0)
            {
                return string.Empty;
            }

            const string Highest = " (Highest Resolution)";
            const string Lowest = " (Lowest Resolution)";
            var selectedText = this.panResolutionComboBox.Items[selectedIndex].ToString();
            selectedText = selectedText.Replace(Highest, string.Empty);
            selectedText = selectedText.Replace(Lowest, string.Empty);

            if (selectedIndex == 1)
            {
                output = " [Pan Resolution] >='" + selectedText + "' AND [Pan Resolution] <= '"
                         + this.panResolutionComboBox.Items[selectedIndex + 1] + "'";
            }
            else
            {
                output = "[Pan Resolution]<='" + selectedText + "'";
            }

            if (!string.IsNullOrEmpty(filter))
            {
                output = "AND " + output;
            }

            return output;
        }

        /// <summary>
        /// The sensor filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string SensorFilterSetup(string filter)
        {
            string output;
            var selectedIndex = this.sensorComboBox.SelectedIndex;
            if (selectedIndex <= 0)
            {
                return string.Empty;
            }

            // Use the correct operator depending on the current status of the filter string.
            if (string.IsNullOrEmpty(filter))
            {
                output = "[Sensor]=";
            }
            else
            {
                output = " AND [Sensor]=";
            }

            // Take the pretty wording and exhcange it with the actual wording
            switch (this.sensorComboBox.Items[selectedIndex].ToString())
            {
                case "WorldView-1":
                    output += "'WORLDVIEW01'";
                    break;

                case "WorldView-2":
                    output += "'WORLDVIEW02'";
                    break;

                case "WorldView-3":
                    output += "'WORLDVIEW03'";
                    break;

                case "GeoEye-1":
                    output += "'GEOEYE01'";
                    break;
                case "QuickBird":
                    output += "'QUICKBIRD02'";
                    break;
            }

            return output;
        }

        /// <summary>
        /// The acquired date filter setup.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string AcquiredDateFilterSetup(string filter, DateTimePicker from, DateTimePicker to)
        {
            var output = string.Empty;

            var startTimeSpan = new TimeSpan(0, 0, 0);
            var endTimeSpan = new TimeSpan(23, 59, 59);

            var fromValue = from.Value.Date + startTimeSpan;
            var toValue = to.Value.Date + endTimeSpan;

            output = "[Acquired] >= #" + fromValue.ToString("s") + "# AND [Acquired] <= #" + toValue.ToString("s") + "#";

            if (!string.IsNullOrEmpty(filter))
            {
                output = "AND " + output;
            }

            return output;
        }

        private static string CatalogIdFilter(string filter, string catId)
        {
            // if there is no update to the Catalog ID filter just return the filter string.
            if (string.IsNullOrEmpty(catId))
            {
                return filter;
            }

            var stringBuilder = new StringBuilder(filter);

            if (!string.IsNullOrEmpty(filter))
            {
                stringBuilder.Append(" AND ");
            }

            stringBuilder.Append(string.Format("[Catalog ID] LIKE '{0}*'", catId));

            return stringBuilder.ToString();
        }

        #endregion

        #region Thread Methods

        /// <summary>
        /// Go out and get the GBD data.
        /// </summary>
        /// <param name="polygons">
        /// The AOI split into 1x1 degree AOI's
        /// </param>
        private void GetGbdData(List<GbdPolygon> polygons)
        {
            // Set control variable to true prior to kicking off work.
            this.okToWork = true;

            var restClient = new RestClient("https://geobigdata.io");

            foreach (var polygon in polygons)
            {
                var request = new RestRequest(Settings.Default.GbdSearchPath, Method.POST);
                request.AddHeader("Authorization", "Bearer " + this.token);
                request.AddHeader("Content-Type", "application/json");

                var searchObject = new GbdSearchObject { searchAreaWkt = polygon.ToString() };
                searchObject.types.Add("Acquisition");

                var serializedString = JsonConvert.SerializeObject(searchObject);

                request.AddParameter("application/json", serializedString, ParameterType.RequestBody);

                restClient.ExecuteAsync<List<GbdResponse>>(
                    request, resp => this.ProcessGbdSearchResult(resp, polygon.ToString(), this.token));
            }
        }

        private void GetIdahoIds(string polygon, string authToken)
        {
            var restClient = new RestClient("https://geobigdata.io");

            var request = new RestRequest(Settings.Default.GbdSearchPath, Method.POST);
            request.AddHeader("Authorization", "Bearer " + authToken);
            request.AddHeader("Content-Type", "application/json");

            var searchObject = new GbdSearchObject { searchAreaWkt = polygon };
            searchObject.types.Add("IDAHOImage");

            var payload = JsonConvert.SerializeObject(searchObject);

            request.AddParameter("application/json", payload, ParameterType.RequestBody);

            restClient.ExecuteAsync<List<GbdResponse>>(request, this.IdahoImageTableUpdate);
        }

        private void IdahoImageTableUpdate(IRestResponse<List<GbdResponse>> response)
        {
            if (response.ResponseUri == null)
            {
                Jarvis.Logger.Info("Response was null so nothing was logged ");
                return;
            }
            Jarvis.Logger.Info(response.ResponseUri.ToString());
            foreach (var resp in response.Data)
            {
                var results = resp.results;

                if (results == null)
                {
                    continue;
                }

                foreach (var result in results)
                {
                    var gbdId = result.properties.vendorDatasetIdentifier3;
                    var idahoId = result.properties.imageId;
                    // if there isn't an id don't continue
                    if (string.IsNullOrEmpty(gbdId) || string.IsNullOrEmpty(idahoId))
                    {
                        continue;
                    }
                    DataRow row = this.localDatatable.Rows.Find(gbdId);

                    // if the row is null... don't continue
                    if (row == null)
                    {
                        continue;
                    }

                    var dataGridView = this.dataGridView1;
                    if (dataGridView != null)
                    {
                        try
                        {
                            var dataGridRow = dataGridView.Rows
                                .Cast<DataGridViewRow>()
                                .First(r => r.Cells["Catalog ID"].Value.ToString().Equals(gbdId));

                            if (dataGridRow != null)
                            {
                                // Now all the work has been completed so lets do a callback to the main thread to merge it with the existing results.
                                this.Invoke(new UpdateImageryCheckbox(this.SetImageryCheckbox), "PAN ID", "PAN", dataGridRow);
                                this.Invoke(new UpdateImageryCheckbox(this.SetImageryCheckbox), "MS ID", "MS", dataGridRow);
                            }
                        }
                        catch (Exception e)
                        {
                            Jarvis.Logger.Error(e);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// The setup threads.
        /// </summary>
        private void SetupThreads()
        {
            // Check the threads.  if they are running lets shut them down.
            this.ThreadLifeCheck(this.worker1Thread);
            this.ThreadLifeCheck(this.worker2Thread);
            this.ThreadLifeCheck(this.worker3Thread);
            this.ThreadLifeCheck(this.worker4Thread);

            // Prepare the threads to run the next jobs
            this.worker1Thread = new Thread(this.ThreadWork) { IsBackground = true };
            this.worker2Thread = new Thread(this.ThreadWork) { IsBackground = true };
            this.worker3Thread = new Thread(this.ThreadWork) { IsBackground = true };
            this.worker4Thread = new Thread(this.ThreadWork) { IsBackground = true };

            this.allResults = new Dictionary<string, Properties>();

            this.localDatatable.Clear();
            this.allResults.Clear();
        }

        /// <summary>
        /// Checks to see if the targeted thread is running.  If it is then tells the thread its time to quit.
        /// </summary>
        /// <param name="targetThread">
        /// The target thread.
        /// </param>
        private void ThreadLifeCheck(Thread targetThread)
        {
            if (targetThread != null && targetThread.IsAlive)
            {
                // turn thread run variable to false to end all threads
                this.okToWork = false;

                targetThread.Interrupt();

                // Wait 2 seconds for the thread to kill itself.
                if (!targetThread.Join(2000))
                {
                    targetThread.Abort();
                }
            }
        }

        private void ProcessGbdSearchResult(IRestResponse<List<GbdResponse>> resp, string polygon, string authToken)
        {

            try
            {
                Jarvis.Logger.Info(resp.ResponseUri.ToString());
            }
            catch (Exception e)
            {
                Jarvis.Logger.Error(e);
            }

            if (resp.Data != null)
            {
                // Create data table in the thread to be mereged later
                var dt = this.CreateDataTable();
                var responses = new Dictionary<string, Properties>();

                // go through all the results and add valid rows to the temporary table.
                foreach (var gbdResponse in resp.Data)
                {
                    if (gbdResponse.results == null)
                    {
                        continue;
                    }

                    foreach (var item in gbdResponse.results)
                    {
                        try
                        {
                            // Currently if we don't have a catalog ID for the item let's omit it from the results.
                            if (string.IsNullOrEmpty(item.properties.catalogID))
                            {
                                continue;
                            }

                            item.properties.Points = GbdJarvis.GetPointsFromWkt(item.properties.footprintWkt);

                            // If the dictionary doesn't have the response lets add it.
                            if (!responses.ContainsKey(item.properties.catalogID))
                            {
                                responses.Add(item.properties.catalogID, item.properties);
                            }

                            // setup new row for datatable complete with VALUES!
                            var row = dt.NewRow();
                            row["Catalog ID"] = item.properties.catalogID;
                            row["Sensor"] = item.properties.sensorPlatformName;
                            row["Acquired"] = Convert.ToDateTime(item.properties.timestamp);
                            row["Cloud Cover"] = Convert.ToDouble(item.properties.cloudCover);
                            row["Off Nadir Angle"] = Convert.ToDouble(item.properties.offNadirAngle);
                            row["Sun Elevation"] = Convert.ToDouble(item.properties.sunElevation);
                            row["Pan Resolution"] = Convert.ToDouble(item.properties.panResolution);

                            dt.Rows.Add(row);
                        }
                        catch (Exception error)
                        {
                            Jarvis.Logger.Error(error);
                        }
                    }
                }

                // Now all the work has been completed so lets do a callback to the main thread to merge it with the existing results.
                this.Invoke(new DataTableDone(this.UpdateDataTable), dt, responses);

                this.GetIdahoIds(polygon, authToken);
            }
        }

        /// <summary>
        /// The thread work.
        /// </summary>
        private void ThreadWork()
        {
            while (this.okToWork && Queue.Synchronized(this.workQueue).Count > 0)
            {
                try
                {
                    // while ok to work variable is true and we have work to do RUN!

                    // perform a thread safe call to dequeue a worker from the queue.
                    var polygon = (GbdPolygon)Queue.Synchronized(this.workQueue).Dequeue();

                    // Dictionary to hold the Properties
                    var responses = new Dictionary<string, Properties>();

                    // Fill out the search object
                    var searchObject = new GbdSearchObject { searchAreaWkt = polygon.ToString() };
                    searchObject.types.Add("Acquisition");

                    // Convert object to json serialized string.
                    var serializedstring = JsonConvert.SerializeObject(searchObject);

                    string decryptedPassword;
                    var success = Aes.Instance.Decrypt128(Settings.Default.password, out decryptedPassword);
                    if (!success)
                    {
                        return;
                    }

                    // Creating network object.
                    var netObj = new NetObject
                                     {
                                         AddressUrl = Settings.Default.GbdSearchPath,
                                         BaseUrl = Settings.Default.DefaultAuthBase,
                                         AuthEndpoint = Settings.Default.authenticationServer,
                                         User = Settings.Default.username,
                                         Password = decryptedPassword,
                                         ApiKey = Settings.Default.apiKey,
                                         AuthUrl = Settings.Default.DefaultAuthBase,
                                     };

                    var result = this.comms.Post<GbdResponse>(netObj, serializedstring);

                    // Create data table in the thread to be mereged later
                    var dt = this.CreateDataTable();

                    // Results null check
                    if (result.results == null)
                    {
                        continue;
                    }

                    // go through all the results and add valid rows to the temporary table.
                    foreach (var item in result.results)
                    {
                        try
                        {
                            // Currently if we don't have a catalog ID for the item let's omit it from the results.
                            if (string.IsNullOrEmpty(item.properties.catalogID))
                            {
                                continue;
                            }

                            item.properties.Points = GbdJarvis.GetPointsFromWkt(item.properties.footprintWkt);

                            // If the dictionary doesn't have the response lets add it.
                            if (!responses.ContainsKey(item.properties.catalogID))
                            {
                                responses.Add(item.properties.catalogID, item.properties);
                            }

                            // setup new row for datatable complete with VALUES!
                            var row = dt.NewRow();
                            row["Catalog ID"] = item.properties.catalogID;
                            row["Sensor"] = item.properties.sensorPlatformName;
                            row["Acquired"] = Convert.ToDateTime(item.properties.timestamp);
                            row["Cloud Cover"] = Convert.ToDouble(item.properties.cloudCover);
                            row["Off Nadir Angle"] = Convert.ToDouble(item.properties.offNadirAngle);
                            row["Sun Elevation"] = Convert.ToDouble(item.properties.sunElevation);
                            row["Pan Resolution"] = Convert.ToDouble(item.properties.panResolution);
                            dt.Rows.Add(row);
                        }
                        catch (Exception error)
                        {
                            this.logWriter.Error(error);
                        }
                    }

                    // Only update the table if we are still allowed to do work.
                    if (this.okToWork)
                    {
                        // Now all the work has been completed so lets do a callback to the main thread to merge it with the existing results.
                        this.Invoke(new DataTableDone(this.UpdateDataTable), dt, responses);
                    }
                }
                catch (Exception error)
                {
                    this.logWriter.Error(error);
                }
            }
        }

        #endregion

        #region Windows Form Event Handlers & Related Methods

        /// <summary>
        /// The select button click event handler
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SelectButtonClick(object sender, EventArgs e)
        {
            this.ResetGbd();

            if (ArcMap.Application.CurrentTool.Name != ThisAddIn.IDs.Gbdx_Gbd_Selector)
            {
                // if there was already a listener established close it.
                GbdRelay.Instance.AoiHasBeenDrawn -= this.InstanceAoiHasBeenDrawn;
                GbdRelay.Instance.AoiHasBeenDrawn += this.InstanceAoiHasBeenDrawn;

                var commandBars = ArcMap.Application.Document.CommandBars;
                var commandId = new UIDClass() { Value = ThisAddIn.IDs.Gbdx_Gbd_Selector };
                var commandItem = commandBars.Find(commandId, false, false);

                if (commandItem != null)
                {
                    this.previouslySelectedItem = ArcMap.Application.CurrentTool;
                    ArcMap.Application.CurrentTool = commandItem;
                }
            }
        }

        /// <summary>
        /// Event handler for when the export button is clicked.
        /// </summary>
        /// <param name="sender">
        /// the button that was clicked
        /// </param>
        /// <param name="e">
        /// event arguments
        /// </param>
        private void ExportButtonClick(object sender, EventArgs e)
        {
            if (Settings.Default.baseUrl.Equals(Settings.Default.DefaultBaseUrl))
            {
                this.ExportSelectionToFile();
            }
            else
            {
                this.OrderImagery();
            }
        }

        /// <summary>
        /// The export selection to file for the user to order imagery.
        /// </summary>
        private void ExportSelectionToFile()
        {
            try
            {
                // Set up the save file dialog
                var saveDialog = new SaveFileDialog { Filter = "CSV File|*.csv", Title = "Export GBD Information" };
                saveDialog.ShowDialog();

                if (saveDialog.FileName == string.Empty)
                {
                    return;
                }

                var fileStream = (System.IO.FileStream)saveDialog.OpenFile();
                var fileWriter = new StreamWriter(fileStream);

                // Make a copy of the current data grid view.
                var currentView = new DataGridView();
                currentView = this.dataGridView1;

                var columnHeader = string.Empty;
                for (int i = 0; i <= currentView.Columns.Count - 1; i++)
                {
                    if (currentView.Columns[i] == null || currentView.Columns[i].Name == "showPolygon"
                        || currentView.Columns[i].Name == "Selected")
                    {
                        continue;
                    }

                    if (columnHeader == string.Empty)
                    {
                        columnHeader += currentView.Columns[i].HeaderText;
                    }
                    else
                    {
                        columnHeader += "," + currentView.Columns[i].HeaderText;
                    }
                }

                fileWriter.WriteLine(columnHeader);

                // Go through each row and setup the CSV format then write to file.
                foreach (DataGridViewRow item in currentView.Rows)
                {
                    var writeRow = false;
                    var rowToBeWritten = string.Empty;
                    for (int i = 0; i <= item.Cells.Count - 1; i++)
                    {
                        // Don't record the value of if the polygon is being displayed or a null value.
                        if (item.Cells[i].Value == null || item.Cells[i].OwningColumn.Name == "showPolygon")
                        {
                            continue;
                        }

                        if (writeRow != true && item.Cells[i].OwningColumn.Name == "Selected")
                        {
                            writeRow = (bool)item.Cells[i].Value;
                            continue;
                        }
                        DataGridViewCell value = item.Cells[i];
                        if (rowToBeWritten == string.Empty)
                        {
                            rowToBeWritten += GetValue(value);
                        }
                        else
                        {
                            rowToBeWritten += "," + GetValue(value);
                        }
                    }

                    // Write the comma delimited line to file
                    if (writeRow)
                    {
                        fileWriter.WriteLine(rowToBeWritten);
                    }
                }

                // All rows have been written too so lets close off the streams.
                fileWriter.Close();
                fileStream.Close();
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        private static string GetValue(DataGridViewCell value)
        {
            string output = string.Format("=\"{0}\"", value.Value);
            return output;
        }

        /// <summary>
        /// Set all checkboxes
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="view">
        /// The view.
        /// </param>
        private void SetAllCheckBoxes(bool value, DataGridView view)
        {
            try
            {
                foreach (DataGridViewRow row in view.Rows)
                {
                    var chkBox = row.Cells["Selected"];
                    var catid = (string)row.Cells["Catalog ID"].Value;
                    var result = this.localDatatable.Select("[Catalog ID] = '" + catid + "'");
                    if (result.Length == 0)
                    {
                        continue;
                    }

                    // Set the value in the actual data table so it will remain persistant that the item has selected.
                    var myRow = result[0];
                    myRow["Selected"] = value;
                    myRow.AcceptChanges();
                }
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// Turn off the select all header and update the map
        /// </summary>
        private void SetHeaderBoxToOff()
        {
            if (this.dataGridView1.Rows.Count <= 0)
            {
                return;
            }

            var dataGridViewColumn = this.dataGridView1.Columns["Selected"];
            if (dataGridViewColumn != null)
            {
                var chkBox = (DataGridViewCheckBoxHeaderCell)dataGridViewColumn.HeaderCell;
                chkBox.isChecked = false;
                this.SetAllCheckBoxes(chkBox.isChecked, this.dataGridView1);

                var graphicsContainer = (IGraphicsContainer)ArcMap.Document.ActiveView.FocusMap;
                graphicsContainer.DeleteAllElements();
                this.DrawAoi(graphicsContainer);
                ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }

        /// <summary>
        /// The set date.
        /// </summary>
        /// <param name="picker">
        /// The picker.
        /// </param>
        /// <param name="originalValue">
        /// The original value.
        /// </param>
        private void SetDate(ref DateTimePicker picker, ref DateTime originalValue)
        {
            // If the date change occurred in the same month no change is necessary.
            if (picker.Value.Month == originalValue.Month)
            {
                // Update original value
                originalValue = picker.Value;
                return;
            }

            var daysInMonth = DateTime.DaysInMonth(picker.Value.Year, picker.Value.Month);

            if (originalValue.Day <= daysInMonth)
            {
                this.dateTimeChanging = true;
                picker.Value = picker.Value.AddDays(originalValue.Day - 1);
                this.dateTimeChanging = false;
            }

            originalValue = picker.Value;
        }

        /// <summary>
        /// The check date time.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void CheckDateTime(object sender)
        {
            if (sender.GetType() != typeof(DateTimePicker) || this.dateTimeChanging)
            {
                return;
            }

            var temp = (DateTimePicker)sender;

            // Check to see which date time picker is being used
            if (temp.Name == this.fromDateTimePicker.Name)
            {
                this.SetDate(ref this.fromDateTimePicker, ref this.startTime);
            }
            else
            {
                this.SetDate(ref this.toDateTimePicker, ref this.endTime);
            }
        }

        /// <summary>
        /// The combo box selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                this.CheckDateTime(sender);

                this.SetHeaderBoxToOff();

                this.dataView.RowFilter = this.FilterSetup();

                this.UpdateSelectedAndTotalLabels();
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The update selected and total label counts.
        /// </summary>
        private void UpdateSelectedAndTotalLabels()
        {
            var selectedRowCount = 0;
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                try
                {
                    if ((bool)row.Cells["Selected"].Value)
                    {
                        selectedRowCount++;
                    }
                }
                catch (Exception error)
                {
                    this.logWriter.Error(error);
                }
            }

            this.selectedItemsLabel.Text = GbdxResources.selectedItems + selectedRowCount;
            this.totalItemsLabel.Text = GbdxResources.totalItems + this.dataGridView1.RowCount;
        }

        /// <summary>
        /// Check all or uncheck all based on what the status is of the header checkbox.
        /// </summary>
        private void HeaderBoxClicked()
        {
            var dataGridViewColumn = this.dataGridView1.Columns["Selected"];
            if (dataGridViewColumn != null)
            {
                var chkBox = (DataGridViewCheckBoxHeaderCell)dataGridViewColumn.HeaderCell;
                this.SetAllCheckBoxes(chkBox.isChecked, this.dataGridView1);

                if (chkBox.isChecked)
                {
                    this.DrawCheckedPolygons();
                }
                else
                {
                    var graphicsContainer = ArcMap.Document.ActiveView.FocusMap as IGraphicsContainer;

                    this.ClearPolygons();
                    this.DrawAoi(graphicsContainer);
                }
            }
        }

        /// <summary>
        /// Event handler for when the data grid view's cells have a click
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DataGridView1CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // header cell click
                if (e.RowIndex == -1)
                {
                    if (this.dataGridView1.Columns[e.ColumnIndex].Name == "Selected")
                    {
                        this.HeaderBoxClicked();
                    }

                    this.UpdateSelectedAndTotalLabels();
                    return;
                }


                var cell = this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (string.Equals(cell.OwningColumn.Name, "Selected"))
                {
                    var formattedValue = this.dataGridView1.Rows[e.RowIndex].Cells["Catalog ID"].FormattedValue;

                    if (formattedValue != null)
                    {
                        // Once null check has been completed convert it to string
                        var catId = formattedValue.ToString();

                        // Find the row that has the corresponding catalog id
                        var result = this.localDatatable.Select("[Catalog ID] = '" + catId + "'");
                        if (result.Length == 0)
                        {
                            return;
                        }

                        // Set the value in the actual data table so it will remain persistant that the item has selected.
                        var myRow = result[0];
                        myRow["Selected"] = !((bool)myRow["Selected"]);
                        myRow.AcceptChanges();
                    }
                }

                this.UpdateSelectedAndTotalLabels();
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The data grid view 1 selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DataGridView1SelectionChanged(object sender, EventArgs e)
        {
            var dgv = (DataGridView)sender;

            // Don't do anything if there are no rows.
            if (dgv.Rows.Count <= 0)
            {
                return;
            }

            var formattedValue = dgv.SelectedCells[0].OwningRow.Cells["Catalog ID"].FormattedValue;

            // Check to make sure that the value isn't null
            if (formattedValue != null)
            {
                var catId = formattedValue.ToString();

                // If there is a current handle abort its operation before starting the next one
                if (this.asyncHandle != null)
                {
                    this.asyncHandle.Abort();
                }

                // Url of the image finder's thumbnail.
                var url = string.Format(
                    "/imagefinder/showBrowseImage?catalogId={0}&imageHeight=512&imageWidth=512",
                    catId);

                var request = new RestRequest(url, Method.GET);

                // Check to see the image has already been retrieved.  if it has use that instead of making the network call.
                if (this.cachedImages.ContainsKey(catId))
                {
                    this.thumbnailPictureBox.Image = this.cachedImages[catId];
                    return;
                }

                // Throw up the please wait image
                this.thumbnailPictureBox.Image = new Bitmap(GbdxResources.PleaseStandBy, this.thumbnailPictureBox.Size);

                // get the image asynchronsly 
                this.asyncHandle = this.client.ExecuteAsync(
                    request,
                    response =>
                    {
                        if (response.RawBytes == null)
                        {
                            return;
                        }

                        var ms = new MemoryStream(response.RawBytes);
                        var returnImage = Image.FromStream(ms);
                        this.thumbnailPictureBox.Image = returnImage;
                        if (!this.cachedImages.ContainsKey(catId))
                        {
                            this.cachedImages.Add(catId, returnImage);
                        }
                    });
            }
        }

        /// <summary>
        /// The thumbnail picture box load completed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ThumbnailPictureBoxLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var picBox = (PictureBox)sender;
            var catId = this.catalogIdRegEx.Match(picBox.ImageLocation).Groups["catId"].ToString();
            this.cachedImages.Add(catId, picBox.Image);
        }

        /// <summary>
        /// Event handler for when the AOI has been drawn.
        /// </summary>
        /// <param name="poly">
        /// The poly.
        /// </param>
        /// <param name="elm">
        /// The elm.
        /// </param>
        private void InstanceAoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            GbdRelay.Instance.AoiHasBeenDrawn -= this.InstanceAoiHasBeenDrawn;
            ArcMap.Application.CurrentTool = this.previouslySelectedItem;

            this.localPolygon = poly;
            this.localElement = elm;
            this.localDatatable.Clear();

            var output = GbdJarvis.CreateAois(this.localPolygon.Envelope);

            if (output == null)
            {
                MessageBox.Show(GbdxResources.redrawBoundingBox);
                return;
            }

            // Login has been completed so lets proceed with the next set of network calls.
            this.GetGbdData(output);
        }

        /// <summary>
        /// The data grid view 1 cell mouse enter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DataGridView1CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (this.displayAllPolgons)
            {
                return;
            }

            var dgv = (DataGridView)sender;
            try
            {
                // If there is actually a row.
                if (e.RowIndex >= 0)
                {
                    var formattedValue = dgv.Rows[e.RowIndex].Cells["Catalog ID"].FormattedValue;
                    if (formattedValue != null)
                    {
                        var catId = formattedValue.ToString();

                        if (!this.allResults.ContainsKey(catId))
                        {
                            return;
                        }

                        var item = this.allResults[catId];

                        var graphicsContainer = ArcMap.Document.ActiveView.FocusMap as IGraphicsContainer;

                        if (graphicsContainer != null)
                        {
                            graphicsContainer.DeleteAllElements();
                            this.DrawAoi(graphicsContainer);
                            this.DrawPoly(item, graphicsContainer);
                        }
                    }

                    this.DrawCheckedPolygons();
                }
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        #endregion

        #region Imagery Ordering

        /// <summary>
        /// Update the order table with imagery that was ordered in the payload
        /// </summary>
        /// <param name="payload">
        /// the GBD orders for the imagery
        /// </param>
        /// <param name="orderTable">
        /// The order data table.
        /// </param>
        private static void UpdateOrderTable(IEnumerable<GbdOrder> payload, ref DataTable orderTable)
        {
            foreach (GbdOrder item in payload)
            {
                // Convert the timestamp received in the gbd order to something more useful.
                var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                epochTime = epochTime.AddMilliseconds(item.header.messageDateTimeStamp);

                var row = orderTable.NewRow();
                row["Order ID"] = item.salesOrderNumber;
                row["Order Date"] = epochTime.ToString("g");
                row["Service Provider"] = item.header.serviceProvider;

                if (item.lines.Count > 0)
                {
                    var status = item.lines[0].lineItemStatus;

                    if (!string.IsNullOrEmpty(status))
                    {
                        row["Order Status"] = item.lines[0].lineItemStatus;
                    }
                }

                orderTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// Get the user selected imagery cat ids for ordering
        /// </summary>
        /// <param name="grid">
        /// The data grid that contains the imagery information.
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        private static List<string> GetImageryToBeOrdered(DataGridView grid)
        {
            var catIdList = new List<string>();
            foreach (DataGridViewRow item in grid.Rows)
            {
                var addRow = false;
                for (int i = 0; i <= item.Cells.Count - 1; i++)
                {
                    // Don't record the value of if the polygon is being displayed or a null value.
                    if (item.Cells[i].Value == null || item.Cells[i].OwningColumn.Name == "showPolygon")
                    {
                        continue;
                    }

                    if (addRow != true && item.Cells[i].OwningColumn.Name == "Selected")
                    {
                        addRow = (bool)item.Cells[i].Value;
                        continue;
                    }

                    if (addRow && item.Cells[i].OwningColumn.Name == "Catalog ID")
                    {
                        catIdList.Add((string)item.Cells[i].Value);
                    }
                }
            }

            return catIdList;
        }

        /// <summary>
        /// Write the GBD orders to file from the GBD Order's Table.
        /// </summary>
        /// <param name="orders">
        /// The orders.
        /// </param>
        private void WriteGbdOrdersToFile(List<GbdOrder> orders)
        {
            var serializedOutput = JsonConvert.SerializeObject(orders);
            File.WriteAllText(this.filePath, serializedOutput);
        }

        /// <summary>
        /// Load the GBD orders from given file path.
        /// </summary>
        /// <param name="filepath">
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        private List<GbdOrder> LoadGbdOrdersFromFile(string filepath)
        {
            var serializedOutput = File.ReadAllText(this.filePath);

            var list = JsonConvert.DeserializeObject<List<GbdOrder>>(serializedOutput);

            return list;
        }

        /// <summary>
        /// Order the selected imagery.  
        /// </summary>
        private void OrderImagery()
        {
            try
            {
                // get the ID's of the images to be ordered
                var catIdList = GetImageryToBeOrdered(this.dataGridView1);

                var output = JsonConvert.SerializeObject(catIdList);

                // setup the request for the orders
                var request = new RestRequest("/raster-catalog/api/gbd/orders/v1", Method.POST);
                request.AddHeader("Authorization", "Bearer " + this.comms.GetAccessToken());
                request.AddParameter("application/json", output, ParameterType.RequestBody);

                var commsClient = this.comms.GetClient();

                if (commsClient == null)
                {
                    commsClient = new RestClient(GbdxHelper.GetEndpointBase(Settings.Default));
                }

                commsClient.ExecuteAsync<List<GbdOrder>>(
                    request,
                    resp => this.Invoke(new ExecuteAfterResponse(this.HandleOrderResponse), resp.Data));
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The handle order response.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void HandleOrderResponse(List<GbdOrder> data)
        {
            try
            {
                UpdateOrderTable(data, ref this.orderTable);
                this.gbdOrderList.AddRange(data);
                this.WriteGbdOrdersToFile(this.gbdOrderList);
                this.UpdateStatus();
                this.tabControl1.SelectTab(this.statusPage);
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The create net object.
        /// </summary>
        /// <param name="success">
        /// The success.
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        private static NetObject CreateNetObject(ref bool success)
        {
            string decryptedPassword;
            success = Aes.Instance.Decrypt128(Settings.Default.password, out decryptedPassword);
            if (!success)
            {
                return null;
            }

            // Creating network object.
            NetObject netObj = new NetObject
                                   {
                                       AddressUrl = Settings.Default.GbdSearchPath,
                                       BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                                       AuthEndpoint = Settings.Default.authenticationServer,
                                       User = Settings.Default.username,
                                       Password = decryptedPassword,
                                   };

            return netObj;
        }


        #endregion

        #region Status Refresh

        /// <summary>
        /// The refresh button click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void RefreshButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.UpdateStatus();
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        private void CheckAccessToken()
        {
            if (this.comms.GetAccessToken() == null)
            {
                string decryptedPassword;
                var success = Aes.Instance.Decrypt128(Settings.Default.password, out decryptedPassword);
                if (!success)
                {
                    return;
                }

                var netObj = new NetObject
                                 {
                                     AddressUrl = Settings.Default.GbdSearchPath,
                                     BaseUrl = GbdxHelper.GetEndpointBase(Settings.Default),
                                     AuthEndpoint = Settings.Default.authenticationServer,
                                     User = Settings.Default.username,
                                     Password = decryptedPassword,
                                     AuthUrl =
                                         string.IsNullOrEmpty(Settings.Default.AuthBase)
                                             ? Settings.Default.DefaultAuthBase
                                             : Settings.Default.AuthBase,
                                     ApiKey = Settings.Default.apiKey,
                                 };

                this.comms.AuthenticateNetworkObject(ref netObj);
            }
        }

        private void UpdateStatus()
        {
            this.CheckAccessToken();

            // make sure the thread isn't running.  if it is gracefully kill it ...
            this.ThreadLifeCheck(this.statusUpdateThread);

            // set up new thread
            this.statusUpdateThread =
                new Thread(
                    () =>
                    this.CheckOrderStatus(
                        this.GetOrderIdsForRefresh(),
                        new RestClient(GbdxHelper.GetEndpointBase(Settings.Default)),
                        this.comms.GetAccessToken()));

            // execute the job
            this.statusUpdateThread.Start();
        }

        /// <summary>
        /// The get order ids for refresh.
        /// </summary>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        private List<string> GetOrderIdsForRefresh()
        {

            List<string> list = new List<string>();
            try
            {
                foreach (DataGridViewRow row in this.orderDataGridView.Rows)
                {
                    var item = row.Cells["Order ID"].Value.ToString();
                    var currentStatus = row.Cells["Order Status"].Value.ToString();
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!string.Equals(currentStatus, "DELIVERED"))
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }

            return list;
        }

        /// <summary>
        /// The check order status.
        /// </summary>
        /// <param name="orderList">
        /// The order list.
        /// </param>
        /// <param name="webClient">
        /// The web client.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        private void CheckOrderStatus(List<string> orderList, IRestClient webClient, string token)
        {
            foreach (var id in orderList)
            {
                if (this.okToWork)
                {
                    var request = new RestRequest("/raster-catalog/api/gbd/orders/v1/status/" + id, Method.GET);
                    request.AddHeader("Authorization", "Bearer " + token);

                    var result = webClient.Execute<GbdOrder>(request);
                    var keepRunning = true;
                    var numTries = 0;
                    while (keepRunning)
                    {
                        if (result.Data.salesOrderNumber == null && numTries <= 5)
                        {
                            numTries++;
                            result = webClient.Execute<GbdOrder>(request);
                        }
                        else
                        {
                            keepRunning = false;
                        }
                    }

                    // Callback to the main UI thread to update the data table
                    this.Invoke(new UpdateStatusCallback(this.UpdateRecordStatus), result.Data);
                }
            }
        }


        /// <summary>
        /// Update record status
        /// </summary>
        /// <param name="orderStatusUpdate">
        /// The order status update.
        /// </param>
        private void UpdateRecordStatus(GbdOrder orderStatusUpdate)
        {
            try
            {
                // Don't bother if the GBD Order is null
                if (orderStatusUpdate.salesOrderNumber != null)
                {
                    foreach (DataRow row in this.orderTable.Rows)
                    {
                        var orderId = row["Order ID"].ToString();
                        if (orderId != orderStatusUpdate.salesOrderNumber)
                        {
                            continue;
                        }

                        row["Order Status"] = orderStatusUpdate.lines[0].lineItemStatus;
                        this.UpdateOrderListStatus(orderId, orderStatusUpdate.lines[0].lineItemStatus);
                        this.WriteGbdOrdersToFile(this.gbdOrderList);

                        // Since there should only be one order that has the specfied order id lets stop searching
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The update order list status.
        /// </summary>
        /// <param name="orderId">
        /// The order id.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        private void UpdateOrderListStatus(string orderId, string status)
        {
            // should only be one item with that order id.
            foreach (GbdOrder item in this.gbdOrderList.Where(item => item.salesOrderNumber == orderId))
            {
                if (item.lines.Count <= 0)
                {
                    continue;
                }

                var line = item.lines[0];
                if (line != null)
                {
                    line.lineItemStatus = status;
                }
            }
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draw all viewable polygons.
        /// </summary>
        private void DrawViewablePolygons()
        {
            // only draw if the user has checked the polygon checkbox.
            if (!this.displayAllPolgons)
            {
                return;
            }

            var graphicsContainer = ArcMap.Document.ActiveView.FocusMap as IGraphicsContainer;

            var rowCollection = this.dataGridView1.Rows;

            // only draw polygons when the graphic container isn't null
            if (graphicsContainer != null)
            {
                for (int i = 0; i <= rowCollection.Count - 1; i++)
                {
                    var row = rowCollection[i];

                    // get the formatted value of the catID column.
                    var formattedValue = row.Cells["Catalog ID"].FormattedValue;
                    if (formattedValue != null)
                    {
                        var catId = formattedValue.ToString();

                        if (!this.allResults.ContainsKey(catId))
                        {
                            continue;
                        }

                        var item = this.allResults[catId];

                        // Draw the polygon
                        this.DrawPoly(item, graphicsContainer);
                    }
                }
            }

            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// Draw the polygons that the user has checked.
        /// </summary>
        private void DrawCheckedPolygons()
        {
            try
            {
                var graphicsContainer = (IGraphicsContainer)ArcMap.Document.ActiveView.FocusMap;
                if (graphicsContainer == null)
                {
                    return;
                }

                var rows = this.dataGridView1.Rows;

                foreach (DataGridViewRow row in rows)
                {
                    var catIdValue = row.Cells["Catalog ID"].Value;
                    if (catIdValue == null)
                    {
                        continue;
                    }

                    var catId = catIdValue.ToString();
                    var drawable = (bool)row.Cells["Selected"].Value;
                    if (drawable)
                    {
                        var itemToDraw = this.allResults[catId];
                        this.DrawPoly(itemToDraw, graphicsContainer);
                    }
                }

                ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch (Exception error)
            {
                this.logWriter.Error(error);
            }
        }

        /// <summary>
        /// The draw aoi.
        /// </summary>
        /// <param name="graphicContainer">
        /// The graphic container.
        /// </param>
        private void DrawAoi(IGraphicsContainer graphicContainer)
        {
            graphicContainer.AddElement(this.localElement, 0);
        }

        /// <summary>
        /// Draw the polygon contained within the properties.
        /// </summary>
        /// <param name="polyToBeDrawn">
        /// The polygon to be drawn.
        /// </param>
        /// <param name="graphicContainer">
        /// The graphic container.
        /// </param>
        private void DrawPoly(Properties polyToBeDrawn, IGraphicsContainer graphicContainer)
        {
            var poly = new PolygonClass();
            poly.Project(ArcMap.Document.ActiveView.Extent.SpatialReference);
            foreach (var pnt in polyToBeDrawn.Points)
            {
                var tempPoint = new PointClass();
                tempPoint.PutCoords(pnt.X, pnt.Y);
                tempPoint.SpatialReference = Jarvis.ProjectedCoordinateSystem;
                tempPoint.Project(ArcMap.Document.ActiveView.Extent.SpatialReference);
                poly.AddPoint(tempPoint);
            }

            IElement elm = new PolygonElementClass();
            elm.Geometry = poly;

            graphicContainer.AddElement(elm, 0);
        }

        /// <summary>
        /// Clear polygons from the display.
        /// </summary>
        private void ClearPolygons()
        {
            var graphicsContainer = (IGraphicsContainer)ArcMap.Document.ActiveView.FocusMap;
            graphicsContainer.DeleteAllElements();
            this.userSelectedPolygons.Clear();
            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        #region AddinClass

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            /// <summary>
            /// The m_window UI.
            /// </summary>
            private GbdDockableWindow dockedWindowUi;

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
                this.dockedWindowUi = new GbdDockableWindow(this.Hook);
                return this.dockedWindowUi.Handle;
            }

            /// <summary>
            /// The dispose.
            /// </summary>
            /// <param name="disposing">
            /// The disposing.
            /// </param>
            protected override void Dispose(bool disposing)
            {
                if (this.dockedWindowUi != null)
                {
                    this.dockedWindowUi.Dispose(disposing);
                }

                base.Dispose(disposing);
            }
        }

        #endregion


        #endregion

        private static string GetAccessToken(string apiKey, string username, string password)
        {
            string tok = string.Empty;

            var restClient = new RestClient("https://geobigdata.io");
            var request = new RestRequest(Settings.Default.authenticationServer, Method.POST);
            request.AddHeader("Authorization", string.Format("Basic {0}", apiKey));
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            var response = restClient.Execute<AccessToken>(request);

            Jarvis.Logger.Info(response.ResponseUri.ToString());

            if (response.Data != null)
            {
                tok = response.Data.access_token;
            }

            return tok;
        }

    }
}