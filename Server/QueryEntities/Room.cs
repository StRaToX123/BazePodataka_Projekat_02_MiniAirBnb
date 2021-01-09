using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraDataLayer.QueryEntities
{
    public class Room
    {
        public string hotelID { get; set; }
        public string roomID { get; set; }
        public string hottub { get; set; }
        public string num { get; set; }
        public string rate { get; set; }
        public string tv { get; set; }
        public string type { get; set; }
    }
}
