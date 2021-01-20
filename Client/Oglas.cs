using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Zahtev
    {
        public Zahtev() { }

        public int id;
        public int status;
        public List<DateTime> datumi;
    }

    public class Oglas
    {
        public Oglas() { }
        public int id;
        public string username;
        public bool wifi;
        public bool tus;
        public bool parking_mesto;
        public bool tv;
        public string adresa;
        public string datumi;
        public byte[] slika;
        public int slikaReceivedNumberOfBytes;
        public string opis;
        public List<Zahtev> zahtevi;
        public bool slikaPrimljena;
        public bool oglasPrimljen;

    }

    public class BoolWrapper
    {
        public BoolWrapper() { }

        private bool _variable;

        public bool Variable { get; set; }
    }
}
