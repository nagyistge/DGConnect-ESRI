// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomMenuItem.cs" company="DigitalGlobe">
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

namespace Dgx.Social_Analytics.Forms
{
    using System.Windows.Forms;

    /// <summary>
    /// The custom menu item.
    /// </summary>
    public class CustomMenuItem : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMenuItem"/> class.
        /// </summary>
        /// <param name="stripitem">
        /// The strip item to be added to the favorites menu.
        /// </param>
        public CustomMenuItem(FavoriteMenuStripItem stripitem)
        {
            this.FavoriteMenuStripItem = stripitem;
        }

        /// <summary>
        /// Gets or sets the favorite menu strip item.
        /// </summary>
        public FavoriteMenuStripItem FavoriteMenuStripItem { get; set; }
    }
}
