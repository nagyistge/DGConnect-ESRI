// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutBox.cs" company="DigitalGlobe">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// The about box.
    /// </summary>
    public partial class AboutBox : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBox"/> class.
        /// </summary>
        public AboutBox()
        {
            this.InitializeComponent();
            this.versionLabel.Text = "Version 0.2 Beta\nBuild Version: " + this.ProductVersion;
        }

        /// <summary>
        /// The ok button_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OkButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
