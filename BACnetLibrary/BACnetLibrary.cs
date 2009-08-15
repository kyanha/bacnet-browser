using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;



namespace BACnetLibraryNS
{


    public class BACnetSegmentation
    {

        public void Encode(byte[] buffer, ref uint pos)
        {
            buffer[pos++] = 0x91;
            buffer[pos++] = 0x03;
        }

        public void Decode(byte[] buffer, ref uint pos)
        {
            pos += 2;
        }
    }


    public class Unsigned
    {
        uint value;

        public Unsigned(uint value)
        {
            this.value = value;
        }

        public uint Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public void Encode(byte[] buffer, ref uint pos)
        {
            if (value <= 0xff)
            {
                buffer[pos++] = 0x21;
                buffer[pos++] = (byte)(value & 0xFF);
                return;
            }

            if (value > 0xff)
            {
                buffer[pos++] = 0x22;
                buffer[pos++] = (byte)(value / 0xFF & 0xFF);
                buffer[pos++] = (byte)(value & 0xFF);
                return;
            }
        }


        public void Decode(byte[] buffer, ref UInt16 pos)
        {
            byte tag = buffer[pos++];
            if (tag == 0x21)
            {
                value = buffer[pos++];
            }
            if (tag == 0x22)
            {
                value = (uint)buffer[pos++] + (uint)buffer[pos++] * 0x100;
            }

        }
    }



    public class BACnetObjectIdentifier
    {
        BACnetEnums.BACNET_OBJECT_TYPE objectType;
        uint objectInstance;

        public BACnetObjectIdentifier()
        {
        }

        public void SetType(BACnetEnums.BACNET_OBJECT_TYPE objectType)
        {
            // Todo, check legal values here...

            this.objectType = objectType;
        }

        public void SetInstance(uint objectInstance)
        {
            // todo, check for legal values, duplicates.

            this.objectInstance = objectInstance;
        }

        public void Encode(byte[] buffer, ref uint pos)
        {
            UInt32 objid;

            objid = ((UInt32)objectType << 22) | objectInstance;

            buffer[pos++] = 0xC4;   // application tag, length 4. TODO, build this properly

            buffer[pos++] = (byte)((objid >> 24) & 0xff);
            buffer[pos++] = (byte)((objid >> 16) & 0xff);
            buffer[pos++] = (byte)((objid >> 8) & 0xff);
            buffer[pos++] = (byte)(objid & 0xff);
        }


        public void Decode(byte[] buffer, ref UInt16 pos)
        {
            pos += 5;
        }
    }


    public class BACnetLibraryCL
    {

        public static void SendIAm(BACnetmanager bnm)
        {
            byte[] data = new byte[1024];
            uint optr = 0;
            uint lengthOffset;

            //            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.103"), 0xBAC0 );
            //            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, bnm.BACnetManagerPort);

            IPEndPoint local_ipep = new IPEndPoint(0, bnm.BACnetManagerPort);

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;



            // Length (2 octets) Note: We derive length when message build is complete, and save the offset position here

            lengthOffset = optr;
            optr += 2;


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
            data[optr++] = 0x00;        // Unconfirmed Service Choice: I-Am

            // object identifier, device object 

            BACnetObjectIdentifier bnoid = new BACnetObjectIdentifier();

            bnoid.SetType(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);

            if (bnm.mode == BACnetEnums.BACNET_MODE.BACnetClient)
            {
                bnoid.SetInstance(BACnetEnums.CLIENT_DEVICE_ID);
            }
            else
            {
                bnoid.SetInstance(BACnetEnums.SERVER_DEVICE_ID);
            }

            bnoid.Encode(data, ref optr);


            // Maximum APDU length (Application Tag, Integer)

            Unsigned apdulen = new Unsigned(1476);

            apdulen.Encode(data, ref optr);


            // Segmentation supported, (Application Tag, Enum)

            BACnetSegmentation bsg = new BACnetSegmentation();

            bsg.Encode(data, ref optr);


            // Vendor ID, (Application Tag, Integer)

            Unsigned vid = new Unsigned(323);

            vid.Encode(data, ref optr);

            data[lengthOffset] = (byte)((optr >> 8) & 0xff);
            data[lengthOffset + 1] = (byte)(optr & 0xff);

            bacnet_master_socket.SendTo(data, (int)optr, SocketFlags.None, ipep);
        }



        public static void SendWhoIs(BACnetmanager bnm)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            //            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.103"), 0xBAC0 );
            //            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, bnm.BACnetManagerPort);

            IPEndPoint local_ipep = new IPEndPoint(0, bnm.BACnetManagerPort);

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
            optr += 2;

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


            data[store_length_here] = 0;
            data[store_length_here + 1] = (byte)optr;

            // if there is no available adapter, this try will throw
            try
            {
                bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                return;
            }
        }



        public static void ReadProperties(BACnetmanager bnm)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            //--- this is the sender -----------------------------------
            //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(segIP), bnm.BACnetManagerPort);
            // hard coded for now is my devices IP address
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.30"), bnm.BACnetManagerPort);
            //--- this is the listener ------------------------------------
            IPEndPoint local_ipep = new IPEndPoint(0, bnm.BACnetManagerPort);

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;  // 81
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;  //0a
            data[optr++] = 0x00;        // Length (2 octets)
            data[optr++] = 0x1b;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;            //
            data[optr++] = 0x24;            //

            data[optr++] = 0x03;            //
            data[optr++] = 0xea;
            data[optr++] = 0x06;            //
            data[optr++] = 0xc0;            //

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU


            // Unconfirmed Request
            // Structure described here http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU

            data[optr++] = 0xa8;            // Encoded APDU type == 01 == Unconfirmed Request
            data[optr++] = 0x01;            // Unconfirmed Service Choice: Who-Is
            //  Service Choice: readProperty (12)

            data[optr++] = 0xcb;            //
            data[optr++] = 0xba;            //


            data[optr++] = 0xc0;            //
            data[optr++] = 0xff;            //
            data[optr++] = 0x02;            //
            data[optr++] = 0x04;            //
            data[optr++] = 0x01;            //  Invoke ID
            data[optr++] = 0x0c;            //
            data[optr++] = 0x0c;            //
            data[optr++] = 0x02;            //



            data[optr++] = 0x00;            //
            data[optr++] = 0x01;            //  this = 256    000000001
            data[optr++] = 0xf4;            //  this = 128    111110100  // this is my devices instance


            data[optr++] = 0x19;            //  19  Context Tag: 1, Length/Value/Type: 1
            data[optr++] = 0x4c;            //  4c  Property Identifier: object-list (76)

            // if there is no available adapter, this try will throw
            try
            {
                bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                return;
            }
            //  sending another who-is, this time with SNET, SADR present..

            optr = 0;

            bacnet_master_socket.Close();

        }
    }
}
