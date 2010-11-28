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
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BACnetLibrary
{ 
    public class BACnetPacket : IEquatable<BACnetPacket>, ICloneable
    {
        public enum ADDRESS_TYPE
        {
            LOCAL_BROADCAST,
            GLOBAL_BROADCAST,
            REMOTE_BROADCAST
        }

        public enum MESSAGE_TYPE
        {
            NETWORK_LAYER,
            APPLICATION,
        }

        bool flagBVLLencoded ;
        bool flagNPCIencoded;

        AppManager _apm;
        BACnetManager _bnm;

        public int Source_Port;
        public byte[] buffer = new byte[2000];
        public int length;          // for incoming messages
        public int iptr;
        public int optr;            // for buildingoutgoing messages

        public MESSAGE_TYPE messageType;

        public int nsdu_offset;
        public int npduLengthOffset;    // This is where the length of the whole BACnet packet - NPDU (NCPI+NSDU) and APDU (if it exists) will eventually be stored

        // APDU Parameters
        // See: http://www.bacnetwiki.com/wiki/index.php?title=PDU_Type

        // How to handle errors http://www.bacnetwiki.com/wiki/index.php?title=Error
        public bool errorFlag;
        public int errorClass;
        public int errorCode;

        public int apdu_offset;
        public int apdu_length;
        public BACnetEnums.BACNET_BVLC_FUNCTION bvlcFunction;
        public BACnetEnums.BACNET_BACNET_REJECT_REASON pduRejectReason;

        public uint lowRange;
        public uint highRange;

        public BACnetEnums.BACNET_PDU_TYPE pduType;
        public BACnetEnums.BACNET_UNCONFIRMED_SERVICE unconfirmedServiceChoice;
        public BACnetEnums.BACNET_CONFIRMED_SERVICE confirmedServiceChoice;
        public bool apduUnconfirmedServiceFlag;
        public bool apduConfirmedServiceTypeFlag;

        public BACnetObjectIdentifier objectID;
        public BACnetEnums.BACNET_PROPERTY_ID propertyID;
        public List<BACnetObjectIdentifier> objectList;
        public int arrayIndex;
        public bool arrayIndexDecoded;

        public byte[] apdu_buf;
        public byte invokeID;


        // NPDU parameters
        public NPDU npdu = new NPDU();
        public List<UInt16> numberList;
        public List<RoutingTableEntry> routerTableList;
        public List<bool> bitString;


        // various objects that will be created depending on the packet received
        //public ADR sadr;
        public DADR dAdr = new DADR();
        public ADR sAdr;
        public Device srcDevice = new Device();

        // todo add hopcount to npdu class
        public byte hopcount = 255 ;
        public long timestamp;

        // public bool apdu_present, is_broadcast, expecting_reply;
        public bool apdu_present;
        public int protocolRevision;

        public myIPEndPoint directlyConnectedIPEndPointOfDevice;


        // Privates
        int npdu_offset;

        public BACnetPacket(AppManager apm)
        {
            _apm = apm;
            // apdu_present = false;
            // npdu.isBroadcast = false;
        }


        public BACnetPacket(AppManager apm, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE netMessageType)
        {
            messageType = MESSAGE_TYPE.NETWORK_LAYER;
            npdu.function = netMessageType;
            _apm = apm;
        }

        public BACnetPacket(AppManager apm, BACnetEnums.BACNET_UNCONFIRMED_SERVICE bacSvc)
        {
            _apm = apm;
            messageType = MESSAGE_TYPE.APPLICATION;
            pduType = BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST;
            unconfirmedServiceChoice = bacSvc;
        }


        public BACnetPacket(AppManager apm, BACnetEnums.BACNET_CONFIRMED_SERVICE bacSvc)
        {
            _apm = apm;
            messageType = MESSAGE_TYPE.APPLICATION;
            pduType = BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST;
            confirmedServiceChoice = bacSvc;
        }

        public BACnetPacket(AppManager apm, BACnetManager bnm, ADR addr, BACnetObjectIdentifier oid, BACnetEnums.BACNET_PROPERTY_ID prop ) 
        {
            _apm = apm; 
            _bnm = bnm;
            messageType = MESSAGE_TYPE.APPLICATION;
            pduType = BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST;
            confirmedServiceChoice = BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY;

            if ( addr.GetType() == typeof(DADR) )
            {
                dAdr = (DADR)addr;
            }
            else
            {
                dAdr = new DADR(addr);
            }

            EncodeNPCI(dAdr, BACnetPacket.MESSAGE_TYPE.APPLICATION);
            EncodeAPDUheader(BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY);

            // Encode the rest of the packet according to http://www.bacnetwiki.com/wiki/index.php?title=Read_Property

            EncodeContextTag(0, oid);
            EncodeContextTag(1, prop );

            // BACnetUtil.SendOffPacket( apm, bnm, pkt, pkt.buffer, pkt.optr);
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool DecodeBACnet()
        {
            bool destinationPresent = false;

            try
            {
                // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                ADR sadr = null;

                if (buffer[iptr++] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
                {
                    // todo3, panic log
                    _apm.MessageProtocolError("m0013 - Not a BACnet/IP message");
                    return false;
                }


                // if it is a BACnet message, it _must_ have come from some device, however sketchily defined...
                // srcDevice = new Device();


                // we could receive an original broadcast, a unicast, a forwarded here...
                // right now
                // BVLC Function Types. See http://www.bacnetwiki.com/wiki/index.php?title=BVLC_Function

                bvlcFunction = (BACnetEnums.BACNET_BVLC_FUNCTION)buffer[iptr++];

                // extract the BACnet length

                int tLen = BACnetUtil.ExtractUInt16(buffer, ref iptr);

                if (tLen != length)
                {
                    // disconnect between Ethernet Packet Lenght and BACnet Packet length. We are not going to tolerate..
                    throw new Exception("m0166-BACnet length mismatch");
                }

                switch (bvlcFunction)
                {
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU:
                        npdu_offset = iptr + 6;
                        break;
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU:
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU:
                        // all these OK
                        npdu_offset = iptr;
                        break;
                    default:
                        throw new Exception("m0012-No such BVLC function");
                }

                // Investigate the NPDU
                // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

                if (buffer[npdu_offset++] != BACnetEnums.BACNET_PROTOCOL_VERSION)
                {
                    // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                    throw new Exception("m0011-BVLC microprotocol problem");
                }

                // expecting reply?

                int bvlc = buffer[npdu_offset++];

                if ((bvlc & 0x04) == 0x04)
                {
                    npdu.expectingReply = true;
                }


                // destination address present?
                // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

                if ((bvlc & 0x20) == 0x20)
                {
                    destinationPresent = true;

                    // dnet, dadr and hop count present
                    // int dAddrOffset = npdu_offset + 2;
                    dAdr.Decode(buffer, ref npdu_offset);

                    if (!dAdr.isBroadcast)
                    {
                        if (dAdr.MACaddress.length != 1 && dAdr.MACaddress.length != 6 && dAdr.MACaddress.length != 0)
                        {
                            throw new Exception("m0009 - Unexpected DADR len");
                        }
                    }
                    // todo, pick up variable length destination address
                }

                // Source address present?
                // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

                if ((bvlc & 0x08) == 0x08)
                {
                    // means SADR, SNET present
                    sadr = new ADR();
                    sadr.Decode(buffer, ref npdu_offset, true );
                    sadr.viaIPEP = directlyConnectedIPEndPointOfDevice;
                }
                else
                {
                    // at this point, if SADR not discovered within the NPDU, then the SADR MAC address is the Ethernet/IP fromaddress
                    // and the device can (must) be considered 'direcly connected'
                    if (directlyConnectedIPEndPointOfDevice != null)
                    {
                        // and even though the device is directly connected, because we are a router, we have an allocated network number to provide
                        // the network number is available outside this class, and will be filled in by the calling function if it is relevant.
                        // TODO !!
                        srcDevice.adr = new ADR(directlyConnectedIPEndPointOfDevice);
                    }
                    else
                    {
                        throw new Exception("m0063 - No From-Address can be determined");
                    }
                }

                if (destinationPresent == true)
                {
                    hopcount = buffer[npdu_offset++];
                    // true broadcast, but check the hopcount

                    if (hopcount == 0)
                    {
                        // out of hops, should never happen to us, so sound a warning
                        // todo, remove this for a functioning systems
                        Console.WriteLine("Hopcount of 0 detected");
                        // return false;

                        // todo, i got here when _sending_ a bad message (see ccontrols bug 003 - I suspect we are reading our own sent packets here...
                    }
                }

                // finished resolving sadr, and dadr. Now populate our devices as required

                if (sadr != null)
                {
                    if (srcDevice.adr != null)
                    {
                        // means this adr was partially populated elsewhere, probably in the receive function, who knows the IP address of the source device.
                        // we now have to tip in the rest of the information
                        srcDevice.adr.networkNumber = sadr.networkNumber;
                        srcDevice.adr.MACaddress = sadr.MACaddress;
                        srcDevice.adr.directlyConnected = false; // if the sadr field exists in the BACnet packet, it is NOT directly connected.
                    }
                    else
                    {
                        srcDevice.adr = sadr;
                    }
                }


                if (( bvlc & 0x80) == 0x80)  // todo magic number
                {
                    // NSDU contains Network Layer Message
                    // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                    apdu_present = false;
                    messageType = MESSAGE_TYPE.NETWORK_LAYER;
                    // npdu.isNPDUmessage = true;

                    // calculate offset to the NSDU

                    npdu.function = (BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE)buffer[npdu_offset++];

                    switch (npdu.function)
                    {
                        case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                            // there may be a list of network numbers after the header

                            routerTableList = new List<RoutingTableEntry>();
                            if ((length - npdu_offset >= 2))
                            {
                                for (int i = 0; i < (length - npdu_offset) / 2; i++)
                                {
                                    RoutingTableEntry rte = new RoutingTableEntry(BACnetUtil.ExtractUInt16(buffer, ref npdu_offset)) ;
                                    rte.farSide = true ;
                                    routerTableList.Add( rte );
                                }
                            }
                            break;

                        case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                            int tiptr = npdu_offset;
                            int numPorts = buffer[tiptr++];
                            if (numPorts < 0 || numPorts > 255) throw new Exception("m0166-Impossible number of Router Table Entries");
                            routerTableList = new List<RoutingTableEntry>();

                            for (int i = 0; i < numPorts; i++)
                            {
                                RoutingTableEntry rp = new RoutingTableEntry();
                                rp.Decode(buffer, ref tiptr);
                                routerTableList.Add(rp);
                            }
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

                    messageType = MESSAGE_TYPE.APPLICATION;
                    apdu_present = true; // todo, with the new messageType, we can lose this flag...

                    apdu_offset = npdu_offset ;

                    apdu_length = length - apdu_offset;
                    if (apdu_length < 0)
                    {
                        throw new Exception("m0006 Illegal APDU length");
                    }

                    // todo - need to extract the apdu for others to refer to. However, APDUs may be customer specific, so extract this as a buffer
                    // only for now, and do some spot checks for relevant functions such as I-Am and Who-Is.

                    apdu_buf = new byte[2000];

                    Buffer.BlockCopy(buffer, apdu_offset, apdu_buf, 0, apdu_length);

                    // the offset here is the APDU. Start parsing APDU.
                    // todo, decided to leave the enum values unshifted today 11/27/09
                    pduType = (BACnetEnums.BACNET_PDU_TYPE)(buffer[apdu_offset] & 0xf0);

                    // make sure that we can handle the rest of the packet

                    //if ((buffer[apdu_offset] & 0x0f) != 0 )
                    //{
                    //    throw new Exception("m0056 - Cannot handle segmented messages yet");
                    //}

                    int tptr = apdu_offset + 1;

                    if (pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST)
                    {
                        // some PDUs have max segs here
                        tptr++;
                    }

                    // now the next byte is the invoke ID

                    invokeID = buffer[tptr++];

                    switch (pduType)
                    {
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                            // todo, offset is not always 1, depends on the flags in the first byte.
                            DecodeUnconfirmedService(apdu_buf, apdu_length);
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                            DecodeConfirmedService(apdu_buf);
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SIMPLE_ACK:
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                            confirmedServiceChoice = (BACnetEnums.BACNET_CONFIRMED_SERVICE)buffer[apdu_offset + 2];
                            DecodeComplexACK(buffer, apdu_offset);
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SEGMENT_ACK:
                            _apm.MessageTodo("m0093 - Segment ACK");
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ERROR:
                            // http://www.bacnetwiki.com/wiki/index.php?title=Error
                            // skip over the Service Choice in the packet
                            tptr++;
                            // Extract the Error Class
                            errorClass = (int) BACnetUtil.ExtractApplicationTagEnum(buffer, ref tptr);
                            // and the Error code
                            errorCode = (int) BACnetUtil.ExtractApplicationTagEnum(buffer, ref tptr);
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_REJECT:
                            pduRejectReason = (BACnetEnums.BACNET_BACNET_REJECT_REASON)buffer[tptr++];
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ABORT:
                            _apm.MessageTodo("m0066 - PDU abort");
                            errorFlag = true;
                            break;
                        default:
                            throw new Exception("m0003 - Illegal PDU type");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("m0001 - BACnet Decode Failed " + ex.ToString());
            }
            return true;
        }


        private void DecodeConfirmedService(byte[] apdu_buf)
        {
            if ((apdu_buf[0] & 0x08) != 0)
            {
                _apm.MessageTodo("m0002 Need to implement confirmed service types with seg=1 still");
                return;
            }

            int iptr = 3;

            confirmedServiceChoice = (BACnetEnums.BACNET_CONFIRMED_SERVICE)apdu_buf[iptr++];
            apduConfirmedServiceTypeFlag = true;

            switch (confirmedServiceChoice)
            {
                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                    // Expecting 2-3 context tags. 
                    // First, mandatory, context 0, object ID
                    objectID = new BACnetObjectIdentifier();
                    objectID.DecodeContextTag(apdu_buf, ref iptr);

                    // Second, mandatory, Property ID
                    propertyID = (BACnetEnums.BACNET_PROPERTY_ID)BACnetUtil.ExtractContextTagUint(apdu_buf, ref iptr, 1);

                    // Third, Array Index, Optional
                    if (iptr < apdu_length)
                    {
                        arrayIndex = (int)BACnetUtil.ExtractContextTagUint(apdu_buf, ref iptr, 2);
                        arrayIndexDecoded = true;
                    }
                    break;

                default:
                    _apm.MessageTodo("m0024 all the other service types");
                    break;
            }


        }


        private bool DecodeUnconfirmedService(byte[] apdu_buf, int apduLen)
        {
            // todo, this code assumes that the service is encoded in postion number one. This is not always the case. Resolve

            if ((apdu_buf[0] & 0x0f) != 0)
            {
                _apm.MessageProtocolError("m0025 this nibble should be zero for Unconfirmed services");
                return false;
            }

            if (apdu_buf[1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS)
            {
                // http://www.bacnetwiki.com/wiki/index.php?title=Who-Is

                apduUnconfirmedServiceFlag = true;
                unconfirmedServiceChoice = BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS;

                if (apduLen != 2)
                {
                    int offset = 2;

                    // means we must have a low and high range..
                    lowRange = BACnetUtil.ExtractContextTagUint(apdu_buf, ref offset, 0);
                    highRange = BACnetUtil.ExtractContextTagUint(apdu_buf, ref offset, 1);
                }

                // and for now, we will assume only workstations issue who-is messages.

                // todonow this.srcDevice.deviceType = CRenums.DEVICE_TYPES.WORKSTATION;
            }
            else if (apdu_buf[1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM)
            {
                apduUnconfirmedServiceFlag = true;
                unconfirmedServiceChoice = BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM;

                // I-Am described right here: http://www.bacnetwiki.com/wiki/index.php?title=I-Am

                // first encoded entity is the Device Identifier...
                // Encoding described here: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

                // Decode Device Identifier

                int offset = 2;

                srcDevice.deviceObjectID.DecodeApplicationTag(apdu_buf, ref offset);
                Console.WriteLine("This is device: " + srcDevice.deviceObjectID);

                // todo, for now, we will ignore device insance xxx if received 
                // and we are the client (bacnet browser )

                srcDevice.packet = this;

                uint maxAPDULen;

                offset += BACnetEncoding.BACnetDecode_uint_deprecated(apdu_buf, offset, out maxAPDULen);

                //Console.WriteLine("Max APDU length accepted: " + maxAPDULen);

                uint segmentation_supported;

                offset += BACnetEncoding.BACnetDecode_uint_deprecated(apdu_buf, offset, out segmentation_supported);

                //Console.WriteLine("Segmentation Supported: " + segmentation_supported);

                srcDevice.SegmentationSupported = (BACnetEnums.BACNET_SEGMENTATION)segmentation_supported;

                srcDevice.vendorID = new VendorID(BACnetUtil.ExtractApplicationTagUint(apdu_buf, ref offset));
            }
            return true;
        }


        public void EncodeBACnet()
        {
            if (buffer == null) buffer = new byte[2000];
            optr = 0;

            if (EncodeBACnet(this.buffer, ref this.optr))
            {
                return;
            }
            else
            {
                throw new Exception("m0163-Encoding error");
            }
        }



        public bool EncodeBACnet(byte[] outbuf, ref int optr)
        {
            int startBACnetPacket = optr;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            outbuf[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;

            if (dAdr.isBroadcast)
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

            if (messageType == MESSAGE_TYPE.NETWORK_LAYER)
            {
                outbuf[store_NPCI] |= 0x80;     // Indicating Network Layer Message
            }

            if (npdu.expectingReply)
            {
                outbuf[store_NPCI] |= 0x04;     // todo- magic number
            }

            if (dAdr.isBroadcast && dAdr.isLocalBroadcast == false )
            {
                outbuf[store_NPCI] |= 0x20;     // Control byte - indicate DADR present 
                BACnetUtil.InsertUint16(outbuf, ref optr, dAdr.networkNumber);
                outbuf[optr++] = 0x00;          // DLEN == 0 indicates broadcast
            }
            else
            {
                // insert dadr - but only if the device is NOT directly coupled. See page 59. If the device is directly coupled
                // then the ethernet address in the packet will suffice.
                if (dAdr.directlyConnected != true && dAdr.isLocalBroadcast == false)
                {
                    // need to insert destination DADR here
                    outbuf[store_NPCI] |= 0x20;         // Control byte - indidate DADR present 
                    dAdr.Encode(outbuf, ref optr);
                }
            }

            // we are a router, so we need to add source address under most circumstances. (not broadcast who-is)
            if (srcDevice != null && srcDevice.adr != null)
            {
                outbuf[store_NPCI] |= 0x08;                 // Control byte - indidate SADR present 
                srcDevice.adr.Encode(outbuf, ref optr);
            }

            if (dAdr.directlyConnected != true && dAdr.isLocalBroadcast == false )
            {
                // insert hopcount here. 
                outbuf[optr++] = hopcount ;        // Hop count
            }

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            if (apdu_present)
            {
                // APDU start
                // http://www.bacnetwiki.com/wiki/index.php?title=APDU

                for (int i = 0; i < apdu_length; i++)
                {
                    outbuf[optr++] = buffer[apdu_offset + i];        // Encoded APDU type == 01 == Unconfirmed Request
                }
            }
            else if (messageType == MESSAGE_TYPE.NETWORK_LAYER)
            {
                // Build the Nsdu
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Message

                outbuf[optr++] = (byte)npdu.function;

                switch (npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                        break;

                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE:
                        if (routerTableList == null)
                        {
                            outbuf[optr++] = 0; // no entries -> should elicit an ack with the full router table from the remote router
                        }
                        else
                        {
                            if ((uint)routerTableList.Count > 255) throw new Exception("m0164-Impossible count");

                            // See http://www.bacnetwiki.com/wiki/index.php?title=Initialize-Routing-Table

                            outbuf[optr++] = (byte)routerTableList.Count;        // Number of port mappings
                            foreach (RoutingTableEntry rte in routerTableList)
                            {
                                //if (rte.portID == 0) throw new Exception("m0167-PortID of 0 not allowed");
                                BACnetUtil.InsertInt16(buffer, ref optr, rte.networkNumber);
                                outbuf[optr++] = (byte)rte.portID;
                                outbuf[optr++] = 0; // Port Info Length
                            }
                        }
                        break;

                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        if (numberList != null)
                        {
                            foreach (uint i in numberList)
                            {
                                BACnetUtil.InsertUint16(outbuf, ref optr, i);
                            }
                        }
                        break;

                    default:
                        _apm.MessageTodo("m0023 Implement " + npdu.function.ToString());
                        break;
                }
            }
            else
            {
                // build an APDU.

                // APDU Header
                outbuf[optr++] = (byte) this.pduType ; 

                switch (this.pduType)
                {
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
//                        outbuf[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request

                        switch (this.unconfirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                                // APDU start
                                outbuf[optr++] = 0x08;
                                break;

                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                // APDU start
                                // http://www.bacnetwiki.com/wiki/index.php?title=APDU

                                outbuf[optr++] = 0x00;        // Unconfirmed Service Choice: I-Am

                                // object identifier, device object 

                                BACnetObjectIdentifier bnoid = new BACnetObjectIdentifier();

                                bnoid.SetType(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);
                                _apm.MessageTodo("m0038 - Establish a mechanism to determine our OWN Device ID");
                                bnoid.SetInstance(_apm.ourDeviceId);
                                bnoid.EncodeApplicationTag(outbuf, ref optr);

                                // Maximum APDU length (Application Tag, Integer)
                                Unsigned apdulen = new Unsigned(1476);
                                apdulen.Encode(outbuf, ref optr);

                                // Segmentation supported, (Application Tag, Enum)
                                BACnetSegmentation bsg = new BACnetSegmentation();

                                bsg.Encode(outbuf, ref optr);

                                // Vendor ID, (Application Tag, Integer)
                                BACnetUtil.InsertApplicationTagUint16(outbuf, ref optr, _apm.ourVendorID);
                                break;

                            default:
                                _apm.MessageTodo("m0022 Build missing service type");
                                break;
                        }
                        break;

                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:

                        // build confirmed request header 
                        // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU
                        outbuf[optr++] = 0x05;  // max segs, max resp. todo
                        outbuf[optr++] = _apm.invokeID++ ;
                        // sequence number may come next, dep on flags. todo
                        outbuf[optr++] = (byte) this.confirmedServiceChoice;

                        switch (this.confirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                                // context encoding
                                // http://www.bacnetwiki.com/wiki/index.php?title=Read_Property
                                this.objectID.Encode(outbuf, ref optr);
                                BACnetUtil.EncodeContextTag(outbuf, ref optr, 1, (int)this.propertyID);
                                break;
                            default:
                                throw new Exception ("m0184-Create confirmed service choice");
                        }
                        break;

                    default:
                        throw new Exception ("m0021 Build missing PDU type");
                }
            }
            BACnetUtil.InsertInt16(outbuf, ref store_length_here, optr - startBACnetPacket);
            return true;
        }

        BACnetEnums.BACNET_BVLC_FUNCTION bacnetBVLCfunc;


        public void EncodeBVLL(BACnetEnums.BACNET_BVLC_FUNCTION bvlcFunc)
        {
            if (bvlcFunc != BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU) throw new Exception("m0202 - This signature for EncodeBVLL is only valid for Broadcasts");
            bacnetBVLCfunc = bvlcFunc;
            dAdr = new DADR(ADDRESS_TYPE.GLOBAL_BROADCAST);
            EncodeBVLL();
        }


        public void EncodeBVLL(DADR dAdr, BACnetEnums.BACNET_BVLC_FUNCTION bvlcFunc)
        {
            bacnetBVLCfunc = bvlcFunc;
            dAdr = dAdr;
            EncodeBVLL();
        }


        public void EncodeBVLL()
        {
            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            buffer[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;  // 81
            if (dAdr.isBroadcast)
            {
                buffer[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            }
            else
            {
                buffer[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;
            }
            // todo, allow users to override BVLCs later, or with parameter
            // buffer[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;  //0a
            npduLengthOffset = optr;    // This is where the final length of the BACnet packet will be stored
            optr += 2;
            flagBVLLencoded = true;
        }


        public void EncodeNPCI()
        {
            EncodeNPCI(buffer, ref optr);
        }


        public void EncodeNPCI(byte[] outbuf, ref int locOptr, MESSAGE_TYPE mType)
        {
            messageType = mType;
            EncodeNPCI(outbuf, ref locOptr);
        }


        public void EncodeNPCI(DADR da, ADR sadr, MESSAGE_TYPE mt)
        {
            dAdr = da;
            sAdr = sadr;
            messageType = mt;
            EncodeNPCI();
        }

        public void EncodeNPCI(DADR da, MESSAGE_TYPE mt )
        {
            dAdr = da;
            messageType = mt;
            EncodeNPCI();
        }


        public void EncodeNPCI(byte[] outbuf, ref int locOptr)
        {
            // Encode the NPCI part of the NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            if ( ! flagBVLLencoded )
            {
                // have not yet started NPCI or the BVLL yet.
                // http://www.bacnetwiki.com/wiki/index.php?title=BVLL
                EncodeBVLL();
            }

            // Start of NPCI
            // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

            outbuf[locOptr++] = 0x01;        // Always 1

            int store_NPCI = locOptr;
            outbuf[locOptr++] = 0x00;        // Control

            switch ( messageType )
            {
                case MESSAGE_TYPE.NETWORK_LAYER:
                    outbuf[store_NPCI] |= 0x80;     // Indicating Network Layer Message
                    break;
                case MESSAGE_TYPE.APPLICATION:
                    break;
                default:
                    throw new Exception("m0188 - Message type must be specified");
            }

            if (npdu.expectingReply)
            {
                outbuf[store_NPCI] |= 0x04;     // todo- magic number
            }

            if (dAdr.isBroadcast)
            {
                outbuf[store_NPCI] |= 0x20;     // Control byte - indicate DADR present 
                outbuf[locOptr++] = 0xff;          // DNET Network - B'cast
                outbuf[locOptr++] = 0xff;
                outbuf[locOptr++] = 0x00;          // DLEN
            }
            else
            {
                // insert dadr - but only if the device is NOT directly coupled. See page 59. If the device is directly coupled
                // then the ethernet address in the packet will suffice.
                if (!dAdr.directlyConnected)
                {
                    // need to insert destination DADR here
                    outbuf[store_NPCI] |= 0x20;         // Control byte - indidate DADR present 
                    dAdr.Encode(outbuf, ref locOptr);
                }
            }

            // we are a router, so we need to add source address under most circumstances. (not broadcast who-is)
            if (srcDevice.adr != null)
            {
                outbuf[store_NPCI] |= 0x08;                 // Control byte - indidate SADR present 
                srcDevice.adr.Encode(outbuf, ref locOptr);
            }
            else if (sAdr != null)
            {
                outbuf[store_NPCI] |= 0x08;                 // Control byte - indidate SADR present 
                sAdr.Encode(outbuf, ref locOptr);
            }

            if (dAdr.isBroadcast || !dAdr.directlyConnected)
            {
                // insert hopcount here. 
                hopcount--;
                outbuf[locOptr++] = (byte)hopcount;
            }
        }

        
        public void EncodeAPDUheader( BACnetEnums.BACNET_PDU_TYPE pduTyp, BACnetEnums.BACNET_UNCONFIRMED_SERVICE svcChoice )
        {
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Unconfirmed-Request-PDU

            buffer[optr++] = (byte)pduTyp;
            buffer[optr++] = (byte)svcChoice;
        }


        public void EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE pduTyp, BACnetEnums.BACNET_CONFIRMED_SERVICE svcChoice)
        {
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU

            buffer[optr++] = (byte)pduTyp;
            buffer[optr++] = 0x05;
            buffer[optr++] = this._apm.invokeID++;
            buffer[optr++] = (byte)svcChoice;
        }


        public void EncodeAPDUheader(BACnetEnums.BACNET_UNCONFIRMED_SERVICE svcChoice)
        {
            EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, svcChoice);
        }


        public void EncodeAPDUheader(BACnetEnums.BACNET_CONFIRMED_SERVICE svcChoice)
        {
            EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST, svcChoice);
        }


        void DecodeComplexACK(byte[] buf, int offset)
        {
            if ((buf[offset] & 0x0f) != 0)
            {
                throw new Exception("m0118 - Not ready to handle segmented messages yet");
            }

            // invoke ID - ignoring for now

            // Service ACK choice

            BACnetEnums.BACNET_CONFIRMED_SERVICE sc = (BACnetEnums.BACNET_CONFIRMED_SERVICE)buf[offset + 2];
            switch (sc)
            {
                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                    offset += 3;

                    // Decode Object ID of the object whos property we are reading

                    BACnetObjectIdentifier oid = new BACnetObjectIdentifier(buf, ref offset, BACnetEnums.TAG.CONTEXT, 0);

                    // Decode the property ID
                    propertyID = (BACnetEnums.BACNET_PROPERTY_ID)BACnetUtil.ExtractContextTagUint(buf, ref offset, 1);

                    // Now decode the Property Value itself. Variable encoding, variable length, etc....

                    switch (oid.objectType)
                    {
                        case BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE:
                            switch (propertyID)
                            {
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                    // decode the list of objects
                                    objectList = new List<BACnetObjectIdentifier>();
                                    if ((buffer[offset] & 0xf0) == 0x20)
                                    {
                                        // context tag 2 means that the ACK is index based, and that there is only one object in the 'list'
                                        // extract and ignore the index
                                        uint arrayIndex = BACnetUtil.ExtractContextTagUint(buffer, ref offset, 2);

                                        if (buffer[offset++] != 0x3e) throw new Exception("m0073 - Opening context tag not found " + buffer[offset - 1].ToString());
                                        if (arrayIndex == 0)
                                        {
                                            // then the following parameter will be an array length
                                            BACnetUtil.ExtractApplicationTagUint(buffer, ref offset);
                                        }
                                        else
                                        {
                                            // otherwise an object ID
                                            objectList.Add(new BACnetObjectIdentifier(buffer, ref offset, BACnetEnums.TAG.APPLICATION, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID));
                                        }
                                        if (buffer[offset++] != 0x3f) throw new Exception("m0074 - Closing context tag not found, only one object expected! " + buffer[offset - 1].ToString());
                                    }
                                    else
                                    {
                                        // there is a list of objects.. process the opening context tag, 0x3e

                                        if (buffer[offset++] != 0x3e) throw new Exception("m0033 - Opening context tag not found " + buffer[offset - 1].ToString());

                                        // now loop until closing tag found (todo - what if we dont find a closing tag......)
                                        while (buffer[offset] != 0x3f)
                                        {
                                            // we should get a list of object IDs, add these to our backnet packet object as they are discovered.

                                            objectList.Add(new BACnetObjectIdentifier(buffer, ref offset, BACnetEnums.TAG.APPLICATION, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID));
                                        }
                                    }
                                    break;

                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE:
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_IDENTIFIER:
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_SERVICES_SUPPORTED:
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_VENDOR_IDENTIFIER:
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_NAME:
                                    // todo _apm.MessageTodo("m0032 - Decode object name");
                                    // so dont care about these in the relay - leave it for the explorer one day
                                    break;

                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_REVISION:
                                    BACnetUtil.ExtractContextOpeningTag(buf, ref offset, 3);
                                    protocolRevision = (int) BACnetUtil.ExtractApplicationTagUint(buf, ref offset);
                                    BACnetUtil.ExtractContextClosingTag(buf, ref offset, 3);
                                    break;

                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED:
                                    BACnetUtil.ExtractContextOpeningTag(buf, ref offset, 3);
                                    bitString = BACnetUtil.ExtractApplicationTagBitString(buf, ref offset);
                                    BACnetUtil.ExtractContextClosingTag(buf, ref offset, 3);
                                    break;

                                default:
                                    _apm.MessageTodo("m0026 Unimplemented Property ID " + propertyID.ToString());
                                    break;
                            }
                            break;

                        default:
                            _apm.MessageTodo("m0061 Unhandled object type " + oid.objectType.ToString());
                            break;
                    }
                    break;
                default:
                    _apm.MessageTodo("m0028 Not ready to deal with this service yet " + sc.ToString());
                    return;
            }
        }


        public void SetDestination(ADDRESS_TYPE adrtyp)
        {
            switch (adrtyp)
            {
                case ADDRESS_TYPE.GLOBAL_BROADCAST:
                    dAdr.networkNumber = 0xffff; 
                    dAdr.isLocalBroadcast = false;
                    break;
                case ADDRESS_TYPE.LOCAL_BROADCAST:
                    dAdr.isLocalBroadcast = true;
                    break;
                default:
                    throw new Exception("m0146 - Choose one of the legal broadcast destinations for this signature");
            }
            dAdr.isRemoteBroadcast = false;
            dAdr.isBroadcast = true;
        }


        public void SetDestination(DADR dadr)
        {
            this.dAdr = dadr;
        }


        public void SetNetworkLayerMessageFunction(BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE messageFunction)
        {
            messageType = BACnetPacket.MESSAGE_TYPE.NETWORK_LAYER;
            npdu.function = messageFunction;
        }


        public void SetApplicationMessageFunction(BACnetEnums.BACNET_UNCONFIRMED_SERVICE service )
        {
            messageType = BACnetPacket.MESSAGE_TYPE.APPLICATION ;
            pduType = BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST;
            unconfirmedServiceChoice = service;
        }

        public void EncodeApplicationTag( BACnetEnums.BACNET_APPLICATION_TAG at, uint value)
        {
            BACnetUtil.InsertApplicationTag(buffer, ref optr, at, value);
        }

        public void EncodeApplicationTag ( BACnetObjectIdentifier oid )
        {
            BACnetObjectIdentifier.EncodeBACnetObjectIdentifierApplicationTag ( this.buffer, ref this.optr, oid ) ;
        }

        public void EncodeApplicationTag(uint i)
        {
            EncodeApplicationTag(BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT, i);
        }

        public void EncodeApplicationTag(BACnetEnums.BACNET_SEGMENTATION a )
        {
            EncodeApplicationTag(BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED, (uint) a );
        }


        // Context tags are encoded basically as ints, along with the context number, the flag and the length
        //
        // See: http://www.bacnetwiki.com/wiki/index.php?title=Context_Tags

        public void EncodeContextTag( int contextNumber, int value)
        {
            BACnetUtil.EncodeContextTag(this.buffer, ref this.optr, contextNumber, value);
        }

        public void EncodeContextTag(int contextNumber, BACnetObjectIdentifier oid)
        {
            BACnetUtil.EncodeContextTag(this.buffer, ref this.optr, contextNumber, (int)(((UInt32)oid.objectType << 22) | oid.objectInstance), 4);
        }

        public void EncodeContextTag(int contextNumber, BACnetEnums.BACNET_PROPERTY_ID pid)
        {
            BACnetUtil.EncodeContextTag(this.buffer, ref this.optr, contextNumber, (int)pid );
        }

        public void SendPacket()
        {
            BACnetUtil.SendOffPacket( _apm, _bnm, this, buffer, optr);
        }

    }
}
