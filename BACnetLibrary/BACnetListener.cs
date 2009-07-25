using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;



namespace BACnetLibraryNS
{
    public class Packet
    {
        public System.Net.IPAddress Source_Address;
        public int Source_Port;
        public byte[] buffer;

        public uint npdu_offset ;
        public uint nsdu_offset=0;
        public uint apdu_offset;
        public uint slen, snet, sadr, sadr_len ;
        public uint dadr_len, dnet, hopcount;

        public bool apdu_present, snet_present, da_present, sa_present, is_broadcast ;

        public IPEndPoint FromAddress;

        public Packet()
        {
            this.apdu_present = false;
            this.sa_present = false;
            this.da_present = false;
            this.is_broadcast = false;
            
        }

    }


    public class BACnetListenerCL
    {
        BACnetmanager bnm;

        public int BACnet_port;

        Socket bacnet_listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            
        public BACnetListenerCL(BACnetmanager bnm)
        {
            this.bnm = bnm;
        }


        public void BACnetListenerClose()
        {
            bacnet_listen_socket.Close();
        }



        // This method that will be called when the thread is started
        public void BACnetListenerMethod()
        {
            Console.WriteLine("Thread starting for port " + Convert.ToString(BACnet_port));

            IPEndPoint local_ipep = new IPEndPoint(0, BACnet_port );

            bacnet_listen_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // bind the local end of the connection to BACnet port number
            bacnet_listen_socket.Bind(local_ipep);

            while (true)
            {
                Byte[] received = new Byte[2000] ;
                Packet packet = new Packet();

                // Create an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;


                try
                {

                    
                    // bacnet_listen_socket.Receive(received);
                    bacnet_listen_socket.ReceiveFrom(received, ref senderRemote);

                    Console.WriteLine("This message was sent from " + ((IPEndPoint) senderRemote).Address.ToString() + "  Port " + ((IPEndPoint) senderRemote).Port.ToString() ) ;

                    packet.FromAddress = (IPEndPoint)senderRemote;

                    //Console.WriteLine("This message was sent from " +
                    //                            remoteIpEndPoint.Address.ToString() +
                    //                            " on their port number " +
                    //                            remoteIpEndPoint.Port.ToString());

                    //packet.Source_Address = remoteIpEndPoint.Address;
                    //packet.Source_Port = remoteIpEndPoint.Port;
                    packet.buffer = received;

                    // attempting to find a way to get ipaddress of sender
                    // todo - we need to ignore messages from ourselves... bacnet_listen_socket.RemoteEndPoint();
                    // todo, for now, we will ignore device instance xxx if received and we are the client (bacnet browser - this has to be done in the parse function for now..)


                    //Console.WriteLine("dataReceived: " + received.Length + " bytes...");
                    Console.WriteLine("Packet received on port : " + BACnet_port);

                    BACnetParserClass.parse(packet, received, bnm );


                    // clean up remoteIpEndPoint for the next go around
                    //remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
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
