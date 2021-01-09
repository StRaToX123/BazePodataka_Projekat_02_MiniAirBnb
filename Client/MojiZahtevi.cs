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
    public partial class MojiZahtevi : Form
    {
        public BoolWrapper _mojiZahteviFormRunning;
        public Button _mojiZahteviButton;
        public IHubProxy _hub;

        private List<Oglas> _mojiOglasi = new List<Oglas>();
        private int _selectedOglasIndex;

        public MojiZahtevi()
        {
            InitializeComponent();
        }

        private void MojiZahtevi_Load(object sender, EventArgs e)
        {
            obrisiZahtevButton.Enabled = false;
            // Podesiti listview da izgleda nalik listbox
            this.zahteviListBox.DrawMode = DrawMode.OwnerDrawVariable;
            this.zahteviListBox.MeasureItem += ListBox1MeasureItem;
            this.zahteviListBox.DrawItem += ListBox1DrawItem;
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


        private void MojiZahtevi_FormClosed(object sender, FormClosedEventArgs e)
        {
            _mojiZahteviFormRunning.Variable = false;
            _mojiZahteviButton.Enabled = true;
        }

        public void AzurirajDataGridView(List<Oglas> oglasi)
        {
            _mojiOglasi = oglasi;
            Action action = delegate () {
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

        private void IscrtajZahteve()
        {
            zahteviListBox.Items.Clear();
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            for (int i = 0; i < _mojiOglasi[_selectedOglasIndex].zahtevi.Count; i++)
            {
                string itemString = "";
                switch (_mojiOglasi[_selectedOglasIndex].zahtevi[i].status)
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

                for (int j = 0; j < _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi.Count; j++)
                {
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Year.ToString();
                    itemString += "-";
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Month.ToString();
                    itemString += "-";
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Day.ToString();
                    itemString += " ";
                    j++;
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Year.ToString();
                    itemString += "-";
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Month.ToString();
                    itemString += "-";
                    itemString += _mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi[j].Day.ToString();
                    if (j != (_mojiOglasi[_selectedOglasIndex].zahtevi[i].datumi.Count - 1))
                    {
                        itemString += "\r\n";
                    }
                }

                zahteviListBox.Items.Add(itemString);
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
        }

        private void obrisiZahtevButton_Click(object sender, EventArgs e)
        {
            if (_selectedOglasIndex == -1)
            {
                return;
            }

            if (zahteviListBox.SelectedItems.Count == 0)
            {
                return;
            }

            // Brisanje oglasa je permanentno
            DialogResult dialogResult = MessageBox.Show("Brisanje oglasa je permanentno. Da li ste sugurni ?", "Brisanje Oglasa", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                // Obavesti server
                _hub.Invoke("ObrisiZahtev", _mojiOglasi[_selectedOglasIndex].id, _mojiOglasi[_selectedOglasIndex].zahtevi[zahteviListBox.SelectedIndex].id);
                // Obrisi zahtev iz lokalne memorije
                _mojiOglasi[_selectedOglasIndex].zahtevi.RemoveAt(zahteviListBox.SelectedIndex);
                // Ponovo iscrtaj zahteve
                IscrtajZahteve();
                obrisiZahtevButton.Enabled = false;
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        private void zahteviListBox_Click(object sender, EventArgs e)
        {
            if (zahteviListBox.SelectedItems.Count != 0)
            {
                obrisiZahtevButton.Enabled = true;
            }
        }
    }
}
