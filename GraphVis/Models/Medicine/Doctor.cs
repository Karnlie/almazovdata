using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphVis.Models.Medicine
{
    public class Doctor
    {
        public int id;
        public String fio;
        public String category;

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Doctor)) 
                return false;

            return ((Doctor)obj).fio.Replace(" ", "").Equals(this.fio.Replace(" ", "")) && ((Doctor)obj).category.Replace(" ", "").Equals(this.category.Replace(" ", ""));
        }

        public override int GetHashCode()
        {
            return (this.fio.Replace(" ", "") + this.category.Replace(" ", "")).GetHashCode();
        }  
    }
}
