using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibraryNS
{
    public class OurSocket : Socket 
    {
        public int OurSocketPort;

        //public OutboundCopy outgoing_buffer_copy = new OutboundCopy();

        public OurSocket(AddressFamily af, SocketType st, ProtocolType pt, int port ) : base ( af, st, pt)
        {
            IPEndPoint local_ipep = new IPEndPoint(0, port);

            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            // bind the local end of the connection to BACnet port number
            base.Bind(local_ipep);

            OurSocketPort = port;
        }




    }
}
