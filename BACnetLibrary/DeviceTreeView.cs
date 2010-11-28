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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BACnetLibrary
{
    public class DeviceTreeView
    {
        AppManager _apm;

        public TreeView TreeViewOnUI;
        public bool regenerate;


        public DeviceTreeView(AppManager apm, TreeView tv)
        {
            _apm = apm;
            TreeViewOnUI = tv;
        }

        myTreeNode findTreeNodeObject(myTreeNode mtn, BACnetObjectIdentifier bno)
        {
            for (int i = 0; i < mtn.Nodes.Count; i++)
            {
                // there may be a mix of myTreeNodes and TreeNodes. Only peer at myTreeNode types
                if (mtn.Nodes[i].GetType() == typeof(myTreeNode))
                {
                    if (((myTreeNode)mtn.Nodes[i]).oID != null)
                    {
                        if (((myTreeNode)mtn.Nodes[i]).oID.Equals(bno) == true) return (myTreeNode)mtn.Nodes[i];
                    }
                }
            }
            return null;
        }


        myTreeNode findTreeNodeDevice(BACnetManager bnm, BACnetPacket pkt)
        {
            // Remember, a device is defined by a Network Number and a MAC address. Nothing else!

            for (int i = 0; i < this.TreeViewOnUI.Nodes.Count; i++)
            {
                myTreeNode tnNetwork = (myTreeNode)this.TreeViewOnUI.Nodes[i];

                if (tnNetwork.networkNumber == pkt.srcDevice.adr.networkNumber)
                {
                    // found Network.. now check if node exists

                    for (int j = 0; j < tnNetwork.Nodes.Count; j++)
                    {
                        myTreeNode tnDevice = (myTreeNode)tnNetwork.Nodes[j];

                        if (tnDevice != null && tnDevice.device != null && tnDevice.device.Equals(pkt.srcDevice))
                        {
                            // found
                            tnDevice.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;
                            return tnDevice;
                        }
                    }
                }
            }
            return null;
        }



        // Returns reference to a network node, creating one if necessary

        myTreeNode EstablishTreeNodeNet(BACnetManager bnm, BACnetPacket pkt)
        {
            // return reference if it is found.

            for (int i = 0; i < this.TreeViewOnUI.Nodes.Count; i++)
            {
                myTreeNode tnNetwork = (myTreeNode)this.TreeViewOnUI.Nodes[i];

                if (tnNetwork.networkNumber == pkt.srcDevice.adr.networkNumber)
                {
                    tnNetwork.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;
                    return tnNetwork;
                }
            }

            // not found, so create it

            myTreeNode newNode = new myTreeNode();

            if (pkt.srcDevice.adr.directlyConnected == true)
            {
                newNode.Text = "Directly Connected Network ( " + pkt.srcDevice.adr.networkNumber + " )";
            }
            else
            {
                newNode.Text = "Network " + pkt.srcDevice.adr.networkNumber;
            }

            newNode.networkNumber = pkt.srcDevice.adr.networkNumber;
            newNode.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;

            this.TreeViewOnUI.Nodes.Add(newNode);
            return newNode;
        }


        // Returns reference to device node, creating one if necessary.

        myTreeNode EstablishTreeNodeDevice(BACnetManager bnm, BACnetPacket pkt)
        {
            // does one exist?
            myTreeNode mtnd = findTreeNodeDevice(bnm, pkt);
            if (mtnd != null) return mtnd;

            // no, time to create it

            // establish the network

            myTreeNode mtnn = EstablishTreeNodeNet(bnm, pkt);

            myTreeNode newNode = new myTreeNode();

            // If this is an I-Am-Router-To-Network message, then there is no deviceObject...
            if (pkt.messageType == BACnetPacket.MESSAGE_TYPE.NETWORK_LAYER )
            {
                if (pkt.npdu.function == BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK ||
                    pkt.npdu.function == BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK)
                {
                    // ignore
                    newNode.Text = "Router";
                    newNode.device.type = BACnetEnums.DEVICE_TYPE.Router;
                    newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.DeviceInstance, "");
                    newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.MACaddress, "");
                    newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.VendorID, "");
                }
                else
                {
                    _apm.MessageTodo("m0140 - What other NPDU only messages could we receive that would want us to create a device?");
                }
            }
            else
            {
                newNode.Text = "Device " + pkt.srcDevice.deviceObjectID.objectInstance;
            }

            newNode.device.adr = pkt.srcDevice.adr;
            //newNode.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.LastAccessTime, "Seconds since msg 0");

            newNode.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;

            mtnn.Nodes.Add(newNode);

            // todo, add a few other parameters.

            newNode.UpdatemyTreeNodeLeaf (myTreeNode.TREENODE_OBJ_TYPE.BACnetADR, pkt.srcDevice.adr.ToString());
            

            mtnn.Expand();

            return newNode;
        }


        void AddDeviceToTreeNode(BACnetManager bnm, BACnetPacket pkt)
        {
            // find the network display node, or else add a new one

            myTreeNode tni = EstablishTreeNodeNet(bnm, pkt);
            // found Network..
            // check if node already exists

            bool founddeviceflag = false;

            for (int j = 0; j < tni.Nodes.Count; j++)
            {
                myTreeNode tnj = (myTreeNode)tni.Nodes[j];

                if (tnj != null && tnj.device.Equals(pkt.srcDevice))
                {
                    // found, so quit
                    tnj.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;
                    founddeviceflag = true;
                    tnj.device = pkt.srcDevice;

                    // right here we can update some parameters... (eg. update i-am details from a router...) todonow

//                    newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.BACnetADR, pkt.srcDevice.adr.ToString());
                    tnj.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.DeviceInstance, pkt.srcDevice.deviceObjectID.objectInstance.ToString());
                    tnj.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.MACaddress, pkt.srcDevice.adr.MACaddress.ToString());
                    tnj.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.VendorID, pkt.srcDevice.vendorID.ToString());
                    tnj.Expand();
                    break;
                }
            }

            // add a device node to the network node

            if (founddeviceflag == false)
            {
                myTreeNode newNode = new myTreeNode();

                newNode.device = pkt.srcDevice;
                newNode.type = myTreeNode.TREENODE_OBJ_TYPE.Device;

                newNode.Name = "NewNode";

                // todonetxt - add back devicetype
                //NewNode.Text = "Device " + devUpdate.deviceObjectID.objectInstance + " " + devUpdate.deviceType.ToString();
                newNode.Text = "Device " ;
                newNode.ToolTipText = "Right Click on Device Objects for further functionality";

                newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.DeviceInstance, pkt.srcDevice.deviceObjectID.objectInstance.ToString());
                newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.MACaddress, pkt.srcDevice.adr.MACaddress.ToString());
                newNode.UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE.VendorID, pkt.srcDevice.vendorID.ToString());

                newNode.Tag = pkt.srcDevice.deviceObjectID.objectInstance;

                // add other paramters to our new node

                newNode.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;

                // todo, need to fix this, since we are adding TreeNodes here, not myTreeNodes...

                // newNode.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.LastAccessTime, "Seconds since msg 0");
                // newNode.Nodes.Add("Seconds since msg 0");

                if (pkt.srcDevice.adr.directlyConnected != true)
                {
                    newNode.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.GenericText, "Router IP Addr    " + pkt.srcDevice.directlyConnectedIPEndPointOfDevice);
                }

                newNode.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.BACnetADR, "BACnet ADR        " + pkt.srcDevice.adr.ToString() );

                newNode.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.GenericText, "Segmentation      " + (int)pkt.srcDevice.SegmentationSupported);

                // migrated away NewNode.isDevice = true;

                newNode.Expand();
                tni.Nodes.Add(newNode);

