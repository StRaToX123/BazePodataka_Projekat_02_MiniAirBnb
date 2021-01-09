using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using Microsoft.AspNet.SignalR.Client;

namespace Client
{
    public partial class MojiOglasi : Form
    {
        public IHubProxy _hub;
        public Button _mojiOglasiButton;
        public BoolWrapper _mojiOglasiFormRunning;

        public DodajOglas _dodajOglasForm;
        public BoolWrapper _dodajOglasFormRunning = new BoolWrapper();

        public List<Oglas> _mojiOglasi = new List<Oglas>();
        // koristi se za poredjivanje sa zahtevima
        // Lista od liste od liste jer podrzavamo undo operaciju
        // za svaki oglas iz _mojiOglasi po jedna undo lista
        public List<List<List<DateTime>>> _undoOglasiDatumi;
        // Za svaki oglas undo lista zahteva tohg oglasa
        public List<List<List<Zahtev>>> _undoZahtevi;

        public int _selectedOglasIndex;

        public MojiOglasi()
        {
            InitializeComponent();
        }

        private void MojiOglasi_Load(object sender, EventArgs e)
        {
            _dodajOglasFormRunning.Variable = false;

            // Podesiti listview da izgleda nalik listbox
            this.zahteviListBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.zahteviListBox.MeasureItem += ListBox1MeasureItem;
            this.zahteviListBox.DrawItem += ListBox1DrawItem;

            undoButton.Enabled = false;
        }

        internal int CountOccurrences(string haystack, string needle)
        {
            int n = 0, pos = 0;
            while ((pos = haystack.IndexOf(needle, pos)) != -1)
            {
                n++;
                pos += needle.Length;
            }
            return n;
        }

