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

namespace BACnetLibrary
{
    class BACnetEncoding
    {

        //public static uint DecodeTagContextUint(byte[] buf, ref int offset, int expectedTagValue)
        //{
        //    // is the next parameter even a context tag 
        //    if ((buf[offset] & 0x08) != 0x08)
        //    {
        //        throw new Exception("m0060 - Expecting a context tag, but none found");
        //    }

        //    if ((buf[offset] & 0xf0) != (expectedTagValue << 4))
        //    {
        //        // we have an unexpected context tag, sort this out
        //        throw new Exception ("m0526 - Unexpected context tag index");
        //        // todo, now is there a way to avoid creating the object? Have to flag it at least...
        //    }

        //    int contextTagSize = buf[offset] & 0x07;
        //    int a = 0;

        //    switch (contextTagSize )
        //    {
        //        case 1:
        //            a = buf[offset+1];
        //            offset += 2;
        //            return (uint) a;

        //        case 2:
        //            a = buf[offset + 1] << 8 ;
        //            a |= buf[offset + 2] ;
        //            offset += 3;
        //            return (uint)a;

        //        case 3:
        //            a = buf[offset + 1] << 16;
        //            a |= buf[offset + 2] << 8;
        //            a |= buf[offset + 3];
        //            offset += 4;
        //            return (uint) a;

        //        case 4:
        //            a = buf[offset + 1] << 24;
        //            a |= buf[offset + 2] << 16;
        //            a |= buf[offset + 3] << 8 ;
        //            a |= buf[offset + 4] ;
        //            offset += 5;
        //            return (uint)a;

        //        default:
        //            // we have an unexpected context tag, sort this out
        //            throw new Exception ("m0035 - Unbelievable length of object identifier");
        //    }
        //}


        public static int BACnetDecode_uint_deprecated( byte[] buffer, int offset, out uint value)
        {
            // take a look at the first octet, this will indicate what type of encoded entity (Tag) we need to decode.
            // See: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

            int len = buffer[offset] & 0x07;

            if ((buffer[offset] & 0x08) == 0x08)
            {
                // we have a Context Tag, todo
                //throw;
            }

            // See: http://www.bacnetwiki.com/wiki/index.php?title=Tag_Number

            switch ((buffer[offset] & 0xf0) >> 4)
            {
                case 0:

                case 1:

                case (int) BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT:
                case (int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED:
                    // this means the lower nibble is the length - which we prepared when we created the variable..
                    switch (len)
                    {
                        case 1:
                            value = buffer[offset+1];
                            return 2;
                        case 2:
                            value = (uint) ( buffer[offset + 1] * 256 + buffer[offset + 2] );
                            return 3;
                        default:
                        //todo - panic
                            break;
                    }
                    break;

                case (int) BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                    // todo - should split into a type and an instance. return an int for now (4byte)
                    // this means the lower nibble is the length - which we prepared when we created the variable..
                    switch (len)
                    {
                        case 4:
//                            value = (uint)((uint)buffer[offset + 1] * (2 ^ 24) + (uint)buffer[offset + 2] * (2 ^ 16) + buffer[offset + 3] * 256 + buffer[offset + 4]);
                            value = (uint)( (buffer[offset + 1] << 24) + (buffer[offset + 2] << 16) + (buffer[offset + 3] * 256) + buffer[offset + 4]);
                            return 5;
                        default:
                            //todo - panic
                            break;
                    }
                    break;
                 
                default:
                    // todo - panic
                    //throw;
                    break;
            }

            value = 0;
            return 0;
        }
    }
}
