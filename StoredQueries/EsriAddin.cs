// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EsriAddin.cs" company="DigitalGlobe">
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
    /// <summary>
    /// The esri addin.
    /// </summary>
    public class EsriAddin
    {
        /// <summary>
        /// Gets or sets the query limit.
        /// </summary>
        public string QueryLimit { get; set; }

        /// <summary>
        /// Gets or sets the date limit.
        /// </summary>
        public string DateLimit { get; set; }
    }
}
