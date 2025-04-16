using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HPLC.Models
{
    public class DataSet
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Date_Added { get; set; }
        public double Frequency { get; set; }
        public ICollection<DataPoint> DataPoints { get; set; } = [];
        public ICollection<Peak> Peaks { get; set; } = [];
    }
}
