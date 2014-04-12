namespace BuildingApi
{
    public class SampleSummary : Link
    {
        public int Count { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public string OldestSample { get; set; }
        public string NewestSample { get; set; }
        public Newest Newest { get; set; }
    }

    public class Newest 
    {
        public double val { get; set; }
        public string ts { get; set; }
    }
}
