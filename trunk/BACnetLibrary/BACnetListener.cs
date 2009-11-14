using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;



namespace BACnetLibraryNS
{


    public class OutboundCopy
    {
        public uint length;
        public byte[] buffer;
    }


    public class BACnetListenerCL
    {
        AppManager _apm;
        BACnetmanager bnm;

        public OurSocket listen_socket;

        public BACnetListenerCL(AppManager apm, BACnetmanager bnm)
        {
            _apm = apm;
            this.bnm = bnm;
        }

        public void BACnetListenerClose()
        {
            try
            {
                listen_socket.Shutdown(SocketShutdown.Both);
                listen_socket.Close();
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe);
            }
        }



        // This method that will be called when the thread is started
        public void BACnetInsideListener()
        {
            //Console.WriteLine("Thread starting for port " + Convert.ToString(BACnet_port));

            while (true)
            {
                Byte[] received = new Byte[2000];
                BACnetPacket packet = new BACnetPacket(_apm);

                // Create an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                try
                {


                    // bacnet_listen_socket.Receive(received);
                    packet.length = listen_socket.ReceiveFrom(received, ref senderRemote);

                    Console.WriteLine("This message was sent from " + ((IPEndPoint)senderRemote).Address.ToString() + "  Port " + ((IPEndPoint)senderRemote).Port.ToString());

                    // packet.fromBIP = (IPEndPoint) senderRemote;

                    packet.fromBIP = new myIPEndPoint();
                    packet.fromBIP.Port = ((IPEndPoint)senderRemote).Port;
                    packet.fromBIP.Address = ((IPEndPoint)senderRemote).Address;

                    packet.buffer = received;

                    packet.DecodeBACnet(received, 0);

                    // now do something with the decoded packet

                    bnm.newPacketQueue.Enqueue(packet);

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
