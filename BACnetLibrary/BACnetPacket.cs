using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BACnetLibraryNS
{


    public class BACnetPacket : IEquatable<BACnetPacket>
    {
        AppManager _apm;

        public int Source_Port;
        public byte[] buffer;
        public int length;          // for incoming messages
        public int optr = 0;       // for buildingoutgoing messages

        public int npdu_offset;
        public int nsdu_offset = 0;

        // APDU Parameters
        // See: http://www.bacnetwiki.com/wiki/index.php?title=PDU_Type

        public int apdu_offset;
        public int apdu_length;
        public BACnetEnums.BACNET_PDU_TYPE pduType;
        public BACnetEnums.BACNET_UNCONFIRMED_SERVICE unconfirmedServiceChoice;
        public BACnetEnums.BACNET_CONFIRMED_SERVICE confirmedServiceChoice;
        public BACnetEnums.BACNET_PROPERTY_ID propertyID;
        public List<BACnetObjectIdentifier> objectList;


        // NPDU parameters
        public NPDU npdu = new NPDU();
        public List<uint> numberList;
        public List<RouterPort> routerPortList;


        // various objects that will be created depending on the packet received
        //public ADR sadr;
        //public ADR dadr;
        public Device srcDevice = new Device();
        public Device dstDevice = new Device();

        // todo add hopcount to npdu class
        public uint hopcount;
        public long timestamp;

        // public bool apdu_present, is_broadcast, expecting_reply;
        public bool apdu_present;

        public myIPEndPoint fromBIP;


        public BACnetPacket(AppManager apm)
        {
            _apm = apm;
            apdu_present = false;
            npdu.isBroadcast = false;
        }

        public bool Equals(BACnetPacket packetToCompare)
        {
            if (this.length == packetToCompare.length)
            {
                // well, lengths are the same, is the data??

                int i;

                for (i = 0; i < this.length; i++)
                {
                    if (this.buffer[i] != packetToCompare.buffer[i])
                    {
                        // mismatch
                        return false;
                    }
                }
            }
            return true;
        }

        public bool DecodeBACnet(byte[] buffer, int offset)
        {
            // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
            ADR sadr = null;
            ADR dadr = null;

            if (buffer[offset] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
            {
                // todo3, panic log
                Console.WriteLine("Not a BACnet/IP message");
                return false;
            }


            // if it is a BACnet message, it _must_ have come from some device, however sketchily defined...
            // srcDevice = new Device();


            // we could receive an original broadcast, a unicast, a forwarded here...
            // right now
            // BVLC Function Types. See http://www.bacnetwiki.com/wiki/index.php?title=BVLC_Function

            switch ((BACnetEnums.BACNET_BVLC_FUNCTION)buffer[offset + 1])
            {
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU:
                    npdu_offset = offset + 10;
                    break;
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU:
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU:
                    // all these OK
                    npdu_offset = offset + 4;
                    break;
                default:
                    BACnetLibraryCL.Panic("nosuch bvlc function");
                    break;
            }


            // Investigate the NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            if (buffer[npdu_offset] != BACnetEnums.BACNET_PROTOCOL_VERSION)
            {
                // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                BACnetLibraryCL.Panic("BVLC microprotocol problem");
                return false;
            }

            // expecting reply?

            if ((buffer[npdu_offset + 1] & 0x04) == 0x04)
            {
                npdu.expectingReply = true;
            }


            // destination address present?
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            if ((buffer[npdu_offset + 1] & 0x20) == 0x20)
            {

                //da_present = true;
                dadr = new ADR();

                // dnet, dadr and hop count present

                int dAddrOffset = npdu_offset + 2;
                dadr.Decode(buffer, ref dAddrOffset);

                //packet.dnet = (uint)((buffer[packet.npdu_offset + 2] << 8) + buffer[packet.npdu_offset + 3]);
                //packet.dadr_len = (uint)(buffer[packet.npdu_offset + 4]);

                if (dadr.MACaddress.length == 0)
                {
                    npdu.isBroadcast = true;

                    // broadcast, but check the DNET
                    if (dadr.networkNumber != 0xffff)
                    {
                        System.Windows.Forms.MessageBox.Show("Broadcast according to DLEN, but DNET not 0xffff");
                        // todo, this means directed broadcast, need to deal with this still
                        return false;
                    }
                }
                if (dadr.MACaddress.length != 1 && dadr.MACaddress.length != 6 && dadr.MACaddress.length != 0)
                {
                    // panic
                    System.Windows.Forms.MessageBox.Show("Unexpected DADR len");
                    return false;
                }

                // todo, pick up variable length destination address

            }


            // Source address present?
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            if ((buffer[npdu_offset + 1] & 0x08) == 0x08)
            {
                //sa_present = true;

                sadr = new ADR();

                int sa_offset = npdu_offset + 2;

                // however, if there is a destination address, move the sa_offset up appropriate amount

                if (dadr != null)
                {
                    sa_offset = npdu_offset + 2 + 3 + (int)dadr.MACaddress.length;
                }

                // means SADR, SNET present

                sadr.Decode(buffer, ref sa_offset);

            }
            else
            {
                // at this point, if SADR not discovered within the NPDU, then the SADR MAC address is the Ethernet/IP fromaddress
                // and the device can (must) be considered 'direcly connected'
                if (fromBIP != null)
                {
                    srcDevice.adr = new ADR(0, fromBIP);
                }
                else
                {
                    BACnetLibraryCL.Panic("No from address can be determined");
                }
            }



            if (dadr != null)
            {
                if (sadr != null)
                {
                    hopcount = (uint)(buffer[npdu_offset + 2 + 2 + 1 + dadr.MACaddress.length + 2 + 1 + sadr.MACaddress.length]);
                }
                else
                {
                    hopcount = (uint)(buffer[npdu_offset + 2 + 2 + 1 + dadr.MACaddress.length]);
                }
                // true broadcast, but check the hopcount

                if (hopcount == 0)
                {
                    // out of hops, should never happen to us, so sound a warning
                    // todo, remove this for a functioning systems
                    System.Windows.Forms.MessageBox.Show("Hopcount of 0 detected");
                    return false;
                }
            }

            // finished resolving sadr, and dadr. Now populate our devices as required

            if (sadr != null)
            {
                if (srcDevice.adr != null)
                {
                    // means this adr was partially populated elsewhere.
                    srcDevice.adr.networkNumber = sadr.networkNumber;
                    srcDevice.adr.MACaddress = sadr.MACaddress;
                }
                else
                {
                    srcDevice.adr = sadr;
                }
            }


            if (dadr != null)
            {
                if (dstDevice.adr != null)
                {
                    // means this adr was partially populated elsewhere.
                    dstDevice.adr.networkNumber = dadr.networkNumber;
                    dstDevice.adr.MACaddress = dadr.MACaddress;
                }
                else
                {
                    dstDevice.adr = dadr;
                }
            }



            if ((buffer[npdu_offset + 1] & 0x80) == 0x80)  // todo magic number
            {
                // NSDU contains Network Layer Message
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                apdu_present = false;
                npdu.isNPDUmessage = true;

                // calculate offset to the NSDU

                nsdu_offset = npdu_offset + 2;

                if (sadr != null) nsdu_offset += 2 + 1 + (int)sadr.MACaddress.length;
                if (dadr != null) nsdu_offset += 2 + 1 + (int)dadr.MACaddress.length + 1;

                npdu.function = (BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE)buffer[nsdu_offset];

                switch (npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        // there may be a list of network numbers after the header

                        if ((length - nsdu_offset >= 3))
                        {
                            numberList = new List<uint>();
                        }

                        for (int i = 0; i < (length - nsdu_offset - 1) / 2; i++)
                        {
                            int tref = nsdu_offset + 1 + i * 2 ;
                            numberList.Add( BACnetLibraryCL.ExtractUint16 ( buffer, ref tref ));
                        }
                        break;

                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                        int tiptr = nsdu_offset + 1;
                        int numPorts = buffer[tiptr++];
                        routerPortList = new List<RouterPort>() ;

                        for (int i = 0; i < numPorts; i++)
                        {
                            RouterPort rp = new RouterPort();
                            rp.Decode(buffer, ref tiptr);
                            routerPortList.Add(rp);
                        }

                        Console.WriteLine("Ack");
                        break;

                    default:
                        // todo
                        break;
                }
            }
            else
            {
                // NSDU contains APDU
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                // determine if SNET, SLEN, SADR present

                apdu_present = true;

                apdu_offset = npdu_offset + 2;

                if (sadr != null) apdu_offset += 2 + 1 + (int)sadr.MACaddress.length;
                if (dadr != null) apdu_offset += 2 + 1 + (int)dadr.MACaddress.length + 1;

                apdu_length = length - apdu_offset;


                // the offset here is the APDU. Start parsing APDU.

                pduType = (BACnetEnums.BACNET_PDU_TYPE)(buffer[apdu_offset] & 0xf0);

                switch (pduType)
                {
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                        unconfirmedServiceChoice = (BACnetEnums.BACNET_UNCONFIRMED_SERVICE)buffer[apdu_offset + 1];
                        switch (unconfirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                                Console.WriteLine("Who-is");
                                break;
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:

                                // I-Am described right here: http://www.bacnetwiki.com/wiki/index.php?title=I-Am

                                // if (sadr != null) srcDevice.adr = sadr;

                                // first encoded entity is the Device Identifier...
                                // Encoding described here: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

                                // Decode Device Identifier

                                //uint offset = BACnetEncoding.BACnetDecode_uint(bytes, packet.apdu_offset + (uint)2, out deviceId);

                                //offset += packet.apdu_offset + (uint)2;


                                // todo - brute force mask off the object type field 
                                //deviceId &= 0x03fffff;

                                offset = apdu_offset + 2;

                                srcDevice.deviceObjectID.Decode(buffer, ref offset);
                                Console.WriteLine("This is device: " + srcDevice.deviceObjectID);

                                // todo, for now, we will ignore device insance xxx if received 
                                // and we are the client (bacnet browser )

                                srcDevice.packet = this;

                                uint maxAPDULen;

                                offset += BACnetEncoding.BACnetDecode_uint(buffer, offset, out maxAPDULen);

                                Console.WriteLine("Max APDU length accepted: " + maxAPDULen);



                                uint segmentation_supported;

                                offset += BACnetEncoding.BACnetDecode_uint(buffer, offset, out segmentation_supported);

                                Console.WriteLine("Segmentation Supported: " + segmentation_supported);

                                srcDevice.SegmentationSupported = (BACnetEnums.BACNET_SEGMENTATION)segmentation_supported;



                                uint vendorId;

                                offset += BACnetEncoding.BACnetDecode_uint(buffer, offset, out vendorId);

                                Console.WriteLine("VendorId: " + vendorId);


                                srcDevice.VendorId = vendorId;
                                break;

                            default:
                                BACnetLibraryCL.Panic("Unhandled service choice");
                                break;
                        }
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SIMPLE_ACK:
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                        confirmedServiceChoice = (BACnetEnums.BACNET_CONFIRMED_SERVICE)buffer[apdu_offset + 2];
                        return DecodeComplexACK(buffer, apdu_offset);

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SEGMENT_ACK:
                        break;
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ERROR:
                        break;
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_REJECT:
                        break;
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ABORT:
                        break;
                }
            }


            return true;
        }


        public bool EncodeBACnet(byte[] outbuf, ref int optr)
        {
            int startBACnetPacket = optr;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            outbuf[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;

            if (npdu.isBroadcast)
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            }
            else
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;
            }

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            outbuf[optr++] = 0x01;        // Always 1

            int store_NPCI = optr;
            outbuf[optr++] = 0x00;        // Control

            if (npdu.isNPDUmessage)
            {
                outbuf[store_NPCI] |= 0x80;     // Indicating Network Layer Message
            }

            if (npdu.expectingReply)
            {
                outbuf[store_NPCI] |= 0x04;     // todo- magic number
            }

            if (npdu.isBroadcast)
            {
                outbuf[store_NPCI] |= 0x20;     // Control byte - indicate DADR present 
                outbuf[optr++] = 0xff;          // DNET Network - B'cast
                outbuf[optr++] = 0xff;
                outbuf[optr++] = 0x00;          // DLEN
            }
            else
            {
                // need to insert destination DADR here
                if (dstDevice.adr != null)
                {
                    outbuf[store_NPCI] |= 0x20;         // Control, destination present
                    dstDevice.adr.Encode(outbuf, ref optr);
                }
            }

            // we are a router, so we need to add source address under most circumstances. (not broadcast who-is)
            if (srcDevice != null && srcDevice.adr != null )
            {
                outbuf[store_NPCI] |= 0x08;
                srcDevice.adr.Encode(outbuf, ref optr);
            }

            if (dstDevice != null)
            {
                outbuf[optr++] = 0xff;        // Hop count
            }


            if (apdu_present)
            {
                // APDU start
                // http://www.bacnetwiki.com/wiki/index.php?title=APDU

                for (int i = 0; i < apdu_length; i++)
                {
                    outbuf[optr++] = buffer[apdu_offset + i];        // Encoded APDU type == 01 == Unconfirmed Request
                }
            }


            if (npdu.isNPDUmessage)
            {
                switch (npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                        outbuf[optr++] = 0x00;        // Who-Is-Router..
                        break;
                }
            }


            outbuf[store_length_here] = (byte)(((optr - startBACnetPacket) >> 8) & 0xff);
            outbuf[store_length_here + 1] = (byte)((optr - startBACnetPacket) & 0xff);

            // bad, wrong optr += 2;

            return true;
        }

        bool DecodeComplexACK(byte[] buf, int offset)
        {

            if ((buf[offset] & 0x0f) != 0)
            {
                BACnetLibraryCL.Panic("Not ready to handle segmented messages yet");
                return false;
            }

            // invoke ID - ignoring for now



            // Service ACK choice

            switch ((BACnetEnums.BACNET_CONFIRMED_SERVICE)buf[offset + 2])
            {
                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                    offset += 3;

                    // Decode Object ID of the object whos property we are reading

                    BACnetObjectIdentifier oid = new BACnetObjectIdentifier(buf, ref offset, BACnetEnums.TAG.CONTEXT, 0);

                    // Decode the property ID
                    propertyID = (BACnetEnums.BACNET_PROPERTY_ID)BACnetEncoding.DecodeTagContextUint(buf, ref offset, 1);

                    // Now decode the Property Value itself. Variable encoding, variable length, etc....

                    switch (oid.objectType)
                    {
                        case BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE:
                            switch (propertyID)
                            {
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                    // decode the list of objects
                                    // process the opening context tag, 0x3e
                                    if (buffer[offset++] != 0x3e)
                                    {
                                        BACnetLibraryCL.Panic("Opening context tag not found");
                                        return false;
                                    }

                                    objectList = new List<BACnetObjectIdentifier>();

                                    // now loop until closing tag found
                                    while (buffer[offset] != 0x3f)
                                    {
                                        // we should get a list of object IDs, add these to our backnet packet object as they are discovered.

                                        objectList.Add(new BACnetObjectIdentifier(buffer, ref offset, BACnetEnums.TAG.APPLICATION, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID));
                                    }
                                    break;
                                default:
                                    BACnetLibraryCL.Panic("Unexpected property ID");
                                    break;
                            }
                            break;

                        default:
                            BACnetLibraryCL.Panic("Unhandled object type");
                            break;
                    }



                    break;
                default:
                    BACnetLibraryCL.Panic("Not ready to deal with this service yet");
                    return false;
            }

            return true;
        }
    }


}

