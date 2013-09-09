using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BACnetTestClient
{
    public partial class WarningBox : Form
    {
        public WarningBox()
        {
            InitializeComponent();

                textBox1.Text = "This is a TEST Tool. It WILL read and WRITE to any and all devices found on any connected " +
                "BACnet Internetwork. ONLY use this tool if you are NOT attached to an operational BACnet Internetwork!";

                textBox2.Text = "It will report error and other statistics to a central location " +
                "for diagnostic purposes. If this is unacceptable, please do not use this application." ;

                textBox3.Text = "The software is provided \"AS IS\", without warranty of any kind, express or implied, including but not " +
                                "limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. " +
                                "In no event shall the authors or copyright holders be liable for any claim, damages or other liability, " +
                                "whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software " +
                                "or the use or other dealings in the software.";
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           BACnetInteropApp.Properties.Settings.Default.LegalAcknowledged = checkBox1.Checked;
           BACnetInteropApp.Properties.Settings.Default.Save();
        }

        private void WarningBox_Load(object sender, EventArgs e)
        {
            ButtonOK.Focus();
        }
    }
}
