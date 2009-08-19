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
        public uint length;

        public uint npdu_offset;
        public uint nsdu_offset = 0;
        public uint apdu_offset;
        // public uint slen, snet, sadr, sadr_len;
        public ADR sadr = new ADR();
        public ADR dadr = new ADR();
        // public uint dadr_len, dnet, hopcount;

        // device id does not belong in a packet public BACnetObjectIdentifier deviceID;

        public uint hopcount;

        public bool apdu_present, snet_present, da_present, sa_present, is_broadcast;

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

        public OurSocket listen_socket;

        public BACnetListenerCL(BACnetmanager bnm)
        {
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
                Packet packet = new Packet();

                // Create an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                try
                {


                    // bacnet_listen_socket.Receive(received);
                    packet.length = (uint)listen_socket.ReceiveFrom(received, ref senderRemote);

                    Console.WriteLine("This message was sent from " + ((IPEndPoint)senderRemote).Address.ToString() + "  Port " + ((IPEndPoint)senderRemote).Port.ToString());

                    packet.FromAddress = (IPEndPoint)senderRemote;

                    packet.buffer = received;



                    BACnetParserClass.parse(packet, received, bnm );
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
