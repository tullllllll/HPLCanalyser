using System;
using System.Collections.Generic;

namespace HLPC.Models
{
    public class DataSet
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date_Added { get; set; }
        public string Machine_Type { get; set; }
        public List<Variable> Variables { get; set; }
    }
}
