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
        //Käyttäjä-luokka
        class User
        {
            //Attribuutit
            public string user_name; //Käyttäjänimi
            public string family_key; //Perheavain
            public bool full_permissions; //Oikeudet
            public List<string> family_members; //Perheenjäsenet
        }

//----- Attributes --------------------------------------------------------------------------------------
        //---Database----------------
        //Tietokantayhteyden osoite
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
        //Tietokantayhteyden tila (päällä=true/katkennut=false)
        bool MySqlConnectionOn;
        //MySql syntaksit
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader dataReader;
        //User-luokan olio
        User user = new User();
        //Tietokantaan tehtävien komentojen muuttujat
        string dropTableQuery, createTableQuery, insertTableQuery, selectTableQuery, updateTableQuery, deleteTableQuery;
        //---Dialog------------------
        DialogResult dialogResult; //YES/NO - dialogi
        string user_info; //Käyttäjän tiedot, kun painetaan nimeä oikealla yläkulmassa
        //---Seach-------------------
        bool result_found; //Etsi-toiminnon tila, löytyikö hakusanalla
        //---listView-Added-Items----
        ListViewItem item; //Informaatio, joka lisätään listoihin
        //---Check-If-Numeric--------
        int n; //Edustaa numeroita
        bool isNumeric; //Onko numeerinen
        //---View-Change-------------
        string view_change, current_view; //Näkymänvaihdon muuttujat
        GroupBox current_groupBox; //Nykyinen näkymä
        Button current_button; //Painettu nappula
        //---Economic----------------
        double n_euro; //Edustaa numeroita (dobule)
        double income, outlay; //Tulo ja meno muuttujat
        bool same_name = true; //Onko saman niminen informaatio
        //---Menu--------------------
        bool menu_progressing = false; //Onko prosessi käynnissä
        int index; //Ruokalista (listView_menu.Items[index])
        //---Shopping-List-----------
        string added_item, previous_shopping_list; //Lisättävä tuote ja edellinen sisältö
        bool list_progressing = false; //Onko prosessi käynnissä
        //---Calendar----------------
        bool same_event = true; //Onko saman niminen tapahtuma
        DateTime temp_daytime; //Käsiteltävä päivämäärä (temp_daytime = (DateTime)dataReader["date"])
        string formatDateTimeForMySql, month, year; //Tapahtuman päivämäärän osat
        string[] months = new string[13]; //Lista kuukausista (months[1] = "Tammikuu";)
        //---Athletic-meter----------
        List<Label> addedLabels = new List<Label>(); //Tulostettavat numerot ja kuukauden nimet
        List<Point> points; //Piirrettävien pylväiden pisteet
        List<Tuple<string, double>> values; //Kuukausien kilometrimäärät (values[Tammikuu, 200])
        double current_kilometers; //Nykyisen kuukauden kilometrit
        bool draw_once; //Piirtää pylväät vain kerran
        //---Checklist---------------
        bool checklist_progressing = false; //Onko prosessi käynnissä
        string previous_checklist; //Edellinen sisältö
        //---Change_tracking---------
        string change; //Kirjattava muutos
//----- Etusivu Load --------------------------------------------------------------------------------------
        private void Etusivu_Load(object sender, EventArgs e)
        {
            conn = new MySqlConnection(connetionString);
            //Alustaa kuukaudet
            months[1] = "Tammikuu"; months[5] = "Toukokuu"; months[9] = "Syyskuu";
            months[2] = "Helmikuu"; months[6] = "Kesäkuu"; months[10] = "Lokakuu";
            months[3] = "Maaliskuu"; months[7] = "Heinäkuu"; months[11] = "Marraskuu";
            months[4] = "Huhtikuu"; months[8] = "Elokuu"; months[12] = "Joulukuu";
            //Asettaa haku-kentälle ennustavan hakutuloksen
            comboBox_search.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox_search.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        public void initializeUserData(string user_name)
        {
            //Alustaa käyttäjän tiedot
            linkLabel_user.Text = user_name;
            user.user_name = user_name;
            //Valitaan tiedot taulukosta
            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                        "FROM users " +
                                        "WHERE username='" + user_name + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                //Luetaan käyttäjätiedot tietokannasta
                while (dataReader.Read())
                {
                    //Perheavain
                    user.family_key = Convert.ToString(dataReader["family_key"]);
                    //Tarkistaa, onko täydet oikeudet
                    if (Convert.ToString(dataReader["permissions"]) == "Vanhempi/Isanta")
                    {
                        user.full_permissions = true;
                    }
                    else
                    {
                        user.full_permissions = false;
                        //Piilottaa Etusivun nappuloita
                        button_economic.Enabled = false;
                        button_economic.Visible = false;
                        button_change_tracking.Enabled = false;
                        button_change_tracking.Visible = false;
                    }
                }
                dataReader.Close();
                conn.Close();
                //Näkymäksi astetaan Etusivu
                view_change = "home";
                viewChange(groupBox_home, button_logo, view_change);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }

        }

        public void searchFamilyMembers()
        {
            //Etsii peheenjäsenet
            user.family_members = new List<string>();
            listView_family_members.Clear();
            listView_family_members.View = View.Details;
            listView_family_members.Columns.Add("Nimi", 20, HorizontalAlignment.Left);
            listView_family_members.Columns.Add("Oikeudet", 20, HorizontalAlignment.Left);
            listView_family_members.GridLines = true;
            listView_family_members.Items.Clear();
            //Valitaan tiedot taulukosta
            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                        "FROM users " +
                                        "WHERE family_key='" + user.family_key + "' ;";
            try
            {
                conn.Open();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                //Luetaan perheenjäsenet tietokannasta
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    {
                            Convert.ToString(dataReader["username"]),
                            Convert.ToString(dataReader["permissions"])
                    });
                    //Lisätään listalle
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
            //Etsii kuukauden tapahtumat
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
            //Luodaan uusi taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS calendar (
                                 id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                 username VARCHAR(30) NOT NULL,
                                 event_name VARCHAR(30) NOT NULL,
                                 location VARCHAR(30) NOT NULL,
                                 date DATE NOT NULL,
                                 month VARCHAR(30) NOT NULL,
                                 year INT(6) NOT NULL ) ;";
            //Valitaan tiedot taulukosta
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
                //Luetaan tapahtumat tietokannasta
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
                        //Lisätään listalle
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
            //Tarkistetaan yhteys tietokantaan
            MySqlConnectionOn = false;
            progressBar_database_connection.Value = 1;
            progressBar_database_connection.ForeColor = Color.Yellow;
            try
            {
                //Avataan yhteys tietokantaan
                conn = new MySqlConnection(connetionString);
                conn.Open();
                //Yhteys-mittari asetetaan vihreäksi ja valmis-tilaan
                progressBar_database_connection.Value = 2;
                progressBar_database_connection.ForeColor = Color.LimeGreen;
                MySqlConnectionOn = true;
                conn.Close();
            }
            catch
            {
                //Jos ei saada yhteyttä
                //Yhteys-mittari asetetaan punaiselle ja error-tilaan
                progressBar_database_connection.Value = 1;
                progressBar_database_connection.ForeColor = Color.Red;
            }
        }
