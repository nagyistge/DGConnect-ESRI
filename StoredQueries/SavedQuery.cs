// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedQuery.cs" company="DigitalGlobe">
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
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StoredQueries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Newtonsoft.Json;

    /// <summary>
    /// The query class. 
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public class SavedQuery
    {
        /// <summary>
        /// The start.
        /// </summary>
        private DateTime start;

        /// <summary>
        /// The end.
        /// </summary>
        private DateTime end;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedQuery"/> class.
        /// </summary>
        public SavedQuery()
        {
            this.timeline = new Timeline();
            this.map = new Map();
            this.polygon = new List<List<Coordinate>>();
            this.properties = new Properties();
        }

        // Json fields

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the query value.
        /// </summary>
        public string query { get; set; }

        /// <summary>
        /// Gets or sets the timeline.
        /// </summary>
        public Timeline timeline { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// Gets or sets the zoom.
        /// </summary>
        public int zoom { get; set; }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        public Map map { get; set; }

        /// <summary>
        /// Gets or sets the polygon.
        /// </summary>
        public List<List<Coordinate>> polygon { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public Properties properties { get; set; }

        // Ignored Json Fields

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        [JsonIgnore]
        public DateTime Start
        {
            get
            {
                return this.start;
            }

            set
            {
                this.start = value;

                // If this value gets modified then lets pass the modification on to the value that will atcually be sent to stored
                // query
                this.timeline.Start = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            }
        }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        [JsonIgnore]
        public DateTime End
        {
            get
            {
                return this.end;
            }

            set
            {
                this.end = value;

                // If this value gets modified then lets pass the modification on to the value that will atcually be sent to stored
                // query
                this.timeline.End = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            }
        }

        /// <summary>
        /// The set date time.
        /// </summary>
        public void SetDateTime()
        {
            this.start = DateTime.ParseExact(
                this.timeline.Start,
                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                null,
                DateTimeStyles.None);
            this.end = DateTime.ParseExact(this.timeline.End, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", null, DateTimeStyles.None);
        }
    }
    }
