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
using System.Text;
using System.Net;

namespace BACnetLibrary
{
    public class ADR : IEquatable<ADR> // , ICloneable
    {
        // todo - does a network number of 0 have any special meaning? how do we indicate a directly connected device?

        public UInt16 networkNumber;
        public BACnetMACaddress MACaddress;
        public bool directlyConnected = false;
        public myIPEndPoint viaIPEP;                // whether this is direct, or via a router

        public ADR()
        {
        }

        public ADR(UInt16 networkNumber, uint MACaddr)
        {
            MACaddress = new BACnetMACaddress(MACaddr);
            this.networkNumber = networkNumber;
        }

        public ADR(UInt16 networkNumber, string MACaddr )
        {
            MACaddress = new BACnetMACaddress( MACaddr);
            this.networkNumber = networkNumber;
        }

        public ADR(UInt16 networkNumber, myIPEndPoint ipep)
        {
            // todo if (networkNumber == 0) throw new Exception("m0160-Network Number of 0 is illegal");
            this.networkNumber = networkNumber;
            this.MACaddress = new BACnetMACaddress(ipep);
        }


        public ADR(myIPEndPoint ipep)
        {
            // if there is _only_ an ipep, then this has to be directly connected. 
            directlyConnected = true;
            this.MACaddress = new BACnetMACaddress(ipep);
        }


        
        //public object Clone()
        //{
        //    return this.MemberwiseClone();
        //}

        public bool Equals(ADR adr)
        {
            if (this.directlyConnected == true)
            {
                if (adr.directlyConnected == false) return false;
                // compare just the directly connected IP address
                if (!this.MACaddress.Equals(adr.MACaddress)) return false;
                return true;
            }
            if (this.networkNumber != adr.networkNumber) return false;
            if (this.MACaddress.Equals(adr.MACaddress) != true) return false;
            return true;
        }

        public virtual void Decode(byte[] buffer, ref int pos, bool tolerate0NN )
        {
            networkNumber = BACnetUtil.ExtractUInt16(buffer, ref pos);

            if ( ! tolerate0NN && networkNumber == 0)
            {
                throw new Exception("m0161-Illegal network number of 0 in decode");

                // So there is at least one router out there that includes a 0 in the NN part of the SADR when sending a who-is-router. This is benign, because routers
                // broadcast their i-am routers. In any event FOR NOW, tolerate this transgression or else comms will not happen on these networks.
                // but we do need to find a cleaner way of dealing with this!!
            }
                

            switch (buffer[pos++])
            {
                case 0:
                    // illegal for sadr
                    throw new Exception("m0178-MAC length of 0 illegal for SADR");
                    // break;
                case 1:
                    MACaddress = new BACnetMACaddress(buffer[pos++]);
                    break;
                case 6:
                    // extract the IP address
                    myIPEndPoint ipep = new myIPEndPoint();
                    ipep.Decode(buffer, ref pos);
                    MACaddress = new BACnetMACaddress(ipep);
                    //MACaddress.ipMACaddress = ipep;
                    break;
                default:
                    throw new Exception("m0178-Illegal MAC address length??");
            }
        }


        public virtual void Encode(byte[] buffer, ref int pos)
        {
            if (networkNumber == 0) throw new Exception("m0145 - Illegal Network Number of 0");

            buffer[pos++] = (byte)(this.networkNumber >> 8);
            buffer[pos++] = (byte)(this.networkNumber & 0xff);

            buffer[pos++] = (byte)MACaddress.length;

            switch (MACaddress.mat)
            {
                case BACnetMACaddress.MACaddrType.None:
                    // remote or global broadcast
                    break;
                case BACnetMACaddress.MACaddrType.Single :
                    buffer[pos++] = (byte)MACaddress.uintMACaddress;
                    break;
                case BACnetMACaddress.MACaddrType.IPEP:
                    MACaddress.ipMACaddress.Encode(buffer, ref pos);
                    break;
                case BACnetMACaddress.MACaddrType.ByteArray:
                    for (int i = 0; i < MACaddress.length; i++) buffer[pos++] = MACaddress.byteMACaddr[i];
                    break;
                default:
                    throw new Exception("m0146 - Illegal MAC address length");
            }
        }

        public override string ToString()
        {
            if (directlyConnected)
            {
                // then the address should be displayed as the IP address.
                return "[" + MACaddress.ipMACaddress.ToString() + "]";
            }
            else
            {
                switch (MACaddress.mat)
                {
                    case BACnetMACaddress.MACaddrType.None:
                        return "[Local Broadcast (should not see this - logic above)]";
                    case BACnetMACaddress.MACaddrType.Single:
                        return "[" + networkNumber.ToString() + "/" + MACaddress.uintMACaddress.ToString() + "] @ " + viaIPEP.ToString() ;
                    case BACnetMACaddress.MACaddrType.IPEP:
                        // extract the IP address
                        return "[" + networkNumber.ToString() + "/" + MACaddress.ipMACaddress.ToString() + "] @ " + viaIPEP.ToString();
                    case BACnetMACaddress.MACaddrType.ByteArray:
                        // extract the IP address
                        string ts = "[";
                        for (int i = 0; i < MACaddress.length; i++) ts += String.Format ( "{0:x,2} ", MACaddress.byteMACaddr[i] );
                        return ts + "] @ " + viaIPEP.ToString();
                    default:
                        // todo
                        // ("Implement MAC addresses of other lengths");
                        return "[m0140-Unimplemented MAC display]";
                }
            }
        }

        public myIPEndPoint ResolvedIPEP()
        {
            if (directlyConnected) return MACaddress.ipMACaddress;
            return viaIPEP;
        }

    }


