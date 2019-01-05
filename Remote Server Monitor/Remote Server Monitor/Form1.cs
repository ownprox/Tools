using System;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace ServerMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool CanClose = false;
        bool[] ServersOnline;
        private void Form1_Load(object sender, EventArgs e)
        {
            if(File.Exists("Servers.conf"))
            using (StreamReader r = new StreamReader("Servers.conf"))
            {
                string line;
                string[] splts;
                while((line = r.ReadLine()) != null)
                {
                    splts = line.Split(',');
                    listView1.Items.Add(new EpiListStatus(splts[0], splts[1]));
                }
            }
            ServersOnline = new bool[listView1.Items.Count];
            GetServerStatus();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            GetServerStatus();
        }

        void GetServerStatus()
        {
            int ServerCount = listView1.Items.Count;
            for (int i = 0; i < ServerCount; i++)
            {
                EpiListStatus ELS = (EpiListStatus)listView1.Items[i];
                try
                {
                    new SourceQuery(ELS.Connection, ELS.Port, (SourceQuery.ServerData SrvData) =>
                    {
                        ELS.LastUpdate = DateTime.Now.AddMinutes(1);
                        ELS.UpdateStatus(this, SrvData.Name, SrvData.Players, true);
                    }).Dispose();

                    if (DateTime.Now.Subtract(ELS.LastUpdate).TotalSeconds > 70)
                    {
                        ServersOnline[i] = false;
                        ELS.UpdateStatus(this, string.Empty, 0, false);
                    }
                    else ServersOnline[i] = true;
                }
                catch { ServersOnline[i] = false; timer2.Start(); ELS.UpdateStatus(this, string.Empty, 0, false); }
            }

            bool EnableBeeper = false;
            for (int i = 0; i < ServerCount; i++) if (!ServersOnline[i]) EnableBeeper = true;
            if (EnableBeeper) timer2.Start();
            else timer2.Stop();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            SystemSounds.Beep.Play();
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showToolStripMenuItem.Text == "Hide")
            {
                showToolStripMenuItem.Text = "Show";
                Hide();
            }
            else
            {
                showToolStripMenuItem.Text = "Hide";
                Show();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!CanClose)
            {
                showToolStripMenuItem.Text = "Show";
                Hide();
                e.Cancel = true;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CanClose = true;
            Close();
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (showToolStripMenuItem.Text == "Hide")
            {
                showToolStripMenuItem.Text = "Show";
                Hide();
            }
            else
            {
                showToolStripMenuItem.Text = "Hide";
                Show();
            }
        }
    }

    class EpiListStatus : ListViewItem
    {
        public DateTime LastUpdate;
        public string Connection;
        public int Port;
        public EpiListStatus(string SrvName, string Connection)
        {
            this.Text = SrvName;
            string[] splts = Connection.Split(':');
            this.Connection = splts[0];
            int.TryParse(splts[1], out Port);
            LastUpdate = DateTime.Now;
            SubItems.AddRange(new string[] { "", "" });
            SubItems[1].Text = "0";
            SubItems[2].Text = "Offline";
        }

        public void UpdateStatus(Form frm, string SrvName, byte Players, bool Online)
        {
            frm.Invoke((MethodInvoker)delegate ()
            {
                if(SrvName != string.Empty) SubItems[0].Text = SrvName;
                SubItems[1].Text = Players.ToString();
                SubItems[2].Text = Online ? "Online" : "Offline";
            });
        }
    };
}