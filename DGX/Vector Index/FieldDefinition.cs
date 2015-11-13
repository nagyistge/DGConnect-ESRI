// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldDefinition.cs" company="DigitalGlobe">
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
//   Defines the FieldDefinition type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx.Vector_Index
{
    using Newtonsoft.Json;

    /// <summary>
    /// The field definition.
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        [JsonProperty(PropertyName = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        [JsonProperty(PropertyName = "length")]
        public int Length { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            var output = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return output;
        }
    }
}