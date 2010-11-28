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
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibrary
{


    public class OutboundCopy
    {
        public uint length;
        public byte[] buffer;
    }


    public class BACnetListener
    {
        AppManager _apm;
        BACnetManager bnm;

        public OurSocket listen_socket;

        public BACnetListener(AppManager apm, BACnetManager bnm)
        {
            _apm = apm;
            this.bnm = bnm;
        }

        public void BACnetListenerClose()
        {
            try
            {
                listen_socket.Shutdown(SocketShutdown.Both);
                listen_socket.Close();
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe);
            }
        }


        // This method that will be called when the thread is started
        public void BACnetInsideListener()
        {
            while (true)
            {
                // Byte[] received = new Byte[2000];
                BACnetPacket packet = new BACnetPacket(_apm);

                // Create an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                try
                {
                    // bacnet_listen_socket.Receive(received);
                    packet.length = listen_socket.ReceiveFrom(packet.buffer, ref senderRemote);

                    // Console.WriteLine("This message was sent from " + ((IPEndPoint)senderRemote).Address.ToString() + "  Port " + ((IPEndPoint)senderRemote).Port.ToString());
                    // packet.fromBIP = (IPEndPoint) senderRemote;
                    packet.directlyConnectedIPEndPointOfDevice = new myIPEndPoint();
                    packet.directlyConnectedIPEndPointOfDevice.Port = ((IPEndPoint)senderRemote).Port;
                    packet.directlyConnectedIPEndPointOfDevice.Address = ((IPEndPoint)senderRemote).Address;

                    // if the packet is from ourselves, discard
                    if (listen_socket.detect_echo_packet(bnm, packet) == true)
                    {
                        continue;
                    }

                    // todo - one day, we will be able to see if this packet was addressed to this host in a broadcast packet, and mark the dadr accordingly, that is, if we care.

                    // todo - remove directlyconnected
                    //packet.srcDevice.adr = new ADR ( packet.directlyConnectedIPEndPointOfDevice ) ;

                    // Make an undecoded copy of the packet for the application layer. This is to ease debugging in the asynch application layer.
                    BACnetPacket appPkt = (BACnetPacket) packet.Clone();

                    // packet.buffer = received;
                    packet.DecodeBACnet();

                    // extract some information from the packet for our caches

                    if (packet.sAdr != null)
                    {
                        lock (_apm.internalRouterInfo)
                        {
                            _apm.internalRouterInfo.AddRoutingTableEntry(packet.sAdr.networkNumber);
                        }
                    }
                    if (packet.routerTableList != null)
                    {
                        lock (_apm.internalRouterInfo)
                        {
                            foreach (RoutingTableEntry re in packet.routerTableList)
                            {
                                // surrounding this because we know of at least one router out there that has illegal network numbers
                                try
                                {
                                    _apm.internalRouterInfo.AddRoutingTableEntry(re.networkNumber);
                                }
                                catch (ProtocolException pe)
                                {
                                    pe.DumpException(_apm);
                                }
                            }
                        }
                    }


                    // display the decoded packet in the UI treeview
                    bnm.newPacketQueue.myEnqueue(packet);

                    // todo, if diagnostics are not running, then this queue will overflow.... hence the check.. find a cleaner method..
                    if (_apm.pktQueueToApplication.Count < 1000)
                    {
                        _apm.pktQueueToApplication.myEnqueue(appPkt);   // notice, this is not a copy of the packet, it is a pointer to the same packet.... kick me if there are problems one day
                    }
                }
                catch (ProtocolException pe)
                {
                    pe.DumpException(_apm);
                }
                catch (Exception efe)
                {
                    // need to catch the inevitable exception when this blocking call is cancelled by the shutdown code
                    Console.WriteLine(efe);
                    _apm.MessagePanic(efe.ToString());
                }
            }

        }

    }
}
