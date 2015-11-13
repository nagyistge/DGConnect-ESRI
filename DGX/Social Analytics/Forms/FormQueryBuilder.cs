// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormQueryBuilder.cs" company="DigitalGlobe">
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

namespace Dgx
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Net;
    using System.Windows.Forms;

    using Dgx.Properties;
    using Dgx.Social_Analytics.Forms;

    using Encryption;

    using ESRI.ArcGIS.Geometry;

    using NetworkConnections;

    using RestSharp;

    using StoredQueries;

    using Point = System.Drawing.Point;

    /// <summary>
    /// The form query builder.
    /// </summary>
    public partial class FormQueryBuilder : Form
    {
        /// <summary>
        /// The query limit radio button list.
        /// </summary>
        private readonly RbList queryLimit;

        /// <summary>
        /// The time limit radio button list.
        /// </summary>
        private readonly RbList timeLimit;

        /// <summary>
        /// Class implementing IDgxComms interface.
        /// </summary>
        private readonly SmaComms cloudComms;

        /// <summary>
        /// The stored query class.
        /// </summary>
        private readonly StoredQuery storedQuery;

        /// <summary>
        /// The valid authentication.
        /// </summary>
        private bool authentication = true;

        /// <summary>
        /// The advanced options shown boolean variable.
        /// </summary>
        private bool advancedOptionsShown;

        /// <summary>
        /// True if the filled heart image is being displayed.
        /// </summary>
        private bool filledHeartImageShown = false;

        /// <summary>
        /// The auto complete source for auto completion on the text box for the keyword search field.
        /// </summary>
        private AutoCompleteStringCollection autoCompleteSource;

        /// <summary>
        /// The list of queries that were retrieved from the stored query service.
        /// </summary>
        private List<SavedQuery> localQueries;

        /// <summary>
        /// The IEnvelope of the bounding box.
        /// </summary>
        private IEnvelope boundingBoxEnvelope;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormQueryBuilder"/> class.
        /// </summary>
        /// <param name="env">
        /// The IEnvelope of the bounding box.
        /// </param>
        public FormQueryBuilder(IEnvelope env)
        {
            this.InitializeComponent();

            if (string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.username) || string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.password))
            {
                MessageBox.Show(DGXSettings.DgxResources.InvalidUserPass);
                this.ValidAuthentication = false;
                return;
            }

            // Stored Query Section
            this.cloudComms = new SmaComms();
            this.storedQuery = new StoredQuery(this.cloudComms);
            this.localQueries = new List<SavedQuery>();
            var nobject = CreateNetObject();
            var authentic = this.cloudComms.AuthenticateNetworkObject(ref nobject);

            if (!authentic)
            {
                MessageBox.Show(DGXSettings.DgxResources.InvalidUserPass);
                this.ValidAuthentication = false;
                return;
            }

            this.boundingBoxEnvelope = env;

            // Set up the table layout correctly for the smaller view
            this.tableLayoutPanel1.SetRow(this.buttonCancel, 2);
            this.tableLayoutPanel1.SetRow(this.buttonRun, 2);
            this.tableLayoutPanel1.SetRow(this.groupBox1, 5);
            this.Size = new Size(412, 220);

            // the default time setting is 1 week
            this.dateTimePickerStart.MinDate = new DateTime(1995, 1, 1);
            this.dateTimePickerStart.MaxDate = DateTime.Now.AddMinutes(-5);
            this.dateTimePickerEnd.MinDate = new DateTime(1995, 1, 1);
            this.dateTimePickerEnd.MaxDate = DateTime.Now;

            DateTime nowTime = DateTime.Now;
            this.dateTimePickerStart.Value = nowTime.AddDays(-7);
            this.dateTimePickerStart.CustomFormat = "MM/dd/yyyy HH:mm";
            this.dateTimePickerStart.Enabled = false;
            this.dateTimePickerEnd.CustomFormat = "MM/dd/yyyy HH:mm";
            this.dateTimePickerEnd.Enabled = false;
            this.rb7Days.Checked = true;

            this.Shown += this.FormQueryBuilderShown;
            this.groupBox1.Visible = this.advancedOptionsShown;
            this.groupBox4.Visible = this.advancedOptionsShown;
            this.groupBox5.Visible = this.advancedOptionsShown;

            this.queryLimit = new RbList();
            this.queryLimit.Add(this.radioButton500);
            this.queryLimit.Add(this.radioButton1000);
            this.queryLimit.Add(this.radioButton2500);
            this.queryLimit.Add(this.radioButton5000);
            this.queryLimit.Add(this.radioButton10000);

            this.timeLimit = new RbList();
            this.timeLimit.Add(this.rb6Hours);
            this.timeLimit.Add(this.rb12Hours);
            this.timeLimit.Add(this.rb1Day);
            this.timeLimit.Add(this.rb7Days);
            this.timeLimit.Add(this.rb1Month);
            this.timeLimit.Add(this.rb3Months);
            this.timeLimit.Add(this.rb6Months);
            this.timeLimit.Add(this.rbCustom);

            this.autoCompleteSource = new AutoCompleteStringCollection();

            // On form creation re-constitute users previous selection.
            this.RestoreUserSelection();
            var request = new RestRequest("/app/broker/userprofile/api/workspace/EsriAddin");
            this.cloudComms.Client.ExecuteAsync<List<SavedQuery>>(
                request,
                response =>
                    {
                        this.Invoke(new CallbackRestResponse(this.ProcessQueries), response);
                    });
        }

        /// <summary>
        /// The callback rest response.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        private delegate void CallbackRestResponse(IRestResponse<List<SavedQuery>> response);

        /// <summary>
        /// Gets or sets a value indicating whether valid authentication.
        /// </summary>
        public bool ValidAuthentication
        {
            get
            {
                return this.authentication;
            }

            set
            {
                this.authentication = value;
            }
        }
        #region Public Methods

        /// <summary>
        /// Get value associated with radio button checked in regards to the number of results to return.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetRows()
        {
            if (this.radioButton500.Checked)
            {
                return 500;
            }

            if (this.radioButton1000.Checked)
            {
                return 1000;
            }

            if (this.radioButton2500.Checked)
            {
                return 2500;
            }

            if (this.radioButton5000.Checked)
            {
                return 5000;
            }

            return 10000;
        }

        /// <summary>
        /// Get the typed keywords from the textbox
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetKeywords()
        {
            return this.keywords.Text;
        }

        /// <summary>
        /// Set a warning text for the dialog
        /// </summary>
        /// <param name="text">
        /// The text of the warning
        /// </param>
        public void SetWarningStatus(string text)
        {
            Color cl = Color.Red;
            this.toolStripStatusLabel.BackColor = cl;
            this.toolStripStatusLabel.Text = text;
        }

        /// <summary>
        /// The set status.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public void SetStatus(string text)
        {
            var cl = Color.Empty;
            this.toolStripStatusLabel.BackColor = cl;
            this.toolStripStatusLabel.Text = text;
        }

        /// <summary>
        /// The get stop time from end date time picker.
        /// </summary>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public DateTime GetStopTime()
        {
            return this.dateTimePickerEnd.Value;
        }

        /// <summary>
        /// The get start time from start date time picker.
        /// </summary>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public DateTime GetStartTime()
        {
            return this.dateTimePickerStart.Value;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a AutoCompleteStringCollection from a list of Query.  
        /// </summary>
        /// <param name="queries">
        /// The queries.
        /// </param>
        /// <param name="source">
        /// Source to have the queries added too.
        /// </param>
        /// <returns>
        /// The <see cref="AutoCompleteStringCollection"/>.
        /// </returns>
        private static AutoCompleteStringCollection GetAutoCompleteSource(List<SavedQuery> queries, AutoCompleteStringCollection source)
        {
            foreach (var item in queries)
            {
                source.Add(item.query);
            }

            return source;
        }

        /// <summary>
        /// Create net object.
        /// </summary>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        private static NetObject CreateNetObject()
        {
            // Object initialize as many of the necessary settings as possible.
            var netobj = new NetObject
            {
                CookieJar = new CookieContainer(),
                TimeoutSetting = 8000,
                User = DGXSettings.Properties.Settings.Default.username,
                BaseUrl = DgxHelper.GetCasBaseEndpoint(DGXSettings.Properties.Settings.Default)
            };
            netobj.AuthEndpoint = DgxHelper.GetCasAuthenticationEndpoint(DGXSettings.Properties.Settings.Default);
            netobj.TicketEndpoint = DgxHelper.GetCasTicketEndpoint(DGXSettings.Properties.Settings.Default);

            // Decrypt the password.  If an error occurs inform the user and return an unusable object.
            string pass;
            var success = Aes.Instance.Decrypt128(DGXSettings.Properties.Settings.Default.password, out pass);
            if (success)
            {
                netobj.Password = pass;
            }
            else
            {
                MessageBox.Show(DGXSettings.DgxResources.PasswordProblemCheckSettings);
                return null;
            }

            return netobj;
        }

        /// <summary>
        /// The picture box favorite click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PictureBoxFavoriteClick(object sender, EventArgs e)
        {
            this.filledHeartImageShown = !this.filledHeartImageShown;
            this.button1.Image = this.filledHeartImageShown ? DGXSettings.DgxResources.filledStar : DGXSettings.DgxResources.emptyStar;

            // If True then lets save the query.
            if (this.filledHeartImageShown)
            {
                this.SaveQuery(this.keywords.Text, this.dateTimePickerStart.Value, this.dateTimePickerEnd.Value, ref this.localQueries, this.storedQuery, this.queryLimit.SelectedText, this.timeLimit.SelectedText);
            }
        }

        /// <summary>
        /// Restore user selection of query limit and time limit
        /// </summary>
        private void RestoreUserSelection()
        {
            if (!string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.sma_time) && this.timeLimit != null)
            {
                this.timeLimit.RestoreSelection(DGXSettings.Properties.Settings.Default.sma_time);
            }

            if (!string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.sma_query_limit) && this.queryLimit != null)
            {
                this.queryLimit.RestoreSelection(DGXSettings.Properties.Settings.Default.sma_query_limit);
            }

            if (this.timeLimit == null)
            {
                return;
            }

            if (!string.Equals(this.timeLimit.SelectedText, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            this.dateTimePickerStart.Value = DGXSettings.Properties.Settings.Default.sma_date_from;
            this.dateTimePickerEnd.Value = DGXSettings.Properties.Settings.Default.sma_date_to;
        }

        #region Radio Button Event Handelers

        /// <summary>
        /// The radio button 1000 checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void RadioButton1000CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton10000.Checked)
            {
                this.SetStatus("Max Records = 10000");
            }
            else
            {
                this.SetStatus("...");
            }
        }

        /// <summary>
        /// The radio button custom checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void RbCustomCheckedChanged(object sender, EventArgs e)
        {
            this.dateTimePickerStart.Enabled = this.rbCustom.Checked;
            this.dateTimePickerEnd.Enabled = this.rbCustom.Checked;
        }

        /// <summary>
        /// The radio button 7 days checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb7DaysCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddDays(-7);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 1 day checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb1DayCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddDays(-1);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 12 hours checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb12HoursCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddHours(-12);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 6 hours checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb6HoursCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddHours(-6);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 1 month checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb1MonthCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddMonths(-1);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 3 months checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb3MonthsCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddMonths(-3);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        /// <summary>
        /// The radio button 6 months checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Rb6MonthsCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var rb = (RadioButton)sender;
                if (!rb.Checked)
                {
                    return;
                }

                this.dateTimePickerStart.Value = DateTime.Now.AddMonths(-6);
                var now = DateTime.Now;
                this.dateTimePickerEnd.MaxDate = now;
                this.dateTimePickerEnd.Value = now;
            }
            catch
            {
                MessageBox.Show(DGXSettings.DgxResources.invalidDateTime);
            }
        }

        #endregion

        /// <summary>
        /// Event handler for the keywords textbox for when a key is pressed
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void KeywordsKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                this.ButtonRunClick(sender, null);
            }
        }

        /// <summary>
        /// Event handler for when the advanced options label link is clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AdvancedOptionsLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.advancedOptionsShown = !this.advancedOptionsShown;

            this.groupBox1.Visible = this.advancedOptionsShown;
            this.groupBox4.Visible = this.advancedOptionsShown;
            this.groupBox5.Visible = this.advancedOptionsShown;
            if (this.advancedOptionsShown)
            {
                this.Size = new Size(412, 585);
                this.advancedOptionsLinkLabel.Text = DGXSettings.DgxResources.Hide_Advanced_Options;
                this.tableLayoutPanel1.SetRow(this.buttonCancel, 5);
                this.tableLayoutPanel1.SetRow(this.groupBox1, 2);
                this.tableLayoutPanel1.SetRow(this.buttonRun, 5);
            }
            else
            {
                this.Size = new Size(412, 220);
                this.advancedOptionsLinkLabel.Text = DGXSettings.DgxResources.Show_Advanced_Options;
                this.tableLayoutPanel1.SetRow(this.buttonCancel, 2);
                this.tableLayoutPanel1.SetRow(this.buttonRun, 2);
                this.tableLayoutPanel1.SetRow(this.groupBox1, 5);
            }
        }

        /// <summary>
        /// The date time picker start key is pressed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DateTimePickerStartKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.SetCancel();
            }
        }

        /// <summary>
        /// Cancel and close the dialog
        /// </summary>
        private void SetCancel()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Event handler for when the run button is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonRunClick(object sender, EventArgs e)
        {
            if (this.Validate())
            {
                // Save the user selected settings
                DGXSettings.Properties.Settings.Default.sma_date_from = this.dateTimePickerStart.Value;
                DGXSettings.Properties.Settings.Default.sma_date_to = this.dateTimePickerEnd.Value;
                DGXSettings.Properties.Settings.Default.sma_query_limit = this.queryLimit.SelectedText;
                DGXSettings.Properties.Settings.Default.sma_time = this.timeLimit.SelectedText;
                DGXSettings.Properties.Settings.Default.Save();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Validate the selected options.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private new bool Validate()
        {
            if (this.dateTimePickerStart.Value >= this.dateTimePickerEnd.Value)
            {
                this.SetWarningStatus("Invalid dates...");
                return false;
            }

            if (this.keywords.Text.Trim() == string.Empty)
            {
                this.SetWarningStatus("Must specify a keyword. For all use *");
                return false;
            }

            if (this.keywords.Text.Trim().StartsWith("_") || this.keywords.Text.Trim().StartsWith("'") || this.keywords.Text.Trim().StartsWith(",") || this.keywords.Text.Trim().StartsWith(".") || this.keywords.Text.Trim().StartsWith("%") || this.keywords.Text.Trim().StartsWith(";") || this.keywords.Text.Trim().StartsWith("-"))
            {
                this.SetWarningStatus("Keyword cannot start with _',.%;-");
                return false;
            }

            if (this.keywords.Text.Trim().Contains("~") || this.keywords.Text.Trim().Contains("!") || this.keywords.Text.Trim().Contains("^") || this.keywords.Text.Trim().Contains("(") || this.keywords.Text.Trim().Contains(")") || this.keywords.Text.Trim().Contains("+") || this.keywords.Text.Trim().Contains("[") || this.keywords.Text.Trim().Contains("]") || this.keywords.Text.Trim().Contains("{") || this.keywords.Text.Trim().Contains("}") || this.keywords.Text.Trim().Contains(":") || this.keywords.Text.Trim().Contains("/") || this.keywords.Text.Trim().Contains("\"") || this.keywords.Text.Trim().Contains("\\"))
            {
                this.SetWarningStatus("Keyword cannot contain ~!^()+[{]}\\:\"/@#&=`$|<>");
                return false;
            }

            if (!this.keywords.Text.Trim().Contains("@") && !this.keywords.Text.Trim().Contains("#") &&
                !this.keywords.Text.Trim().Contains("&") && !this.keywords.Text.Trim().Contains("=") &&
                !this.keywords.Text.Trim().Contains("`") && !this.keywords.Text.Trim().Contains("$") &&
                !this.keywords.Text.Trim().Contains("|") && !this.keywords.Text.Trim().Contains("<") &&
                !this.keywords.Text.Trim().Contains(">"))
            {
                return true;
            }

            this.SetWarningStatus("Keyword cannot contain ~!^()+[{]}\\:\"/@#&=`$|<>");
            return false;
        }

        /// <summary>
        /// Event handler for when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonCancelClick(object sender, EventArgs e)
        {
            this.SetCancel();
        }

        /// <summary>
        /// The form query builder's shown event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void FormQueryBuilderShown(object sender, EventArgs e)
        {
            this.keywords.Focus();
        }

        /// <summary>
        /// Process the response that has all of the queries
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        private void ProcessQueries(IRestResponse<List<SavedQuery>> response)
        {
            // only proceed if we have an ok status code
            if (response.StatusCode != HttpStatusCode.OK || response.Data == null)
            {
                return;
            }

            this.LoadFavoritesMenuStrip(ref this.favoritesToolStripMenuItem, response.Data);
            this.localQueries = response.Data;
            this.autoCompleteSource = GetAutoCompleteSource(response.Data, this.autoCompleteSource);

            this.keywords.AutoCompleteCustomSource = this.autoCompleteSource;
            this.keywords.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.keywords.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        #endregion
        #region Stored Queries

        /// <summary>
        /// Saves the user settings associated with the query.
        /// </summary>
        /// <param name="queryToBesaved">
        /// The actual query in the keywords textbox
        /// </param>
        /// <param name="startTime">
        /// Start time of the query
        /// </param>
        /// <param name="endTime">
        /// End time of the query
        /// </param>
        /// <param name="currentQueryList">
        /// Current list of queries
        /// </param>
        /// <param name="sq">
        /// Stored Query class
        /// </param>
        /// <param name="qLimit">
        /// Query Limit Text
        /// </param>
        /// <param name="dateLimit">
        /// query date limit text
        /// </param>
        private void SaveQuery(
            string queryToBesaved,
            DateTime startTime,
            DateTime endTime,
            ref List<SavedQuery> currentQueryList,
            IStoredQuery sq,
            string qLimit,
            string dateLimit)
        {
            // Need a network object so lets create one.
            var netobj = CreateNetObject();
            netobj.AddressUrl = "/app/broker/userprofile/api/workspace/EsriAddin";

            // Create the new query class
            var newQuery = new SavedQuery
                               {
                                   name = queryToBesaved,
                                   query = queryToBesaved,
                                   Start = startTime,
                                   End = endTime,
                                   date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
                                   properties =
                                       new StoredQueries.Properties
                                           {
                                               EsriAddin =
                                                   new EsriAddin()
                                                       {
                                                           DateLimit = dateLimit,
                                                           QueryLimit = qLimit
                                                       }
                                           },
                                   map =
                                       new Map()
                                           {
                                               Bottom = this.boundingBoxEnvelope.YMin,
                                               Left = this.boundingBoxEnvelope.XMin,
                                               Right = this.boundingBoxEnvelope.XMax,
                                               Top = this.boundingBoxEnvelope.YMax
                                           }
                               };

            sq.UpdateQuery(netobj, newQuery);

            currentQueryList.Add(newQuery);

            this.autoCompleteSource.Add(queryToBesaved);

            var favoriteMenuItem = new FavoriteMenuStripItem(newQuery);
            favoriteMenuItem.Click += this.QueryItemClick;
            favoriteMenuItem.MouseDown += this.QueryItemMouseDown;
            this.favoritesToolStripMenuItem.DropDownItems.Add(favoriteMenuItem);
        }

        /// <summary>
        /// Load the favorites menu strip item.
        /// </summary>
        /// <param name="favorites">
        /// ToolStripMenuItem that is getting children.
        /// </param>
        /// <param name="queries">
        /// Queries being added as children
        /// </param>
        private void LoadFavoritesMenuStrip(ref ToolStripMenuItem favorites, IEnumerable<SavedQuery> queries)
        {
            // Clear the drop down items
            favorites.DropDownItems.Clear();

            // Take the list and add sub items to the "Favorites" menu item.
            foreach (var queryItem in queries.Select(item => new FavoriteMenuStripItem(item)))
            {
                queryItem.Click += this.QueryItemClick;
                queryItem.MouseDown += this.QueryItemMouseDown;
                favorites.DropDownItems.Add(queryItem);
            }
        }

        /// <summary>
        /// When favorite item gets a mouse down event.
        /// </summary>
        /// <param name="sender">
        /// Favorite menu strip item
        /// </param>
        /// <param name="e">
        /// Mouse Event args
        /// </param>
        private void QueryItemMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:

                    // make the sender usable
                    var item = (FavoriteMenuStripItem) sender;

                    // Create the menu that appears on right click
                    var contextMenu = new ContextMenu();

                    // Create the delete menu item
                    var item1 = new CustomMenuItem(item)
                    {
                        Text = "Delete"
                    };

                    // Add it to the context menu
                    contextMenu.MenuItems.Add(item1);

                    // Setup event handler for click
                    item1.Click += this.DelClick;

                    // Everything is all good so lets display the context menu to the user.
                    contextMenu.Show(this.menuStrip1, this.menuStrip1.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y)));
                    break;
                case MouseButtons.Left:
                    return;
            }
        }

        /// <summary>
        /// Event handler to create a query item.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void QueryItemClick(object sender, EventArgs e)
        {
            var queryMenuitem = (FavoriteMenuStripItem) sender;

            // Set the Custom radio button
            this.rbCustom.Checked = true;

            // Set Query, start and end time
            this.keywords.Text = queryMenuitem.SourceSavedQuery.query;

            // Restore the selected time if custom was choosen otherwise re-select originally selected radio button
            if(queryMenuitem.SourceSavedQuery.properties.EsriAddin.DateLimit == "Custom")
            {
                this.dateTimePickerStart.Value = queryMenuitem.SourceSavedQuery.Start;
                this.dateTimePickerEnd.MaxDate = DateTime.Now;
                this.dateTimePickerEnd.Value = queryMenuitem.SourceSavedQuery.End;
            }
            else
            {
                this.timeLimit.RestoreSelection(queryMenuitem.SourceSavedQuery.properties.EsriAddin.DateLimit);
            }

            // Restore the originally selected query limit
            this.queryLimit.RestoreSelection(queryMenuitem.SourceSavedQuery.properties.EsriAddin.QueryLimit);

            this.filledHeartImageShown = !this.filledHeartImageShown;
            this.button1.Image = DGXSettings.DgxResources.filledStar;
        }

        /// <summary>
        /// Event handler for when a stored query is deleted by right clicking and selecting delete.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DelClick(object sender, EventArgs e)
        {
            var item = (CustomMenuItem) sender;

            // Need a network object so lets create one.
            var netobj = CreateNetObject();

            netobj.AddressUrl = "/app/broker/userprofile/api/workspace/EsriAddin?name="+item.FavoriteMenuStripItem.SourceSavedQuery.name;

            // Collapses the menu strip
            item.FavoriteMenuStripItem.Owner.Hide();

            this.localQueries.Remove(item.FavoriteMenuStripItem.SourceSavedQuery);

            this.storedQuery.DeleteQuery(netobj);

            this.autoCompleteSource.Remove(item.FavoriteMenuStripItem.SourceSavedQuery.query);
            this.favoritesToolStripMenuItem.DropDownItems.Remove(item.FavoriteMenuStripItem);
        }

        #endregion

        /// <summary>
        /// The keywords text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void KeywordsTextChanged(object sender, EventArgs e)
        {
            this.filledHeartImageShown = !this.filledHeartImageShown;
            this.button1.Image = DGXSettings.DgxResources.emptyStar;
        }
    }
}
