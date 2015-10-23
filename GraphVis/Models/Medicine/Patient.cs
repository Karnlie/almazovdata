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

    }
}
