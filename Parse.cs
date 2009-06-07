using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using BACnetLibraryNS;


namespace BACnetInteropApp
{

    //class Parse
    //{
    //}

    public class Alpha
    {
        public int timekeeper;
        public int BACnet_port;
        public Base control;

        public void parse(Packet packet, byte[] bytes)
        {
            int apdu_offset = 0;
            int npdu_offset = 0;

            int snet=0, sadr=0, sadr_len = 0 ;

            
            if (BACnet_port == 0xBAC1)
            {
                // an opportunity to debug just port one
                Console.WriteLine ( "Port 1" ) ;
            }

            // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            if (bytes[0] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
            {
                // todo3, panic log
                Console.WriteLine("Not a BACnet/IP message");
                return;
            }



            // we could receive an original broadcast, a unicast, a forwarded here...
            // right now
            // BVLC Function Types. See http://www.bacnetwiki.com/wiki/index.php?title=BVLC_Function

            switch ( (BACnetEnums.BACNET_BVLC_FUNCTION) bytes[1] )
            {
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU:
                    npdu_offset = 10;
                    break;
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU:
                case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU:
                    // all these OK
                    npdu_offset = 4;
                    break;
                default:
                    // OK, do a panic here
                    //todo1
                    break;
            }


            // Investigate the NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            if (bytes[npdu_offset] != BACnetEnums.BACNET_PROTOCOL_VERSION)
            {
                // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                // todo3, panic
                Console.WriteLine("Protocol version problem");
                return;
            }


            if ( (bytes[npdu_offset + 1] & 0x80) == 0x80)
            {
                // NSDU contains Network Layer Message
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                int nsdu_offset;

                if ((bytes[npdu_offset + 1] & 0x20) == 0x20)
                {
                    // means DADR, DNET present
                    nsdu_offset = npdu_offset + 6; // todo - variable length, fix
                }
                else
                {
                    nsdu_offset = npdu_offset + 2; 
                }

                switch ( (BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE) bytes[nsdu_offset] ) 
                { 
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_REJECT_MESSAGE_TO_NETWORK: 
                        System.Windows.Forms.MessageBox.Show ( "Network Message \"Reject Message to Network\" received" ) ;
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE:
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                        Console.WriteLine("Router-Table-Ack");
                        break;
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        // todo - conformance, check that this is _always_ a broadcast message.
                        Console.WriteLine("I-Am-Router-to-Network");
                        // Parse list of network numbers available via this router.
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("Vendor Proprietory Network Message received");
                        break;
                }
            }
            else
            {
                // NSDU contains APDU
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                // determine if SNET, SLEN, SADR present

                if ((bytes[npdu_offset + 1] & 0x08) == 0x08)
                {
                    // means SADR, SNET present
                    snet = ( bytes[npdu_offset + 5] << 8 ) + bytes[npdu_offset + 6];
                    sadr_len = bytes[npdu_offset + 7];
                    if (sadr_len != 1)
                    {
                        // panic
                        System.Windows.Forms.MessageBox.Show("Unexpected SADR len");
                        return;
                    }
                    sadr = bytes[npdu_offset + 8];

                    apdu_offset = npdu_offset + 10;
                }
                else
                {
                    // absent
                    apdu_offset = npdu_offset + 6;
                }


                // the offset here is the APDU. Start parsing APDU.

                if (bytes[apdu_offset] == (byte)BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST)
                {
                    Console.WriteLine("This is an unconfirmed request");
                }
                else
                {
                    // todo
                    return;
                }



                if (bytes[apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS)
                {
                    Console.WriteLine("Who-is");
                }
                else if (bytes[apdu_offset + 1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM)
                {
                    Console.WriteLine("We have received an 'I-Am'");

                    // I-Am described right here: http://www.bacnetwiki.com/wiki/index.php?title=I-Am

                    Device D = new Device();

                    // first encoded entity is the Device Identifier...
                    // Encoding described here: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

                    // Decode Device Identifier

                    uint deviceId;

                    int offset = BACnetEncoding.BACnetDecode_uint(bytes, apdu_offset + 2, out deviceId);

                    offset += apdu_offset + 2;


                    // todo - brute force mask off the object type field 
                    deviceId &= 0x03fffff;
                    Console.WriteLine("This is device: " + deviceId);

                    D.DeviceId = deviceId;
                    D.packet = packet;
                    D.SourceAddress = sadr;
                    D.NetworkNumber = snet;


                    uint maxAPDULen;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out maxAPDULen);

                    Console.WriteLine("Max APDU length accepted: " + maxAPDULen);



                    uint segmentation_supported;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out segmentation_supported);

                    Console.WriteLine("Segmentation Supported: " + segmentation_supported);

                    D.SegmentationSupported = (BACnetEnums.BACNET_SEGMENTATION)segmentation_supported ;



                    uint vendorId;

                    offset += BACnetEncoding.BACnetDecode_uint(bytes, offset, out vendorId);

                    Console.WriteLine("VendorId: " + vendorId);

                    D.VendorId = vendorId;
                    

                    // Devicelist.Add(D);
                    MainForm.Controlbase.NewDeviceQueue.Enqueue(D);


                }
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
            Console.WriteLine( "Thread starting for port " + Convert.ToString(BACnet_port)  );

            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Open the BACnet socket. There is a chance that another BACnet program is running on this machine. Anticipate this. // todo-allow this
            try
            {
                udpRecClient = new UdpClient( this.BACnet_port, AddressFamily.InterNetwork);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            while (true)
            {
                Byte[] received;
                Packet packet = new Packet() ;

                try
                {

                    received = udpRecClient.Receive(ref remoteIpEndPoint);

                    Console.WriteLine("This message was sent from " +
                                                remoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                remoteIpEndPoint.Port.ToString());

                    packet.Source_Address = remoteIpEndPoint.Address;
                    packet.Source_Port = remoteIpEndPoint.Port;
                    packet.buffer = received;
                    

                    timekeeper++;

                    //Console.WriteLine("dataReceived: " + received.Length + " bytes...");
                    //Console.WriteLine("From: " + remoteIpEndPoint + "  Count " + timekeeper);

                    parse(packet, received);

                    // clean up remoteIpEndPoint for the next go around
                    remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
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
