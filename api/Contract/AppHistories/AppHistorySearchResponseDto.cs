namespace Contract.AppHistories
{
    public class AppHistorySearchResponseDto
    {
        public int TotalItem { set; get; } = 0;
        public List<AppHistoryDto> Result { set; get; } = new List<AppHistoryDto>();
    }
}
