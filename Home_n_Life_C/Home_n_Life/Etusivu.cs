using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace Home_n_Life
{
    public partial class Etusivu : Form
    {
        public Etusivu()
        {
            InitializeComponent();
        }
//----- Attributes --------------------------------------------------------------------------------------
        //---Database----------------
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
        MySqlConnection conn;
        MySqlCommand cmd;
        string dropTableQuery, createTableQuery, insertTableQuery, selectTableQuery, updateTableQuery, deleteTableQuery;
        //---View-Change-------------
        string view_change;
        GroupBox current_groupBox;
        Button current_button;
        //---Shopping-List-----------
        string info_text, added_item;
        bool list_progressing;
        //---Change_tracking---------
        string change;
//----- Etusivu Load --------------------------------------------------------------------------------------
        private void Etusivu_Load(object sender, EventArgs e)
        {
            view_change = "home";
            conn = new MySqlConnection(connetionString);
            current_button = button_logo;
            viewChange(groupBox_home, button_logo);
        }
//----- Change View --------------------------------------------------------------------------------------
        private void viewChange(GroupBox current_groupBox_, Button current_button_)
        {
            if (current_groupBox_ != current_groupBox)
            {
                if (current_button != button_logo)
                {
                    current_button.BackColor = Color.DodgerBlue;
                }
                if (current_button_ != button_logo)
                {
                    current_button = current_button_;
                    current_button.BackColor = Color.CadetBlue;
                }
                if (current_groupBox != null)
                {
                    current_groupBox.Enabled = false;
                    current_groupBox.Visible = false;
                }
                current_groupBox = current_groupBox_;
                current_groupBox.Location = new Point(12, 170);
                current_groupBox.Size = new Size(967, 518);
                current_groupBox.Visible = true;
                current_groupBox.Enabled = true;
                switch (view_change)
                {
                    case "home":
                        break;
                    case "shopping_list":
                        textBox_shopping_list.Text = "";
                        textBox_shopping_list.ForeColor = Color.Gray;
                        info_text = "Kirjoita tähän tuotteet, esim näin:" +
                                    System.Environment.NewLine +
                                    "   - Tuote1" +
                                    System.Environment.NewLine +
                                    "   - Tuote2" +
                                    System.Environment.NewLine +
                                    "   - Tuote3" +
                                    System.Environment.NewLine +
                                    "   - Tuote4" +
                                    System.Environment.NewLine +
                                    "   - Tuote5";
                        textBox_shopping_list.Text += info_text;
                        break;
                    case "change_tracking":
                        break;
                }
            }
        }
//----- Database --------------------------------------------------------------------------------------
        private void checkDatabaseConnection()
        {
            progressBar_database_connection.Value = 1;
            progressBar_database_connection.ForeColor = Color.Yellow;
            try
            {
                conn = new MySqlConnection(connetionString);
                conn.Open();
                progressBar_database_connection.Value = 2;
                progressBar_database_connection.ForeColor = Color.LimeGreen;
                conn.Close();
            }
            catch
            {
                MessageBox.Show("Tietokantaan ei saatu yhteyttä.", "Error");
                progressBar_database_connection.Value = 1;
                progressBar_database_connection.ForeColor = Color.Red;
            }
        }
//----- Economic --------------------------------------------------------------------------------------
        private void button_economic_Click(object sender, EventArgs e)
        {
            checkDatabaseConnection();
            view_change = "economic";
            viewChange(groupBox_economic, button_economic);
            createTableQuery = @"CREATE TABLE IF NOT EXISTS economic_primary (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        balance DOUBLE(10,2) NOT NULL);";
            insertTableQuery = @"INSERT INTO economic_primary (id, username, balance) " +
                                    "SELECT * FROM(SELECT 0, '" + linkLabel_user.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT username FROM economic_primary WHERE username = '" + linkLabel_user.Text + "' " +
                                    ") LIMIT 1 ;";
            selectTableQuery = @"SELECT id, username, balance " +
                                " FROM economic_primary " +
                                " WHERE username='" + linkLabel_user.Text + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                createTableQuery = @"CREATE TABLE IF NOT EXISTS economic_secondary (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        type DATETIME NOT NULL,
                                        amount VARCHAR(100) NOT NULL);";
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                createTableQuery = @"CREATE TABLE IF NOT EXISTS economic_tertiary (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        type DATETIME NOT NULL,
                                        amount VARCHAR(100) NOT NULL);";
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(insertTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    textBox_deposit_amount.Text = Convert.ToString(dataReader["balance"]);
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }
        private void button_deposit_Click(object sender, EventArgs e)
        {
            double added_amount = 0;
            selectTableQuery = @"SELECT id, username, balance " +
                                " FROM economic_primary " +
                                " WHERE username='" + linkLabel_user.Text + "' ;";
            try
            {
                
                conn.Open();
                cmd = new MySqlCommand(selectTableQuery, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    added_amount = (double.Parse(Convert.ToString(dataReader["balance"])) + double.Parse(textBox_deposit_amount.Text));
                }
                dataReader.Close();
                updateTableQuery = @"UPDATE economic_primary " +
                                     "SET balance='" + Convert.ToString(added_amount) + "' " +
                                     "WHERE username='" + linkLabel_user.Text + "' ;";
                cmd = new MySqlCommand(updateTableQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                MessageBox.Show("Talletettu");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }
//----- Shopping list --------------------------------------------------------------------------------------
        private void readShoppingLists()
        {
            comboBox_shopping_lists.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS shoppinglist (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        listname VARCHAR(30) NOT NULL,
                                        text TEXT(500) NOT NULL);";
            selectTableQuery = @"SELECT id, username, listname, text " +
                                " FROM shoppinglist " +
                                " WHERE username='" + linkLabel_user.Text + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    comboBox_shopping_lists.Items.Add(Convert.ToString(dataReader["listname"]));
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void button_shopping_list_Click(object sender, EventArgs e)
        {
            checkDatabaseConnection();
            view_change = "shopping_list";
            viewChange(groupBox_shopping_list, button_shopping_list);
            textBox_list_name.Text = "";
            readShoppingLists();
        }

        private void button_add_item_Click(object sender, EventArgs e)
        {
            if (textBox_item_name.Text.Length > 0)
            {
                if (textBox_shopping_list.Text == info_text)
                {
                    textBox_shopping_list.ForeColor = Color.Black;
                    textBox_shopping_list.Text = "";
                }
                added_item = "> " + textBox_item_name.Text + "  ";
                added_item += textBox_item_amount.Text + " " + comboBox_amount_type.SelectedItem;
                added_item += System.Environment.NewLine;
                textBox_shopping_list.Text += added_item;
                textBox_text_length.Text = Convert.ToString(textBox_shopping_list.Text.Length);
            }
            else
            {
                MessageBox.Show("Syötä tuotteen nimi", "Tuotteen lisääminen");
            }
        }

        private void label_logo_Click(object sender, EventArgs e)
        {
        }

        private void comboBox_shopping_lists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!list_progressing)
            {
                textBox_shopping_list.ForeColor = Color.Black;
                textBox_shopping_list.Text = "";
                textBox_list_name.Text = Convert.ToString(comboBox_shopping_lists.SelectedItem);
                selectTableQuery = @"SELECT id, username, listname, text " +
                                    "FROM shoppinglist WHERE username='" + linkLabel_user.Text + "' " +
                                    "AND listname='" + comboBox_shopping_lists.SelectedItem + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        textBox_list_name.Text = Convert.ToString(dataReader["listname"]);
                        textBox_shopping_list.Text = Convert.ToString(dataReader["text"]);
                    }
                    dataReader.Close();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
        }

        private void button_list_save_Click(object sender, EventArgs e)
        {
            textBox_list_name.Text = textBox_list_name.Text.Replace(" ", "_");
            if (textBox_list_name.Text.Length > 0)
            {
                insertTableQuery = @"INSERT INTO shoppinglist (id, username, listname, text) " +
                                    "SELECT * FROM(SELECT 0, '" + linkLabel_user.Text + "', '" + textBox_list_name.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT listname FROM shoppinglist WHERE listname = '" + textBox_list_name.Text + "' " +
                                    ") LIMIT 2 ;";
                updateTableQuery = @"UPDATE shoppinglist " +
                                     "SET text='" + textBox_shopping_list.Text + "' " +
                                     "WHERE listname='" + textBox_list_name.Text + "' ;";

                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(insertTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(updateTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    readShoppingLists();
                    list_progressing = true;
                    comboBox_shopping_lists.SelectedItem = textBox_list_name.Text;
                    list_progressing = false;
                    MessageBox.Show("Kauppalista tallennettu", "Tallenna");
                    change = "Kauppalista " + textBox_list_name.Text + " tallennettu";
                    addChangeTracking(change);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
            else
            {
                MessageBox.Show("Kauppalistan nimi on virheellinen tai sisältää!", "Tallenna");
            }
        }

        private void textBox_shopping_list_MouseEnter(object sender, EventArgs e)
        {
            if (textBox_shopping_list.Text == info_text)
            {
                textBox_shopping_list.ForeColor = Color.Black;
                textBox_shopping_list.Text = "";
            }
        }

        private void textBox_shopping_list_MouseLeave(object sender, EventArgs e)
        {
            if (textBox_shopping_list.Text == "")
            {
                textBox_shopping_list.ForeColor = Color.Gray;
                textBox_shopping_list.Text = info_text;
            }
        }

        private void button_logo_Click(object sender, EventArgs e)
        {
            view_change = "home";
            viewChange(groupBox_home, button_logo);
        }

        private void textBox_shopping_list_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox_shopping_list.Text == info_text)
            {
                textBox_shopping_list.ForeColor = Color.Black;
                textBox_shopping_list.Text = "";
            }
            textBox_text_length.Text = Convert.ToString(textBox_shopping_list.Text.Length);
        }

        private void textBox_shopping_list_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox_shopping_list.Text == "")
            {
                textBox_shopping_list.ForeColor = Color.Gray;
                textBox_shopping_list.Text = info_text;
            }
            textBox_text_length.Text = Convert.ToString(textBox_shopping_list.Text.Length);
        }

        private void button_list_delete_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen kauppalistan?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                deleteTableQuery = @"DELETE FROM shoppinglist WHERE username='" + linkLabel_user.Text +
                                "' AND listname='" + textBox_list_name.Text + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(deleteTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    textBox_list_name.Text = "";
                    textBox_shopping_list.Text = "";
                    conn.Close();
                    readShoppingLists();
                    comboBox_shopping_lists.SelectedItem = null;
                    MessageBox.Show("Kauppalista on poistettu", "Poista");
                    change = "Kauppalista " + textBox_list_name.Text + " poistettu";
                    addChangeTracking(change);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }   
        }
