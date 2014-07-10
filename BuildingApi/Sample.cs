using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BuildingApi
{
    public class Sample
    {
        [JsonProperty(PropertyName = "val")]
        public double Value { get; set; }
        [JsonProperty(PropertyName = "ts")]
        public DateTime Timestamp { get; set; }
        public IEnumerable<string> Flags { get; set; }
    }
}
