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
using System.Net;
using System.Net.Sockets;

namespace BACnetLibrary
{
    public class OurSocket : Socket
    {
        AppManager _apm;

        public int ourSocketPort;

        public List<BACnetPacket> outgoing_buffer_copy_queue = new List<BACnetPacket>();


        //public OurSocket(AppManager apm, AddressFamily af, SocketType st, ProtocolType pt)
        //    : base(af, st, pt)
        //{
        //    _apm = apm;
        //}

        //public OurSocket(AppManager apm, AddressFamily af, SocketType st, ProtocolType pt, int port)
        //    : base(af, st, pt)
        //{
        //    _apm = apm;

        //    IPEndPoint local_ipep = new IPEndPoint(0, port);

        //    base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //    base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        //    // bind the local end of the connection to BACnet port number
        //    base.Bind(local_ipep);

        //    ourSocketPort = port;
        //}


        public OurSocket(AppManager apm, int port)
            : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            _apm = apm;

            IPEndPoint local_ipep = new IPEndPoint(0, port);

            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            // bind the local end of the connection to BACnet port number
            base.Bind(local_ipep);

            ourSocketPort = port;
        }


        public int OurSendTo(byte[] outbuf, int sz, IPEndPoint ipep)
        {

            try
            {
                return base.SendTo(outbuf, sz, SocketFlags.None, ipep);
            }
            catch
            {
                _apm.MessageLog("m0035 - Unable to send packet to " + ipep.ToString());
                throw new Exception("m0171 - What happens next?");
            }
        }


        public bool detect_echo_packet(BACnetManager bnm, BACnetPacket packet)
        {
            foreach (IPAddress ipa in bnm.OurIPAddressList)
            {
                if (packet.directlyConnectedIPEndPointOfDevice.Address.Equals(ipa))
                {
                    // when the sent IP address matches one of ours, check the contents of the packet against the packets stored in the outbound copy queue

                    //    // remove all expired packets
                    //    foreach (BACnetPacket pkt in outgoing_buffer_copy_queue)
                    //    {
                    //        if (pkt.timestamp + 5000 < bnm._stopWatch.ElapsedMilliseconds)
                    //        {
                    //            // drop it
                    //            outgoing_buffer_copy_queue.Remove(pkt);
                    //            // and quit from this loop, since foreach may fail...
                    //            // todo, find a better way to remove all > 5000 ms items
                    //            break;
                    //        }
                    //    }

                    //    if (outgoing_buffer_copy_queue.Count > 100)
                    //    {
                    //        // time to panic
                    //        // todo Console.WriteLine("Outbound copy queue overflow");
                    //        outgoing_buffer_copy_queue.Clear();
                    //        return false;
                    //    }

                    //    if (outgoing_buffer_copy_queue.Contains(packet))
                    //    {
                    //        // Console.WriteLine("This message is from ourselves");

                    //        // inform that the packet was a match
                    //        return true;
                    //    }
                    //}

                    // todo - drop a warning...
                    return true;
                }
            }
            return false;
        }
    }
}
