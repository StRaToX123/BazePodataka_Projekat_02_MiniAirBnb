
namespace Client
{
    partial class MojiZahtevi
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Slika = new System.Windows.Forms.DataGridViewImageColumn();
            this.Adresa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Wifi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parkingMesto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tv = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datumi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.opis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zahteviListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.obrisiZahtevButton = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
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
            this.dataGridView1.Location = new System.Drawing.Point(12, 53);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 256;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.Size = new System.Drawing.Size(380, 385);
            this.dataGridView1.TabIndex = 67;
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Adresa.DefaultCellStyle = dataGridViewCellStyle1;
            this.Adresa.HeaderText = "Adresa";
            this.Adresa.Name = "Adresa";
            this.Adresa.Width = 65;
            // 
            // Wifi
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Wifi.DefaultCellStyle = dataGridViewCellStyle2;
            this.Wifi.HeaderText = "Wifi";
            this.Wifi.Name = "Wifi";
            this.Wifi.Width = 50;
            // 
            // Tus
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Tus.DefaultCellStyle = dataGridViewCellStyle3;
            this.Tus.HeaderText = "Tus";
            this.Tus.Name = "Tus";
            this.Tus.Width = 50;
            // 
            // parkingMesto
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.parkingMesto.DefaultCellStyle = dataGridViewCellStyle4;
            this.parkingMesto.HeaderText = "Parking Mesto";
            this.parkingMesto.Name = "parkingMesto";
            this.parkingMesto.Width = 50;
            // 
            // tv
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tv.DefaultCellStyle = dataGridViewCellStyle5;
            this.tv.HeaderText = "TV";
            this.tv.Name = "tv";
            this.tv.Width = 46;
            // 
            // datumi
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.datumi.DefaultCellStyle = dataGridViewCellStyle6;
            this.datumi.HeaderText = "Datumi";
            this.datumi.Name = "datumi";
            this.datumi.Width = 65;
            // 
            // opis
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.opis.DefaultCellStyle = dataGridViewCellStyle7;
            this.opis.HeaderText = "Opis";
            this.opis.Name = "opis";
            this.opis.Width = 256;
            // 
            // zahteviListBox
            // 
            this.zahteviListBox.FormattingEnabled = true;
            this.zahteviListBox.Location = new System.Drawing.Point(398, 53);
            this.zahteviListBox.Name = "zahteviListBox";
            this.zahteviListBox.Size = new System.Drawing.Size(390, 381);
            this.zahteviListBox.TabIndex = 80;
            this.zahteviListBox.Click += new System.EventHandler(this.zahteviListBox_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(395, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 18);
            this.label1.TabIndex = 83;
            this.label1.Text = "Moji Zahtevi";
            // 
            // obrisiZahtevButton
            // 
            this.obrisiZahtevButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.obrisiZahtevButton.Location = new System.Drawing.Point(666, 21);
            this.obrisiZahtevButton.Name = "obrisiZahtevButton";
            this.obrisiZahtevButton.Size = new System.Drawing.Size(122, 26);
            this.obrisiZahtevButton.TabIndex = 84;
            this.obrisiZahtevButton.Text = "Obrisi Zahtev";
            this.obrisiZahtevButton.UseVisualStyleBackColor = true;
            this.obrisiZahtevButton.Click += new System.EventHandler(this.obrisiZahtevButton_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MojiZahtevi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.obrisiZahtevButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.zahteviListBox);
            this.Controls.Add(this.dataGridView1);
            this.Name = "MojiZahtevi";
            this.Text = "MojiZahtevi";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MojiZahtevi_FormClosed);
            this.Load += new System.EventHandler(this.MojiZahtevi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewImageColumn Slika;
        private System.Windows.Forms.DataGridViewTextBoxColumn Adresa;
        private System.Windows.Forms.DataGridViewTextBoxColumn Wifi;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tus;
        private System.Windows.Forms.DataGridViewTextBoxColumn parkingMesto;
        private System.Windows.Forms.DataGridViewTextBoxColumn tv;
        private System.Windows.Forms.DataGridViewTextBoxColumn datumi;
        private System.Windows.Forms.DataGridViewTextBoxColumn opis;
        private System.Windows.Forms.ListBox zahteviListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button obrisiZahtevButton;
        private System.Windows.Forms.ImageList imageList1;
    }
}