//                tni.ExpandAll();
                tni.Expand();
            }
        }

        public void UpdateDeviceTreeView(BACnetManager bnm, BACnetPacket pkt)
        {
            if (!pkt.apdu_present)
            {
                // means that this packet must be a NPDU message. Treat it differently

                switch (pkt.npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:

                        // means we had a response from a router - establish the router in our tree.
                        myTreeNode mtnd = EstablishTreeNodeDevice(bnm, pkt);

                        if (mtnd != null)
                        {
                            // found, or established, the treenode matching the device, 
                            // now add the objects to it.

                            // update the type of device - we now know it is a router (even if we did not know before).
                            if (mtnd.device != null)
                            {

                                if (mtnd.device.GetType() == typeof(Device))
                                {
                                    // it is still a simple device type, upgrade it
                                    _apm.MessageConfigChange(String.Format("m0139 - Device changing from a {0} to a Router", mtnd.Text));
                                    //if (mtnd.router == null) mtnd.router = new Router(mtnd.device);
                                    //mtnd.device = null;
                                    mtnd.device = new Router(mtnd.device);
                                    mtnd.type = myTreeNode.TREENODE_OBJ_TYPE.Router;
                                    mtnd.Text = "Router";
                                }
                            }

                            ((Router)mtnd.device).AddRoutingTableEntries(pkt.routerTableList);

                            // now go through the Router's routing table and display each entry appropriately

                            foreach (RoutingTableEntry rte in ((Router)mtnd.device).routingTableEntries)
                            {
                                bool found = false;

                                for (int i = 0; i < mtnd.Nodes.Count; i++)
                                {
                                    if (mtnd.Nodes[i].GetType() == typeof(myTreeNode))
                                    {
                                        myTreeNode mtnObj = (myTreeNode)mtnd.Nodes[i];
                                        if (mtnObj.type == myTreeNode.TREENODE_OBJ_TYPE.RouterTableEntry && mtnObj.rte.networkNumber == rte.networkNumber)
                                        {
                                            // if we get here, the object already exists in the display list and we must not add it again.
                                            // but let us check farSide flag for consistency
                                            if (mtnObj.rte.farSide != true) _apm.MessageTodo("m0143-Farside error");
                                            mtnObj.rte.farSide = true;
                                            found = true;
                                            // Redo the text, something (like portID) may have been updated
                                            mtnObj.Text = rte.ToString();
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    // not found, so add
                                    myTreeNode ntn = mtnd.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.RouterTableEntry, rte.ToString());
                                    ntn.rte = rte;
                                    ntn.ToolTipText = "Do not right click on this item, it has no effect";
                                    mtnd.Expand();
                                }
                            }
                        }
                        mtnd.Expand();
                        break;

                    //case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:

                    //    myTreeNode mtnrt = EstablishTreeNodeDevice(bnm, pkt);

                    //    if (mtnrt != null)
                    //    {
                    //        // found, or established, the treenode matching the device, 
                    //        // now add the objects to it.
                    //        // update the type of device - we now know it is a router (even if we did not know before).

                    //        mtnrt.device.type = BACnetEnums.DEVICE_TYPE.Router;
                    //        mtnrt.Text = "Router";

                    //        foreach (RoutingTableEntry rp in pkt.routerPortList)
                    //        {
                    //            bool found = false;

                    //            for (int i = 0; i < mtnrt.Nodes.Count; i++)
                    //            {
                    //                if (mtnrt.Nodes[i].GetType() == typeof(myTreeNode))
                    //                {
                    //                    myTreeNode mtnObj = (myTreeNode)mtnrt.Nodes[i];
                    //                    if (mtnObj.type == myTreeNode.TREENODE_OBJ_TYPE.RouterPort && mtnObj.rp.networkNumber == rp.networkNumber)
                    //                    {
                    //                        // if we get here, the object already exists in the list and we must not add it again.
                    //                        found = true;
                    //                        // farside nearside stuff mtnObj.networkNumberFromInitRouterTable = true;

                    //                        // Let us update it though, may be new information
                    //                        // mtnObj.Text = rp.ToString() ;
                    //                        break;
                    //                    }
                    //                }
                    //            }

                    //            if (!found)
                    //            {
                    //                //// not found, so add
                    //                myTreeNode ntn = mtnrt.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.RouterPort, rp.ToString());
                    //                ntn.rp = rp;
                    //                // nearside farside stuff ntn.networkNumberFromInitRouterTable = true;
                    //                // ntn.ToolTipText = "Do not right click on this item, it has no effect";
                    //                mtnrt.Expand();
                    //            }
                    //        }
                    //    }
                    //    break;

                }
                return;
            }
            else
            {
                // If we reach here, means APDU is present

                switch (pkt.pduType)
                {
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                        switch (pkt.unconfirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                pkt.srcDevice.directlyConnectedIPEndPointOfDevice = pkt.directlyConnectedIPEndPointOfDevice;  // was fromBIP

                                // todo, not sure where I use this - check
                                // bnm.deviceList.Add(pkt.srcDevice);

                                AddDeviceToTreeNode(bnm, pkt);
                                // bnm.newPacketQueue.myEnqueue(packet);
                                break;

                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                                // ignore reflected who-is messages
                                break;

                            default:
                                throw new Exception("m0178-Todo");
                        }
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                        switch (pkt.confirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                                myTreeNode mtn;
                                switch (pkt.propertyID)
                                {
                                    case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                        // what we need to do here is list the objects on the treeview...
                                        // first find the device in the tree
                                        mtn = findTreeNodeDevice(bnm, pkt);

                                        // add the objects to it
                                        if (mtn != null)
                                        {
                                            // found the treenode matching the device, add the objects to it.

                                            foreach (BACnetObjectIdentifier bno in pkt.objectList)
                                            {
                                                // does it exist? if so, ignore
                                                if (findTreeNodeObject(mtn, bno) == null)
                                                {
                                                    // not found, so add
                                                    myTreeNode ntn = new myTreeNode();
                                                    ntn.oID = bno;
                                                    ntn.type = myTreeNode.TREENODE_OBJ_TYPE.BACnetObject;
                                                    ntn.Text = bno.objectType.ToString() + "   Instance: " + bno.objectInstance.ToString();
                                                    ntn.ToolTipText = "Right-click to read more data";
                                                    mtn.Nodes.Add(ntn);
                                                }
                                            }
                                        }

                                        break;
                                    case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE:
                                        // these properties are of academic interest only. We already have this data from the object list.
                                        _apm.MessageTodo("m0080 - Perhaps we need to confirm we have the object on our list already and panic if not");
                                        break;

                                    case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_NAME:

                                        // Leave these for when we implement stuff on the Browser.

                                        // find the device, then the object, then append the name.
                                        //mtn = findTreeNodeDevice(bnm, pkt);
                                        //myTreeNode mtn2 = findTreeNodeObject(mtn, pkt.objectID);
                                        //if (mtn2 == null)
                                        //{
                                        //    _apm.MessageTodo("m0081 - this is a surprise");
                                        //}
                                        //else
                                        //{
                                        //    mtn2.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.GenericText, "Name");
                                        //}
                                        break;

                                    default:
                                        mtn = findTreeNodeDevice(bnm, pkt);
                                        if (mtn != null)
                                        {
                                            // for now, just add the object.
                                            // _apm.MessageTodo("m0073");
                                            //myTreeNode ntn = new myTreeNode();
                                            //ntn.type = myTreeNode.TREENODE_OBJ_TYPE.BACnetObject;
                                            //ntn.Text = pkt.propertyID.ToString();
                                            //ntn.ToolTipText = "Do not right click on this item, it has no effect";
                                            //mtn.Nodes.Add(ntn);
                                        }
                                        break;
                                }
                                break;
                            default:
                                throw new Exception("m0179-Todo");
                        }
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_REJECT:
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ERROR:
                        // These messages do not affect the Device TreeView, so we will ignore them..
                        break;

                    default:
                        _apm.MessageTodo("m0027 - Device Treeview needs to parse this message still: " + pkt.pduType.ToString());
                        break;
                }
            }

        }
    }
}
