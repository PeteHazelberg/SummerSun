
using Newtonsoft.Json;

namespace BuildingApi
{
    public class PointRole : EntityLink
    {
        [JsonProperty(PropertyName = "role")]
        public EntityLink Type { get; set; }
        public EntityLink Owner { get; set; }
        public EntityLink Point { get; set; }
    }
}
