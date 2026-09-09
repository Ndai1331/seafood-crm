namespace Contract.Positions
{
    public class CreateUpdatePositionDto
    {
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public int? ParentCode { get; set;}
        public int ODX { get; set; }
    }
}