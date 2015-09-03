using System;

namespace BuildingApi
{
    public class Point : Entity, IEquatable<Point>
    {
        public EntityLink Units { get; set; }
        public EntityLink States { get; set; }
        public SampleSummary SampleSummary { get; set; }
        public bool? SamplesRequested { get; set; }
        public EntityLink Samples { get; set; }
        public Page<PointRole> PerformsPointRoles { get; set; }
        public Page<Equipment> IsUsedByEquipment { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Point)obj);
        }

        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
