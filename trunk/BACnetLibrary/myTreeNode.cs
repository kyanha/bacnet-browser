/*
 * The MIT License
 * 
 * Copyright (c) 2010 BACnet Iteroperability Testing Services, Inc.
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 *  BACnet Interoperability Testing Services, Inc.
 *      http://www.bac-test.com
 * 
 * BACnet Wiki
 *      http://www.bacnetwiki.com
 * 
 * MIT License - OSI (Open Source Initiative) Approved License
 *      http://www.opensource.org/licenses/mit-license.php
 * 
*/

/*
 * 28 Nov 10    EKH Releasing under MIT license
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using BACnetLibrary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Diagnostics;

namespace BACnetLibrary
{
    public class myTreeNode : TreeNode
    {
        public enum TREENODE_OBJ_TYPE
        {
            NetworkNumber = 33,
            LastAccessTime,
            BACnetObject,
            Device,
            DeviceInstance,
            VendorID,
            Router,
            BACnetPort,
            GenericText,
            State,
            MACaddress,
            PortNetIPEP,
            CloudNetName,
            BACnetADR,
            RouterTableEntry,
            DiagnosticDetails,
            ScheduledDiagnostics,
            CompleteDiagnostics,
        }

        public TREENODE_OBJ_TYPE type;
        // public bool isDevice = false; // todo, migrate isDevice to type

        public Device device = new Device();
        // public Router router;

        public IPEndPoint ipep;

        public long lastHeardFromTime;
        public uint networkNumber;
        public RoutingTableEntry rte;
        public Diagnostic diagnostic;

        public BACnetObjectIdentifier oID;

        public myTreeNode()
        {
        }

        public myTreeNode(string text)
        {
            Text = text;
        }

        public myTreeNode(TREENODE_OBJ_TYPE tnoType, string text)
        {
            Text = TextFromType(tnoType) + text;
            this.type = tnoType;
        }

        //public myTreeNode(TREENODE_OBJ_TYPE tnoType, string text, System.Drawing.Color clr)
        //{
        //    Text = TextFromType(tnoType) + text;
        //    this.type = tnoType;
        //    this.BackColor = clr;
        //}

        public myTreeNode(TREENODE_OBJ_TYPE tnoType)
        {
            this.type = tnoType;
            Text = TextFromType(tnoType);
        }

        string TextFromType(TREENODE_OBJ_TYPE tnoType)
        {
            switch (tnoType)
            {
                case TREENODE_OBJ_TYPE.NetworkNumber:
                    return "Network Number  ";
                case TREENODE_OBJ_TYPE.PortNetIPEP:
                    return "Site IP Addr    ";
                case TREENODE_OBJ_TYPE.State:
                    return "State           ";
                case TREENODE_OBJ_TYPE.BACnetPort:
                    return "Port            ";
                case TREENODE_OBJ_TYPE.LastAccessTime:
                    return "Last comms      ";
                case TREENODE_OBJ_TYPE.CloudNetName:
                    return "CloudNet Name   ";
            }
            return String.Format ( "{0,-17}", tnoType.ToString()) ;
        }

        public override string ToString()
        {
            return TextFromType ( type ) ;
        }

        public myTreeNode AddMyTreeNodeObject(TREENODE_OBJ_TYPE tnoType, string text)
        {
            myTreeNode mtn = new myTreeNode(text);
            mtn.type = tnoType;
            base.Nodes.Add(mtn);
            return mtn;
        }


        public myTreeNode FindmyTreeNode ( TREENODE_OBJ_TYPE tnoType )
        {
            foreach ( myTreeNode mtn in this.Nodes )
            {
                if ( mtn.type == tnoType ) return mtn ;
            }
            return null;
        }


        public myTreeNode EnsureMyTreeNodeObject(TREENODE_OBJ_TYPE tnoType, string text)
        {
            // Only adds if it has to, if there is already such an object, simply returns that object
            myTreeNode mtn = FindmyTreeNode ( tnoType ) ;
            if ( mtn != null )
            {
                // update the text
                mtn.Text = text ;
                return mtn;
            }
            // else create one
            mtn = new myTreeNode(text);
            mtn.type = tnoType;
            base.Nodes.Add(mtn);
            return mtn;
        }

        public void UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE objType, string text)
        {
            // search for a subnode that matches objType and update the text field
            if (Nodes.Count > 0) foreach (myTreeNode mtn in Nodes)
                {
                    if (mtn.type == objType)
                    {
                        mtn.Text = TextFromType(objType) + text;
                        return;
                    }
                }
            // None found, so create
            AddMyTreeNodeObject(objType, TextFromType(objType) + text);
        }

        //public void UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE objType, string text, System.Drawing.Color clr)
        //{
        //    // search for a subnode that matches objType and update the text field
        //    if (Nodes.Count > 0) foreach (myTreeNode mtn in Nodes)
        //        {
        //            if (mtn.type == objType)
        //            {
        //                mtn.Text = TextFromType(objType) + text;
        //                mtn.BackColor = clr;
        //                return;
        //            }
        //        }
        //    // None found, so create
        //    AddMyTreeNodeObject(objType, TextFromType ( objType) + text);
        //}
    }
}
