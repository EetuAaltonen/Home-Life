using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Home_n_Life
{
    public partial class Kirjautuminen : Form
    {
        public Kirjautuminen()
        {
            InitializeComponent();
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            Etusivu Etusivu = new Etusivu();
            Etusivu.Show();
            Etusivu.linkLabel_user.Text = textBox_username.Text;
            this.Hide();
        }

        private void Kirjautuminen_Load(object sender, EventArgs e)
        {
            groupBox_login.Location = new Point(13,65);
            groupBox_login.Size = new Size(478,337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
        }

        private void button_create_account_Click(object sender, EventArgs e)
        {
            groupBox_login.Location = new Point(13, 65);
            groupBox_login.Size = new Size(478, 337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
        }

        private void button_sign_up_Click(object sender, EventArgs e)
        {
            groupBox_registration.Location = new Point(13, 65);
            groupBox_registration.Size = new Size(478, 337);
            groupBox_registration.Enabled = true;
            groupBox_registration.Visible = true;
            groupBox_login.Enabled = false;
            groupBox_login.Visible = false;
        }
    }
}
