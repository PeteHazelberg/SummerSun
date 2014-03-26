using Newtonsoft.Json;

namespace BuildingApi
{
    public class Equipment : EntityLink
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public EntityLink Type { get; set; }
        [JsonProperty(PropertyName = "isLocatedInSpace")]
        public EntityLink Location { get; set; }
        public Page<PointRole> PointRoles { get; set; }
    }
}
