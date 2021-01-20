
namespace Client
{
    partial class DodajOglas
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.DatumOd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DatumDo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.opisTextBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dodajSlikuButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dodajDatumButton = new System.Windows.Forms.Button();
            this.postaviOglasButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.obrisiDatumButton = new System.Windows.Forms.Button();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.adresaTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DatumOd,
            this.DatumDo});
            this.dataGridView1.Location = new System.Drawing.Point(12, 262);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(248, 240);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // DatumOd
            // 
            this.DatumOd.HeaderText = "Datum Od";
            this.DatumOd.Name = "DatumOd";
            // 
            // DatumDo
            // 
            this.DatumDo.HeaderText = "Datum Do";
            this.DatumDo.Name = "DatumDo";
            // 
            // opisTextBox
            // 
            this.opisTextBox.Location = new System.Drawing.Point(266, 362);
            this.opisTextBox.Multiline = true;
            this.opisTextBox.Name = "opisTextBox";
            this.opisTextBox.Size = new System.Drawing.Size(556, 140);
            this.opisTextBox.TabIndex = 1;
            this.opisTextBox.TextChanged += new System.EventHandler(this.opisTextBox_TextChanged);
            this.opisTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.opisTextBox_KeyDown);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(269, 37);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(553, 302);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // dodajSlikuButton
            // 
            this.dodajSlikuButton.Location = new System.Drawing.Point(747, 8);
            this.dodajSlikuButton.Name = "dodajSlikuButton";
            this.dodajSlikuButton.Size = new System.Drawing.Size(75, 23);
            this.dodajSlikuButton.TabIndex = 3;
            this.dodajSlikuButton.Text = "Dodaj Sliku";
            this.dodajSlikuButton.UseVisualStyleBackColor = true;
            this.dodajSlikuButton.Click += new System.EventHandler(this.dodajSlikuButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(266, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Slika";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(266, 346);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Opis";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Datumi Dostupnosti";
            // 
            // dodajDatumButton
            // 
            this.dodajDatumButton.Location = new System.Drawing.Point(12, 233);
            this.dodajDatumButton.Name = "dodajDatumButton";
            this.dodajDatumButton.Size = new System.Drawing.Size(116, 23);
            this.dodajDatumButton.TabIndex = 7;
            this.dodajDatumButton.Text = "Dodaj Datum";
            this.dodajDatumButton.UseVisualStyleBackColor = true;
            this.dodajDatumButton.Click += new System.EventHandler(this.dodajDatumButton_Click);
            // 
            // postaviOglasButton
            // 
            this.postaviOglasButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.postaviOglasButton.Location = new System.Drawing.Point(711, 508);
            this.postaviOglasButton.Name = "postaviOglasButton";
            this.postaviOglasButton.Size = new System.Drawing.Size(111, 45);
            this.postaviOglasButton.TabIndex = 8;
            this.postaviOglasButton.Text = "Postavi Oglas";
            this.postaviOglasButton.UseVisualStyleBackColor = true;
            this.postaviOglasButton.Click += new System.EventHandler(this.postaviOglasButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 100);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(44, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Wifi";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(15, 123);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(44, 17);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "Tuš";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(15, 146);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(93, 17);
            this.checkBox3.TabIndex = 11;
            this.checkBox3.Text = "Parking mesto";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(15, 169);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(40, 17);
            this.checkBox4.TabIndex = 12;
            this.checkBox4.Text = "TV";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // obrisiDatumButton
            // 
            this.obrisiDatumButton.Location = new System.Drawing.Point(134, 233);
            this.obrisiDatumButton.Name = "obrisiDatumButton";
            this.obrisiDatumButton.Size = new System.Drawing.Size(116, 23);
            this.obrisiDatumButton.TabIndex = 13;
            this.obrisiDatumButton.Text = "Obrisi Datum";
            this.obrisiDatumButton.UseVisualStyleBackColor = true;
            this.obrisiDatumButton.Click += new System.EventHandler(this.obrisiDatumButton_Click);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // adresaTextBox
            // 
            this.adresaTextBox.Location = new System.Drawing.Point(12, 37);
            this.adresaTextBox.Name = "adresaTextBox";
            this.adresaTextBox.Size = new System.Drawing.Size(100, 20);
            this.adresaTextBox.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Adresa";
            // 
            // DodajOglas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 556);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.adresaTextBox);
            this.Controls.Add(this.obrisiDatumButton);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.postaviOglasButton);
            this.Controls.Add(this.dodajDatumButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dodajSlikuButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.opisTextBox);
            this.Controls.Add(this.dataGridView1);
            this.Name = "DodajOglas";
            this.Text = "DodajOglas";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DodajOglas_FormClosed);
            this.Load += new System.EventHandler(this.DodajOglas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox opisTextBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button dodajSlikuButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button dodajDatumButton;
        private System.Windows.Forms.Button postaviOglasButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button obrisiDatumButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn DatumOd;
        private System.Windows.Forms.DataGridViewTextBoxColumn DatumDo;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.TextBox adresaTextBox;
        private System.Windows.Forms.Label label4;
    }
}