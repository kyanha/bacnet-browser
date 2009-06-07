using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Threading;

using BACnetLibraryNS;

namespace BACnetInteropApp
{
    public partial class MainForm : Form
    {

        Alpha BAC0listener_object;
        Thread BAC0listener_thread;

        Alpha BAC1listener_object;
        Thread BAC1listener_thread;


        public static Base Controlbase = new Base();


        public MainForm()
        {
            InitializeComponent();


            // fire up a thread to watch for incoming packets

            BAC0listener_object = new Alpha() { BACnet_port = 0xbac0 };
            BAC1listener_object = new Alpha() { BACnet_port = 0xbac1 };

            BAC0listener_thread = new Thread(new ThreadStart(BAC0listener_object.Beta));
            BAC0listener_thread.Start();

            BAC1listener_thread = new Thread(new ThreadStart(BAC1listener_object.Beta));
            BAC1listener_thread.Start();

        }



        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.ExpandAll();
        }

        private void CollapseAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.CollapseAll();
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //}


        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }


        //private void button5_Click(object sender, EventArgs e)
        //{
        //}

        static int snode = 0;

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    this.treeView2.SelectedNode = this.treeView2.Nodes[snode++];
        //    this.treeView2.SelectedNode.ExpandAll();
        //}

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Sending Who_is");
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs();
        }



        List<Device> ourdevices = new List<Device>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Check to see if there is a new "I-Am" message to process.

            while (Controlbase.NewDeviceQueue.Count != 0)
            {

                Device D = Controlbase.NewDeviceQueue.Dequeue();

                // is this a new Device?

                if (!ourdevices.Contains(D))
                {
                    // is new

                    TreeNode NewNode = new TreeNode();

                    NewNode.Name = "NewNode";

                    NewNode.Text = "Device " + D.DeviceId;
                    NewNode.Tag = D.DeviceId;

                    this.treeView2.Nodes.Add(NewNode);
                    this.treeView2.SelectedNode = NewNode;

                    // add other paramters to our new node

                    this.treeView2.SelectedNode.Nodes.Add("Vendor ID     " + D.VendorId);
                    this.treeView2.SelectedNode.Nodes.Add("Network Number " + D.NetworkNumber);
                    this.treeView2.SelectedNode.Nodes.Add("Source Address " + D.SourceAddress);
                    this.treeView2.SelectedNode.Nodes.Add("Segmentation   " + (int) D.SegmentationSupported );
                    this.treeView2.SelectedNode.Nodes.Add("IP Address     " + D.packet.Source_Address);
                    this.treeView2.SelectedNode.Nodes.Add("Port           " + D.packet.Source_Port);
                    this.treeView2.SelectedNode.Expand();

                    ourdevices.Add(D);
                }
                else
                {
                    //if we get here, we _know_ that treeview2 contains our node

                    for (int i = 0; i < this.treeView2.Nodes.Count; i++)
                    {
                        TreeNode tni = this.treeView2.Nodes[i];

                        if (tni.Tag != null)
                        {
                            if (tni.Tag.Equals(D.DeviceId))
                            {
                                // found it.. select it
                                this.treeView2.SelectedNode = tni;

                            }
                        }

                    }
                }

            }

        }


        private void Quit_Click(object sender, EventArgs e)
        {
            BAC0listener_thread.Abort();
            BAC1listener_thread.Abort();

            //oThread.Join(2000);

            // cancel our outstanding receive call
            try
            {
                BAC0listener_object.BetaClose();
                BAC1listener_object.BetaClose();
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe);
            }

            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void whoisrouterbtn_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC1);// NOTE, NOT BAC0, so that we can run IUT bacnet server on same machine

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;

            int store_length_here = optr;

            data[optr++] = 0x00;        // Length (2 octets)
            data[optr++] = 0x0c;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Version Always 1
            data[optr++] = 0x80 | 0x20;       // NPCI - Control Information
            data[optr++] = 0xff;        // DNET Network - B'cast
            data[optr++] = 0xff;
            data[optr++] = 0x00;        // DLEN
            data[optr++] = 0xff;        // Hop count

            // No APDU (see NPCI - Control Information

            data[optr++] = 0x00;        // Who-Is-Router..


            data[store_length_here] = 0;
            data[store_length_here + 1] = (byte)optr;

            bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);




            //  sending another who-is, this time with SNET, SADR present..

            optr = 0;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            data[optr++] = 0x00;        // Length (2 octets)
            data[optr++] = 21;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Always 1
            data[optr++] = 0x28;        // Control (Destination present, Source present)
            data[optr++] = 0xff;        // DNET - Network - B'cast
            data[optr++] = 0xff;
            data[optr++] = 0x00;        // DLEN

            // source address

            data[optr++] = 0x00;        // SNET - 0x11
            data[optr++] = 0x11;

            data[optr++] = 0x06;        // SLEN = 6 (MAC Layer Address is an IP/Port combination
            data[optr++] = 192;         // Harcoding an IP address for now
            data[optr++] = 168;         // IP Addr
            data[optr++] = 0;
            data[optr++] = 3;
            data[optr++] = 0xBA;         // Port number
            data[optr++] = 0xC1;

            data[optr++] = 0xff;        // Hop count

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            data[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
            data[optr++] = 0x08;        // Unconfirmed Service Choice: Who-Is

            // todo, re-enable once server can process multiple messages
            // bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);

            bacnet_master_socket.Close();

        }

        private void Initialize_Routing_Table_Click(object sender, EventArgs e)
        {
            // Query Routing Tables (Initialize Routing tables message with no parameters)
            // http://www.bacnetwiki.com/wiki/index.php?title=Initialize-Routing-Table

            byte[] data = new byte[1024];
            int optr = 0;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC1);// NOTE, NOT BAC0, so that we can run IUT bacnet server on same machine

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;

            int store_length_here = optr;

            data[optr++] = 0x00;        // Length (2 octets)
            data[optr++] = 0x0c;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Version Always 1
            data[optr++] = 0x80 | 0x20;       // NPCI - Control Information
            data[optr++] = 0xff;        // DNET Network - B'cast
            data[optr++] = 0xff;
            data[optr++] = 0x00;        // DLEN
            data[optr++] = 0xff;        // Hop count

            data[optr++] = (byte)BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE;
            data[optr++] = 0x00;        // Number of port mappings


            data[store_length_here] = 0;
            data[store_length_here + 1] = (byte)optr;

            bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);
        }

    }
}
