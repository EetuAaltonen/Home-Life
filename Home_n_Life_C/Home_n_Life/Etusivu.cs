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
using System.Text.RegularExpressions;

namespace Home_n_Life
{
    public partial class Etusivu : Form
    {
        public Etusivu()
        {
            InitializeComponent();
        }

        class User
        {
            public string user_name;
            public string family_key;
            public bool full_permissions;
            public List<string> family_members;
        }

//----- Attributes --------------------------------------------------------------------------------------
        //---Database----------------
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
        bool MySqlConnectionOn;
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader dataReader;
        User user = new User();
        string dropTableQuery, createTableQuery, insertTableQuery, selectTableQuery, updateTableQuery, deleteTableQuery;
        //---Dialog------------------
        DialogResult dialogResult;
        //---listView-Added-Items----
        ListViewItem item;
        //---Check-If-Numeric--------
        int n;
        bool isNumeric;
        //---View-Change-------------
        string view_change, current_view;
        GroupBox current_groupBox;
        Button current_button;
        //---Economic----------------
        double n_euro;
        double income, outlay;
        bool same_name = true;
        //---Menu--------------------
        bool menu_progressing = false;
        int index;
        //---Shopping-List-----------
        string added_item, previous_shopping_list;
        bool list_progressing = false;
        //---Calendar----------------
        bool same_event = true;
        DateTime temp_daytime;
        string formatDateTimeForMySql, month, year;
        string[] months = new string[13];
        //---Athletic-meter----------
        List<Label> addedLabels = new List<Label>();
        List<Point> points;
        List<Tuple<string, double>> values;
        double current_kilometers;
        //---Checklist---------------
        bool checklist_progressing = false;
        string previous_checklist;
        //---Change_tracking---------
        string change;
        bool draw_once;
//----- Etusivu Load --------------------------------------------------------------------------------------
        private void Etusivu_Load(object sender, EventArgs e)
        {
            conn = new MySqlConnection(connetionString);

            months[1] = "Tammikuu"; months[5] = "Toukokuu"; months[9] = "Syyskuu";
            months[2] = "Helmikuu"; months[6] = "Kesäkuu"; months[10] = "Lokakuu";
            months[3] = "Maaliskuu"; months[7] = "Heinäkuu"; months[11] = "Marraskuu";
            months[4] = "Huhtikuu"; months[8] = "Elokuu"; months[12] = "Joulukuu";

            comboBox_search.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox_search.AutoCompleteSource = AutoCompleteSource.ListItems;
            
            view_change = "home";
            viewChange(groupBox_home, button_logo, view_change);
        }

        public void initializeUserData(string user_name)
        {
            linkLabel_user.Text = user_name;
            user.user_name = user_name;

            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                        "FROM users " +
                                        "WHERE username='" + user_name + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    user.family_key = Convert.ToString(dataReader["family_key"]);
                    if (Convert.ToString(dataReader["permissions"]) == "Vanhempi/Isanta")
                    {
                        user.full_permissions = true;
                    }
                    else
                    {
                        user.full_permissions = false;
                        button_economic.Enabled = false;
                        button_economic.Visible = false;
                        button_change_tracking.Enabled = false;
                        button_change_tracking.Visible = false;
                    }
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }

        }

        public void searchFamilyMembers()
        {
            user.family_members = new List<string>();
            listView_family_members.Clear();
            listView_family_members.View = View.Details;
            listView_family_members.Columns.Add("Nimi", 20, HorizontalAlignment.Left);
            listView_family_members.Columns.Add("Oikeudet", 20, HorizontalAlignment.Left);
            listView_family_members.GridLines = true;
            listView_family_members.Items.Clear();
            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                        "FROM users " +
                                        "WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    {
                            Convert.ToString(dataReader["username"]),
                            Convert.ToString(dataReader["permissions"])
                    });
                    listView_family_members.Items.Add(item);
                    user.family_members.Add(Convert.ToString(dataReader["username"]));
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        }

        public void searchThisMonthEvents()
        {
            listView_this_month_events.Clear();
            listView_this_month_events.View = View.Details;
            listView_this_month_events.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
            listView_this_month_events.Columns.Add("Paikka", 20, HorizontalAlignment.Left);
            listView_this_month_events.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
            listView_this_month_events.GridLines = true;
            listView_this_month_events.FullRowSelect = true;
            listView_this_month_events.Items.Clear();
            month = DateTime.Now.ToString(" M ");
            year = DateTime.Now.ToString(" yyyy ");
            createTableQuery = @"CREATE TABLE IF NOT EXISTS calendar (
                                 id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                 username VARCHAR(30) NOT NULL,
                                 event_name VARCHAR(30) NOT NULL,
                                 location VARCHAR(30) NOT NULL,
                                 date DATE NOT NULL,
                                 month VARCHAR(30) NOT NULL,
                                 year INT(6) NOT NULL ) ;";
            selectTableQuery = @"SELECT id, username, event_name, location, date, month, year " +
                                " FROM calendar " +
                                " WHERE username='" + user.user_name + "' AND month='" + months[Int32.Parse(month)] + "' AND year='" + year + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    temp_daytime = (DateTime)dataReader["date"];
                    if (temp_daytime >= DateTime.Now.Date)
                    {
                        item = new ListViewItem(new string[]
                        {
                                Convert.ToString(dataReader["event_name"]),
                                Convert.ToString(dataReader["location"]),
                                temp_daytime.ToString(" d.M.yyyy ")
                        });
                        listView_this_month_events.Items.Add(item);
                    }
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_this_month_events.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_this_month_events.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        }
//----- Database --------------------------------------------------------------------------------------
        public void checkDatabaseConnection()
        {
            MySqlConnectionOn = false;
            progressBar_database_connection.Value = 1;
            progressBar_database_connection.ForeColor = Color.Yellow;
            try
            {
                conn = new MySqlConnection(connetionString);
                conn.Open();
                progressBar_database_connection.Value = 2;
                progressBar_database_connection.ForeColor = Color.LimeGreen;
                MySqlConnectionOn = true;
                conn.Close();
            }
            catch
            {
                progressBar_database_connection.Value = 1;
                progressBar_database_connection.ForeColor = Color.Red;
            }
        }
