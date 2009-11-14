using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace BACnetLibraryNS
{
    public class OurSocket : Socket 
    {
        public int OurSocketPort;

        public Stopwatch _socketTimer = Stopwatch.StartNew();

        public List<BACnetPacket> outgoing_buffer_copy_queue = new List<BACnetPacket>();

        public OurSocket(AddressFamily af, SocketType st, ProtocolType pt, int port ) : base ( af, st, pt)
        {
            IPEndPoint local_ipep = new IPEndPoint(0, port);

            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            // bind the local end of the connection to BACnet port number
            base.Bind(local_ipep);

            OurSocketPort = port;
        }

        public bool detect_echo_packet( BACnetmanager bnm, BACnetPacket packet)
        {
            foreach (IPAddress ipa in bnm.OurIPAddressList)
            {
                if (packet.fromBIP.Address.Equals(ipa))
                {
                    // when the sent IP address matches one of ours, check the contents of the packet against the packets stored in the outbound copy queue

                    // remove all expired packets
                    foreach (BACnetPacket pkt in outgoing_buffer_copy_queue)
                    {
                        if (pkt.timestamp + 5000 < _socketTimer.ElapsedMilliseconds)
                        {
                            // drop it
                            outgoing_buffer_copy_queue.Remove(pkt);
                        }
                    }

                    if (outgoing_buffer_copy_queue.Count > 100)
                    {
                        // time to panic
                        Console.WriteLine("Outbound copy queue overflow");
                        outgoing_buffer_copy_queue.Clear();
                        return false;
                    }

                    if ( outgoing_buffer_copy_queue.Contains ( packet ) )
                    {
                        Console.WriteLine("This message is from ourselves");
                        
                        // inform that the packet was a match
                        return true ;
                    }
                }
            }
            return false;
        }
    }
}
