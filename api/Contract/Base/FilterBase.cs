using System;

namespace Contract.Base
{
    public class FilterBase
    {
        public string? Text { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public DateTime? StartDay { get; set;}
        public DateTime? EndDay { get; set; }
    }
}