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
    public class DiagnosticBIT00000Example : Diagnostic
    {
        // Notes
        // When done writing a new diagnostic, don't forget to include it in AddDiagnosticListAll()

        // This tests the following ASHRAE conformance test from "ASHRAE-D-86446-20071031 - Conformance" :
        //
        // 135.1-2007 - Section XX.X.X - pg xxx - Aaaaa
        //
        //  Further information could be found here: http://www.bac-test.com/wiki/index.php?title=BIT00000
        //
        // If this page does not exist, please create, using: http://www.bac-test.com/wiki/index.php?title=BIT00000_-_Template
        //

        public DiagnosticBIT00000Example(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00000 - Template for Shared (Public) Diagnostic";

        public override void RunDiagnosticHelp()
        {
            System.Diagnostics.Process.Start("http://www.bac-test.com/wiki/index.php?title=BIT00000_-_Template");
        }


        public override string ToString()
        {
            return diagnosticName;
        }

        public override void Execute()
        {
            try
            {
                ClearIncomingPacketQueue();

                // Create, encode and send a packet
                BACnetPacket pkt = new BACnetPacket(_apm, _bnm, this.device.adr, this.device.deviceObjectID, BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_REVISION);
                pkt.SendPacket();

                // Wait for response
                BACnetPacket responsePkt = waitForPacket(5000);

                // Since we made a deliberate mistake with our request, we expect a reject PDU. Make sure we get it

                if (responsePkt.pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK)
                {
                    if (responsePkt.confirmedServiceChoice == BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY)
                    {
                        _apm.MessageLog("Protocol revision = " + responsePkt.protocolRevision.ToString());
                    }
                    else
                    {
                        MarkDiagnosticFailed("errDiag00001 - Wrong confirmedServiceChoice in response");
                        return;
                    }
                }
                else
                {
                    MarkDiagnosticFailed("errDiag00002 - Complex ACK not received");
                    return;
                }
            }
            catch (TimeoutException)
            {
                MarkDiagnosticFailed("Timeout");
                return;
            }
            catch (Exception ex)
            {
                _apm.MessagePanic("Diagnostic Exception " + ex.ToString());
                return;
            }
            MarkDiagnosticSuccess();
        }
    }
}
