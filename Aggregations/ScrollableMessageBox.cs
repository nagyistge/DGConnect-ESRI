using System.Windows.Forms;

namespace Aggregations
{
    using ESRI.ArcGIS.Carto;

    public partial class ScrollableMessageBox : Form
    {
        public ScrollableMessageBox()
        {
            this.InitializeComponent();
        }

        public DialogResult Show(string message)
        {
            this.messageBox.Text = message;
            return this.ShowDialog();
        }
    }
}
