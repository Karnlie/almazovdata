using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphVis.Models.Medicine
{
    class EdgeDoctor
    {
        public int index1;
        public int index2;

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Doctor))
                return false;

            return ((EdgeDoctor)obj).index1 == this.index1 && ((EdgeDoctor)obj).index2 == this.index2 || ((EdgeDoctor)obj).index2 == this.index1 && ((EdgeDoctor)obj).index1 == this.index2;
        }

        public override int GetHashCode()
        {
            return (this.index1 + this.index2).GetHashCode();
        }  
    }
}
