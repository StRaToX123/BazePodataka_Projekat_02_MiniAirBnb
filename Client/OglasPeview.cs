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
    public partial class OglasPeview : Form
    {
        public BoolWrapper _oglasPreviewFormRunning;
        public IHubProxy _hub;

        private List<DateTimePicker> _datumiOd = new List<DateTimePicker>();
        private List<DateTimePicker> _datumiDo = new List<DateTimePicker>();

        private Oglas _trenutnoPrikazanOglas;

        public OglasPeview()
        {
            InitializeComponent();
        }

        private void OglasPeview_Load(object sender, EventArgs e)
        { 
            dataGridView1.MultiSelect = false;
            AddNewDateTimePickerToGridView(true);
            AddNewDateTimePickerToGridView(false);
        }

        private void OglasPeview_FormClosed(object sender, FormClosedEventArgs e)
        {
            _oglasPreviewFormRunning.Variable = false;
        }

        public void AzurirajPrikaz(Oglas oglas, Image slika)
        {
            adresaTextBox.Text = oglas.adresa;
            wifiCheckBox.Checked = oglas.wifi;
            tusCheckBox.Checked = oglas.tus;
            parkingMestoCheckBox.Checked = oglas.parking_mesto;
            tvCheckBox.Checked = oglas.tv;
            opisTextBox.Text = oglas.opis;
            dostupniDatumiTextBox.Text = oglas.datumi;
           // Bitmap bmp;
           // using (var ms = new MemoryStream(slika))
            //{
               // bmp = new Bitmap(ms);
           // }
            pictureBox1.Image = slika;

            dataGridView1.Rows.Clear();
            _datumiOd.Clear();
            _datumiDo.Clear();
            AddNewDateTimePickerToGridView(true);
            AddNewDateTimePickerToGridView(false);

            _trenutnoPrikazanOglas = oglas;
        }

        private void dodajVrstuButton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            AddNewDateTimePickerToGridView(true);
            AddNewDateTimePickerToGridView(false);
        }

        private void obrisiVrstuButton_Click(object sender, EventArgs e)
        {
            _datumiOd.RemoveAt(dataGridView1.SelectedRows[0].Index);
            _datumiDo.RemoveAt(dataGridView1.SelectedRows[0].Index);
            dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DateTimePicker newDateTimePicker;
            if (e.ColumnIndex == 0) // datumi od
            {
                newDateTimePicker = _datumiOd[e.RowIndex];
            }
            else // datumi do
            {
                newDateTimePicker = _datumiDo[e.RowIndex];
            }



            Rectangle oRectangle = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            newDateTimePicker.Size = new Size(oRectangle.Width, oRectangle.Height);
            newDateTimePicker.Location = new Point(oRectangle.X, oRectangle.Y);
            newDateTimePicker.Visible = true;
        }

        private void AddNewDateTimePickerToGridView(bool odIliDo)
        {
            if (odIliDo == true)
            {
                DateTimePicker newDateTimePicker = new DateTimePicker();
                newDateTimePicker.Visible = false;
                newDateTimePicker.Format = DateTimePickerFormat.Short;
                newDateTimePicker.TextChanged += new EventHandler(dateTimePicker_OnTextChange);
                newDateTimePicker.CloseUp += new EventHandler(DateTimePicker_CloseUp);
                _datumiOd.Add(newDateTimePicker);
                newDateTimePicker.Name = "0 " + (_datumiOd.Count - 1).ToString();
                newDateTimePicker.Value = DateTime.Now;
                dataGridView1.Controls.Add(newDateTimePicker);
            }
            else
            {
                DateTimePicker newDateTimePicker = new DateTimePicker();
                newDateTimePicker.Visible = false;
                newDateTimePicker.Format = DateTimePickerFormat.Short;
                newDateTimePicker.TextChanged += new EventHandler(dateTimePicker_OnTextChange);
                newDateTimePicker.CloseUp += new EventHandler(DateTimePicker_CloseUp);
                _datumiDo.Add(newDateTimePicker);
                newDateTimePicker.Name = "1 " + (_datumiOd.Count - 1).ToString();
                newDateTimePicker.Value = DateTime.Now;
                dataGridView1.Controls.Add(newDateTimePicker);
            }
        }

        private void dateTimePicker_OnTextChange(object sender, EventArgs e)
        {
            var dateTimePicker = (DateTimePicker)sender;
            string[] data = dateTimePicker.Name.Split(' ');
            int doIliOd;
            int kojiDateTimePicker;
            Int32.TryParse(data[0], out doIliOd);
            Int32.TryParse(data[1], out kojiDateTimePicker);

            if (doIliOd == 0)
            {
                dataGridView1.Rows[kojiDateTimePicker].Cells[doIliOd].Value = _datumiOd[kojiDateTimePicker].Text.ToString();
            }
            else
            {
                dataGridView1.Rows[kojiDateTimePicker].Cells[doIliOd].Value = _datumiDo[kojiDateTimePicker].Text.ToString();
            }

        }

        void DateTimePicker_CloseUp(object sender, EventArgs e)
        {
            var dtp = (DateTimePicker)sender;
            dtp.Visible = false;
        }

        private void postaviZahtevButton_Click(object sender, EventArgs e)
        {
            // pretvoriti sve unete datume u string i poslati serveru na obradu
            // server ce proveriti da li to validni datumi za taj stan i ako jesu napravice adekvatne izmene
            // i obavestiti nas o rezultatu obrade zahteva
            // Provera da li su datumi validni
            for (int i = 0; i < _datumiOd.Count; i++)
            {
                if (_datumiOd[i].Value > _datumiDo[i].Value)
                {
                    MessageBox.Show("Datum od na poziciji " + i.ToString() + " vam je nakon od datumi do");
                    return;
                }
            }

            // Napravi listu od localdates
            string datumi = "";
            int index = 0;
            for (int i = 0; i < (_datumiOd.Count * 2); i++)
            {
                if ((i % 2) == 0) // datumi od
                {
                    datumi += _datumiOd[index].Value.Year.ToString();
                    datumi += "-";
                    datumi += _datumiOd[index].Value.Month.ToString();
                    datumi += "-";
                    datumi += _datumiOd[index].Value.Day.ToString();
                    datumi += "@";
                }
                else // datumi do
                {
                    datumi += _datumiDo[index].Value.Year.ToString();
                    datumi += "-";
                    datumi += _datumiDo[index].Value.Month.ToString();
                    datumi += "-";
                    datumi += _datumiDo[index].Value.Day.ToString();
                    index++;
                    if (i != ((_datumiOd.Count * 2) - 1))
                    {
                        datumi += "@";
                    }
                }
            }

            _hub.Invoke("PostaviZahtev", _trenutnoPrikazanOglas.id, datumi);
        }
    }
}
