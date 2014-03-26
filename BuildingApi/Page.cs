using System.Collections.Generic;

namespace BuildingApi
{
    public class Page<T> : Link
    {
        public IEnumerable<T> Items { get; set; }
        public Link Next { get; set; }
        public Link Prev { get; set; }
    }
}
