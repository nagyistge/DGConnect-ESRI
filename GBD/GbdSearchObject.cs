// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdSearchObject.cs" company="DigitalGlobe">
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
//   Defines the GbdSearchObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The GBD search object.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public class GbdSearchObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GbdSearchObject"/> class.
        /// </summary>
        public GbdSearchObject()
        {
            this.searchAreaWkt = string.Empty;
            this.startDate = string.Empty;
            this.endDate = string.Empty;
            this.filters = new List<string>();
            this.tagResults = false;
            this.types = new List<string>();
        }

        /// <summary>
        /// Gets or sets the search area WKT (well known text).
        /// </summary>
        public string searchAreaWkt { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public string startDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public string endDate { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        public List<string> filters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tag results.
        /// </summary>
        public bool tagResults { get; set; }

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        public List<string> types { get; set; }
    }
}