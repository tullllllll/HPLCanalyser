namespace HLPC.Models
{
    public class Variable
    {
        public int ID { get; set; }
        public int DataSetID { get; set; }
        // Using this "Type" you can cast to a specific type, whether it is a custom type or not
        public string Type { get; set; }
        public string Value { get; set; }
    }
}