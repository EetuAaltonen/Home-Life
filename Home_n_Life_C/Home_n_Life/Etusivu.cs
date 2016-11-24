using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Home_n_Life
{
    public partial class Etusivu : Form
    {
        public Etusivu()
        {
            InitializeComponent();
        }
        //---View-Change-------------
        string view_change;
        GroupBox current_groupBox;
        //---Shopping-List-----------
        string item_info;
        //---------------------------

        private void viewChange(GroupBox current_groupBox_) 
        {
            if (current_groupBox != null)
            {
                current_groupBox.Enabled = false;
                current_groupBox.Visible = false;
            }
            current_groupBox = current_groupBox_;
            current_groupBox.Location = new Point(12, 170);
            current_groupBox.Size = new Size(964, 518);
            current_groupBox.Visible = true;
            current_groupBox.Enabled = true;
            switch (view_change)
            {
                case "home":
                    break;
                case "shopping_list":
                    listBox_shopping_list.Items.Clear();
                    item_info = "";
                    item_info = "<x> ------ Tuoteen nimi ---------------------------------------- Määrä --------------------------------- <x>";
                    listBox_shopping_list.Items.Add(item_info);
                    break; 
            }
        }

        private void button_shopping_list_Click(object sender, EventArgs e)
        {
            view_change = "shopping_list";
            viewChange(groupBox_shopping_list);
        }

        private void button_add_item_Click(object sender, EventArgs e)
        {
            item_info = "";
            item_info += "<" + (listBox_shopping_list.Items.Count) + ">";
            if (listBox_shopping_list.Items.Count >= 10)
            {
                item_info += " ---- ";
            }
            else
            {
                item_info += " ------ ";
            }
            item_info += textBox_item_name.Text + " ";
            for (int i = 0; i < 56 - textBox_item_name.Text.Length; i++)
            {
                item_info += "-";
            }
            item_info += " " + textBox_item_amount.Text + " -----------------------------------------";
            item_info += " <" + (listBox_shopping_list.Items.Count) + ">";
            listBox_shopping_list.Items.Add(item_info);
        }

        private void label_logo_Click(object sender, EventArgs e)
        {
            view_change = "home";
            viewChange(groupBox_home);
        }

        private void button_clear_item_Click(object sender, EventArgs e)
        {
            string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
            string deleteTableQuery = string.Format(@"DROP TABLE MyGuests;");
            string createTableQuery = string.Format(@"CREATE TABLE MyGuests (
                                                    id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                                    firstname VARCHAR(30) NOT NULL,
                                                    lastname VARCHAR(30) NOT NULL);");
            string insertTableQuery = string.Format(@"INSERT INTO MyGuests (id,firstname, lastname)
                                                    VALUES (5,'John', 'Doe');");
            string selectTableQuery = string.Format(@"SELECT id, firstname, lastname FROM MyGuests ;");
            MySqlConnection conn = new MySqlConnection(connetionString);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(deleteTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(insertTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    MessageBox.Show(Convert.ToString(dataReader["firstname"]) + "," + Convert.ToString(dataReader["lastname"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            finally
            {
                conn.Close();
            }
        }

        private void button_logout_Click_1(object sender, EventArgs e)
        {
            Kirjautuminen Kirjautuminen = new Kirjautuminen();
            Kirjautuminen.Show();
            this.Close();
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Etusivu_Load(object sender, EventArgs e)
        {
            view_change = "home";
            viewChange(groupBox_home);
        }
    }
}
