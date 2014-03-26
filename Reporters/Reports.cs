using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Reporters
{
    public static class Reports
    {

        public enum MESSAGE_TYPE
        {
            Notification = 1,
            Panic,
            Startup
        }
        public static string version
        {
            get
            {
                    string[] appv = Application.ProductVersion.Split(new char[] { '.' });
                    return "v" + appv[0] + "." + appv[1] + "." + appv[2];
            }
        }

        public static void SendDebugString(MESSAGE_TYPE mt, string msg)
        {
            // ask permission to report this message.

            var rc = MessageBox.Show("Please help improve this program and report this error. The information to be sent is this: " + Environment.NewLine + Environment.NewLine + msg, "An error has been detected, will you supply permission to report?", MessageBoxButtons.YesNo);

            if (rc != DialogResult.Yes) return;

            UdpClient newsock = new UdpClient();
            byte[] data = new byte[10000];

            Encoding.ASCII.GetBytes("Eddie", 0, 5, data, 0);

            // encode the message
            data[5] = (byte)mt;

            string mdetails = Application.ProductName + " " + version + " ";

            int length = Encoding.ASCII.GetBytes(mdetails + msg, 0, mdetails.Length + msg.Length, data, 6);
            data[6 + length] = 0;

            try
            {
                // ekhtodo - sometimes these messages get long.... use TCP?
                newsock.Send(data, Math.Min(1200, length + 7), "debug01cloudrouter.dyndns.org", 502);
            }
            catch (SocketException)
            {
                // possibly not even connected - ignore
            }
            catch
            {
                // Socket send fails if not connected to the internet...
                // todo, at least log this exception to file, ready for email.
            }
        }

    }
}