//----- Change View --------------------------------------------------------------------------------------
        private void viewChange(GroupBox groupBox_change, Button button_change, string view_change)
        {
            //Näkymän vaihto
            //Tarkistaa yhteyden tietokantaan
            checkDatabaseConnection();
            if (MySqlConnectionOn)
            {
                //Tarkistaa, onko valittu näkymä eri kuin nykyinen
                if (view_change != current_view)
                {
                    if (current_groupBox != groupBox_change)
                    {
                        //Kysyy tietyistä näkymistä lähdettäessä, onko työt tallennetu
                        dialogResult = DialogResult.Yes;
                        if (current_view == "menu" || current_view == "shopping_list" || current_view == "checklist")
                        {
                            dialogResult = MessageBox.Show("Muistithan tallentaa keskeneräiset työsi?", "Siirry", MessageBoxButtons.YesNo);
                        }
                        if (dialogResult == DialogResult.Yes)
                        {
                            //Tarkistaa avataanko näkymiä ensimmmäistä kertaa
                            if (current_button != null)
                            {
                                //Varmistaa, etti logo-nappulan väri muutu
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
                            //Tarkistaa avataanko näkymiä ensimmmäistä kertaa
                            if (current_groupBox != null)
                            {
                                current_groupBox.Enabled = false;
                                current_groupBox.Visible = false;
                            }
                            //Vaihtaa näkymää
                            current_groupBox = groupBox_change;
                            current_groupBox.Location = new Point(12, 170);
                            current_groupBox.Size = new Size(967, 518);
                            current_groupBox.Visible = true;
                            current_groupBox.Enabled = true;
                            //Tarkistaa yhteyden tietokantaan
                            checkDatabaseConnection();
                            current_view = view_change;
                            switch (current_view)
                            {
                                //Etusivu
                                case "home":
                                    label_welcome.Text = "Tevetuloa " + user.user_name + " !";
                                    //Listaa oikeudet
                                    treeView_home_permissions.Nodes.Clear();
                                    treeView_home_permissions.Nodes.Add(new TreeNode("Näkymät ja hallinta"));
                                    treeView_home_permissions.Nodes.Add(new TreeNode("Toiminnot"));
                                    treeView_home_permissions.Nodes[1].Nodes.Add(new TreeNode("Henkilökohtaisten listojen ja tietojen luominen, muokkaaminen, poistaminen"));
                                    if (user.full_permissions)
                                    {
                                        treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Talous"));
                                        treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Muutosseuranta"));
                                        treeView_home_permissions.Nodes[1].Nodes.Add(new TreeNode("Yhteisten listojen ja tietojen luominen, muokkaaminen, poistaminen"));
                                    }
                                    else
                                    {
                                        treeView_home_permissions.Nodes[1].Nodes.Add(new TreeNode("Yhteisten listojen ja tietojen ehdottaminen"));
                                    }
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Ruokalista"));
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Siivousvuorot"));
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Kauppalappu"));
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Kalenteri"));
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Liikuntamittari"));
                                    treeView_home_permissions.Nodes[0].Nodes.Add(new TreeNode("Muistilistat"));
                                    treeView_home_permissions.ExpandAll();
                                    //Lisää haku-kenttaan toimivat hakusanat
                                    comboBox_search.Items.Clear();
                                    if (user.full_permissions)
                                    {
                                        comboBox_search.Items.AddRange(new string[]
                                        {
                                            "Etusivu", "Talous", "Ruokalista", "Siivousvuorot", "Kauppalappu",
                                            "Kalenteri", "Liikuntamittari", "Muistilistat", "Muutosseuranta"

                                        });
                                    }
                                    else
                                    {
                                        comboBox_search.Items.AddRange(new string[]
                                        {
                                            "Etusivu", "Ruokalista", "Siivousvuorot", "Kauppalappu",
                                            "Kalenteri", "Liikuntamittari", "Muistilistat"

                                        });
                                    }
                                    searchFamilyMembers();
                                    searchThisMonthEvents();
                                    break;
                                //Talous
                                case "economic":
                                    //Alustaa Tulot-listan
                                    listView_income.Clear();
                                    listView_income.View = View.Details;
                                    listView_income.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    listView_income.Columns.Add("Summa", 20, HorizontalAlignment.Left);
                                    listView_income.Columns.Add("Type", 20, HorizontalAlignment.Left);
                                    listView_income.GridLines = true;
                                    listView_income.FullRowSelect = true;
                                    //Alustaa Menot-listan
                                    listView_outlay.Clear();
                                    listView_outlay.View = View.Details;
                                    listView_outlay.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    listView_outlay.Columns.Add("Summa", 20, HorizontalAlignment.Left);
                                    listView_outlay.Columns.Add("Type", 20, HorizontalAlignment.Left);
                                    listView_outlay.GridLines = true;
                                    listView_outlay.FullRowSelect = true;
                                    //Tyhjentää lomakkeen
                                    textBox_economic_name.Text = "";
                                    textBox_economic_amount.Text = "";
                                    comboBox_economic_type.SelectedIndex = -1;
                                    textBox_all_income.Text = "";
                                    textBox_all_outlay.Text = "";
                                    textBox_balance.Text = "";
                                    textBox_economic_name.Select();
                                    //Hakee menot ja tulot tietokannasta
                                    readEconomicLists();
                                    break;
                                //Ruokalista
                                case "menu":
                                    //Alustaa Ruokalistan
                                    listView_menu.Clear();
                                    listView_menu.View = View.Details;
                                    listView_menu.Columns.Add("Ruoka", 20, HorizontalAlignment.Left);
                                    listView_menu.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    comboBox_menus.SelectedItem = null;
                                    listView_menu.Items.Clear();
                                    textBox_menu_name.Text = "";
                                    textBox_menu_food.Text = "";
                                    textBox_menu_description.Text = "";
                                    //Tarkistaa, onko kaikki oikeudet
                                    if (!user.full_permissions)
                                    {
                                        //Piilottaa Ruokalistan nappuloita
                                        button_menu_delete.Visible = false;
                                        button_menu_delete.Enabled = false;
                                        button_menu_remove.Visible = false;
                                        button_menu_remove.Enabled = false;
                                        textBox_menu_name.ReadOnly = true;
                                        button_menu_add.Visible = false;
                                        button_menu_add.Enabled = false;
                                        button_menu_add.Text = "Ehdota ruokaa";
                                        button_menu_save.Visible = false;
                                        button_menu_save.Enabled = false;
                                    }
                                    comboBox_menus.Select();
                                    //Hakee ruokalistat
                                    readMenus();
                                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_menu.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                    break;
                                //Siivousvuorot
                                case "cleaning_shift":
                                    //Tarkistaa, onko kaikki oikeudet
                                    if (!user.full_permissions)
                                    {
                                        //Piilottaa Siivousvuoron nappuloita
                                        listView_cleaning_shift_family_members.Enabled = false;
                                        textBox_cleaning_shift_work.Enabled = false;
                                        button_cleaning_shift_add.Enabled = false;
                                        button_cleaning_shift_add.Visible = false;
                                        button_cleaning_shift_remove.Enabled = false;
                                        button_cleaning_shift_remove.Visible = false;
                                        label_cleaning_shift_info.Text = "Valitse perheenjäsen listalta\n ja tarkastele";
                                    }
                                    //Alustaa Perheenjäsenet-listan
                                    listView_cleaning_shift_family_members.Clear();
                                    listView_cleaning_shift_family_members.View = View.Details;
                                    listView_cleaning_shift_family_members.Columns.Add("Perheenjäsen", 20, HorizontalAlignment.Left);
                                    //Alustaa askare-listan
                                    listView_cleaning_shift_list.Clear();
                                    listView_cleaning_shift_list.View = View.Details;
                                    listView_cleaning_shift_list.Columns.Add("Askare", 20, HorizontalAlignment.Left);
                                    listView_cleaning_shift_list.Columns.Add("Perheenjäsen", 20, HorizontalAlignment.Left);

                                    textBox_cleaning_shift_work.Select();
                                    //Hakee siivousvuorot
                                    readCleaning();

                                    listView_cleaning_shift_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_cleaning_shift_family_members.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                                    listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                    listView_cleaning_shift_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                    break;
                                //Kauppalappu
                                case "shopping_list":
                                    //Tyhjentää lomakkeen
                                    textBox_list_name.Text = "";
                                    richTextBox_shopping_list.Text = "";
                                    textBox_item_name.Text = "";
                                    textBox_item_amount.Text = "";
                                    comboBox_amount_type.SelectedItem = null;
                                    //Tarkitaa, onko kaikki oikeudet
                                    if (!user.full_permissions)
                                    {
                                        //Piilottaa Kauppalapun nappuloita
                                        button_list_delete.Visible = false;
                                        button_list_delete.Enabled = false;
                                        richTextBox_shopping_list.ReadOnly = true;
                                        textBox_list_name.ReadOnly = true;
                                        button_add_item.Visible = false;
                                        button_add_item.Enabled = false;
                                        button_add_item.Text = "Ehdota tuotetta";
                                        button_list_save.Visible = false;
                                        button_list_save.Enabled = false;
                                        button_shopping_list_save_as_file.Visible = false;
                                        button_shopping_list_save_as_file.Enabled = false;
                                    }
                                    comboBox_shopping_lists.Select();
                                    //Hakee kauppalaput
                                    readShoppingLists();
                                    break;
                                //Kalenteri
                                case "calendar":
                                    //Alustaa tapahtumat-listan
                                    listView_events.Clear();
                                    listView_events.View = View.Details;
                                    listView_events.Columns.Add("Kuvaus", 20, HorizontalAlignment.Left);
                                    listView_events.Columns.Add("Paikka", 20, HorizontalAlignment.Left);
                                    listView_events.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                                    listView_events.GridLines = true;
                                    listView_events.FullRowSelect = true;
                                    //Hakee tapahtumat
                                    readCalendarLists();
                                    //Tyhjentää lomakkeen
                                    textBox_event_name.Text = "";
                                    textBox_event_location.Text = "";
                                    dateTimePicker_event_datetime.Value = DateTime.Now;

                                    comboBox_event_search_month.SelectedItem = "Koko vuosi";
                                    textBox_event_search_year.Text = "";
                                    dateTimePicker_event_search_datetime.Value = DateTime.Now;
                                    textBox_event_name.Select();
                                    break;
                                //Liikuntamittari
                                case "athletic_meter":
                                    //Tyhjentää lomakkeen
                                    textBox_athletic_year.Text = Convert.ToString(DateTime.Now.Year);
                                    textBox_athletic_add_kilometers.Select();
                                    //Hakee kilometrit
                                    readAthleticMeter();
                                    break;
                                //Muistilista
                                case "checklist":
                                    //Tyhjentää lomakkeen
                                    textBox_checklist_name.Text = "";
                                    textBox_checklist.Text = "";
                                    comboBox_checklists.Select();
                                    //Hakee muistilistat
                                    readChecklists();
                                    break;
                                //Muutosseuranta
                                case "change_tracking":
                                    //Alutaa muutosseuranta-listan
                                    listView_change_tracking.Clear();
                                    listView_change_tracking.View = View.Details;
                                    listView_change_tracking.Columns.Add("Id", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Käyttäjänimi", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Päivä", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Columns.Add("Muutos", 20, HorizontalAlignment.Left);
                                    listView_change_tracking.Select();
                                    //Hakee muutokset
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
            //Haku
            comboBox_search.Text = comboBox_search.Text.Replace(" ", ""); //Poistaa syötetystä hakusanasta välit
            //Tarkistaa, ettei hakusana sisällä erikoismerkkejä
            if (Regex.IsMatch(comboBox_search.Text, @"^[a-zA-Z]+$"))
            {
                //Tarkistaa, löytyykö hakusanalla tuloksia
                result_found = false;
                foreach (string result in comboBox_search.Items)
                {
                    if (result == comboBox_search.Text)
                    {
                        result_found = true;
                    }
                }
                if (result_found)
                {
                    //Vaihtaa näkymää hakusanan mukaan
                    switch (comboBox_search.Text)
                    {
                        case "Etusivu":view_change = "home";viewChange(groupBox_home, button_logo, view_change);
                            break;
                        case "Talous":
                            view_change = "economic";viewChange(groupBox_economic, button_economic, view_change);
                            break;
                        case "Ruokalista":
                            view_change = "menu";
                            viewChange(groupBox_menu, button_menu, view_change);
                            break;
                        case "Siivousvuorot":
                            view_change = "cleaning_shift";
                            viewChange(groupBox_cleaning_shift, button_cleaning_shift, view_change);
                            break;
                        case "Kauppalappu":
                            view_change = "shopping_list";
                            viewChange(groupBox_shopping_list, button_shopping_list, view_change);
                            break;
                        case "Kalenteri":
                            view_change = "calendar";
                            viewChange(groupBox_calendar, button_calendar, view_change);
                            break;
                        case "Liikuntamittari":
                            view_change = "athletic_meter";
                            viewChange(groupBox_athletic_meter, button_athletic_meter, view_change);
                            break;
                        case "Muistilistat":
                            view_change = "checklist";
                            viewChange(groupBox_checklist, button_checklist, view_change);
                            break;
                        case "Muutosseuranta":
                            view_change = "change_tracking";
                            viewChange(groupBox_change_tracking, button_change_tracking, view_change);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Syöttämälläsi hakusanalla ei löytynyt tuloksia", "Haku");
                }
            }
            else
            {
                MessageBox.Show("Haku ei saa sisältää numeroita tai erikoismerkkejä\nEikä se saa olla tyhjä", "Etsi");
            }
        }

        private void button_logo_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Etusivu
            view_change = "home";
            viewChange(groupBox_home, button_logo, view_change);
        }

        private void linkLabel_user_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Tulostaa käyttäjätiedot
            user_info = "Käyttäjänimi: " + user.user_name;
            if (user.full_permissions)
            {
                user_info += "\nKäyttäjätyyppi: Vanhempi/Isäntä";
            }
            else
            {
                user_info += "\nKäyttäjätyyppi: Lapsi/Asukas";
            }
            user_info += "\nPerheavain: " + user.family_key;
            MessageBox.Show(user_info,"Käyttäjätiedot");
        }

        private void button_economic_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Talouden
            view_change = "economic";
            viewChange(groupBox_economic, button_economic, view_change);
        }

        private void button_menu_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Ruokalistan
            view_change = "menu";
            viewChange(groupBox_menu, button_menu, view_change);
        }

        private void button_cleaning_shift_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Siivousvuorot
            view_change = "cleaning_shift";
            viewChange(groupBox_cleaning_shift, button_cleaning_shift, view_change);
        }

        private void button_shopping_list_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Kauppalapun
            view_change = "shopping_list";
            viewChange(groupBox_shopping_list, button_shopping_list, view_change);
        }

        private void button_calendar_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Kalenterin
            view_change = "calendar";
            viewChange(groupBox_calendar, button_calendar, view_change);
        }

        private void button_athletic_meter_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Liikuntamittarin
            view_change = "athletic_meter";
            viewChange(groupBox_athletic_meter, button_athletic_meter, view_change);
        }

        private void button_checklist_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Muistilistan
            view_change = "checklist";
            viewChange(groupBox_checklist, button_checklist, view_change);
        }

        private void button_change_tracking_Click(object sender, EventArgs e)
        {
            //Asettaa näkymäksi Muutosseurannan
            view_change = "change_tracking";
            viewChange(groupBox_change_tracking, button_change_tracking, view_change);
        }
