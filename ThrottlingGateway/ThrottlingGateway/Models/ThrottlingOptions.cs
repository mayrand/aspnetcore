using System.Collections.Generic;

namespace ThrottlingGateway.Models
{
    public class ThrottlingOptions
    {
        public string ThrottlingInfoUrl { get; set; }
        public int FunctionLevelCheckInterval { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
        public int IdleCacheTime { get; set; }
        public int BusyCacheTime { get; set; }
        public bool LogRequestResponse { get; set; }
    }
}
