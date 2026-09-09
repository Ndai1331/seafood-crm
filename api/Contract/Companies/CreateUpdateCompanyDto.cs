using System;

namespace Contract.Companies
{
    public class CreateUpdateCompanyDto
    {
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? ContractNo { get; set; }
        public DateTime? ContractDate { get; set; }
        public string? Address { get; set; }
        public string? Tel { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public int ODX { get; set; }
    }
}