using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibraryNS
{
    public class RouterHandlerCL
    {
        static public void RouterProcessPacket( Packet packet )
        {




        //            // if we are a router, forward this message

        //            hopcount--;

        //            // rebuild the BVLC, NPDU etc before passing on

        //            byte[] outbuf = new byte[2000];
        //            int optr = 0;
        //            int iptr = 0;
        //            int RouterOuputPort;

        //            if (bnm.BACnetManagerPort == 0xbac1)
        //            {
        //                RouterOuputPort = 0xbac0;
        //            }
        //            else
        //            {
        //                RouterOuputPort = 0xbac1;
        //            }

        //            // chose the OTHER port to send on.

        //            //IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, RouterOuputPort);

        //            //IPEndPoint local_ipep = new IPEndPoint(0, RouterOuputPort);

        //            //Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //            //bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        //            //bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        //            //// bind the local end of the connection to BACnet port number
        //            //bacnet_master_socket.Bind(local_ipep);

        //            //// BVLC Part
        //            //// http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

        //            //data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
        //            //data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_DISTRIBUTE_BROADCAST_TO_NETWORK;
        //            //data[optr++] = 0x00;        // Length (2 octets)
        //            //data[optr++] = 0x0c;

        //            //// Start of NPDU
        //            //// http://www.bacnetwiki.com/wiki/index.php?title=NPDU

        //            //data[optr++] = 0x01;        // Always 1
        //            //data[optr++] = 0x20;        // Control (Destination present, no source)
        //            //data[optr++] = 0xff;        // DNET Network - B'cast
        //            //data[optr++] = 0xff;
        //            //data[optr++] = 0x00;        // DLEN
        //            //data[optr++] = 0xff;        // Hop count

        //            //// APDU start
        //            //// http://www.bacnetwiki.com/wiki/index.php?title=APDU

        //            //data[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
        //            //data[optr++] = 0x08;        // Unconfirmed Service Choice: Who-Is

        //            //// if there is no available adapter, this try will throw
        //            //try
        //            //{
        //            //    bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);
        //            //}
        //            //catch (SocketException)
        //            //{
        //            //    System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
        //            //    return;
        //            //}


        //        }
        //    }
                   

        }
    }
}
