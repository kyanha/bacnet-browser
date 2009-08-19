using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using BACnetLibraryNS ;

namespace BACnetInteropApp
{
    class myTreeNode : TreeNode
    {
        public bool isDevice = false;
        public Device device = new Device();

    }
}
