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

namespace Gbdx
{
    using System;
    using System.Drawing;
    using System.Net;
    using System.Windows.Forms;

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
        /// Class that implements the IGbdxComms interface.
        /// </summary>
        private IGbdxComms comms;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormConfiguration"/> class.
        /// </summary>
        public FormConfiguration()
        {
            this.InitializeComponent();

            // If the setting is not set default to my documents otherwise load the setting
            if (string.IsNullOrEmpty(GbdxSettings.Properties.Settings.Default.geoDatabase))
            {
                GbdxSettings.Properties.Settings.Default.geoDatabase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                this.fileGdbDirectoryTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                this.fileGdbDirectoryTextBox.Text = GbdxSettings.Properties.Settings.Default.geoDatabase;
            }

            if (string.IsNullOrEmpty(GbdxSettings.Properties.Settings.Default.AuthBase))
            {
                this.authTextBox.Text = GbdxSettings.Properties.Settings.Default.DefaultAuthBase;
            }
            else
            {
                this.authTextBox.Text = GbdxSettings.Properties.Settings.Default.AuthBase;
            }

            //textBoxUrl.Text = Settings.Default.url;
            this.UserNameTextBox.Text = GbdxSettings.Properties.Settings.Default["username"].ToString();
            var tempPassword = string.Empty;

            if (Aes.Instance.Decrypt128(GbdxSettings.Properties.Settings.Default["password"].ToString(), out tempPassword))
            {
                this.PasswordTextBox.Text = tempPassword;
            }
            
            // Set the base url text box.
            this.urlTextBox.Text = string.IsNullOrEmpty(GbdxSettings.Properties.Settings.Default.baseUrl) ? GbdxSettings.Properties.Settings.Default.DefaultBaseUrl : GbdxSettings.Properties.Settings.Default.baseUrl;

            this.comms = new GbdxComms();
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
            GbdxSettings.Properties.Settings.Default["username"] = this.UserNameTextBox.Text;
            GbdxSettings.Properties.Settings.Default.geoDatabase = this.fileGdbDirectoryTextBox.Text;
            GbdxSettings.Properties.Settings.Default.baseUrl = this.urlTextBox.Text;
            GbdxSettings.Properties.Settings.Default.AuthBase = this.authTextBox.Text;
            string temp;
            if (Aes.Instance.Encrypt128(this.PasswordTextBox.Text, out temp))
            {
                GbdxSettings.Properties.Settings.Default["password"] = temp;
            }
            else
            {
                MessageBox.Show(GbdxSettings.GbdxResources.ErrorSavingPassword);
                return;
            }

            GbdxSettings.Properties.Settings.Default.Save();
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
            var netObj = new NetObject
                             {
                                 AuthEndpoint = GbdxSettings.Properties.Settings.Default.authenticationServer,
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
                MessageBox.Show(GbdxSettings.GbdxResources.SuccessfulConnection);
                return;
            }

            if (netObj.ResponseStatusCode == HttpStatusCode.Unauthorized)
            {
                MessageBox.Show(GbdxSettings.GbdxResources.InvalidUserPass);
                this.PasswordTextBox.BackColor = Color.Tomato;
                this.UserNameTextBox.BackColor = Color.Tomato;
                return;
            }

            MessageBox.Show(GbdxSettings.GbdxResources.InvalidUrl);
            this.urlTextBox.BackColor = Color.Tomato;
            this.PasswordTextBox.BackColor = Color.White;
            this.UserNameTextBox.BackColor = Color.White;
        }
        #endregion
    }
}
