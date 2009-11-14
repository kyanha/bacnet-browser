using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using BACnetLibraryNS;


namespace BACnetLibraryNS
{
    public class Device : IEquatable<Device>
    {
        #region Fields & Constants

        public BACnetEnums.DEVICE_TYPE type;

        public ADR adr;

        public myIPEndPoint directlyConnectedIPEndPointOfDevice ;           // this is the Router or the MAC address that is the IP address access to the device
        public myIPEndPoint siteIPEndPoint;

        // public uint SourceAddress;
        public uint I_Am_Count = 0;

        private uint vendorId;
        public BACnetObjectIdentifier deviceObjectID = new BACnetObjectIdentifier();              // note! This is NOT the same as the MAC address stored in dadr.


        public BACnetEnums.BACNET_SEGMENTATION SegmentationSupported;

        private int maxAPDULength;

        public BACnetPacket packet;   // a place to store the source IP address and port number

        #endregion

        #region Properties

        // Todo, make this return manufacturer names.
        public uint VendorId
        {
            get { return vendorId; }
            set { vendorId = value; }
        }

        #endregion


        public bool Equals(Device d)
        {
            // Devices are considered equal if their network numbers and MAC addresses match.

            if (this.adr == null && d.adr != null) return false;
            if (this.adr != null && d.adr == null) return false;
            if (this.adr != null && d.adr != null)
            {
                if (!this.adr.Equals(d.adr)) return false;
            }

#if TODO
            // commented this out 11/2 because of the above rule...

            if (!this.directlyConnectedIPEndPointOfDevice.Equals(d.directlyConnectedIPEndPointOfDevice)) return false;

            if (this.siteIPEndPoint == null && d.siteIPEndPoint != null) return false;
            if (this.siteIPEndPoint != null && d.siteIPEndPoint == null) return false;
            if (this.siteIPEndPoint != null && d.siteIPEndPoint != null)
            {
                if (!this.siteIPEndPoint.Equals(d.siteIPEndPoint)) return false;
            }

            if (this.directlyConnectedIPEndPointOfDevice == null && d.directlyConnectedIPEndPointOfDevice != null) return false;
            if (this.directlyConnectedIPEndPointOfDevice != null && d.directlyConnectedIPEndPointOfDevice == null) return false;
            if (this.directlyConnectedIPEndPointOfDevice != null && d.directlyConnectedIPEndPointOfDevice != null)
            {
                if (!this.directlyConnectedIPEndPointOfDevice.Equals(d.directlyConnectedIPEndPointOfDevice)) return false;
            }
#endif 

            return true;
            //todo, add check that device instances (Device Object IDs) are equal here too... ( as a sanity check)
        }

        public void parse(byte[] bytes)
        {
            byte[] temp = new byte[4];
            temp[0] = bytes[2];
            temp[1] = bytes[1];
            temp[2] = bytes[0];
            temp[3] = 0x00;
            // todo this.deviceId = BitConverter.ToInt32(temp, 0);

            //bytes[17];
            temp = new byte[2];
            temp[0] = bytes[19];
            temp[1] = bytes[18];
            this.maxAPDULength = BitConverter.ToInt16(temp, 0);

            //bytes[19];
            //bytes[20];

            //bytes[21];
            this.vendorId = bytes[22];
        }

        public void Send_IAm(BACnetmanager bnm)
        {
            byte[] data = new byte[1024];
            int optr = 0;
            int lengthOffset;

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

            if (adr.networkNumber == 0)
            {
                data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            }
            else
            {
                // else we emulate a forwarded NPDU
                data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU;
            }


            // Length (2 octets) Note: We derive length when message build is complete, and save the offset position here

            lengthOffset = optr;
            optr += 2;

            if (adr.networkNumber != 0)
            {
                // make space for the IP address (AKA MAC address)
                // todo, dummy it up for now
                data[optr++] = 192;
                data[optr++] = 168;
                data[optr++] = 1;
                data[optr++] = 11;
                data[optr++] = 0xBA;
                data[optr++] = 0xC0;
            }


            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;        // Always 1

            if (adr.networkNumber == 0)
            {
                data[optr++] = 0x20;        // Control (Destination present, no source)
            }
            else
            {
                // insert source net, slen, addr
                data[optr++] = 0x20 | 0x08;
            }

            // Destination (b'cast)
            data[optr++] = 0xff;        // DNET Network - B'cast
            data[optr++] = 0xff;
            data[optr++] = 0x00;        // DLEN


            if (adr.networkNumber != 0)
            {
                data[optr++] = (byte)((adr.networkNumber >> 8) & 0xff);
                data[optr++] = (byte)((adr.networkNumber) & 0xff);
                data[optr++] = 6;
                data[optr++] = 192;
                data[optr++] = 168;
                data[optr++] = 1;
                data[optr++] = 55;
                data[optr++] = 0xba;
                data[optr++] = 0xc0;
            }

            data[optr++] = 0xff;        // Hop count

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            data[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
            data[optr++] = 0x00;        // Unconfirmed Service Choice: I-Am

            // object identifier, device object 

            BACnetObjectIdentifier bnoid = new BACnetObjectIdentifier();

            bnoid.SetType(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);

            // todo - for now hard code the device ID
            bnoid.SetInstance(999);

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

        /*
                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("*********************");
                    builder.AppendLine("Vendor Id: " + this.vendorId);
                    builder.AppendLine("Device id: " + this.deviceId);
                    builder.AppendLine("Max APDU length: " + this.maxAPDULength);
                    builder.AppendLine("Segmentation supported: " + this.segmentationSupported);
                    builder.AppendLine("*********************");
                    return builder.ToString();
                }
         */
    }


    public class ServerDevice : Device
    {

        public ServerDevice( /*DeviceId,*/ uint NetworkNumber)
        {
            // constructor 

            //this.DeviceId = 999 ; // todo
            this.adr.networkNumber = adr.networkNumber;
        }
    }

}
