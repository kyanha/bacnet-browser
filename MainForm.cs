/*
 * The MIT License
 * 
 * Copyright (c) 2010 BACnet Iteroperability Testing Services, Inc.
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 *  BACnet Interoperability Testing Services, Inc.
 *      http://www.bac-test.com
 * 
 * BACnet Wiki
 *      http://www.bacnetwiki.com
 * 
 * MIT License - OSI (Open Source Initiative) Approved License
 *      http://www.opensource.org/licenses/mit-license.php
 * 
*/

/*
 * 28 Nov 10    EKH Releasing under MIT license
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using BACnetLibrary;
using Diagnostics;
using BaseLibrary;
using BACnetTestClient;

namespace BACnetInteropApp
{

    public partial class MainForm : Form
    {
        AppManager _apm = new AppManager();

        int defaultPort = BACnetInteropApp.Properties.Settings.Default.BACnetPort;

#if BACNET_TARGET_REMOTE
        OurSocket inside_socket = new OurSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, 0xEDD1);
#else
        OurSocket inside_socket;
#endif

        BACnetManager _bnm;

        myIPEndPoint IPEPDestination;

        Stopwatch _stopWatch = Stopwatch.StartNew();

        long _lastUpdatedTreeView;

        public MainForm()
        {
            InitializeComponent();

            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, defaultPort);

            // Change the radio buttons to match the default

            switch (defaultPort)
            {
                case 0xBAC0:
                    inside_socket = new OurSocket(_apm, defaultPort);
                    // todo, we cannot update the radiobuttons yet because they will close and reopen bnm, and that is not defined yet....
                    this.radioButtonBAC0.Checked = true;
                    this.radioButtonBAC1.Checked = false;
                    break;
                case 0xBAC1:
                    inside_socket = new OurSocket(_apm, defaultPort);
                    this.radioButtonBAC1.Checked = true;
                    this.radioButtonBAC0.Checked = false;
                    break;
                default:
                    // todo-panic here
                    break;
            }

            _bnm = new BACnetManager(_apm, inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            TreeViewDevices.ShowNodeToolTips = true;
            _apm.treeViewUpdater = new DeviceTreeView(_apm, TreeViewDevices);
            _apm.bnm = _bnm;

            // backgroundWorkerApplication.RunWorkerAsync();
        }


        private void mainform_closing(object sender, FormClosingEventArgs e)
        {

            _bnm.BACnetManagerClose();

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

            lock (_apm.internalRouterInfo)
            {
                _apm.internalRouterInfo.routingTableEntries.Clear();
            }

        }


        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            BACnetUtil.SendWhoIs(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST);
        }


        private void buttonStartScan_Click(object sender, EventArgs e)
        {
            OurApplication oa = new OurApplication(_apm, _bnm);
            Thread oThread = new Thread(new ThreadStart(oa.OurApplicationThread));
            oThread.Start();
        }


        private void timerUpdateUItick(object sender, EventArgs e)
        {
            if (_apm.DiagnosticLogProtocol.Count > 0)
            {
                textBoxProtocol.AppendText(DateTime.Now.ToString("MM/dd HH:mm:ss - ") + _apm.DiagnosticLogProtocol.myDequeue());
                tabPageProtocol.Text = "Protocol *";
            }

            if (_apm.DiagnosticLogPanic.Count > 0)
            {
                textBoxPanics.BackColor = Color.Pink;
                textBoxPanics.AppendText(DateTime.Now.ToString("MM/dd HH:mm:ss - ") + _apm.DiagnosticLogPanic.myDequeue());
                tabControlLogs.SelectTab(tabPageErrors);
                tabPageErrors.Text = "Errors *";
            }

            while (_bnm.newPacketQueue.Count != 0)
            {

                BACnetPacket p = _bnm.newPacketQueue.myDequeue();
                try
                {
                    _apm.treeViewUpdater.UpdateDeviceTreeView(_bnm, p);
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
                        //mtnDev.Nodes[0].Text = "Seconds since msg " + ((_stopWatch.ElapsedMilliseconds - mtnDev.lastHeardFromTime) / 1000).ToString();

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
            BACnetLibrary.BACnetUtil.SendWhoIsRouter(_apm, _apm.bnm, BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST);
        }


        private void Initialize_Routing_Table_Click(object sender, EventArgs e)
        {
            // which router? We will broadcast this if the button is clicked and deal with all the responses one by one
            // IPEPDestination is set to broadcast
            BACnetLibrary.BACnetUtil.SendInitRoutingTable(_apm, _bnm, IPEPDestination);
        }


        private void SendWhoIs_Tick(object sender, EventArgs e)
        {
            BACnetLibrary.BACnetUtil.SendWhoIs(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST);
        }

        private void buttonSendReadProperty_Click(object sender, EventArgs e)
        {
            // todo- if the IP address does not exist, but is within our subnet, then there is no transmission, but neither is there
            // an exception - simply nothing appears in Wireshark. Investigate and go proactive warning the user...\

            // set up temporary device and packet information for the purposes of testing this function call
            Device tempdevice = new Device();
            tempdevice.packet = new BACnetPacket(_apm);
            tempdevice.adr = new ADR(77, 4);

            tempdevice.packet.directlyConnectedIPEndPointOfDevice = new myIPEndPoint(IPAddress.Parse("192.168.0.68"), 0xBAC0);

            BACnetLibrary.BACnetUtil.ReadPropertyObjectList_deprecated(_bnm, tempdevice);
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timerHeartbeatWhoIs.Enabled = true;
                BACnetLibrary.BACnetUtil.SendWhoIs(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST);
            }
            else
            {
                timerHeartbeatWhoIs.Enabled = false;
            }
        }


        Device menuDevice;
        BACnetObjectIdentifier BACnetOID;
        Diagnostic savedDiagnostic;
        myTreeNode savedMTN;

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

                    switch (mtn.type)
                    {
                        case myTreeNode.TREENODE_OBJ_TYPE.Device:
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
                            break;

                        case myTreeNode.TREENODE_OBJ_TYPE.BACnetObject:
                            BACnetOID = mtn.oID;
                            contextMenuStripForObject.Show(TreeViewDevices, e.Location);
                            break;

                        case myTreeNode.TREENODE_OBJ_TYPE.NetworkNumber:
                            break;

                        case myTreeNode.TREENODE_OBJ_TYPE.DiagnosticDetails:

                            savedMTN = mtn;
                            savedDiagnostic = mtn.diagnostic;
                            contextMenuStripForDiagnostic.Show((Control)sender, e.Location);
                            break;

                        default:
                            _bnm.MessageTodo("m0192 - Unimplemented Menu");
                            break;
                    }
                }
            }
        }

        private void whoIsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // when user right clicks on a device, and then selects the who-is menu item... we end up here
            BACnetLibrary.BACnetUtil.SendWhoIs(_apm, _bnm, menuDevice);
        }

        private void readPropertyObjectListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BACnetLibrary.BACnetUtil.ReadPropertyObjectList_deprecated(_bnm, menuDevice);
        }

        private void radioButtonBAC0_Click(object sender, EventArgs e)
        {
            // switch to BAC0

            _bnm.BACnetManagerClose();

            inside_socket = new OurSocket(_apm, 0xBAC0);

            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, 0xBAC0);

            _bnm = new BACnetManager(_apm, inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            BACnetInteropApp.Properties.Settings.Default.BACnetPort = 0xBAC0;
            BACnetInteropApp.Properties.Settings.Default.Save();
        }

        private void radioButtonBAC1_Click(object sender, EventArgs e)
        {
            // switch to BAC1

            _bnm.BACnetManagerClose();

            inside_socket = new OurSocket(_apm, 0xBAC1);

            IPEPDestination = new myIPEndPoint(IPAddress.Broadcast, 0xBAC1);

            _bnm = new BACnetManager(_apm, inside_socket, BACnetEnums.SERVER_DEVICE_ID, IPEPDestination);

            BACnetInteropApp.Properties.Settings.Default.BACnetPort = 0xBAC1;
            BACnetInteropApp.Properties.Settings.Default.Save();
        }


        private void MainForm_Resize(object sender, EventArgs e)
        {
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

        private void backgroundWorkerDiagnosticManager_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonStartTests.Enabled = true;
        }

        private void backgroundWorkerDiagnosticManager_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is List<Diagnostic>)
            {
                Diagnostic.DiagnosticManager(_apm, e.Argument as List<Diagnostic>);
            }
            else
            {
                throw new Exception("m0162-Not passing the right parameter");
            }
        }


        private void buttonAllTests_Click(object sender, EventArgs e)
        {
            lock (_apm)
            {
                if (!_apm.diagnosticsRunning)
                {
                    _apm.diagnosticsRunning = true;
                    buttonStartTests.Enabled = false;

                    // browse the whole tree, looking for devices (that have valid diagnostics), create a list of diagnostics to execute and pass it to the diagnosticator

                    // now fire up a new thread to go through all the devices again, pausing on each diagnostic until it is complete..
                    // for now, no thread

                    List<Diagnostic> diagList = new List<Diagnostic>();

                    foreach (TreeNode networkNode in TreeViewDevices.Nodes)
                    {
                        foreach (myTreeNode deviceNode in networkNode.Nodes)
                        {
                            foreach (myTreeNode objNode in deviceNode.Nodes)
                            {
                                if (objNode.type == myTreeNode.TREENODE_OBJ_TYPE.ScheduledDiagnostics)
                                {
                                    foreach (myTreeNode schdNode in objNode.Nodes)
                                    {
                                        if (schdNode.type == myTreeNode.TREENODE_OBJ_TYPE.DiagnosticDetails)
                                        {
                                            diagList.Add(schdNode.diagnostic);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    backgroundWorkerDiagnosticManager.RunWorkerAsync(diagList);
                }
            }
        }

        static public void AddDiagnosticsTomyTreeNodeDiagnosticListAll(AppManager apm, BACnetManager bnm, myTreeNode deviceTreeNode, myTreeNode mtnDiagnosticList)
        {
            List<Diagnostic> diagList;

            // Adding 'standard' diagnostics 
            diagList = Diagnostics.DiagSetup.BuildDiagnosticListStandard(apm, bnm, deviceTreeNode);
            Diagnostic.AddUniqueDiagnostics(mtnDiagnosticList, diagList);

            // Adding 'private' or 'user' diagnostics
            diagList = DiagnosticPrivate.PDiagSetup.BuildDiagnosticListPrivate(apm, bnm, deviceTreeNode);
            Diagnostic.AddUniqueDiagnostics(mtnDiagnosticList, diagList);
        }

        public void SetupDiagnosticsNew()
        {
            lock (_apm)
            {
                foreach (TreeNode networkNode in TreeViewDevices.Nodes)
                {
                    foreach (myTreeNode deviceNode in networkNode.Nodes)
                    {
                        myTreeNode mtn = deviceNode.EnsureMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.ScheduledDiagnostics, "Scheduled Diagnostics");
                        // Add our new test
                        AddDiagnosticsTomyTreeNodeDiagnosticListAll(_apm, _bnm, deviceNode, mtn);
                    }
                }
            }
        }


        private void buttonClearProtocol_Click(object sender, EventArgs e)
        {
            textBoxProtocol.Clear();
            tabPageProtocol.Text = "Protocol";
        }

        private void contextMenuStripForDiagnostic_MouseClick(object sender, MouseEventArgs e)
        {

            ((ContextMenuStrip)sender).Hide();
            TreeViewDevices.SelectedNode = null;

            int row = e.Y / 0x12;

            switch (row)
            {
                case 0:
                    // Execute diagnostic
                    lock (_apm)
                    {
                        if (!_apm.diagnosticsRunning)
                        {
                            _apm.diagnosticsRunning = true;
                            buttonStartTests.Enabled = false;
                            List<Diagnostic> diagList = new List<Diagnostic>();
                            diagList.Add(savedDiagnostic);
                            backgroundWorkerDiagnosticManager.RunWorkerAsync(diagList);
                        }
                    }
                    break;
                case 1:
                    // ask for help
                    savedDiagnostic.RunDiagnosticHelp();
                    break;

            }
        }

        private void contextMenuStripForDiagnostic_Click(object sender, EventArgs e)
        {

        }

        private void buttonSendIAm_Click(object sender, EventArgs e)
        {
            BACnetUtil.SendIAm(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST, (uint)Convert.ToDecimal(textBoxDeviceInstance.Text));

        }

        private void buttonWhoIsLongMAC_Click(object sender, EventArgs e)
        {
            // BTL Implementation Guide, Section 2.9 - be prepared to accept a MAC address up to 18 bytes long. Here is the test. I have not found any device that passes this yet.
            BACnetPacket pkt = new BACnetPacket(_apm);
            pkt.EncodeBVLL(BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU);
            pkt.EncodeNPCI(new DADR(BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST), new ADR(3333, "0102030405060708"), BACnetLibrary.BACnetPacket.MESSAGE_TYPE.APPLICATION);
            pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);

            BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);
        }

        private void buttonPrepNewTests_Click(object sender, EventArgs e)
        {
            SetupDiagnosticsNew();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Show the warning box at least once !
            if (!Properties.Settings.Default.LegalAcknowledged)
            {
                WarningBox wb = new WarningBox();
                wb.ShowDialog();
            }
            UserManagement.UpdateTitle(this);
        }
    }
}
