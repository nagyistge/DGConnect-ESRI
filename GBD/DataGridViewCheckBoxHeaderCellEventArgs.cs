// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGridViewCheckBoxHeaderCellEventArgs.cs" company="DigitalGlobe">
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
//   Defines the DataGridViewCheckBoxHeaderCellEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System;

    /// <summary>
    /// The data grid view check box header cell event args.
    /// </summary>
    public class DataGridViewCheckBoxHeaderCellEventArgs : EventArgs
    {
        /// <summary>
        /// The value determining if it's checked.
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridViewCheckBoxHeaderCellEventArgs"/> class.
        /// </summary>
        /// <param name="isChecked">
        /// The is checked.
        /// </param>
        public DataGridViewCheckBoxHeaderCellEventArgs(bool isChecked)
        {
            this.isChecked = isChecked;
        }

        /// <summary>
        /// Gets a value indicating whether checked.
        /// </summary>
        public bool Checked
        {
            get { return this.isChecked; }
        }
    }
}