    public class DADR : ADR
    {
        public bool isBroadcast;                    // todo - 3 types of broadcasts...
        public bool isLocalBroadcast;               // if local and remote are false, then it is a global b'cast
        public bool isRemoteBroadcast;

        public DADR()
        {
            // A dest address starts out this way.... surely a source address can too?
            isBroadcast = true;
            isLocalBroadcast = true;
            // todonow 
            // MACaddress = new BACnetMACaddress();
        }

        public DADR(ADR a)
        {
            //todo - memberwise clone?
            this.networkNumber = a.networkNumber;
            this.MACaddress = a.MACaddress;
            this.directlyConnected = a.directlyConnected;
            this.viaIPEP = a.viaIPEP;                // whether this is direct, or via a router

        }

        public DADR(UInt16 networkNumber, uint MACaddr)
            : base(networkNumber, MACaddr)
        {
            if (networkNumber == 0xffff)
            {
                isLocalBroadcast = false;
                isRemoteBroadcast = false;
                isBroadcast = true;
            }
        }

        public DADR(UInt16 networkNumber, myIPEndPoint ipep)
            : base(networkNumber, ipep)
        {
            // todo if (networkNumber == 0) throw new Exception("m0160-Network Number of 0 is illegal");
            //this.networkNumber = networkNumber;
            //this.MACaddress = new BACnetMACaddress(ipep);
            if (networkNumber == 0xffff)
            {
                isLocalBroadcast = false;
                isRemoteBroadcast = false;
                isBroadcast = true;
            }
        }

        public DADR(BACnetPacket.ADDRESS_TYPE adrtyp)
        {
            switch (adrtyp)
            {
                case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
                    throw new Exception("m0205-A local broadcast requires an IP address, network number, cannot use this constructor for this");
                case BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST:
                    this.networkNumber = 0xffff;
                    isBroadcast = true;
                    isLocalBroadcast = false;
                    isRemoteBroadcast = false;
                    MACaddress = new BACnetMACaddress();
                    break;
                case BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST:
                    throw new Exception("m0203-A remote broadcast requires an IP address, network number, cannot use this constructor for this");
                default:
                    throw new Exception("m0204-Bad parameter");
            }
        }

        public DADR(BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrtyp)
        {
            switch (adrtyp)
            {
                case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
                    isBroadcast = true;
                    isLocalBroadcast = true;
                    viaIPEP = new myIPEndPoint(IPAddress.Broadcast, bnm.insideSocket.ourSocketPort);
                    break;
                case BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST:
                    this.networkNumber = 0xffff;
                    isBroadcast = true;
                    isLocalBroadcast = false;
                    isRemoteBroadcast = false;
                    MACaddress = new BACnetMACaddress();
                    break;
                case BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST:
                    isRemoteBroadcast = true;
                    isLocalBroadcast = false;
                    throw new Exception("m0167-A remote broadcast requires an IP address, network number, cannot use this constructor for this");
                default:
                    throw new Exception("m0168-Bad parameter");
            }
        }


        public DADR(BACnetManager bnm, BACnetPacket.ADDRESS_TYPE adrtyp, UInt16 networkNumber)
        {
            switch (adrtyp)
            {
                case BACnetPacket.ADDRESS_TYPE.REMOTE_BROADCAST:
                    isBroadcast = true;
                    viaIPEP = new myIPEndPoint(IPAddress.Broadcast, bnm.insideSocket.ourSocketPort);
                    this.networkNumber = networkNumber;
                    break;
                case BACnetPacket.ADDRESS_TYPE.LOCAL_BROADCAST:
                case BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST:
                    throw new Exception("m0169-cannot use this constructor for this");
                default:
                    throw new Exception("m0168-Bad parameter");
            }
        }


        public void Decode(byte[] buffer, ref int pos )
        {
            networkNumber = BACnetUtil.ExtractUInt16(buffer, ref pos);

            if (networkNumber == 0) throw new Exception("m0205-Illegal network number of 0 in decode");

            switch (buffer[pos++])
            {
                case 0:
                    // indicates a remote, or possibly a global, broadcast, perfectly legal.
                    isBroadcast = true;
                    isLocalBroadcast = false;
                    if (networkNumber != 0xffff)
                    {
                        isRemoteBroadcast = true;
                    }
                    else
                    {
                        // a remote b'cast with a dest network number is a global broadcast
                        isRemoteBroadcast = false;
                    }
                    break;
                case 1:
                    MACaddress = new BACnetMACaddress(buffer[pos++]);
                    //MACaddress.length = buffer[pos++];
                    //MACaddress.uintMACaddress = buffer[pos++];
                    break;
                case 6:
                    // extract the IP address
                    myIPEndPoint ipep = new myIPEndPoint();
                    ipep.Decode(buffer, ref pos);
                    MACaddress = new BACnetMACaddress(ipep);
                    //MACaddress.ipMACaddress = ipep;
                    // pos += 6;
                    break;
                default:
                    throw new Exception("m0178-Illegal MAC address length??");
                //break;
            }
        }


        public override void Encode(byte[] buffer, ref int pos)
        {
            if (isLocalBroadcast) return;   // local broadcasts dont have dnet, dmac...
            if (isBroadcast && !isRemoteBroadcast) networkNumber = 0xffff;
            base.Encode(buffer, ref pos);
        }


        public override string ToString()
        {
            if (isBroadcast && isLocalBroadcast)
            {
                return "[Local Broadcast]";
            }
            else if (isBroadcast && isRemoteBroadcast)
            {
                return "[Remote Broadcast to Network " + networkNumber.ToString() + "]";
            }
            else if (isBroadcast)
            {
                return "[Global Broadcast]";
            }
            return base.ToString();
        }

    }
}
