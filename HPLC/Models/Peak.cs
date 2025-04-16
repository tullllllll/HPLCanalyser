using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HPLC.Models
{
    public class Peak
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int DataSetID { get; set; }
        [Required]
        public double StartTime { get; set; }
        [Required]
        public double EndTime { get; set; }
        public string Name { get; set; }

        // Required for reference navigation
        [NotMapped]
        public DataSet DataSet { get; set; } = null!;
    }
}