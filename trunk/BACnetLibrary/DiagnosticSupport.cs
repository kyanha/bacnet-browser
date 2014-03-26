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
using System.Text;
using System.Threading;
using System.Drawing;
using BACnetLibrary;
using Reporters;

namespace Diagnostics
{
    public partial class Diagnostic
    {
        public AppManager _apm;
        public BACnetManager _bnm;
        public myTreeNode displayTreeNode;          // this is the Node in the Treeview that relates to the diagnosic, used for colorization, error messages, etc.
        public myTreeNode devicTreeNode;
        public Device device;

        public Diagnostic(AppManager apm, BACnetManager bnm)
        {
            _apm = apm;
            _bnm = bnm;
        }

        public Diagnostic(AppManager apm, BACnetManager bnm, myTreeNode deviceTreeNode)
        {
            this.devicTreeNode = deviceTreeNode;
            this.device = deviceTreeNode.device;
            _apm = apm;
            _bnm = bnm;
        }

        public virtual void RunDiagnosticHelp()
        {
            System.Diagnostics.Process.Start("http://www.bac-test.com");
        }


        public static void AddUniqueDiagnostics(myTreeNode mtnDiagnosticList, List<Diagnostic> diagList)
        {
            // remove existing diags from the above list if they already exist in the treeview
            foreach (myTreeNode mtn in mtnDiagnosticList.Nodes)
            {
                // remove matching diagnostic in diagList
                diagList.RemoveAll(d => d.GetType() == mtn.diagnostic.GetType());
            }

            // add remaining diags to the diag list in the treenode
            foreach (Diagnostic d in diagList)
            {
                myTreeNode mtnd = mtnDiagnosticList.AddMyTreeNodeObject(myTreeNode.TREENODE_OBJ_TYPE.DiagnosticDetails, d.ToString());
                mtnd.diagnostic = d;
                d.displayTreeNode = mtnd;
            }
        }


        public static void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }


        public BACnetPacket waitForPacket(int timeout, BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE netMessage)
        {
            long startTime = _apm._stopWatch.ElapsedMilliseconds;
            while (_apm._stopWatch.ElapsedMilliseconds < startTime + timeout)
            {
                BACnetPacket inPkt = waitForPacket(timeout);
                if (inPkt.messageType == BACnetPacket.MESSAGE_TYPE.NETWORK_LAYER && inPkt.npdu.function == netMessage) return inPkt;
                // else try again
            }
            throw new TimeoutException();
        }


        public BACnetPacket waitForPacket(int timeout, BACnetEnums.BACNET_UNCONFIRMED_SERVICE bus )
        {
            do
            {
                BACnetPacket pkt = waitForPacket(timeout);
                if (pkt.messageType == BACnetPacket.MESSAGE_TYPE.APPLICATION && 
                    pkt.pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST &&
                    pkt.unconfirmedServiceChoice == bus ) return pkt;
            } while ( true ) ;
        }


        public BACnetPacket waitForPacket(int timeout)
        {
            long startTime = _apm._stopWatch.ElapsedMilliseconds;

            while (_apm._stopWatch.ElapsedMilliseconds < startTime + timeout)
            {
                if (_apm.pktQueueToApplication.Count > 0)
                {
                    BACnetPacket bpk = _apm.pktQueueToApplication.myDequeue();
                    bpk.DecodeBACnet();
                    return bpk;
                }
                Sleep(10);
            }
            // timeout - todo - one day create a timeout type of exception
            throw new TimeoutException();
        }

        public static void DiagnosticManager(AppManager apm, List<Diagnostic> diagList)
        {
            foreach (Diagnostic d in diagList)
            {
                Console.WriteLine("Starting " + d.ToString());
                d.MarkDiagnosticBusy();
                Thread oThread = new Thread(new ThreadStart(d.Execute));
                oThread.Name = d.ToString();
                oThread.Start();
                oThread.Join();
            }
            lock (apm)
            {
                apm.diagnosticsRunning = false;
            }
        }

        public virtual void Execute()
        {
            Console.WriteLine("wrong execute");
        }

        public void MarkDiagnosticFailed()
        {
            // Mark the diagnostic that failed 
            displayTreeNode.BackColor = System.Drawing.Color.HotPink;
            // also mark the device to show it has a failed diagnostic
            devicTreeNode.BackColor = System.Drawing.Color.HotPink;
        }

        void AddNewMyTreeNodeWithTextColor(myTreeNode mtn, myTreeNode relatedDeviceNode, String text, System.Drawing.Color col )
        {
            mtn.BackColor = col;
            mtn.Nodes.Add(text);

            // expand any failed diagnostics
            if (col == System.Drawing.Color.HotPink)
            {
                // todo, clean this up, bad form to depend on color to indicate failure
                mtn.Expand();
            }

            // todo, create heirarchy of colors
            relatedDeviceNode.BackColor = col ;
        }

        public delegate void AddNewMyTreeNodeCallback(myTreeNode mtn, myTreeNode relatedDeviceNode, String text, System.Drawing.Color col );

        public void MarkDiagnosticFailed( String comment )
        {
            // Mark the diagnostic that failed 
            _apm.treeViewUpdater.TreeViewOnUI.Invoke(new AddNewMyTreeNodeCallback(AddNewMyTreeNodeWithTextColor), displayTreeNode, devicTreeNode, comment, System.Drawing.Color.HotPink);
        }

        public void MarkDiagnosticWithNote(String comment)
        {
            _apm.treeViewUpdater.TreeViewOnUI.Invoke(new AddNewMyTreeNodeCallback(AddNewMyTreeNodeWithTextColor), displayTreeNode, devicTreeNode, comment, System.Drawing.Color.LightGoldenrodYellow );
        }

        public void MarkDiagnosticSuccess()
        {
            // Mark the diagnostic that succeeded 
            displayTreeNode.BackColor = System.Drawing.Color.LightGreen;
        }

        public void MarkDiagnosticBusy()
        {
            // Mark the diagnostic that succeeded 
            displayTreeNode.BackColor = System.Drawing.Color.GreenYellow;
        }

        public void ClearIncomingPacketQueue()
        {
            _apm.pktQueueToApplication.Clear();
        }


        public void WaitForQuietOnIncomingPacketQueue()
        {
            int initialCount;

            do
            {
                // ClearIncomingPacketQueue();
                initialCount = _apm.pktQueueToApplication.Count;
                Sleep(500);
            }
            while ( initialCount != _apm.pktQueueToApplication.Count );
        }

    
    }


    //public class HexdumpException : Exception
    //{
    //    int len;
    //    byte[] buf;
    //    string message;

    //    public HexdumpException()
    //        : base()
    //    { }

    //    public HexdumpException(string message, byte[] buf, int len)
    //        : base(message)
    //    {
    //        this.message = message;
    //        this.len = len;
    //        this.buf = buf;
    //    }

    //    public HexdumpException(string message, System.Exception inner)
    //        : base(message, inner)
    //    { }

    //    public void DumpException()
    //    {
    //        string ms = message + Environment.NewLine + "CRP: ";
    //        for (int i = 0; i < 32; i++)
    //        {
    //            ms += String.Format("{0:x2} ", buf[i]);
    //        }
    //        ms += Environment.NewLine + "BAC: ";
    //        for (int i = 0; i < 64; i++)
    //        {
    //            ms += String.Format("{0:x2} ", buf[32 + i]);
    //        }
    //        Reports.SendDebugString(ms);
    //    }
    //}



}
