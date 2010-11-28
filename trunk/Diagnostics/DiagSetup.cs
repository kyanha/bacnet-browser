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
using System.Windows.Forms;


namespace Diagnostics
{
    public class DiagSetup
    {
        public static List<Diagnostic> BuildDiagnosticListStandard(AppManager apm, BACnetManager bnm, myTreeNode deviceNode)
        {
            List<Diagnostic> diagList = new List<Diagnostic>();
            
            // Adding diagnostics 
            diagList.Add(new DiagnosticBIT00000Example(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00004ThreeRapidWhoIs(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00006RemoteBroadcast(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00008ReadPropertyProtocolServices(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00010ForceRouteDiscovery(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00011ReadWildcardDevice(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00012deviceIDrange(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00013whoIsRange1(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00014whoIsRangeTargeted(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00015whoIsBadMessage1(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00016whoIsBadMessage3(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00017Tag(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00019Missing(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00020TooManyProps(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00021RPwithInvalidIndex(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00022requiredDeviceProperties(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00023objectTypesCheck(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00024WhoIsNoSADR(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00026whoIsBadMessage2(apm, bnm, deviceNode));
            diagList.Add(new DiagnosticBIT00025LocalBroadcast(apm, bnm, deviceNode));

            // BACnet Router only tests
            if (deviceNode.type == myTreeNode.TREENODE_OBJ_TYPE.Router)
            {
                diagList.Add(new DiagnosticBIT00001DuplicateRoutingEntry(apm, bnm, deviceNode));
                diagList.Add(new DiagnosticBIT00018CheckPortID(apm, bnm, deviceNode));
            }

            return diagList;
        }
    }

}
