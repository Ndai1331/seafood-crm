using System;

namespace Contract.Base
{
    public class DropDownListBase
    {
        public string? Text { get; set; }
    }

    public class DropDownListIntBase : DropDownListBase
    {
        public int? Value { get; set; }
    }
    
    public class DropDownListBase<T> : DropDownListBase
    {
        public T? Value { get; set; }
    }

    public class DropDownListBoolBase : DropDownListBase
    {
        public bool? Value { get; set; }
    }
}
