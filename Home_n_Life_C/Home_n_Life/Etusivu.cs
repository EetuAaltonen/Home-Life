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
        string dropTableQuery, createTableQuery, insertTableQuery, selectTableQuery, updateTableQuery, deleteTableQuery, alterTableQuery;
        //---View-Change-------------
        string view_change;
        GroupBox current_groupBox;
        Button current_button;
        //---Shopping-List-----------
        string info_text, added_item;
        bool list_progressing;
        //---Change_tracking---------
        string change;
        //---Economic----------------
        double income, outlay;
        bool same_name = true;
        //---Calendar----------------
        bool same_event = true;

//----- Etusivu Load --------------------------------------------------------------------------------------
        private void Etusivu_Load(object sender, EventArgs e)
        {
            view_change = "home";
            conn = new MySqlConnection(connetionString);
            current_button = button_logo;
            viewChange(groupBox_home, button_logo);
        }

        private void button_logo_Click(object sender, EventArgs e)
        {
            if (view_change != "home")
            {
                view_change = "home";
                viewChange(groupBox_home, button_logo);
            }
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
                        readShoppingLists();
                        break;
                    case "change_tracking":
                        listView_change_tracking.Clear();
                        listView_change_tracking.View = View.Details;
                        listView_change_tracking.Columns.Add("Id", 20, HorizontalAlignment.Left);
                        listView_change_tracking.Columns.Add("Käyttäjänimi", 20, HorizontalAlignment.Left);
                        listView_change_tracking.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                        listView_change_tracking.Columns.Add("Muutos", 20, HorizontalAlignment.Left);
                        break;
                    case "economic":
                        listView_income.Clear();
                        listView_income.View = View.Details;
                        listView_income.Columns.Add("Id", 20, HorizontalAlignment.Left);
                        listView_income.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                        listView_income.Columns.Add("Summa", 20, HorizontalAlignment.Left);
                        listView_income.Columns.Add("Type", 20, HorizontalAlignment.Left);
                        listView_income.GridLines = true;
                        listView_income.FullRowSelect = true;

                        listView_outlay.Clear();
                        listView_outlay.View = View.Details;
                        listView_outlay.Columns.Add("Id", 20, HorizontalAlignment.Left);
                        listView_outlay.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                        listView_outlay.Columns.Add("Summa", 20, HorizontalAlignment.Left);
                        listView_outlay.Columns.Add("Type", 20, HorizontalAlignment.Left);
                        listView_outlay.GridLines = true;
                        listView_outlay.FullRowSelect = true;

                        textBox_economic_name.Text = "";
                        textBox_economic_amount.Text = "";
                        comboBox_economic_type.SelectedIndex = -1;
                        textBox_all_income.Text = "";
                        textBox_all_outlay.Text = "";
                        textBox_balance.Text = "";
                        break;
                    case "calendar":
                        listView_events.Clear();
                        listView_events.View = View.Details;
                        listView_events.Columns.Add("Id", 20, HorizontalAlignment.Left);
                        listView_events.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                        listView_events.Columns.Add("Paikka", 20, HorizontalAlignment.Left);
                        listView_events.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                        listView_events.GridLines = true;
                        listView_events.FullRowSelect = true;
                        readCalendarLists();
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
        private void readEconomicLists()
        {
            income = 0;
            outlay = 0;
            listView_income.Items.Clear();
            listView_outlay.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS economic (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            description VARCHAR(100) NOT NULL,
                                            type VARCHAR(5) NOT NULL,
                                            amount DOUBLE(10,2) NOT NULL);";
            selectTableQuery = @"SELECT id, username, description, type, amount " +
                                " FROM economic " +
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
                            Convert.ToString(dataReader["description"]),
                            Convert.ToString(dataReader["amount"]),
                            Convert.ToString(dataReader["type"])
                    });
                    if (Convert.ToString(dataReader["type"]) == "Tulo")
                    {
                        income += double.Parse(Convert.ToString(dataReader["amount"]));
                        listView_income.Items.Add(item);
                    }
                    else if (Convert.ToString(dataReader["type"]) == "Meno")
                    {
                        outlay += double.Parse(Convert.ToString(dataReader["amount"]));
                        listView_outlay.Items.Add(item);
                    }
                    string euro = Encoding.Default.GetString(new byte[] { 128 }); //€
                    textBox_all_income.Text = Convert.ToString(income) + " " + euro;
                    textBox_all_outlay.Text = Convert.ToString(outlay) + " " + euro;
                    textBox_balance.Text = Convert.ToString((income - outlay) + " " + euro);
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_income.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_income.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_income.Columns[0].Width = 0; //Hide id
            listView_income.Columns[3].Width = 0; //Hide type

            listView_outlay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_outlay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_outlay.Columns[0].Width = 0; //Hide id
            listView_outlay.Columns[3].Width = 0; //Hide type
        }

        private void button_economic_Click(object sender, EventArgs e)
        {
            if (view_change != "economic")
            {
                checkDatabaseConnection();
                view_change = "economic";
                viewChange(groupBox_economic, button_economic);
                readEconomicLists();
            }
        }

        private void listView_income_Click(object sender, EventArgs e)
        {
            if (listView_outlay.SelectedIndices.Count > 0)
            {
                listView_outlay.SelectedItems[0].Selected = false;
            }
            if (listView_income.SelectedIndices.Count > 0)
            {
                if (listView_income.SelectedIndices.Count == 1)
                {
                    textBox_economic_name.Text = listView_income.SelectedItems[0].SubItems[1].Text;
                    textBox_economic_amount.Text = listView_income.SelectedItems[0].SubItems[2].Text;
                    if (listView_income.SelectedItems[0].SubItems[3].Text == "Tulo")
                    {
                        comboBox_economic_type.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox_economic_type.SelectedIndex = 1;
                    }
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Menot");
                }
            }
        }

        private void listView_outlay_Click(object sender, EventArgs e)
        {
            if (listView_income.SelectedIndices.Count > 0)
            {
                listView_income.SelectedItems[0].Selected = false;
            }
            if (listView_outlay.SelectedIndices.Count > 0)
            {
                if (listView_outlay.SelectedIndices.Count == 1)
                {
                    textBox_economic_name.Text = listView_outlay.SelectedItems[0].SubItems[1].Text;
                    textBox_economic_amount.Text = listView_outlay.SelectedItems[0].SubItems[2].Text;
                    if (listView_outlay.SelectedItems[0].SubItems[3].Text == "Tulo")
                    {
                        comboBox_economic_type.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox_economic_type.SelectedIndex = 1;
                    }
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Tulot");
                }
            }
        }

        private void button_economic_add_Click(object sender, EventArgs e)
        {
            if (textBox_economic_name.Text != "")
            {
                if (textBox_economic_amount.Text != "")
                {
                    if (Convert.ToString(comboBox_economic_type.SelectedItem) != "")
                    {
                        same_name = false;
                        if (Convert.ToString(comboBox_economic_type.SelectedItem) == "Tulo")
                        {
                            for (int i = 0; i < listView_income.Items.Count; i++)
                            {
                                if (textBox_economic_name.Text == listView_income.Items[i].SubItems[1].Text)
                                {
                                    same_name = true;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < listView_outlay.Items.Count; i++)
                            {
                                if (textBox_economic_name.Text == listView_outlay.Items[i].SubItems[1].Text)
                                {
                                    same_name = true;
                                }
                            }
                        }
                        if (!same_name)
                        {
                            insertTableQuery = @"INSERT INTO economic(id, username, description, type, amount) " +
                                                "VALUES('null', '" + linkLabel_user.Text + "', '" + textBox_economic_name.Text + "', '" + Convert.ToString(comboBox_economic_type.SelectedItem) + "', '" + textBox_economic_amount.Text + "');";
                            try
                            {
                                conn.Open();
                                cmd = new MySqlCommand(insertTableQuery, conn);
                                cmd.ExecuteNonQuery();
                                conn.Close();
                                change = "Talouteen lisätty uusi " + Convert.ToString(comboBox_economic_type.SelectedItem).ToLower() + " " + textBox_economic_name.Text;
                                addChangeTracking(change);
                                textBox_economic_name.Text = "";
                                textBox_economic_amount.Text = "";
                                comboBox_economic_type.SelectedIndex = -1;
                                listView_income.Sorting = SortOrder.Ascending;
                                readEconomicLists();
                                MessageBox.Show("Uusi listaus lisätty", "Lisää");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Convert.ToString(ex));
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nimi on jo olemassa", "Lisää");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Valitse tyyppi", "Lisää");
                    }
                }
                else
                {
                    MessageBox.Show("Syötä euro määrä", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Syötä nimi tai kuvaus", "Lisää");
            }
        }

        private void button_economic_remove_Click(object sender, EventArgs e)
        {
            if ((listView_income.SelectedIndices.Count == 1 && listView_outlay.SelectedIndices.Count == 0) ||
                (listView_outlay.SelectedIndices.Count == 1 && listView_income.SelectedIndices.Count == 0))
            {
                DialogResult dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän tiedon?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM economic WHERE username='" + linkLabel_user.Text +
                                        "' AND description='" + textBox_economic_name.Text +
                                        "' AND type='" + Convert.ToString(comboBox_economic_type.SelectedItem) + "' ;";
                    alterTableQuery = @"ALTER TABLE economic DROP COLUMN id;
                                        ALTER TABLE economic ADD COLUMN id BIGINT UNSIGNED DEFAULT 1 PRIMARY KEY FIRST ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        cmd = new MySqlCommand(alterTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        change = "Taloudesta poistettu " + Convert.ToString(comboBox_economic_type.SelectedItem).ToLower() + " " + textBox_economic_name.Text;
                        addChangeTracking(change);
                        textBox_economic_name.Text = "";
                        textBox_economic_amount.Text = "";
                        comboBox_economic_type.SelectedIndex = -1;
                        readEconomicLists();
                        MessageBox.Show("Listaus poistettu", "Poista");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
            else
            {
                MessageBox.Show("Valitse yksi tulo tai meno", "Poista");
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
            if (view_change != "shopping_list");
            {
                checkDatabaseConnection();
                view_change = "shopping_list";
                viewChange(groupBox_shopping_list, button_shopping_list);
                textBox_list_name.Text = "";
            }
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
                MessageBox.Show("Kauppalistan nimi on virheellinen tai sisältää", "Tallenna");
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

//----- Calendar --------------------------------------------------------------------------------------
        private void readCalendarLists()
        {
            listView_events.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS calendar (
                                 id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                 username VARCHAR(30) NOT NULL,
                                 event_name VARCHAR(30) NOT NULL,
                                 location VARCHAR(30) NOT NULL,
                                 date DATETIME NOT NULL) ;";
            selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                " FROM calendar " +
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
                            Convert.ToString(dataReader["event_name"]),
                            Convert.ToString(dataReader["location"]),
                            Convert.ToString(dataReader["date"])
                    });
                    listView_events.Items.Add(item);
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_events.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_events.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_events.Columns[0].Width = 0; //Hide id
        }

        private void listView_events_Click(object sender, EventArgs e)
        {
            if (listView_events.SelectedIndices.Count > 0)
            {
                if (listView_events.SelectedIndices.Count == 1)
                {
                    textBox_event_name.Text = listView_events.SelectedItems[0].SubItems[1].Text;
                    textBox_event_location.Text = listView_events.SelectedItems[0].SubItems[2].Text;
                    dateTimePicker_event_datetime.Value = DateTime.Parse(listView_events.SelectedItems[0].SubItems[3].Text);
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Tapahtumat");
                }
            }
        }

        private void button_calendar_Click(object sender, EventArgs e)
        {
            if (view_change != "calendar")
            {
                checkDatabaseConnection();
                view_change = "calendar";
                viewChange(groupBox_calendar, button_calendar);
            }
        }

        private void button_event_add_Click(object sender, EventArgs e)
        {
            DateTime dateValue = dateTimePicker_event_datetime.Value;
            // "2000-01-12 20:10:00Z"    
            dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
            string formatForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (textBox_event_name.Text != "")
            {
                if (textBox_event_location.Text != "")
                {
                    if (Convert.ToString(dateTimePicker_event_datetime.Value) != "")
                    {
                        same_event = false;
                        for (int i = 0; i < listView_events.Items.Count; i++)
                        {
                            if (textBox_event_name.Text == listView_events.Items[i].SubItems[1].Text &&
                                formatForMySql == listView_events.Items[i].SubItems[3].Text)
                            {
                                same_event = true;
                            }
                        }
                        if (!same_event)
                        {
                            insertTableQuery = @"INSERT INTO calendar (id, username, event_name, location, date)" +
                                                "VALUES(null, '" + linkLabel_user.Text + "', '" + textBox_event_name.Text + "', '" + textBox_event_location.Text + "', '" + formatForMySql + "');";
                            try
                            {
                                conn.Open();
                                cmd = new MySqlCommand(insertTableQuery, conn);
                                cmd.ExecuteNonQuery();
                                conn.Close();
                                MessageBox.Show("Uusi tapahtuma lisätty", "Lisää");
                                //change = "Tapahtuma " + textBox_event_name.Text + " lisätty kalenteriin";
                                //addChangeTracking(change);
                                readCalendarLists();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Convert.ToString(ex));
                            }
                        }
                    }
                }
            }
        }

        private void button_event_delete_Click(object sender, EventArgs e)
        {
            if (listView_events.SelectedIndices.Count == 1)
            {
                DateTime dateValue = dateTimePicker_event_datetime.Value;
                // "2000-01-12 20:10:00Z"    
                dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
                string formatForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                alterTableQuery = @"ALTER TABLE calendar DROP COLUMN id;
                                            ALTER TABLE calendar ADD COLUMN id BIGINT UNSIGNED DEFAULT 1 PRIMARY KEY FIRST ;";
                DialogResult dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen tapahtuman?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM calendar WHERE username='" + linkLabel_user.Text +
                                        "' AND event_name='" + textBox_event_name.Text + "' ;";
                                        //"' AND date='" + formatForMySql + "' ;";
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
                        MessageBox.Show("Tapahtuma on poistettu", "Poista");
                        //change = "Kauppalista " + textBox_list_name.Text + " poistettu";
                        //addChangeTracking(change);
                        readCalendarLists();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
        }

        private void button_event_search_Click(object sender, EventArgs e)
        {

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
            if (view_change != "change_tracking")
            {
                checkDatabaseConnection();
                view_change = "change_tracking";
                viewChange(groupBox_change_tracking, button_change_tracking);
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
