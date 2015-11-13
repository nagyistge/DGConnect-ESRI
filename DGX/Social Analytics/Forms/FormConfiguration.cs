// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormConfiguration.cs" company="DigitalGlobe">
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
//   Defines the FormConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx.Forms
{
    using System;
    using System.Drawing;
    using System.Net;
    using System.Net.Configuration;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using Dgx.Properties;

    using Encryption;

    using NetworkConnections;

    /// <summary>
    /// The configuration form.
    /// </summary>
    public partial class FormConfiguration : Form
    {
        /// <summary>
        /// value indicating that the class was instantiated as part of a unit test
        /// </summary>
        private bool isUnitTest = false;

        /// <summary>
        /// Class that implements the IDgxComms interface.
        /// </summary>
        private IDgxComms comms;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormConfiguration"/> class.
        /// </summary>
        public FormConfiguration()
        {
            this.InitializeComponent();

            // If the setting is not set default to my documents otherwise load the setting
            if (string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.geoDatabase))
            {
                DGXSettings.Properties.Settings.Default.geoDatabase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                this.fileGdbDirectoryTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                this.fileGdbDirectoryTextBox.Text = DGXSettings.Properties.Settings.Default.geoDatabase;
            }

            //textBoxUrl.Text = Settings.Default.url;
            this.UserNameTextBox.Text = DGXSettings.Properties.Settings.Default["username"].ToString();
            var tempPassword = string.Empty;

            if (Aes.Instance.Decrypt128(DGXSettings.Properties.Settings.Default["password"].ToString(), out tempPassword))
            {
                this.PasswordTextBox.Text = tempPassword;
            }
            
            // Set the base url text box.
            this.urlTextBox.Text = string.IsNullOrEmpty(DGXSettings.Properties.Settings.Default.baseUrl) ? DGXSettings.Properties.Settings.Default.DefaultBaseUrl : DGXSettings.Properties.Settings.Default.baseUrl;

            this.comms = new DgxComms();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormConfiguration"/> class for use in unit tests.
        /// </summary>
        /// <param name="isUnitTest">
        /// The is unit test.
        /// </param>
        public FormConfiguration(bool isUnitTest)
        {
            this.isUnitTest = isUnitTest;
        }

        #region Public Methods

        /// <summary>
        /// Validate username
        /// </summary>
        /// <param name="username">
        /// The username to be validated
        /// </param>
        /// <returns>
        /// true if the username is valid
        /// </returns>
        public static bool ValidUsername(string username)
        {
            return Regex.IsMatch(username, @"^[a-zA-Z.]+$");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Event handler for when the use DEV check box is checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void CheckBoxUseDevCheckedChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event handler for when the browse button is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void BrowseButtonClick(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();

            // Open folder dialog.  If use clicks ok apply settings
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            //Settings.Default.geoDatabase = folderDialog.SelectedPath;
            this.fileGdbDirectoryTextBox.Text = folderDialog.SelectedPath;
        }

        /// <summary>
        /// Event handler for when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void CancelButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for when text in the URL text box changes.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void TextBoxUrlTextChanged(object sender, EventArgs e)
        {
            // Text was changed in one of the text boxs so revert background back to white
            // because their validity is no longer known until the user clicks the test
            // connection button.
            this.PasswordTextBox.BackColor = Color.White;
            this.UserNameTextBox.BackColor = Color.White;
            this.urlTextBox.BackColor = Color.White;
        }

        /// <summary>
        /// Event handler for when the OK button is clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            // Enforce username requirements
            if (!ValidUsername(this.UserNameTextBox.Text))
            {
                MessageBox.Show(DGXSettings.DgxResources.InvalidUsername);
                return;
            }

            DGXSettings.Properties.Settings.Default["username"] = this.UserNameTextBox.Text;
            DGXSettings.Properties.Settings.Default.geoDatabase = this.fileGdbDirectoryTextBox.Text;
            DGXSettings.Properties.Settings.Default.baseUrl = this.urlTextBox.Text;
            string temp;
            if (Aes.Instance.Encrypt128(this.PasswordTextBox.Text, out temp))
            {
                DGXSettings.Properties.Settings.Default["password"] = temp;
            }
            else
            {
                MessageBox.Show(DGXSettings.DgxResources.ErrorSavingPassword);
                return;
            }

            DGXSettings.Properties.Settings.Default.Save();
            this.Close();
        }

        /// <summary>
        /// Event handler for when the test button is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void ButtonTestClick(object sender, EventArgs e)
        {
            // Enforce username requirements
            if (!ValidUsername(this.UserNameTextBox.Text))
            {
                MessageBox.Show(DGXSettings.DgxResources.InvalidUsername);
                return;
            }

            var netObj = new NetObject
                             {
                                 AuthEndpoint = DGXSettings.Properties.Settings.Default.authenticationServer,
                                 BaseUrl = this.urlTextBox.Text,
                                 User = this.UserNameTextBox.Text,
                                 Password = this.PasswordTextBox.Text
                             };

            var result = this.comms.AuthenticateNetworkObject(ref netObj);

            if (result && netObj.ResponseStatusCode == HttpStatusCode.OK)
            {
                // Test was successful inform the user
                this.PasswordTextBox.BackColor = Color.GreenYellow;
                this.UserNameTextBox.BackColor = Color.GreenYellow;
                this.urlTextBox.BackColor = Color.GreenYellow;
                MessageBox.Show(DGXSettings.DgxResources.SuccessfulConnection);
                return;
            }

            if (netObj.ResponseStatusCode == HttpStatusCode.Unauthorized)
            {
                MessageBox.Show(DGXSettings.DgxResources.InvalidUserPass);
                this.PasswordTextBox.BackColor = Color.Tomato;
                this.UserNameTextBox.BackColor = Color.Tomato;
                return;
            }

            MessageBox.Show(DGXSettings.DgxResources.InvalidUrl);
            this.urlTextBox.BackColor = Color.Tomato;
            this.PasswordTextBox.BackColor = Color.White;
            this.UserNameTextBox.BackColor = Color.White;
        }
        #endregion
    }
}
