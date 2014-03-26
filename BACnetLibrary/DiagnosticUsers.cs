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
using BACnetLibrary;

namespace Diagnostics
{
    public class DiagnosticBIT00001DuplicateRoutingEntry : Diagnostic
    {
        public DiagnosticBIT00001DuplicateRoutingEntry(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            return "BIT00001 - Add Duplicate Routing Entry";
        }

        public override void Execute()
        {
            BACnetPacket incomingPkt;

            //if (!(deviceNode.device.GetType() == typeof(Router)))
            //{
            //    MarkDiagnosticWithNote("Device is not a router, cannot run this test");
            //    return;
            //}

            // todo, block this queue unless a diagnostic is running
            // clear the queue
            ClearIncomingPacketQueue();

            try
            {
                BACnetUtil.SendInitRoutingTable(_apm, _bnm, new DADR(devicTreeNode.device.adr));
                incomingPkt = waitForPacket(5000, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK);  // todo, add fromaddress...
                // got it.
                foreach (RoutingTableEntry rte in incomingPkt.routerTableList)
                {
                    Console.WriteLine(rte.ToString());
                }

                // record the number of entries, and record the last item
                int entrycount = incomingPkt.routerTableList.Count;
                if (entrycount < 1 || entrycount > 255)
                {
                    Console.WriteLine("Failed this time around");
                    // test fails, impossible count
                    return;
                    //throw new Exception("m0525-Totally bogus router table entry count");
                }

                RoutingTableEntry rtDupe = incomingPkt.routerTableList[entrycount - 1];

                // now write the last entry again. 
                BACnetPacket outgoingPkt = new BACnetPacket(_apm, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE);
                outgoingPkt.SetDestination(new DADR(incomingPkt.srcDevice.adr));
                outgoingPkt.routerTableList = new List<RoutingTableEntry>();
                outgoingPkt.routerTableList.Add(rtDupe);
                outgoingPkt.EncodeBACnet();
                _bnm.insideSocket.OurSendTo(outgoingPkt.buffer, outgoingPkt.optr, incomingPkt.srcDevice.adr.ResolvedIPEP());

                // wait for the ack
                incomingPkt = waitForPacket(5000, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK);

                // drop the packet

                // re-read the routing table
                BACnetUtil.SendInitRoutingTable(_apm, _bnm, new DADR(devicTreeNode.device.adr));
                incomingPkt = waitForPacket(5000, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK);

                foreach (RoutingTableEntry rte in incomingPkt.routerTableList)
                {
                    Console.WriteLine(rte.ToString());
                }

                // record the number of entries, and record the last item
                if (incomingPkt.routerTableList.Count != entrycount)
                {
                    Console.WriteLine("m0165-Failed entrycount");
                    // Todo, check if this _IS_ OK.
                    MarkDiagnosticFailed("The entrycount did not remain the same, i.e. The router allowed the addition of a duplicate Network Number");
                    return;
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
                //BACnetLibrary.Panic(ex.ToString());
                //return;
            }

            MarkDiagnosticSuccess();
            Console.WriteLine("Diag done");
        }

    }


