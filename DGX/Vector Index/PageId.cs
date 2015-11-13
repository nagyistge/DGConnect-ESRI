// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PageId.cs" company="DigitalGlobe">
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
//   Defines the PageId type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx.Vector_Index
{
    using Newtonsoft.Json;

    /// <summary>
    /// The page id class.  Currently only used on getting the first page id.  Subsequent page id's are contained within a header.
    /// </summary>
    public class PageId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageId"/> class.
        /// </summary>
        /// <param name="itemCount">
        /// The item count.
        /// </param>
        public PageId(int itemCount)
        {
            this.ItemCount = itemCount;
        }

        /// <summary>
        /// Gets or sets the paging id.
        /// </summary>
        [JsonProperty(PropertyName = "pagingId")]
        public string PagingId { get; set; }

        /// <summary>
        /// Gets or sets the item count.
        /// </summary>
        [JsonProperty(PropertyName = "itemCount")]
        public int ItemCount { get; set; }
    }
}