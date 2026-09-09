namespace Contract.Base
{
    public class FilterPagingBase : FilterBase
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public string? Sort { set; get; }
    }
}