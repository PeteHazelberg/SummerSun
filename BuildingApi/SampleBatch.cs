namespace BuildingApi
{
    public class SampleBatch
    {
        public EntityLink Point { get; set; }

        public Sample[] Items { get; set; }

        public SampleBatch()
        { }

        public SampleBatch(string pointId, Sample[] samples)
        {
            this.Point = new EntityLink { Id = pointId };
            this.Items = samples;
        }
    }
}
