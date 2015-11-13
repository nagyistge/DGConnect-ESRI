// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RBList.cs" company="DigitalGlobe">
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

namespace Dgx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// The radio button list.
    /// </summary>
    public class RbList
    {
        /// <summary>
        /// The list of radio buttons.
        /// </summary>
        private List<RadioButton> list = new List<RadioButton>();

        /// <summary>
        /// The selected text.
        /// </summary>
        private string selectedText = string.Empty;

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        public string SelectedText
        {
            get
            {
                return this.selectedText;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Add radio button to the list.
        /// </summary>
        /// <param name="button">
        /// The button.
        /// </param>
        public void Add(RadioButton button)
        {
            this.list.Add(button);
            button.CheckedChanged += this.ButtonCheckedChanged;
        }

        /// <summary>
        /// Remove radio button from the list.
        /// </summary>
        /// <param name="button">
        /// The button.
        /// </param>
        public void Remove(RadioButton button)
        {
            this.list.Remove(button);
        }

        /// <summary>
        /// Restore which radio button was selected.
        /// </summary>
        /// <param name="selectedString">
        /// The selected string.
        /// </param>
        public void RestoreSelection(string selectedString)
        {
            var query = from i in this.list where i.Text == selectedString select i ;
            foreach (var button in query)
            {
                this.selectedText = button.Text;
                button.Checked = true;
            }
        }

        /// <summary>
        /// The button_ checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonCheckedChanged(object sender, EventArgs e)
        {
            var button = (RadioButton)sender;

            if (button.Checked)
            {
                this.selectedText = button.Text;
            }
        }
    }
}
