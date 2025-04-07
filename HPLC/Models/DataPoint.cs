using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HPLC.Models
{
    public class DataPoint
    {
        [Key]
        public int ID { get; set; }
        public int DataSetID { get; set; }
        [Required]
        public double Time { get; set; }
        [Required]
        public double Value { get; set; }

        // Required for reference navigation
        [NotMapped]
        public DataSet DataSet { get; set; } = null!;
    }
}