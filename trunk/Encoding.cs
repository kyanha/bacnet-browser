using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BACnetLibraryNS;

namespace BACnetInteropApp
{
    class BACnetEncoding
    {
        public static int BACnetDecode_uint( byte[] buffer, int offset, out uint value)
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