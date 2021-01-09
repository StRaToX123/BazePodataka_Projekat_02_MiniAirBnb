using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;

using Cassandra;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.IO;

namespace Server.SignalRServer
{
    [HubName("TestHub")]
    public class TestHub : Hub
    {
        private ISession session; // cassandra session

        public class Zahtev
        {
            public Zahtev() { }
            public string id;
            public string stan_id;
            public string status;
            public string datumi;
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
            public LocalDate earliest_date;
            public LocalDate farthest_date;
            public List<LocalDate> datumi;
            public byte[] slika;
            public string opis;
            public List<Zahtev> zahtevi;
            public bool dodajOglasPozvato;
            public bool slikaPostavljena;
        }

        // imamo dva recnika jedan u kome je connectionID ismedju servera i klijenta kljuc koji vodi ka account username
        // i reverse recnik od toga, gde account username vodi ka connectionID.
        public static ConcurrentDictionary<string, string> connectedUsersConnectionIDToUsername = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> connectedUsersUsernameToConnectionID = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> connectedUsersConnectionIDToPassword = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, List<byte[]>> connectedUsersConnectionIDToImageBlockList = new ConcurrentDictionary<string, List<byte[]>>();
        public static ConcurrentDictionary<string, Oglas> connectedUsersConnectionIDToOglas = new ConcurrentDictionary<string, Oglas>();

