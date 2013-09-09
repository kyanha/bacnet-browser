using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Reporters;

namespace BaseLibrary
{
    public class staticsForBaseLib
    {
        static public bool shutDownInitiated;
    }



    public class BaseLib
    {
        public static Icon globalIcon;

        public static string version
        {
            get
            {
                return Reports.version;
            }
            set
            {
                throw new Exception ("m0505 - Use the Assembly properties to set the version");
            }
        }


    }
}
