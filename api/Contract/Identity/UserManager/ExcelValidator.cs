using System.Collections.Generic;
using Contract.Common;
using Contract.Common.Excels;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserValidatorExcel
    {
        public List<Cell> InvalidCells { get; set; } = new List<Cell>();
        public List<string> InvalidLogics { get; set; } = new List<string>();
        public bool IsSuccessful { get; set; }
    }
    
}