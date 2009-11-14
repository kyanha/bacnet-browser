﻿using System;
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
using System.Diagnostics;

using BACnetLibraryNS;

namespace BACnetInteropApp
{

    public partial class MainForm : Form
    {
        AppManager _apm = new AppManager();

        int defaultPort = BACnetInteropApp.Properties.Settings.Default.BACnetPort;
        //int defaultPort = 0xBAC0;

#if BACNET_TARGET_REMOTE
        OurSocket inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xEDD1);
#else
        OurSocket inside_socket;
#endif

        BACnetmanager bnm;

        myIPEndPoint IPEPDestination;

        Stopwatch _stopWatch = Stopwatch.StartNew();

        long _lastUpdatedTreeView;

        public MainForm()
        {
            InitializeComponent();

#if BACNET_TARGET_REMOTE
            IPHostEntry hostEntry = Dns.GetHostEntry("cloudrouter.dyndns.org");
            IPEPDestination = new IPEndPoint(hostEntry.AddressList[0], 0xEDD1);
#else
            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, defaultPort);
#endif

            // Change the radio buttons to match the default

            switch (defaultPort)
            {
                case 0xBAC0:
                    inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, defaultPort);
                    // todo, we cannot update the radiobuttons yet because they will close and reopen bnm, and that is not defined yet....
                    this.radioButtonBAC0.Checked = true;
                    this.radioButtonBAC1.Checked = false;
                    break;
                case 0xBAC1:
                    inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, defaultPort);
                    this.radioButtonBAC1.Checked = true;
                    this.radioButtonBAC0.Checked = false;
                    break;
                default:
                    // todo-panic here
                    break;
            }

            bnm = new BACnetmanager(inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            TreeViewDevices.ShowNodeToolTips = true;
            _apm.treeViewUpdater = new DeviceTreeView(_apm, TreeViewDevices);
            _apm.bnm = bnm;
        }


        private void mainform_closing(object sender, FormClosingEventArgs e)
        {

            bnm.BACnetManagerClose();

            Application.Exit();
        }


        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            this.TreeViewDevices.ExpandAll();
        }

        private void CollapseAllButton_Click(object sender, EventArgs e)
        {
            this.TreeViewDevices.CollapseAll();
        }


        private void buttonClear_Click(object sender, EventArgs e)
        {
            while (TreeViewDevices.Nodes.Count > 0)
            {
                TreeViewDevices.Nodes.Clear();
            }

        }


        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            bnm.MessageTodo("Hello");
            System.Diagnostics.Debug.WriteLine("Sending Who_is");
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, true, IPEPDestination);
        }


        private void timerUpdateUItick(object sender, EventArgs e)
        {
            if (bnm.DiagnosticLogMessage.Count > 0)
            {
                textBoxDiagnosticLog.AppendText(bnm.DiagnosticLogMessage.Dequeue());
            }

            while (bnm.newPacketQueue.Count != 0)
            {

                BACnetPacket p = bnm.newPacketQueue.Dequeue();

                try
                {
                    _apm.treeViewUpdater.UpdateDeviceTreeView(bnm, p);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }


            // now go through and update all the 'lastheardfrom' times - only once per second to avoid flicker, etc.

            if (_stopWatch.ElapsedMilliseconds > _lastUpdatedTreeView + 1000)
            {
                _lastUpdatedTreeView = _stopWatch.ElapsedMilliseconds;

                foreach (myTreeNode mtnNet in _apm.treeViewUpdater.TreeViewOnUI.Nodes)
                {
                    // each network node has device subnodes
                    foreach (myTreeNode mtnDev in mtnNet.Nodes)
                    {

                        mtnDev.Nodes[0].Text = "Seconds since msg " + ((_stopWatch.ElapsedMilliseconds - mtnDev.lastHeardFromTime) / 1000).ToString();

                        // change color if node is timing out

                        if (_stopWatch.ElapsedMilliseconds - mtnDev.lastHeardFromTime > 21000)
                        {
                            mtnDev.BackColor = System.Drawing.Color.BlanchedAlmond;
                        }
                        else
                        {
                            mtnDev.BackColor = System.Drawing.Color.White;
                        }


                        // remove node if gone for too long. (5 days)

                        if (_stopWatch.ElapsedMilliseconds - mtnDev.lastHeardFromTime > 5 * 24 * 60 * 60 * 1000)
                        {
                            mtnDev.Remove();
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

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, defaultPort);

            IPEndPoint local_ipep = new IPEndPoint(0, defaultPort);

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            BACnetPacket pkt = new BACnetPacket( _apm );

            pkt.npdu.isNPDUmessage = true;
            pkt.npdu.function = BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK;
            pkt.npdu.isBroadcast = true;

            pkt.EncodeBACnet(data, ref optr);


            bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);


        }

        private void Initialize_Routing_Table_Click(object sender, EventArgs e)
        {
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
            tempdevice.packet = new BACnetPacket(_apm);
            tempdevice.adr = new ADR(77, 4);

            tempdevice.packet.fromBIP = new myIPEndPoint(IPAddress.Parse("192.168.0.68"), 0xBAC0);

            BACnetLibraryCL.ReadProperties(bnm, tempdevice);
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
                TreeViewDevices.SelectedNode = e.Node;

                // Only works if the TreeNode is of type myTreeNode sub-class of TreeNode.

                if (e.Node is myTreeNode)
                {
                    myTreeNode mtn = (myTreeNode)e.Node;

                    if (mtn.isDevice == true)
                    {
                        // store the IP address, dest network, etc for the next action - todo, there must be a better way to do this; create a new class??

                        switch (mtn.device.type)
                        {
                            case BACnetEnums.DEVICE_TYPE.Router:
                                menuDevice = mtn.device;
                                contextMenuStripForRouter.Show(TreeViewDevices, e.Location);
                                break;

                            default:
                                menuDevice = mtn.device;
                                mycontextMenuStrip.Show(TreeViewDevices, e.Location);
                                break;

                        }


                    }
                }
            }
        }

        private void mycontextMenuStrip_Opening(object sender, CancelEventArgs e)
        {

        }

        private void whoIsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // when user right clicks on a device, and then selects the who-is menu item... we end up here
            BACnetLibraryNS.BACnetLibraryCL.SendWhoIs(bnm, menuDevice);
        }

        private void readPropertyObjectListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BACnetLibraryCL.ReadProperties(bnm, menuDevice);
        }

        private void radioButtonBAC0_Click(object sender, EventArgs e)
        {
            // switch to BAC0

            bnm.BACnetManagerClose();

            inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xBAC0);

            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, 0xBAC0);

            bnm = new BACnetmanager(inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            BACnetInteropApp.Properties.Settings.Default.BACnetPort = 0xBAC0;
            BACnetInteropApp.Properties.Settings.Default.Save();
        }

        private void radioButtonBAC1_Click(object sender, EventArgs e)
        {
            // switch to BAC1

            bnm.BACnetManagerClose();

            inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xBAC1);

            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, 0xBAC1);

            bnm = new BACnetmanager(inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            BACnetInteropApp.Properties.Settings.Default.BACnetPort = 0xBAC1;
            BACnetInteropApp.Properties.Settings.Default.Save();
        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            //this.ShowInTaskbar = false;  // Removes the application from the taskbar
            //Hide();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void readRouterTableToolStripMenuItem_Click(object sender, EventArgs e)
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
    }
}
