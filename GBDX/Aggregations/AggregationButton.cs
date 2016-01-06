// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregationButton.cs" company="DigitalGlobe">
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

namespace Gbdx.Aggregations
{
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geometry;

    using global::Aggregations;

    using Button = ESRI.ArcGIS.Desktop.AddIns.Button;

    /// <summary>
    /// The aggregation button.
    /// </summary>
    public class AggregationButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationButton"/> class.
        /// </summary>
        public AggregationButton()
        {
        }

        /// <summary>
        /// The on click.
        /// </summary>
        protected override void OnClick()
        {
            if (ArcMap.Application.CurrentTool.Name != ThisAddIn.IDs.Gbdx_Aggregations_AggregationSelector)
            {
                UID theUid = new UIDClass();
                theUid.Value = ThisAddIn.IDs.Gbdx_Aggregations_AggregationWindow;
                var window = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
                window.Show(!window.IsVisible());
            }
        }

        /// <summary>
        /// The on update.
        /// </summary>
        protected override void OnUpdate()
        {
        }
    }
}
