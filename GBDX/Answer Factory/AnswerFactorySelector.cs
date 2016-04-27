using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gbdx.Answer_Factory
{
    using System.Windows.Forms;

    using AnswerFactory;

    using ESRI.ArcGIS.Carto;
    
    using Gbdx.Vector_Index;

    public class AnswerFactorySelector : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AnswerFactorySelector()
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
            AnswerFactoryRelay.Instance.SetPolygonAndElement(poly,elm);
        }
    }

}
