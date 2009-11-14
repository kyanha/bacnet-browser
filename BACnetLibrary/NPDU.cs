using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibraryNS
{
    public class RouterPort
    {
        public uint networkNumber;
        public uint ID;
        public uint portInfoLen;
        public byte[] portInfo;

        public bool Decode ( byte[] buf, ref int iptr )
        {
            networkNumber = BACnetLibraryCL.ExtractUint16(buf, ref iptr);
            ID = buf[iptr++];
            portInfoLen = buf[iptr++];
            if (portInfoLen != 0)
            {
                // we are not ready to handle this.
                BACnetLibraryCL.Panic("todo");
            }
            iptr += (int) portInfoLen;
            return true;
        }


    }


    public class NPDU
    {
        public bool isNPDUmessage;
        public bool isBroadcast;
        public bool expectingReply;

        public BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE function;


        public void Copy(NPDU src)
        {
            isNPDUmessage = src.isNPDUmessage; // todo, reverse this, it should be at the packet level (like I reversed the APDU)
            isBroadcast = src.isBroadcast;
            expectingReply = src.expectingReply;
        }
                //switch (npdu.function)
                //{
                //    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_REJECT_MESSAGE_TO_NETWORK:
                //        System.Windows.Forms.MessageBox.Show("Network Message \"Reject Message to Network\" received");
                //        break;
                //    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                //    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE:
                //        break;
                //    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                //        Console.WriteLine("Router-Table-Ack");
                //        break;
                //    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                //        // todo - conformance, check that this is _always_ a broadcast message.
                //        Console.WriteLine("I-Am-Router-to-Network");
                //        // Parse list of network numbers available via this router.
                //        break;
                //    default:
                //        System.Windows.Forms.MessageBox.Show("Vendor Proprietory Network Message received");
                //        break;
                //}

    }
}
