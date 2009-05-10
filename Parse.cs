using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BACnetInteropApp
{

    class Parse
    {
    }

    public class Alpha
    {
        public int timekeeper;
        public List<Device> Devicelist = new List<Device>();

        public Queue<Device> NewDeviceQueue = new Queue<Device>();

        public void parse(byte[] bytes)
        {
            byte[] temp;
            int apdu_offset = 0;
            int npdu_offset = 0;

            // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            /*
                        temp = new byte[2];
                        temp[0] = bytes[7];
                        temp[1] = bytes[6];
                        int destNet = BitConverter.ToUInt16(temp, 0);
                        Console.WriteLine("Destination network: " + destNet);

                        Console.WriteLine("Dest. MAC layer address: " + bytes[8]);

                        Console.WriteLine("Hop count: " + bytes[9]);

            */

            if (bytes[0] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
            {
                // todo3, panic log
                Console.WriteLine("Not a BACnet/IP message");
                return;
            }

            // we could receive an original broadcast, a unicast, a forwarded here...
            // right now

            switch (bytes[1])
            {
                case BACnetEnums.BACNET_UNICAST_NPDU:
                case BACnetEnums.BACNET_FORWARDED_NPDU:
                    npdu_offset = 10;
                    break;
                case BACnetEnums.BACNET_ORIGINAL_BROADCAST_NPDU:
                    // all these OK
                    npdu_offset = 4;
                    break;
                default:
                    // OK, do a panic here
                    //todo1
                    break;
            }

            int len = bytes[2] + bytes[3];

            if (bytes[npdu_offset] != BACnetEnums.BACNET_PROTOCOL_VERSION)
            {
                // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                // todo3, panic
                Console.WriteLine("Protocol version problem");
                return;
            }


            if (bytes[npdu_offset + 1] != 0x20)
            {
                // todo2, investigate
                Console.WriteLine("Control flags not OK");
            }

            apdu_offset = npdu_offset + 6;

            // the offset here is the APDU. Start parsing APDU.

            if (bytes[apdu_offset] == (byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST)
            {
                Console.WriteLine("This is an unconfirmed request");
            }

            if (bytes[apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS)
            {
                Console.WriteLine("Who-is");
            }
            else if (bytes[apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM)
            {
                Console.WriteLine("We have received an 'i-Am'");

                Device D = new Device();

                //bytes[12];
                //bytes[13];
                //bytes[14];
                //bytes[15];
                temp = new byte[4];
                temp[0] = bytes[apdu_offset + 6];
                temp[1] = bytes[apdu_offset + 5];
                temp[2] = bytes[apdu_offset + 4];
                temp[3] = 0x00;
                int deviceId = BitConverter.ToInt32(temp, 0);
                Console.WriteLine("This is device: " + deviceId);

                D.DeviceId = deviceId;


                //bytes[17];
                temp = new byte[2];
                temp[0] = bytes[apdu_offset + 9];
                temp[1] = bytes[apdu_offset + 8];
                //int maxAPDULen = bytes[18];
                int maxAPDULen = BitConverter.ToInt16(temp, 0);
                Console.WriteLine("Max APDU length accepted: " + maxAPDULen);

                //bytes[19];
                //bytes[20];

                // todo, we are not decoding an encoded value properly here
                int vendorId = bytes[apdu_offset + 14] + bytes[apdu_offset + 13] * 256;
                Console.WriteLine("VendorId: " + vendorId);

                D.VendorId = vendorId;
                D.NetworkNumber = 0;

                // Devicelist.Add(D);
                NewDeviceQueue.Enqueue(D);
                
            }

        }




        UdpClient udpRecClient;

        public void BetaClose()
        {
            udpRecClient.Close();
        }


        // This method that will be called when the thread is started
        public void Beta()
        {
            Console.WriteLine("Thread starting");

            //            IPAddress mcAddress = IPAddress.Parse("224.0.0.1");
            //            udpRecClient.JoinMulticastGroup(mcAddress);

            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);


            // Open the BACnet socket. There is a chance that another BACnet program is running on this machine. Anticipate this. // todo-allow this
            try
            {
                udpRecClient = new UdpClient(0xBAC0, AddressFamily.InterNetwork);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            while (true)
            {
                Byte[] received;

                Console.Write(".");

                try
                {
                    received = udpRecClient.Receive(ref remoteIpEndPoint);

                    timekeeper++;

                    //Console.WriteLine("dataReceived: " + received.Length + " bytes...");
                    //Console.WriteLine("From: " + remoteIpEndPoint + "  Count " + timekeeper);

                    parse(received);
                }
                catch (Exception efe)
                {
                    // need to catch the inevitable exception when this blocking call is cancelled by the shutdown code
                    Console.WriteLine(efe);
                }
            }
        }
    }
}
