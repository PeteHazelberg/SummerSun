using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingApi
{
    public class Equipment : Entity
    {
        public EntityLink Type { get; set; }
        
        [JsonProperty(PropertyName = "isLocatedInSpace")]
        public EntityLink Location { get; set; }

        [JsonProperty(PropertyName = "servesEquipment")]
        public Page<Equipment> servesEquipment { get; set; }

        [JsonProperty(PropertyName = "isServedByEquipment")]
        public Page<Equipment> isServedByEquipment { get; set; }

        public Page<PointRole> PointRoles { get; set; }

        [JsonProperty(PropertyName = "servesSpaces")]
        public Page<Space> servesSpaces { get; set; }
        
    }
}
