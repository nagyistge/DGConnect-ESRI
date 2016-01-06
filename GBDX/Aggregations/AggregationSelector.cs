using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Aggregations;
using Gbdx.Vector_Index;
using ESRI.ArcGIS.Carto;

namespace Gbdx.Aggregations
{
    using Gbdx.Vector_Index;

    public class AggregationSelector : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AggregationSelector()
        {
        }

        protected override void OnUpdate()
        {

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
            AggregationRelay.Instance.SetPolygonAndElement(poly, elm);
        }
    }

}
