namespace Contract.Common.Excels
{
    public class DataValidatorExcel
    {
        public List<Cell> InvalidCells { get; set; } = new List<Cell>();
        public List<string> InvalidLogics { get; set; } = new List<string>();
        public bool IsSuccessful { get; set; }
    }
}
