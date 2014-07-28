using System;

namespace BuildingApi
{
    public class AuditInfo
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public DateTime At { get; set; }
    }
}
