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

// AppManager is our catch all for 'Global Objects' as well as the interface between Application Runtime code and the various User Interface forms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BACnetLibrary
{
    public class AppManager
    {
        public Stopwatch _stopWatch = Stopwatch.StartNew();

        public DeviceTreeView treeViewUpdater;

        // temporary until debug messages migrated
        public BACnetManager bnm;

        public bool diagnosticsRunning ;
        public myQueue<String> DiagnosticLogMessage = new myQueue<string>();
        public myQueue<String> DiagnosticLogProtocol = new myQueue<string>();
        public myQueue<String> DiagnosticLogPanic = new myQueue<string>();

        // public myQueue<CRPpacket> pktQueueToApplication = new myQueue<CRPpacket>();
        public myQueue<BACnetPacket> pktQueueToApplication = new myQueue<BACnetPacket>();

        public UInt16 ourVendorID = 343;
        //todo, need to establish what this is automatically and not hard coded.
        public UInt32 ourDeviceId = 12345;
        // public bool enableApplicationQueue;

        // the 'database' of bacnet devices, etc.
        public List<Device> deviceList = new List<Device>();
        public Router internalRouterInfo = new Router(new Device());

        public Byte invokeID;

        public AppManager()
        {
        }

        public void MessageLog(string msg)
        {
            DiagnosticLogMessage.myEnqueue(msg + Environment.NewLine);
        }


        public void MessageProtocolError(string msg)
        {
            // These messages indicate an error in the protocol..
            DiagnosticLogProtocol.myEnqueue(msg + Environment.NewLine);
            BACnetUtil.SendDebugString(msg);
        }


        List<String> panicList = new List<String>();

        public void MessageTodo(string msg)
        {
            DiagnosticLogMessage.myEnqueue("Todo: " + msg + Environment.NewLine);
        }


        public void MessageConfigChange(string msg)
        {
            DiagnosticLogMessage.myEnqueue("Configuration Change: " + msg + Environment.NewLine);
        }


        public void MessagePanic(string panicmessage)
        {
            // do we already have this message, if so, ignore it
            if (panicList.Contains(panicmessage)) return;

            panicList.Add(panicmessage);
            DiagnosticLogPanic.myEnqueue(panicmessage + Environment.NewLine + Environment.NewLine);
            BACnetUtil.SendDebugString(panicmessage);
        }
    }
}
