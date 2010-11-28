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
using BACnetLibrary;

namespace Diagnostics
{
    public class DiagnosticBIT00023objectTypesCheck : Diagnostic
    {
        // Notes
        //
        // Each protocol revision seems to add a few more object types. Check the length of this list against the revision

        //  Further information could be found here: 

        string helpString = "http://www.bac-test.com/wiki/index.php?title=BIT00023";

        // If this page does not exist, please create, using: http://www.bac-test.com/wiki/index.php?title=BIT00000_-_Template
        //


        public DiagnosticBIT00023objectTypesCheck(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00023 - Object Types Supported list check";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void RunDiagnosticHelp()
        {
            System.Diagnostics.Process.Start(helpString);
        }


        public override void Execute()
        {
            int protocolRevision;

            try
            {
                ClearIncomingPacketQueue();
                BACnetPacket pkt = new BACnetPacket(_apm, _bnm, this.device.adr, this.device.deviceObjectID, BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_REVISION);
                pkt.SendPacket();
                BACnetPacket respPkt = waitForPacket(5000);

                protocolRevision = respPkt.protocolRevision;

                pkt = new BACnetPacket(_apm, _bnm, this.device.adr, this.device.deviceObjectID, BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED);
                pkt.SendPacket();
                respPkt = waitForPacket(5000);


                // Since we made a deliberate mistake with our request, we expect a reject PDU. Make sure we get it

                if (respPkt.pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK)
                {
                    if (respPkt.confirmedServiceChoice == BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY)
                    {
                        // so far, so good, continue
                        switch (protocolRevision)
                        {
                            case 2:
                                if (respPkt.bitString != null && respPkt.bitString.Count() != 0x19)
                                {
                                    MarkDiagnosticFailed("Object Type List not the correct length for the protocol");
                                    return;
                                }
                                break;
                            case 6:
                                if (respPkt.bitString != null && respPkt.bitString.Count() != 38) 
                                {
                                    MarkDiagnosticFailed("Object Type List not the correct length for the protocol");
                                    return;
                                }
                                break;
                            case 10:
                                if (respPkt.bitString != null && respPkt.bitString.Count() != 0x33)
                                {
                                    MarkDiagnosticFailed("Object Type List not the correct length for the protocol");
                                    return;
                                }
                                break;
                            default:
                                MarkDiagnosticFailed("Unknown protocol revision");
                                return;
                        }
                    }
                    else
                    {
                        MarkDiagnosticFailed("Did not receive a response to the confirmed read property");
                        return;
                    }
                }
                else
                {
                    MarkDiagnosticFailed("Expected a Complex ACK response");
                    return;
                }

            }
            catch (TimeoutException)
            {
                // todo-put a wrapper around execute and catch all executions in a common area..
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                // other types of exception...
                throw ex;
            }
            MarkDiagnosticSuccess();
        }
    }


}
