using Newtonsoft.Json;

namespace BuildingApi
{
    public class Link
    {
        [JsonProperty(PropertyName = "href", NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; set; }
    }
}
