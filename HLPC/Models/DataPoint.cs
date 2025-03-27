namespace HLPC.Models
{
    public class DataPoint
    {
        public int ID { get; set; }
        public int DataSetID { get; set; }
        // Using this "Type" you can cast to a specific type, whether it is a custom type or not
        public double Time { get; set; }
        public double Value { get; set; }
    }
}