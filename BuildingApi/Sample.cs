using System;
using Newtonsoft.Json;

namespace BuildingApi
{
    public class Sample
    {
        [JsonProperty(PropertyName = "val")]
        public double? Value { get; set; }

        [JsonProperty(PropertyName = "ts")]
        public DateTime Timestamp { get; set; }

        public string[] Flags { get; set; }

        public AuditInfo Created { get; set; }

        public AuditInfo Updated { get; set; }

    }
}
