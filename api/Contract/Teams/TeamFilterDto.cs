namespace Contract.Teams
{
    public class TeamFilterDto : BaseFilterPagingDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }
}

