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

namespace BACnetLibrary
{
    //public class RoutingTableEntry : IEquatable<RoutingTableEntry>
    //{
    //    public uint networkNumber;
    //    public uint portID;
    //    public uint portInfoLen;
    //    public byte[] portInfo;

    //    public bool seenOnRouterInit;           // These flags help us establish that.
    //    public bool farSide;                    // This BACnet Network is definitely on the far side of another router.
    //    public bool nearSide;


    //    public RoutingTableEntry()
    //    {
    //    }

    //    public RoutingTableEntry( int networkNumber)
    //    {
    //        this.networkNumber = (uint) networkNumber;
    //    }

    //    public bool Equals( RoutingTableEntry rp )
    //    {
    //        if (this.networkNumber != rp.networkNumber) return false;
    //        return true;
    //    }

    //    public bool Decode ( byte[] buf, ref int iptr )
    //    {
    //        networkNumber = BACnetLibrary.ExtractUInt16(buf, ref iptr);
    //        portID = buf[iptr++];
    //        portInfoLen = buf[iptr++];
    //        if (portInfoLen != 0)
    //        {
    //            // we are not ready to handle this.
    //            BACnetLibrary.Panic("todo");
    //        }
    //        iptr += (int) portInfoLen;
    //        return true;
    //    }




    public class NPDU
    {
        // public bool isNPDUmessage;
        // public bool isBroadcast;
        public bool expectingReply;

        public BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE function;


        public void Copy(NPDU src)
        {
            // isNPDUmessage = src.isNPDUmessage; // todo, reverse this, it should be at the packet level (like I reversed the APDU)
            // isBroadcast = src.isBroadcast;
            expectingReply = src.expectingReply;
        }
    }
}
