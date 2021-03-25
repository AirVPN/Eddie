using System.Windows.Forms;

namespace Eddie.Forms.Skin
{
    class TableLayoutPanel : System.Windows.Forms.TableLayoutPanel
    {
        public TableLayoutPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }
    }
}