using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using BACnetLibraryNS;


namespace BACnetLibraryNS
{


    public class BACnetParserClass
    {

        //public BACnetBase control;
        //public BACnetBase Controlbase = new BACnetBase();

        public static void parse(Packet packet, byte[] bytes, BACnetmanager bnm)
        {
            // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            if (bytes[0] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
            {
                // todo3, panic log
                Console.WriteLine("Not a BACnet/IP message");
                return;
            }



            // we could receive an original broadcast, a unicast, a forwarded here...
            // right now
            // BVLC Function Types. See http://www.bacnetwiki.com/wiki/index.php?title=BVLC_Function

            switch ((BACnetEnums.BACNET_BVLC_FUNCTION)bytes[1])
            {
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU:
                    packet.npdu_offset = 10;
                    break;
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU:
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU:
                    // all these OK
                    packet.npdu_offset = 4;
                    break;
                default:
                    // OK, do a panic here
                    //todo1
                    break;
            }


            // Investigate the NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            if (bytes[packet.npdu_offset] != BACnetEnums.BACNET_PROTOCOL_VERSION)
            {
                // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                // todo3, panic
                Console.WriteLine("Protocol version problem");
                return;
            }


            // destination address present?
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            if ((bytes[packet.npdu_offset + 1] & 0x20) == 0x20)
            {

                packet.da_present = true;

                // dnet, dadr and hop count present
                packet.dnet = (uint)((bytes[packet.npdu_offset + 2] << 8) + bytes[packet.npdu_offset + 3]);
                packet.dadr_len = (uint)(bytes[packet.npdu_offset + 4]);

                if (packet.dadr_len == 0)
                {
                    packet.is_broadcast = true;

                    // broadcast, but check the DNET
                    if (packet.dnet != 0xffff)
                    {
                        System.Windows.Forms.MessageBox.Show("Broadcast according to DLEN, but DNET not 0xffff");
                        // todo, this means directed broadcast, need to deal with this still
                        return;
                    }
                }
                if (packet.dadr_len != 1 && packet.dadr_len != 6 && packet.dadr_len != 0)
                {
                    // panic
                    System.Windows.Forms.MessageBox.Show("Unexpected DADR len");
                    return;
                }

                // todo, pick up variable length destination address

            }


            // Source address present?
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            if ((bytes[packet.npdu_offset + 1] & 0x08) == 0x08)
            {
                packet.sa_present = true;

                uint sa_offset = packet.npdu_offset + 2;

                // however, if there is a destination address, move the sa_offset up appropriate amount

                if (packet.da_present)
                {
                    sa_offset = packet.npdu_offset + 2 + 3 + packet.dadr_len;
                }

                // means SADR, SNET present
                packet.snet = (uint)((bytes[sa_offset] << 8) + bytes[sa_offset + 1]);
                packet.sadr_len = bytes[sa_offset + 2];
                if (packet.sadr_len != 1 && packet.sadr_len != 6)
                {
                    // panic
                    System.Windows.Forms.MessageBox.Show("Unexpected SADR len");
                    return;
                }
                packet.sadr = bytes[sa_offset + 3];
            }

            if (packet.da_present)
            {
                if (packet.sa_present)
                {
                    packet.hopcount = (uint)(bytes[packet.npdu_offset + 2 + 2 + 1 + packet.dadr_len + 2 + 1 + packet.sadr_len]);
                }
                else
                {
                    packet.hopcount = (uint)(bytes[packet.npdu_offset + 2 + 2 + 1 + packet.dadr_len]);
                }
                // true broadcast, but check the hopcount

                if (packet.hopcount == 0)
                {
                    // out of hops, should never happen to us, so sound a warning
                    // todo, remove this for a functioning systems
                    System.Windows.Forms.MessageBox.Show("Hopcount of 0 detected");
                    return;
                }
            }


            if ((bytes[packet.npdu_offset + 1] & 0x80) == 0x80)
            {
                // NSDU contains Network Layer Message
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                packet.apdu_present = false;

                //if ((bytes[packet.npdu_offset + 1] & 0x20) == 0x20)
                //{
                //    // means DADR, DNET present
                //    packet.nsdu_offset = packet.npdu_offset + 6; // todo - variable length, fix
                //}
                //else
                //{
                //    packet.nsdu_offset = packet.npdu_offset + 2;
                //}

                // calculate offset to the NSDU

                packet.nsdu_offset = packet.npdu_offset + 2;

                if (packet.sa_present) packet.nsdu_offset += 2 + 1 + packet.sadr_len;
                if (packet.da_present) packet.nsdu_offset += 2 + 1 + packet.dadr_len + 1;

                switch ((BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE)bytes[packet.nsdu_offset])
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_REJECT_MESSAGE_TO_NETWORK:
                        System.Windows.Forms.MessageBox.Show("Network Message \"Reject Message to Network\" received");
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE:
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                        Console.WriteLine("Router-Table-Ack");
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        // todo - conformance, check that this is _always_ a broadcast message.
                        Console.WriteLine("I-Am-Router-to-Network");
                        // Parse list of network numbers available via this router.
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("Vendor Proprietory Network Message received");
                        break;
                }
            }
            else
            {
                // NSDU contains APDU
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                // determine if SNET, SLEN, SADR present

                packet.apdu_present = true;

                //if ((bytes[packet.npdu_offset + 1] & 0x08) == 0x08)
                //{
                //    // means SADR, SNET present
                //    packet.snet = (uint)((bytes[packet.npdu_offset + 5] << 8) + bytes[packet.npdu_offset + 6]);
                //    packet.sadr_len = bytes[packet.npdu_offset + 7];
                //    if (packet.sadr_len != 1 && packet.sadr_len != 6)
                //    {
                //        // panic
                //        System.Windows.Forms.MessageBox.Show("Unexpected SADR len");
                //        return;
                //    }
                //    packet.sadr = bytes[packet.npdu_offset + 8];

                //    packet.apdu_offset = packet.npdu_offset + 9 + packet.sadr_len;

                //    packet.sa_present = true;
                //}
                //else
                //{
                //    // absent
                //    packet.apdu_offset = packet.npdu_offset + 6;
                //}

                // calculate offset to the NSDU

                packet.apdu_offset = packet.npdu_offset + 2;

                if (packet.sa_present) packet.apdu_offset += 2 + 1 + packet.sadr_len;
                if (packet.da_present) packet.apdu_offset += 2 + 1 + packet.dadr_len + 1 ;



                // the offset here is the APDU. Start parsing APDU.

                if (bytes[packet.apdu_offset] == (byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST)
                {
                    Console.WriteLine("This is an unconfirmed request");
                }
                else
                {
                    // todo
                    return;
                }



                if (bytes[packet.apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS)
                {
                    Console.WriteLine("Who-is");
#if BACNET_SERVER
                    // todo - even if we are NOT a server, e.g. if we are a router, or even a browser
                    // we might have to respond to this Who-Is. i.e. if we have an application layer with the 
                    // right device object ID

                    BACnetLibraryCL.SendIAm(bnm);
#endif
                }
                else if (bytes[packet.apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM)
                {
#if BACNET_BROWSER
                    Console.WriteLine("We have received an 'I-Am'");

                    // I-Am described right here: http://www.bacnetwiki.com/wiki/index.php?title=I-Am

                    Device D = new Device();

                    // first encoded entity is the Device Identifier...
                    // Encoding described here: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

                    // Decode Device Identifier

                    uint deviceId;

                    uint offset = BACnetEncoding.BACnetDecode_uint(bytes, packet.apdu_offset + (uint)2, out deviceId);

                    offset += packet.apdu_offset + (uint)2;


                    // todo - brute force mask off the object type field 
                    deviceId &= 0x03fffff;
                    Console.WriteLine("This is device: " + deviceId);

                    // todo, for now, we will ignore device insance xxx if received 
                    // and we are the client (bacnet browser )

                    if (bnm.mode == BACnetEnums.BACNET_MODE.BACnetClient && deviceId == BACnetEnums.CLIENT_DEVICE_ID)
                    {
                        return;
                    }



                    D.DeviceId = deviceId;
                    D.packet = packet;
                    D.SourceAddress = packet.sadr;
                    D.NetworkNumber = packet.snet;


                    uint maxAPDULen;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out maxAPDULen);

                    Console.WriteLine("Max APDU length accepted: " + maxAPDULen);



                    uint segmentation_supported;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out segmentation_supported);

                    Console.WriteLine("Segmentation Supported: " + segmentation_supported);

                    D.SegmentationSupported = (BACnetEnums.BACNET_SEGMENTATION)segmentation_supported;



                    uint vendorId;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out vendorId);

                    Console.WriteLine("VendorId: " + vendorId);

                    D.VendorId = vendorId;


                    // Devicelist.Add(D);
                    bnm.NewDeviceQueue.Enqueue(D);

#endif
                }


                // at this point, if we are acting as a router, route the message.
                // todo - even if we are a router, the message may be addressed to the router's application layer - watch for that
                // if we are not a router, then check that the message is addressed to us before acting on it.

#if BACNET_ROUTER
                RouterHandlerCL.RouterProcessPacket( packet );
#endif


            }


        }


    }
}
