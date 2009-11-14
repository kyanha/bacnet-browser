using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using BACnetLibraryNS ;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibraryNS
{
    public class myTreeNode : TreeNode
    {
        public BACnetEnums.TREENODE_OBJ_TYPE type;
        public bool isDevice = false; // todo, migrate isDevice to type

        public Device device = new Device();

        public IPEndPoint ipep;

        public long lastHeardFromTime  ;
        public uint networkNumber;
        public BACnetObjectIdentifier oID;

        // We can determine our local (directly connected) network number from the single network number
        // in a router table that is not reported by the who-is-router request. These flags help do that.
        public bool networkNumberFromWhoIsRouter;       
        public bool networkNumberFromInitRouterTable;

        public myTreeNode()
        {
        }

        public myTreeNode(string text)
        {
            Text = text;
        }

        public myTreeNode AddMyTreeNodeObject ( BACnetEnums.TREENODE_OBJ_TYPE tnoType, string text )
        {
            myTreeNode mtn = new myTreeNode( text );
            mtn.type = tnoType;
            base.Nodes.Add(mtn);
            return mtn;
        }
    }
}
