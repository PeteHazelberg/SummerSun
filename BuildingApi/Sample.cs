using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuildingApi
{
    public class Sample
    {
        [JsonProperty(PropertyName = "val")]
        public double Value { get; set; }
        [JsonProperty(PropertyName = "ts")]
        public DateTime Timestamp { get; set; }
        public string Flags { get; set; }
    }
}
