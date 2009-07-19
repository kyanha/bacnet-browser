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

        BACnetmanager bnm = new BACnetmanager(0xBAC0, BACnetEnums.BACNET_MODE.BACnetClient, BACnetEnums.CLIENT_DEVICE_ID);

        public MainForm()
        {
            InitializeComponent();
        }



        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.ExpandAll();
        }

        private void CollapseAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.CollapseAll();
        }


        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Sending Who_is");
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs( bnm );
        }



        List<Device> ourdevices = new List<Device>();
        List<BACnetNetwork> ourNetworks = new List<BACnetNetwork>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Check to see if there is a new "I-Am" message to process.

            while ( bnm.NewDeviceQueue.Count != 0)
            {

                Device D = bnm.NewDeviceQueue.Dequeue();

                BACnetNetwork N = new BACnetNetwork() ;
                
                N.NetworkNumber = D.NetworkNumber;

                // is this a new BACnet Network?

                if (!ourNetworks.Contains(N))
                {
                    // New network, let's add it

                    TreeNode NewNode = new TreeNode();

                    NewNode.Name = "NewNode";

                    NewNode.Text = "Network " + N.NetworkNumber ;
                    NewNode.Tag = N.NetworkNumber;

                    this.treeView2.Nodes.Add(NewNode);
                    this.treeView2.SelectedNode = NewNode;

                    ourNetworks.Add(N);

                }


                // find the network and add device to it

                for (int i = 0; i < this.treeView2.Nodes.Count; i++)
                {
                    TreeNode tni = this.treeView2.Nodes[i];

                    if (tni.Tag != null)
                    {
                        if (tni.Tag.Equals(N.NetworkNumber))
                        {
                            // found Network.. select it
                            //this.treeView2.SelectedNode = tni;


                            // check if node already exists
                            
                            bool founddeviceflag = false ;

                            for (int j=0; j < tni.Nodes.Count; j++)
                            {
                                TreeNode tnj = tni.Nodes[j] ;

                                if ( tnj != null && tnj.Tag.Equals(D.DeviceId) )
                                {
                                    // found, so quit
                                    founddeviceflag = true; 
                                    break;
                                }
                            }

                            // add a device node to the network node

                            if ( founddeviceflag == false )
                            {
                                TreeNode NewNode = new TreeNode();

                                NewNode.Name = "NewNode";

                                NewNode.Text = "Device " + D.DeviceId;
                                NewNode.Tag = D.DeviceId;

                                // add other paramters to our new node

                                NewNode.Nodes.Add("Vendor ID     " + D.VendorId);

                                NewNode.Nodes.Add("Network Number " + D.NetworkNumber);
                                NewNode.Nodes.Add("Source Address " + D.SourceAddress);
                                NewNode.Nodes.Add("Segmentation   " + (int)D.SegmentationSupported);
                                NewNode.Nodes.Add("IP Address     " + D.packet.Source_Address);
                                NewNode.Nodes.Add("Port           " + D.packet.Source_Port);

                                tni.Nodes.Add(NewNode);
                                
                                //NewNode.Expand();
                            }


                        }
                    }

                }

            }

        }


        private void Quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void whoisrouterbtn_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

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

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

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

        private void mainform_closing(object sender, FormClosingEventArgs e)
        {
            bnm.BAClistener_thread.Abort();
            //BAC1listener_thread.Abort();

            // cancel our outstanding socket receives
            try
            {
                bnm.BAClistener_object.BACnetListenerClose();
                //BAC1listener_object.BetaClose();
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe);
            }

        }

    }
}
