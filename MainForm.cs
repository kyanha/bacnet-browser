using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Threading;

namespace BACnetInteropApp
{
    public partial class MainForm : Form
    {

        static void SendWhoIs()
        {
            byte[] data = new byte[1024];
            int optr = 0;

            //            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.103"), 0xBAC0 );
            //            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC0);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 0xBAC0);

            IPEndPoint local_ipep = new IPEndPoint(0, 0xBAC1);// NOTE, NOT BAC0, so that we can run IUT bacnet server on same machine

            Socket bacnet_master_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // bind the local end of the connection to BACnet port number
            bacnet_master_socket.Bind(local_ipep);

            bacnet_master_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP ;
            data[optr++] = BACnetEnums.BACNET_ORIGINAL_BROADCAST_NPDU;
            data[optr++] = 0x00;
            data[optr++] = 0x0c;
            data[optr++] = 0x01;
            data[optr++] = 0x20;
            data[optr++] = 0xff;
            data[optr++] = 0xff;
            data[optr++] = 0x00;
            data[optr++] = 0xff;
            data[optr++] = 0x10;
            data[optr++] = 0x08;

            Console.WriteLine("Sending\n");

            bacnet_master_socket.SendTo(data, optr, SocketFlags.None, ipep);

            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint tmpRemote = (EndPoint)sender;

            //data = new byte[1024];
            //int recv = bacnet_master_socket.ReceiveFrom(data, ref tmpRemote);

            //Console.WriteLine("Message received from {0}:", tmpRemote.ToString());
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
            //Console.WriteLine(Encoding.ASCII.GetBytes((char[])data, 0, recv));
            /*
                        server.SendTo(Encoding.ASCII.GetBytes("message 1"), tmpRemote);
                        server.SendTo(Encoding.ASCII.GetBytes("message 2"), tmpRemote);
                        server.SendTo(Encoding.ASCII.GetBytes("message 3"), tmpRemote);
                        server.SendTo(Encoding.ASCII.GetBytes("message 4"), tmpRemote);
                        server.SendTo(Encoding.ASCII.GetBytes("message 5"), tmpRemote);
            */

            // Console.WriteLine("Stopping client");
            bacnet_master_socket.Close();

        }

        Alpha oAlpha;
        Thread oThread;

        public MainForm()
        {
            InitializeComponent();

            // fire up a thread to watch for incoming packets

            oAlpha = new Alpha();
            
            oThread = new Thread(new ThreadStart(oAlpha.Beta));
            oThread.Start();

        }



        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.ExpandAll();     


        }

        private void CollapseAllButton_Click(object sender, EventArgs e)
        {
            this.treeView2.CollapseAll();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TreeNode mainNode = new TreeNode();

            mainNode.Name = "mainNode";

            mainNode.Text = "Added";

            this.treeView2.Nodes.Add(mainNode);
        }


        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode nod = new TreeNode();
            nod.Name = "able bunny";
            nod.Text = "able bunny2";
            nod.Tag = "able bunny3";
            this.treeView2.SelectedNode.Nodes.Add(nod);
            this.treeView2.SelectedNode.ExpandAll();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            TreeNode nod = new TreeNode();
            nod.Name = "able bunny";
            nod.Text = "able bunny2";
            nod.Tag = "able bunny3";
            this.treeView2.SelectedNode.Nodes.Add(nod);
            this.treeView2.SelectedNode.Expand();
        }

        static int snode = 0;

        private void button6_Click(object sender, EventArgs e)
        {
            this.treeView2.SelectedNode = this.treeView2.Nodes[snode++];
            this.treeView2.SelectedNode.ExpandAll();
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            AboutBox1 mab = new AboutBox1();

            mab.ShowDialog();
        }

        //private void toolStripButton1_Click(object sender, EventArgs e)
        //{

        //}

        private void SendWhoIsButton(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Sending Who_is");
            SendWhoIs();
        }



        List<Device> ourdevices = new List<Device>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Check to see if there is a new "I-Am" message to process.

            while (  oAlpha.NewDeviceQueue.Count != 0 ) {

                Device D = oAlpha.NewDeviceQueue.Dequeue();

                // is this a new Device?

                if (!ourdevices.Contains(D))
                {
                    // is new

                    TreeNode NewNode = new TreeNode();

                    NewNode.Name = "NewNode";

                    NewNode.Text = "Device " + D.DeviceId ;
                    NewNode.Tag = D.DeviceId ;

                    this.treeView2.Nodes.Add(NewNode);
                    this.treeView2.SelectedNode = NewNode;

                    // add other paramters to our new node

                    this.treeView2.SelectedNode.Nodes.Add( "Vendor ID      " + D.VendorId );
                    this.treeView2.SelectedNode.Nodes.Add( "Network Number " + D.NetworkNumber );
                    
                    /*
                    TreeNode ntn = new TreeNode( "Test4" ) ;

                    this.treeView2.SelectedNode.Nodes.Add(ntn);

                    TreeNode sn = this.treeView2.SelectedNode.Nodes[0];

                    sn.Nodes.Add("Test5");
                    */

                    ourdevices.Add(D);
                }
                else
                {
                    //if we get here, we _know_ that treeview2 contains our node
                                        
                    for ( int i=0; i<this.treeView2.Nodes.Count; i++ )
                    {
                        TreeNode tni = this.treeView2.Nodes[i];

                        if (tni.Tag != null)
                        {
                            if (tni.Tag.Equals(D.DeviceId))
                            {
                                // found it.. select it
                                this.treeView2.SelectedNode = tni;
                                
                            }
                        }
                        
                    }
                }

            }

            }
                       
    
        private void button7_Click(object sender, EventArgs e)
        {
            oThread.Abort();
            //oThread.Join(2000);

            // cancel our outstanding receive call
            try
            {
                oAlpha.BetaClose();
            }
            catch( Exception fe )
            {
                Console.WriteLine(fe);
            }

            Application.Exit();
        }

        // on shutdown

        

    }
}
