using System.Windows.Forms;

namespace BaseLibrary
{
    public class UserManagement
    {
        public static void UpdateTitle( Form uForm )
        {
                uForm.Text = string.Format("{2}, Version {0} - Licensed to: {1}", BaseLibrary.BaseLib.version, "SourceForge Demo", Application.ProductName);
        }
    }
}
