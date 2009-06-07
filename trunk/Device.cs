using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BACnetLibraryNS;


namespace BACnetInteropApp
{
    public class Device : IComparable<Device>, IEquatable<Device>
    {
        #region Fields & Constants

        public int NetworkNumber;           // The network number of the BACnet network that the Device is attached to
        public int SourceAddress;
        public int I_Am_Count = 0;

        private uint vendorId;
        private uint deviceId;

        public BACnetEnums.BACNET_SEGMENTATION SegmentationSupported;

        private int maxAPDULength;

        public Packet packet;   // a place to store the source IP address and port number

        #endregion

        #region Properties

        public uint VendorId
        {
            get { return vendorId; }
            set { vendorId = value; }
        }

        public uint DeviceId
        {
            get { return this.deviceId; }
            set { this.deviceId = value; }
        }

        #endregion

        public int CompareTo(Device d)
        {

            // sort order is relevant...
            if (this.NetworkNumber > d.NetworkNumber) return 1;
            if (this.deviceId > d.deviceId) return 1;

            if (this.NetworkNumber < d.NetworkNumber) return -1;
            if (this.deviceId < d.deviceId) return -1;

            // devices must be equal
            return 0;
        }

        public bool Equals(Device d)
        {
            if (this.NetworkNumber == d.NetworkNumber && this.deviceId == d.deviceId) return true;
            return false;
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

}
