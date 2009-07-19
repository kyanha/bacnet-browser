using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BACnetLibraryNS
{
    public class BACnetmanager
    {

        public BACnetListenerCL BAClistener_object;
        public Thread BAClistener_thread;

        public List<Device> Devicelist = new List<Device>();
        public Queue<Device> NewDeviceQueue = new Queue<Device>();

        public BACnetEnums.BACNET_MODE mode;
        public int ourdeviceID;

        public int BACnetManagerPort;

        public BACnetmanager(int BACnetManagerPort, BACnetEnums.BACNET_MODE mode, int deviceID)
        {

            this.BACnetManagerPort = BACnetManagerPort;
            this.mode = mode;
            this.ourdeviceID = deviceID;

            // fire up a thread to watch for incoming packets

            BAClistener_object = new BACnetListenerCL(this) { BACnet_port = BACnetManagerPort };
            //BAC1listener_object = new Alpha() { BACnet_port = 0xbac1 };

            BAClistener_thread = new Thread(new ThreadStart(BAClistener_object.BACnetListenerMethod));
            BAClistener_thread.Start();


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
                BACnetLibraryNS.BACnetLibraryCL.SendWhoIs( this );
#endif

        }

    }

#if ( ! BACNET_SERVER ) && ( ! BACNET_ROUTER ) && ( ! BACNET_BROWSER )
    #error
#endif
}
