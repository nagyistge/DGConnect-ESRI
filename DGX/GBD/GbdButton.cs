// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdButton.cs" company="DigitalGlobe">
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

namespace Gbdx.Gbd
{
    using ESRI.ArcGIS.Desktop.AddIns;
    using ESRI.ArcGIS.esriSystem;

    /// <summary>
    /// The GBD button.
    /// </summary>
    public class GbdButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GbdButton"/> class.
        /// </summary>
        public GbdButton()
        {
        }

        /// <summary>
        /// The on click.
        /// </summary>
        protected override void OnClick()
        {
            if (ArcMap.Application.CurrentTool.Name == ThisAddIn.IDs.Gbdx_Gbd_Selector)
            {
                return;
            }

            UID theUid = new UIDClass();
            theUid.Value = ThisAddIn.IDs.Gbdx_Gbd_GbdDockableWindow;

            var window = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
            window.Show(!window.IsVisible());
        }

        /// <summary>
        /// The on update.
        /// </summary>
        protected override void OnUpdate()
        {
        }
    }
}
