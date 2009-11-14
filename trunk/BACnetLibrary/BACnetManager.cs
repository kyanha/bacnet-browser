using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Windows.Forms;
//using System.Diagnostics;

namespace BACnetLibraryNS
{
    public class BACnetmanager
    {
        AppManager _apm;

        public BACnetListenerCL BAClistener_insideobject;
        public Thread BAClistener_insidethread;

        public List<Device> Devicelist = new List<Device>();
        public Queue<BACnetPacket> newPacketQueue = new Queue<BACnetPacket>();

        public Queue<String> DiagnosticLogMessage = new Queue<string>();
        public BACnetEnums.BACNET_MODE mode;
        public int ourdeviceID;

        public int BACnetManagerPort;

        public IPHostEntry OurIPAddressEntry;
        public IPAddress[] OurIPAddressList;

        public OurSocket insidesocket;


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

        public BACnetmanager( OurSocket insidesocket, int deviceID, IPEndPoint destination )
        {

            this.ourdeviceID = deviceID;
            this.insidesocket = insidesocket;

            // Establish our own IP address, port

            String strHostName = Dns.GetHostName();
            this.OurIPAddressEntry = Dns.GetHostEntry(strHostName);
            this.OurIPAddressList = this.OurIPAddressEntry.AddressList;
          


            // fire up a thread to watch for incoming packets

            BAClistener_insideobject = new BACnetListenerCL( _apm, this) { listen_socket = insidesocket };
            BAClistener_insidethread = new Thread(new ThreadStart(BAClistener_insideobject.BACnetInsideListener));
            BAClistener_insidethread.Start();


#if BACNET_SERVER
           

                // send one i-am the old way for now

                System.Diagnostics.Debug.WriteLine("Sending I-Am");
                BACnetLibraryNS.BACnetLibraryCL.SendIAm(this);
                
                
                // create many devices...

                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 1, 0));          // todo, until we know better, assume net 0 is the local IP network
                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 2, 0));

                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 12, 100));
                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 13, 100));

                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 12, 200));
                Devicelist.Add(new ServerDevice(BACnetEnums.SERVER_DEVICE_ID + 13, 200));

                foreach (ServerDevice D in Devicelist)
                {
                    D.Send_IAm( this );

                    // todo, why does this not output anything??
                    System.Diagnostics.Debug.WriteLine("Sending I-Am" );
                }


#endif



        }

        public void MessageLog(string msg)
        {
            DiagnosticLogMessage.Enqueue(  msg+Environment.NewLine);
        }


        public void MessageProtocolError(string msg)
        {
            // These messages indicate an error in the protocol..
            DiagnosticLogMessage.Enqueue("Proto: " + msg + Environment.NewLine);
        }


        public void MessageTodo(string msg)
        {
            DiagnosticLogMessage.Enqueue("Todo: " + msg + Environment.NewLine);
        }


        public void NewPanic(string panicmessage)
        {
            BACnetLibraryCL.Panic(panicmessage);

            // todo - figure out how to make this form behave...

            //_panicForm.textBoxError.Text = panicmessage;
            //_panicForm.Show();
            //_panicForm.PerformLayout();
            //_panicForm.Update();
        }

    }

#if ( ! BACNET_SERVER ) && ( ! BACNET_ROUTER ) && ( ! BACNET_BROWSER )
    #error
#endif
}
