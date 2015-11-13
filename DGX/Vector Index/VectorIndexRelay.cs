// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndexRelay.cs" company="DigitalGlobe">
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
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// Polygon setup complete delegate
    /// </summary>
    /// <param name="poly">
    /// The polygon drawn on the map
    /// </param>
    /// <param name="elm">
    /// The IElement of the bounding box drawn on the map.
    /// </param>
    public delegate void PolygonSetupComplete(IPolygon poly, IElement elm);
    
    /// <summary>
    /// VectorIndexRelay class is meant to act as a relay between the ITool that has to be called
    /// to get the area of the polygon.  Once called the polygon will be set in the relay which will
    /// in turn fire off an event being listened for in the form.
    /// </summary>
    public class VectorIndexRelay
    {
        /// <summary>
        /// The current instance of the VectorIndexRelay
        /// </summary>
        private static VectorIndexRelay instance;

        /// <summary>
        /// The polygon of the bounding box
        /// </summary>
        private IPolygon poly;

        /// <summary>
        /// The rectangle IElement of the bounding box
        /// </summary>
        private IElement rectElement;

        /// <summary>
        /// Prevents a default instance of the <see cref="VectorIndexRelay"/> class from being created.
        /// </summary>
        private VectorIndexRelay()
        {
        }

        /// <summary>
        /// Event that the polygon has been setup
        /// </summary>
        public event PolygonSetupComplete PolygonHasBeenSet;

        /// <summary>
        /// Gets the instance of the VectorIndexRelay class.
        /// </summary>
        public static VectorIndexRelay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VectorIndexRelay();
                }

                return instance;
            }
        }

        /// <summary>
        /// Sets the Polygon.
        /// </summary>
        public IPolygon Polygon
        {
            set
            {
                this.poly = value;
            }
        }

        /// <summary>
        /// Set the polygon and element of the drawn bounding box
        /// </summary>
        /// <param name="polygon">
        /// The IPolygon of the bounding box
        /// </param>
        /// <param name="elm">
        /// The IElement of the bounding box.
        /// </param>
        public void SetPolygonAndElement(IPolygon polygon, IElement elm)
        {
            this.poly = polygon;
            this.rectElement = elm;

            if (this.PolygonHasBeenSet != null)
            {
                this.PolygonHasBeenSet(this.poly, this.rectElement);
            }
        }
    }
}
