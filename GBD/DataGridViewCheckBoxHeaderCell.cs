// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGridViewCheckBoxHeaderCell.cs" company="DigitalGlobe">
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

namespace GBD
{
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    /// <summary>
    /// The check box clicked handler.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    public delegate void CheckBoxClickedHandler(bool state);

    /// <summary>
    /// The data grid view check box header cell.
    /// </summary>
    public class DataGridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        /// <summary>
        /// The check box location.
        /// </summary>
        private Point checkBoxLocation;

        /// <summary>
        /// The check box size.
        /// </summary>
        private Size checkBoxSize;

        /// <summary>
        /// The _checked.
        /// </summary>
        public bool isChecked = false;

        /// <summary>
        /// The _cell location.
        /// </summary>
        private Point cellLocation = new Point();

        /// <summary>
        /// The checkbox state
        /// </summary>
        private CheckBoxState cbState = CheckBoxState.UncheckedNormal;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridViewCheckBoxHeaderCell"/> class.
        /// </summary>
        public DataGridViewCheckBoxHeaderCell()
        {           
        }

        /// <summary>
        /// The on check box clicked.
        /// </summary>
        public event CheckBoxClickedHandler OnCheckBoxClicked;

        /// <summary>
        /// The paint.
        /// </summary>
        /// <param name="graphics">
        /// The graphics.
        /// </param>
        /// <param name="clipBounds">
        /// The clip bounds.
        /// </param>
        /// <param name="cellBounds">
        /// The cell bounds.
        /// </param>
        /// <param name="rowIndex">
        /// The row index.
        /// </param>
        /// <param name="dataGridViewElementState">
        /// The data grid view element state.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="formattedValue">
        /// The formatted value.
        /// </param>
        /// <param name="errorText">
        /// The error text.
        /// </param>
        /// <param name="cellStyle">
        /// The cell style.
        /// </param>
        /// <param name="advancedBorderStyle">
        /// The advanced border style.
        /// </param>
        /// <param name="paintParts">
        /// The paint parts.
        /// </param>
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds, 
            int rowIndex, 
            DataGridViewElementStates dataGridViewElementState, 
            object value, 
            object formattedValue, 
            string errorText, 
            DataGridViewCellStyle cellStyle, 
            DataGridViewAdvancedBorderStyle advancedBorderStyle, 
            DataGridViewPaintParts paintParts)
        {
            base.Paint(
                graphics,
                clipBounds,
                cellBounds,
                rowIndex,
                dataGridViewElementState,
                string.Empty,
                string.Empty,
                errorText,
                cellStyle,
                advancedBorderStyle,
                paintParts);

            Point p = new Point();
            Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            p.X = cellBounds.Location.X + 
                (cellBounds.Width / 2) - (s.Width / 2) ;
            p.Y = cellBounds.Location.Y + 
                (cellBounds.Height / 2) - (s.Height / 2);
            this.cellLocation = cellBounds.Location;
            this.checkBoxLocation = p;
            this.checkBoxSize = s;

            if (this.isChecked)
            {
                this.cbState = CheckBoxState.CheckedNormal;
            }
            else
            {
                this.cbState = CheckBoxState.UncheckedNormal;
            }

            CheckBoxRenderer.DrawCheckBox(graphics, this.checkBoxLocation, this.cbState);
        }

        /// <summary>
        /// The on mouse click.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            Point p = new Point(e.X + this.cellLocation.X, e.Y + this.cellLocation.Y);
            if (p.X >= this.checkBoxLocation.X && p.X <= this.checkBoxLocation.X + this.checkBoxSize.Width
            && p.Y >= this.checkBoxLocation.Y && p.Y <= this.checkBoxLocation.Y + this.checkBoxSize.Height)
            {
                this.isChecked = !this.isChecked;
                
                if (this.OnCheckBoxClicked != null)
                {
                    this.OnCheckBoxClicked(this.isChecked);
                    //this.DataGridView.InvalidateCell(this);
                }
                this.DataGridView.InvalidateCell(this);
            }

            base.OnMouseClick(e);
        }    
    }
}