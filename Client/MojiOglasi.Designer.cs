
namespace Client
{
    partial class MojiOglasi
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Slika = new System.Windows.Forms.DataGridViewImageColumn();
            this.Adresa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Wifi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parkingMesto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tv = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datumi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.opis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.obrisiOglasButton = new System.Windows.Forms.Button();
            this.dodajOglasButton = new System.Windows.Forms.Button();
            this.odobriZahtevButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.odbijZahtevButton = new System.Windows.Forms.Button();
            this.undoButton = new System.Windows.Forms.Button();
            this.obrisiZahtevButton = new System.Windows.Forms.Button();
            this.potvrdiIzmeneButton = new System.Windows.Forms.Button();
            this.zahteviListBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Slika,
            this.Adresa,
            this.Wifi,
            this.Tus,
            this.parkingMesto,
            this.tv,
            this.datumi,
            this.opis});
            this.dataGridView1.Location = new System.Drawing.Point(12, 62);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 256;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.Size = new System.Drawing.Size(380, 385);
            this.dataGridView1.TabIndex = 66;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            // 
            // Slika
            // 
            this.Slika.HeaderText = "Slika";
            this.Slika.Name = "Slika";
            // 
            // Adresa
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Adresa.DefaultCellStyle = dataGridViewCellStyle8;
            this.Adresa.HeaderText = "Adresa";
            this.Adresa.Name = "Adresa";
            this.Adresa.Width = 65;
            // 
            // Wifi
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Wifi.DefaultCellStyle = dataGridViewCellStyle9;
            this.Wifi.HeaderText = "Wifi";
            this.Wifi.Name = "Wifi";
            this.Wifi.Width = 50;
            // 
            // Tus
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Tus.DefaultCellStyle = dataGridViewCellStyle10;
            this.Tus.HeaderText = "Tus";
            this.Tus.Name = "Tus";
            this.Tus.Width = 50;
            // 
            // parkingMesto
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.parkingMesto.DefaultCellStyle = dataGridViewCellStyle11;
            this.parkingMesto.HeaderText = "Parking Mesto";
            this.parkingMesto.Name = "parkingMesto";
            this.parkingMesto.Width = 50;
            // 
            // tv
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tv.DefaultCellStyle = dataGridViewCellStyle12;
            this.tv.HeaderText = "TV";
            this.tv.Name = "tv";
            this.tv.Width = 46;
            // 
            // datumi
            // 
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.datumi.DefaultCellStyle = dataGridViewCellStyle13;
            this.datumi.HeaderText = "Datumi";
            this.datumi.Name = "datumi";
            this.datumi.Width = 65;
            // 
            // opis
            // 
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.opis.DefaultCellStyle = dataGridViewCellStyle14;
            this.opis.HeaderText = "Opis";
            this.opis.Name = "opis";
            this.opis.Width = 256;
            // 
            // obrisiOglasButton
            // 
            this.obrisiOglasButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.obrisiOglasButton.Location = new System.Drawing.Point(154, 12);
            this.obrisiOglasButton.Name = "obrisiOglasButton";
            this.obrisiOglasButton.Size = new System.Drawing.Size(133, 28);
            this.obrisiOglasButton.TabIndex = 67;
            this.obrisiOglasButton.Text = "Obrisi Oglas";
            this.obrisiOglasButton.UseVisualStyleBackColor = true;
            this.obrisiOglasButton.Click += new System.EventHandler(this.obrisiOglasButton_Click);
            // 
            // dodajOglasButton
            // 
            this.dodajOglasButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dodajOglasButton.Location = new System.Drawing.Point(12, 12);
            this.dodajOglasButton.Name = "dodajOglasButton";
            this.dodajOglasButton.Size = new System.Drawing.Size(136, 28);
            this.dodajOglasButton.TabIndex = 68;
            this.dodajOglasButton.Text = "Dodaj Oglas";
            this.dodajOglasButton.UseVisualStyleBackColor = true;
            this.dodajOglasButton.Click += new System.EventHandler(this.dodajOglasButton_Click);
            // 
            // odobriZahtevButton
            // 
            this.odobriZahtevButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.odobriZahtevButton.Location = new System.Drawing.Point(517, 4);
            this.odobriZahtevButton.Name = "odobriZahtevButton";
            this.odobriZahtevButton.Size = new System.Drawing.Size(122, 26);
            this.odobriZahtevButton.TabIndex = 71;
            this.odobriZahtevButton.Text = "Odobri Zahtev";
            this.odobriZahtevButton.UseVisualStyleBackColor = true;
            this.odobriZahtevButton.Click += new System.EventHandler(this.odobriZahtevButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 72;
            this.label1.Text = "Moji Oglasi";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(395, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 73;
            this.label2.Text = "Zahtevi";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // odbijZahtevButton
            // 
            this.odbijZahtevButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.odbijZahtevButton.Location = new System.Drawing.Point(517, 33);
            this.odbijZahtevButton.Name = "odbijZahtevButton";
            this.odbijZahtevButton.Size = new System.Drawing.Size(122, 26);
            this.odbijZahtevButton.TabIndex = 74;
            this.odbijZahtevButton.Text = "Odbij Zahtev";
            this.odbijZahtevButton.UseVisualStyleBackColor = true;
            this.odbijZahtevButton.Click += new System.EventHandler(this.odbijZahtevButton_Click);
            // 
            // undoButton
            // 
            this.undoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.undoButton.Location = new System.Drawing.Point(666, 4);
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new System.Drawing.Size(122, 26);
            this.undoButton.TabIndex = 76;
            this.undoButton.Text = "Undo";
            this.undoButton.UseVisualStyleBackColor = true;
            this.undoButton.Click += new System.EventHandler(this.undoButton_Click);
            // 
            // obrisiZahtevButton
            // 
            this.obrisiZahtevButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.obrisiZahtevButton.Location = new System.Drawing.Point(666, 33);
            this.obrisiZahtevButton.Name = "obrisiZahtevButton";
            this.obrisiZahtevButton.Size = new System.Drawing.Size(122, 26);
            this.obrisiZahtevButton.TabIndex = 77;
            this.obrisiZahtevButton.Text = "Obrisi Zahtev";
            this.obrisiZahtevButton.UseVisualStyleBackColor = true;
            this.obrisiZahtevButton.Click += new System.EventHandler(this.obrisiZahtevButton_Click);
            // 
            // potvrdiIzmeneButton
            // 
            this.potvrdiIzmeneButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.potvrdiIzmeneButton.Location = new System.Drawing.Point(641, 384);
            this.potvrdiIzmeneButton.Name = "potvrdiIzmeneButton";
            this.potvrdiIzmeneButton.Size = new System.Drawing.Size(147, 54);
            this.potvrdiIzmeneButton.TabIndex = 78;
            this.potvrdiIzmeneButton.Text = "Potvrdi Izmene";
            this.potvrdiIzmeneButton.UseVisualStyleBackColor = true;
            this.potvrdiIzmeneButton.Click += new System.EventHandler(this.potvrdiIzmeneButton_Click);
            // 
            // zahteviListBox
            // 
            this.zahteviListBox.FormattingEnabled = true;
            this.zahteviListBox.Location = new System.Drawing.Point(398, 62);
            this.zahteviListBox.Name = "zahteviListBox";
            this.zahteviListBox.Size = new System.Drawing.Size(390, 316);
            this.zahteviListBox.TabIndex = 79;
            // 
            // MojiOglasi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.zahteviListBox);
            this.Controls.Add(this.potvrdiIzmeneButton);
            this.Controls.Add(this.obrisiZahtevButton);
            this.Controls.Add(this.undoButton);
            this.Controls.Add(this.odbijZahtevButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.odobriZahtevButton);
            this.Controls.Add(this.dodajOglasButton);
            this.Controls.Add(this.obrisiOglasButton);
            this.Controls.Add(this.dataGridView1);
            this.Name = "MojiOglasi";
            this.Text = "MojiOglasi";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MojiOglasi_FormClosed);
            this.Load += new System.EventHandler(this.MojiOglasi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button obrisiOglasButton;
        private System.Windows.Forms.Button dodajOglasButton;
        private System.Windows.Forms.Button odobriZahtevButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.DataGridViewImageColumn Slika;
        private System.Windows.Forms.DataGridViewTextBoxColumn Adresa;
        private System.Windows.Forms.DataGridViewTextBoxColumn Wifi;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tus;
        private System.Windows.Forms.DataGridViewTextBoxColumn parkingMesto;
        private System.Windows.Forms.DataGridViewTextBoxColumn tv;
        private System.Windows.Forms.DataGridViewTextBoxColumn datumi;
        private System.Windows.Forms.DataGridViewTextBoxColumn opis;
        private System.Windows.Forms.Button odbijZahtevButton;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.Button obrisiZahtevButton;
        private System.Windows.Forms.Button potvrdiIzmeneButton;
        private System.Windows.Forms.ListBox zahteviListBox;
    }
}