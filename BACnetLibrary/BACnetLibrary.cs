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
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace BACnetLibrary
{

    public class ProtocolException : Exception
    {
        int len;
        byte[] buf;
        string message;

        public ProtocolException()
            : base()
        { }

        public ProtocolException(string message, byte[] buf, int len)
            : base(message)
        {
            this.message = message;
            this.len = len;
            this.buf = buf;
        }

        public ProtocolException(string message)
            : base(message)
        {
            this.message = message;
        }

        public ProtocolException(string message, System.Exception inner)
            : base(message, inner)
        { }

        public void DumpException(AppManager apm)
        {
            apm.MessageProtocolError(message);
        }
    }



    public class myIPEndPoint : IPEndPoint
    {

        public myIPEndPoint(EndPoint ep)
            : base(((IPEndPoint)ep).Address, ((IPEndPoint)ep).Port)
        {
        }

        public myIPEndPoint(IPAddress address, int port)
            : base(address, port)
        {
        }

        public myIPEndPoint()
            : base(IPAddress.Any, 0)
        {
        }

        // public IPEndPoint ipep = new IPEndPoint(0,0);

        public void Decode(byte[] buffer, ref int offset)
        {
            int port = 0;

            byte[] bytearr = new byte[4];

            Buffer.BlockCopy(buffer, offset, bytearr, 0, 4);

            port = buffer[offset + 4];
            port = (port << 8) | buffer[offset + 5];

            base.Address = new IPAddress(bytearr);
            base.Port = port;
            offset += 6;
        }

        public void Encode(byte[] buffer, ref int optr)
        {
            byte[] addrb = base.Address.GetAddressBytes();

            for (int i = 0; i < 4; i++)
            {
                buffer[optr++] = addrb[i];
            }

            buffer[optr++] = (byte)((Port >> 8) & 0xff);
            buffer[optr++] = (byte)(Port & 0xff);
        }


        public bool Equals(myIPEndPoint ep)
        {
            if (!this.Address.Equals(ep.Address)) return false;
            if (!this.Port.Equals(ep.Port)) return false;
            return true;
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


    public class BACnetMACaddress : IEquatable<BACnetMACaddress>
    {
        public enum MACaddrType { None, Single, IPEP, ByteArray } ;

        public MACaddrType mat;
        public uint length;
        public uint uintMACaddress;
        public myIPEndPoint ipMACaddress;
        public byte[] byteMACaddr;

        public BACnetMACaddress()
        {
            mat = MACaddrType.None;
        }

        public BACnetMACaddress(uint madr)
        {
            mat = MACaddrType.Single;
            length = 1;
            uintMACaddress = madr;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public BACnetMACaddress( string macAddr )
        {
            mat = MACaddrType.ByteArray;
            length = (uint) ( macAddr.Length / 2 ) ;
            byteMACaddr = StringToByteArray(macAddr);
        }

        public BACnetMACaddress(myIPEndPoint madr)
        {
            mat = MACaddrType.IPEP;
            length = 6;
            ipMACaddress = madr;
        }

        public bool Equals(BACnetMACaddress madr)
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
                    throw new Exception("m0173-Illegal MAC address length??");
                // return false;
            }
            return true;
        }

        public override string ToString()
        {
            switch (length)
            {
                case 0:
                    return "Broadcast";
                case 1:
                    return uintMACaddress.ToString();
                case 6:
                    return ipMACaddress.ToString();
                default:
                    throw new Exception("m0516-Illegal MAC length");
                // return "Illegal MAC address";
            }
        }
    }


    //public class ADR : IEquatable<ADR>
    //{
    //    // todo - does a network number of 0 have any special meaning? how do we indicate a directly connected device?

    //    public bool isBroadcast;                    // todo - 3 types of broadcasts...
    //    public bool isLocalBroadcast;               // see above
    //    public UInt16 networkNumber;
    //    public BACnetMACaddress MACaddress;
    //    public bool directlyConnected;
    //    public myIPEndPoint viaIPEP;                // whether this is direct, or via a router

    //    public ADR()
    //    {
    //        MACaddress = new BACnetMACaddress();
    //    }

    //    public ADR(UInt16 networkNumber, uint MACaddr)
    //    {
    //        MACaddress = new BACnetMACaddress(MACaddr);
    //        this.networkNumber = networkNumber;
    //    }

    //    public ADR(UInt16 networkNumber, myIPEndPoint ipep)
    //    {
    //        if (networkNumber == 0) throw new Exception("m0160-Network Number of 0 is illegal");
    //        this.networkNumber = networkNumber;
    //        this.MACaddress = new BACnetMACaddress(ipep);
    //    }

    //    public ADR( myIPEndPoint ipep)
    //    {
    //        // if there is _only_ an ipep, then this has to be directly connected. 
    //        directlyConnected = true;
    //        this.MACaddress = new BACnetMACaddress(ipep);
    //        // this.viaIPEP = ipep;
    //    }

    //    public ADR(BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrtyp)
    //    {
    //        switch (adrtyp)
    //        {
    //            case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
    //                isBroadcast = true;
    //                isLocalBroadcast = true;
    //                viaIPEP = new myIPEndPoint( IPAddress.Broadcast, bnm.insidesocket.ourSocketPort );
    //                break;
    //            case BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST:
    //                this.networkNumber = 0xffff;
    //                isBroadcast = true;
    //                MACaddress = new BACnetMACaddress ( ) ;
    //                break;
    //            case BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST:
    //                throw new Exception("m0510-A remote broadcast requires an IP address, network number, cannot use this constructor for this");
    //            default:
    //                throw new Exception("m0168-Bad parameter");
    //        }
    //    }


    //    public ADR(BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrtyp, UInt16 networkNumber )
    //    {
    //        switch (adrtyp)
    //        {
    //            case BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST:
    //                isBroadcast = true;
    //                viaIPEP = new myIPEndPoint(IPAddress.Broadcast, bnm.insidesocket.ourSocketPort);
    //                this.networkNumber = networkNumber;
    //                break;
    //            case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
    //            case BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST:
    //                throw new Exception("m0511-cannot use this constructor for this");
    //            default:
    //                throw new Exception("m0168-Bad parameter");
    //        }
    //    }


    //    public bool Equals(ADR adr)
    //    {
    //        // no, mac address equality does not depend on directly or indirectly connected status
    //        // if (this.directlyConnected != adr.directlyConnected) return false;

    //        if (this.directlyConnected == true)
    //        {
    //            if (adr.directlyConnected == false) return false;
    //            // compare just the directly connected IP address
    //            if (!this.MACaddress.Equals(adr.MACaddress)) return false;
    //            return true;
    //        }
    //        if (this.networkNumber != adr.networkNumber) return false;
    //        if (this.MACaddress.Equals(adr.MACaddress) != true) return false;
    //        return true;
    //    }


    //    public void SetDestination(BACnetPacket.ADDRESS_TYPE adrTyp)
    //    {
    //        switch (adrTyp)
    //        {
    //            case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
    //                isBroadcast = true;
    //                isLocalBroadcast = true;
    //                break;
    //            default:
    //                throw new Exception("m0182-Not yet implemented");
    //        }
    //    }


    //    public void Decode(byte[] buffer, ref int pos)
    //    {
    //        networkNumber = BACnetLibrary.ExtractUInt16(buffer, ref pos);

    //        if (networkNumber == 0) throw new Exception("m0512-Illegal network number of 0 in decode");

    //        MACaddress.length = buffer[pos++];

    //        switch (MACaddress.length)
    //        {
    //            case 0:
    //                // indicates a broadcast, perfectly legal.
    //                isBroadcast = true;
    //                break;
    //            case 1:
    //                MACaddress.uintMACaddress = buffer[pos++];
    //                break;
    //            case 6:
    //                // extract the IP address
    //                myIPEndPoint ipep = new myIPEndPoint();
    //                ipep.Decode(buffer, pos);
    //                MACaddress.ipMACaddress = ipep;
    //                pos += 6;
    //                break;
    //            default:
    //                throw new Exception("m0517-Illegal MAC address length??");
    //                break;
    //        }
    //    }


    //    public void Encode(byte[] buffer, ref int pos)
    //    {
    //        if (isLocalBroadcast) return;   // local broadcasts dont have dnet, dmac...

    //        if (networkNumber == 0) throw new Exception("m0513 - Illegal Network Number of 0");

    //        buffer[pos++] = (byte)(this.networkNumber >> 8);
    //        buffer[pos++] = (byte)(this.networkNumber & 0xff);

    //        buffer[pos++] = (byte)MACaddress.length;

    //        switch (MACaddress.length)
    //        {
    //            case 0:
    //                // remote or global broadcast
    //                break;
    //            case 1:
    //                buffer[pos++] = (byte)MACaddress.uintMACaddress;
    //                break;
    //            case 6:
    //                MACaddress.ipMACaddress.Encode(buffer, ref pos);
    //                break;
    //            default:
    //                throw new Exception("m0514 - Illegal MAC address length");
    //        }
    //    }


    //    public override string ToString()
    //    {
    //        if (directlyConnected)
    //        {
    //            // then the address should be displayed as the IP address.
    //            return MACaddress.ipMACaddress.ToString();
    //        }
    //        else
    //        {
    //        switch (MACaddress.length)
    //        {
    //            case 0:
    //                return "[Broadcast]";
    //            case 1:
    //                return "[" + networkNumber.ToString() + "/" + MACaddress.uintMACaddress.ToString()+"]";
    //            case 6:
    //                // extract the IP address
    //                return "[" + networkNumber.ToString() + "/" + MACaddress.ipMACaddress.ToString() + "]";
    //            default:
    //                // todo
    //                // ("Implement MAC addresses of other lengths");
    //                return "[m0140-Unimplemented MAC display]";
    //            }
    //        }
    //    }

    //    public myIPEndPoint ResolvedIPEP()
    //    {
    //        if (directlyConnected) return MACaddress.ipMACaddress;
    //        return viaIPEP;
    //    }
    //}


    public class BACnetObjectIdentifier : IEquatable<BACnetObjectIdentifier>
    {
        public BACnetEnums.BACNET_OBJECT_TYPE objectType;
        public uint objectInstance;

        public BACnetObjectIdentifier()
        {
        }

        public BACnetObjectIdentifier(byte[] buf, ref int offset)
        {
            Decode(buf, ref offset);
        }

        public BACnetObjectIdentifier(BACnetEnums.BACNET_OBJECT_TYPE objectType )
        {
            this.objectType = objectType;
            this.objectInstance = 4194303;  // wildcard
        }


        public BACnetObjectIdentifier(BACnetEnums.BACNET_OBJECT_TYPE objectType, uint objectInstance)
        {
            if (objectInstance >= BACnetEnums.BACNET_MAX_INSTANCE) throw new Exception("m0183 - Illegal instance value");

            this.objectType = objectType;
            this.objectInstance = objectInstance;
        }

        public bool Equals(BACnetObjectIdentifier oid)
        {
            if (oid.objectType != this.objectType) return false;
            if (oid.objectInstance != this.objectInstance) return false;
            return true;
        }


        public override string ToString()
        {
            string bs;

            switch (objectType)
            {
                case BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE:
                    bs = "D :";
                    break;
                default:
                    bs = "XX:";
                    break;
            }
            bs += string.Format(objectInstance.ToString("D6"));
            return bs;
        }

        // Create a new Application Tag

        public BACnetObjectIdentifier(byte[] buf, ref int offset, BACnetEnums.TAG tagType, BACnetEnums.BACNET_APPLICATION_TAG appTag)
        {
            // is the next parameter even an application tag 
            if ((buf[offset] & 0x08) != 0x00)
            {
                // we have an unexpected context tag, sort this out
                throw new Exception("m0515-Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                //return;
            }

            if ((BACnetEnums.BACNET_APPLICATION_TAG)(((buf[offset] & 0xf0) >> 4)) != appTag)
            {
                // we have an unexpected context tag, sort this out
                throw new Exception("m0519 - Unexpected application tag xxx , expecting " + appTag.ToString());
            }

            int contextTagSize = buf[offset] & 0x07;

            offset++;

            switch (appTag)
            {
                case BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                    if (contextTagSize != 4)
                    {
                        // we dont have a legal object ID!
                        throw new Exception("m0176-Illegal length");
                        //return;
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
            if ((buf[offset] & 0x08) != 0x08)
            {
                // we have an unexpected context tag, sort this out
                throw new Exception("m0177-Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                //return;
            }

            if ((buf[offset] & 0xf0) != (tagValue << 4))
            {
                // we have an unexpected context tag, sort this out
                throw new Exception("m0174-Unexpected context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                //return;
            }

            int contextTagSize = buf[offset] & 0x07;

            // the length of a bacnet object identifier better be 4

            if (contextTagSize != 4)
            {
                // we have an unexpected context tag, sort this out
                throw new Exception("m0175-Unbelievable length of object identifier");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                //return;
            }


            objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buf[offset + 1] << 2) | ((uint)buf[offset + 2] >> 6));

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
            // todo, check for duplicates. (?)
            if (objectInstance >= (1 << 22)) // may have to be power of 22... (todo)
            {
                throw new Exception("Object Instance out of range " + objectInstance.ToString());
            }
            this.objectInstance = (objectInstance & 0x3fffff);
        }


        public void EncodeApplicationTag(byte[] buffer, ref int pos)
        {
            UInt32 objid = ((UInt32)objectType << 22) | (objectInstance & 0x3ffffff);
            BACnetUtil.InsertApplicationTag(buffer, ref pos, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID, objid);
        }

        public static void EncodeBACnetObjectIdentifierApplicationTag(byte[] buffer, ref int pos, BACnetObjectIdentifier oid)
        {
            oid.EncodeApplicationTag(buffer, ref pos);
        }

        public void EncodeContextTag(byte[] buffer, ref int pos)
        {

        }


        public void Encode(byte[] buffer, ref int pos)
        {
            BACnetUtil.EncodeContextTag(buffer, ref pos, 0, (int)(((UInt32)objectType << 22) | objectInstance), 4);
        }


        public void DecodeContextTag(byte[] buffer, ref int pos)
        {
            if ((buffer[pos++] & 0x0f) != (0x08 | 0x04))
            {
                throw new Exception("m0045 - Illegal context tag for Object Identifier");
            }
            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }


        public void DecodeApplicationTag(byte[] buffer, ref int pos)
        {
            // get the tag class, length

            uint cl = buffer[pos++];

            if (cl != 0xc4)
            {
                throw new Exception("m0518 - Missing Application Tag for Object Identifier");
            }

            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }


        public void Decode(byte[] buffer, ref int pos)
        {
            // get the tag class, length

            uint cl = buffer[pos++];

            if (cl != 0xc4)
            {
                throw new Exception("m0041 - Missing Application Tag for Object Identifier");
            }

            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }
    }


    public class BACnetUtil
    {
        public static void SendDebugString(String msg)
        {
            UdpClient newsock = new UdpClient();

            byte[] data = new byte[2000];

            Encoding.ASCII.GetBytes("Eddie", 0, 5, data, 0);

            // encode the message
            data[5] = 1;
            // string welcome = "A message from the CloudRouter";
            int length = Encoding.ASCII.GetBytes(msg, 0, msg.Length, data, 6);
            data[6 + length] = 0;

            newsock.Send(data, length + 7, "debug01cloudrouter.dyndns.org", 502);
        }



        public static byte[] StrToByteArray(string s)
        {
            List<byte> value = new List<byte>();
            // foreach (char c in s.ToCharArray()) value.Add(c.ToByte());
            foreach (char c in s.ToCharArray())
            {
                value.Add(Convert.ToByte(c));
            }
            return value.ToArray();
        }

        //public static void Panic(String message)
        //{
        //    throw new Exception("m0039 - Old style Panic");
        //}


        static public UInt16 ExtractBACnetPort(byte[] buffer, ref int iptr)
        {
            UInt16 bacnetport = ExtractUInt16(buffer, ref iptr);
            if (bacnetport < 47808 || bacnetport > 47811)
            {
                throw new Exception("m0119 - Illegal BACnet Port value; " + bacnetport.ToString());
            }
            return bacnetport;
        }


        static public int ExtractInt16(byte[] buffer, ref int iptr)
        {
            int tint = 0;
            tint = (int)buffer[iptr++] << 8;
            tint |= (int)buffer[iptr++];
            return tint;
        }

        static public int ExtractContextOpeningTag(byte[] buffer, ref int iptr, int context)
        {
            int tint = buffer[iptr++];
            if ((tint & 0x0e) != 0x0e) throw new Exception("m0207 - Expected opening context tag - no tag");
            if ((tint & 0xf0) != (context << 4)) throw new Exception("m0208 - Expected opening context tag - wrong context number");
            return ((tint & 0xf0) >> 4);
        }

        static public int ExtractContextClosingTag(byte[] buffer, ref int iptr, int context)
        {
            int tint = buffer[iptr++];
            if ((tint & 0x0e) != 0x0e) throw new Exception("m0209 - Expected closing context tag - no tag");
            if ((tint & 0xf0) != (context << 4)) throw new Exception("m0210 - Expected closing context tag - wrong context number");
            return ((tint & 0xf0) >> 4);
        }

        static public UInt16 ExtractUInt16(byte[] buffer, ref int iptr)
        {
            UInt16 tint = 0;
            tint = (UInt16)(buffer[iptr++] << 8);
            tint |= (UInt16)buffer[iptr++];
            return tint;
        }

        static public uint ExtractUint32(byte[] buffer, ref int iptr)
        {
            uint tint = 0;
            tint = (uint)buffer[iptr++] << 24;
            tint |= (uint)buffer[iptr++] << 16;
            tint |= (uint)buffer[iptr++] << 8;
            tint |= (uint)buffer[iptr++];
            return tint;
        }


        static public uint ExtractApplicationTagUint(byte[] buffer, ref int optr)
        {
            if ((buffer[optr] & 0x08) == 0x08) throw new Exception("m0520 - Not an application tag");

            int length = buffer[optr] & 0x07;
            int retval = 0;

            if (length > 4) throw new Exception("m0075 - illegal uint length");

            if ((buffer[optr++] >> 4) != (int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT)
            {
                throw new Exception("m0521 - Application tag type not uint");
            }
            for (int i = 0; i < length; i++)
            {
                retval <<= 8;
                retval |= buffer[optr++];
            }
            return (uint)retval;
        }


        static public List<bool> ExtractApplicationTagBitString(byte[] buffer, ref int optr)
        {
            if ((buffer[optr] & 0x08) == 0x08) throw new Exception("m0211 - Not an application tag");

            int lengthInBytes = buffer[optr++] & 0x07;
            if (lengthInBytes == 5)
            {
                // extended value, next byte is the length
                lengthInBytes = buffer[optr++];
            }
            int remainder = buffer[optr++];
            int lengthInBits = (lengthInBytes-1) * 8 - remainder;

            List<bool> tlist = new List<bool>();

            for (int i = 0; i < lengthInBits; i++)
            {
                int b = i / 8 ;
                int m = ( 1 << ( b % 8 ) ) ;
                if ((buffer[optr+b] & m) != 0)
                {
                    tlist.Add(true);
                }
                else
                {
                    tlist.Add(false);
                }
            }
            optr += lengthInBytes-1;
            return tlist;
        }


        static public int ExtractApplicationTagEnum(byte[] buffer, ref int optr)
        {
            if ((buffer[optr] & 0x08) == 0x08) throw new Exception("m0079 - Not an application tag");

            int length = buffer[optr] & 0x07;
            int retval = 0;

            if (length > 4) throw new Exception("m0075 - illegal uint length");

            if ((buffer[optr++] >> 4) != (int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED)
            {
                throw new Exception("m0076 - Application tag type not an Enum");
            }
            for (int i = 0; i < length; i++)
            {
                retval <<= 8;
                retval |= buffer[optr++];
            }
            return retval;
        }



        static public uint ExtractContextTagUint(byte[] buffer, ref int optr, int contextTagNumber)
        {
            if ((buffer[optr] & 0x08) != 0x08) throw new Exception("m0077 - Not a context tag");
            int length = buffer[optr] & 0x07;
            int retval = 0;

            if (length > 4) throw new Exception("m0078 - illegal uint length");

            if ((buffer[optr++] >> 4) != contextTagNumber) throw new Exception("m0079 - Context tag number does not match");

            for (int i = 0; i < length; i++)
            {
                retval <<= 8;
                retval |= buffer[optr++];
            }
            return (uint)retval;
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


        static public void InsertApplicationTagUint16(byte[] buffer, ref int optr, UInt16 value)
        {
            // We need a slightly special case of UInt16 Application Tag because some BACnet Packets call out for UInt16 specifically. e.g. I-Am.
            // http://www.bacnetwiki.com/wiki/index.php?title=Application_Tags

            buffer[optr++] = (byte)(((int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT << 4) | 2);

            buffer[optr++] = (byte)((value >> 8) & 0xff);
            buffer[optr++] = (byte)(value & 0xff);
        }


        static public unsafe string ExtractString(byte[] buf, ref int iptr)
        {
            // first byte is length (incl encoding), then encoding, then string.
            if (buf[iptr + 1] != 0) throw new Exception("m0097 - Cannot handle this string encoding yet");
            if (buf[iptr] == 0) throw new Exception("m0129 - Strings cannot be zero length (yet)");

            fixed (byte* a = &buf[iptr])
            {
                sbyte* sa = (sbyte*)a;
                string ns = new string(sa, 2, buf[iptr] - 1);
                iptr += buf[iptr] + 1;
                return ns;
            }
        }


        static public void InsertString(byte[] buf, ref int optr, string strval)
        {
            buf[optr++] = (byte)(strval.Length + 1);
            // character set - ascii
            buf[optr++] = 0;

            // todo, handling ascii encoding only for now..
            byte[] tarray = StrToByteArray(strval);

            tarray.CopyTo(buf, optr);
            optr += strval.Length;
        }


        static public void InsertApplicationTagString(byte[] buffer, ref int optr, string strval)
        {
            buffer[optr++] = 0x75;

            // todo, use above InsertString...
            buffer[optr++] = (byte)(strval.Length + 1);
            // character set - ascii
            buffer[optr++] = 0;
            // todo, handling ascii encoding only for now..
            byte[] tarray = StrToByteArray(strval);
            tarray.CopyTo(buffer, optr);
            optr += strval.Length;
        }



        static public void EncodeContextTag(byte[] buffer, ref int optr, int contextNumber, int value, int len)
        {
            // 0x08 is flag indicating this is a context tag.

            buffer[optr++] = (byte)((contextNumber << 4) | 0x08 | len);

            for (int i = 0; i < len; i++)
            {
                buffer[optr + i] = (byte)((value >> (len - i - 1) * 8) & 0xff);
            }
            optr += len;
        }


        // This encodes a context tag with no specific length...

        static public void EncodeContextTag(byte[] buffer, ref int optr, int contextNumber, int value)
        {
            // For description of Context Tag structure
            // http://www.bacnetwiki.com/wiki/index.php?title=Context

            int len;
            UInt32 tval = (UInt32)value;

            if (tval < 0x100)
            {
                len = 1;
            }
            else if (tval < 0x10000)
            {
                len = 2;
            }
            else if (tval < 0x1000000)
            {
                len = 3;
            }
            else
            {
                len = 4;
            }
            EncodeContextTag(buffer, ref optr, contextNumber, value, len);
        }


        static public void InsertContextClosingTag(byte[] buffer, ref int optr, int contextNumber)
        {
            buffer[optr++] = (byte)((contextNumber << 4) | 0x0f);
        }


        static public void InsertContextOpeningTag(byte[] buffer, ref int optr, int contextNumber)
        {
            buffer[optr++] = (byte)((contextNumber << 4) | 0x0e);
        }


        static public void InsertApplicationTag(byte[] buffer, ref int optr, BACnetEnums.BACNET_APPLICATION_TAG apptag, UInt32 value)
        {
            int saveOptr = optr++;
            int length = 1;

            if (value >= (1 << 24))
            {
                buffer[optr++] = (byte)((value >> 24) & 0xff);
                length++;
            }
            if (value >= (1 << 16))
            {
                buffer[optr++] = (byte)((value >> 16) & 0xff);
                length++;
            }
            if (value >= (1 << 8))
            {
                buffer[optr++] = (byte)((value >> 8) & 0xff);
                length++;
            }
            buffer[optr++] = (byte)(value & 0xff);
            buffer[saveOptr] = (byte)(((int)apptag << 4) | length);
        }


        // the better method is to populate the pkt structure, then call encode...
        public static void ReadPropertyObjectList_deprecated(BACnetManager bnm, Device device)
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
            device.deviceObjectID.Encode(data, ref optr);

            // Property Identifier (Object List)

            data[optr++] = 0x19;            //  19  Context Tag: 1, Length/Value/Type: 1
            data[optr++] = 0x4c;            //  4c  Property Identifier: object-list (76)

            BACnetUtil.InsertInt16(data, ref store_length_here, optr);

            bnm.insideSocket.OurSendTo(data, optr, device.packet.directlyConnectedIPEndPointOfDevice);
        }


       public static void InsertBitString(byte[] outbuf, int optr, int maxbytes, int bit)
        {
            if (bit / 8 + 1 > maxbytes) throw new Exception("m0057 - Bitstring range exceeded");

            outbuf[optr + bit / 8] |= (byte)(0x80 >> (bit % 8));
        }


        public static void SendWhoIs(AppManager apm, BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrTyp)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
            // todo, set adr destination issues, local, remote, global...
            pkt.SetDestination(adrTyp);
            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, new myIPEndPoint(IPAddress.Broadcast, bnm.insideSocket.ourSocketPort));
        }


        public static void SendWhoIs(AppManager apm, BACnetManager bnm, Device device)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
            pkt.SetDestination(new DADR(device.adr));
            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, device.adr.ResolvedIPEP());
        }


        public static void SendWhoIs(AppManager apm, BACnetManager bnm, DADR dadr)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS);
            pkt.SetDestination(dadr);
            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, dadr.ResolvedIPEP());
        }


        public static void SendInitRoutingTable(AppManager apm, BACnetManager bnm, IPEndPoint IPEPDestination)
        {
            BACnetPacket pkt = new BACnetPacket(apm);

            int optr = 0;
            byte[] outbuf = new byte[2000];

            //pkt.SetMessageType(BACnetPacket.MESSAGE_TYPE.NETWORK_LAYER);
            pkt.SetDestination(BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST);
            pkt.SetNetworkLayerMessageFunction(BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE);

            pkt.EncodeBACnet(outbuf, ref optr);

            bnm.insideSocket.OurSendTo(outbuf, optr, IPEPDestination);
        }


        public static void SendInitRoutingTable(AppManager apm, BACnetManager bnm, DADR dadr)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE);
            pkt.SetDestination(dadr);
            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, dadr.ResolvedIPEP());
        }


        public static void SendWhoIsRouter(AppManager apm, BACnetManager bnm, BACnetPacket.ADDRESS_TYPE dest)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE);
            pkt.SetDestination(dest);
            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, new myIPEndPoint(IPAddress.Broadcast, bnm.insideSocket.ourSocketPort));
        }

        public static void SendOffPacket(AppManager apm, BACnetManager bnm, BACnetPacket pkt, byte[] outbuf, int optr)
        {
            int npduLengthOffset = pkt.npduLengthOffset ;
            InsertUint16(outbuf, ref npduLengthOffset, (uint) optr);

            if (pkt.dAdr.directlyConnected == true)
            {
                apm.bnm.insideSocket.OurSendTo(outbuf, optr, (IPEndPoint)pkt.dAdr.MACaddress.ipMACaddress);
            }
            else
            {
                if ( pkt.dAdr.isLocalBroadcast || ( pkt.dAdr.isBroadcast && ! pkt.dAdr.isRemoteBroadcast ) )
                {
                    // todonow, what is our port number??
                    apm.bnm.insideSocket.OurSendTo(outbuf, optr, new IPEndPoint(IPAddress.Broadcast, bnm.insideSocket.ourSocketPort) );
                }
                else
                {
                    apm.bnm.insideSocket.OurSendTo(outbuf, optr, (IPEndPoint)pkt.dAdr.viaIPEP);
                }
            }

            //PacketLog pktlog = new PacketLog(true, pkt.srcPortNet.portNetEPIP, crp);
            //pktlog.BACnetPacket = (BACnetPacket)crp;
            //apm.bnm.BACnetMessageLog.myEnqueue(pktlog);
        }



        public static void SendReadProperty(AppManager apm, BACnetManager bnm, Device device, BACnetObjectIdentifier oid, BACnetEnums.BACNET_PROPERTY_ID property)
        {
            SendReadProperty(apm, bnm, device, oid, (int) property);
        }


        public static void SendReadProperty(AppManager apm, BACnetManager bnm, Device device, BACnetObjectIdentifier oid, int property)
        {
            BACnetPacket pkt = new BACnetPacket(apm);
            pkt.EncodeBVLL( new DADR ( device.adr ), BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU );
            pkt.EncodeNPCI(new DADR(device.adr), BACnetLibrary.BACnetPacket.MESSAGE_TYPE.APPLICATION );
            // todonow, put these next steps into functions
            // build confirmed request header 
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU
            pkt.buffer[pkt.optr++] = (Byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST;
            pkt.buffer[pkt.optr++] = 0x05;  // max segs, max resp. todo
            pkt.buffer[pkt.optr++] = apm.invokeID++;
            // sequence number may come next, dep on flags. todo
            pkt.buffer[pkt.optr++] = (byte)BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY;
            // context encoding
            // http://www.bacnetwiki.com/wiki/index.php?title=Read_Property
            oid.Encode(pkt.buffer, ref pkt.optr);
            BACnetUtil.EncodeContextTag(pkt.buffer, ref pkt.optr, 1, property);
            SendOffPacket(apm, bnm, pkt, pkt.buffer, pkt.optr);
        }


        public static void SendReadProperty(AppManager apm, BACnetManager bnm, Device device, BACnetObjectIdentifier oid, BACnetEnums.BACNET_PROPERTY_ID property, int index)
        {
            BACnetPacket pkt = new BACnetPacket(apm);
            pkt.EncodeBVLL(new DADR(device.adr), BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU);
            pkt.EncodeNPCI(new DADR(device.adr), BACnetLibrary.BACnetPacket.MESSAGE_TYPE.APPLICATION);
            // todonow, put these next steps into functions
            // build confirmed request header 
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU
            pkt.buffer[pkt.optr++] = (Byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST;
            pkt.buffer[pkt.optr++] = 0x05;  // max segs, max resp. todo
            pkt.buffer[pkt.optr++] = apm.invokeID++;
            // sequence number may come next, dep on flags. todo
            pkt.buffer[pkt.optr++] = (byte)BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY;
            // context encoding
            // http://www.bacnetwiki.com/wiki/index.php?title=Read_Property
            oid.Encode(pkt.buffer, ref pkt.optr);
            BACnetUtil.EncodeContextTag(pkt.buffer, ref pkt.optr, 1, (int) property);
            BACnetUtil.EncodeContextTag(pkt.buffer, ref pkt.optr, 2, (int) index);
            SendOffPacket(apm, bnm, pkt, pkt.buffer, pkt.optr);
        }



        public static void SendReadProperty_old(AppManager apm, BACnetManager bnm, Device device, BACnetObjectIdentifier oid, BACnetEnums.BACNET_PROPERTY_ID property)
        {
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY);

            DADR dadr = new DADR(device.adr);

            pkt.SetDestination(dadr);

            pkt.objectID = oid;
            pkt.propertyID = property;

            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, dadr.ResolvedIPEP());
        }


        public static void SendReadProtocolServices(AppManager apm, BACnetManager bnm, Device device)
        {
            // todo, use the above function
            BACnetPacket pkt = new BACnetPacket(apm, BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY);

            DADR dadr = new DADR(device.adr);

            pkt.SetDestination(dadr);

            pkt.objectID = device.deviceObjectID;
            pkt.propertyID = BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_SERVICES_SUPPORTED;

            pkt.EncodeBACnet();
            bnm.insideSocket.OurSendTo(pkt.buffer, pkt.optr, dadr.ResolvedIPEP());
        }


        public static void SendIAm(AppManager apm, BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrTyp, uint deviceInstance)
        {
            BACnetPacket pkt = new BACnetPacket(apm);
            pkt.EncodeBVLL(BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU);
            pkt.EncodeNPCI(new DADR(BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST), BACnetLibrary.BACnetPacket.MESSAGE_TYPE.APPLICATION);
            pkt.EncodeAPDUheader(BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST, BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM);

            // now encode the variable service request for I-Am
            // http://www.bacnetwiki.com/wiki/index.php?title=I-am

            // Application tags:
            // Device Identifier (Device Object ID)
            pkt.EncodeApplicationTag(new BACnetObjectIdentifier(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE, deviceInstance));

            // Max APDU
            pkt.EncodeApplicationTag((uint)1476);

            // segmentation supported (enum)
            pkt.EncodeApplicationTag( BACnetEnums.BACNET_SEGMENTATION.SEGMENTATION_NONE );

            // Vendor ID
            pkt.EncodeApplicationTag((uint)323);

            SendOffPacket(apm, bnm, pkt, pkt.buffer, pkt.optr);
        }



    }
}
