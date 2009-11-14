// AppManager is our catch all for 'Global Objects' as well as the interface between Application Runtime code and the various User Interface forms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BACnetLibraryNS
{
    public class AppManager
    {
        public Stopwatch _stopWatch = Stopwatch.StartNew();

        public DeviceTreeView treeViewUpdater;

        // temporary until debub messages migrated

        public BACnetmanager bnm;
        
    }
}
