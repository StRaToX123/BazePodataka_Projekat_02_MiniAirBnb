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
    public partial class Client : Form
    {
        private HubConnection _connection;
        private IHubProxy _hub;

        private MojiOglasi _mojiOglasiForm;
        private BoolWrapper _mojiOglasiFormRunning = new BoolWrapper();

        public OglasPeview _oglasPreviewForm;
        public BoolWrapper _oglasPreviewFormRunning = new BoolWrapper();

        public MojiZahtevi _mojiZahteviForm;
        public BoolWrapper _mojiZahteviFormRunning = new BoolWrapper();

        public List<Oglas> _searchOglasi = new List<Oglas>();
        public List<Oglas> _mojiOglasi = new List<Oglas>();
        public List<Oglas> _mojiZahtevi = new List<Oglas>();
 



        public Client()
        {
            string url = @"http://localhost:8080/";
            _connection = new HubConnection(url);
            _hub = _connection.CreateHubProxy("TestHub");
            try
            {
                _connection.Start().Wait();
            }
            catch
            {
                MessageBox.Show("Server je offline :( \n pokusajte ponovo kasnije");
                System.Environment.Exit(1);
                return;
            }

            // Registrovanje callback funkcija za signalR poruke
            _hub.On("ReceiveLength", x => ReceiveLength(x));
            _hub.On("LogInSuccessful", x => LogInSuccessful(x));
            _hub.On("LogOutSuccessful", x => LogOutSuccessful(x));
            _hub.On("LogInFailed", x => LogInFailed(x));
            _hub.On("AccountCreatedSuccessfuly", x => MessageBox.Show("Uspedno ste napravili novi nalog!"));
            _hub.On("AccountCreationFailed", x => MessageBox.Show("Doslo je do greske pri pravljenju novog naloga!"));
            _hub.On("PribaviMojeOglaseOdgovor", x => PribaviMojeOglaseOdgovor(x));
            _hub.On<int, int, int, bool, byte[]>("PribaviMojeOglaseSlikaOdgovor", (x, y, z, w, t) => PribaviMojeOglaseSlikaOdgovor(x, y, z, w, t));
            _hub.On("PretraziOglaseOdgovor", x => PretraziOglaseOdgovor(x));
            _hub.On<int, int, int, bool, byte[]>("PretraziOglaseSlikaOdgovor", (x, y, z, w, t) => PretraziOglaseSlikaOdgovor(x, y, z, w, t));
            _hub.On("OglasUspednoPostavljen", x => OglasUspednoPostavljen(x));
            _hub.On<bool>("PostaviZahtevOdgovor", x => PostaviZahtevOdgovor(x));
            _hub.On("OglasUspesnoObrisan", x => OglasUspesnoObrisan(x));
            _hub.On("PromenaZahtevaStanovaOdgovor", x => PromenaZahtevaStanovaOdgovor(x));
            _hub.On("ZahtevUspesnoObrisan", x => ZahtevUspesnoObrisan(x));
            _hub.On("PribaviMojeZahteveOdgovor", x => PribaviMojeZahteveOdgovor(x));
            _hub.On<int, int, int, bool, byte[]>("PribaviMojeZahteveSlikaOdgovor", (x, y, z, w, t) => PribaviMojeZahteveSlikaOdgovor(x, y, z, w, t));
            InitializeComponent();
        }


        private void ReceiveLength(string x)
        {
            MessageBox.Show(x);
        }

        private void Client_Load(object sender, EventArgs e)
        {
            this.Text = "Cassandra MiniAirBnb";
            _mojiOglasiFormRunning.Variable = false;
            _oglasPreviewFormRunning.Variable = false;
            _mojiZahteviFormRunning.Variable = false;
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            _connection.Stop();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            _hub.Invoke("LogIn", usernameTextBox.Text, passwordTextBox.Text);
        }

        private void LogInFailed(string nista)
        {
            MessageBox.Show("Doslo je do greske pri prijavljivanju");
        }

        private void LogInSuccessful(string username)
        {
            Action actionChangeFormName = delegate () {
                this.Text = usernameTextBox.Text;
            };
            this.BeginInvoke(actionChangeFormName);
            Action action = delegate () {
                label1.Visible = false;
            };
            label1.BeginInvoke(action);
            Action action2 = delegate () {
                label2.Visible = false;
            };
            label2.BeginInvoke(action2);
            Action action3 = delegate () {
                label3.Visible = false;
            };
            label3.BeginInvoke(action3);
            Action action4 = delegate () {
                label4.Visible = false;
            };
            label4.BeginInvoke(action4);
            Action action5 = delegate () {
                label5.Visible = false;
            };
            label5.BeginInvoke(action5);
            Action action6 = delegate () {
                label6.Visible = false;
            };
            label6.BeginInvoke(action6);
            Action action7 = delegate () {
                label7.Visible = false;
            };
            label7.BeginInvoke(action7);
            Action action8 = delegate () {
                label8.Visible = false;
            };
            label8.BeginInvoke(action8);


            Action action9 = delegate () {
                usernameTextBox.Visible = false;
            };
            usernameTextBox.BeginInvoke(action9);
            Action action10 = delegate () {
                passwordTextBox.Visible = false;
            };
            passwordTextBox.BeginInvoke(action10);
            Action action11 = delegate () {
                loginButton.Visible = false;
            };
            loginButton.BeginInvoke(action11);

            Action action12 = delegate () {
                usernameCreateAccountTextBox.Visible = false;
            };
            Action action13 = delegate () {
                passwordCreateAccountTextBox.Visible = false;
            };
            Action action14 = delegate () {
                confirmPasswordCreateAccountTextBox.Visible = false;
            };
            Action action15 = delegate () {
                emailCreateAccountTextBox.Visible = false;
            };
            Action action16 = delegate () {
                createAccountButton.Visible = false;
            };

            usernameCreateAccountTextBox.BeginInvoke(action12);
            passwordCreateAccountTextBox.BeginInvoke(action13);
            confirmPasswordCreateAccountTextBox.BeginInvoke(action14);
            emailCreateAccountTextBox.BeginInvoke(action15);
            createAccountButton.BeginInvoke(action16);

            // Sada upaliti sve ostale kontrole
            Action action17 = delegate () {
                label9.Visible = true;
            };
            label9.BeginInvoke(action17);
            Action action18 = delegate () {
                label10.Visible = true;
            };
            label10.BeginInvoke(action18);
            Action action19 = delegate () {
                label11.Visible = true;
            };
            label11.BeginInvoke(action19);
            Action action20 = delegate () {
                addressSearchTextBox.Visible = true;
            };
            addressSearchTextBox.BeginInvoke(action20);
            Action action21 = delegate () {
                dateFromDateTimePicker.Visible = true;
                dateFromDateTimePicker.MinDate = DateTime.Now;
            };
            dateFromDateTimePicker.BeginInvoke(action21);
            Action action22 = delegate () {
                dateToDateTimePicker.Visible = true;
            };
            dateToDateTimePicker.BeginInvoke(action22);
            Action action23 = delegate () {
                pretraziButton.Visible = true;
            };
            pretraziButton.BeginInvoke(action23);
            Action action24 = delegate () {
                mojiOglasiButton.Visible = true;
            };
            mojiOglasiButton.BeginInvoke(action24);
            Action action25 = delegate () {
                this.Text = "Prijavjen kao : " + username;
            };
            this.BeginInvoke(action25);
            Action action26 = delegate () {
                loggoutButton.Visible = true;
            };
            loggoutButton.BeginInvoke(action26);
            Action action27 = delegate () {
                mojiZahteviButton.Visible = true;
            };
            mojiZahteviButton.BeginInvoke(action27);
            /*
            Action action27 = delegate () {
                dataGridView1.Visible = true;
                DirectoryInfo dir = new DirectoryInfo(@"c:\pic");
                foreach (FileInfo file in dir.GetFiles())
                {
                    try
                    {
                        this.imageList1.Images.Add(Image.FromFile(file.FullName));
                    }
                    catch
                    {
                        Console.WriteLine("This is not an image file");
                    }
                }
                this.imageList1.ImageSize = new Size(256, 256);
                dataGridView1.RowTemplate.Resizable = DataGridViewTriState.True;
                dataGridView1.RowTemplate.Height = 256;

                dataGridView1.RowTemplate.MinimumHeight = 256;
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
               // for (int i = 0; i < dataGridView1.Columns.Count; i++)
               // {
                  //  dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                  //  dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
               // }
                dataGridView1.Rows.Add();
                dataGridView1.Rows[0].Cells[0].Value = imageList1.Images[0];
                dataGridView1.Rows.Add();
                dataGridView1.Rows[1].Cells[0].Value = imageList1.Images[1];
            };
            dataGridView1.BeginInvoke(action27);
            */
        }

        private void LogOutSuccessful(string nista)
        {
            // Ukljuciti sve kontrole od login screen
            Action actionChangeFormName = delegate ()
            {
                this.Text = "Cassandra MiniAirBnb";
            };
            this.BeginInvoke(actionChangeFormName);
            Action action = delegate ()
            {
                label1.Visible = true;
            };
            label1.BeginInvoke(action);
            Action action2 = delegate ()
            {
                label2.Visible = true;
            };
            label2.BeginInvoke(action2);
            Action action3 = delegate ()
            {
                label3.Visible = true;
            };
            label3.BeginInvoke(action3);
            Action action4 = delegate ()
            {
                label4.Visible = true;
            };
            label4.BeginInvoke(action4);
            Action action5 = delegate ()
            {
                label5.Visible = true;
            };
            label5.BeginInvoke(action5);
            Action action6 = delegate ()
            {
                label6.Visible = true;
            };
            label6.BeginInvoke(action6);
            Action action7 = delegate ()
            {
                label7.Visible = true;
            };
            label7.BeginInvoke(action7);
            Action action8 = delegate ()
            {
                label8.Visible = true;
            };
            label8.BeginInvoke(action8);


            Action action9 = delegate ()
            {
                usernameTextBox.Visible = true;
            };
            usernameTextBox.BeginInvoke(action9);
            Action action10 = delegate ()
            {
                passwordTextBox.Visible = true;
            };
            passwordTextBox.BeginInvoke(action10);
            Action action11 = delegate ()
            {
                loginButton.Visible = true;
            };
            loginButton.BeginInvoke(action11);

            Action action12 = delegate ()
            {
                usernameCreateAccountTextBox.Visible = true;
            };
            Action action13 = delegate ()
            {
                passwordCreateAccountTextBox.Visible = true;
            };
            Action action14 = delegate ()
            {
                confirmPasswordCreateAccountTextBox.Visible = true;
            };
            Action action15 = delegate ()
            {
                emailCreateAccountTextBox.Visible = true;
            };
            Action action16 = delegate ()
            {
                createAccountButton.Visible = true;
            };

            usernameCreateAccountTextBox.BeginInvoke(action12);
            passwordCreateAccountTextBox.BeginInvoke(action13);
            confirmPasswordCreateAccountTextBox.BeginInvoke(action14);
            emailCreateAccountTextBox.BeginInvoke(action15);
            createAccountButton.BeginInvoke(action16);

            Action action17 = delegate ()
            {
                label9.Visible = false;
            };
            label9.BeginInvoke(action17);
            Action action18 = delegate ()
            {
                label10.Visible = false;
            };
            label10.BeginInvoke(action18);
            Action action19 = delegate ()
            {
                label11.Visible = false;
            };
            label11.BeginInvoke(action19);
            Action action20 = delegate ()
            {
                pretraziButton.Visible = false;
            };
            pretraziButton.BeginInvoke(action20);
            Action action21 = delegate ()
            {
                mojiOglasiButton.Visible = false;
            };
            mojiOglasiButton.BeginInvoke(action21);
            Action action22 = delegate ()
            {
                mojiZahteviButton.Visible = false;
            };
            mojiZahteviButton.BeginInvoke(action22);
            Action action23 = delegate ()
            {
                loggoutButton.Visible = false;
            };
            loggoutButton.BeginInvoke(action23);
            Action action24 = delegate ()
            {
                addressSearchTextBox.Visible = false;
            };
            addressSearchTextBox.BeginInvoke(action24);
            Action action25 = delegate ()
            {
                dateFromDateTimePicker.Visible = false;
            };
            dateFromDateTimePicker.BeginInvoke(action25);
            Action action26 = delegate ()
            {
                dateToDateTimePicker.Visible = false;
            };
            dateToDateTimePicker.BeginInvoke(action26);
            Action action27 = delegate ()
            {
                dataGridView1.Visible = false;
            };
            dataGridView1.BeginInvoke(action27);

            if (_mojiOglasiFormRunning.Variable == true)
            {
                _mojiOglasiFormRunning.Variable = false;
                _mojiOglasiForm.Close();
            }

            if (_mojiZahteviFormRunning.Variable == true)
            {
                _mojiZahteviFormRunning.Variable = false;
                _mojiZahteviForm.Close();
            }

            if (_oglasPreviewFormRunning.Variable == true)
            {
                _oglasPreviewFormRunning.Variable = false;
                _oglasPreviewForm.Close();
            }

            _searchOglasi.Clear();
            _mojiOglasi.Clear();
            _mojiZahtevi.Clear();
        }

            private void createAccountButton_Click(object sender, EventArgs e)
        {
            // Prvo proveriti da li su sva polja popunjena
            bool emailCheckResult = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailCreateAccountTextBox.Text);
                emailCheckResult = addr.Address == emailCreateAccountTextBox.Text;
            }
            catch
            {
                emailCheckResult = false;
            }

            if (emailCheckResult == false)
            {
                MessageBox.Show("Niste lepo uneli Email adresu");
                return;
            }

            // Provera validno popunjen username
            if (usernameCreateAccountTextBox.Text.Contains(" "))
            {
                MessageBox.Show("Username nesme da ima razmake");
                return;
            }

            if (usernameCreateAccountTextBox.Text.Length < 3)
            {
                MessageBox.Show("Username nesme da ima manje od 3 karaktera");
                return;
            }

            char[] SpecialChars = "!@#$%^&*()".ToCharArray();
            int indexOf = usernameCreateAccountTextBox.Text.IndexOfAny(SpecialChars);
            if (indexOf != -1)
            {
                MessageBox.Show("Username nesme da sadrzi specijalne karaktere");
                return;
            }

            // Provera validno popunjen password
            if (passwordCreateAccountTextBox.Text.Contains(" "))
            {
                MessageBox.Show("Password nesme da ima razmake");
                return;
            }

            if (passwordCreateAccountTextBox.Text.Length < 3)
            {
                MessageBox.Show("Password nesme da ima manje od 3 karaktera");
                return;
            }

            if (passwordCreateAccountTextBox.Text != confirmPasswordCreateAccountTextBox.Text)
            {
                MessageBox.Show("Passwords se ne poklapaju");
                return;
            }

            _hub.Invoke("CreateAccount", usernameCreateAccountTextBox.Text,
                passwordCreateAccountTextBox.Text,
                emailCreateAccountTextBox.Text);
        }



        private void mojiOglasiButton_Click(object sender, EventArgs e)
        {
            if (_mojiOglasiFormRunning.Variable == false)
            {
                _mojiOglasiFormRunning.Variable = true;
                _mojiOglasiForm = new MojiOglasi();
                _mojiOglasiForm._hub = _hub;
                _mojiOglasiForm._mojiOglasiButton = mojiOglasiButton;
                _mojiOglasiForm._mojiOglasiFormRunning = _mojiOglasiFormRunning;
                mojiOglasiButton.Enabled = false;
                _mojiOglasi.Clear();
                _mojiOglasiForm.Show();
                _hub.Invoke("PribaviMojeOglase", "nista");
            }
        }

        private void PribaviMojeOglaseOdgovor(string data)
        {
            // Raspakovanje informacija
            string[] oglasiStrings = data.Split('%');
            foreach (string oglasString in oglasiStrings)
            {
                if (oglasString == "")
                {
                    continue;
                }
                string[] oglasData = oglasString.Split('@');
                int oglasIndex;
                Int32.TryParse(oglasData[0], out oglasIndex);

                _mojiOglasi[oglasIndex].slikaReceivedNumberOfBytes = 0;
                _mojiOglasi[oglasIndex].adresa = oglasData[1];
                _mojiOglasi[oglasIndex].opis = oglasData[2];
                _mojiOglasi[oglasIndex].wifi = oglasData[3] == "True" ? true : false;
                _mojiOglasi[oglasIndex].tus = oglasData[4] == "True" ? true : false;
                _mojiOglasi[oglasIndex].parking_mesto = oglasData[5] == "True" ? true : false;
                _mojiOglasi[oglasIndex].tv = oglasData[6] == "True" ? true : false;
                _mojiOglasi[oglasIndex].datumi = oglasData[7];
                Int32.TryParse(oglasData[8], out _mojiOglasi[oglasIndex].id);
                // Ako postoje zahtevi za ovaj oglas onda ih treba prikazati
                if (oglasData.Length == 10)
                {
                    string[] zahtevi = oglasData[9].Split('$');
                    foreach (string zahtev in zahtevi)
                    {
                        string[] zahtevData = zahtev.Split('#');
                        Zahtev noviZahtev = new Zahtev();
                        Int32.TryParse(zahtevData[0], out noviZahtev.id);
                        Int32.TryParse(zahtevData[1], out noviZahtev.status);
                        string[] datumi = zahtevData[2].Split(' ');
                        noviZahtev.datumi = new List<DateTime>();
                        foreach (string datum in datumi)
                        {
                            string[] datumData = datum.Split('-');
                            int godina;
                            int mesec;
                            int dan;
                            Int32.TryParse(datumData[0], out godina);
                            Int32.TryParse(datumData[1], out mesec);
                            Int32.TryParse(datumData[2], out dan);
                            noviZahtev.datumi.Add(new DateTime(godina, mesec, dan));
                        }

                        _mojiOglasi[oglasIndex].zahtevi.Add(noviZahtev);
                    }
                }
            }

            if (_mojiOglasiFormRunning.Variable == true)
            {
                _mojiOglasiForm.AzurirajDataGridView(_mojiOglasi);
            }
        }

        private void PribaviMojeOglaseSlikaOdgovor(int indexOglasa, int offsetBloka, int velicinaSlikeBytes, bool jestePoslednjiBlok, byte[] blok)
        {
            // Ako je stigao blok slike od oglasaza koji nemamo mesto u listi oglasa, treba povecati kapacitet liste
            if ((indexOglasa + 1) > _mojiOglasi.Count)
            {
                int trenutanCount = (indexOglasa + 1) - _mojiOglasi.Count;
                for (int i = 0; i < trenutanCount; i++)
                {
                    _mojiOglasi.Add(new Oglas());
                    _mojiOglasi.Last().zahtevi = new List<Zahtev>();
                }
            }

            // Ako je ovo prvi blok koji je stigao, treba alocirati mesto na klijentu za celu sliku
            if (_mojiOglasi[indexOglasa].slika == null)
            {
                _mojiOglasi[indexOglasa].slika = new byte[velicinaSlikeBytes];
            }

            // Iskopirati blok
            System.Buffer.BlockCopy(blok, 0, _mojiOglasi[indexOglasa].slika, offsetBloka, blok.Length);

            // Ako je ovo bio poslednji blok onda mozemo da prikazemo sliku
            if (jestePoslednjiBlok)
            {
                _mojiOglasiForm.AzurirajDataGridViewSliku(_mojiOglasi, indexOglasa);
            }
        }

        private void pretraziButton_Click(object sender, EventArgs e)
        {
            // Proveriti da li su datumi lepo unešeni
            if (dateFromDateTimePicker.Value >= dateToDateTimePicker.Value)
            {
                MessageBox.Show("Proverite datum od i datum do");
                return;
            }

            string adresa = "";
            if (addressSearchTextBox.Text != null)
            {
                adresa = addressSearchTextBox.Text;
            }

            _searchOglasi.Clear();
            dataGridView1.Rows.Clear();
            imageList1.Images.Clear();

            _hub.Invoke("PretraziOglase",
                adresa,
                dateFromDateTimePicker.Value.Year,
                dateFromDateTimePicker.Value.Month,
                dateFromDateTimePicker.Value.Day,
                dateToDateTimePicker.Value.Year,
                dateToDateTimePicker.Value.Month,
                dateToDateTimePicker.Value.Day);
        }

        private void PretraziOglaseOdgovor(string data)
        {
            // Raspakovanje informacija
            _searchOglasi.Clear();
            string[] oglasiStrings = data.Split('%');
            foreach (string oglasString in oglasiStrings)
            {
                if (oglasString == "")
                {
                    continue;
                }
                string[] oglasData = oglasString.Split('@');
                int oglasIndex;
                Int32.TryParse(oglasData[0], out oglasIndex);

                if ((oglasIndex + 1) > _searchOglasi.Count)
                {
                    int trenutanCount = (oglasIndex + 1) - _searchOglasi.Count;
                    for (int i = 0; i < trenutanCount; i++)
                    {
                        _searchOglasi.Add(new Oglas());
                    }
                }

                _searchOglasi[oglasIndex].slikaReceivedNumberOfBytes = 0;
                _searchOglasi[oglasIndex].adresa = oglasData[1];
                _searchOglasi[oglasIndex].opis = oglasData[2];
                _searchOglasi[oglasIndex].wifi = oglasData[3] == "True" ? true : false;
                _searchOglasi[oglasIndex].tus = oglasData[4] == "True" ? true : false;
                _searchOglasi[oglasIndex].parking_mesto = oglasData[5] == "True" ? true : false;
                _searchOglasi[oglasIndex].tv = oglasData[6] == "True" ? true : false;
                _searchOglasi[oglasIndex].datumi = oglasData[7];
                Int32.TryParse(oglasData[8], out _searchOglasi[oglasIndex].id);
            }

            Action action = delegate () {

                int counter = 0;
                for (int i = (_searchOglasi.Count - 1); i >= 0; i--)
                {
                    dataGridView1.Rows[i].Cells[1].Value = _searchOglasi[i].adresa;
                    dataGridView1.Rows[i].Cells[2].Value = _searchOglasi[i].wifi.ToString();
                    dataGridView1.Rows[i].Cells[3].Value = _searchOglasi[i].tus.ToString();
                    dataGridView1.Rows[i].Cells[4].Value = _searchOglasi[i].parking_mesto.ToString();
                    dataGridView1.Rows[i].Cells[5].Value = _searchOglasi[i].tv.ToString();
                    dataGridView1.Rows[i].Cells[6].Value = _searchOglasi[i].datumi;
                    dataGridView1.Rows[i].Cells[7].Value = _searchOglasi[i].opis;

                    dataGridView1.RowTemplate.Resizable = DataGridViewTriState.True;
                    dataGridView1.RowTemplate.Height = 256;

                    dataGridView1.RowTemplate.MinimumHeight = 256;
                    dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;


                    dataGridView1.Rows[i].Cells[0].Value = imageList1.Images[i];
                    counter++;
                }
            };

            dataGridView1.BeginInvoke(action);
        }

        private void PretraziOglaseSlikaOdgovor(int indexOglasa, int offsetBloka, int velicinaSlikeBytes, bool jestePoslednjiBlok, byte[] blok)
        {
            // Ako je stigao blok slike od oglasaza koji nemamo mesto u listi oglasa, treba povecati kapacitet liste
            if ((indexOglasa + 1) > _searchOglasi.Count)
            {
                int trenutanCount = (indexOglasa + 1) - _searchOglasi.Count;
                for (int i = 0; i < trenutanCount; i++)
                {
                    _searchOglasi.Add(new Oglas());
                    Action action2 = delegate ()
                    {
                        dataGridView1.Rows.Add();
                    };
                    dataGridView1.BeginInvoke(action2);
                }
            }

            // Ako je ovo prvi blok koji je stigao, treba alocirati mesto na klijentu za celu sliku
            if (_searchOglasi[indexOglasa].slika == null)
            {
                _searchOglasi[indexOglasa].slika = new byte[velicinaSlikeBytes];
            }

            // Iskopirati blok
            System.Buffer.BlockCopy(blok, 0, _searchOglasi[indexOglasa].slika, offsetBloka, blok.Length);

            // Ako je ovo bio poslednji blok onda mozemo da prikazemo sliku
            if (jestePoslednjiBlok)
            {
                Action action = delegate () {
                    Bitmap bmp;
                    using (var ms = new MemoryStream(_searchOglasi[indexOglasa].slika))
                    {
                        bmp = new Bitmap(ms);
                    }

                    imageList1.Images.Add(bmp);
                    imageList1.ImageSize = new Size(256, 256);
                };

                dataGridView1.BeginInvoke(action);
            }
        }

        private void OglasUspednoPostavljen(string nista)
        {
            MessageBox.Show("Oglas Uspesno Postavljen !");
        }

        private void dataGridView1_CellPainting_1(object sender, DataGridViewCellPaintingEventArgs e)
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

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_oglasPreviewFormRunning.Variable == false)
            {
                _oglasPreviewFormRunning.Variable = true;
                _oglasPreviewForm = new OglasPeview();
                _oglasPreviewForm._oglasPreviewFormRunning = _oglasPreviewFormRunning;
                _oglasPreviewForm._hub = _hub;
                _oglasPreviewForm.Show();
            }

            _oglasPreviewForm.AzurirajPrikaz(_searchOglasi[e.RowIndex], imageList1.Images[e.RowIndex]);
        }

        private void PostaviZahtevOdgovor(bool odgovor)
        {
            if (odgovor == false)
            {
                MessageBox.Show("Proverite unešene datume, moraju biti u opsegu dostupnosti stana");
            }
            else
            {
                MessageBox.Show("Zahtev uspešno poslat");
            }
        }

        private void OglasUspesnoObrisan(string nista)
        {
            MessageBox.Show("Oglas Uspesno Obrisan");
        }

        private void PromenaZahtevaStanovaOdgovor(string nista)
        {
            MessageBox.Show("Izmene Uspedno Obradjene");
        }

        private void zahteviButton_Click(object sender, EventArgs e)
        {
            if (_mojiZahteviFormRunning.Variable == false)
            {
                _mojiZahteviFormRunning.Variable = true;
                _mojiZahteviForm = new MojiZahtevi();
                _mojiZahteviForm._mojiZahteviFormRunning = _mojiZahteviFormRunning;
                _mojiZahteviForm._hub = _hub;
                mojiZahteviButton.Enabled = false;
                _mojiZahteviForm._mojiZahteviButton = mojiZahteviButton;
                _mojiZahtevi.Clear();
                _mojiZahteviForm.Show();
                _hub.Invoke("PribaviMojeZahteve", "nista");
            }
        }

        private void ZahtevUspesnoObrisan(string nista)
        {
            MessageBox.Show("Zahtev Uspesno Obrisan");
        }

        private void PribaviMojeZahteveOdgovor(string data)
        {
            // Raspakovanje informacija
            string[] oglasiStrings = data.Split('%');
            foreach (string oglasString in oglasiStrings)
            {
                if (oglasString == "")
                {
                    continue;
                }
                string[] oglasData = oglasString.Split('@');
                int oglasIndex;
                Int32.TryParse(oglasData[0], out oglasIndex);

                _mojiZahtevi[oglasIndex].slikaReceivedNumberOfBytes = 0;
                _mojiZahtevi[oglasIndex].adresa = oglasData[1];
                _mojiZahtevi[oglasIndex].opis = oglasData[2];
                _mojiZahtevi[oglasIndex].wifi = oglasData[3] == "True" ? true : false;
                _mojiZahtevi[oglasIndex].tus = oglasData[4] == "True" ? true : false;
                _mojiZahtevi[oglasIndex].parking_mesto = oglasData[5] == "True" ? true : false;
                _mojiZahtevi[oglasIndex].tv = oglasData[6] == "True" ? true : false;
                _mojiZahtevi[oglasIndex].datumi = oglasData[7];
                Int32.TryParse(oglasData[8], out _mojiZahtevi[oglasIndex].id);
                // Ako postoje zahtevi za ovaj oglas onda ih treba prikazati
                if (oglasData.Length == 10)
                {
                    string[] zahtevi = oglasData[9].Split('$');
                    foreach (string zahtev in zahtevi)
                    {
                        string[] zahtevData = zahtev.Split('#');
                        Zahtev noviZahtev = new Zahtev();
                        Int32.TryParse(zahtevData[0], out noviZahtev.id);
                        Int32.TryParse(zahtevData[1], out noviZahtev.status);
                        string[] datumi = zahtevData[2].Split(' ');
                        noviZahtev.datumi = new List<DateTime>();
                        foreach (string datum in datumi)
                        {
                            string[] datumData = datum.Split('-');
                            int godina;
                            int mesec;
                            int dan;
                            Int32.TryParse(datumData[0], out godina);
                            Int32.TryParse(datumData[1], out mesec);
                            Int32.TryParse(datumData[2], out dan);
                            noviZahtev.datumi.Add(new DateTime(godina, mesec, dan));
                        }

                        _mojiZahtevi[oglasIndex].zahtevi.Add(noviZahtev);
                    }
                }
            }

            if (_mojiZahteviFormRunning.Variable == true)
            {
                _mojiZahteviForm.AzurirajDataGridView(_mojiZahtevi);
            }
        }

        private void PribaviMojeZahteveSlikaOdgovor(int indexOglasa, int offsetBloka, int velicinaSlikeBytes, bool jestePoslednjiBlok, byte[] blok)
        {
            // Ako je stigao blok slike od oglasaza koji nemamo mesto u listi oglasa, treba povecati kapacitet liste
            if ((indexOglasa + 1) > _mojiZahtevi.Count)
            {
                int trenutanCount = (indexOglasa + 1) - _mojiZahtevi.Count;
                for (int i = 0; i < trenutanCount; i++)
                {
                    _mojiZahtevi.Add(new Oglas());
                    _mojiZahtevi.Last().zahtevi = new List<Zahtev>();
                }
            }

            // Ako je ovo prvi blok koji je stigao, treba alocirati mesto na klijentu za celu sliku
            if (_mojiZahtevi[indexOglasa].slika == null)
            {
                _mojiZahtevi[indexOglasa].slika = new byte[velicinaSlikeBytes];
            }

            // Iskopirati blok
            System.Buffer.BlockCopy(blok, 0, _mojiZahtevi[indexOglasa].slika, offsetBloka, blok.Length);

            // Ako je ovo bio poslednji blok onda mozemo da prikazemo sliku
            if (jestePoslednjiBlok)
            {
                _mojiZahteviForm.AzurirajDataGridViewSliku(_mojiZahtevi, indexOglasa);
            }
        }

        private void loggoutButton_Click(object sender, EventArgs e)
        {
            _hub.Invoke("LogOut", "nista").Wait();
        }
    }
}