//----- Change tracking --------------------------------------------------------------------------------------
        private void addChangeTracking(string change)
        {
            DateTime dateValue = DateTime.Now;
            // "2000-01-12 20:10:00Z"    
            dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
            string formatForMySql = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        date DATETIME NOT NULL,
                                        changes VARCHAR(100) NOT NULL);";
            insertTableQuery = @"INSERT INTO change_tracking (id, username, date, changes)" +
                                "VALUES(null, '" + linkLabel_user.Text + "', '" + formatForMySql + "', '" + change + "');";

            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(insertTableQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }
        private void button_change_tracking_Click(object sender, EventArgs e)
        {
            checkDatabaseConnection();
            view_change = "change_tracking";
            viewChange(groupBox_change_tracking, button_change_tracking);
            listView_change_tracking.Clear();
            listView_change_tracking.View = View.Details;
            listView_change_tracking.Columns.Add("Id", 20, HorizontalAlignment.Left);
            listView_change_tracking.Columns.Add("Käyttäjänimi", 20, HorizontalAlignment.Left);
            listView_change_tracking.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
            listView_change_tracking.Columns.Add("Muutos", 20, HorizontalAlignment.Left);
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        date DATETIME NOT NULL,
                                        changes VARCHAR(100) NOT NULL);";
            selectTableQuery = @"SELECT id, username, date, changes " +
                                " FROM change_tracking " +
                                " WHERE username='" + linkLabel_user.Text + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    ListViewItem item = new ListViewItem(new string[]
                    {
                        Convert.ToString(dataReader["id"]),
                        Convert.ToString(dataReader["username"]),
                        Convert.ToString(dataReader["date"]),
                        Convert.ToString(dataReader["changes"])
                    });
                    listView_change_tracking.Items.Add(item);
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_change_tracking.GridLines = true;
            listView_change_tracking.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_change_tracking.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
//----- Top Banner Buttons --------------------------------------------------------------------------------------
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
    }
}