        public TestHub()
        {
            try
            {
                Cluster cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
                session = cluster.Connect("MiniAirBnb");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
  
        public override Task OnDisconnected(bool stopCalled)
        {
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            if (callerUsername != null)
            {
                string garbage;
                connectedUsersUsernameToConnectionID.TryRemove(callerUsername, out garbage);
                connectedUsersConnectionIDToUsername.TryRemove(Context.ConnectionId, out garbage);
            }
            
            return base.OnDisconnected(stopCalled);
        }


        public void LogIn(string username, string password)
        {
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            if (callerUsername != null)
            {
                Clients.Caller.LogInFailed("nista");
                return;
            }

            // Provera da li su ta kombinacija username-a i password-a postoji
            Row foundAccount = session.Execute("SELECT username FROM \"Accounts\" where username = '" + username + "' and \"password\" = '" + password + "'").FirstOrDefault();
            if (foundAccount == null)
            {
                Clients.Caller.LogInFailed("nesto");
                return;
            }

            connectedUsersConnectionIDToUsername.TryAdd(Context.ConnectionId, username);
            connectedUsersUsernameToConnectionID.TryAdd(username, Context.ConnectionId);
            connectedUsersConnectionIDToPassword.TryAdd(Context.ConnectionId, password);

            Clients.Caller.LogInSuccessful(username);
        }

        // Radi isto sto i Disconnect samo na zahtev korisnika
        public void LogOut(string nista)
        {
            // Prvo moramo javiti 
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            if (callerUsername != null)
            {
                // Sada mozemo da obrisemo klijenta koji se diskonektovao is nasih recnika
                string garbage;
                List<byte[]> garbageList;
                Oglas garbageOglas;
                connectedUsersUsernameToConnectionID.TryRemove(callerUsername, out garbage);
                connectedUsersConnectionIDToUsername.TryRemove(Context.ConnectionId, out garbage);
                connectedUsersConnectionIDToPassword.TryRemove(Context.ConnectionId, out garbage);
                connectedUsersConnectionIDToImageBlockList.TryRemove(Context.ConnectionId, out garbageList);
                connectedUsersConnectionIDToOglas.TryRemove(Context.ConnectionId, out garbageOglas);

                Clients.Caller.LogOutSuccessful("nista");
            }
        }

        public void DetermineLength(string message)
        {
            // Console.WriteLine(message);

            // string newMessage = string.Format(@"{0} has a length of: {1}", message, message.Length);
            // Clients.Caller.ReceiveLength(newMessage);
            MessageBox.Show(message);
        }

        public void CreateAccount(string username, string password, string email)
        {
            // Provera da li takav nalog vec postoji
            Row foundUsername = session.Execute("SELECT username FROM \"Accounts\" where username = '" + username + "'").FirstOrDefault();

            if (foundUsername != null)
            {
                Clients.Caller.AccountCreationFailed("nesto");
                return;
            }

            // Napravi novi nalog
            RowSet accountData = session.Execute("insert into \"Accounts\" (username, \"password\", email, zahtevi)  values ('" + username + "', '" + password + "', '" + email + "', [])");

            Clients.Caller.AccountCreatedSuccessfuly("nista");
        }

        public void DodajOglas(string adresa, bool wifi, bool tus, bool parking_mesto, bool tv, string opis, string datumi)
        {
            Oglas oglas;
            connectedUsersConnectionIDToOglas.TryGetValue(Context.ConnectionId, out oglas);
            if (oglas == null)
            {
                oglas = new Oglas();
                oglas.datumi = new List<LocalDate>();
                oglas.dodajOglasPozvato = true;
                oglas.slikaPostavljena = false;
                connectedUsersConnectionIDToOglas.TryAdd(Context.ConnectionId, oglas);
            }

            oglas.adresa = adresa;
            oglas.wifi = wifi;
            oglas.tus = tus;
            oglas.parking_mesto = parking_mesto;
            oglas.tv = tv;
            oglas.opis = opis;
            // Popuniti listu LocalDate
            string[] dates = datumi.Split('@');
            for (int i = 0; i < dates.Length; i++)
            {
                string[] data = dates[i].Split('-');
                int godina;
                int mesec;
                int dan;
                Int32.TryParse(data[0], out godina);
                Int32.TryParse(data[1], out mesec);
                Int32.TryParse(data[2], out dan);
                oglas.datumi.Add(new LocalDate(godina, mesec, dan));

                if (i == 0)
                {
                    oglas.earliest_date = oglas.datumi.Last();
                    oglas.farthest_date = oglas.earliest_date;
                }
                else
                {
                    if (oglas.datumi.Last() < oglas.earliest_date)
                    {
                        oglas.earliest_date = oglas.datumi.Last();
                    }

                    if (oglas.datumi.Last() > oglas.farthest_date)
                    {
                        oglas.farthest_date = oglas.datumi.Last();
                    }
                }
            }          

            oglas.dodajOglasPozvato = true;

            if (oglas.slikaPostavljena == true) // znaci da se ovaj metod pozvao nakon slanja slike
            {
                // Nabavi id stana
                Row maxStanId = session.Execute("SELECT MAX(id) FROM \"Stanovi\"").FirstOrDefault();
                int maxStanIdInt = 0;
                if (maxStanId == null)
                {
                    maxStanIdInt = 0;
                }
                else
                {
                    if (maxStanId["system.max(id)"] == null)
                    {
                        maxStanIdInt = 0;
                    }
                    else
                    {
                        maxStanIdInt = (int)maxStanId["system.max(id)"];
                        maxStanIdInt++;
                    }
                }

                // Nabavi id za UsernameToStanovi
                Row maxUsernameToStanoviId = session.Execute("SELECT MAX(id) FROM \"UsernameToStanovi\"").FirstOrDefault();
                int maxUsernameToStanoviIdInt = 0;
                if (maxUsernameToStanoviId == null)
                {
                    maxUsernameToStanoviIdInt = 0;
                }
                else
                {
                    if (maxUsernameToStanoviId["system.max(id)"] == null)
                    {
                        maxUsernameToStanoviIdInt = 0;
                    }
                    else
                    {
                        maxUsernameToStanoviIdInt = (int)maxUsernameToStanoviId["system.max(id)"];
                        maxUsernameToStanoviIdInt++;
                    }
                }

                // Nabavi id za UsernameToStanovi
                Row maxDatumiToStanoviId = session.Execute("SELECT MAX(id) FROM \"DatumiToStanovi\"").FirstOrDefault();
                int maxDatumiToStanoviIdInt = 0;
                if (maxDatumiToStanoviId == null)
                {
                    maxDatumiToStanoviIdInt = 0;
                }
                else
                {
                    if (maxDatumiToStanoviId["system.max(id)"] == null)
                    {
                        maxDatumiToStanoviIdInt = 0;
                    }
                    else
                    {
                        maxDatumiToStanoviIdInt = (int)maxDatumiToStanoviId["system.max(id)"];
                        maxDatumiToStanoviIdInt++;
                    }
                }

                string callerUsername;
                connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);

                var upit = session.Prepare("insert into \"Stanovi\" " +
                    "(id, username_to_stan_id, datumi_to_stan_id, username, adresa, adresa_terms, wifi, tus, parking_mesto, tv, datumi, opis, slika) " +
                    "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

                LocalDate[] localDateArray = new LocalDate[oglas.datumi.Count];
                for (int i = 0; i < oglas.datumi.Count; i++)
                {
                    localDateArray[i] = oglas.datumi[i];
                }

                // napravi listu svih mogucih kombinacija reci za adresa_terms radi robusnije pretrage
                List<string> listaReciAdrese = (oglas.adresa.ToLower()).Split(' ').ToList();
                int comboCount = (int)Math.Pow(2, listaReciAdrese.Count) - 1;
                List<List<string>> sveMogucePermutacijeReciAdrese = new List<List<string>>();
                for (int i = 1; i < comboCount + 1; i++)
                {
                    sveMogucePermutacijeReciAdrese.Add(new List<string>());
                    for (int j = 0; j < listaReciAdrese.Count; j++)
                    {
                        if ((i >> j) % 2 != 0)
                        {
                            sveMogucePermutacijeReciAdrese.Last().Add(listaReciAdrese[j]);
                        }
                            
                    }
                }

                string[] sveMogucePermutacijeReciAdreseArray = new string[sveMogucePermutacijeReciAdrese.Count];
                for (int i = 0; i < sveMogucePermutacijeReciAdrese.Count; i++)
                {
                    for (int j = 0; j < sveMogucePermutacijeReciAdrese[i].Count; j++)
                    {
                        sveMogucePermutacijeReciAdreseArray[i] += sveMogucePermutacijeReciAdrese[i][j];
                        if (j != (sveMogucePermutacijeReciAdrese[i].Count - 1))
                        {
                            sveMogucePermutacijeReciAdreseArray[i] += " ";
                        }
                    }
                }

                session.Execute(upit.Bind(
                    maxStanIdInt,
                    maxUsernameToStanoviIdInt,
                    maxDatumiToStanoviIdInt,
                    callerUsername,
                    oglas.adresa,
                    sveMogucePermutacijeReciAdreseArray,
                    oglas.wifi,
                    oglas.tus,
                    oglas.parking_mesto,
                    oglas.tv,
                    localDateArray,
                    oglas.opis,
                    oglas.slika
                    ));

                oglas.dodajOglasPozvato = false;
                oglas.slikaPostavljena = false;

                // Dodaj maping na ovaj stan u UsernameToStanovi tabeli
                session.Execute("INSERT INTO \"UsernameToStanovi\" (id, stan_id, username) VALUES (" + maxUsernameToStanoviIdInt + ", " + maxStanIdInt + ", '" + callerUsername + "')");
                // Dodaj maping na ovaj stan u DatumiToStanovi tabeli
                var upitDatumi = session.Prepare("INSERT INTO \"DatumiToStanovi\" (id, stan_id, earliest_date, farthest_date) VALUES (?, ?, ?, ?)");
                session.Execute(upitDatumi.Bind(
                    maxStanIdInt,
                    maxDatumiToStanoviIdInt,
                    oglas.earliest_date,
                    oglas.farthest_date
                    ));

                // Izbrisati asocijacije stavke iz recnika
                List<byte[]> garbageList;
                Oglas garbageOglas;
                connectedUsersConnectionIDToImageBlockList.TryRemove(Context.ConnectionId, out garbageList);
                connectedUsersConnectionIDToOglas.TryRemove(Context.ConnectionId, out garbageOglas);

                // Obavesti klijenta da je oglas uspesno postavljen
                Clients.Caller.OglasUspednoPostavljen("nista");
            }
        }

        public void ClientSaljeBlockSlike(string blockIndex, string maxBrojBlokova, byte[] blok)
        {
            int blockIndexInt;
            Int32.TryParse(blockIndex, out blockIndexInt);
            int maxBrojBlokovaInt;
            Int32.TryParse(maxBrojBlokova, out maxBrojBlokovaInt);

            List<byte[]> blokovi;
            connectedUsersConnectionIDToImageBlockList.TryGetValue(Context.ConnectionId, out blokovi);
            if (blokovi == null)
            {
                blokovi = new List<byte[]>(maxBrojBlokovaInt);
                connectedUsersConnectionIDToImageBlockList.TryAdd(Context.ConnectionId, blokovi);
            }

            //blokovi.Insert(blockIndexInt, Encoding.ASCII.GetBytes(blok));
            blokovi.Insert(blockIndexInt, blok);

            // Ako smo upravo primili poslednji blok
            if (blokovi.Count == maxBrojBlokovaInt)
            {
                // Strpati sve u jedan byte array
                int imageByteSize = 0;
                for (int i = 0; i < blokovi.Count; i++)
                {
                    imageByteSize += blokovi[i].Length;
                }

                byte[] image = new byte[imageByteSize];
                int offset = 0;
                for (int i = 0; i < blokovi.Count; i++)
                {
                    System.Buffer.BlockCopy(blokovi[i], 0, image, offset, blokovi[i].Length);
                    offset += blokovi[i].Length;
                }

                // Sada treba upisati sliku u oglas i ako je oglas vec popunjen onda ga poslati
                Oglas oglas;
                connectedUsersConnectionIDToOglas.TryGetValue(Context.ConnectionId, out oglas);
                if (oglas == null)
                {
                    oglas = new Oglas();
                    oglas.datumi = new List<LocalDate>();
                    oglas.dodajOglasPozvato = false;
                    oglas.slikaPostavljena = true;
                    connectedUsersConnectionIDToOglas.TryAdd(Context.ConnectionId, oglas);
                }

                oglas.slika = image;
                /*
                if (oglas.dodajOglasPozvato == true) // znaci da se ovaj metod pozvao nakon slanja slike
                {
                    // Nabavi id stana
                    Row maxStanId = session.Execute("SELECT MAX(stan_id) FROM \"Stanovi\"").FirstOrDefault();
                    int maxStanIdInt = (int)maxStanId["system.max(stan_id)"];
                    maxStanIdInt++;

                    var upit = session.Prepare("insert into \"Stanovi\" " +
                        "(stan_id, username, adresa, wifi, tus, parking_mesto, tv, datumi, opis, slika) " +
                        "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

                    LocalDate[] localDateArray = new LocalDate[oglas.datumi.Count];
                    for (int i = 0; i < oglas.datumi.Count; i++)
                    {
                        localDateArray[i] = oglas.datumi[i];
                    }

                    string callerUsername;
                    connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);

                    session.Execute(upit.Bind(
                        maxStanIdInt,
                        callerUsername,
                        oglas.adresa,
                        oglas.wifi,
                        oglas.tus,
                        oglas.parking_mesto,
                        oglas.tv,
                        localDateArray,
                        oglas.opis,
                        oglas.slika
                        ));

                    oglas.dodajOglasPozvato = false;
                    oglas.slikaPostavljena = false;

                    Clients.Caller.OglasUspednoPostavljen("nista");
                }
                */

                //string saveImagePath = "D:\\Files\\slika.png";
                //File.WriteAllBytes(saveImagePath, image);
            }
        }

        public void PribaviMojeOglase(string nista)
        {
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);

            // Nabavi sve stan_id-ove koje pripadaju ovom korisniku
            RowSet stanIds = session.Execute("SELECT stan_id FROM \"UsernameToStanovi\" where username = '" + callerUsername + "'");
            if (stanIds == null) // ako korisnik nema nijedan postavljen stan
            {
                return;
            }

            List<int> stanIdsList = new List<int>();
            foreach (Row stanId in stanIds)
            {
                stanIdsList.Add((int)stanId["stan_id"]);
            }

            if (stanIdsList.Count == 0)
            {
                return;
            }

            string stanIdsString = "(";
            for(int i = 0; i < stanIdsList.Count; i++)
            {
                stanIdsString += stanIdsList[i].ToString();
                if (i != (stanIdsList.Count - 1))
                {
                    stanIdsString += ", ";
                }
                else
                {
                    stanIdsString += ")";
                }
            }

            // Sada nabaviti sve stanove sa stan_ids koje smo pribavili
            RowSet oglasi = session.Execute("SELECT * FROM \"Stanovi\" WHERE id IN " + stanIdsString);
            // Sve pretvori u string i vrati klijentu
            string oglasiString = "";
            int counter = 0;
            foreach (Row oglas in oglasi)
            {
                oglasiString += counter.ToString();
                oglasiString += "@";
                oglasiString += oglas["adresa"].ToString();
                oglasiString += "@";
                oglasiString += oglas["opis"].ToString();
                oglasiString += "@";
                oglasiString += oglas["wifi"].ToString();
                oglasiString += "@";
                oglasiString += oglas["tus"].ToString();
                oglasiString += "@";
                oglasiString += oglas["parking_mesto"].ToString();
                oglasiString += "@";
                oglasiString += oglas["tv"].ToString();
                oglasiString += "@";
                LocalDate[] datumi = (LocalDate[])oglas["datumi"];
                bool temp = false;
                foreach (LocalDate datum in datumi)
                {
                    oglasiString += datum.Year.ToString();
                    oglasiString += "-";
                    oglasiString += datum.Month.ToString();
                    oglasiString += "-";
                    oglasiString += datum.Day.ToString();
                    if (temp == false)
                    {
                        oglasiString += "\r\n | \r\n";
                        temp = true;
                    }
                    else
                    {
                        oglasiString += "\r\n";
                        temp = false;
                    }
                }

                oglasiString += "@";
                oglasiString += ((int)oglas["id"]).ToString();
                // sada treba proveriti da li ovaj stan ima zahteve, ako ima onda ih treba pribaviti
                // Nabavi sve zahteve za ovaj oglas
                int[] zahteviArray = (int[])oglas["zahtevi"];
                if (zahteviArray != null)
                {
                    List<int> zahteviIdsList = zahteviArray.ToList();
                    if (zahteviIdsList.Count != 0)
                    {
                        string zahteviIdsString = "(";
                        for (int i = 0; i < zahteviIdsList.Count; i++)
                        {
                            zahteviIdsString += zahteviIdsList[i].ToString();
                            if (i != (zahteviIdsList.Count - 1))
                            {
                                zahteviIdsString += ", ";
                            }
                        }
                        zahteviIdsString += ")";
                        RowSet zahtevi = session.Execute("SELECT * FROM \"Zahtevi\" WHERE id IN " + zahteviIdsString);
                        if (zahtevi != null)
                        {
                            bool doOnce = true;
                            foreach (Row zahtev in zahtevi)
                            {
                                if (zahtev != null)
                                {
                                    if (doOnce == true)
                                    {
                                        oglasiString += "@";
                                        doOnce = false;
                                    }

                                    oglasiString += ((int)zahtev["id"]).ToString();
                                    oglasiString += "#";
                                    oglasiString += ((int)zahtev["status"]).ToString();
                                    oglasiString += "#";
                                    LocalDate[] zahtevDatumiArray = (LocalDate[])zahtev["datumi"];
                                    for (int j = 0; j < zahtevDatumiArray.Length; j++)
                                    {
                                        oglasiString += zahtevDatumiArray[j].Year.ToString();
                                        oglasiString += "-";
                                        oglasiString += zahtevDatumiArray[j].Month.ToString();
                                        oglasiString += "-";
                                        oglasiString += zahtevDatumiArray[j].Day.ToString();

                                        if (j != (zahtevDatumiArray.Length - 1))
                                        {
                                            oglasiString += " ";
                                        }
                                    }

                                    oglasiString += "$";
                                }
                            }

                            oglasiString = oglasiString.Remove(oglasiString.Length - 1); // da se skloni poslednji $ simbol
                        }
                    }
                }
                

                oglasiString += "%";

                // Slanje slike korisniku u blokovima od 30KB
                byte[] slika = ((byte[])oglas["slika"]);
                int numberOfIterations = slika.Length / 30720;
                int maxNumberOfBlocks = numberOfIterations;
                if (slika.Length % 30720 != 0)
                {
                    maxNumberOfBlocks++;
                }

                int offset = 0;
                for (int j = 0; j < numberOfIterations; j++)
                {
                    byte[] block = new byte[30720];
                    System.Buffer.BlockCopy(slika, offset, block, 0, 30720);
                    Clients.Caller.PribaviMojeOglaseSlikaOdgovor(counter, offset, slika.Length, false, block);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    Clients.Caller.PribaviMojeOglaseSlikaOdgovor(counter, offset, slika.Length, true, lastBlock);
                }

                counter++;
            }

            // Vrati informacije o oglasima klijentu bez slika,
            // Slike cemo naknadno poslati preko blokova
            Clients.Caller.PribaviMojeOglaseOdgovor(oglasiString);
        }
        
        
        public void PretraziOglase(string adresa,
            int datumOdGodina,
            int datumOdMesec,
            int datumOdDan,
            int datumDoGodina,
            int datumDoMesec,
            int datumDoDan)
        {
            RowSet oglasi;
            RowSet stanIds = session.Execute("SELECT stan_id FROM \"DatumiToStanovi\" WHERE earliest_date <= '" + datumOdGodina.ToString() + "-" + datumOdMesec.ToString() + "-" + datumOdDan.ToString() + "' AND farthest_date >= '" + datumDoGodina.ToString() + "-" + datumDoMesec.ToString() + "-" + datumDoDan.ToString() + "' ALLOW FILTERING");
            if (stanIds == null)
            {
                return;
            }

            List<int> stanIdsList = new List<int>();
            foreach (Row stanId in stanIds)
            {
                stanIdsList.Add((int)stanId["stan_id"]);
            }

            if (adresa != "") // pretraga i po adresi
            {
                stanIds = session.Execute("SELECT id FROM \"Stanovi\" WHERE adresa_terms CONTAINS '" + adresa + "'");
                if (stanIds != null)
                {
                    foreach (Row stanId in stanIds)
                    {
                        stanIdsList.Add((int)stanId["id"]);
                    }
                }
            }

            // Odvojiti samo unikatne stan_id-ove
            stanIdsList = stanIdsList.Distinct().ToList();

            string stanIdsString = "(";
            for (int i = 0; i < stanIdsList.Count; i++)
            {
                stanIdsString += stanIdsList[i].ToString();
                if (i != (stanIdsList.Count - 1))
                {
                    stanIdsString += ", ";
                }

            }

            stanIdsString += ")";
            oglasi = session.Execute("SELECT * FROM \"Stanovi\" WHERE id IN " + stanIdsString);       

            // Sada mozemo da vratimo sve oglse klijentu
            string oglasiString = "";
            int counter = 0;
            foreach (Row oglas in oglasi)
            {
                oglasiString += counter.ToString();
                oglasiString += "@";
                oglasiString += oglas["adresa"].ToString();
                oglasiString += "@";
                oglasiString += oglas["opis"].ToString();
                oglasiString += "@";
                oglasiString += oglas["wifi"].ToString();
                oglasiString += "@";
                oglasiString += oglas["tus"].ToString();
                oglasiString += "@";
                oglasiString += oglas["parking_mesto"].ToString();
                oglasiString += "@";
                oglasiString += oglas["tv"].ToString();
                oglasiString += "@";
                LocalDate[] datumi = (LocalDate[])oglas["datumi"];
                bool temp = false;
                foreach (LocalDate datum in datumi)
                {
                    oglasiString += datum.Year.ToString();
                    oglasiString += "-";
                    oglasiString += datum.Month.ToString();
                    oglasiString += "-";
                    oglasiString += datum.Day.ToString();
                    if (temp == false)
                    {
                        oglasiString += "\n | \n";
                        temp = true;
                    }
                    else
                    {
                        oglasiString += "\n";
                        temp = false;
                    }
                }

                oglasiString += "@";
                oglasiString += ((int)oglas["id"]).ToString();

                oglasiString += "%";

                byte[] slika = ((byte[])oglas["slika"]);
                int numberOfIterations = slika.Length / 30720;
                int maxNumberOfBlocks = numberOfIterations;
                if (slika.Length % 30720 != 0)
                {
                    maxNumberOfBlocks++;
                }

                int offset = 0;
                for (int j = 0; j < numberOfIterations; j++)
                {
                    byte[] block = new byte[30720];
                    System.Buffer.BlockCopy(slika, offset, block, 0, 30720);
                    Clients.Caller.PretraziOglaseSlikaOdgovor(counter, offset, slika.Length, false, block);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    Clients.Caller.PretraziOglaseSlikaOdgovor(counter, offset, slika.Length, true, lastBlock);
                }

                counter++;
            }

            // Vrati informacije o oglasima klijentu bez slika,
            // Slike cemo naknadno poslati preko blokova
            Clients.Caller.PretraziOglaseOdgovor(oglasiString);
        }

        public void PostaviZahtev(int stanId, string zahtevDatumi)
        {
            List<LocalDate> zahtevDatumiList = new List<LocalDate>();
            string zahtevDatumiString = "["; // koristi se za upit koji postavlja zahtev u data bazu na samom kraju ako je zahtev validan
            string[] zahtevDatumiArray = zahtevDatumi.Split('@');
            for (int i = 0; i < zahtevDatumiArray.Length; i++)
            {
                zahtevDatumiString += "'";
                zahtevDatumiString += zahtevDatumiArray[i];
                zahtevDatumiString += "'";
                if (i != (zahtevDatumiArray.Length - 1))
                {
                    zahtevDatumiString += ", ";
                }
                string[] data = zahtevDatumiArray[i].Split('-');
                int godina;
                int mesec;
                int dan;
                Int32.TryParse(data[0], out godina);
                Int32.TryParse(data[1], out mesec);
                Int32.TryParse(data[2], out dan);
                zahtevDatumiList.Add(new LocalDate(godina, mesec, dan));
            }

            zahtevDatumiString += "]";

            // Sada pribavi sve datume od ovog stana
            Row datumiStan = session.Execute("SELECT datumi FROM \"Stanovi\" WHERE id = " + stanId).FirstOrDefault();
            List<LocalDate> datumiStanList = ((LocalDate[])datumiStan["datumi"]).ToList();

            // Sada treba uporediti da li se datumi zahteva uklapaju u datume kada je stan raspoloziv
            List<int> uKojeSveParoveDatumaUpadaZahtevDatumOd = new List<int>();
            for (int i = 0; i < zahtevDatumiList.Count; i += 2)
            {
                uKojeSveParoveDatumaUpadaZahtevDatumOd.Clear();
                for (int j = 0; j < datumiStanList.Count; j += 2)
                {
                    if (zahtevDatumiList[i] >= datumiStanList[j])
                    {
                        uKojeSveParoveDatumaUpadaZahtevDatumOd.Add(j);
                    }
                }

                if (uKojeSveParoveDatumaUpadaZahtevDatumOd.Count == 0)
                {
                    Clients.Caller.PostaviZahtevOdgovor(false);
                    return;
                }

                bool postojiValidanPar = false;
                for (int j = 0; j < uKojeSveParoveDatumaUpadaZahtevDatumOd.Count; j++)
                {
                    if (zahtevDatumiList[i + 1] <= datumiStanList[uKojeSveParoveDatumaUpadaZahtevDatumOd[j] + 1])
                    {
                        postojiValidanPar = true;
                        bool zahtevOdSePoklapa = false;
                        if (zahtevDatumiList[i] == datumiStanList[uKojeSveParoveDatumaUpadaZahtevDatumOd[j]])
                        {
                            zahtevOdSePoklapa = true;
                        }

                        bool zahtevDoSePoklapa = false;
                        if (zahtevDatumiList[i + 1] == datumiStanList[uKojeSveParoveDatumaUpadaZahtevDatumOd[j] + 1])
                        {
                            zahtevDoSePoklapa = true;
                        }

                        if ((zahtevOdSePoklapa == true) && (zahtevDoSePoklapa == true))
                        {
                            datumiStanList.RemoveAt(uKojeSveParoveDatumaUpadaZahtevDatumOd[j]);
                            datumiStanList.RemoveAt(uKojeSveParoveDatumaUpadaZahtevDatumOd[j]);
                        }

                        if ((zahtevOdSePoklapa == true) && (zahtevDoSePoklapa == false))
                        {
                            datumiStanList[uKojeSveParoveDatumaUpadaZahtevDatumOd[j]] = zahtevDatumiList[i + 1];
                        }

                        if ((zahtevOdSePoklapa == false) && (zahtevDoSePoklapa == true))
                        {
                            datumiStanList[uKojeSveParoveDatumaUpadaZahtevDatumOd[j] + 1] = zahtevDatumiList[i + 1];
                        }

                        if ((zahtevOdSePoklapa == false) && (zahtevDoSePoklapa == false))
                        {
                            datumiStanList.Insert(uKojeSveParoveDatumaUpadaZahtevDatumOd[j] + 1, zahtevDatumiList[i]);
                            datumiStanList.Insert(uKojeSveParoveDatumaUpadaZahtevDatumOd[j] + 2, zahtevDatumiList[i + 1]);
                        }

                        break;
                    }
                }

                if (postojiValidanPar == false)
                {
                    Clients.Caller.PostaviZahtevOdgovor(false);
                    return;
                }
            }

            // Sada mozemo napraviti novi zahtev
            // prvo naci maxId za zahtev
            Row maxZahtevId = session.Execute("SELECT MAX(id) FROM \"Zahtevi\"").FirstOrDefault();
            int maxZahtevIdInt = 0;
            if (maxZahtevId == null)
            {
                maxZahtevIdInt = 0;
            }
            else
            {
                if (maxZahtevId["system.max(id)"] == null)
                {
                    maxZahtevIdInt = 0;
                }
                else
                {
                    maxZahtevIdInt = (int)maxZahtevId["system.max(id)"];
                    maxZahtevIdInt++;
                }
            }

            session.Execute("INSERT INTO \"Zahtevi\" (id, stan_id, status, datumi) VALUES (" + maxZahtevIdInt.ToString() + ", " + stanId.ToString() + ", 0, " + zahtevDatumiString + ")");
            // Ubaciti referencu u stan za koji je zahtev namenjen
            session.Execute("UPDATE \"Stanovi\" SET zahtevi = [" + maxZahtevIdInt + "] + zahtevi WHERE id = " + stanId);
            // Ubaciti referencu u nalog koji je napravio zahtev
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            string callerPassword;
            connectedUsersConnectionIDToPassword.TryGetValue(Context.ConnectionId, out callerPassword);
            session.Execute("UPDATE \"Accounts\" SET zahtevi = [" + maxZahtevIdInt + "] + zahtevi WHERE username = '" + callerUsername + "' AND \"password\" = '" + callerPassword + "'");

            Clients.Caller.PostaviZahtevOdgovor(true);
        }

        public void ObrisiStan(int stanId)
        {
            Row upit = session.Execute("SELECT datumi_to_stan_id, username_to_stan_id, zahtevi FROM \"Stanovi\" WHERE id = " + stanId.ToString()).FirstOrDefault();
            if (upit != null)
            {
                // Prvo cemo postaviti status svih zahteva za ovaj stan na 3, sto signalizira da je oglas stana izbrisan
                List<int> zahteviIdsList = ((int[])upit["zahtevi"]).ToList();
                if (zahteviIdsList.Count != 0)
                {
                    string zahteviIdsString = "(";
                    for (int i = 0; i < zahteviIdsList.Count; i++)
                    {
                        zahteviIdsString += zahteviIdsList[i].ToString();
                        if (i != (zahteviIdsList.Count - 1))
                        {
                            zahteviIdsString += ", ";
                        }
                    }

                    zahteviIdsString += ")";
                    session.Execute("UPDATE \"Zahtevi\" SET status = 2 WHERE id IN " + zahteviIdsString);
                }

                // Sada cemo izbrisati vrstu iz UsernameToStanovi tabele
                session.Execute("DELETE FROM \"UsernameToStanovi\" WHERE id = " + ((int)upit["username_to_stan_id"]).ToString());

                // Sada cemo izbrisati vrstu iz DatumiToStanovi tabele
                session.Execute("DELETE FROM \"DatumiToStanovi\" WHERE id = " + ((int)upit["datumi_to_stan_id"]).ToString());
            }

            Clients.Caller.OglasUspesnoObrisan("nista");
        }

        // Kada klijent izmeni zahteve svojih oglasa onda se poziva ova funkcija
        public void PromenaZahtevaStanova(string data)
        {
            List<string> oglasi = data.Split('@').ToList();
            for (int i = 0; i < oglasi.Count; i++)
            {
                List<string> oglasData = oglasi[i].Split('#').ToList();
                string zahteviIdsString = "[";
                if (oglasData.Count == 3) // ako postoje zahtevi
                {
                    List<string> zahteviList = oglasData[2].Split('%').ToList();
                    for (int j = 0; j < zahteviList.Count; j++)
                    {
                        string[] zahtevData = zahteviList[j].Split(' ');
                        zahteviIdsString += zahtevData[0];
                        if (j != (zahteviList.Count - 1))
                        {
                            zahteviIdsString += ", ";
                        }

                        // Postavi novi status za ovaj oglas
                        session.Execute("UPDATE \"Zahtevi\" SET status = " + zahtevData[1] + " WHERE id = " + zahtevData[0]);
                    }
                }

                zahteviIdsString += "]";
                session.Execute("UPDATE \"Stanovi\" SET zahtevi = " + zahteviIdsString + ", datumi = " + oglasData[1] + " WHERE id = " + oglasData[0]);
            }

            Clients.Caller.PromenaZahtevaStanovaOdgovor("nista");
        }

        public void ObrisiZahtev(int stanId, int zahtevId)
        {
            // Prvo moramo izbrisati referencu na zahtev u stanu
            Row zahteviStana = session.Execute("SELECT zahtevi FROM \"Stanovi\" WHERE id = " + stanId).FirstOrDefault();
            if (zahteviStana != null)
            {
                int[] zahteviIdsArray = (int[])zahteviStana["zahtevi"];
                if (zahteviIdsArray != null)
                {
                    List<int> zahteviIdsList = zahteviIdsArray.ToList();
                    for (int i = 0; i < zahteviIdsList.Count; i++)
                    {
                        if (zahteviIdsList[i] == zahtevId)
                        {
                            zahteviIdsList.RemoveAt(i);
                            break;
                        }
                    }

                    string zahteviIdsString = "[";
                    for (int i = 0; i < zahteviIdsList.Count; i++)
                    {
                        zahteviIdsString += zahteviIdsList[i].ToString();
                        if (i != (zahteviIdsList.Count - 1))
                        {
                            zahteviIdsString += ", ";
                        }
                    }

                    zahteviIdsString += "]";

                    session.Execute("UPDATE \"Stanovi\" SET zahtevi = " + zahteviIdsString + " WHERE id = " + stanId);
                }
            }

            // Sada treba izbrisati referencu na ovaj zahtev iz tabele naloga 
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            string callerPassword;
            connectedUsersConnectionIDToPassword.TryGetValue(Context.ConnectionId, out callerPassword);
            Row zahteviNaloga = session.Execute("SELECT zahtevi FROM \"Accounts\" WHERE username = '" + callerUsername + "' AND \"password\" = '" + callerPassword + "'").FirstOrDefault();
            if (zahteviNaloga != null)
            {
                int[] zahteviIdsArray = (int[])zahteviNaloga["zahtevi"];
                if (zahteviIdsArray != null)
                {
                    List<int> zahteviIdsList = zahteviIdsArray.ToList();
                    for (int i = 0; i < zahteviIdsList.Count; i++)
                    {
                        if (zahteviIdsList[i] == zahtevId)
                        {
                            zahteviIdsList.RemoveAt(i);
                            break;
                        }
                    }

                    string zahteviIdsString = "[";
                    for (int i = 0; i < zahteviIdsList.Count; i++)
                    {
                        zahteviIdsString += zahteviIdsList[i].ToString();
                        if (i != (zahteviIdsList.Count - 1))
                        {
                            zahteviIdsString += ", ";
                        }
                    }

                    zahteviIdsString += "]";

                    session.Execute("UPDATE \"Accounts\" SET zahtevi = " + zahteviIdsString + " WHERE username = '" + callerUsername + "' AND \"password\" = '" + callerPassword + "'");
                }
            }


            // I sada mozemo izbrisati i sam zahtev iz tabele zahteva
            session.Execute("DELETE FROM \"Zahtevi\" WHERE id = " + zahtevId);

            Clients.Caller.ZahtevUspesnoObrisan("nista");
        }

        public void PribaviMojeZahteve(string nista)
        {
            string callerUsername;
            connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
            string callerPassword;
            connectedUsersConnectionIDToPassword.TryGetValue(Context.ConnectionId, out callerPassword);

            // Nabavi sve stan_id-ove koje pripadaju ovom korisniku
            Row zahteviIdsUpit = session.Execute("SELECT zahtevi FROM \"Accounts\" where username = '" + callerUsername + "' AND \"password\" = '" + callerPassword + "'").FirstOrDefault();
            if (zahteviIdsUpit == null) // ako korisnik nema nijedan postavljen stan
            {
                return;
            }

            int[] zahteviIdsArray = (int[])zahteviIdsUpit["zahtevi"];
            if (zahteviIdsArray == null)
            {
                return;
            }
            
            List<int> zahteviIdsList = zahteviIdsArray.ToList();
            if (zahteviIdsList.Count == 0)
            {
                return;
            }

            
            string zahteviIdsString = "(";
            for (int i = 0; i < zahteviIdsList.Count; i++)
            {
                zahteviIdsString += zahteviIdsList[i].ToString();
                if (i != (zahteviIdsList.Count - 1))
                {
                    zahteviIdsString += ", ";
                }
                else
                {
                    zahteviIdsString += ")";
                }
            }

            // Sada nabaviti sve ids od stanove za koje su namenjeni ovi zahtevi
            RowSet zahteviUpit = session.Execute("SELECT * FROM \"Zahtevi\" WHERE id IN " + zahteviIdsString);
            if (zahteviUpit == null)
            {
                return;
            }

            List<Zahtev> zahteviList = new List<Zahtev>();
            string stanIdsString = "(";
            foreach (Row zahtev in zahteviUpit)
            {
                if (zahtev != null)
                {
                    Zahtev noviZahtev = new Zahtev();
                    noviZahtev.id = ((int)zahtev["id"]).ToString();
                    noviZahtev.stan_id = ((int)zahtev["stan_id"]).ToString();
                    noviZahtev.status = ((int)zahtev["status"]).ToString();
                    // pretvoriti localdates u string
                    LocalDate[] zahtevDatumiArray = (LocalDate[])zahtev["datumi"];
                    for (int j = 0; j < zahtevDatumiArray.Length; j++)
                    {
                        noviZahtev.datumi += zahtevDatumiArray[j].Year.ToString();
                        noviZahtev.datumi += "-";
                        noviZahtev.datumi += zahtevDatumiArray[j].Month.ToString();
                        noviZahtev.datumi += "-";
                        noviZahtev.datumi += zahtevDatumiArray[j].Day.ToString();

                        if (j != (zahtevDatumiArray.Length - 1))
                        {
                            noviZahtev.datumi += " ";
                        }
                    }

                    zahteviList.Add(noviZahtev);

                    // dodaj u stanIdsString
                    stanIdsString += noviZahtev.stan_id;
                    stanIdsString += ", ";
                }
            }

            stanIdsString = stanIdsString.Remove(stanIdsString.Length - 2, 2);
            stanIdsString += ")";

            // Sada nabaviti sve stanove sa stan_ids koje smo pribavili
            RowSet oglasi = session.Execute("SELECT * FROM \"Stanovi\" WHERE id IN " + stanIdsString);
            List<Oglas> oglasiList = new List<Oglas>();
            foreach (Row oglas in oglasi)
            {
                Oglas noviOglas = new Oglas();
                noviOglas.id = (int)oglas["id"];
                noviOglas.adresa = oglas["adresa"].ToString();
                noviOglas.opis = oglas["opis"].ToString();
                noviOglas.wifi = (bool)oglas["wifi"];
                noviOglas.tus = (bool)oglas["tus"];
                noviOglas.parking_mesto = (bool)oglas["parking_mesto"];
                noviOglas.tv = (bool)oglas["tv"];
                noviOglas.datumi = ((LocalDate[])oglas["datumi"]).ToList();
                noviOglas.slika = (byte[])oglas["slika"];
                // Pridruziti ovom oglasu sve zahteve koje klijent ima za njega
                noviOglas.zahtevi = new List<Zahtev>();
                for (int i = 0; i < zahteviList.Count; i++)
                {
                    int zahtevStanId;
                    Int32.TryParse(zahteviList[i].stan_id, out zahtevStanId);
                    if (zahtevStanId == noviOglas.id)
                    {
                        noviOglas.zahtevi.Add(zahteviList[i]);
                    }
                }

                oglasiList.Add(noviOglas);
            }

            // Sve pretvori u string i vrati klijentu
            string resultString = "";
            for(int i = 0; i < oglasiList.Count; i++)
            {
                resultString += i.ToString();
                resultString += "@";
                resultString += oglasiList[i].adresa;
                resultString += "@";
                resultString += oglasiList[i].opis;
                resultString += "@";
                resultString += oglasiList[i].wifi.ToString();
                resultString += "@";
                resultString += oglasiList[i].tus.ToString();
                resultString += "@";
                resultString += oglasiList[i].parking_mesto.ToString();
                resultString += "@";
                resultString += oglasiList[i].tv.ToString();
                resultString += "@";
                bool temp = false;
                foreach (LocalDate datum in oglasiList[i].datumi)
                {
                    resultString += datum.Year.ToString();
                    resultString += "-";
                    resultString += datum.Month.ToString();
                    resultString += "-";
                    resultString += datum.Day.ToString();
                    if (temp == false)
                    {
                        resultString += "\r\n | \r\n";
                        temp = true;
                    }
                    else
                    {
                        resultString += "\r\n";
                        temp = false;
                    }
                }

                resultString += "@";
                resultString += oglasiList[i].id.ToString();
                // Sada dodati zahteve koje imamo ka ovim stanovima
                if (oglasiList[i].zahtevi.Count > 0)
                {
                    resultString += "@";
                }

                for (int j = 0; j < oglasiList[i].zahtevi.Count; j++)
                {
                    resultString += oglasiList[i].zahtevi[j].id;
                    resultString += "#";
                    resultString += oglasiList[i].zahtevi[j].status;
                    resultString += "#";
                    resultString += oglasiList[i].zahtevi[j].datumi;
                    if (j != (oglasiList[i].zahtevi.Count - 1))
                    {
                        resultString += "$";
                    }
                }

                resultString += "%"; // odvaja oglase

                // Slanje slike korisniku u blokovima od 30KB
                byte[] slika = oglasiList[i].slika;
                int numberOfIterations = slika.Length / 30720;
                int maxNumberOfBlocks = numberOfIterations;
                if (slika.Length % 30720 != 0)
                {
                    maxNumberOfBlocks++;
                }

                int offset = 0;
                for (int j = 0; j < numberOfIterations; j++)
                {
                    byte[] block = new byte[30720];
                    System.Buffer.BlockCopy(slika, offset, block, 0, 30720);
                    Clients.Caller.PribaviMojeZahteveSlikaOdgovor(i, offset, slika.Length, false, block);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    Clients.Caller.PribaviMojeZahteveSlikaOdgovor(i, offset, slika.Length, true, lastBlock);
                }
            }

            // Vrati informacije o oglasima klijentu bez slika,
            // Slike cemo naknadno poslati preko blokova
            Clients.Caller.PribaviMojeZahteveOdgovor(resultString);
        }
    }
}

