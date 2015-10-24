using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphVis.Models.Medicine
{
    public class Patient
    {
        public Patient(String id)
        {
            this.id = id;
        }
        public String id;
        public List<Visit> visitList;

        public void addNewVisit(Visit visit)
        {
            if (visitList == null)
            {
                visitList=new List<Visit>();
            }
            visitList.Add(visit);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Patient))
                return false;

            return ((Patient)obj).id.Equals(this.id);
        }

        public override int GetHashCode()
        {
            return (this.id).GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(id);
            builder.Append('\n');
            foreach (var visit in visitList)
            {
                builder.Append(';');
                builder.Append(visit.id);
                builder.Append(';');
                builder.Append(visit.fio);
                builder.Append(';');
                builder.Append(visit.category);
                builder.Append(';');
                builder.Append(visit.date);
                builder.Append('\n');
            }
            return builder.ToString();
        }
    }
}
