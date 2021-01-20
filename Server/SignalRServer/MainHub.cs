﻿using System;
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
using System.Drawing;
using System.Reflection;

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
            public bool dodajOglasPozvano;
            public bool slikaPostavljena;
        }

        public class BlokSlike
        {
            public BlokSlike() { }
            public int oglasIndex;
            public int offset;
            public byte[] blok;
            public int velicinaCeleSlike;
            public bool jestePoslednji;
        }

        public class StaTrebaPoslatiKlijentu
        {
            
            public StaTrebaPoslatiKlijentu() { }
            public string oglasiData;
            public List<List<BlokSlike>> slikeUDeloviva; // za svaku sliku po jedna lista byte arrays
            public int kojuSlikuSaljemo;
            public int kojiDeoSlike;
        }

        // imamo dva recnika jedan u kome je connectionID ismedju servera i klijenta kljuc koji vodi ka account username
        // i reverse recnik od toga, gde account username vodi ka connectionID.
        public static ConcurrentDictionary<string, string> connectedUsersConnectionIDToUsername = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> connectedUsersUsernameToConnectionID = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> connectedUsersConnectionIDToPassword = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, List<byte[]>> connectedUsersConnectionIDToImageBlockList = new ConcurrentDictionary<string, List<byte[]>>();
        public static ConcurrentDictionary<string, Oglas> connectedUsersConnectionIDToOglas = new ConcurrentDictionary<string, Oglas>();
        public static ConcurrentDictionary<string, StaTrebaPoslatiKlijentu> connectedUserConnectionIdToStaTrebaPoslatiKlijentu = new ConcurrentDictionary<string, StaTrebaPoslatiKlijentu>();

        // Radi isto sto i PostaviOglasi, samo sto je ta funkcija namenjena za klijente dok je ovo verzija te iste funkcije koja je namenjena za server
        // Server je koristi kako bi hardkodovano ucitao sample vrednosti u databazu kada je ona inicialno prazna radi lakseg ocenjivanja
        private void PostaviOglasServerVerzija(string username, string adresa, bool wifi, bool tus, bool parking_mesto, bool tv, string opis, LocalDate[] datumi, string slikaFilename)
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


            var upit = session.Prepare("insert into \"Stanovi\" " +
                     "(id, username_to_stan_id, username, adresa, adresa_terms, wifi, tus, parking_mesto, tv, datumi, earliest_date, farthest_date, opis, slika, zahtevi, obrisan) " +
                     "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");


            // Odredi najraniji i najkasniji datum
            LocalDate earliestDate = datumi[0];
            LocalDate farthestDate = datumi[0];
            for (int i = 0; i < datumi.Length; i++)
            {
                if (datumi[i] < earliestDate)
                {
                    earliestDate = datumi[i];
                }

                if (datumi[i] > farthestDate)
                {
                    farthestDate = datumi[i];
                }
            }

            // napravi listu svih mogucih kombinacija reci za adresa_terms radi robusnije pretrage
            List<string> listaReciAdrese = (adresa.ToLower()).Split(' ').ToList();
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

            // Pretvaranje slike u byte array
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), slikaFilename);
            Bitmap myBitmap = new Bitmap(path);
            byte[] imageByteArray;
            using (var memoryStream = new MemoryStream())
            {
                myBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                imageByteArray = memoryStream.ToArray();
            }

            int[] emptyIntArray = { };
            session.Execute(upit.Bind(
                    maxStanIdInt,
                    maxUsernameToStanoviIdInt,
                    username,
                    adresa,
                    sveMogucePermutacijeReciAdreseArray,
                    wifi,
                    tus,
                    parking_mesto,
                    tv,
                    datumi,
                    earliestDate,
                    farthestDate,
                    opis,
                    imageByteArray,
                    emptyIntArray,
                    false
                    ));


                // Dodaj maping na ovaj stan u UsernameToStanovi tabeli
                session.Execute("INSERT INTO \"UsernameToStanovi\" (id, stan_id, username) VALUES (" + maxUsernameToStanoviIdInt + ", " + maxStanIdInt + ", '" + username + "')");
        }

        // Ova verzija funkcije ne proverava da li se datumi u klapaju u dostupnost stana
        // samo postavlja zahtev, sto je ok jer ce mo ovu funkciju koristiti kako bi iz hardkodovano ubacili neke
        // primere u databazu radi lakseg ocenjivanja
        private void PostaviZahtevServerVerzija(int stanId, string zahtevDatumi, string username, string password)
        {
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

            session.Execute("INSERT INTO \"Zahtevi\" (id, stan_id, status, datumi) VALUES (" + maxZahtevIdInt.ToString() + ", " + stanId.ToString() + ", 0, " + zahtevDatumi + ")");
            // Ubaciti referencu u stan za koji je zahtev namenjen
            session.Execute("UPDATE \"Stanovi\" SET zahtevi = [" + maxZahtevIdInt + "] + zahtevi WHERE id = " + stanId);
            // Ubaciti referencu u nalog koji je napravio zahtev
            session.Execute("UPDATE \"Accounts\" SET zahtevi = [" + maxZahtevIdInt + "] + zahtevi WHERE username = '" + username + "' AND \"password\" = '" + password + "'");
        }

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

            // Provera da li je databaza prazna, ako jeste onda radi lakseg ocenjivanja ce mo je popuniti sa par naloga, stanova i zahteva
            // VEOMA BITNO, ovaj kod se nece pokrenuti sve dok se ne stvori prvi klijent, jer signalR nece da napravi instancu ove klase dotle
            RowSet numberOfAccounts = session.Execute("SELECT * FROM \"Accounts\"");
            int brojac = 0;
            if (numberOfAccounts != null)
            {
                foreach (Row account in numberOfAccounts)
                {
                    brojac++;
                    if (brojac > 0)
                    {
                        break;
                    }
                }
            }
            if ((numberOfAccounts == null) || (brojac == 0))
            {
                // Popunjavamo databazu
                // Dodavanje par naloga
                session.Execute("INSERT INTO \"Accounts\" (username, \"password\", email, zahtevi) VALUES ('pavle', 'pavle', 'klm@gmail.com', [])");
                session.Execute("INSERT INTO \"Accounts\" (username, \"password\", email, zahtevi) VALUES ('djordje', 'djordje', 'djoletovMail@gmail.com', [])");
                session.Execute("INSERT INTO \"Accounts\" (username, \"password\", email, zahtevi) VALUES ('misa', 'misa', 'imePrezime@gmail.com', [])");

                // Dodavanje stanova
                // pavle izdaje dva stana
                DateTime datum01 = DateTime.Now;
                DateTime datum02 = datum01.AddDays(4);
                DateTime datum03 = datum02.AddDays(1);
                DateTime datum04 = datum03.AddDays(7);
                LocalDate localDate01 = new LocalDate(datum01.Year, datum01.Month, datum01.Day);
                LocalDate localDate02 = new LocalDate(datum02.Year, datum02.Month, datum02.Day);
                LocalDate localDate03 = new LocalDate(datum03.Year, datum03.Month, datum03.Day);
                LocalDate localDate04 = new LocalDate(datum04.Year, datum04.Month, datum04.Day);
                LocalDate[] datumiStan01 = { localDate01, localDate02, localDate03, localDate04 };
                PostaviOglasServerVerzija("pavle", "Ucitelj Tasina 44", true, true, false, false, "Bas lepo mesto, sve je kako treba", datumiStan01, "Slika01.jpg");

                datum01 = datum01.AddDays(14);
                datum02 = datum01.AddDays(2);
                datum03 = datum02.AddDays(3);
                datum04 = datum03.AddDays(5);
                localDate01 = new LocalDate(datum01.Year, datum01.Month, datum01.Day);
                localDate02 = new LocalDate(datum02.Year, datum02.Month, datum02.Day);
                localDate03 = new LocalDate(datum03.Year, datum03.Month, datum03.Day);
                localDate04 = new LocalDate(datum04.Year, datum04.Month, datum04.Day);
                LocalDate[] datumiStan02 = { localDate01, localDate02, localDate03, localDate04 };
                PostaviOglasServerVerzija("pavle", "Ucitelj Milina 22", false, true, true, true, "Ima najnoviji flat screen TV !!!", datumiStan02, "Slika02.jpg");

                // Misa ima oglas za jedan stan
                // Identicni datumi kao pavlov drugi
                PostaviOglasServerVerzija("misa", "Dusanova 2 A", true, true, true, true, "Mozete me kontaktirati preko mog fejsbook naloga Covek123 za vise informacija", datumiStan02, "Slika03.jpg");

                // Postavicemo i par oglasa i djordje i misa traze pavlov stan
                DateTime zahtevDatum = datum03.AddDays(2);
                string zahtevDatumi = "['";
                zahtevDatumi += localDate03.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += localDate03.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += localDate03.Day.ToString();
                zahtevDatumi += "', '";
                zahtevDatumi += zahtevDatum.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Day.ToString();
                zahtevDatumi += "']";
                PostaviZahtevServerVerzija(1, zahtevDatumi, "misa", "misa");

                // i djordje trazi pavlov stan
                // ali ima komplikovaniji zahtev koji se preklapa sa misinim
                zahtevDatumi = "['";
                zahtevDatum = datum01.AddDays(1);
                zahtevDatumi += zahtevDatum.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Day.ToString();
                zahtevDatumi += "', '";
                zahtevDatum = zahtevDatum.AddDays(1);
                zahtevDatumi += zahtevDatum.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Day.ToString();
                zahtevDatumi += "', '";
                zahtevDatum = datum03.AddDays(1);
                zahtevDatumi += zahtevDatum.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Day.ToString();
                zahtevDatumi += "', '";
                zahtevDatum = zahtevDatum.AddDays(2);
                zahtevDatumi += zahtevDatum.Year.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Month.ToString();
                zahtevDatumi += "-";
                zahtevDatumi += zahtevDatum.Day.ToString();
                zahtevDatumi += "']";
                PostaviZahtevServerVerzija(1, zahtevDatumi, "djordje", "djordje");

                // Eto sada imamo nesto u data bazi
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

            List<byte[]> garbageList;
            Oglas garbageOglas;
            connectedUsersConnectionIDToUsername.TryAdd(Context.ConnectionId, username);
            connectedUsersUsernameToConnectionID.TryAdd(username, Context.ConnectionId);
            connectedUsersConnectionIDToPassword.TryAdd(Context.ConnectionId, password);
            connectedUsersConnectionIDToImageBlockList.TryRemove(Context.ConnectionId, out garbageList);
            connectedUsersConnectionIDToOglas.TryRemove(Context.ConnectionId, out garbageOglas);
            StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
            Clients.Caller.LogInSuccessful(username);
        }

        // Radi isto sto i Disconnect samo na zahtev korisnika
        public void LogOut(string nista)
        {
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
                StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
                connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);

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
                oglas.dodajOglasPozvano = true;
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

            oglas.dodajOglasPozvano = true;

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

                string callerUsername;
                connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);

                var upit = session.Prepare("insert into \"Stanovi\" " +
                     "(id, username_to_stan_id, username, adresa, adresa_terms, wifi, tus, parking_mesto, tv, datumi, earliest_date, farthest_date, opis, slika, zahtevi, obrisan) " +
                     "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

                LocalDate[] localDateArray = new LocalDate[oglas.datumi.Count];
                for (int i = 0; i < oglas.datumi.Count; i++)
                {
                    localDateArray[i] = oglas.datumi[i];
                }

                // Odredi najraniji i najkasniji datum
                LocalDate earliestDate = localDateArray[0];
                LocalDate farthestDate = localDateArray[0];
                for (int i = 0; i < localDateArray.Length; i++)
                {
                    if (localDateArray[i] < earliestDate)
                    {
                        earliestDate = localDateArray[i];
                    }

                    if (localDateArray[i] > farthestDate)
                    {
                        farthestDate = localDateArray[i];
                    }
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

                int[] emptyIntArray = { };
                session.Execute(upit.Bind(
                    maxStanIdInt,
                    maxUsernameToStanoviIdInt,
                    callerUsername,
                    oglas.adresa,
                    sveMogucePermutacijeReciAdreseArray,
                    oglas.wifi,
                    oglas.tus,
                    oglas.parking_mesto,
                    oglas.tv,
                    localDateArray,
                    earliestDate,
                    farthestDate,
                    oglas.opis,
                    oglas.slika,
                    emptyIntArray,
                    false
                    ));

                oglas.dodajOglasPozvano = false;
                oglas.slikaPostavljena = false;

                // Dodaj maping na ovaj stan u UsernameToStanovi tabeli
                session.Execute("INSERT INTO \"UsernameToStanovi\" (id, stan_id, username) VALUES (" + maxUsernameToStanoviIdInt + ", " + maxStanIdInt + ", '" + callerUsername + "')");

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
                    oglas.dodajOglasPozvano = false;
                    oglas.slikaPostavljena = true;
                    connectedUsersConnectionIDToOglas.TryAdd(Context.ConnectionId, oglas);
                }

                oglas.slika = image;
            }
        }

        public void PribaviMojeOglase(string nista)
        {
            // Da li ovaj klijent vec ima neki aktivan zahtev
            StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu != null)
            {
                return;
            }

            // Ako ovaj klijent nema na cekanju neki zahtev onda mozemo da obradimo sta je hteo
            outStaTrebaPoslatiKlijentu = new StaTrebaPoslatiKlijentu();
            outStaTrebaPoslatiKlijentu.slikeUDeloviva = new List<List<BlokSlike>>();
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryAdd(Context.ConnectionId, outStaTrebaPoslatiKlijentu);

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
                if ((bool)oglas["obrisan"] == true) // stan je obrisan ali ga jos uvek cuvamo jer nisu svi klijenti videli da su njihovi zahtevi odbijeni
                {
                    continue;
                }

                outStaTrebaPoslatiKlijentu.slikeUDeloviva.Add(new List<BlokSlike>());

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
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = block;
                    noviBlokSlike.jestePoslednji = false;
                    noviBlokSlike.oglasIndex = counter;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PribaviMojeOglaseSlikaOdgovor(counter, offset, slika.Length, false, block);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[counter].Add(noviBlokSlike);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = lastBlock;
                    noviBlokSlike.jestePoslednji = true;
                    noviBlokSlike.oglasIndex = counter;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PribaviMojeOglaseSlikaOdgovor(counter, offset, slika.Length, true, lastBlock);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[counter].Add(noviBlokSlike);
                }

                counter++;
            }


            outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo = 0;
            outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            outStaTrebaPoslatiKlijentu.oglasiData = oglasiString;

            // Sada mozemo poslati klijentu nazad prvi deo prve slike
            if (outStaTrebaPoslatiKlijentu.oglasiData == "")
            {
                // nema sta da saljemo nazad
                StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
                connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
                Clients.Caller.NistaNijeNadjeno("nista");
                return;
            }

            Clients.Caller.PribaviMojeOglaseSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PribaviMojeOglaseSlikaOdgovorPrimljen(string nista)
        {
            StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu == null)
            {
                return;
            }

            outStaTrebaPoslatiKlijentu.kojiDeoSlike++;
            if (outStaTrebaPoslatiKlijentu.kojiDeoSlike > (outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo].Count - 1))
            {
                outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo++;
                outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            }

            if (outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo > (outStaTrebaPoslatiKlijentu.slikeUDeloviva.Count - 1))
            {
                // Ako smo poslali sve delove svih slika, onda mozemo da posaljemo oglasi data
                Clients.Caller.PribaviMojeOglaseOdgovor(outStaTrebaPoslatiKlijentu.oglasiData);
            }
            else
            {
                // Ako ima jos blokova slika koje treba poslati onda ovde saljemo naredni
                Clients.Caller.PribaviMojeOglaseSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
            }
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PribaviMojeOglaseOdgovorPrimljen(string nista)
        {
            // Ako je klijent primio sve mozemo ga izbrisati iz nasih zahteva
            StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
        }





        public void PretraziOglase(string adresa,
            int datumOdGodina,
            int datumOdMesec,
            int datumOdDan,
            int datumDoGodina,
            int datumDoMesec,
            int datumDoDan)
        {
            // Da li ovaj klijent vec ima neki aktivan zahtev
            StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu != null)
            {
                return;
            }

            // Ako ovaj klijent nema na cekanju neki zahtev onda mozemo da obradimo sta je hteo
            outStaTrebaPoslatiKlijentu = new StaTrebaPoslatiKlijentu();
            outStaTrebaPoslatiKlijentu.slikeUDeloviva = new List<List<BlokSlike>>();
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryAdd(Context.ConnectionId, outStaTrebaPoslatiKlijentu);

            // Pretraga po datumu
            LocalDate datumOd = new LocalDate(datumOdGodina, datumOdMesec, datumOdDan);
            LocalDate datumDo = new LocalDate(datumDoGodina, datumDoMesec, datumDoDan);
            RowSet upitPoDatumima = session.Execute("SELECT id, earliest_date, farthest_date, obrisan FROM \"Stanovi\"");

            if (upitPoDatumima == null)
            {
                return;
            }

            List<int> stanIdsList = new List<int>();
            foreach (Row row in upitPoDatumima)
            {
                if (row != null)
                {
                    if ((bool)row["obrisan"] == true) // ako je stan obrisan ali ga cuvamo jer nisu jos svi klijenti videli da su im zahtevi odbijeni, onda preskacemo ovaj stan
                    {
                        continue; 
                    }

                    LocalDate earliestDate = (LocalDate)row["earliest_date"];
                    LocalDate farthestDate = (LocalDate)row["farthest_date"];

                    if ((datumOd > farthestDate) || (datumDo < earliestDate))
                    {
                        continue;
                    }
                    else
                    {
                        stanIdsList.Add((int)row["id"]);
                    }
                }
            }

            // pretraga i po adresi
            if (adresa != "") // ako se trazi
            {
                List<int> stanIdsListAdrese = new List<int>();
                RowSet pretragaPoAdresiUpit = session.Execute("SELECT id, obrisan FROM \"Stanovi\" WHERE adresa_terms CONTAINS '" + adresa + "'");
                if (pretragaPoAdresiUpit != null)
                {
                    foreach (Row row in pretragaPoAdresiUpit)
                    {
                        if ((bool)row["obrisan"] == true) // ako je stan obrisan ali ga cuvamo jer nisu jos svi klijenti videli da su im zahtevi odbijeni, onda preskacemo ovaj stan
                        {
                            continue; 
                        }

                        stanIdsListAdrese.Add((int)row["id"]);
                    }

                    // Izdvojitisam samo one standIds koji su u ove liste, i po datumu i po adresi
                    for (int i = 0; i < stanIdsList.Count; i++)
                    {
                        bool postojiUObeListe = false;
                        for (int j = 0; j < stanIdsListAdrese.Count; j++)
                        {
                            if (stanIdsListAdrese[j] == stanIdsList[i])
                            {
                                postojiUObeListe = true;
                                break;
                            }
                        }

                        if (postojiUObeListe == false)
                        {
                            stanIdsList.RemoveAt(i);
                            i--;
                        }
                    }

                    // Nakon ovoga ako smo radili i pretrazivanje po adresi u stanIdsList se nalaze stanIds koji su na toj adresi i upadaju u opseg datuma
                }
            }


            // Pretvoriti stanIdsList u string
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
            RowSet oglasi = session.Execute("SELECT * FROM \"Stanovi\" WHERE id IN " + stanIdsString);       

            // Sada mozemo da vratimo sve oglse klijentu
            string oglasiString = "";
            int counter = 0;
            foreach (Row oglas in oglasi)
            {
                outStaTrebaPoslatiKlijentu.slikeUDeloviva.Add(new List<BlokSlike>());

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

                // Dodati u listu slika koje treba poslati
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
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = block;
                    noviBlokSlike.jestePoslednji = false;
                    noviBlokSlike.oglasIndex = counter;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PretraziOglaseSlikaOdgovor(counter, offset, slika.Length, false, block);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[counter].Add(noviBlokSlike);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = lastBlock;
                    noviBlokSlike.jestePoslednji = true;
                    noviBlokSlike.oglasIndex = counter;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PretraziOglaseSlikaOdgovor(counter, offset, slika.Length, true, lastBlock);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[counter].Add(noviBlokSlike);
                }

                counter++;
            }

            
            outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo = 0;
            outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            outStaTrebaPoslatiKlijentu.oglasiData = oglasiString;

            // Sada mozemo poslati klijentu nazad prvi deo prve slike
            if (outStaTrebaPoslatiKlijentu.oglasiData == "")
            {
                // nema sta da saljemo nazad
                StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
                connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
                Clients.Caller.NistaNijeNadjeno("nista");
                return;
            }

            Clients.Caller.PretraziOglaseSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo, 
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PretraziOglaseSlikaOdgovorPrimljen(string nista)
        {
            StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu == null)
            {
                return;
            }

            outStaTrebaPoslatiKlijentu.kojiDeoSlike++;
            if (outStaTrebaPoslatiKlijentu.kojiDeoSlike > (outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo].Count - 1))
            {
                outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo++;
                outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            }

            if (outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo > (outStaTrebaPoslatiKlijentu.slikeUDeloviva.Count - 1))
            {
                // Ako smo poslali sve delove svih slika, onda mozemo da posaljemo oglasi data
                Clients.Caller.PretraziOglaseOdgovor(outStaTrebaPoslatiKlijentu.oglasiData);
            }
            else
            {
                // Ako ima jos blokova slika koje treba poslati onda ovde saljemo naredni
                Clients.Caller.PretraziOglaseSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
            }
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PretraziOglaseOdgovorPrimljen(string nista)
        {
            // Ako je klijent primio sve mozemo ga izbrisati iz nasih zahteva
            StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
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
            Row upit = session.Execute("SELECT username_to_stan_id, zahtevi FROM \"Stanovi\" WHERE id = " + stanId.ToString()).FirstOrDefault();
            if (upit != null)
            {
                // Prvo cemo postaviti status svih zahteva za ovaj stan na , sto signalizira da su zahtevi odbijeni
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
                string callerUsername;
                connectedUsersConnectionIDToUsername.TryGetValue(Context.ConnectionId, out callerUsername);
                session.Execute("DELETE FROM \"UsernameToStanovi\" WHERE username = '" + callerUsername + "' AND id = " + ((int)upit["username_to_stan_id"]).ToString());

                // Sada cemo izbrisati stan
                // Ako stan ima zahteve onda ne mozemo ga jos uvek izbrisati iz databaze, inace klijenti koji su poslali zahtev nece moci videti
                // da je njihov zahtev odbijen, jer prikaz zahteva (Dugme moji zahtevi) zahteva i prikaz stana za koji je zahtev napravljen
                // Ali zato mozemo oznaciti stan da nije ativan vise kako ga nebi uzimali u obzir pri pretragama
                // stan cemo izbrisati kada se poslednji zahtev ukloni (kada ga vidi korisnik)
                if (zahteviIdsList.Count != 0)
                {
                    session.Execute("UPDATE \"Stanovi\" SET obrisan = true WHERE id = " + stanId.ToString());
                }
                else
                {
                    session.Execute("DELETE FROM \"Stanovi\" WHERE id = " + stanId.ToString());
                }
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

                    // Provera da li je ovaj stan vec izbrisan od strane vlasnika, samo cekamo poslednji zahtev da se obrise
                    // kako bi uklonili ceo stan iz databaze
                    Row obrisanStatus = session.Execute("SELECT obrisan FROM \"Stanovi\" WHERE id = " + stanId.ToString()).FirstOrDefault();
                    if (obrisanStatus != null)
                    {
                        if (((bool)obrisanStatus["obrisan"] == true) && (zahteviIdsList.Count == 0))
                        {
                            // mozemo da izbrisemo ceo stan
                            session.Execute("DELETE FROM \"Stanovi\" WHERE id = " + stanId.ToString());
                        }
                        else
                        {
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

                            session.Execute("UPDATE \"Stanovi\" SET zahtevi = " + zahteviIdsString.ToString() + " WHERE id = " + stanId.ToString());
                        }
                    }
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
            // Da li ovaj klijent ima vec neki zahtev koji server obradjuje
             StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu != null)
            {
                return;
            }

            // Ako ovaj klijent nema na cekanju neki zahtev onda mozemo da obradimo sta je hteo
            outStaTrebaPoslatiKlijentu = new StaTrebaPoslatiKlijentu();
            outStaTrebaPoslatiKlijentu.slikeUDeloviva = new List<List<BlokSlike>>();
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryAdd(Context.ConnectionId, outStaTrebaPoslatiKlijentu);

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
                outStaTrebaPoslatiKlijentu.slikeUDeloviva.Add(new List<BlokSlike>());

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
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = block;
                    noviBlokSlike.jestePoslednji = false;
                    noviBlokSlike.oglasIndex = i;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PribaviMojeZahteveSlikaOdgovor(i, offset, slika.Length, false, block);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[i].Add(noviBlokSlike);
                    offset += 30720;
                }

                if (numberOfIterations != maxNumberOfBlocks)
                {
                    byte[] lastBlock = new byte[slika.Length - offset];
                    System.Buffer.BlockCopy(slika, offset, lastBlock, 0, slika.Length - offset);
                    BlokSlike noviBlokSlike = new BlokSlike();
                    noviBlokSlike.blok = lastBlock;
                    noviBlokSlike.jestePoslednji = true;
                    noviBlokSlike.oglasIndex = i;
                    noviBlokSlike.velicinaCeleSlike = slika.Length;
                    noviBlokSlike.offset = offset;
                    //Clients.Caller.PribaviMojeZahteveSlikaOdgovor(i, offset, slika.Length, true, lastBlock);
                    outStaTrebaPoslatiKlijentu.slikeUDeloviva[i].Add(noviBlokSlike);
                }
            }

            outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo = 0;
            outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            outStaTrebaPoslatiKlijentu.oglasiData = resultString;

            // Sada mozemo poslati klijentu nazad prvi deo prve slike
            if (outStaTrebaPoslatiKlijentu.oglasiData == "")
            {
                // nema sta da saljemo nazad
                StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
                connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
                Clients.Caller.NistaNijeNadjeno("nista");
                return;
            }

            Clients.Caller.PribaviMojeZahteveSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PribaviMojeZahteveSlikaOdgovorPrimljen(string nista)
        {
            StaTrebaPoslatiKlijentu outStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryGetValue(Context.ConnectionId, out outStaTrebaPoslatiKlijentu);
            if (outStaTrebaPoslatiKlijentu == null)
            {
                return;
            }

            outStaTrebaPoslatiKlijentu.kojiDeoSlike++;
            if (outStaTrebaPoslatiKlijentu.kojiDeoSlike > (outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo].Count - 1))
            {
                outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo++;
                outStaTrebaPoslatiKlijentu.kojiDeoSlike = 0;
            }

            if (outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo > (outStaTrebaPoslatiKlijentu.slikeUDeloviva.Count - 1))
            {
                // Ako smo poslali sve delove svih slika, onda mozemo da posaljemo oglasi data
                Clients.Caller.PribaviMojeZahteveOdgovor(outStaTrebaPoslatiKlijentu.oglasiData);
            }
            else
            {
                // Ako ima jos blokova slika koje treba poslati onda ovde saljemo naredni
                Clients.Caller.PribaviMojeZahteveSlikaOdgovor(outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].offset,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].velicinaCeleSlike,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].jestePoslednji,
                outStaTrebaPoslatiKlijentu.slikeUDeloviva[outStaTrebaPoslatiKlijentu.kojuSlikuSaljemo][outStaTrebaPoslatiKlijentu.kojiDeoSlike].blok);
            }
        }

        // Pozivom ove funkcije nam klijent odgovara da je primio poruku
        public void PribaviMojeZahteveOdgovorPrimljen(string nista)
        {
            // Ako je klijent primio sve mozemo ga izbrisati iz nasih zahteva
            StaTrebaPoslatiKlijentu garbageStaTrebaPoslatiKlijentu;
            connectedUserConnectionIdToStaTrebaPoslatiKlijentu.TryRemove(Context.ConnectionId, out garbageStaTrebaPoslatiKlijentu);
        }
    }
}

