using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO.Model
{
    [Serializable]
    public class HoursEntry
    {
        public HoursEntry()
        {
            //Empty
        }    

        public string JobNumber { get; set; }
        public string PhaseCode { get; set; }
        public double SummedHours { get; set; }
    }
}
