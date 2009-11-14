using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


namespace BACnetLibraryNS
{

    public class myIPEndPoint : IPEndPoint
    {

        public myIPEndPoint(IPAddress address, int port) 
            : base ( address, port )
        {
        }

        public myIPEndPoint()
            : base(IPAddress.Any, 0)
        {
        }

        // public IPEndPoint ipep = new IPEndPoint(0,0);

        public void Decode(byte[] buffer, int offset)
        {
            int port = 0;

            byte[] bytearr = new byte[4];

            Buffer.BlockCopy(buffer, offset, bytearr, 0, 4);

            port = buffer[offset + 4];
            port = (port << 8) | buffer[offset + 5];

            base.Address = new IPAddress(bytearr);
            base.Port = port;

        }

        public void Encode(byte[] buffer, int offset)
        {
            int offsetcount = offset;

            byte[] addrb = base.Address.GetAddressBytes();

            for (int i = 0; i < 4; i++)
            {
                buffer[offsetcount++] = addrb[i];
            }

            buffer[offsetcount++] = (byte)((Port >> 8) & 0xff);
            buffer[offsetcount++] = (byte)(Port & 0xff);
        }
    }

    public class BACnetSegmentation
    {

        public void Encode(byte[] buffer, ref int pos)
        {
            buffer[pos++] = 0x91;
            buffer[pos++] = 0x03;
        }

        public void Decode(byte[] buffer, ref int pos)
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

        public void Encode(byte[] buffer, ref int pos)
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


    public class MACaddress : IEquatable<MACaddress>
    {
        public uint length;
        public uint uintMACaddress;
        public myIPEndPoint ipMACaddress;

        public MACaddress()
        {
        }

        public MACaddress ( uint madr )
        {
            length = 1;
            uintMACaddress = madr;
        }

        public MACaddress(myIPEndPoint madr)
        {
            length = 6;
            ipMACaddress = madr;
        }

        public bool Equals(MACaddress madr)
        {
            if (length != madr.length) return false;

            switch (length)
            {
                case 0:
                    break;
                case 1:
                    if (madr.uintMACaddress != uintMACaddress) return false;
                    break;
                case 6:
                    if (ipMACaddress.Equals(madr.ipMACaddress) != true) return false;
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            switch (length)
            {
                case 0:
                    return "Broadcast";
                    break;
                case 1:
                    return uintMACaddress.ToString();
                    break;
                case 6:
                    return ipMACaddress.ToString();
                    //return "todo"; 
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC length");
                    return "Illegal MAC address";
                    break;
            } 
        }
    }


    public class ADR : IEquatable<ADR>
    {
        // todo - does a network number of 0 have any special meaning? how do we indicate a directly connected device?

        public uint networkNumber;
        public MACaddress MACaddress ;
        public bool directlyConnected = false ;

        public ADR()
        {
            this.MACaddress = new MACaddress();
        }

        public ADR(uint networkNumber, uint MACaddr)
        {
            this.MACaddress = new MACaddress ( MACaddr ) ;
            this.networkNumber = networkNumber;
        }

        public ADR(uint networkNumber, myIPEndPoint ipep)
        {
            this.networkNumber = networkNumber;

            if (networkNumber == 0)
            {
                // we can argue whether a network number of 0 indicates directly connected or not later... for now we will
                // assume no network number == directly connected
                directlyConnected = true;
            }

            this.MACaddress = new MACaddress ( ipep ) ;
        }


        public bool Equals(ADR adr)
        {
            if (this.directlyConnected != adr.directlyConnected) return false;
            if (this.networkNumber != adr.networkNumber) return false;
            if (this.MACaddress.Equals (adr.MACaddress ) != true ) return false;
            return true;
        }


        public void Decode(byte[] buffer, ref int pos)
        {
            networkNumber = (uint)buffer[pos++] << 8;
            networkNumber |= buffer[pos++];

            MACaddress.length = buffer[pos++];

            switch (MACaddress.length)
            {
                case 0:
                    // indicates a broadcast, perfectly legal.
                    break;
                case 1:
                    MACaddress.uintMACaddress = buffer[pos++];
                    break;
                case 6:
                    // extract the IP address
                    myIPEndPoint ipep = new myIPEndPoint();
                    ipep.Decode(buffer, pos);
                    MACaddress.ipMACaddress = ipep;
                    pos += 6;
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    break;
            }
        }

