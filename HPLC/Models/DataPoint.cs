using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HPLC.Models
{
    public class DataPoint : ObservableObject
    {
        [Key]
        public int ID { get; set; }
        public int DataSetID { get; set; }
        [Required]
        public double Time { get; set; }
        [Required]
        public double Value { get; set; }

        // Required for reference navigation
        public DataSet DataSet { get; set; } = null!;
    }
}