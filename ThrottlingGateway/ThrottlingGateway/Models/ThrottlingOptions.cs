using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThrottlingGateway.Models
{
    public class ThrottlingOptions
    {
        public string ThrottlingInfoUrl { get; set; }
        public int FunctionLevelCheckInterval { get; set; }
        public List<Method> Methods { get; set; }
        public int IdleCacheTime { get; set; }
        public int BusyCacheTime { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public int LowThrottleTime { get; set; }
        public int HighThrottleTime { get; set; }
        public bool Enabled { get; set; }
        public string Level { get; set; }
    }
}
