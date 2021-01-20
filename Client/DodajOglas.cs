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
    public partial class DodajOglas : Form
    {
        public IHubProxy _hub;

        public Button _dodajOglasButton;
        public BoolWrapper _dodajOglasFormRunning;
        private List<DateTimePicker> _datumiOd = new List<DateTimePicker>();
        private List<DateTimePicker> _datumiDo = new List<DateTimePicker>();
        private List<byte[]> _imageAsByteArrayList = new List<byte[]>();


        public DodajOglas()
        {
            InitializeComponent();
        }

        private void DodajOglas_Load(object sender, EventArgs e)
        {
            dataGridView1.MultiSelect = false;
            AddNewDateTimePickerToGridView(true);
            AddNewDateTimePickerToGridView(false);
        }

        private void DodajOglas_FormClosed(object sender, FormClosedEventArgs e)
        {
            _dodajOglasFormRunning.Variable = false;
            _dodajOglasButton.Enabled = true;
        }

        private void dodajDatumButton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            AddNewDateTimePickerToGridView(true);
            AddNewDateTimePickerToGridView(false);
        }

        private void obrisiDatumButton_Click(object sender, EventArgs e)
        {
            _datumiOd.RemoveAt(dataGridView1.SelectedRows[0].Index);
            _datumiDo.RemoveAt(dataGridView1.SelectedRows[0].Index);
            dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
        }

        private void postaviOglasButton_Click(object sender, EventArgs e)
        {
            if (adresaTextBox.Text == "")
            {
                MessageBox.Show("Niste uneli adresu stana");
                return;
            }
            if (opisTextBox.Text == "")
            {
                MessageBox.Show("Niste uneli opis stana");
                return;
            }
            if (_datumiOd.Count == 0)
            {
                MessageBox.Show("Niste uneli datume za izdavanje stana");
            }
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Niste izabrali sliku stana");
            }

            // Provera da li su datumi validni
            for (int i = 0; i < _datumiOd.Count; i++)
            {
                if (_datumiOd[i].Value > _datumiDo[i].Value)
                {
                    MessageBox.Show("Datum od na poziciji " + i.ToString() + " vam je nakon od datumi do");
                    return;
                }

                for (int j = 0; j < i; j++)
                {
                    if ((_datumiOd[j].Value >= _datumiOd[i].Value) || (_datumiDo[j].Value >= _datumiDo[i].Value))
                    {
                        MessageBox.Show("Proverite datume, preplicu se, moraju da budu u rastucem redosledu");
                        return;
                    }
                }
            }

            for (int i = 0; i < _imageAsByteArrayList.Count; i++)
            {
                _hub.Invoke("ClientSaljeBlockSlike",
                    i.ToString(),
                    _imageAsByteArrayList.Count.ToString(),
                    _imageAsByteArrayList[i]);
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

            // Sada jada je slika poslata, treba poslati i ostatak informacija o stanu
            _hub.Invoke("DodajOglas",
                adresaTextBox.Text,
                checkBox1.Checked,
                checkBox2.Checked,
                checkBox3.Checked,
                checkBox4.Checked,
                opisTextBox.Text,
                datumi);
        }

        private void dodajSlikuButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Select Photo";

            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    FileInfo fi = new FileInfo(openFileDialog1.FileNames[0]);
                    if (fi.Length > 41943040)
                    {
                        MessageBox.Show("Molimo vas izaverite sliku manju od 40 MB");
                        return;
                    }
                    Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                    Bitmap myBitmap = new Bitmap(openFileDialog1.FileNames[0]);
                    Image myThumbnail = myBitmap.GetThumbnailImage(300, 300,
                        myCallback, IntPtr.Zero);

                    _imageAsByteArrayList.Clear();

                    byte[] imageByteArray = ConvertImageToByteArray(myBitmap, ".bmp");
                    int numberOfIterations = imageByteArray.Length / 30720;
                    int maxNumberOfBlocks = numberOfIterations;
                    if (imageByteArray.Length % 30720 != 0)
                    {
                        maxNumberOfBlocks++;
                    }

                    int offset = 0;
                    for (int i = 0; i < numberOfIterations; i++)
                    {
                        byte[] block = new byte[30720];
                        System.Buffer.BlockCopy(imageByteArray, offset, block, 0, 30720);
                        offset += 30720;
                        _imageAsByteArrayList.Add(block);
                    }

                    if (numberOfIterations != maxNumberOfBlocks)
                    {
                        byte[] lastBlock = new byte[imageByteArray.Length - offset];
                        System.Buffer.BlockCopy(imageByteArray, offset, lastBlock, 0, imageByteArray.Length - offset);
                        _imageAsByteArrayList.Add(lastBlock);
                    }


                    pictureBox1.Image = myThumbnail;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        private bool ThumbnailCallback()
        {
            return false;
        }

        public byte[] ConvertImageToByteArray(Image image, string extension)
        {
            using (var memoryStream = new MemoryStream())
            {
                switch (extension)
                {
                    case ".jpeg":
                    case ".jpg":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".png":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".gif":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case ".bmp":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }
                return memoryStream.ToArray();
            }
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

        private void opisTextBox_TextChanged(object sender, EventArgs e)
        {
            char[] SpecialChars = "@#$%^&".ToCharArray();
            int indexOf = opisTextBox.Text.IndexOfAny(SpecialChars);
            if (indexOf != -1)
            {
                opisTextBox.Text = opisTextBox.Text.Remove(opisTextBox.Text.Length - 1, 1);
                opisTextBox.SelectionStart = opisTextBox.Text.Length;
                opisTextBox.SelectionLength = 0;
            }
        }

        private void opisTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}
