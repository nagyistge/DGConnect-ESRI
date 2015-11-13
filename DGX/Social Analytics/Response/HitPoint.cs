// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HitPoint.cs" company="DigitalGlobe">
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

namespace Dgx
{
    using Newtonsoft.Json;

    /// <summary>
    /// The hit point.
    /// </summary>
    public class HitPoint
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        [JsonProperty(PropertyName = "lat")]
        public double Lat  { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        [JsonProperty(PropertyName = "lon")]
        public double Lon  { get; set; }
    }
}
