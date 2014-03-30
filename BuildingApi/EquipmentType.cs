using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingApi
{
    public class EquipmentType : EntityLink
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public EntityLink[] PointRoleTypes { get; set; }
    }
}
