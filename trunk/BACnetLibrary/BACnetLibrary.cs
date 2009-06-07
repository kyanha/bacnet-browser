using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;



namespace BACnetLibraryNS
{

    public class BACnetLibraryCL
    {

        public static void SendWhoIs()
        {
            byte[] data = new byte[1024];
            int optr = 0;

            //            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.103"), 0xBAC0 );
            //            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC1);// NOTE, NOT BAC0, so that we can run IUT bacnet server on same machine

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            data[optr++] = 0x00;        // Length (2 octets)
            data[optr++] = 0x0c;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Always 1
            data[optr++] = 0x20;        // Control (Destination present, no source)
            data[optr++] = 0xff;        // DNET Network - B'cast
            data[optr++] = 0xff;
            data[optr++] = 0x00;        // DLEN
            data[optr++] = 0xff;        // Hop count

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            data[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
            data[optr++] = 0x08;        // Unconfirmed Service Choice: Who-Is

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

    }
}
