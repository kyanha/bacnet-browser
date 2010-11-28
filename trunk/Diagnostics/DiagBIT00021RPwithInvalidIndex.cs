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
    public class DiagnosticBIT00021RPwithInvalidIndex : Diagnostic
    {
        // Notes
        //
        // This tests the following ASHRAE conformance test from "ASHRAE-D-86446-20071031 - Conformance" :
        //
        // 135.1-2007 - 13.4.4 - pg 455 - Missing Required Parameter
        //
        //  Further information could be found here: http://www.bac-test.com/wiki/index.php?title=BIT00021
        //
        // If this page does not exist, please create, using: http://www.bac-test.com/wiki/index.php?title=BIT00000_-_Template
        //


        public DiagnosticBIT00021RPwithInvalidIndex(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

        string diagnosticName = "BIT00021 - Read Property with Invalid Index";

        public override string ToString()
        {
            return diagnosticName;
        }

        public override void RunDiagnosticHelp()
        {
            System.Diagnostics.Process.Start("http://www.bac-test.com/wiki/index.php?title=BIT00021");
        }


        public override void Execute()
        {
            try
            {
                ClearIncomingPacketQueue();

                BACnetPacket pkt = new BACnetPacket(_apm, BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY);

                pkt.dAdr = new DADR( this.device.adr);

                pkt.EncodeNPCI(pkt.dAdr, BACnetPacket.MESSAGE_TYPE.APPLICATION);
                pkt.EncodeAPDUheader(BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY);
                
                // Encode the rest of the packet according to http://www.bacnetwiki.com/wiki/index.php?title=Read_Property

                pkt.EncodeContextTag(0, this.device.deviceObjectID);
                pkt.EncodeContextTag(1, BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE);
                // Now, by definition, there can only be one device instance. Try and read the 3rd. If the device responds with anything but an error, it fails
                pkt.EncodeContextTag(2, 3); 

                BACnetUtil.SendOffPacket( this._apm, this._bnm, pkt, pkt.buffer, pkt.optr);

                BACnetPacket respPkt = waitForPacket(5000);

                // Since we made a deliberate mistake with our request, we expect a reject PDU. Make sure we get it

                if ( respPkt.pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ERROR )
                {
                    if ( respPkt.errorClass == (int) BACnetEnums.BACNET_ERROR_CLASS.ERROR_CLASS_PROPERTY && 
                         respPkt.errorCode  == (int) BACnetEnums.BACNET_ERROR_CODE.ERROR_CODE_PROPERTY_IS_NOT_AN_ARRAY )
                    {
                        MarkDiagnosticSuccess();
                        return;
                    }
                    else
                    {
                        MarkDiagnosticFailed("Error PDU OK, but error class or error code not OK");
                        return;
                    }
                }
                else
                {
                    MarkDiagnosticFailed("Error PDU expected");
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
        }
    }


}
