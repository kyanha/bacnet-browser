using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace BACnetLibraryNS
{
    public class PacketLog
    {
        public bool sending;
        public IPEndPoint ipep;
        public BACnetPacket BACnetPacket;
        public CRPpacket CRPpacket;

        static int _count;


        public PacketLog(bool sending, IPEndPoint ipep)
        {
            this.sending = sending;
            this.ipep = ipep;  // ipep sending to, or receiving from
        }

        public PacketLog(bool sending, IPEndPoint ipep, BACnetPacket packet)
        {
            this.sending = sending;
            this.ipep = ipep;
            this.BACnetPacket = packet;
        }

        public PacketLog(bool sending, IPEndPoint ipep, CRPpacket packet)
        {
            this.sending = sending;
            this.ipep = ipep;
            this.CRPpacket = packet;
        }

        public PacketLog(bool sending, CRPpacket packet)
        {
            this.sending = sending;
            this.CRPpacket = packet;
        }

        public PacketLog(bool sending, BACnetPacket packet)
        {
            this.sending = sending;
            this.BACnetPacket = packet;
        }

        public static void TreeViewUpdate(BACnetManager bnm, TreeView treeViewMessageLog)
        {
            if (bnm.BACnetMessageLog.Count > 0)
            {

                while (bnm.BACnetMessageLog.Count > 0)
                {
                    // remove the first treenode

                    _count++;

                    PacketLog pktlog = bnm.BACnetMessageLog.Dequeue();

                    if (bnm.logPings == false &&
                        pktlog.CRPpacket != null &&
                        (pktlog.CRPpacket.function == CRPpacket.CR_PACKET_FUNC.CRP_PING || pktlog.CRPpacket.function == CRPpacket.CR_PACKET_FUNC.CRP_PONG))
                    {
                        // dont want to see noisy pings and pongs
                        continue;
                    }

                    TreeNode ntn = new TreeNode();

                    if (pktlog.sending == true)
                    {
                        ntn.Text = _count.ToString() + " Sent";
                        ntn.BackColor = Color.LightPink;
                    }
                    else
                    {
                        ntn.Text = _count.ToString() + " Recd";
                        ntn.BackColor = Color.LightGreen;
                    }

                    if (pktlog.CRPpacket != null)
                    {
                        switch (pktlog.CRPpacket.function)
                        {
                            case CRPpacket.CR_PACKET_FUNC.CRP_PING:
                                ntn.Text += " Ping";
                                ntn.Nodes.Add("Site ID:     " + pktlog.CRPpacket.siteID.ToString());
                                ntn.Nodes.Add("Network:     " + pktlog.CRPpacket.networkNumber.ToString());
                                ntn.Nodes.Add("BACnet Port: " + pktlog.CRPpacket.BACnetPort.ToString());
                                break;
                            case CRPpacket.CR_PACKET_FUNC.CRP_PONG:
                                ntn.Text += " Pong";
                                ntn.Nodes.Add("Site ID:     " + pktlog.CRPpacket.siteID.ToString());
                                ntn.Nodes.Add("Network:     " + pktlog.CRPpacket.networkNumber.ToString());
                                ntn.Nodes.Add("BACnet Port: " + pktlog.CRPpacket.BACnetPort.ToString());
                                break;
                            case CRPpacket.CR_PACKET_FUNC.CRP_SEND_BACNET_PACKET_TO_RELAY:
                                ntn.Text += " Send to CloudRelay";
                                break;
                            case CRPpacket.CR_PACKET_FUNC.CRP_SENDTOCLOUDROUTER:
                                ntn.Text += " Send to CloudRouter";
                                break;
                            case CRPpacket.CR_PACKET_FUNC.CRP_SEND_BACNET_VIA_PROXY:
                                ntn.Text += " Send to CloudRouter as Proxy";
                                break;
                            default:
                                ntn.Text += " Unknown CRP message";
                                break;
                        }

                        if (pktlog.ipep != null)
                        {
                            if (pktlog.sending == true)
                            {
                                ntn.Nodes.Add("To:   " + pktlog.ipep.ToString());
                            }
                            else
                            {
                                ntn.Nodes.Add("From: " + pktlog.ipep.ToString());
                            }
                        }

                    }

                    if (pktlog.BACnetPacket != null)
                    {

                        // display some NPDU parameters

                        if (pktlog.BACnetPacket.npdu.isBroadcast)
                        {
                            ntn.Nodes.Add("Broadcast");
                        }
                        else
                        {
                            if (pktlog.BACnetPacket.dadr != null)
                            {
                                ntn.Nodes.Add(pktlog.BACnetPacket.dadr.ToString());
                            }
                        }

                        if (pktlog.BACnetPacket.npdu.expectingReply)
                        {
                            ntn.Nodes.Add("Expecting Reply");
                        }

                        if (pktlog.BACnetPacket.apdu_present == false)
                        {
                            // todo, clean up this flag one day
                            ntn.Nodes.Add("NPDU Function: " + pktlog.BACnetPacket.npdu.function.ToString() ) ;
                        }


                        if (pktlog.BACnetPacket.APDUunconfirmedServiceDecoded)
                        {
                            switch (pktlog.BACnetPacket.serviceType)
                            {
                                case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                    ntn.Text += " I-Am";
                                    break;
                                case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                                    ntn.Text += " Who-Is";
                                    break;
                                default:
                                    ntn.Text += " Unknown BACnet unconfirmed service type - fix line 142 PacketLog.cs";
                                    break;
                            }
                        }

                        if (pktlog.BACnetPacket.APDUconfirmedServiceTypeDecoded)
                        {
                            switch (pktlog.BACnetPacket.confirmedServiceType )
                            {
                                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                                    ntn.Text += " Read Property";
                                    break;
                                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROP_MULTIPLE:
                                    ntn.Text += " Read Property Multiple";
                                    break;
                                default:
                                    ntn.Text += " Unknown BACnet confirmed service type - fix line 156 PacketLog.cs";
                                    break;
                            }
                        }


                    }



                    treeViewMessageLog.Nodes.Add(ntn);
                    treeViewMessageLog.ExpandAll();

                    // remove all the earlier nodes...
                    while (treeViewMessageLog.Nodes.Count > 150)
                    {
                        treeViewMessageLog.Nodes.RemoveAt(0);
                    }

                }

            }
        }
    }
}
