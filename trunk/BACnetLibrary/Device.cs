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
using System.Net.Sockets;

using BACnetLibrary;


namespace BACnetLibrary
{
    public class Device : IEquatable<Device>
    {
        #region Fields & Constants

        public BACnetEnums.DEVICE_TYPE type;

        public ADR adr;

        public myIPEndPoint directlyConnectedIPEndPointOfDevice ;           // this is the Router or the MAC address that is the IP address access to the device
        // not used in browser public myIPEndPoint siteIPEndPoint;

        // public uint SourceAddress;
        public uint I_Am_Count = 0;

        public VendorID vendorID;
        public BACnetObjectIdentifier deviceObjectID = new BACnetObjectIdentifier();              // note! This is NOT the same as the MAC address stored in dadr.


        public BACnetEnums.BACNET_SEGMENTATION SegmentationSupported;

        // private int maxAPDULength;

        public BACnetPacket packet;   // a place to store the source IP address and port number

        #endregion

        #region Properties

        // Todo, make this return manufacturer names.
        #endregion

        public Device(Device d)
        {
            this.adr = d.adr;
        }

        public Device()
        {
        }

        public Device(UInt16 networkNumber, myIPEndPoint mac)
        {
            adr = new ADR(networkNumber, mac);
        }


        public override string ToString()
        {
            return deviceObjectID.ToString();
        }

        public bool Equals(Device d)
        {
            // Devices are considered equal if their network numbers and MAC addresses match.

            if (this.adr == null && d.adr != null) return false;
            if (this.adr != null && d.adr == null) return false;
            if (this.adr != null && d.adr != null)
            {
                if (!this.adr.Equals(d.adr)) return false;
            }
            return true;
            //todo, add check that device instances (Device Object IDs) are equal here too... ( as a sanity check)
        }

        public bool Equals(BACnetObjectIdentifier deviceID)
        {
            return (this.deviceObjectID.Equals(deviceID));
        }
    }


    public class ServerDevice : Device
    {

        public ServerDevice( /*DeviceId,*/ uint NetworkNumber)
        {
            // constructor 

            //this.DeviceId = 999 ; // todo
            this.adr.networkNumber = adr.networkNumber;
        }
    }

}
