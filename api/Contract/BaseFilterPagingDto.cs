using Core.Enum;

namespace Contract
{
    public class BaseFilterPagingDto
    {
        public string? FilterText { get; set; }
        public string? Title { get; set; }
        public string? PostCategoryName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? No { get; set; }
        public string? Code { get; set; }
        public string? UserFullName { get; set; }
        public string? CompanyName { get; set; }
        public string? UserCode { get; set; }
        public int? IntId { get; set; }
        public bool? IsActive { get; set; }
        public int? GuidId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public string? Sort { set; get; }
        public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Now;
    }

    public class BaseFilterByDateTimeDto : BaseFilterPagingDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }


    public class FilterByDateTimeDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
