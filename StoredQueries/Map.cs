// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Map.cs" company="DigitalGlobe">
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

namespace StoredQueries
{
    using Newtonsoft.Json;

    /// <summary>
    /// The map.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        [JsonProperty(PropertyName = "bottom")]
        public double Bottom { get; set; }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        [JsonProperty(PropertyName = "left")]
        public double Left { get; set; }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        [JsonProperty(PropertyName = "right")]
        public double Right { get; set; }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        [JsonProperty(PropertyName = "top")]
        public double Top { get; set; }
    }
}
