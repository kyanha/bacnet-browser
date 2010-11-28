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
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace BACnetLibrary
{
    public class BACnetManager
    {
        AppManager _apm;

        public BACnetListener BAClistener_insideobject;
        public Thread BAClistener_insidethread;

        public List<Device> Devicelist = new List<Device>();
        public myQueue<BACnetPacket> newPacketQueue = new myQueue<BACnetPacket>();

        public myQueue<String> DiagnosticLogMessage = new myQueue<string>();
        public BACnetEnums.BACNET_MODE mode;
        public int ourdeviceID;

        // public int BACnetManagerPort;

        public IPHostEntry OurIPAddressEntry;
        public IPAddress[] OurIPAddressList;

        public OurSocket insideSocket;

        public Stopwatch _stopWatch = Stopwatch.StartNew();


        public void BACnetManagerClose()
        {
            // here we have a conundrum; the thread we are about to abort is most likely blocked waiting for an ethernet packet to arrive.
            // if we destroy the socket that the thread is using, the thread will do something nasty.
            // if we abort the thread, it will possibly never end since it is waiting for that packet to arrive..
            // ... so we destroy the socket, and catch the thread exception.

            BAClistener_insideobject.BACnetListenerClose();

            BAClistener_insidethread.Abort();
            // Wait until the thread terminates
            BAClistener_insidethread.Join();

        }

        public BACnetManager( AppManager apm, OurSocket insidesocket, int deviceID, IPEndPoint destination )
        {

            _apm = apm;

            this.ourdeviceID = deviceID;
            this.insideSocket = insidesocket;

            // Establish our own IP address, port

            String strHostName = Dns.GetHostName();
            this.OurIPAddressEntry = Dns.GetHostEntry(strHostName);
            this.OurIPAddressList = this.OurIPAddressEntry.AddressList;
          


            // fire up a thread to watch for incoming packets

            BAClistener_insideobject = new BACnetListener( _apm, this) { listen_socket = insidesocket };
            BAClistener_insidethread = new Thread(new ThreadStart(BAClistener_insideobject.BACnetInsideListener));
            BAClistener_insidethread.Start();
        }

        public void MessageLog(string msg)
        {
            DiagnosticLogMessage.myEnqueue(  msg+Environment.NewLine);
        }


        public void MessageProtocolError(string msg)
        {
            // These messages indicate an error in the protocol..
            DiagnosticLogMessage.myEnqueue("Proto: " + msg + Environment.NewLine);
        }


        public void MessageTodo(string msg)
        {
            DiagnosticLogMessage.myEnqueue("Todo: " + msg + Environment.NewLine);
        }


        //public void NewPanic(string panicmessage)
        //{
        //    BACnetLibrary.Panic(panicmessage);
        //    // todo - figure out how to make this form behave...
            //_panicForm.textBoxError.Text = panicmessage;
            //_panicForm.Show();
            //_panicForm.PerformLayout();
            //_panicForm.Update();
        //}

    }
}
