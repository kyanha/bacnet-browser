using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace BACnetInteropApp
{
    public class Packet
    {
        public System.Net.IPAddress Source_Address;
        public int Source_Port;
        public byte[] buffer; 
    }
}
