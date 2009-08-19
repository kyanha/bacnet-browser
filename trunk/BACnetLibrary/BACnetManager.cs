﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace BACnetLibraryNS
{
    public class BACnetmanager
    {

        public BACnetListenerCL BAClistener_insideobject;
        public Thread BAClistener_insidethread;

        public List<Device> Devicelist = new List<Device>();
        public Queue<Device> NewDeviceQueue = new Queue<Device>();

        public BACnetEnums.BACNET_MODE mode;
        public int ourdeviceID;

        public int BACnetManagerPort;

        public IPHostEntry OurIPAddressEntry;
        public IPAddress[] OurIPAddressList;

        public OurSocket insidesocket;

        public void BACnetManagerClose()
        {
            BAClistener_insideobject.BACnetListenerClose();
            
            BAClistener_insidethread.Abort();
            
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

            BAClistener_insideobject = new BACnetListenerCL(this) { listen_socket = insidesocket };
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

#if BACNET_BROWSER
                System.Diagnostics.Debug.WriteLine("Sending Who_is");
                BACnetLibraryNS.BACnetLibraryCL.SendWhoIs( this, true, destination );
#endif


        }

    }

#if ( ! BACNET_SERVER ) && ( ! BACNET_ROUTER ) && ( ! BACNET_BROWSER )
    #error
#endif
}