    public class DiagnosticBIT00018CheckPortID : Diagnostic
    {
        public DiagnosticBIT00018CheckPortID(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00018 - Check Port ID - value between 1 and 255";

        public override string ToString()
        {
            // http://www.bacnetwiki.com/wiki/index.php?title=Port_ID
            return diagnosticName ;
        }

        public override void RunDiagnosticHelp()
        {
            System.Diagnostics.Process.Start("http://www.bacnetwiki.com/wiki/index.php?title=BIT00018");
        }


        public override void Execute()
        {
            try
            {
                // basic test, interrogate router for Routing Table, check the Port IDs

                BACnetUtil.SendInitRoutingTable(_apm, _bnm, new DADR(devicTreeNode.device.adr));

                BACnetPacket incomingPkt = waitForPacket(5000, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK);

                foreach (RoutingTableEntry rte in incomingPkt.routerTableList)
                {
                    if (rte.portID == 0)
                    {
                        // fails
                        //Console.WriteLine("Illegal Port ID " + rte.ToString());
                        // Need to mark diagnostic as failed, with a comment
                        MarkDiagnosticFailed("Port ID found to be out of range: " + rte.ToString() );
                        return;
                    }
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            MarkDiagnosticSuccess();
        }
    }


    public class DiagnosticBIT00024WhoIsNoSADR : Diagnostic
    {
        public DiagnosticBIT00024WhoIsNoSADR(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            return "BIT00003 - Sending a very short Who-Is message, local broadcast with no SADR";
        }

        public override void Execute()
        {
            BACnetUtil.SendWhoIs(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST);

            MarkDiagnosticSuccess();
        }
    }


    public class DiagnosticBIT00004ThreeRapidWhoIs : Diagnostic
    {
        public DiagnosticBIT00004ThreeRapidWhoIs(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            return "BIT00004 - Sending 3 Who-Is messages very quickly and checking for all I-Ams";
        }

        // Notes: This test may timeout on devices the other side of routers due to insufficiencies in the router itself. Be aware of this for 
        // now, and make changes to isolate and not report these cases.

        public override void Execute()
        {
            // If the device is the other side of a router, do not execute this test on it.
            //if (!deviceNode.device.adr.directlyConnected)
            //{
            //    MarkDiagnosticWithNote("This device is the other side of a Router, cannot complete this test");
            //    return;
            //}

            int receiveCount = 0;

            ClearIncomingPacketQueue();
            try
            {
                int tries = 3;
                for (int i = 0; i < tries; i++)
                {
                    BACnetUtil.SendWhoIs(_apm, _bnm, devicTreeNode.device);
                    Sleep(10);  // without a little sleep, it seems that the first few packets out the ports may stumble
                }
                for (int i = 0; i < tries; i++)
                {
                    if (devicTreeNode.device.adr.directlyConnected == true)
                    {
                        waitForPacket(1000); // todo set the IAM field in waitforpacket
                    }
                    else
                    {
                        waitForPacket(5000);
                    }
                    receiveCount++;
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..

                if (receiveCount > 0)
                {
                    // got at least one message. 
                    if (!devicTreeNode.device.adr.directlyConnected)
                    {
                        // and device is the other side of a router. We can be a bit forgiving here - some routers dont queue the messages.
                        MarkDiagnosticWithNote("This device failed to respond to all three messages, but it did respond to at least one");
                        MarkDiagnosticWithNote("   - and it is the other side of a router, so the failure may be due to the router in the way");
                        return;
                    }
                }
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
            }

            MarkDiagnosticSuccess();
        }
    }

    public class DiagnosticBIT00025LocalBroadcast : Diagnostic
    {
        public DiagnosticBIT00025LocalBroadcast(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            return "BIT00005 - Local Broadcast";
        }

        // tests that only local devices answer to local broadcasts. (A type of router test).

        public override void Execute()
        {
            bool someFails = false;

            WaitForQuietOnIncomingPacketQueue();
            ClearIncomingPacketQueue();

            try
            {
                DADR dadr = new DADR(_bnm, BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST);
                BACnetUtil.SendWhoIs(_apm, _bnm, dadr);
                // Sleep(1000);        // and perhaps longer on large networks....
                WaitForQuietOnIncomingPacketQueue();

                // check the incoming packets, make sure that they are all local addresses
                while (_apm.pktQueueToApplication.Count > 0)
                {
                    if (_apm.pktQueueToApplication.Count == 0)
                    {
                        // wait just a while longer, there may be more packets dribbling in. So by pausing here
                        // we wait 1 second longer than the last packet to arrive... seems reasonable to me
                        Sleep(1000);
                    }

                    BACnetPacket pkt = _apm.pktQueueToApplication.myDequeue();
                    pkt.DecodeBACnet();

                    if (pkt.srcDevice.adr.directlyConnected != true)
                    {
                        MarkDiagnosticFailed("A device [" + pkt.srcDevice.adr.ToString() + "] that is not directly connected responded");
                        someFails = true;
                    }
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            if (someFails == false) MarkDiagnosticSuccess(); // Mark this diagnostic a success, any failures will re
        }


    }


    public class DiagnosticBIT00006RemoteBroadcast : Diagnostic
    {
        public DiagnosticBIT00006RemoteBroadcast(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            return "BIT00006 - Remote Broadcast";
        }

        public override void Execute()
        {
            bool someFails = false;
            WaitForQuietOnIncomingPacketQueue();
            ClearIncomingPacketQueue();
            try
            {
                if (this.devicTreeNode.device.adr.directlyConnected == true)
                {
                    MarkDiagnosticWithNote("This node is directly connected. Diagnostic not appropriate");
                    return;
                }
                // todo. the ADR constructor is going to use an IP broadcast. We should know the router for this device and transmit the packet only to that router...
                DADR dadr = new DADR(_bnm, BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST, this.devicTreeNode.device.adr.networkNumber);
                BACnetUtil.SendWhoIs(_apm, _bnm, dadr);
                WaitForQuietOnIncomingPacketQueue();
                // check the incoming packets, make sure that they are all from the desired address
                if (_apm.pktQueueToApplication.Count == 0)
                {
                    MarkDiagnosticFailed("No response to the Who-Is");
                    return;
                }
                while (_apm.pktQueueToApplication.Count > 0)
                {
                    //                    BACnetPacket pkt = _apm.pktQueueToApplication.myDequeue();
                    BACnetPacket pkt = waitForPacket(1);
                    if (pkt.srcDevice.adr.networkNumber != this.devicTreeNode.device.adr.networkNumber)
                    {
                        MarkDiagnosticFailed("A device [" + pkt.srcDevice.adr.ToString() + "] from network [" + pkt.srcDevice.adr.networkNumber.ToString() + "] that is not directly connected responded");
                        someFails = true;
                    }
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
                //BACnetLibrary.Panic(ex.ToString());
                //return;
            }
            if (!someFails) MarkDiagnosticSuccess(); // Mark this diagnostic a success, any failures will re
        }


    }



    public class Diagnostic0007IAmRouterIsLocalBroadcast : Diagnostic
    {
        public Diagnostic0007IAmRouterIsLocalBroadcast(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        public override string ToString()
        {
            // i believe who-is-router and i-am-router are local broadcasts.
            // 6.4.2 pg 54 states using broadcast mac address
            // 6.6.3.3. states local broadcasts
            // 6.6.3.2. routers are responsible for forwarding who-is-router-to-network requests -> implies they are NOT broadcasts.

            // todo, resolve 
            // todo, wiki
            // todo, bacnet-l

            return "BIT00007 - Is the I-Am Router a local Broadcast (if directly connected)";
            // todo, do a diagnostic of any router discovered on the far side of another router
            // todo, use cloudrouter to catch any illegal events.

        }

        public override void Execute()
        {
            bool someFails = false;
            WaitForQuietOnIncomingPacketQueue();
            ClearIncomingPacketQueue();
            try
            {
                if (this.devicTreeNode.device.adr.directlyConnected == true)
                {
                    MarkDiagnosticWithNote("This node needs to be directly connected for this Diagnostic. Diagnostic not appropriate");
                    return;
                }
                // todo. the ADR constructor is going to use an IP broadcast. We should know the router for this device and transmit the packet only to that router...
                BACnetUtil.SendWhoIsRouter(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST);
                WaitForQuietOnIncomingPacketQueue();
                // check the incoming packets, make sure that they are all from local addresses
                if (_apm.pktQueueToApplication.Count == 0)
                {
                    MarkDiagnosticFailed("No response to the Who-Is-Router");
                    return;
                }
                while (_apm.pktQueueToApplication.Count > 0)
                {
                    BACnetPacket pkt = waitForPacket(1);
                    if (!pkt.dAdr.isLocalBroadcast)
                    {
                        MarkDiagnosticFailed("A Router [" + pkt.srcDevice.adr.ToString() + "] did not use a local broadcast");
                        someFails = true;
                    }
                }
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!someFails) MarkDiagnosticSuccess(); // Mark this diagnostic a success, any failures will re
        }
    }


    public class DiagnosticBIT00008ReadPropertyProtocolServices : Diagnostic
    {
        public DiagnosticBIT00008ReadPropertyProtocolServices(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }
        public override string ToString()
        {
            return "BIT00008 - Read Protocol Services Supported";
        }

        public override void Execute()
        {
            try
            {
                ClearIncomingPacketQueue();
                BACnetUtil.SendReadProtocolServices(_apm, _bnm, devicTreeNode.device);
                waitForPacket(5000);
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
                //BACnetLibrary.Panic(ex.ToString());
                //return;
            }
            MarkDiagnosticSuccess();
        }
    }


    public class DiagnosticBIT00010ForceRouteDiscovery : Diagnostic
    {
        string diagnosticName = "BIT00010 - Force the Device to discover Route";

        public DiagnosticBIT00010ForceRouteDiscovery(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }
        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            try
            {
                _apm.MessageLog("Executing " + diagnosticName);

                ClearIncomingPacketQueue();

                // find a non-existing network number in the whole attached BACnet internetwork by examining our internal router table that
                // has been built up during device discovery phase
                int useThisNetworkNumber = 9;
                for (int i = 0xfffe; i > 1; i--)
                {
                    if (_apm.internalRouterInfo.FindRoutingTableEntry(i) == null)
                    {
                        // we have not seen or used this network number yet
                        useThisNetworkNumber = i;
                        break;
                    }
                }

                //                BACnetUtil.SendReadProperty(_apm, _bnm, deviceNode.device, deviceNode.device.deviceObjectID, BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE);

                BACnetPacket pkt = new BACnetPacket(_apm);
                // pkt.EncodeBVLL(new DADR(deviceNode.device.adr), BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU);
                pkt.EncodeNPCI(new DADR(devicTreeNode.device.adr), new ADR((ushort)useThisNetworkNumber, (uint)3), BACnetLibrary.BACnetPacket.MESSAGE_TYPE.APPLICATION);
                // todonow, put these next steps into functions
                // build confirmed request header 
                // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU
                pkt.buffer[pkt.optr++] = (Byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST;
                pkt.buffer[pkt.optr++] = 0x05;  // max segs, max resp. todo
                pkt.buffer[pkt.optr++] = _apm.invokeID++;
                // sequence number may come next, dep on flags. todo
                pkt.buffer[pkt.optr++] = (byte)BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY;
                // context encoding
                // http://www.bacnetwiki.com/wiki/index.php?title=Read_Property
                devicTreeNode.device.deviceObjectID.Encode(pkt.buffer, ref pkt.optr);
                BACnetUtil.EncodeContextTag(pkt.buffer, ref pkt.optr, 1, (int)BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE);
                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                BACnetPacket respPkt = waitForPacket(5000);

            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
            }

            MarkDiagnosticSuccess();
        }
    }



    public class DiagnosticBIT00011ReadWildcardDevice : Diagnostic
    {
        // BTL Implementation Guide, Section 3.7
        //
        // Device instance number 4194303 reserved for reading a Device Objects Object_Identifier
        //
        // http://www.bacnetinternational.org/associations/8066/files/BTL%20Implementation%20Guidelines-v26.pdf
        string diagnosticName = "BIT00011 - Reading the Device Object using the 'wildcard' address";

        public DiagnosticBIT00011ReadWildcardDevice(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }
        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);

            // create a device object without instance number - this will create a wildcard instance
            BACnetObjectIdentifier oid = new BACnetObjectIdentifier(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);

            try
            {
                ClearIncomingPacketQueue();
                BACnetUtil.SendReadProperty(_apm, _bnm, devicTreeNode.device, oid, BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_IDENTIFIER);
                BACnetPacket pkt = waitForPacket(5000);

                if (pkt.pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK &&
                     pkt.propertyID == BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_IDENTIFIER)
                {
                    if (pkt.objectID != null && pkt.objectID.objectInstance == devicTreeNode.device.deviceObjectID.objectInstance)
                    {
                        MarkDiagnosticSuccess();
                    }
                    else
                    {
                        MarkDiagnosticFailed("Response received, but the Object ID is not that of the Device");
                    }
                }
                else
                {
                    MarkDiagnosticFailed("Proper Complex-ACK not received");
                }

            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }
        }
    }


    public class DiagnosticBIT00012deviceIDrange : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00012deviceIDrange(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00012 - Device ID Range";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);

            if ((this.devicTreeNode.device.deviceObjectID.objectInstance & 0x200000) == 0 ||
                   (this.devicTreeNode.device.deviceObjectID.objectInstance & 0x3ff) != this.devicTreeNode.device.vendorID.vendorID)
            {
                MarkDiagnosticFailed("Device ID not set to rules at www.bac-test.com");
            }
            else
            {
                MarkDiagnosticSuccess();
            }
        }
    }



    public class DiagnosticBIT00013whoIsRange1 : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00013whoIsRange1(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00013 - Who-Is, Device ID range - all";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);

            try
            {
                BACnetPacket resp;
                BACnetPacket pkt = new BACnetPacket(this._apm);
                pkt.EncodeNPCI(new DADR(), BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
                pkt.EncodeContextTag(0, 0);
                pkt.EncodeContextTag(1, BACnetEnums.BACNET_MAX_INSTANCE);

                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                // wait for _our_ device to respond
                do
                {
                    resp = waitForPacket(5000, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);
                    if (resp.srcDevice.deviceObjectID.Equals(devicTreeNode.device.deviceObjectID))
                    {
                        Sleep(5000);
                        ClearIncomingPacketQueue();
                        MarkDiagnosticSuccess();
                        return;
                    }
                }
                while (true);
            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }

        }
    }


    public class DiagnosticBIT00014whoIsRangeTargeted : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00014whoIsRangeTargeted(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00014 - Who-Is, Device ID range - targeted";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);

            bool foundFlag = false;

            try
            {
                BACnetPacket resp;
                BACnetPacket pkt = new BACnetPacket(this._apm);
                pkt.EncodeNPCI(new DADR(), BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
                pkt.EncodeContextTag(0, (int)devicTreeNode.device.deviceObjectID.objectInstance);
                pkt.EncodeContextTag(1, (int)devicTreeNode.device.deviceObjectID.objectInstance);

                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                // wait for _our_ device to respond
                do
                {
                    try
                    {
                        // We expect to timeout
                        resp = waitForPacket(5000, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);
                        if (resp.srcDevice.deviceObjectID.Equals(devicTreeNode.device.deviceObjectID))
                        {
                            foundFlag = true;
                        }
                        else
                        {
                            Sleep(5000);
                            ClearIncomingPacketQueue();
                            MarkDiagnosticFailed();
                            return;
                        }
                    }
                    catch (TimeoutException)
                    {
                        // remember we expect to time out
                        if (foundFlag == true)
                        {
                            MarkDiagnosticSuccess();
                            return;
                        }
                        else
                        {
                            MarkDiagnosticFailed();
                            return;
                        }
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }
        }
    }


    public class DiagnosticBIT00015whoIsBadMessage1 : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00015whoIsBadMessage1(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00015 - Who-Is, Bad Message 1";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);
            try
            {
                ClearIncomingPacketQueue();
                BACnetPacket pkt = new BACnetPacket(this._apm);
                pkt.EncodeNPCI(new DADR(), BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
                pkt.EncodeContextTag(0, (int)devicTreeNode.device.deviceObjectID.objectInstance);
                // Ha! Notice we did not encode the second part of the who-is. Devices should not respond.
                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                // wait for _our_ device to respond
                while ( true )
                {
                    try
                    {
                        // We expect to timeout
                        waitForPacket(5000, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);
                        ClearIncomingPacketQueue();
                        MarkDiagnosticFailed("There should be NO response from the device");
                        return;
                    }
                    catch (TimeoutException)
                    {
                        // remember we expect to time out
                        MarkDiagnosticSuccess();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }
        }
    }


    public class DiagnosticBIT00026whoIsBadMessage2 : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00026whoIsBadMessage2(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00015 - Who-Is, Bad Message 2";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);
            try
            {
                BACnetPacket pkt = new BACnetPacket(this._apm);
                pkt.EncodeNPCI(new DADR(), BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
                pkt.EncodeContextTag(0, (int)devicTreeNode.device.deviceObjectID.objectInstance);
                pkt.EncodeContextTag(0, (int)devicTreeNode.device.deviceObjectID.objectInstance); // Note the bad Context tag number
                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                // wait for _our_ device to respond
                while(true)
                {
                    try
                    {
                        // We expect to timeout
                        waitForPacket(5000, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);
                        ClearIncomingPacketQueue();
                        MarkDiagnosticFailed();
                        return;
                    }
                    catch (TimeoutException)
                    {
                        // remember we expect to time out
                        MarkDiagnosticSuccess();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }
        }
    }



    public class DiagnosticBIT00016whoIsBadMessage3 : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        public DiagnosticBIT00016whoIsBadMessage3(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00015 - Who-Is, Range reversed";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            _apm.MessageLog("Executing " + diagnosticName);
            try
            {
                BACnetPacket pkt = new BACnetPacket(this._apm);
                pkt.EncodeNPCI(new DADR(), BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
                pkt.EncodeContextTag(0, (int)devicTreeNode.device.deviceObjectID.objectInstance+1);
                pkt.EncodeContextTag(1, (int)devicTreeNode.device.deviceObjectID.objectInstance - 1);
                BACnetUtil.SendOffPacket(_apm, _bnm, pkt, pkt.buffer, pkt.optr);

                // wait for _our_ device to respond
                while ( true )
                {
                    try
                    {
                        // We expect to timeout
                        waitForPacket(5000, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);
                        ClearIncomingPacketQueue();
                        MarkDiagnosticFailed();
                        return;
                    }
                    catch (TimeoutException)
                    {
                        // remember we expect to time out
                        MarkDiagnosticSuccess();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // other types of exception...
                _apm.MessagePanic(ex.ToString());
                return;
            }
        }
    }













































}
