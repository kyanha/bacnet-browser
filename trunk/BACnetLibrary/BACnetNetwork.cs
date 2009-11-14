using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibraryNS
{
    public class BACnetNetwork : IComparable<BACnetNetwork>, IEquatable<BACnetNetwork>
    {
        public uint NetworkNumber;
        public bool directlyConnected = false ; 

        public int CompareTo(BACnetNetwork d)
        {
            if (this.directlyConnected == true && d.directlyConnected == false) return 1;
            if (this.directlyConnected == false && d.directlyConnected == true) return -1;

            // sort order is relevant...
            if (this.NetworkNumber > d.NetworkNumber) return 1;

            if (this.NetworkNumber < d.NetworkNumber) return -1;

            // Networks must be equal
            return 0;
        }

        public bool Equals(BACnetNetwork d)
        {
            if (this.directlyConnected != d.directlyConnected) return false;
            if (this.NetworkNumber != d.NetworkNumber ) return false;
            return true;
        }

    }
}
