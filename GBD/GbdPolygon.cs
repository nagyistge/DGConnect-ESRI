// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdPolygon.cs" company="DigitalGlobe">
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
//   Defines the GbdPolygon type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System.Collections.Generic;

    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// The GBD polygon.
    /// </summary>
    public class GbdPolygon
    {
        /// <summary>
        /// The local points list.
        /// </summary>
        private List<IPoint> localPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbdPolygon"/> class.
        /// </summary>
        public GbdPolygon()
        {
            this.localPoints = new List<IPoint>();
        }

        /// <summary>
        /// Add point to the private localPoints list.
        /// </summary>
        /// <param name="pointToBeAdded">
        /// The point to be added.
        /// </param>
        public void AddPoint(IPoint pointToBeAdded)
        {
            this.localPoints.Add(pointToBeAdded);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            var output = "POLYGON((";

            for (var i = 0; i <= this.localPoints.Count - 1; i++)
            {
                output += this.localPoints[i].X + " " + this.localPoints[i].Y +", ";
            }

            if(this.localPoints.Count > 1)
            {
                output += this.localPoints[0].X+" "+this.localPoints[0].Y + "))";
            }

            return output;
        }
    }
}