        public void ListBox1MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)((CountOccurrences(((ListBox)sender).Items[e.Index].ToString(), "\n") + 1) * ((ListBox)sender).Font.GetHeight() + 2);
        }

        public void ListBox1DrawItem(object sender, DrawItemEventArgs e)
        {
            if (zahteviListBox.Items.Count == 0)
            {
                return;
            }

            string text = ((ListBox)sender).Items[e.Index].ToString();
            e.DrawBackground();
            using (Brush b = new SolidBrush(e.ForeColor)) e.Graphics.DrawString(text, e.Font, b, new RectangleF(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height));
            e.DrawFocusRectangle();
        }

        // friendsListView.Items[friendsListView.Items.Count - 1].ForeColor = Color.Green;

        private void MojiOglasi_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_dodajOglasFormRunning.Variable == true)
            {
                _dodajOglasFormRunning.Variable = false;
                _dodajOglasForm.Close();
            }

            _mojiOglasiFormRunning.Variable = false;
            _mojiOglasiButton.Enabled = true;
        }

        private void dodajOglasButton_Click(object sender, EventArgs e)
        {
            if (_dodajOglasFormRunning.Variable == false)
            {
                _dodajOglasFormRunning.Variable = true;
                DodajOglas _dodajOglasForm = new DodajOglas();
                _dodajOglasForm._hub = _hub;
                _dodajOglasForm._dodajOglasButton = dodajOglasButton;
                _dodajOglasForm._dodajOglasFormRunning = _dodajOglasFormRunning;
                dodajOglasButton.Enabled = false;
                _dodajOglasForm.Show();
            }
        }
        
        private string Filter(string str, List<char> charsToRemove)
        {
            foreach (char c in charsToRemove)
            {
                str = str.Replace(c.ToString(), String.Empty);
            }

            return str;
        }

        public void AzurirajDataGridView(List<Oglas> oglasi)
        {
            // inicializovati _undoOglasiDatumi i _undoZahtevi
            _undoOglasiDatumi = new List<List<List<DateTime>>>();
            _undoZahtevi = new List<List<List<Zahtev>>>();
            for (int i = 0; i < oglasi.Count; i++)
            {
                _undoZahtevi.Add(new List<List<Zahtev>>());
                _undoZahtevi[i].Add(new List<Zahtev>());
                // Popuni _undoZahtevi sa kopijama zahteva oglasa
                if (oglasi[i].zahtevi.Count != 0)
                {
                    // napravi kompiju oglasa za undo listu
                    List<Zahtev> kopijaZahteva = new List<Zahtev>();
                    for (int j = 0; j < oglasi[i].zahtevi.Count; j++)
                    {
                        Zahtev kopija = new Zahtev();
                        kopija.id = oglasi[i].zahtevi[j].id;
                        kopija.status = oglasi[i].zahtevi[j].status;
                        kopija.datumi = new List<DateTime>();
                        for (int k = 0; k < oglasi[i].zahtevi[j].datumi.Count; k++)
                        {
                            kopija.datumi.Add(oglasi[i].zahtevi[j].datumi[k]);
                        }

                        _undoZahtevi[i][0].Add(kopija);
                    }
                }

                _undoOglasiDatumi.Add(new List<List<DateTime>>());
                _undoOglasiDatumi[i].Add(new List<DateTime>());
                string datumiFilteredString = Filter(oglasi[i].datumi, new List<char> { ' ', '\r' }); // moramo da pretvorimo formatiran string datuma u listu DateTime varijabli radi poredjivanja datuma
                for (int j = 0; j < datumiFilteredString.Length; j++)
                {
                    if (datumiFilteredString[j] == '\n')
                    {
                        if ((j + 1) <= (datumiFilteredString.Length - 1))
                        {
                            if (datumiFilteredString[j + 1] == '|')
                            {
                                datumiFilteredString = datumiFilteredString.Remove(j, 1);
                                j++;
                                datumiFilteredString = datumiFilteredString.Remove(j, 1);
                                j--;
                            }
                            else
                            {
                                datumiFilteredString = datumiFilteredString.Remove(j, 1);
                                datumiFilteredString = datumiFilteredString.Insert(j, "|");
                            }
                        }
                        else
                        {
                            datumiFilteredString = datumiFilteredString.Remove(j, 1);
                        }
                    }
                }
                string[] datumiArray = datumiFilteredString.Split('|');
                foreach (string datum in datumiArray)
                {
                    string[] datumData = datum.Split('-');
                    int godina;
                    int mesec;
                    int dan;
                    Int32.TryParse(datumData[0], out godina);
                    Int32.TryParse(datumData[1], out mesec);
                    Int32.TryParse(datumData[2], out dan);

                    _undoOglasiDatumi[i].Last().Add(new DateTime(godina, mesec, dan));
                }

                int waste = 0;
            }
            

            Action action = delegate () {
                _mojiOglasi = oglasi;
                for (int i = 0; i < oglasi.Count; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[1].Value = oglasi[i].adresa;
                    dataGridView1.Rows[i].Cells[2].Value = oglasi[i].wifi.ToString();
                    dataGridView1.Rows[i].Cells[3].Value = oglasi[i].tus.ToString();
                    dataGridView1.Rows[i].Cells[4].Value = oglasi[i].parking_mesto.ToString();
                    dataGridView1.Rows[i].Cells[5].Value = oglasi[i].tv.ToString();
                    dataGridView1.Rows[i].Cells[6].Value = oglasi[i].datumi;
                    dataGridView1.Rows[i].Cells[7].Value = oglasi[i].opis;

                    dataGridView1.Rows[i].Cells[0].Value = imageList1.Images[i];
                }

                dataGridView1.RowTemplate.Resizable = DataGridViewTriState.True;
                dataGridView1.RowTemplate.Height = 256;

                dataGridView1.RowTemplate.MinimumHeight = 256;
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                
            };

            dataGridView1.BeginInvoke(action);
        }

        public void AzurirajDataGridViewSliku(List<Oglas> oglasi, int oglasIndex)
        {
            Action action = delegate () {
                Bitmap bmp;
                using (var ms = new MemoryStream(oglasi[oglasIndex].slika))
                {
                    bmp = new Bitmap(ms);
                }

                this.imageList1.Images.Add(bmp);
                this.imageList1.ImageSize = new Size(256, 256);
                
            };

            dataGridView1.BeginInvoke(action);
        }



        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            foreach (DataGridViewRow x in dataGridView1.Rows)
            {
                x.MinimumHeight = 256;
            }

            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                if (c.Index == 0)
                {
                    c.MinimumWidth = 256;
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > (_mojiOglasi.Count - 1)) // jer uvek na datagridview postoji jedna vise vrsta na kraju
            {
                _selectedOglasIndex = -1;
            }
            else
            {
                _selectedOglasIndex = e.RowIndex;
            }
            
            IscrtajZahteve();
            IscrtajDatumeOgalsa();
        }

        private void IscrtajDatumeOgalsa()
        {
            for (int i = 0; i < _undoOglasiDatumi.Count; i++)
            {
                string datumiString = "";
                bool temp = false;
                for (int j = 0; j < _undoOglasiDatumi[i].Last().Count; j++)
                {
                    datumiString += _undoOglasiDatumi[i].Last()[j].Year.ToString();
                    datumiString += "-";
                    datumiString += _undoOglasiDatumi[i].Last()[j].Month.ToString();
                    datumiString += "-";
                    datumiString += _undoOglasiDatumi[i].Last()[j].Day.ToString();
                    if (temp == false)
                    {
                        temp = true;
                        datumiString += "\r\n | \r\n";
                    }
                    else
                    {
                        temp = false;
                        if (j != (_undoOglasiDatumi[i].Last().Count - 1))
                        {
                            datumiString += "\r\n";
                        }
                    }
                }

                dataGridView1.Rows[i].Cells[6].Value = datumiString;
            }
        }

        private void IscrtajZahteve()
        {
            zahteviListBox.Items.Clear();
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            for (int i = 0; i < _undoZahtevi[_selectedOglasIndex].Last().Count; i++)
            {
                string itemString = "";
                switch (_undoZahtevi[_selectedOglasIndex].Last()[i].status)
                {
                    case 0:
                        {
                            itemString += "Nije Obradjen \r\n";
                            break;
                        }
                    case 1:
                        {
                            itemString += "Odobren \r\n";
                            break;
                        }
                    case 2:
                        {
                            itemString += "Odbijen \r\n";
                            break;
                        }
                }

                for (int j = 0; j < _undoZahtevi[_selectedOglasIndex].Last()[i].datumi.Count; j++)
                {
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Year.ToString();
                    itemString += "-";
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Month.ToString();
                    itemString += "-";
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Day.ToString();
                    itemString += " ";
                    j++;
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Year.ToString();
                    itemString += "-";
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Month.ToString();
                    itemString += "-";
                    itemString += _undoZahtevi[_selectedOglasIndex].Last()[i].datumi[j].Day.ToString();
                    if (j != (_undoZahtevi[_selectedOglasIndex].Last()[i].datumi.Count - 1))
                    {
                        itemString += "\r\n";
                    }
                }

                zahteviListBox.Items.Add(itemString);
            }
        }

        private void PromenaStanja(int promenaDatumaOglasaIndex, 
            List<DateTime> datumiOglasa, 
            int promenaZahtevaOglasIndex, 
            int promenaZahtevaIndex,
            int noviStatus)
        {
            for (int i = 0; i < _undoOglasiDatumi.Count; i++)
            {
                if (i != promenaDatumaOglasaIndex)
                {
                    List<DateTime> kopija = new List<DateTime>();
                    for (int j = 0; j < _undoOglasiDatumi[i].Last().Count; j++)
                    {
                        kopija.Add(_undoOglasiDatumi[i].Last()[j]);
                    }

                    _undoOglasiDatumi[i].Add(kopija);
                }
                else
                {
                    _undoOglasiDatumi[i].Add(datumiOglasa);  
                }

                List<Zahtev> kopijaListeZahteva = new List<Zahtev>();
                for (int j = 0; j < _undoZahtevi[i].Last().Count; j++)
                {
                    Zahtev kopijaZahteva = new Zahtev();
                    kopijaZahteva.id = _undoZahtevi[i].Last()[j].id;
                    if ((i == promenaZahtevaOglasIndex) && (j == promenaZahtevaIndex))
                    {
                        kopijaZahteva.status = noviStatus;
                    }
                    else
                    {
                        kopijaZahteva.status = _undoZahtevi[i].Last()[j].status;
                    }
                    
                    kopijaZahteva.datumi = new List<DateTime>();
                    for (int k = 0; k < _undoZahtevi[i].Last()[j].datumi.Count; k++)
                    {
                        kopijaZahteva.datumi.Add(_undoZahtevi[i].Last()[j].datumi[k]);
                    }

                    kopijaListeZahteva.Add(kopijaZahteva);
                }

                _undoZahtevi[i].Add(kopijaListeZahteva);
            }
        }

        private void odobriZahtevButton_Click(object sender, EventArgs e)
        {
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            if (zahteviListBox.SelectedItems.Count != 0)
            {
                if (_undoZahtevi[_selectedOglasIndex].Last()[zahteviListBox.SelectedIndex].status != 1)
                {
                    List<DateTime> noviDatumiOglasaList;
                    noviDatumiOglasaList = OdobriZahtev(
                        _undoZahtevi[_selectedOglasIndex].Last()[zahteviListBox.SelectedIndex].datumi,
                        _undoOglasiDatumi[_selectedOglasIndex].Last());

                    if (noviDatumiOglasaList != null)
                    {
                        PromenaStanja(_selectedOglasIndex,
                            noviDatumiOglasaList,
                            _selectedOglasIndex,
                            zahteviListBox.SelectedIndex,
                            1);

                        IscrtajDatumeOgalsa();
                        IscrtajZahteve();

                        undoButton.Enabled = true;
                    }
                }
            }
        }

        private void odbijZahtevButton_Click(object sender, EventArgs e)
        {
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            if (zahteviListBox.SelectedItems.Count != 0)
            {
                if (_undoZahtevi[_selectedOglasIndex].Last()[zahteviListBox.SelectedIndex].status != 2) // ako oglas vec nije odbijen
                {
                    // Ako odbijamo zahtev koji smo primenili onda opet treba da se promene adrese stana
                    if (_undoZahtevi[_selectedOglasIndex].Last()[zahteviListBox.SelectedIndex].status == 1) // ako je oglas odobren
                    {
                        List<DateTime> noviStanDatumi;
                        noviStanDatumi = OdbijOdobrenZahtev(_undoZahtevi[_selectedOglasIndex].Last()[zahteviListBox.SelectedIndex].datumi,
                                                                _undoOglasiDatumi[_selectedOglasIndex].Last());

                        PromenaStanja(_selectedOglasIndex, noviStanDatumi, _selectedOglasIndex, zahteviListBox.SelectedIndex, 2);
                        IscrtajDatumeOgalsa();
                    }
                    else // ako je oglas ne obradjen
                    {
                        PromenaStanja(-1,
                        null,
                        _selectedOglasIndex,
                        zahteviListBox.SelectedIndex,
                        2);
                    }

                    IscrtajZahteve();
                    undoButton.Enabled = true;
                }
            }  
        }
        
        private void undoButton_Click(object sender, EventArgs e)
        {
            // Izbrisemo poslednje stavke u undo listama i onda ponovo iscrtamo datume i zahteve
            if (_undoOglasiDatumi.Count == 0)
            {
                return; // nema sta da se undo-uje
            }

            if (_undoOglasiDatumi[0].Count == 1)
            {
                return; // nema sta da se undo-uje
            }

            for (int i = 0; i < _undoOglasiDatumi.Count; i++)
            {
                _undoOglasiDatumi[i].RemoveAt(_undoOglasiDatumi[i].Count - 1); // ukloni poslednju promenu
                _undoZahtevi[i].RemoveAt(_undoZahtevi[i].Count - 1);
            }

            IscrtajDatumeOgalsa();
            IscrtajZahteve();

            if (_undoOglasiDatumi[0].Count == 1)
            {
                undoButton.Enabled = false;
            }
        }

        private void obrisiZahtevButton_Click(object sender, EventArgs e)
        {
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            // Napravimo celokupno novo stanje
            PromenaStanja(-1, null, -1, -1, -1);
            // Pa onda izbrisemo zahtev
            _undoZahtevi[_selectedOglasIndex].Last().RemoveAt(zahteviListBox.SelectedIndex);

            IscrtajZahteve();

            undoButton.Enabled = true;
        }

        private List<DateTime> OdobriZahtev(List<DateTime> zahtevDatumiList, List<DateTime> datumiStanListOriginal)
        {
            // Napravi kopiju liste datuma stana
            List<DateTime> datumiStanList = new List<DateTime>();
            for (int i = 0; i < datumiStanListOriginal.Count; i++)
            {
                datumiStanList.Add(datumiStanListOriginal[i]);
            }

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
                    MessageBox.Show("Ovaj zahtev nije moguce primeniti nad datumi dostupnosti stana");
                    return null;
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
                    MessageBox.Show("Ovaj zahtev nije moguce primeniti nad datumi dostupnosti stana");
                    return null;
                }
            }


            return datumiStanList;
        }

        // Treba da se datumi zahteva stope sa datumima stana
        private List<DateTime> OdbijOdobrenZahtev(List<DateTime> zahtevDatumiList, List<DateTime> datumiStanListOriginal)
        {
            // Prvo Napravi kopiju liste datuma stana
            List<DateTime> datumiStanList = new List<DateTime>();
            for (int i = 0; i < datumiStanListOriginal.Count; i++)
            {
                datumiStanList.Add(datumiStanListOriginal[i]);
            }

            for (int i = 0; i < zahtevDatumiList.Count; i += 2) // za svaki par datuma iz zahteva
            {
                int odIndex = -1;
                int doIndex = -1;
                for (int j = 0; j < datumiStanList.Count; j++)
                {
                    if (zahtevDatumiList[i] <= datumiStanList[j])
                    {
                        odIndex = j;
                        break;
                    }
                }

                if (odIndex == -1)
                {
                    odIndex = datumiStanList.Count;
                }

                for (int j = 0; j < datumiStanList.Count; j++)
                {
                    if (zahtevDatumiList[i + 1] <= datumiStanList[j])
                    {
                        doIndex = j;
                        break;
                    }
                }

                if (doIndex == -1)
                {
                    doIndex = datumiStanList.Count;
                }

                // brisu se dates pocevsi od odIndex + (((zahtevDatumiList[i] == datumiStanList[odIndex]) && ((odIndex % 2) == 0) ? 1 : 0)
                // broj dates koji se brise je
                // doIndex - odIndex - (((zahtevDatumiList[i] == datumiStanList[odIndex]) && ((odIndex % 2) == 0)) ? 1 : 0)               
                // ubacujemo u odIndex mesto zahtevDatumiList[i] samo ako (((zahtevDatumiList[i] != datumiStanList[odIndex]) && ((odIndex % 2) == 0) ? 1 : 0)
                // (obratiti paznju nije isti upit) vrlo bitno promenio se znak u prvom delu if-a sada je != 

                // ako je zahtevDatumList[i + 1] == datumiStanList[doIndex]                    
                // onda 
                // ako je doIndex paran broj onda se brise i taj datum na tom indexu
                // ako je doIndex neparan onda se nista ne desava
                // ako ovo zahtevDatumList[i + 1] == datumiStanList[doIndex] ne prodje
                // onda ako je doIndex paran onda na kraju upisuje na doIndex mestu              
                // datum iz zahtevDatumiList[i + 1]

                bool daLiCemoNaKrajuUbacitiZahtevOd = ((zahtevDatumiList[i] != datumiStanList[odIndex]) && ((odIndex % 2) == 0)) ? true : false;
                int odakleKreceBrisanje = odIndex + (((zahtevDatumiList[i] == datumiStanList[odIndex]) && ((odIndex % 2) == 0)) ? 1 : 0);
                int kolikoDatumaBrisemo = doIndex - odIndex - (((zahtevDatumiList[i] == datumiStanList[odIndex]) && ((odIndex % 2) == 0)) ? 1 : 0);

                if (zahtevDatumiList[i + 1] == datumiStanList[doIndex])
                {
                    if ((doIndex % 2) == 0) // odIndex je paran
                    {
                        datumiStanList.RemoveAt(doIndex);
                    }
                }
                else
                {
                    if ((doIndex % 2) == 0) // odIndex je paran
                    {
                        datumiStanList.Insert(doIndex, zahtevDatumiList[i + 1]);
                    }
                }

                for (int j = 0; j < kolikoDatumaBrisemo; j++)
                {
                    datumiStanList.RemoveAt(odakleKreceBrisanje);
                }

                if (daLiCemoNaKrajuUbacitiZahtevOd == true)
                {
                    datumiStanList.Insert(odIndex, zahtevDatumiList[i]);
                }
            }

            return datumiStanList;
        }

        private void potvrdiIzmeneButton_Click(object sender, EventArgs e)
        {
            // Treba poslati serveru konacno stanje na klijentskoj strani
            string konacnoStanjeString = "";
            // Prvo cemo ubaciti trenutno aktivne stanove i njihove nove datume
            for (int i = 0; i < _mojiOglasi.Count; i++)
            {
                konacnoStanjeString += _mojiOglasi[i].id.ToString();
                konacnoStanjeString += "#"; // odvaja id od datuma stana i od zahteva stana
                konacnoStanjeString += "[";
                for (int j = 0; j < _undoOglasiDatumi[i].Last().Count; j++)
                {
                    konacnoStanjeString += "'"; 
                    konacnoStanjeString += _undoOglasiDatumi[i].Last()[j].Year.ToString();
                    konacnoStanjeString += "-";
                    konacnoStanjeString += _undoOglasiDatumi[i].Last()[j].Month.ToString();
                    konacnoStanjeString += "-";
                    konacnoStanjeString += _undoOglasiDatumi[i].Last()[j].Day.ToString();
                    konacnoStanjeString += "'";
                    if (j != (_undoOglasiDatumi[i].Last().Count - 1))
                    {
                        konacnoStanjeString += ", "; // odvaja datume stana
                    }
                
                }

                konacnoStanjeString += "]";
                if (_undoZahtevi[i].Last().Count != 0)
                {
                    konacnoStanjeString += "#";

                    // Sada ubaciti zahteve za ovaj stan
                    for (int j = 0; j < _undoZahtevi[i].Last().Count; j++) // za svaki zahtev
                    {
                        konacnoStanjeString += _undoZahtevi[i].Last()[j].id.ToString();
                        konacnoStanjeString += " "; // razdvaja informacije zahteva
                        konacnoStanjeString += _undoZahtevi[i].Last()[j].status.ToString();
                        if (j != (_undoZahtevi[i].Last().Count - 1))
                        {
                            konacnoStanjeString += "%"; // razdvaja zahteve za ovaj oglas
                        }
                    }
                }

                if (i != (_mojiOglasi.Count - 1))
                {
                    konacnoStanjeString += "@"; // odvaja oglase
                }
            }

            _hub.Invoke("PromenaZahtevaStanova", konacnoStanjeString);

            // Sada mozemo da postavimo poslednje stanje u undo listi kao pocetno stanje
            for (int i = 0; i < _undoOglasiDatumi.Count; i++)
            {
                for (int j = 0; j < (_undoOglasiDatumi[i].Count - 1); j++) // brisemo sve osim poslednjeg stanja
                {
                    _undoOglasiDatumi[i].RemoveAt(0);
                    _undoZahtevi[i].RemoveAt(0);
                }
            }

            undoButton.Enabled = false;
        }

        private void obrisiOglasButton_Click(object sender, EventArgs e)
        {
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            // Brisanje oglasa je permanentno
            DialogResult dialogResult = MessageBox.Show("Brisanje oglasa je permanentno. Da li ste sugurni ?", "Brisanje Oglasa", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                _undoOglasiDatumi.RemoveAt(_selectedOglasIndex);
                _undoZahtevi.RemoveAt(_selectedOglasIndex);
                // Obavesti server
                _hub.Invoke("ObrisiStan", _mojiOglasi[_selectedOglasIndex].id);

                _mojiOglasi.RemoveAt(_selectedOglasIndex);
                _selectedOglasIndex = -1;
                IscrtajZahteve();
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }
    }
}
