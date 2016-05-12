/*
* Class was taken from CodeProject.com
* http://www.codeproject.com/Articles/31829/Disabled-Checkbox-Column-in-the-DataGridView
* 
* Licensed Under The Code Project Open License (CPOL)
*/

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GbdxTools
{
    public class DataGridViewDisableCheckBoxColumn : DataGridViewCheckBoxColumn
    {
        public DataGridViewDisableCheckBoxColumn()
        {
            this.CellTemplate = new DataGridViewDisableCheckBoxCell();
        }
    }

    public class DataGridViewDisableCheckBoxCell : DataGridViewCheckBoxCell
    {

        private bool enabledValue;
        /// <summary>
        /// This property decides whether the checkbox should be shown checked or unchecked.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabledValue;
            }
            set
            {
                enabledValue = value;
            }
        }
        /// Override the Clone method so that the Enabled property is copied.
        public override object Clone()
        {
            DataGridViewDisableCheckBoxCell cell =
                (DataGridViewDisableCheckBoxCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
        }


        public DataGridViewDisableCheckBoxCell()
        {

        }
        /// <summary>
        /// Override the Paint method to show the disabled checked/unchecked datagridviewcheckboxcell.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds"></param>
        /// <param name="rowIndex"></param>
        /// <param name="elementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle"></param>
        /// <param name="advancedBorderStyle"></param>
        /// <param name="paintParts"></param>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (this.Enabled)
            {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
            else
            {
                SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
                graphics.FillRectangle(cellBackground, cellBounds);
                cellBackground.Dispose();
                PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                Rectangle checkBoxArea = cellBounds;
                Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
                checkBoxArea.X += buttonAdjustment.X;
                checkBoxArea.Y += buttonAdjustment.Y;

                checkBoxArea.Height -= buttonAdjustment.Height;
                checkBoxArea.Width -= buttonAdjustment.Width;
                Point drawInPoint = new Point(cellBounds.X + cellBounds.Width / 2 - 7, cellBounds.Y + cellBounds.Height / 2 - 7);

                if (this.enabledValue)
                    CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, CheckBoxState.CheckedDisabled);
                else
                    CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, CheckBoxState.UncheckedDisabled);

            }

        }

    }
}