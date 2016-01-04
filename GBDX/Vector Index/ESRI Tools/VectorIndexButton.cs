// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndexButton.cs" company="DigitalGlobe">
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

namespace Gbdx.Vector_Index.ESRI_Tools
{
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;

    using Button = ESRI.ArcGIS.Desktop.AddIns.Button;

    /// <summary>
    /// The vector index button.
    /// </summary>
    public class VectorIndexButton : Button
    {
        /// <summary>
        /// The on click method for the button are the arcmap toolbar.
        /// </summary>
        protected override void OnClick()
        {
            UID theUid = new UIDClass();
            theUid.Value = ThisAddIn.IDs.Gbdx_Vector_Index_Forms_VectorIndexDockable;

            IDockableWindow vectorIndexDockableWindow = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
            vectorIndexDockableWindow.Show(!vectorIndexDockableWindow.IsVisible());
        }

        /// <summary>
        /// The on update.
        /// </summary>
        protected override void OnUpdate()
        {
        }
    }
}
