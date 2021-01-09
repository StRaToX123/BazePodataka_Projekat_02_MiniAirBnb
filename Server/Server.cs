using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



using Microsoft.Owin.Hosting;
using Server.SignalRServer;

namespace CassandraDataLayer
{
    public partial class Server : Form
    {
       
        private IDisposable signalRServer;

        public Server()
        {
            InitializeComponent();
        }
       
        private void Server_Load(object sender, EventArgs e)
        {
            string url = @"http://localhost:8080/";
            signalRServer = WebApp.Start<Startup>(url);
        }


    }
}
