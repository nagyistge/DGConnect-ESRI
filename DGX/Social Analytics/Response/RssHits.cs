// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RssHits.cs" company="DigitalGlobe">
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
//   The rss hits.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using Newtonsoft.Json;

    /// <summary>
    /// The rss hits.
    /// </summary>
    public class RssHits
    {
        /// <summary>
        /// Gets or sets the _index.
        /// </summary>
        [JsonProperty(PropertyName = "_index")]
        public string Index { get; set; }

        /// <summary>
        /// Gets or sets the _type.
        /// </summary>
        [JsonProperty(PropertyName = "_type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the _id.
        /// </summary>
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the _score.
        /// </summary>
        [JsonProperty(PropertyName = "_score")]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the _source.
        /// </summary>
        [JsonProperty(PropertyName = "_source")]
        public RssHitSource Source { get; set; }
    }
}