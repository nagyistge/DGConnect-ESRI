// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdRelay.cs" company="DigitalGlobe">
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

namespace Gbdx.Gbd
{
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// The AOI drawn.
    /// </summary>
    /// <param name="poly">
    /// The poly.
    /// </param>
    /// <param name="elm">
    /// The elm.
    /// </param>
    public delegate void AoiDrawn(IPolygon poly, IElement elm);

    /// <summary>
    /// The GBD relay.
    /// </summary>
    public class GbdRelay
    {
        /// <summary>
        /// The instance.
        /// </summary>
        private static GbdRelay instance;

        /// <summary>
        /// The local polygon.
        /// </summary>
        private IPolygon localPolygon;

        /// <summary>
        /// The local element.
        /// </summary>
        private IElement localElement;

        /// <summary>
        /// Prevents a default instance of the <see cref="GbdRelay"/> class from being created.
        /// </summary>
        private GbdRelay()
        {
        }

        /// <summary>
        /// The AOI has been drawn.
        /// </summary>
        public event AoiDrawn AoiHasBeenDrawn;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GbdRelay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GbdRelay();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the polygon.
        /// </summary>
        public IPolygon Polygon
        {
            get
            {
                return this.localPolygon;
            }

            set
            {
                this.localPolygon = value;
            }
        }

        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        public IElement Element
        {
            get
            {
                return this.localElement;
            }

            set
            {
                this.localElement = value;
            }
        }

        /// <summary>
        /// The set polygon and element.
        /// </summary>
        /// <param name="polygon">
        /// The polygon.
        /// </param>
        /// <param name="elm">
        /// The elm.
        /// </param>
        public void SetPolygonAndElement(IPolygon polygon, IElement elm)
        {
            this.localPolygon = polygon;
            this.localElement = elm;

            if (this.AoiHasBeenDrawn != null)
            {
                this.AoiHasBeenDrawn(this.localPolygon, this.localElement);
            }
        }
    }
}