using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace APIServices
{
    public static class State
    {
        public static int Delay = 0;
        public static readonly List<string> Values = new List<string> { "value1", "value2" };
    }
}
