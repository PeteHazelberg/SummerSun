using System.Collections.Generic;

namespace BuildingApi
{
    public class RawSamplesFile
    {
        public string Customer { get; set; }
        public string ReceivedVia { get; set; }
        public AuditInfo Created { get; set; }
        public IEnumerable<SampleBatch> Samples { get; set; }
    }
}
