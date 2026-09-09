using System;
using System.Collections.Generic;

namespace Contract
{
    public class BaseDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; }
        public int ODX { get; set; }
    }
}