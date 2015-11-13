// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoundingBox.cs" company="DigitalGlobe">
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
//   Defines the BoundingBox type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx.Vector_Index
{
    /// <summary>
    /// The bounding box.
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// Gets or sets the x minimum.
        /// </summary>
        public double Xmin { get; set; }

        /// <summary>
        /// Gets or sets the y minimum.
        /// </summary>
        public double Ymin { get; set; }

        /// <summary>
        /// Gets or sets the x maximum.
        /// </summary>
        public double Xmax { get; set; }

        /// <summary>
        /// Gets or sets the y maximum.
        /// </summary>
        public double Ymax { get; set; }
    }
}