using System;

namespace Contract.AppHistories
{
    public class CreateUpdateAppHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { set; get; }
        public string IpAddress { get; set; }
        public string Functions { get; set; }
        public string Operation { get; set; }
    }
}