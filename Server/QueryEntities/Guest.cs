using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraDataLayer.QueryEntities
{
    public class Guest
    {
        public string phone { get; set; }
        public string email { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
    }
}
