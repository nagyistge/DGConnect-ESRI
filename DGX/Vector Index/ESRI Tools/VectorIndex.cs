// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndex.cs" company="DigitalGlobe">
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

namespace Dgx.Vector_Index
{
    using System.Windows.Forms;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Desktop.AddIns;

    /// <summary>
    /// The vector index.
    /// </summary>
    public class VectorIndex : Tool
    {
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
        /// The MouseEventArgs.
        /// </param>
        protected override void OnMouseDown(MouseEventArgs arg)
        {
            if (arg.Button != MouseButtons.Left)
            {
                return;
            }

            IElement elm;
            var poly = VectorIndexHelper.DrawRectangle(out elm);

            // This line sets the polygon and element in the VectorIndexRelay class.  When this function gets called it fires an event which the
            // the vector index dockable form is subscribed too.  This acts as a way to get the data from this tool to the dockable form.
            VectorIndexRelay.Instance.SetPolygonAndElement(poly, elm);
        }
    }
}