//----- Change View --------------------------------------------------------------------------------------
        private void viewChange(GroupBox groupBox_change, Button button_change, string view_change)
        {
            checkDatabaseConnection();
            if (MySqlConnectionOn)
            {
                if (view_change != current_view)
                {
                    if (current_groupBox != groupBox_change)
                    {
                        dialogResult = DialogResult.Yes;
                        if (current_view == "menu" || current_view == "shopping_list" || current_view == "checklist")
                        {
                            dialogResult = MessageBox.Show("Muistithan tallentaa keskeneräiset työsi?", "Siirry", MessageBoxButtons.YesNo);
                        }
                        if (dialogResult == DialogResult.Yes)
                        {
                            if (current_button != null)
                            {
                                if (current_button != button_logo)
                                {
                                    current_button.BackColor = Color.DodgerBlue;
                                }
                                if (button_change != button_logo)
                                {
                                    button_change.BackColor = Color.CadetBlue;
                                }
                            }
                            current_button = button_change;
                            if (current_groupBox != null)
                            {
                                current_groupBox.Enabled = false;
                                current_groupBox.Visible = false;
                            }
                            current_groupBox = groupBox_change;
                            current_groupBox.Location = new Point(12, 170);
                            current_groupBox.Size = new Size(967, 518);
                            current_groupBox.Visible = true;
                            current_groupBox.Enabled = true;
                            checkDatabaseConnection();
                            current_view = view_change;
                            switch (current_view)
                            {
                                case "home":
                                    searchFamilyMembers();
                                    searchThisMonthEvents();
                                    break;
                                case "menu":
                                    listView_menu.Clear();
                                    listView_menu.View = View.Details;
                                    listView_menu.Columns.Add("Ruoka", 20, HorizontalAlignment.Left);
                                    listView_menu.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    comboBox_menus.SelectedItem = null;
                                    listView_menu.Items.Clear();
                                    textBox_menu_name.Text = "";
                                    textBox_menu_food.Text = "";
                                    textBox_menu_description.Text = "";
                                    if (!user.full_permissions)
                                    {
                                        button_menu_delete.Visible = false;
                                        button_menu_delete.Enabled = false;
                                        button_menu_remove.Visible = false;
                                        button_menu_remove.Enabled = false;
                                        textBox_menu_name.ReadOnly = true;
                                        button_menu_add.Text = "Ehdota ruokaa";
                                    }
                                    readMenus();
                                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                    break;
                                case "cleaning_shift":

                                    if (!user.full_permissions)
                                    {
                                        listView_cleaning_shift_family_members.Enabled = false;
                                        textBox_cleaning_shift_work.Enabled = false;
                                        button_cleaning_shift_add.Enabled = false;
                                        button_cleaning_shift_add.Visible = false;
                                        button_cleaning_shift_remove.Enabled = false;
                                        button_cleaning_shift_remove.Visible = false;
                                        label_cleaning_shift_info.Text = "Valitse perheenjäsen listalta\n ja tarkastele";
                                    }

                                    listView_cleaning_shift_family_members.Clear();
                                    listView_cleaning_shift_family_members.View = View.Details;
                                    listView_cleaning_shift_family_members.Columns.Add("Perheenjäsen", 20, HorizontalAlignment.Left);

                                    listView_cleaning_shift_list.Clear();
                                    listView_cleaning_shift_list.View = View.Details;
                                    listView_cleaning_shift_list.Columns.Add("Askare", 20, HorizontalAlignment.Left);
                                    listView_cleaning_shift_list.Columns.Add("Perheenjäsen", 20, HorizontalAlignment.Left);

                                    readCleaning();

                                    listView_cleaning_shift_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_cleaning_shift_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                                    listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                    break;
                                case "economic":
                                    listView_income.Clear();
                                    listView_income.View = View.Details;
                                    listView_income.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    listView_income.Columns.Add("Summa", 20, HorizontalAlignment.Left);
                                    listView_income.Columns.Add("Type", 20, HorizontalAlignment.Left);
                                    listView_income.GridLines = true;
                                    listView_income.FullRowSelect = true;

                                    listView_outlay.Clear();
                                    listView_outlay.View = View.Details;
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

                                    readEconomicLists();
                                    break;
                                case "shopping_list":
                                    textBox_list_name.Text = "";
                                    richTextBox_shopping_list.Text = "";
                                    textBox_item_name.Text = "";
                                    textBox_item_amount.Text = "";
                                    comboBox_amount_type.SelectedItem = null;
                                    if (!user.full_permissions)
                                    {
                                        button_list_delete.Visible = false;
                                        button_list_delete.Enabled = false;
                                        richTextBox_shopping_list.ReadOnly = true;
                                        textBox_list_name.ReadOnly = true;
                                        button_add_item.Text = "Ehdota tuotetta";
                                    }
                                    readShoppingLists();
                                    break;
                                case "calendar":
                                    listView_events.Clear();
                                    listView_events.View = View.Details;
                                    listView_events.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    listView_events.Columns.Add("Paikka", 20, HorizontalAlignment.Left);
                                    listView_events.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                                    listView_events.GridLines = true;
                                    listView_events.FullRowSelect = true;
                                    readCalendarLists();
                                    textBox_event_name.Text = "";
                                    textBox_event_location.Text = "";
                                    dateTimePicker_event_datetime.Value = DateTime.Now;

                                    comboBox_event_search_month.SelectedItem = "Koko vuosi";
                                    textBox_event_search_year.Text = "";
                                    dateTimePicker_event_search_datetime.Value = DateTime.Now;
                                    break;
                                case "athletic_meter":
                                    textBox_athletic_year.Text = Convert.ToString(DateTime.Now.Year);
                                    readAthleticMeter();
                                    break;
                                case "checklist":
                                    textBox_checklist_name.Text = "";
                                    textBox_checklist.Text = "";
                                    readChecklists();
                                    break;
                                case "change_tracking":
                                    listView_change_tracking.Clear();
                                    listView_change_tracking.View = View.Details;
                                    listView_change_tracking.Columns.Add("Id", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Käyttäjänimi", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Muutos", 20, HorizontalAlignment.Left);
                                    readChangeTrackingLists();
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Tietokantaan ei saatu yhteyttä\nYritä korjata yhteytesi, jotta voit jatkaa", "Tietokanta");
            }
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            comboBox_search.Text = comboBox_search.Text.Replace(" ", "");
            if (Regex.IsMatch(comboBox_search.Text, @"^[a-zA-Z]+$"))
            {
                if (comboBox_search.Text.Contains("Kala"))
                {
                    view_change = "home";
                    viewChange(groupBox_home, button_logo, view_change);
                }
            }
            else
            {
                MessageBox.Show("Haku ei saa sisältää numeroita tai erikoismerkkejä\nEikä se saa olla tyhjä", "Etsi");
            }
        }

        private void button_logo_Click(object sender, EventArgs e)
        {
            view_change = "home";
            viewChange(groupBox_home, button_logo, view_change);
        }

        private void button_economic_Click(object sender, EventArgs e)
        {
            view_change = "economic";
            viewChange(groupBox_economic, button_economic, view_change);
        }

        private void button_menu_Click(object sender, EventArgs e)
        {
            view_change = "menu";
            viewChange(groupBox_menu, button_menu, view_change);
        }

        private void button_cleaning_shift_Click(object sender, EventArgs e)
        {
            view_change = "cleaning_shift";
            viewChange(groupBox_cleaning_shift, button_cleaning_shift, view_change);
        }

        private void button_shopping_list_Click(object sender, EventArgs e)
        {
            view_change = "shopping_list";
            viewChange(groupBox_shopping_list, button_shopping_list, view_change);
        }

        private void button_calendar_Click(object sender, EventArgs e)
        {
            view_change = "calendar";
            viewChange(groupBox_calendar, button_calendar, view_change);
        }

        private void button_athletic_meter_Click(object sender, EventArgs e)
        {
            view_change = "athletic_meter";
            viewChange(groupBox_athletic_meter, button_athletic_meter, view_change);
        }

        private void button_checklist_Click(object sender, EventArgs e)
        {
            view_change = "checklist";
            viewChange(groupBox_checklist, button_checklist, view_change);
        }

        private void button_change_tracking_Click(object sender, EventArgs e)
        {
            view_change = "change_tracking";
            viewChange(groupBox_change_tracking, button_change_tracking, view_change);
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
                                " WHERE username='" + user.user_name + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    {
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
            listView_income.Columns[2].Width = 0; //Hide type

            listView_outlay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_outlay.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_outlay.Columns[2].Width = 0; //Hide type
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
                    textBox_economic_name.Text = listView_income.SelectedItems[0].SubItems[0].Text;
                    textBox_economic_amount.Text = listView_income.SelectedItems[0].SubItems[1].Text;
                    if (listView_income.SelectedItems[0].SubItems[2].Text == "Tulo")
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
                    textBox_economic_name.Text = listView_outlay.SelectedItems[0].SubItems[0].Text;
                    textBox_economic_amount.Text = listView_outlay.SelectedItems[0].SubItems[1].Text;
                    if (listView_outlay.SelectedItems[0].SubItems[2].Text == "Tulo")
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
                textBox_economic_amount.Text = textBox_economic_amount.Text.Replace(".",",");
                if (textBox_economic_amount.Text != "")
                {
                    isNumeric = double.TryParse(textBox_economic_amount.Text, out n_euro);
                    if (isNumeric)
                    {
                        if (Convert.ToString(comboBox_economic_type.SelectedItem) != "")
                        {
                            same_name = false;
                            if (Convert.ToString(comboBox_economic_type.SelectedItem) == "Tulo")
                            {
                                for (int i = 0; i < listView_income.Items.Count; i++)
                                {
                                    if (textBox_economic_name.Text == listView_income.Items[i].SubItems[0].Text)
                                    {
                                        same_name = true;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < listView_outlay.Items.Count; i++)
                                {
                                    if (textBox_economic_name.Text == listView_outlay.Items[i].SubItems[0].Text)
                                    {
                                        same_name = true;
                                    }
                                }
                            }
                            if (!same_name)
                            {
                                insertTableQuery = @"INSERT INTO economic(id, username, description, type, amount) " +
                                                    "VALUES('null', '" + user.user_name + "', '" + textBox_economic_name.Text + "', '" + Convert.ToString(comboBox_economic_type.SelectedItem) + "', '" + textBox_economic_amount.Text.Replace(",", ".") + "');";
                                try
                                {
                                    conn.Open();
                                    cmd = new MySqlCommand(insertTableQuery, conn);
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
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
                        MessageBox.Show("Syöttämäsi euromäärä ei ole luku", "Lisää");
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
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän tiedon?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM economic WHERE username='" + user.user_name +
                                        "' AND description='" + textBox_economic_name.Text +
                                        "' AND type='" + Convert.ToString(comboBox_economic_type.SelectedItem) + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
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
                MessageBox.Show("Et ole valinnut poistettavaa tuloa tai menoa", "Poista");
            }
        }
//----- Menu --------------------------------------------------------------------------------------
        private void readMenus()
        {
            comboBox_menus.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS menu (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        family_key VARCHAR(30) NOT NULL,
                                        menu_name VARCHAR(30) NOT NULL,
                                        food VARCHAR(30) NOT NULL,
                                        description VARCHAR(30) NOT NULL);";
            selectTableQuery = @"SELECT id, family_key, menu_name, food, description " +
                                " FROM menu " +
                                " WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    same_name = false;
                    foreach (string item in comboBox_menus.Items)
                    {
                        if (item == Convert.ToString(dataReader["menu_name"]))
                        {
                            same_name = true;
                        }
                    }
                    if (!same_name)
                    {
                        comboBox_menus.Items.Add(Convert.ToString(dataReader["menu_name"]));
                    }
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void comboBox_menus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!menu_progressing)
            {
                listView_menu.Items.Clear();
                textBox_menu_name.Text = Convert.ToString(comboBox_menus.SelectedItem);
                selectTableQuery = @"SELECT id, family_key, menu_name, food, description " +
                                    " FROM menu " +
                                    " WHERE family_key='" + user.family_key + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(createTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        item = new ListViewItem(new string[]
                        {
                            Convert.ToString(dataReader["food"]),
                            Convert.ToString(dataReader["description"])
                        });
                        listView_menu.Items.Add(item);
                    }
                    dataReader.Close();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
                listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private void button_menu_add_Click(object sender, EventArgs e)
        {
            if (textBox_menu_food.Text.Length > 0)
            {
                if (!user.full_permissions)
                {
                    textBox_menu_description.Text += " (ehdotettu)";
                }
                item = new ListViewItem(new string[]
                {
                        textBox_menu_food.Text,
                        textBox_menu_description.Text
                });
                textBox_menu_description.Text = textBox_menu_description.Text.Replace(" (ehdotettu)", "");
                listView_menu.Items.Add(item);
                listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                MessageBox.Show("Uusi ruoka lisätty ruokalistaan", "Lisää");
            }
            else
            {
                MessageBox.Show("Syötä ruuan nimi", "Lisää");
            }
        }

        private void button_menu_remove_Click(object sender, EventArgs e)
        {
            if (listView_menu.SelectedIndices.Count > 0)
            {
                menu_progressing = true;
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän ruuan?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    listView_menu.Items.Remove(listView_menu.SelectedItems[0]);
                    textBox_menu_food.Text = "";
                    textBox_menu_description.Text = "";
                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    MessageBox.Show("Ruoka on poistettu ruokalistalta", "Poista");
                }
                menu_progressing = false;
            }
            else
            {
                MessageBox.Show("Et ole valinnut poistettavaa ruokaa", "Poista");
            }
        }

        private void button_menu_save_Click(object sender, EventArgs e)
        {
            textBox_menu_name.Text = textBox_menu_name.Text.Replace(" ", "_");
            if (textBox_menu_name.Text.Length > 0)
            {
                if (listView_menu.Items.Count > 0)
                {
                    deleteTableQuery = @"DELETE FROM menu " +
                                        "WHERE menu_name='" + textBox_menu_name.Text + "' AND family_key='" + user.family_key + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        for (index = 0; index < listView_menu.Items.Count; index++)
                        {
                            insertTableQuery = @"INSERT INTO menu (id, family_key, menu_name, food, description) " +
                                        "VALUES(null, '" + user.family_key + "', '" + textBox_menu_name.Text + "', '" + listView_menu.Items[index].SubItems[0].Text + "', '" + listView_menu.Items[index].SubItems[1].Text + "' );";
                            cmd = new MySqlCommand(insertTableQuery, conn);
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                        readMenus();
                        menu_progressing = true;
                        comboBox_menus.SelectedItem = textBox_menu_name.Text;
                        menu_progressing = false;
                        MessageBox.Show("Ruokalista tallennettu", "Tallenna");
                        change = "Ruokalista " + textBox_menu_name.Text + " tallennettu";
                        addChangeTracking(change);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
                else
                {
                    MessageBox.Show("Ruokalistalla ei ole ruokia", "Tallenna");
                }
            }
            else
            {
                MessageBox.Show("Ruokalistan nimi on virheellinen", "Tallenna");
            }
        }

        private void button_menu_delete_Click(object sender, EventArgs e)
        {
            if (comboBox_menus.SelectedItem != null)
            {
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen ruokalistan?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM menu " +
                                        "WHERE menu_name='" + textBox_menu_name.Text + "' AND family_key='" + user.family_key + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        readMenus();
                        comboBox_menus.SelectedItem = null;
                        listView_menu.Items.Clear();
                        textBox_menu_name.Text = "";
                        textBox_menu_food.Text = "";
                        textBox_menu_description.Text = "";
                        MessageBox.Show("Ruokalista poistettu", "Poista");
                        change = "Ruokalista " + textBox_menu_name.Text + " poistettu";
                        addChangeTracking(change);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
            else
            {
                MessageBox.Show("Et ole valinnut poistettavaa ruokalistaa", "Poista");
            }
        }

        private void listView_menu_Click(object sender, EventArgs e)
        {
            textBox_menu_food.Text = listView_menu.SelectedItems[0].SubItems[0].Text;
            textBox_menu_description.Text = listView_menu.SelectedItems[0].SubItems[1].Text.Replace(" (ehdotettu)", "");
        }
//----- Cleaning --------------------------------------------------------------------------------------
        private void readCleaning()
        {
            listView_cleaning_shift_list.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS cleaning_shift (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            family_key VARCHAR(30) NOT NULL,
                                            description VARCHAR(30) NOT NULL,
                                            worker VARCHAR(30) NOT NULL);";
            selectTableQuery = @"SELECT id, family_key, description, worker " +
                                " FROM cleaning_shift " +
                                " WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    {
                            Convert.ToString(dataReader["description"]),
                            Convert.ToString(dataReader["worker"])
                    });
                    listView_cleaning_shift_list.Items.Add(item);
                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            listView_cleaning_shift_family_members.Items.Clear();
            foreach (string member in user.family_members)
            {
                listView_cleaning_shift_family_members.Items.Add(member);
            }
        }

        private void listView_cleaning_shift_family_members_Click(object sender, EventArgs e)
        {
            if (listView_cleaning_shift_list.SelectedIndices.Count > 0)
            {
                listView_cleaning_shift_list.SelectedItems[0].Selected = false;
            }
            if (listView_cleaning_shift_family_members.SelectedIndices.Count > 0)
            {
                if (listView_cleaning_shift_family_members.SelectedIndices.Count == 1)
                {
                    textBox_cleaning_shift_worker.Text = listView_cleaning_shift_family_members.SelectedItems[0].SubItems[0].Text;
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Menot");
                }
            }
        }

        private void listView_cleaning_shift_list_Click(object sender, EventArgs e)
        {
            if (listView_cleaning_shift_family_members.SelectedIndices.Count > 0)
            {
                listView_cleaning_shift_family_members.SelectedItems[0].Selected = false;
            }
            if (listView_cleaning_shift_list.SelectedIndices.Count > 0)
            {
                if (listView_cleaning_shift_list.SelectedIndices.Count == 1)
                {
                    textBox_cleaning_shift_work.Text = listView_cleaning_shift_list.SelectedItems[0].SubItems[0].Text;
                    textBox_cleaning_shift_worker.Text = listView_cleaning_shift_list.SelectedItems[0].SubItems[1].Text;
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Menot");
                }
            }
        }

        private void button_cleaning_shift_add_Click(object sender, EventArgs e)
        {
            if (textBox_cleaning_shift_worker.Text != "")
            {
                if (textBox_cleaning_shift_work.Text != "")
                {
                    insertTableQuery = @"INSERT INTO cleaning_shift(id, family_key, description, worker) " +
                                        "VALUES('null', '" + user.family_key + "', '" + textBox_cleaning_shift_work.Text + "', '" + textBox_cleaning_shift_worker.Text + "');";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(insertTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        change = "Siivousvuoroihin lisätty " + textBox_cleaning_shift_work.Text;
                        addChangeTracking(change);
                        textBox_cleaning_shift_work.Text = "";
                        textBox_cleaning_shift_worker.Text = "";
                        readCleaning();
                        MessageBox.Show("Uusi askare lisätty", "Lisää");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
                else
                {
                    MessageBox.Show("Syötä askare", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Valitse perheenjäsen vasemmalta listalta", "Lisää");
            }
        }

        private void button_cleaning_shift_remove_Click(object sender, EventArgs e)
        {
            if (listView_cleaning_shift_list.SelectedIndices.Count == 1)
            {
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän tiedon?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM cleaning_shift WHERE family_key='" + user.family_key +
                                        "' AND description='" + listView_cleaning_shift_list.SelectedItems[0].SubItems[0].Text +
                                        "' AND worker='" + listView_cleaning_shift_list.SelectedItems[0].SubItems[1].Text + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        change = "Siivousvuoroista poistettu " + textBox_cleaning_shift_work.Text;
                        addChangeTracking(change);
                        textBox_cleaning_shift_worker.Text = "";
                        textBox_cleaning_shift_work.Text = "";
                        readCleaning();
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
                MessageBox.Show("Valitse askare listalta", "Poista");
            }
        }
//----- Shopping list --------------------------------------------------------------------------------------
        private void readShoppingLists()
        {
            comboBox_shopping_lists.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS shoppinglist (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        family_key VARCHAR(30) NOT NULL,
                                        listname VARCHAR(30) NOT NULL,
                                        text TEXT(1000) NOT NULL);";
            selectTableQuery = @"SELECT id, family_key, listname, text " +
                                "FROM shoppinglist " +
                                "WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    comboBox_shopping_lists.Items.Add(Convert.ToString(dataReader["listname"]));
                }
                dataReader.Close();
                conn.Close();
                previous_shopping_list = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void button_add_item_Click(object sender, EventArgs e)
        {
            if (textBox_item_name.Text.Length > 0)
            {
                added_item = "> " + textBox_item_name.Text + "  ";
                added_item += textBox_item_amount.Text + " " + comboBox_amount_type.SelectedItem;
                if (!user.full_permissions)
                {
                    added_item += " (" + user.user_name + " ehdottama)";
                }
                added_item += System.Environment.NewLine;
                if ((Int32.Parse(textBox_text_length.Text) + added_item.Length) < 1000)
                {
                    richTextBox_shopping_list.Text += added_item;
                }
                else
                {
                    MessageBox.Show("Et voi lisätä enempää tuotteita listalle", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Syötä tuotteen nimi", "Tuotteen lisääminen");
            }
        }

        private void richTextBox_shopping_list_TextChanged(object sender, EventArgs e)
        {
            textBox_text_length.Text = Convert.ToString(richTextBox_shopping_list.Text.Length);
            if (Int32.Parse(textBox_text_length.Text) > 1000)
            {
                MessageBox.Show("Et voi lisätä enempää tuotteita listalle", "Lisää");
                if (previous_shopping_list != "")
                {
                    richTextBox_shopping_list.Text = previous_shopping_list;
                    textBox_text_length.Text = Convert.ToString(richTextBox_shopping_list.Text.Length);
                }
            }
            previous_shopping_list = richTextBox_shopping_list.Text;
        }

        private void comboBox_shopping_lists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!list_progressing)
            {
                richTextBox_shopping_list.Text = "";
                textBox_list_name.Text = Convert.ToString(comboBox_shopping_lists.SelectedItem);
                selectTableQuery = @"SELECT id, family_key, listname, text " +
                                    "FROM shoppinglist WHERE family_key='" + user.family_key + "' " +
                                    "AND listname='" + comboBox_shopping_lists.SelectedItem + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        textBox_list_name.Text = Convert.ToString(dataReader["listname"]);
                        richTextBox_shopping_list.Text = Convert.ToString(dataReader["text"]);
                    }
                    dataReader.Close();
                    conn.Close();
                    previous_shopping_list = "";
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
                insertTableQuery = @"INSERT INTO shoppinglist (id, family_key, listname, text) " +
                                    "SELECT * FROM(SELECT 0, '" + user.family_key + "', '" + textBox_list_name.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT family_key, listname FROM shoppinglist WHERE family_key='" + user.family_key + "' AND listname='" + textBox_list_name.Text + "' " +
                                    ") LIMIT 2 ;";
                updateTableQuery = @"UPDATE shoppinglist " +
                                     "SET text='" + richTextBox_shopping_list.Text + "' " +
                                     "WHERE listname='" + textBox_list_name.Text + "' AND family_key='" + user.family_key + "' ;";

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
                    MessageBox.Show("Kauppalappu tallennettu", "Tallenna");
                    change = "Kauppalappu " + textBox_list_name.Text + " tallennettu";
                    addChangeTracking(change);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
            else
            {
                MessageBox.Show("Kauppalapun nimi on virheellinen", "Tallenna");
            }
        }

        private void button_list_delete_Click(object sender, EventArgs e)
        {
            if (comboBox_shopping_lists.SelectedItem != null)
            {
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen kauppalapun?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM shoppinglist WHERE family_key='" + user.family_key +
                                    "' AND listname='" + textBox_list_name.Text + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Kauppalappu on poistettu", "Poista");
                        change = "Kauppalappu " + textBox_list_name.Text + " poistettu";
                        addChangeTracking(change);
                        textBox_list_name.Text = "";
                        richTextBox_shopping_list.Text = "";
                        comboBox_shopping_lists.SelectedItem = null;
                        comboBox_amount_type.SelectedItem = null;
                        textBox_item_name.Text = "";
                        textBox_item_amount.Text = "";
                        readShoppingLists();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
            else
            {
                MessageBox.Show("Et ole valinnut poistettavaa kauppalappua", "Poista");
            }
        }

        private void button_shopping_list_save_as_file_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "*.txt";
            saveFile.Filter = "Tekstitiedostot (*.txt)|*.txt";

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
            saveFile.FileName.Length > 0)
            {
                richTextBox_shopping_list.SaveFile(saveFile.FileName, RichTextBoxStreamType.PlainText);
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
                                 date DATE NOT NULL,
                                 month VARCHAR(30) NOT NULL,
                                 year INT(6) NOT NULL ) ;";
            selectTableQuery = @"SELECT id, username, event_name, location, date, month, year " +
                                " FROM calendar " +
                                " WHERE username='" + user.user_name + "';";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    temp_daytime = (DateTime)dataReader["date"];
                    if (checkBox_event_show_past.Checked)
                    {
                        item = new ListViewItem(new string[]
                        {
                                Convert.ToString(dataReader["event_name"]),
                                Convert.ToString(dataReader["location"]),
                                temp_daytime.ToString("d/M/yyyy")
                        });
                        listView_events.Items.Add(item);
                    }
                    else
                    {
                        if (temp_daytime.Date >= DateTime.Now.Date)
                        {
                            item = new ListViewItem(new string[]
                            {
                                    Convert.ToString(dataReader["event_name"]),
                                    Convert.ToString(dataReader["location"]),
                                    temp_daytime.ToString("d/M/yyyy")
                            });
                            listView_events.Items.Add(item);
                        }
                    }
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
        }

        private void searchCalendarLists(string selectTableQuery_)
        {
            selectTableQuery = selectTableQuery_;
            listView_events.Items.Clear();
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    temp_daytime = (DateTime)dataReader["date"];
                    if (checkBox_event_show_past.Checked)
                    {
                        item = new ListViewItem(new string[]
                        {
                                Convert.ToString(dataReader["event_name"]),
                                Convert.ToString(dataReader["location"]),
                                temp_daytime.ToString("d/M/yyyy")
                        });
                        listView_events.Items.Add(item);
                    }
                    else
                    {
                        if (temp_daytime.Date >= DateTime.Now.Date)
                        {
                            item = new ListViewItem(new string[]
                            {
                                    Convert.ToString(dataReader["event_name"]),
                                    Convert.ToString(dataReader["location"]),
                                    temp_daytime.ToString("d/M/yyyy")
                            });
                            listView_events.Items.Add(item);
                        }
                    }
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
        }

        private void setSearchMonth(ref string month)
        {
            switch (Convert.ToString(comboBox_event_search_month.SelectedItem))
            {
                case "Tammikuu": month = "1"; break;
                case "Helmikuu": month = "2"; break;
                case "Maaliskuu": month = "3"; break;
                case "Huhtikuu": month = "4"; break;
                case "Toukokuu": month = "5"; break;
                case "Kesäkuu": month = "6"; break;
                case "Heinäkuu": month = "7"; break;
                case "Elokuu": month = "8"; break;
                case "Syyskuu": month = "9"; break;
                case "Lokakuu": month = "10"; break;
                case "Marraskuu": month = "11"; break;
                case "Joulukuu": month = "12"; break;
            }
        }

        private void listView_events_Click(object sender, EventArgs e)
        {
            if (listView_events.SelectedIndices.Count > 0)
            {
                if (listView_events.SelectedIndices.Count == 1)
                {
                    textBox_event_name.Text = listView_events.SelectedItems[0].SubItems[0].Text;
                    textBox_event_location.Text = listView_events.SelectedItems[0].SubItems[1].Text;
                    dateTimePicker_event_datetime.Value = DateTime.Parse(listView_events.SelectedItems[0].SubItems[2].Text);
                }
                else
                {
                    MessageBox.Show("Valitse vain yksi kerrallaan", "Tapahtumat");
                }
            }
        }

        private void button_event_add_Click(object sender, EventArgs e)
        {
            if (dateTimePicker_event_datetime.Value.Date >= DateTime.Now.Date)
            {
                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("d/M/yyyy");
                if (textBox_event_name.Text != "")
                {
                    if (textBox_event_location.Text != "")
                    {
                        if (Convert.ToString(dateTimePicker_event_datetime.Value) != "")
                        {
                            same_event = false;
                            for (int i = 0; i < listView_events.Items.Count; i++)
                            {
                                if (textBox_event_name.Text == listView_events.Items[i].SubItems[0].Text &&
                                    formatDateTimeForMySql == listView_events.Items[i].SubItems[2].Text)
                                {
                                    same_event = true;
                                }
                            }
                            if (!same_event)
                            {
                                month = dateTimePicker_event_datetime.Value.ToString(" MM ");
                                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd");
                                insertTableQuery = @"INSERT INTO calendar (id, username, event_name, location, date, month, year)" +
                                                    "VALUES(null, '" + user.user_name + "', '" + textBox_event_name.Text +
                                                    "', '" + textBox_event_location.Text + "', '" + formatDateTimeForMySql +
                                                    "', '" + months[Int32.Parse(month)] + "', '" + dateTimePicker_event_datetime.Value.ToString("yyyy") + "');";
                                try
                                {
                                    conn.Open();
                                    cmd = new MySqlCommand(insertTableQuery, conn);
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                    MessageBox.Show("Uusi tapahtuma lisätty", "Lisää");
                                    formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("d/M/yyyy");
                                    readCalendarLists();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(Convert.ToString(ex));
                                }
                            }
                            else
                            {
                                MessageBox.Show("Samanniminen tapahtuma samalle päivälle on jo olemassa", "Lisää");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Et ole valinnut päivää", "Lisää");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Et ole syöttänyt tapahtumapaikkaa", "Lisää");
                    }
                }
                else
                {
                    MessageBox.Show("Et ole syöttänyt tapahtuman kuvausta", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Et voi lisätä tapahtumaa mennelle ajalle", "Lisää");
            }
        }

        private void button_event_set_today_Click(object sender, EventArgs e)
        {
            dateTimePicker_event_datetime.Value = DateTime.Now;
        }

        private void button_event_search_set_today_Click(object sender, EventArgs e)
        {
            dateTimePicker_event_search_datetime.Value = DateTime.Now;
        }

        private void button_event_delete_past_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Haluatko varmasti poistaa menneet tapahtumat?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                deleteTableQuery = @"DELETE FROM calendar WHERE username='" + user.user_name +
                                        "' AND date<'" + DateTime.Now.ToString("yyyy-MM-dd") + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(deleteTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Menneet tapahtumat on poistettu", "Poista");

                    readCalendarLists();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
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
                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd");
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen tapahtuman?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    deleteTableQuery = @"DELETE FROM calendar WHERE username='" + user.user_name +
                                        "' AND event_name='" + textBox_event_name.Text +
                                        "' AND date='" + formatDateTimeForMySql + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Tapahtuma on poistettu", "Poista");
                        formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("d/M/yyyy");
                        textBox_event_name.Text = "";
                        textBox_event_location.Text = "";
                        dateTimePicker_event_datetime.Value = DateTime.Now;
                        readCalendarLists();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
        }

        private void button_event_search_1_Click(object sender, EventArgs e)
        {
            if (Convert.ToString(comboBox_event_search_month.SelectedItem) != "Koko vuosi" &&
            textBox_event_search_year.Text != "")
            {
                isNumeric = int.TryParse(textBox_event_search_year.Text, out n);
                if (isNumeric)
                {
                    if (int.Parse(textBox_event_search_year.Text) < DateTime.Now.Year)
                    {
                        checkBox_event_show_past.Checked = true;
                    }
                    month = dateTimePicker_event_datetime.Value.ToString(" MM ");
                    setSearchMonth(ref month);
                    selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                        " FROM calendar " +
                                        " WHERE username='" + user.user_name +
                                        "' AND month='" + months[Int32.Parse(month)] +
                                        "' AND year=" + Int32.Parse(textBox_event_search_year.Text) + " ;";
                    searchCalendarLists(selectTableQuery);
                }
                else
                {
                    MessageBox.Show("Syöttämäsi vuosi ei ole vuosiluku", "Hae");
                }
            }
            else if (Convert.ToString(comboBox_event_search_month.SelectedItem) != "Koko vuosi")
            {
                month = Convert.ToString(comboBox_event_search_month.SelectedItem);
                setSearchMonth(ref month);
                selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                    " FROM calendar " +
                                    " WHERE username='" + user.user_name +
                                    "' AND month='" + months[Int32.Parse(month)] + "' ;";
                searchCalendarLists(selectTableQuery);
            }
            else if (textBox_event_search_year.Text != "")
            {
                isNumeric = int.TryParse(textBox_event_search_year.Text, out n);
                if (isNumeric)
                {
                    if (int.Parse(textBox_event_search_year.Text) < DateTime.Now.Year)
                    {
                        checkBox_event_show_past.Checked = true;
                    }
                    selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                        " FROM calendar " +
                                        " WHERE username='" + user.user_name +
                                        "' AND year=" + Int32.Parse(textBox_event_search_year.Text) + " ;";
                    searchCalendarLists(selectTableQuery);
                }
                else
                {
                    MessageBox.Show("Syöttämäsi vuosi ei ole vuosiluku", "Hae");
                }
            }
            else
            {
                readCalendarLists();
            }
        }

        private void button_clear_search_settings_Click(object sender, EventArgs e)
        {
            comboBox_event_search_month.SelectedItem = "Koko vuosi";
            textBox_event_search_year.Text = "";
        }

        private void button_event_search_2_Click(object sender, EventArgs e)
        {
            formatDateTimeForMySql = dateTimePicker_event_search_datetime.Value.ToString("yyyy-MM-dd");
            selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                " FROM calendar " +
                                " WHERE username='" + user.user_name +
                                "' AND date='" + formatDateTimeForMySql + "' ;";
            searchCalendarLists(selectTableQuery);
        }
//----- Athletic meter --------------------------------------------------------------------------------------
        private void readAthleticMeter()
        {
            if (addedLabels.Count > 0)
            {
                int listCount = addedLabels.Count;
                for (int i = 0; i < listCount; i++)
                {
                    Label removeLabel = addedLabels[0];
                    addedLabels.Remove(removeLabel);
                    panel_athletic_statistics.Controls.Remove(removeLabel);
                }
            }
            month = DateTime.Now.ToString(" M ");
            textBox_athletic_month.Text = months[Int32.Parse(month)];
            year = DateTime.Now.ToString(" yyyy ");
            textBox_athletic_year.Text = textBox_athletic_year.Text.Replace(" ", "");
            if (textBox_athletic_year.Text.Length > 0)
            {

                year = textBox_athletic_year.Text;
            }
            else
            {
                textBox_athletic_year.Text = year.Replace(" ", ""); ;
            }
            createTableQuery = @"CREATE TABLE IF NOT EXISTS athletic_meter (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            month VARCHAR(100) NOT NULL,
                                            year VARCHAR(5) NOT NULL,
                                            amount DOUBLE(10,2) NOT NULL);";
            selectTableQuery = @"SELECT id, username, month, year, amount " +
                                " FROM athletic_meter " +
                                " WHERE username='" + user.user_name +
                                "' AND month='" + months[Int32.Parse(month)] +
                                "' AND year=" + Int32.Parse(year) + " ;";
            try
            {
                values = new List<Tuple<string, double>>();
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    textBox_athletic_kilometers.Text = Convert.ToString(dataReader["amount"]);
                }
                dataReader.Close();
                selectTableQuery = @"SELECT id, username, month, year, amount " +
                                " FROM athletic_meter " +
                                " WHERE username='" + user.user_name +
                                "' AND year=" + Int32.Parse(year) + " ;";
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                double largest = 0;
                double zoom = 0;
                while (dataReader.Read())
                {
                    if (double.Parse(Convert.ToString(dataReader["amount"])) >= largest)
                    {
                        largest = double.Parse(Convert.ToString(dataReader["amount"]));
                    }
                    values.Add(Tuple.Create(Convert.ToString(dataReader["month"]), double.Parse(Convert.ToString(dataReader["amount"]))));
                }
                zoom = largest / 240;
                dataReader.Close();
                conn.Close();

                addedLabels = new List<Label>();
                Label label;
                for (int i = 1; i < months.Length; i++)
                {
                    label = new Label();
                    label.Text = months[i];
                    label.Size = new Size(60, 20);
                    label.BackColor = System.Drawing.Color.Transparent;
                    label.Location = new Point((80 * (i - 1)) + 5, 270);
                    panel_athletic_statistics.Controls.Add(label);
                    addedLabels.Insert(0, label);
                }
                Label labelValue;
                for (int i = 1; i < 13; i++)
                {
                    double tempValue = 0;
                    foreach (var value in values)
                    {
                        if (value.Item1.Equals(months[i]))
                        {
                            tempValue = value.Item2;
                        }
                    }
                    if (i == 1)
                    {
                        points = new List<Point>();
                        if (tempValue > 0)
                        {
                            if (270 - Convert.ToInt32(tempValue / zoom) <= 240)
                            {
                                points.Add(new Point(30, 260 - Convert.ToInt32(tempValue / zoom)));
                            }
                            else
                            {
                                points.Add(new Point(30, 250));
                            }
                        }
                        else
                        {
                            points.Add(new Point(30, 250));
                        }
                        points.Add(new Point(30, 250));
                        labelValue = new Label();
                        labelValue.Text = Convert.ToString(tempValue);
                        labelValue.Size = new Size(60, 20);
                        labelValue.BackColor = System.Drawing.Color.Transparent;
                        if (tempValue > 0)
                        {
                            if (240 - Convert.ToInt32(tempValue / zoom) <= 210)
                            {
                                labelValue.Location = new Point(20, 250 - Convert.ToInt32(tempValue / zoom));
                            }
                            else
                            {
                                labelValue.Location = new Point(20, 220);
                            }
                        }
                        else
                        {
                            labelValue.Location = new Point(20, 220);
                        }
                        panel_athletic_statistics.Controls.Add(labelValue);
                        addedLabels.Insert(0, labelValue);
                    }
                    else
                    {
                        points.Add(new Point(30 + ((i - 1) * 80), 250));
                        if (tempValue > 0)
                        {
                            if (270 - Convert.ToInt32(tempValue / zoom) <= 240)
                            {
                                points.Add(new Point(30 + ((i - 1) * 80), 270 - Convert.ToInt32(tempValue / zoom)));
                            }
                            else
                            {
                                points.Add(new Point(30 + ((i - 1) * 80), 250));
                            }
                        }
                        else
                        {
                            points.Add(new Point(30 + ((i - 1) * 80), 250));
                        }
                        points.Add(new Point(30 + ((i - 1) * 80), 250));

                        labelValue = new Label();
                        labelValue.Text = Convert.ToString(tempValue);
                        labelValue.Size = new Size(60, 20);
                        labelValue.BackColor = System.Drawing.Color.Transparent;
                        if (tempValue > 0)
                        {
                            if (240 - Convert.ToInt32(tempValue / zoom) <= 210)
                            {
                                labelValue.Location = new Point(20 + ((i - 1) * 80), 250 - Convert.ToInt32(tempValue / zoom));
                            }
                            else
                            {
                                labelValue.Location = new Point(20 + ((i - 1) * 80), 220);
                            }
                        }
                        else
                        {
                            labelValue.Location = new Point(20 + ((i - 1) * 80), 220);
                        }
                        panel_athletic_statistics.Controls.Add(labelValue);
                        addedLabels.Insert(0, labelValue);
                    }
                }
                draw_once = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void button_athletic_add_kilometers_Click(object sender, EventArgs e)
        {
            current_kilometers = 0;
            if ((double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text)) >= 0)
            {
                current_kilometers = (double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text));
            }
            textBox_athletic_year.Text = "";
            month = DateTime.Now.ToString(" M ");
            year = DateTime.Now.ToString(" yyyy ");
            insertTableQuery = @"INSERT INTO athletic_meter (id, username, month, year, amount) " +
                                "SELECT * FROM(SELECT -1, '" + user.user_name + "', '" + months[Int32.Parse(month)] + "', " + Int32.Parse(year) + ", " + Convert.ToString(current_kilometers) +  ") AS tmp " +
                                "WHERE NOT EXISTS( " +
                                "SELECT username, month, year FROM athletic_meter WHERE username='" + user.user_name + "' AND month='" + months[Int32.Parse(month)] + "' AND year=" + Int32.Parse(year) +
                                ") LIMIT 2 ;";
            updateTableQuery = @"UPDATE athletic_meter " +
                                 "SET amount='" + Convert.ToString(current_kilometers) + "' " +
                                 "WHERE username='" + user.user_name + "' AND month='" + months[Int32.Parse(month)] + "' AND year="+ Int32.Parse(year) + " ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(insertTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(updateTableQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                readAthleticMeter();
                MessageBox.Show("Kilometrit lisätty", "Lisää");
                textBox_athletic_add_kilometers.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void panel_athletic_statistics_Paint(object sender, PaintEventArgs e)
        {
            if (draw_once)
            {
                panel_athletic_statistics.Refresh();
                Pen pen = new Pen(Color.Black, 10);
                Point[] points_ = points.ToArray();
                e.Graphics.DrawLines(pen, points_);
                draw_once = false;
            }
        }

        private void button_athletic_search_Click(object sender, EventArgs e)
        {
            readAthleticMeter();
        }
//----- Checklist --------------------------------------------------------------------------------------
        private void readChecklists()
        {
            comboBox_checklists.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS checklist (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        checklist_name VARCHAR(30) NOT NULL,
                                        text TEXT(2000) NOT NULL);";
            selectTableQuery = @"SELECT id, username, checklist_name, text " +
                                " FROM checklist " +
                                " WHERE username='" + user.user_name + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    comboBox_checklists.Items.Add(Convert.ToString(dataReader["checklist_name"]));
                }
                dataReader.Close();
                conn.Close();
                previous_checklist = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void comboBox_checklists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!checklist_progressing)
            {
                textBox_checklist.Text = "";
                textBox_checklist_name.Text = Convert.ToString(comboBox_shopping_lists.SelectedItem);
                selectTableQuery = @"SELECT id, username, checklist_name, text " +
                                    "FROM checklist WHERE username='" + user.user_name + "' " +
                                    "AND checklist_name='" + comboBox_checklists.SelectedItem + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        textBox_checklist_name.Text = Convert.ToString(dataReader["checklist_name"]);
                        textBox_checklist.Text = Convert.ToString(dataReader["text"]);
                    }
                    dataReader.Close();
                    conn.Close();
                    previous_checklist = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
        }

        private void button_checklist_save_Click(object sender, EventArgs e)
        {
            textBox_checklist_name.Text = textBox_checklist_name.Text.Replace(" ", "_");
            if (textBox_checklist_name.Text.Length > 0)
            {
                createTableQuery = @"CREATE TABLE IF NOT EXISTS checklist (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            checklist_name VARCHAR(30) NOT NULL,
                                            text VARCHAR(1000) NOT NULL);";
                insertTableQuery = @"INSERT INTO checklist (id, username, checklist_name, text) " +
                                    "SELECT * FROM(SELECT 0, '" + user.user_name + "', '" + textBox_checklist_name.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT username, checklist_name FROM checklist WHERE username='" + user.user_name + "' AND checklist_name='" + textBox_checklist_name.Text + "' " +
                                    ") LIMIT 2 ;";
                updateTableQuery = @"UPDATE checklist " +
                                     "SET text='" + textBox_checklist.Text + "' " +
                                     "WHERE checklist_name='" + textBox_checklist_name.Text + "' ;";

                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(createTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(insertTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(updateTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    readChecklists();
                    checklist_progressing = true;
                    comboBox_checklists.SelectedItem = textBox_checklist_name.Text;
                    checklist_progressing = false;
                    MessageBox.Show("Muistilista tallennettu", "Tallenna");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
            else
            {
                MessageBox.Show("Muistilistan nimi on virheellinen", "Tallenna");
            }
        }

        private void button_checklist_delete_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen muistilistan?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                deleteTableQuery = @"DELETE FROM checklist WHERE username='" + user.user_name +
                                "' AND checklist_name='" + textBox_checklist_name.Text + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(deleteTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();                    
                    MessageBox.Show("Muistilista on poistettu", "Poista");
                    textBox_checklist_name.Text = "";
                    textBox_checklist.Text = "";
                    comboBox_checklists.SelectedItem = null;
                    readChecklists();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
        }

        private void textBox_checklist_TextChanged(object sender, EventArgs e)
        {
            textBox_checklist_text_length.Text = Convert.ToString(textBox_checklist.Text.Length);
            if (Int32.Parse(textBox_checklist_text_length.Text) > 2000)
            {
                MessageBox.Show("Et voi lisätä enempää tektiä muistilistalle", "Lisää");
                if (previous_checklist != "")
                {
                    textBox_checklist.Text = previous_checklist;
                    textBox_checklist_text_length.Text = Convert.ToString(textBox_checklist.Text.Length);
                }
            }
            previous_checklist = textBox_checklist.Text;
        }
//----- Change tracking --------------------------------------------------------------------------------------
        private void readChangeTrackingLists()
        {
            listView_change_tracking.Items.Clear();
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            family_key VARCHAR(30) NOT NULL,
                                            date DATETIME NOT NULL,
                                            changes VARCHAR(100) NOT NULL);";
            selectTableQuery = @"SELECT id, username, family_key, date, changes " +
                                " FROM change_tracking " +
                                " WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
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

        private void addChangeTracking(string change)
        {
            DateTime dateValue = DateTime.Now;
            // "2000-01-12 20:10:00Z"    
            dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
            formatDateTimeForMySql = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        family_key VARCHAR(30) NOT NULL,
                                        date DATETIME NOT NULL,
                                        changes VARCHAR(100) NOT NULL);";
            insertTableQuery = @"INSERT INTO change_tracking (id, username, family_key, date, changes)" +
                                "VALUES(null, '" + user.user_name + "', '" + user.family_key + "', '" + formatDateTimeForMySql + "', '" + change + "');";

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

        private void button_clear_change_tracking_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Haluatko varmasti tyhjentää tiedot muutoksista?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                dropTableQuery = @"DROP TABLE IF EXISTS change_tracking ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(dropTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
                readChangeTrackingLists();
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
