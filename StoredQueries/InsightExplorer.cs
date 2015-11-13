// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsightExplorer.cs" company="DigitalGlobe">
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
    /// The insight explorer.
    /// </summary>
    public class InsightExplorer
    {
        /// <summary>
        /// Gets or sets a value indicating whether exact geo.
        /// </summary>
        [JsonProperty(PropertyName = "exactGeo")]
        public bool ExactGeo { get; set; }
    }
}
