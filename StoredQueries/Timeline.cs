// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Timeline.cs" company="DigitalGlobe">
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
//   The timeline.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StoredQueries
{
    using Newtonsoft.Json;

    /// <summary>
    /// The timeline.
    /// </summary>
    public class Timeline
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        [JsonProperty(PropertyName = "start")]
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        [JsonProperty(PropertyName = "end")]
        public string End { get; set; }
    }
}