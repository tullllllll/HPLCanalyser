using System;
using System.Collections.Generic;

namespace HLPC.Models
{
    public class DataSet
    {
        // Backing field for Date_Added in UTC
        private DateTime _dateAddedUtc;

        public int ID { get; set; }
        public string Name { get; set; }

        // Property for Date_Added that returns Local Time (for UI purposes)
        public DateTime Date_Added { get; set; }

        public List<DataPoint> DataPoints { get; set; }
    }
}