        public void Encode(byte[] buffer, ref int pos)
        {

            buffer[pos++] = (byte)(this.networkNumber >> 8);
            buffer[pos++] = (byte)(this.networkNumber & 0xff);

            buffer[pos++] = (byte)MACaddress.length;

            switch (MACaddress.length)
            {
                case 1:
                    buffer[pos++] = (byte)MACaddress.uintMACaddress;
                    break;
                case 6:
                    MACaddress.ipMACaddress.Encode(buffer, pos);
                    pos += 6;
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    break;
            }
        }
    }



    public class BACnetObjectIdentifier : IEquatable<BACnetObjectIdentifier>
    {
        public BACnetEnums.BACNET_OBJECT_TYPE objectType;
        public uint objectInstance;

        public BACnetObjectIdentifier()
        {
        }

        public bool Equals(BACnetObjectIdentifier oid)
        {
            if (oid.objectType != this.objectType) return false;
            if (oid.objectInstance != this.objectInstance) return false;
            return true;
        }

        public BACnetObjectIdentifier(byte[] buf, ref int offset)
        {
            Decode(buf, ref offset);
        }




        // Create a new Application Tag

        public BACnetObjectIdentifier(byte[] buf, ref int offset, BACnetEnums.TAG tagType, BACnetEnums.BACNET_APPLICATION_TAG appTag)
        {
            // is the next parameter even an application tag 
            if ((buf[offset] & 0x08) != 0x00)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            if ((BACnetEnums.BACNET_APPLICATION_TAG)(((buf[offset] & 0xf0) >> 4)) != appTag)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unexpected application tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            int contextTagSize = buf[offset] & 0x07;

            offset++;

            switch (appTag)
            {
                case BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                    if (contextTagSize != 4)
                    {
                        // we dont have a legal object ID!
                        BACnetLibraryCL.Panic("Illegal length");
                        return;
                    }

                    this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buf[offset] << 2) | ((uint)buf[offset + 1] >> 6));

                    objectInstance = ((uint)buf[offset + 1] & 0x3f) << 16;
                    objectInstance |= ((uint)buf[offset + 2]) << 8;
                    objectInstance |= ((uint)buf[offset + 3]);

                    offset += 4;
                    return;
            }
        }




            
            // Create a new Context Tag

        public BACnetObjectIdentifier(byte[] buf, ref int offset, BACnetEnums.TAG tagType, int tagValue)
        {
            // is the next parameter even a context tag 
            if ((buf[offset] & 0x08) != 0x08 )
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            if ((buf[offset] & 0xf0) != (tagValue << 4))
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unexpected context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return; 
            }

            int contextTagSize = buf[offset] & 0x07;

            // the length of a bacnet object identifier better be 4

            if ( contextTagSize != 4 )
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unbelievable length of object identifier");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }


            objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buf[offset+1] << 2) | ((uint)buf[offset + 2] >> 6));

            objectInstance = ((uint)buf[offset + 2] & 0x3f) << 16;
            objectInstance |= ((uint)buf[offset + 3]) << 8;
            objectInstance |= ((uint)buf[offset + 4]);

            offset += 5;
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

        public void Encode(byte[] buffer, ref int pos)
        {
            UInt32 objid;

            objid = ((UInt32)objectType << 22) | objectInstance;

            // buffer[pos++] = 0xC4;   // application tag, length 4. TODO, build this properly
            buffer[pos++] = 0x0c;       // context tag, TODO, understand differenct between application and context tags!!

            buffer[pos++] = (byte)((objid >> 24) & 0xff);
            buffer[pos++] = (byte)((objid >> 16) & 0xff);
            buffer[pos++] = (byte)((objid >> 8) & 0xff);
            buffer[pos++] = (byte)(objid & 0xff);
        }


        public void Decode(byte[] buffer, ref int pos)
        {
            // get the tag class, length

            uint cl = buffer[pos++];

            if (cl != 0xc4)
            {
                MessageBox.Show("Todo - dont know how to handle this yet");
                pos += 4;
                return;
            }

            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }
    }


    public class BACnetLibraryCL
    {
        public static void Panic(String message)
        {
            Debug.Assert(false, message);
            // todo - do a throw here. so we can recover best we can
        }



        static public int ExtractInt16(byte[] buffer, ref int iptr)
        {
            int tint = 0;
            tint = (int)buffer[iptr++] << 8;
            tint |= (int)buffer[iptr++];
            return tint;
        }

        static public uint ExtractUint16(byte[] buffer, ref int iptr)
        {
            uint tint = 0;
            tint = (uint)buffer[iptr++] << 8;
            tint |= (uint)buffer[iptr++];
            return tint;
        }

        static public uint ExtractUint32(byte[] buffer, ref int iptr)
        {
            uint tint = 0;
            tint = (uint)buffer[iptr++] << 24;
            tint |= (uint)buffer[iptr++] << 16;
            tint |= (uint)buffer[iptr++] << 8;
            tint |= (uint)buffer[iptr++] ;
            return tint;
        }

        static public void InsertInt16(byte[] buffer, ref int optr, int val)
        {
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }

        static public void InsertUint16(byte[] buffer, ref int optr, uint val)
        {
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }

        static public void InsertUint32(byte[] buffer, ref int optr, uint val)
        {
            buffer[optr++] = (byte)((val >> 24) & 0xff);
            buffer[optr++] = (byte)((val >> 16) & 0xff);
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }

        public static void SendIAm(BACnetmanager bnm)
        {
            byte[] data = new byte[1024];
            int optr = 0;
            int lengthOffset;

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



        public static void SendWhoIs(BACnetmanager bnm, bool include_dnet, IPEndPoint destination)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Always 1

            if (include_dnet == true)
            {
                data[optr++] = 0x20;        // Control (Destination present, no source)
                data[optr++] = 0xff;        // DNET Network - B'cast
                data[optr++] = 0xff;
                data[optr++] = 0x00;        // DLEN
                data[optr++] = 0xff;        // Hop count
            }
            else
            {
                data[optr++] = 0x00;        // Control (No DNET, no SNET )
            }

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            data[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
            data[optr++] = 0x08;        // Unconfirmed Service Choice: Who-Is

            data[store_length_here] = 0;
            data[store_length_here + 1] = (byte)optr;

            // if there is no available adapter, this try will throw
            try
            {
                bnm.insidesocket.SendTo(data, optr, SocketFlags.None, destination);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                return;
            }
        }



        public static void SendWhoIs(BACnetmanager bnm, Device device)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;  // 81
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;  //0a

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Always 1
            data[optr++] = 0x20;        // Control (Destination present, no source)

            // Encode network number, mac address of the device
            device.adr.Encode(data, ref optr);

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
                bnm.insidesocket.SendTo(data, (int)optr, SocketFlags.None, device.packet.fromBIP);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                return;
            }
        }



        public static void ReadProperties(BACnetmanager bnm, Device device)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;  // 81
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;  //0a

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;            //  Version
            data[optr++] = 0x24;            //  NCPI - Dest present, expecting reply

            if (device.adr != null)
            {
                device.adr.Encode(data, ref optr);
            }
            else
            {
                // this means we have an ethernet/IP address for a MAC address. At present
                // we then dont know the network number
                // todo - resolve the network number issue
                ADR tempAdr = new ADR(0, device.directlyConnectedIPEndPointOfDevice);
                tempAdr.Encode(data, ref optr);
            }

            data[optr++] = 0xff;            // Hopcount


            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU


            // Unconfirmed Request
            // Structure described here http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU

            data[optr++] = 0x02;            //  PDU Type=0 and SA=1
            data[optr++] = 0x04;            //  Max Resp (Encoded)


            data[optr++] = 0x01;            //  Invoke ID

            data[optr++] = 0x0c;            //  Service Choice 12 = ReadProperty

            // Service Request 

            // Object type, instance (Device Object)
#if false
            data[optr++] = 0x0c;            //
            data[optr++] = 0x02;            //
            data[optr++] = 0x01;            //
            data[optr++] = 0x1d;            // 
            data[optr++] = 0x2c;            //  
#else
            device.deviceObjectID.Encode(data, ref optr);
#endif

            // Property Identifier (Object List)

            data[optr++] = 0x19;            //  19  Context Tag: 1, Length/Value/Type: 1
            data[optr++] = 0x4c;            //  4c  Property Identifier: object-list (76)


            data[store_length_here] = (byte)((optr >> 8) & 0xff);
            data[store_length_here + 1] = (byte)(optr & 0xff);


            // if there is no available adapter, this try will throw
            try
            {
                bnm.insidesocket.SendTo(data, (int)optr, SocketFlags.None, device.packet.fromBIP);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                return;
            }

        }
    }
}
