// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Selector.cs" company="DigitalGlobe">
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

namespace Dgx.Gbd
{
    using System.Windows.Forms;

    using Dgx.Vector_Index;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Desktop.AddIns;

    /// <summary>
    /// The selector.
    /// </summary>
    public class Selector : Tool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Selector"/> class.
        /// </summary>
        public Selector()
        {
        }

        /// <summary>
        /// The on update.
        /// </summary>
        protected override void OnUpdate()
        {
            this.Enabled = ArcMap.Application != null;
        }

        /// <summary>
        /// The on mouse down.
        /// </summary>
        /// <param name="arg">
        /// The mouse event arguments.
        /// </param>
        protected override void OnMouseDown(MouseEventArgs arg)
        {
            if (arg.Button != MouseButtons.Left)
            {
                return;
            }

            IElement elm;
            var poly = VectorIndexHelper.DrawRectangle(out elm);
            GbdRelay.Instance.SetPolygonAndElement(poly, elm);
        }
    }
}
