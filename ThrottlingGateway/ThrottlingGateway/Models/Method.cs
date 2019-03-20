using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThrottlingGateway.Models
{
    public class Method
    {
        public string Name { get; set; }
        public int LowThrottleTime { get; set; }
        public int HighThrottleTime { get; set; }
        public bool Enabled { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Levels Level { get; set; }
    }

    public enum Levels
    {
        [EnumMember(Value = "Low")]
        Low,
        [EnumMember(Value = "High")]
        High,
        [EnumMember(Value = "Function")]
        Function
    }
}