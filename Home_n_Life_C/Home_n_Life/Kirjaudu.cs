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

        private void button_login_Click(object sender, EventArgs e)
        {
            Etusivu Etusivu = new Etusivu();
            Etusivu.Show();
            Etusivu.linkLabel_user.Text = "kaldjlaksdjlakdjklasdjlkasjsdlk";
            this.Hide();
        }
    }
}
