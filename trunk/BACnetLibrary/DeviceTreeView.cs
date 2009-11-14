using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BACnetLibraryNS
{
    public class DeviceTreeView
    {
        AppManager _apm;

        public TreeView TreeViewOnUI;
        public bool regenerate;


        public DeviceTreeView(AppManager ram, TreeView tv)
        {
            _apm = ram;
            TreeViewOnUI = tv;
        }

        myTreeNode findTreeNodeObject(myTreeNode mtn, BACnetObjectIdentifier bno)
        {
            for (int i = 0; i < mtn.Nodes.Count; i++)
            {
                // there may be a mix of myTreeNodes and TreeNodes. Only peer at myTreeNode types
                if (mtn.Nodes[i].GetType() == typeof(myTreeNode))
                {
                    if (((myTreeNode)mtn.Nodes[i]).oID.Equals(bno) == true) return (myTreeNode)mtn.Nodes[i];
                }
            }
            return null;
        }


        myTreeNode findTreeNodeDevice(BACnetmanager bnm, BACnetPacket pkt)
        {
            for (int i = 0; i < this.TreeViewOnUI.Nodes.Count; i++)
            {
                myTreeNode tnNetwork = (myTreeNode)this.TreeViewOnUI.Nodes[i];

                if (tnNetwork.networkNumber == pkt.srcDevice.adr.networkNumber)
                {
                    // found Network.. now check if node exists

                    for (int j = 0; j < tnNetwork.Nodes.Count; j++)
                    {
                        myTreeNode tnDevice = (myTreeNode)tnNetwork.Nodes[j];

                        if (tnDevice != null && tnDevice.device.Equals(pkt.srcDevice))
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

        myTreeNode EstablishTreeNodeNet(BACnetmanager bnm, BACnetPacket pkt)
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
                newNode.Text = "Directly Connected";
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

        myTreeNode EstablishTreeNodeDevice(BACnetmanager bnm, BACnetPacket pkt)
        {
            // does one exist?
            myTreeNode mtnd = findTreeNodeDevice(bnm, pkt);
            if (mtnd != null ) return mtnd;

            // no, time to create it

            // establish the network

            myTreeNode mtnn = EstablishTreeNodeNet(bnm, pkt);

            myTreeNode newNode = new myTreeNode();

            newNode.Text = "Device " + pkt.srcDevice.deviceObjectID.objectInstance;

            newNode.device.adr = pkt.srcDevice.adr;

            newNode.AddMyTreeNodeObject( BACnetEnums.TREENODE_OBJ_TYPE.LastAccessTime, "Seconds since msg 0");

            newNode.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;

            mtnn.Nodes.Add(newNode);

            // todo, add a few other parameters.

            mtnn.Expand();

            return newNode;
        }


        void AddDevice(BACnetmanager bnm, BACnetPacket pkt)
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

                    break;
                }
            }

            // add a device node to the network node

            if (founddeviceflag == false)
            {
                myTreeNode NewNode = new myTreeNode();

                NewNode.device = pkt.srcDevice;

                NewNode.Name = "NewNode";

                // todonetxt - add back devicetype
                //NewNode.Text = "Device " + devUpdate.deviceObjectID.objectInstance + " " + devUpdate.deviceType.ToString();
                NewNode.Text = "Device " + pkt.srcDevice.deviceObjectID.objectInstance;

                NewNode.Tag = pkt.srcDevice.deviceObjectID.objectInstance;

                // add other paramters to our new node

                NewNode.lastHeardFromTime = _apm._stopWatch.ElapsedMilliseconds;

                // todo, need to fix this, since we are adding TreeNodes here, not myTreeNodes...

                NewNode.Nodes.Add("Seconds since msg 0");
                NewNode.Nodes.Add("Vendor ID         " + pkt.srcDevice.VendorId);
                NewNode.Nodes.Add("MAC Address       " + pkt.srcDevice.adr.MACaddress);

                if (pkt.srcDevice.adr.directlyConnected != true)
                {
                    NewNode.Nodes.Add("Router IP Addr " + pkt.srcDevice.directlyConnectedIPEndPointOfDevice);
                }

                NewNode.Nodes.Add("Segmentation   " + (int)pkt.srcDevice.SegmentationSupported);

                NewNode.isDevice = true;

                tni.Nodes.Add(NewNode);

                tni.ExpandAll();
            }
        }

        public void UpdateDeviceTreeView(BACnetmanager bnm, BACnetPacket pkt)
        {
            if (!pkt.apdu_present)
            {
                // means that this packet must be a NPDU message. Treat it differently

                switch (pkt.npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        //if (pkt.numberList.Count > 0)
                        //{
                            // means we had a response from a router - establish the router in our tree.
                            myTreeNode mtnd = EstablishTreeNodeDevice(bnm, pkt);

                            if (mtnd != null)
                            {
                                // found, or established, the treenode matching the device, 
                                // now add the objects to it.

                                // update the type of device - we now know it is a router (even if we did not know before).

                                mtnd.device.type = BACnetEnums.DEVICE_TYPE.Router;
                                mtnd.Text = "Router";

                                foreach (int bno in pkt.numberList)
                                {
                                    bool found = false;

                                    for (int i = 0; i < mtnd.Nodes.Count; i++)
                                    {
                                        if (mtnd.Nodes[i].GetType() == typeof(myTreeNode))
                                        {
                                            myTreeNode mtnObj = (myTreeNode)mtnd.Nodes[i];
                                            if (mtnObj.type == BACnetEnums.TREENODE_OBJ_TYPE.NetworkNumber && mtnObj.networkNumber == bno)
                                            {
                                                // if we get here, the object already exists in the list and we must not add it again.
                                                mtnObj.networkNumberFromWhoIsRouter = true;
                                                found = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!found)
                                    {
                                        //// not found, so add
                                        myTreeNode ntn = mtnd.AddMyTreeNodeObject(BACnetEnums.TREENODE_OBJ_TYPE.NetworkNumber, "Network " + bno.ToString());
                                        ntn.networkNumber = (uint)bno;
                                        ntn.networkNumberFromWhoIsRouter = true;
                                        ntn.ToolTipText = "Do not right click on this item, it has no effect";
                                        mtnd.Expand();
                                    }
                                }

                            }

                        //}
                        break;

                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                        _apm.bnm.MessageTodo("Implement " + pkt.npdu.function);

                        myTreeNode mtnrt = EstablishTreeNodeDevice(bnm, pkt);

                        if (mtnrt != null)
                        {
                            // found, or established, the treenode matching the device, 
                            // now add the objects to it.
                            // update the type of device - we now know it is a router (even if we did not know before).

                            mtnrt.device.type = BACnetEnums.DEVICE_TYPE.Router;
                            mtnrt.Text = "Router";

                            foreach ( RouterPort rp in pkt.routerPortList )
                            {
                                bool found = false;

                                for (int i = 0; i < mtnrt.Nodes.Count; i++)
                                {
                                    if (mtnrt.Nodes[i].GetType() == typeof(myTreeNode))
                                    {
                                        myTreeNode mtnObj = (myTreeNode)mtnrt.Nodes[i];
                                        if (mtnObj.type == BACnetEnums.TREENODE_OBJ_TYPE.NetworkNumber && mtnObj.networkNumber == rp.networkNumber )
                                        {
                                            // if we get here, the object already exists in the list and we must not add it again.
                                            found = true;
                                            mtnObj.networkNumberFromInitRouterTable = true;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    //// not found, so add
                                    myTreeNode ntn = mtnrt.AddMyTreeNodeObject(BACnetEnums.TREENODE_OBJ_TYPE.NetworkNumber, "Network " + rp.networkNumber.ToString());
                                    ntn.networkNumber = rp.networkNumber ;
                                    ntn.networkNumberFromInitRouterTable = true;
                                    ntn.ToolTipText = "Do not right click on this item, it has no effect";
                                    mtnrt.Expand();
                                }
                            }

                        }

                        break;

                }

                return;
            }

            switch (pkt.pduType)
            {
                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                    switch (pkt.unconfirmedServiceChoice)
                    {
                        case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                            pkt.srcDevice.directlyConnectedIPEndPointOfDevice = pkt.fromBIP;

                            // todo, not sure where I use this - check
                            bnm.Devicelist.Add(pkt.srcDevice);

                            AddDevice(bnm, pkt);
                            // bnm.newPacketQueue.Enqueue(packet);
                            break;

                        case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                            // ignore reflected who-is messages
                            break;

                        default:
                            BACnetLibraryCL.Panic("Todo");
                            break;
                    }
                    break;

                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                    switch (pkt.confirmedServiceChoice)
                    {
                        case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                            switch (pkt.propertyID)
                            {
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                    // what we need to do here is list the objects on the treeview...

                                    // find the device in the tree
                                    // add the objects to it

                                    myTreeNode mtn = findTreeNodeDevice(bnm, pkt);

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
                                                ntn.Text = bno.objectType.ToString() + "   Instance: " + bno.objectInstance.ToString();
                                                ntn.ToolTipText = "Do not right click on this item, it has no effect";
                                                mtn.Nodes.Add(ntn);
                                            }
                                        }

                                        // now remove the object list??? (but it will be removed when packet is destroyed....

                                    }

                                    break;
                                default:
                                    BACnetLibraryCL.Panic("Todo");
                                    break;
                            }
                            break;
                        default:
                            BACnetLibraryCL.Panic("Todo");
                            break;
                    }
                    break;

                default:
                    BACnetLibraryCL.Panic("Todo");
                    break;
            }

        }
    }
}
