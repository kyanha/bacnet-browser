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

namespace BACnetLibrary
{
    public class Router : Device
    {
        public List<RoutingTableEntry> routingTableEntries = new List<RoutingTableEntry>();
        public bool linkDiscovered;

        public Router(Device d) : base ( d ) 
        {
            type = BACnetEnums.DEVICE_TYPE.Router;
            adr = d.adr;
            directlyConnectedIPEndPointOfDevice = d.directlyConnectedIPEndPointOfDevice;

            // todo, there are many other parameters to copy across - how about device ID, name, manuf, etc etc.
        }


        //public string ToString()
        //{
        //    return adr.ToString();
        //}


        public void AddRoutingTableEntries(List<UInt16> numberList)
        {
            // Only I-Am-Routers add RoutingTableEntries this way, so we know that all entries will be farSide
            foreach (UInt16 nle in numberList)
            {
                AddRoutingTableEntry(nle);
            }
            DiscoverLink();
        }


        public void AddRoutingTableEntries(List<RoutingTableEntry> rteList)
        {
            foreach (RoutingTableEntry rte in rteList)
            {
                AddRoutingTableEntry(rte.networkNumber, rte.portID);
            }
            DiscoverLink();
        }


        void DiscoverLink()
        {
            // now go through the whole list again, and see if there is only one nearSide entry. If so, then
            // this is the Network Number for the connection between us and the BACnet Router.
            if (linkDiscovered == false)
            {
                List<RoutingTableEntry> rtl = routingTableEntries.FindAll(delegate(RoutingTableEntry drt) { return (drt.farSide != true); });
                if (rtl.Count == 1)
                {
                    // we have it
                    rtl[0].linkNetworkNumber = true;
                    linkDiscovered = true;
                }
                else
                {
                    // drop a message, link not discoverable...
                }
            }
        }


        public void AddRoutingTableEntry(UInt16 networkNumber, Byte portID)
        {
            // do we already have such an entry, create it if not
            RoutingTableEntry frp = routingTableEntries.Find(delegate(RoutingTableEntry drp) { return drp.networkNumber == networkNumber; });

            if (frp == null)
            {
                // create a new entry
                routingTableEntries.Add(new RoutingTableEntry(networkNumber, portID));
                frp = routingTableEntries[routingTableEntries.Count - 1];
            }

            // let's make sure that port IDs have been made
            if (frp.portID != 0 && frp.portID != portID)
            {
                // sound a warning message. Also, use something other than 0 to indicate uninitialized
            }
            frp.portID = portID;
        }

        public RoutingTableEntry FindRoutingTableEntry(int networkNumber)
        {
            // do we already have such an entry, create it if not
            return routingTableEntries.Find(delegate(RoutingTableEntry drp) { return drp.networkNumber == networkNumber; });
        }


        public void AddRoutingTableEntry(UInt16 networkNumber)
        {
            if (networkNumber < 1 || networkNumber > 0xfffe)
            {
                // these are illegal network numbers
                throw new ProtocolException ("m0204 - Illegal Network Number " + networkNumber.ToString() );
            }

            // do we already have such an entry, create it if not
            // RoutingTableEntry frp = routingTableEntries.Find(delegate(RoutingTableEntry drp) { return drp.networkNumber == networkNumber; });
            RoutingTableEntry frp = FindRoutingTableEntry ( networkNumber );

            if (frp == null)
            {
                // create a new entry
                routingTableEntries.Add(new RoutingTableEntry(networkNumber, (byte)0));  // todo, how to create uninitialized PortID?
                frp = routingTableEntries[routingTableEntries.Count - 1];
            }
            // Only I-Am-Router-to-network adds RoutingTableEntries this way, so we know that the entry is a farSide
            frp.farSide = true;
        }


        public RoutingTableEntry EstablishRouterPort(UInt16 networkNumber)
        {
            // destinationDev = _bnm.deviceList.Find(delegate(Device d) { return d.adr.Equals(incomingCRPpacket.dadr); });
            RoutingTableEntry frp = routingTableEntries.Find(delegate(RoutingTableEntry drp) { return drp.networkNumber == networkNumber; });

            if (frp == null)
            {
                // create a new routerport and add to the list
                frp = new RoutingTableEntry();
                frp.networkNumber = networkNumber;
                // todo we run the risk of partially populating this object..... 
                routingTableEntries.Add(frp);
            }
            return frp;
        }
    }

    //public class RouterTask
    //{
    //    AppManager _apm;
    //    BACnetManager _bnm;

    //    public RouterTask(AppManager apm, BACnetManager bnm)
    //    {
    //        _apm = apm;
    //        _bnm = bnm;
    //    }


    //}

    public class RoutingTableEntry
    {
        public UInt16 networkNumber;
        public Byte portID;
        int portInfoLen;

        public bool farSide;                    // This BACnet Network is definitely on the far side of another router.
        // public bool nearSide;
        public bool linkNetworkNumber;          // this is the Network Number that connects us to the BACnet Router

        public RoutingTableEntry()
        {
        }


        public RoutingTableEntry(UInt16 networkNumber)
        {
            this.networkNumber = networkNumber;
        }


        public RoutingTableEntry(UInt16 networkNumber, Byte portID)
        {
            this.networkNumber = networkNumber;
            this.portID = portID;
        }


        public override String ToString()
        {
            if (linkNetworkNumber == true)
            {
                return String.Format("Network: {0,5}   ID: {1,-3} Link", networkNumber, portID);
            }
            else if (farSide == true)
            {
                return String.Format("Network: {0,5}   ID: {1,-3} Far Side", networkNumber, portID);
            }
            else
            {
                return String.Format("Network: {0,5}   ID: {1,-3} Near Side", networkNumber, portID);
            }
        }


        public bool Decode(byte[] buf, ref int iptr)
        {
            networkNumber = BACnetUtil.ExtractUInt16(buf, ref iptr);
            portID = buf[iptr++];
            portInfoLen = buf[iptr++];
            if (portInfoLen != 0)
            {
                // we are not ready to handle this.
                throw new Exception("m0154-Cannot deal with portInfoLen > 0");
            }
            iptr += (int)portInfoLen;
            return true;
        }


    }


}
