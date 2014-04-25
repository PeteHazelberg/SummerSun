namespace BuildingApi
{
    public class SampleSummary : Link
    {
        public int Count { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public string OldestSample { get; set; }
        public string NewestSample { get; set; }
        public Sample Newest { get; set; }
    }
}
