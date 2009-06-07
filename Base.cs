using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetInteropApp
{
    public class Base
    {
        public List<Device> Devicelist = new List<Device>();
        public Queue<Device> NewDeviceQueue = new Queue<Device>();

    }
}
