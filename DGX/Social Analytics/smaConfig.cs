// --------------------------------------------------------------------------------------------------------------------
// <copyright file="smaConfig.cs" company="DigitalGlobe">
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

namespace Gbdx
{
    using System.Windows.Forms;

    using Gbdx;

    using Button = ESRI.ArcGIS.Desktop.AddIns.Button;

    /// <summary>
    /// The sma config.
    /// </summary>
    public class SmaConfig : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmaConfig"/> class.
        /// </summary>
        public SmaConfig()
        {
        }

        /// <summary>
        /// The on click.
        /// </summary>
        protected override void OnClick()
        {
            FormConfiguration fc = new FormConfiguration();
            if (fc.ShowDialog() == DialogResult.OK)
            {
            }
        }

        /// <summary>
        /// The on update.
        /// </summary>
        protected override void OnUpdate()
        {
            this.Enabled = ArcMap.Application != null;
        }
    }
}
