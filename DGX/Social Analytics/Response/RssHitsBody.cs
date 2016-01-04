// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RssHitsBody.cs" company="DigitalGlobe">
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
//   The rss hits body.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using Newtonsoft.Json;

    /// <summary>
    /// The rss hits body.
    /// </summary>
    public class RssHitsBody
    {
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the max_score.
        /// </summary>
        [JsonProperty(PropertyName = "max_score")]
        public double MaxScore { get; set; }

        /// <summary>
        /// Gets or sets the hits.
        /// </summary>
        [JsonProperty(PropertyName = "hits")]
        public RssHits[] Hits { get; set; }
    }
}