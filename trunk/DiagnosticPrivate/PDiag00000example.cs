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
using Diagnostics;

namespace DiagnosticPrivate
{
    public class PDiag00000example : Diagnostic
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

            public PDiag00000example(AppManager apm, BACnetManager bnm, myTreeNode device) : base(apm, bnm, device) { }

            string diagnosticName = "PD00000 - Template for Private Diagnostic";

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
                    _apm.MessageLog("Executing " + diagnosticName);

                    // put diagnostic code here

                    /* Example
                  
                    ClearIncomingPacketQueue();

                    BACnetLibrary.SendWhoIs(_apm, _bnm, deviceNode.device);

                    waitForPacket(5000);
                
                    */

                }
                catch (TimeoutException)
                {
                    // todo-put a wrapper around execute and catch all executions in a common area..
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

