// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseRss.cs" company="DigitalGlobe">
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
//   The response rss.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// The response rss.
    /// </summary>
    public class ResponseRss
    {
        /// <summary>
        /// Gets or sets the took.
        /// </summary>
        [JsonProperty(PropertyName = "took")]
        public int Took { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether timed_out.
        /// </summary>
        [JsonProperty(PropertyName = "timed_out")]
        public bool TimedOut { get; set; }

        /// <summary>
        /// Gets or sets the _shards.
        /// </summary>
        [JsonProperty(PropertyName = "_shards")]
        public Dictionary<string, object> Shards { get; set; }

        /// <summary>
        /// Gets or sets the hits.
        /// </summary>
        [JsonProperty(PropertyName = "hits")]
        public RssHitsBody Hits { get; set; }
    }
}