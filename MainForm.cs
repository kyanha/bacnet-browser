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

#if BACNET_TARGET_REMOTE
        OurSocket inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xEDD1);
#else
        OurSocket inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xBAC0);
#endif

        BACnetmanager bnm;

        IPEndPoint IPEPDestination;

        public MainForm()
        {
            InitializeComponent();

#if BACNET_TARGET_REMOTE
            IPHostEntry hostEntry = Dns.GetHostEntry("cloudrouter.dyndns.org");
            IPEPDestination = new IPEndPoint(hostEntry.AddressList[0], 0xEDD1);
#else
            IPEPDestination = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);
#endif

            bnm = new BACnetmanager(inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

        }


        private void mainform_closing(object sender, FormClosingEventArgs e)
        {

            bnm.BACnetManagerClose();

            Application.Exit();
        }


        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            this.BACnetInternetworkTreeView.ExpandAll();
        }

        private void CollapseAllButton_Click(object sender, EventArgs e)
        {
            this.BACnetInternetworkTreeView.CollapseAll();
        }


        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Sending Who_is");
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, true, IPEPDestination);
        }



        List<Device> ourdevices = new List<Device>();
        List<BACnetNetwork> ourNetworks = new List<BACnetNetwork>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Check to see if there is a new "I-Am" message to process.

            while (bnm.NewDeviceQueue.Count != 0)
            {

                Device D = bnm.NewDeviceQueue.Dequeue();

                BACnetNetwork N = new BACnetNetwork();

                N.NetworkNumber = D.adr.networkNumber;

                // is this a new BACnet Network?

                if (!ourNetworks.Contains(N))
                {
                    // New network, let's add it

                    myTreeNode NewNode = new myTreeNode();

                    NewNode.Name = "NewNode";

                    NewNode.Text = "Network " + N.NetworkNumber;
                    NewNode.Tag = N.NetworkNumber;

                    this.BACnetInternetworkTreeView.Nodes.Add(NewNode);
                    this.BACnetInternetworkTreeView.SelectedNode = NewNode;

                    ourNetworks.Add(N);

                }


                // find the network and add device to it

                for (int i = 0; i < this.BACnetInternetworkTreeView.Nodes.Count; i++)
                {
                    myTreeNode tni = (myTreeNode)this.BACnetInternetworkTreeView.Nodes[i];

                    if (tni.Tag != null)
                    {
                        if (tni.Tag.Equals(N.NetworkNumber))
                        {
                            // found Network..
                            // check if node already exists

                            bool founddeviceflag = false;

                            for (int j = 0; j < tni.Nodes.Count; j++)
                            {
                                myTreeNode tnj = (myTreeNode)tni.Nodes[j];

                                if (tnj != null && tnj.device.Equals(D))
                                {
                                    // found, so quit
                                    founddeviceflag = true;
                                    break;
                                }
                            }

                            // add a device node to the network node

                            if (founddeviceflag == false)
                            {
                                myTreeNode NewNode = new myTreeNode();

                                NewNode.device = D;

                                NewNode.Name = "NewNode";

                                NewNode.Text = "Device  " + D.deviceID.objectInstance ;
                                NewNode.Tag = D.deviceID.objectInstance ;

                                // add other paramters to our new node

                                // todo, need to fix this, since we are adding TreeNodes here, not myTreeNodes...

                                NewNode.Nodes.Add("Vendor ID      " + D.VendorId);

                                NewNode.Nodes.Add("Network Number " + D.adr.networkNumber);
                                NewNode.Nodes.Add("MAC Address    " + D.adr.MACaddress);
                                NewNode.Nodes.Add("Segmentation   " + (int)D.SegmentationSupported);
                                NewNode.Nodes.Add("IP Address     " + D.packet.FromAddress.Address);
                                NewNode.Nodes.Add("Port           " + D.packet.FromAddress.Port);

                                NewNode.isDevice = true;

                                tni.Nodes.Add(NewNode);

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

        private void button5_Click(object sender, EventArgs e)
        {
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, false, IPEPDestination);

        }

        private void SendWhoIs_Tick(object sender, EventArgs e)
        {
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, true, IPEPDestination);
        }

        private void buttonSendReadProperty_Click(object sender, EventArgs e)
        {
            // todo- if the IP address does not exist, but is within our subnet, then there is no transmission, but neither is there
            // an exception - simply nothing appears in Wireshark. Investigate and go proactive warning the user...\

            // set up temporary device and packet information for the purposes of testing this function call
            Device tempdevice = new Device();
            tempdevice.packet = new Packet();
            tempdevice.adr = new ADR( 77, 1, 4 );

            tempdevice.packet.FromAddress = new IPEndPoint(IPAddress.Parse("192.168.0.68"), 0xBAC0);

            BACnetLibraryCL.ReadProperties(bnm, tempdevice );
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timerHeartbeatWhoIs.Enabled = true;
                BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, true, IPEPDestination);
            }
            else
            {
                timerHeartbeatWhoIs.Enabled = false;
            }
        }

        private void mycontextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        Device menuDevice;

        private void BACnetInternetworkTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // select the node, just to highlight the last item right clicked on
                BACnetInternetworkTreeView.SelectedNode = e.Node;

                if (((myTreeNode)e.Node).isDevice == true)
                {
                    // store the IP address, dest network, etc for the next action - todo, there must be a better way to do this; create a new class??

                    menuDevice = ((myTreeNode)e.Node).device;

                    mycontextMenuStrip.Show(BACnetInternetworkTreeView, e.Location);
                }
                else
                {
                    MessageBox.Show("You must right-click on a Device for the context menu");
                }

            }
        }

        private void mycontextMenuStrip_Opening(object sender, CancelEventArgs e)
        {

        }

        //private void BACnetInternetworkTreeView_MouseClick(object sender, MouseEventArgs e)
        //{
        //}

        private void whoIsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // when user right clicks on a device, and then selects the who-is menu item... we end up here
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, menuDevice);
        }

        private void readPropertyObjectListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BACnetLibraryCL.ReadProperties(bnm, menuDevice );
        }


    }
}