//----- Economic --------------------------------------------------------------------------------------
        private void readEconomicLists()
        {
            //Luetaan tulot ja menot
            income = 0;
            outlay = 0;
            listView_income.Items.Clear();
            listView_outlay.Items.Clear();
            //Luodaan uusi taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS economic (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            description VARCHAR(100) NOT NULL,
                                            type VARCHAR(5) NOT NULL,
                                            amount DOUBLE(10,2) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan tulot ja menot tietokannasta
                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    {
                            Convert.ToString(dataReader["description"]),
                            Convert.ToString(dataReader["amount"]),
                            Convert.ToString(dataReader["type"])
                    });
                    //Lisätään ne omiin listoihinsa
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
            //Valitaan tuloa listalta
            if (listView_outlay.SelectedIndices.Count > 0)
            {
                listView_outlay.SelectedItems[0].Selected = false;
            }
            if (listView_income.SelectedIndices.Count > 0)
            {
                if (listView_income.SelectedIndices.Count == 1)
                {
                    //Vaihtaa lomakkeen tietoja
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
            //Valitaan meno listalta
            if (listView_income.SelectedIndices.Count > 0)
            {
                listView_income.SelectedItems[0].Selected = false;
            }
            if (listView_outlay.SelectedIndices.Count > 0)
            {
                if (listView_outlay.SelectedIndices.Count == 1)
                {
                    //Vaihtaa lomakkeen tietoja
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
            //Lisätään tulo tai meno
            //Tarkistaa, ettei kuvaus ole tyhjä
            if (textBox_economic_name.Text != "")
            {
                textBox_economic_amount.Text = textBox_economic_amount.Text.Replace(".",","); //Muuttaa euromäärän desimaali erottimet pilkuksi
                //Tarkistaa, ettei euromäärä ole tyhjä
                if (textBox_economic_amount.Text != "")
                {
                    //Tarkistaa, onko euromäärä numeerinen
                    isNumeric = double.TryParse(textBox_economic_amount.Text, out n_euro);
                    if (isNumeric)
                    {
                        //Tarkistaa, onko tyyppi valittu
                        if (Convert.ToString(comboBox_economic_type.SelectedItem) != "")
                        {
                            //Tarkistaa, ettei ole samannimistä
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
                                //Lisätään taulukkoon
                                insertTableQuery = @"INSERT INTO economic(id, username, description, type, amount) " +
                                                    "VALUES('null', '" + user.user_name + "', '" + textBox_economic_name.Text + "', '" + Convert.ToString(comboBox_economic_type.SelectedItem) + "', '" + textBox_economic_amount.Text.Replace(",", ".") + "');";
                                try
                                {
                                    conn.Open();
                                    cmd = new MySqlCommand(insertTableQuery, conn);
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                    //Tyhjennetään lomake
                                    textBox_economic_name.Text = "";
                                    textBox_economic_amount.Text = "";
                                    comboBox_economic_type.SelectedIndex = -1;
                                    listView_income.Sorting = SortOrder.Ascending;
                                    //Haetaan uudet tulot ja menot
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
            //Poistetaan tulo tai meno
            //Tarkistaa, onko tietoa valittu
            if ((listView_income.SelectedIndices.Count == 1 && listView_outlay.SelectedIndices.Count == 0) ||
                (listView_outlay.SelectedIndices.Count == 1 && listView_income.SelectedIndices.Count == 0))
            {
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän tiedon?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan taulukosta
                    deleteTableQuery = @"DELETE FROM economic WHERE username='" + user.user_name +
                                        "' AND description='" + textBox_economic_name.Text +
                                        "' AND type='" + Convert.ToString(comboBox_economic_type.SelectedItem) + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Tyhjentää lomakkeen
                        textBox_economic_name.Text = "";
                        textBox_economic_amount.Text = "";
                        comboBox_economic_type.SelectedIndex = -1;
                        //Hakee uudet tulot ja menot
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
            //Luetaan ruokalistat
            comboBox_menus.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS menu (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        family_key VARCHAR(30) NOT NULL,
                                        menu_name VARCHAR(30) NOT NULL,
                                        food VARCHAR(30) NOT NULL,
                                        description VARCHAR(30) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan ruokalistat tietokannasta
                while (dataReader.Read())
                {
                    //Tarkistaa, onko saman nimisiä listoja
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
            //Tarkistaa, onko kaikki oikeudet
            if (!user.full_permissions)
            {
                //Tarkistaa, onko ruokalistaa valittu
                if (comboBox_menus.SelectedItem != null)
                {
                    //Näyttää nappulat
                    button_menu_add.Visible = true;
                    button_menu_add.Enabled = true;
                    button_menu_save.Visible = true;
                    button_menu_save.Enabled = true;
                }
            }
            //Tarkistaa, ettei ole prosessia käynnissä
            if (!menu_progressing)
            {
                listView_menu.Items.Clear();
                textBox_menu_name.Text = Convert.ToString(comboBox_menus.SelectedItem);
                //Valitaan tiedot taulukosta
                selectTableQuery = @"SELECT id, family_key, menu_name, food, description " +
                                    " FROM menu " +
                                    " WHERE family_key='" + user.family_key + "' AND menu_name='" + textBox_menu_name.Text + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(createTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    //Luetaan ruokalistat tietokannasta
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
            //Lisätään ruoka
            //Tarkistaa, ettei ruuan nimi ole liian lyhyt
            if (textBox_menu_food.Text.Length > 0)
            {
                //Tarkistaa, onko kaikki oikeudet
                if (!user.full_permissions)
                {
                    //Lisää ehdotettu-tagin
                    textBox_menu_description.Text += " (ehdotettu)";
                }
                //Lisätään ruoka listalle
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
            //Poistetaan ruoka
            if (listView_menu.SelectedIndices.Count > 0)
            {
                menu_progressing = true;
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän ruuan?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistaa tiedon
                    listView_menu.Items.Remove(listView_menu.SelectedItems[0]);
                    //Tyhjentää lomakkeen
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
            //Ruokalista tallennetaan
            textBox_menu_name.Text = textBox_menu_name.Text.Replace(" ", "_"); //Muuttaa syötetystä nimestä välit ala-viivoiksi
            //Tarkistaa, ettei nimi ole liian lyhyt
            if (textBox_menu_name.Text.Length > 0)
            {
                //Tarkistaa, onko listalla ruokia
                if (listView_menu.Items.Count > 0)
                {
                    //Poistetaan vanha tietokannasta
                    deleteTableQuery = @"DELETE FROM menu " +
                                        "WHERE menu_name='" + textBox_menu_name.Text + "' AND family_key='" + user.family_key + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        for (index = 0; index < listView_menu.Items.Count; index++)
                        {
                            //Lisätään tietokantaan
                            insertTableQuery = @"INSERT INTO menu (id, family_key, menu_name, food, description) " +
                                        "VALUES(null, '" + user.family_key + "', '" + textBox_menu_name.Text + "', '" + listView_menu.Items[index].SubItems[0].Text + "', '" + listView_menu.Items[index].SubItems[1].Text + "' );";
                            cmd = new MySqlCommand(insertTableQuery, conn);
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                        //Hakee uudet ruokalistat
                        readMenus();
                        menu_progressing = true;
                        comboBox_menus.SelectedItem = textBox_menu_name.Text;
                        menu_progressing = false;
                        MessageBox.Show("Ruokalista tallennettu", "Tallenna");
                        //Lisää tiedon muutosseurantaan
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
            //Poistetaan ruokalista
            //Tarkistaa, onko ruokalistaa valittu
            if (comboBox_menus.SelectedItem != null)
            {
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen ruokalistan?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan tietokannasta
                    deleteTableQuery = @"DELETE FROM menu " +
                                        "WHERE menu_name='" + textBox_menu_name.Text + "' AND family_key='" + user.family_key + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Hakee uudet ruokalistat
                        readMenus();
                        //Tyhjentää lomakkeen
                        comboBox_menus.SelectedItem = null;
                        listView_menu.Items.Clear();
                        textBox_menu_name.Text = "";
                        textBox_menu_food.Text = "";
                        textBox_menu_description.Text = "";
                        MessageBox.Show("Ruokalista poistettu", "Poista");
                        //Lisää tiedon muutosseurantaan
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
            //Valitaan ruoka listalsta
            textBox_menu_food.Text = listView_menu.SelectedItems[0].SubItems[0].Text;
            textBox_menu_description.Text = listView_menu.SelectedItems[0].SubItems[1].Text.Replace(" (ehdotettu)", "");
        }
//----- Cleaning --------------------------------------------------------------------------------------
        private void readCleaning()
        {
            //Luetaan siivousvuorot
            listView_cleaning_shift_list.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS cleaning_shift (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            family_key VARCHAR(30) NOT NULL,
                                            description VARCHAR(30) NOT NULL,
                                            worker VARCHAR(30) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan siivousvuorot tietokannasta
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
            //Haetaan perheenjäsenet
            listView_cleaning_shift_family_members.Items.Clear();
            foreach (string member in user.family_members)
            {
                listView_cleaning_shift_family_members.Items.Add(member);
            }
        }

        private void listView_cleaning_shift_family_members_Click(object sender, EventArgs e)
        {
            //Valitaan perheenjäsen listalta
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
            //Valitaan askare listalta
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
            //Lisätään askare
            //Tarkistaa, että perheenjäsen on valittu
            if (textBox_cleaning_shift_worker.Text != "")
            {
                //Tarkistaa, ettei askare ole tyhjä
                if (textBox_cleaning_shift_work.Text != "")
                {
                    //Lisätään tietokantaan
                    insertTableQuery = @"INSERT INTO cleaning_shift(id, family_key, description, worker) " +
                                        "VALUES('null', '" + user.family_key + "', '" + textBox_cleaning_shift_work.Text + "', '" + textBox_cleaning_shift_worker.Text + "');";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(insertTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Lisää tiedon muutosseurantaan
                        change = "Siivousvuoroihin lisätty " + textBox_cleaning_shift_work.Text;
                        addChangeTracking(change);
                        //Tyhjentää lomakkeen
                        textBox_cleaning_shift_work.Text = "";
                        textBox_cleaning_shift_worker.Text = "";
                        //Hakee uudet siivousvuorot
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
            //Poistetaan siivousvuoro
            if (listView_cleaning_shift_list.SelectedIndices.Count == 1)
            {
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa tämän tiedon?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan tietokannasta
                    deleteTableQuery = @"DELETE FROM cleaning_shift WHERE family_key='" + user.family_key +
                                        "' AND description='" + listView_cleaning_shift_list.SelectedItems[0].SubItems[0].Text +
                                        "' AND worker='" + listView_cleaning_shift_list.SelectedItems[0].SubItems[1].Text + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Lisätään tieto muutosseurantaan
                        change = "Siivousvuoroista poistettu " + textBox_cleaning_shift_work.Text;
                        addChangeTracking(change);
                        //Tyhjentää lomakkeen
                        textBox_cleaning_shift_worker.Text = "";
                        textBox_cleaning_shift_work.Text = "";
                        //Hakee uudet siivousvuorot
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
                MessageBox.Show("Et ole valinnut poistettavaa askaretta", "Poista");
            }
        }
//----- Shopping list --------------------------------------------------------------------------------------
        private void readShoppingLists()
        {
            //Lukee kauppalaput
            comboBox_shopping_lists.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS shoppinglist (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        family_key VARCHAR(30) NOT NULL,
                                        listname VARCHAR(30) NOT NULL,
                                        text TEXT(1000) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan kauppalaput tietokannasta
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
            //Lisätään tuote kauppalappuun
            if (textBox_item_name.Text.Length > 0)
            {
                added_item = "> " + textBox_item_name.Text + "  ";
                added_item += textBox_item_amount.Text + " " + comboBox_amount_type.SelectedItem;
                //Tarkistaa, onko kaikki oikeudet
                if (!user.full_permissions)
                {
                    //Lisää ehdotettu-tagin
                    added_item += " (" + user.user_name + " ehdottama)";
                }
                added_item += System.Environment.NewLine;
                //Tarkistaa, ettei sisällön pituusraja ylity
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
            //Laskee kauppalapun sisällön pituuden
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
            //Valitaan kauppalappu
            //Tarkistaa, onko kaikki oikeudet
            if (!user.full_permissions)
            {
                //Tarkistaa, onko kauppalappua valittu
                if (comboBox_shopping_lists.SelectedItem != null)
                {
                    //Näyttää nappulat
                    button_add_item.Visible = true;
                    button_add_item.Enabled = true;
                    button_add_item.Text = "Ehdota tuotetta";
                    button_list_save.Visible = true;
                    button_list_save.Enabled = true;
                    button_shopping_list_save_as_file.Visible = true;
                    button_shopping_list_save_as_file.Enabled = true; 
                }
            }
            //Tarkistaa, ettei prosessia ole käynnissä
            if (!list_progressing)
            {
                richTextBox_shopping_list.Text = "";
                textBox_list_name.Text = Convert.ToString(comboBox_shopping_lists.SelectedItem);
                //Valitaan tiedot taulukosta
                selectTableQuery = @"SELECT id, family_key, listname, text " +
                                    "FROM shoppinglist WHERE family_key='" + user.family_key + "' " +
                                    "AND listname='" + comboBox_shopping_lists.SelectedItem + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    //Lukee kauppalapun tiedot
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
            //Tallennetaan kauppalappu
            textBox_list_name.Text = textBox_list_name.Text.Replace(" ", "_"); //Muuttaa syötetystä nimestä välit ala-viivoiksi
            if (textBox_list_name.Text.Length > 0)
            {
                //Lisätään taulukkoon, jos ei ole vielä olemassa
                insertTableQuery = @"INSERT INTO shoppinglist (id, family_key, listname, text) " +
                                    "SELECT * FROM(SELECT 0, '" + user.family_key + "', '" + textBox_list_name.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT family_key, listname FROM shoppinglist WHERE family_key='" + user.family_key + "' AND listname='" + textBox_list_name.Text + "' " +
                                    ") LIMIT 2 ;";
                //Päivitetään taulukkoa
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
                    //Hakee uudet kauppalaput
                    readShoppingLists();
                    list_progressing = true;
                    comboBox_shopping_lists.SelectedItem = textBox_list_name.Text;
                    list_progressing = false;
                    MessageBox.Show("Kauppalappu tallennettu", "Tallenna");
                    //Lisätään tieto muutosseurantaan
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
            //Poistetaan kauppalappu
            //Tarkistaa, onko kauppalappua valittu
            if (comboBox_shopping_lists.SelectedItem != null)
            {
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen kauppalapun?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan tietokannasta
                    deleteTableQuery = @"DELETE FROM shoppinglist WHERE family_key='" + user.family_key +
                                    "' AND listname='" + textBox_list_name.Text + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Kauppalappu on poistettu", "Poista");
                        //Lisätään tieto muutosseurantaan
                        change = "Kauppalappu " + textBox_list_name.Text + " poistettu";
                        addChangeTracking(change);
                        //Tyhjennetään lomake
                        textBox_list_name.Text = "";
                        richTextBox_shopping_list.Text = "";
                        comboBox_shopping_lists.SelectedItem = null;
                        comboBox_amount_type.SelectedItem = null;
                        textBox_item_name.Text = "";
                        textBox_item_amount.Text = "";
                        //Hakee uudet kauppalaput
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
            //Tallennetaan kauppalappu tiedostoon
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "*.txt";
            saveFile.Filter = "Tekstitiedostot (*.txt)|*.txt";
            //Kysyy, mihin halutaan tallentaa
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
            saveFile.FileName.Length > 0)
            {
                richTextBox_shopping_list.SaveFile(saveFile.FileName, RichTextBoxStreamType.PlainText);
            }
        }
//----- Calendar --------------------------------------------------------------------------------------
        private void readCalendarLists()
        {
            //Lukee tapahtumat
            listView_events.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS calendar (
                                 id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                 username VARCHAR(30) NOT NULL,
                                 event_name VARCHAR(30) NOT NULL,
                                 location VARCHAR(30) NOT NULL,
                                 date DATE NOT NULL,
                                 month VARCHAR(30) NOT NULL,
                                 year INT(6) NOT NULL ) ;";
            //Valitaan tiedot taulukosta
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
                //Luetaan tapahtumat tietokannasta
                while (dataReader.Read())
                {
                    //Tarkistaa, näytetäänkö menneitä tapahtumia
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
                        //Tarkistaa, onko tapahtuma tuleva
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
            //Haetaan tapahtumia
            selectTableQuery = selectTableQuery_;
            listView_events.Items.Clear();
            try
            {
                conn.Open();
                cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                //Luetaan tapahtumat tietokannasta
                while (dataReader.Read())
                {
                    //Tarkistaa, näytetäänkö menneitä tapahtumia
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
                        //Tarkistaa, onko tapahtuma tuleva
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
            //Valitaan kuukausi listalta
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
            //Valitaan tapahtuma listalta
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
            //Lisätään tapahtuma
            //Tarkistaa, ettei päivämäärä ole mennyttä aikaa
            if (dateTimePicker_event_datetime.Value.Date >= DateTime.Now.Date)
            {
                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("d/M/yyyy");
                //Tarkistaa, ettei kuvaus ole tyhjä
                if (textBox_event_name.Text != "")
                {
                    //Tarkistaa, ettei tapahtumapaikka ole tyhjä
                    if (textBox_event_location.Text != "")
                    {
                        //Tarkistaa, ettei päivämäärä ole tyhjä
                        if (Convert.ToString(dateTimePicker_event_datetime.Value) != "")
                        {
                            //Tarkistaa, onko saman nimisiä
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
                                //Konvertoi päivämääriä
                                month = dateTimePicker_event_datetime.Value.ToString(" MM ");
                                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd");
                                //Lisätään tietokantaan
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
                                    //Hakee uudet tapahtumat
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
                    MessageBox.Show("Et ole syöttänyt tapahtuman nimeä tai kuvausta", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Et voi lisätä tapahtumaa mennelle ajalle", "Lisää");
            }
        }

        private void button_event_set_today_Click(object sender, EventArgs e)
        {
            //Asettaa kalenterin päivämäärän nykyiseksi päiväksi
            dateTimePicker_event_datetime.Value = DateTime.Now;
        }

        private void button_event_search_set_today_Click(object sender, EventArgs e)
        {
            //Asettaa kalenterin päivämäärän nykyiseksi päiväksi
            dateTimePicker_event_search_datetime.Value = DateTime.Now;
        }

        private void button_event_delete_past_Click(object sender, EventArgs e)
        {
            //Poistetaan menneet tapahtumat
            //Kysyy, halutaanko poistaa
            dialogResult = MessageBox.Show("Haluatko varmasti poistaa menneet tapahtumat?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //Poistetaan tietokannasta
                deleteTableQuery = @"DELETE FROM calendar WHERE username='" + user.user_name +
                                        "' AND date<'" + DateTime.Now.ToString("yyyy-MM-dd") + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(deleteTableQuery, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Menneet tapahtumat on poistettu", "Poista");
                    //Luetaan uudet tapahtumat
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
            //Poistetaan tapahtuma
            //Tarkistaa, onko tapahtumaa valittu
            if (listView_events.SelectedIndices.Count > 0)
            {
                //Konvertoi päivämääriä
                DateTime dateValue = dateTimePicker_event_datetime.Value;
                // "2000-01-12 20:10:00Z"    
                dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
                formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("yyyy-MM-dd");
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen tapahtuman?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan tietokannasta
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
                        //Tyhjennetään lomake
                        formatDateTimeForMySql = dateTimePicker_event_datetime.Value.ToString("d/M/yyyy");
                        textBox_event_name.Text = "";
                        textBox_event_location.Text = "";
                        dateTimePicker_event_datetime.Value = DateTime.Now;
                        //Hakee uudet tapahtumat
                        readCalendarLists();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
            else
            {
                MessageBox.Show("Et ole valinnut poistettavaa tapahtumaa", "Poista");
            }
        }

        private void button_event_search_1_Click(object sender, EventArgs e)
        {
            //Haetaan tapahtumaa kuukauden ja/tai vuoden perusteella
            //Tarkistaa, minkä mukaan etsitään
            if (Convert.ToString(comboBox_event_search_month.SelectedItem) != "Koko vuosi" &&
            textBox_event_search_year.Text != "")
            {
                //Tarkistaa, onko syötetty vuosiluku numeerinen
                isNumeric = int.TryParse(textBox_event_search_year.Text, out n);
                if (isNumeric)
                {
                    //Tarkistaa, onko vuosiluku vanha
                    if (int.Parse(textBox_event_search_year.Text) < DateTime.Now.Year)
                    {
                        //Näyttää menneetkin tapahtumat
                        checkBox_event_show_past.Checked = true;
                    }
                    //Konvertoi kuukauden
                    month = dateTimePicker_event_datetime.Value.ToString(" MM ");
                    setSearchMonth(ref month);
                    //Valitaan tiedot taulukosta
                    selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                        " FROM calendar " +
                                        " WHERE username='" + user.user_name +
                                        "' AND month='" + months[Int32.Parse(month)] +
                                        "' AND year=" + Int32.Parse(textBox_event_search_year.Text) + " ;";
                    //Hakee tietokannasta kriteerien perusteella
                    searchCalendarLists(selectTableQuery);
                }
                else
                {
                    MessageBox.Show("Syöttämäsi vuosi ei ole vuosiluku", "Hae");
                }
            }
            else if (Convert.ToString(comboBox_event_search_month.SelectedItem) != "Koko vuosi")
            {
                //Konvertoi kuukauden
                month = Convert.ToString(comboBox_event_search_month.SelectedItem);
                setSearchMonth(ref month);
                //Valitaan tiedot taulukosta
                selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                    " FROM calendar " +
                                    " WHERE username='" + user.user_name +
                                    "' AND month='" + months[Int32.Parse(month)] + "' ;";
                //Hakee tietokannasta kriteerien perusteella
                searchCalendarLists(selectTableQuery);
            }
            else if (textBox_event_search_year.Text != "")
            {
                //Tarkistaa, onko syötetty vuosiluku numeerinen
                isNumeric = int.TryParse(textBox_event_search_year.Text, out n);
                if (isNumeric)
                {
                    //Tarkistaa, onko vuosiluku vanha
                    if (int.Parse(textBox_event_search_year.Text) < DateTime.Now.Year)
                    {
                        //Näyttää menneetkin tapahtumat
                        checkBox_event_show_past.Checked = true;
                    }
                    //Valitaan tiedot taulukota
                    selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                        " FROM calendar " +
                                        " WHERE username='" + user.user_name +
                                        "' AND year=" + Int32.Parse(textBox_event_search_year.Text) + " ;";
                    //Hakee tietokannasta kriteerien perusteella
                    searchCalendarLists(selectTableQuery);
                }
                else
                {
                    MessageBox.Show("Syöttämäsi vuosi ei ole vuosiluku", "Hae");
                }
            }
            else
            {
                //Hakee tapahtumat, jos kriteerejä ei syötetty
                readCalendarLists();
            }
        }

        private void button_clear_search_settings_Click(object sender, EventArgs e)
        {
            //Tyhjentää lomakkeen
            comboBox_event_search_month.SelectedItem = "Koko vuosi";
            textBox_event_search_year.Text = "";
        }

        private void button_event_search_2_Click(object sender, EventArgs e)
        {
            //Haetaan tapahtumaa päivän perusteella
            formatDateTimeForMySql = dateTimePicker_event_search_datetime.Value.ToString("yyyy-MM-dd");
            //Valitaan tiedot taulukosta
            selectTableQuery = @"SELECT id, username, event_name, location, date " +
                                " FROM calendar " +
                                " WHERE username='" + user.user_name +
                                "' AND date='" + formatDateTimeForMySql + "' ;";
            //Hakee tietokannasta kriteerien perusteella
            searchCalendarLists(selectTableQuery);
        }
//----- Athletic meter --------------------------------------------------------------------------------------
        private void readAthleticMeter()
        {
            //Luetaan kilometrit
            //Konvertoidaan päivämääriä
            month = DateTime.Now.ToString(" M ");
            textBox_athletic_month.Text = months[Int32.Parse(month)];
            year = DateTime.Now.ToString(" yyyy ");
            textBox_athletic_year.Text = textBox_athletic_year.Text.Replace(" ", ""); //Poistaa välit syötetystä vuosiluvusta
            //Tarkistaa, ettei vuosiluku ole tyhjä
            if (textBox_athletic_year.Text.Length > 0)
            {
                //Tarkistaa, onko vuosiluku numeerinen
                isNumeric = Int32.TryParse(textBox_athletic_year.Text, out n);
                if (isNumeric)
                {
                    year = textBox_athletic_year.Text;
                }
                else
                {
                    MessageBox.Show("Syötä vuosiluku numeroina", "Hae");
                    textBox_athletic_year.Text = year.Replace(" ", ""); //Poistaa välit syötetystä vuosiluvusta
                }
            }
            else
            {
                textBox_athletic_year.Text = year.Replace(" ", ""); //Poistaa välit syötetystä vuosiluvusta
            }
            //Tarkistaa, onko numeroita ja kuukausia tulostettu
            if (addedLabels.Count > 0)
            {
                //Tyhjentää numerot ja kuukaudet
                int listCount = addedLabels.Count;
                for (int i = 0; i < listCount; i++)
                {
                    Label removeLabel = addedLabels[0];
                    addedLabels.Remove(removeLabel);
                    panel_athletic_statistics.Controls.Remove(removeLabel);
                }
            }
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS athletic_meter (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            month VARCHAR(100) NOT NULL,
                                            year VARCHAR(5) NOT NULL,
                                            amount DOUBLE(10,2) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan kilometrit tietokannasta
                while (dataReader.Read())
                {
                    textBox_athletic_kilometers.Text = Convert.ToString(dataReader["amount"]);
                }
                dataReader.Close();
                //Valitaan tiedot taulukosta
                selectTableQuery = @"SELECT id, username, month, year, amount " +
                                " FROM athletic_meter " +
                                " WHERE username='" + user.user_name +
                                "' AND year=" + Int32.Parse(year) + " ;";
                cmd = new MySqlCommand(selectTableQuery, conn);
                dataReader = cmd.ExecuteReader();
                double largest = 0; //Suurin kilometrimäärä
                double zoom = 0; //Piirto etäisyys (jos kilometreissä on suuria eroja, piirto pidetään järkevänä)
                //Luetaan tiedot
                while (dataReader.Read())
                {
                    //Jos luku on suurempi kuin suurin kilometrimäärä
                    if (double.Parse(Convert.ToString(dataReader["amount"])) >= largest)
                    {
                        //Luku asetetaan suurimmaksi
                        largest = double.Parse(Convert.ToString(dataReader["amount"]));
                    }
                    values.Add(Tuple.Create(Convert.ToString(dataReader["month"]), double.Parse(Convert.ToString(dataReader["amount"]))));
                }
                //Laksetaan piirtoetäisyys suurimman luvun avulla
                zoom = largest / 240;
                dataReader.Close();
                conn.Close();
                //Tulostetaan kuukaudet alareunaan
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
                //Tulostetaan numerot ja piirretään pylväät
                //Tätä on todella vaikeaa lähteä selittämään, ja itse olen kehitellyt koko lausekkeen
                //Siinä lasketaan pylväiden korkeudet, sijainnit ja piirtoetäisyyksiä
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
            //Lisätään tai vähennetään kilometrejä
            //Tarkistaa, ettei syötetty kilometrimäärä ole tyhjä
            if (textBox_athletic_add_kilometers.Text.Length > 0)
            {
                textBox_athletic_add_kilometers.Text = textBox_athletic_add_kilometers.Text.Replace(".", ","); //Muuttaa syötetyn määrän desimaali merkit pilkuiksi
                //Tarkistaa, onko kilometrimäärä numeerinen
                isNumeric = double.TryParse(textBox_athletic_add_kilometers.Text, out n_euro);
                if (isNumeric)
                {
                    //Tarkistaa, ettei kilometrimäärä mene alle nollan
                    current_kilometers = 0;
                    if ((double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text)) >= 0)
                    {
                        current_kilometers = (double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text));
                    }
                    textBox_athletic_year.Text = "";
                    //Konvertoi päivämääriä
                    month = DateTime.Now.ToString(" M ");
                    year = DateTime.Now.ToString(" yyyy ");
                    //Lisätään taulukkoon, jos ei ole vielä olemassa
                    insertTableQuery = @"INSERT INTO athletic_meter (id, username, month, year, amount) " +
                                        "SELECT * FROM(SELECT -1, '" + user.user_name + "', '" + months[Int32.Parse(month)] + "', " + Int32.Parse(year) + ", " + (double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text)).ToString().Replace(",", ".") + ") AS tmp " +
                                        "WHERE NOT EXISTS( " +
                                        "SELECT username, month, year FROM athletic_meter WHERE username='" + user.user_name + "' AND month='" + months[Int32.Parse(month)] + "' AND year=" + Int32.Parse(year) +
                                        ") LIMIT 2 ;";
                    //Päivitetään taulukkoon
                    updateTableQuery = @"UPDATE athletic_meter " +
                                         "SET amount='" + (double.Parse(textBox_athletic_kilometers.Text) + double.Parse(textBox_athletic_add_kilometers.Text)).ToString().Replace(",", ".") + "' " +
                                         "WHERE username='" + user.user_name + "' AND month='" + months[Int32.Parse(month)] + "' AND year=" + Int32.Parse(year) + " ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(insertTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        cmd = new MySqlCommand(updateTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        //Hakee uudet kilometrit
                        readAthleticMeter();
                        MessageBox.Show("Kilometrit lisätty", "Lisää");
                        textBox_athletic_add_kilometers.Text = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
                else
                {
                    MessageBox.Show("Syötä kilometrimäärä numeroina", "Lisää");
                }
            }
            else
            {
                MessageBox.Show("Syötä kilometrimäärä", "Lisää");
            }
        }

        private void panel_athletic_statistics_Paint(object sender, PaintEventArgs e)
        {
            //Piirtää tilasto-pylväät
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
            //Hakee kilometrimäärät syötetyn vuoden perusteella
            readAthleticMeter();
        }

//----- Checklist --------------------------------------------------------------------------------------
        private void readChecklists()
        {
            //Lukee muistilistat
            comboBox_checklists.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS checklist (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        checklist_name VARCHAR(30) NOT NULL,
                                        text TEXT(5000) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan muistilistat tietokannasta
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
            //Tarkistaa, ettei prosessia ole käynnissä
            if (!checklist_progressing)
            {
                textBox_checklist.Text = "";
                textBox_checklist_name.Text = Convert.ToString(comboBox_shopping_lists.SelectedItem);
                //Valitaan tiedot taulukosta
                selectTableQuery = @"SELECT id, username, checklist_name, text " +
                                    "FROM checklist WHERE username='" + user.user_name + "' " +
                                    "AND checklist_name='" + comboBox_checklists.SelectedItem + "' ;";
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand(selectTableQuery, conn);
                    dataReader = cmd.ExecuteReader();
                    //Luetaan muistilistan sisältä tietokannasta
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
            //Tallennetaan muistilista
            textBox_checklist_name.Text = textBox_checklist_name.Text.Replace(" ", "_"); //Muuttaa syötetyn nimen välit ala-viivoiksi
            //Tarkistaa, ettei syötetty nimi ole liian lyhyt
            if (textBox_checklist_name.Text.Length > 0)
            {
                //Luodaan uusi taulukko, jos ei ole vielä olemassa
                createTableQuery = @"CREATE TABLE IF NOT EXISTS checklist (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            checklist_name VARCHAR(30) NOT NULL,
                                            text VARCHAR(1000) NOT NULL);";
                //Lisätään tietokantaan
                insertTableQuery = @"INSERT INTO checklist (id, username, checklist_name, text) " +
                                    "SELECT * FROM(SELECT 0, '" + user.user_name + "', '" + textBox_checklist_name.Text + "', 'null') AS tmp " +
                                    "WHERE NOT EXISTS( " +
                                    "SELECT username, checklist_name FROM checklist WHERE username='" + user.user_name + "' AND checklist_name='" + textBox_checklist_name.Text + "' " +
                                    ") LIMIT 2 ;";
                //Päivitetään tietokantaan
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
                    //Hakee uudet muistilistat
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
            //Poistetaan muistilista
            //Tarkistaa, onko muistilistaa valittu
            if (comboBox_checklists.SelectedItem != null)
            {
                //Kysyy, halutaanko poistaa
                dialogResult = MessageBox.Show("Haluatko varmasti poistaa nykyisen muistilistan?", "Poista", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //Poistetaan tietokannasta
                    deleteTableQuery = @"DELETE FROM checklist WHERE username='" + user.user_name +
                                    "' AND checklist_name='" + textBox_checklist_name.Text + "' ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(deleteTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Muistilista on poistettu", "Poista");
                        //Tyhjentää lomakkeen
                        textBox_checklist_name.Text = "";
                        textBox_checklist.Text = "";
                        comboBox_checklists.SelectedItem = null;
                        //Hakee uudet muistilistat
                        readChecklists();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                }
            }
            else
            {
                MessageBox.Show("Et ole valinnut poistettavaa muistilistaa", "Poista");
            }
        }

        private void textBox_checklist_TextChanged(object sender, EventArgs e)
        {
            //Tarkistaa, ettei muistilistan sisältön pituus ylitä rajaa
            textBox_checklist_text_length.Text = Convert.ToString(textBox_checklist.Text.Length);
            if (Int32.Parse(textBox_checklist_text_length.Text) > 5000)
            {
                MessageBox.Show("Et voi lisätä enempää tektiä muistilistalle", "Lisää");
                //Palauttaa edellisen sisällön
                if (previous_checklist != "")
                {
                    textBox_checklist.Text = previous_checklist;
                    textBox_checklist_text_length.Text = Convert.ToString(textBox_checklist.Text.Length);
                }
                textBox_checklist.SelectionStart = textBox_checklist.Text.Length - 1; //Add some logic if length is 0
                textBox_checklist.SelectionLength = 0;
            }
            previous_checklist = textBox_checklist.Text;
        }
//----- Change tracking --------------------------------------------------------------------------------------
        private void readChangeTrackingLists()
        {
            //Lukee muutokset
            listView_change_tracking.Items.Clear();
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                            username VARCHAR(30) NOT NULL,
                                            family_key VARCHAR(30) NOT NULL,
                                            date DATETIME NOT NULL,
                                            changes VARCHAR(100) NOT NULL);";
            //Valitaan tiedot taulukosta
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
                //Luetaan muutokset tietokannasta
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
            //Lisätään muutos muutosseurantaan
            //Konvertoi päivämääriä
            DateTime dateValue = DateTime.Now;
            // "2000-01-12 20:10:00Z"    
            dateValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern);
            formatDateTimeForMySql = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
            //Luodaan taulukko, jos sitä ei ole olemassa
            createTableQuery = @"CREATE TABLE IF NOT EXISTS change_tracking (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        family_key VARCHAR(30) NOT NULL,
                                        date DATETIME NOT NULL,
                                        changes VARCHAR(100) NOT NULL);";
            //Lisätään tietokantaan
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
            //Poistetaan muutokset
            //Kysyy, halutaanko poistaa
            dialogResult = MessageBox.Show("Haluatko varmasti tyhjentää tiedot muutoksista?", "Poista", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //Poistetaan tietokannasta
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
            //Kirjaudutaan ulos
            //Asetetaan näkymäksi Kirjautuminen
            Kirjautuminen Kirjautuminen = new Kirjautuminen();
            Kirjautuminen.Show();
            //Suljetaan nykyinen ikkuna
            this.Close();
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            //Pienennetään ikkuna
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
