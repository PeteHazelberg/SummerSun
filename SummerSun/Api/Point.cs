
namespace SummerSun.Api
{
    public class Point : EntityLink
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public EntityLink Units { get; set; }
        public EntityLink States { get; set; }
        public SampleSummary SampleSummary { get; set; }
    }
}
