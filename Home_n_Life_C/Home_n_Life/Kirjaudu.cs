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
using System.Text.RegularExpressions;

namespace Home_n_Life
{
    public partial class Kirjautuminen : Form
    {
        public Kirjautuminen()
        {
            InitializeComponent();
        }
        //Tietokantayhteyden osoite
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
        //Käyttäjätietojen muuttujat (tiedot haetaan tietokannasta)
        string query_password;
        List<string> query_users;
        List<string> query_family_keys;
        //Tietokantayhteyden tila (päällä=true/katkennut=false)
        bool MySqlConnectionOn;
        //MySql syntaksit
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader dataReader;
        //YES/NO - dialogi
        DialogResult dialogResult;
        //Tietokantaan tehtävien komentojen muuttujat
        string createDatabase, createTableQuery, insertTableQuery, selectTableQuery;
//----- Database --------------------------------------------------------------------------------------
        private void checkDatabaseConnection()
        {
            //Tarkistetaan yhteys tietokantaan
            MySqlConnectionOn = false; //Yhteyden tilaksi asetetaan katkaistu
            progressBar_database_connection.Value = 1; //Yhteys-mittari asetetaan puoliväliin (ikkunan ylälaita)
            progressBar_database_connection.ForeColor = Color.Yellow; //Yhteys-mittarin väri muutetaan keltaiseksi
            try
            {
                //Avataan yhteys tietokantaan
                conn = new MySqlConnection(connetionString); 
                conn.Open();
                //Yhteys-mittari asetetaan vihreäksi ja valmis-tilaan
                progressBar_database_connection.Value = 2;
                progressBar_database_connection.ForeColor = Color.LimeGreen;
                //Yhteyden tilaksi asetetaan: päällä
                MySqlConnectionOn = true;
                //Suljetaan yhteys
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
//----- Login --------------------------------------------------------------------------------------
        private void Kirjautuminen_Load(object sender, EventArgs e)
        {
            try
            {
                //Avataan yhteys tietokantaan ja tarkistetaan, onko tietokantaa olemassa
                conn = new MySqlConnection(connetionString);
                conn.Open();
                conn.Close();
            }
            catch
            {
                //Jos tietokantaa ei ole, ohjelma luo uuden
                conn = new MySqlConnection("server=localhost;uid=root;pwd=;"); //Osoite uutta tietokantaa varten
                createDatabase = "CREATE DATABASE IF NOT EXISTS `home&life`;"; //Komento
                MySqlCommand cmd = new MySqlCommand(createDatabase, conn);
                try
                {
                    //Avataan yhteys tietokantaan
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Et vielä omistanut tietokantaa nimeltä 'home&life'\nSe on nyt luotu puolestasi", "Tietokanta");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex));
                }
            }
            //Tarkistetaan yhteys tietokantaan
            checkDatabaseConnection();
            if (MySqlConnectionOn)
            {
                //Näkymäksi astetaan Kirjautuminen
                groupBox_login.Location = new Point(13, 65);
                groupBox_login.Size = new Size(478, 337);
                groupBox_login.Enabled = true;
                groupBox_login.Visible = true;
                //Piilotetaan Rekisteröityminen
                groupBox_registration.Enabled = false;
                groupBox_registration.Visible = false;
                textBox_username.Select();
            }
            else
            {
                //Jos tietokantaan ei saada yhteyttä, ohjelma suljetaan
                MessageBox.Show("Tietokantaan ei saatu yhteyttä\nYritä korjata yhteytesi, jotta voit jatkaa\nTarkista, että sinulla on XAMPP yhteys päällä\nOhjelma joudutaan sulkemaan", "Tietokanta");
                Application.Exit();
            }
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            //Pienentää ikkunan
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            //Sulkee ohjelman
            Application.Exit();
        }

        private void button_show_password_Click(object sender, EventArgs e)
        {
            //Muuttaa salasanan kirjautuessa näkyväksi tai piilottaa sen
            if (textBox_password.PasswordChar == '*')
            {
                textBox_password.PasswordChar = '\0';
            }
            else
            {
                textBox_password.PasswordChar = '*';
            }
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            //Tarkistaa yhteyden tietokantaan
            checkDatabaseConnection();
            if (MySqlConnectionOn)
            {
                conn = new MySqlConnection(connetionString);
                //Tarkistaa, ettei käyttäjänimi ole tyhjä
                if (textBox_username.Text.Length > 0)
                {
                    //Tarkistaa, ettei salasana ole tyhjä
                    if (textBox_password.Text.Length > 0)
                    {
                        //Luodaan uusi taulukko, jos sitä ei ole olemassa
                        createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                                    id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                                    username VARCHAR(30) NOT NULL,
                                                    password VARCHAR(30) NOT NULL,
                                                    family_key VARCHAR(30) NOT NULL,
                                                    permissions VARCHAR(30) NOT NULL);";
                        //Valitaan tiedot taulukosta
                        selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                            "FROM users " +
                                            "WHERE username='" + textBox_username.Text + "' ;";
                        try
                        {
                            conn.Open();
                            cmd = new MySqlCommand(createTableQuery, conn);
                            cmd.ExecuteNonQuery();
                            cmd = new MySqlCommand(selectTableQuery, conn);
                            dataReader = cmd.ExecuteReader();
                            //Luetaan käyttäjät tietokannasta
                            query_users = new List<string>();
                            while (dataReader.Read())
                            {
                                if (dataReader["username"] != null)
                                {
                                    query_users.Add(Convert.ToString(dataReader["username"]));
                                }
                            }
                            dataReader.Close();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Convert.ToString(ex));
                        }
                        //Jos syötetty käyttäjänimi on olemassa
                        if (query_users.Contains(textBox_username.Text))
                        {
                            query_password = "";
                            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                            " FROM users " +
                                            " WHERE username='" + textBox_username.Text + "' ;";
                            try
                            {
                                conn.Open();
                                cmd = new MySqlCommand(createTableQuery, conn);
                                cmd.ExecuteNonQuery();
                                cmd = new MySqlCommand(selectTableQuery, conn);
                                dataReader = cmd.ExecuteReader();
                                //Luetaan käyttäjät tietokannasta
                                while (dataReader.Read())
                                {
                                    if (Convert.ToString(dataReader["password"]) == textBox_password.Text)
                                    {
                                        query_password = Convert.ToString(dataReader["password"]);
                                    }
                                }
                                dataReader.Close();
                                conn.Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Convert.ToString(ex));
                            }
                            //Jos syötetty salasana on olemassa
                            if (query_password == textBox_password.Text)
                            {
                                //Siirrytään Etusivulle ja alustetaan käyttäjätiedot
                                Etusivu etusivu = new Etusivu();
                                etusivu.Show();
                                etusivu.button_economic.Select();
                                etusivu.initializeUserData(textBox_username.Text); //Tiedot alustetaan
                                etusivu.searchFamilyMembers(); //Hakee perheenjäsenet Etusivum taulukkoon
                                etusivu.searchThisMonthEvents(); //Hakee tapahtumat Etusivum taulukkoon
                                this.Hide(); //Piilottaa Kirjautumis-ikkunan
                            }
                            else
                            {
                                MessageBox.Show("Virheellinen salasana", "Kirjautuminen");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Käyttäjänimeä ei ole olemassa", "Kirjautuminen");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Syötä salasana", "Kirjautuminen");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Syötä käyttäjänimi", "Kirjautuminen");
                }
            }
            else
            {
                MessageBox.Show("Tietokantaan ei saatu yhteyttä\nYritä korjata yhteytesi, jotta voit jatkaa\nTarkista, että sinulla on XAMPP yhteys päällä", "Tietokanta");
            }
        }

        private void button_registration_Click(object sender, EventArgs e)
        {
            //Näkymäksi astetaan Rekisteröityminen
            groupBox_registration.Location = new Point(13, 65);
            groupBox_registration.Size = new Size(478, 337);
            groupBox_registration.Enabled = true;
            groupBox_registration.Visible = true;
            //Piilotetaan Kirjautuminen       
            groupBox_login.Enabled = false;
            groupBox_login.Visible = false;
            //Tyhjennetään lomake
            textBox_account_name.Select();
            textBox_account_name.Text = "";
            textBox_account_password.Text = "";
            textBox_family_key.Text = "";
            comboBox_account_permissions.SelectedItem = null;
        }

//----- Sign up --------------------------------------------------------------------------------------

        private void button_create_account_Click(object sender, EventArgs e)
        {
            //Luodaan uusi käyttäjä
            //Tarkistaa yhteyden tietokantaan
            checkDatabaseConnection();
            if (MySqlConnectionOn)
            {
                conn = new MySqlConnection(connetionString);
                textBox_account_name.Text = textBox_account_name.Text.Replace(" ", ""); //Poistaa syötetystä käyttäjänimestä välit
                textBox_family_key.Text = textBox_family_key.Text.Replace(" ", ""); //Poistaa syötetystä perheavaimesta välit
                //Tarkistaa, ettei käyttäjänimi ole liian lyhyt
                if (textBox_account_name.Text.Length >= 4)
                {
                    //Tarkistaa, ettei käyttäjänimi sisällä erikoismerkkejä
                    if (Regex.IsMatch(textBox_account_name.Text, @"^[a-zA-Z0-9_]+$"))
                    {
                        // Tarkistaa, ettei salasana ole liian lyhyt
                        if (textBox_account_password.Text.Length >= 4)
                        {
                            // Tarkistaa, ettei perheavain ole liian lyhyt
                            if (textBox_family_key.Text.Length >= 4)
                            {
                                //Tarkistaa, ettei perheavain sisällä erikoismerkkejä
                                if (Regex.IsMatch(textBox_family_key.Text, @"^[a-zA-Z0-9_]+$"))
                                {
                                    //Tarkistaa, että oikeudet on valittu
                                    if (comboBox_account_permissions.SelectedItem != null)
                                    {
                                        //Luo uuden taulun, jos sitä ei ole olemassa
                                        createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                                            id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                                            username VARCHAR(30) NOT NULL,
                                                            password VARCHAR(30) NOT NULL,
                                                            family_key VARCHAR(30) NOT NULL,
                                                            permissions VARCHAR(30) NOT NULL);";
                                        //Valitaan tiedot taulukosta
                                        selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                                            " FROM users ;";
                                        //Lisätään uusi käyttäjä
                                        insertTableQuery = @"INSERT INTO users (id, username, password, family_key, permissions) " +
                                                            "VALUES('null', '" + textBox_account_name.Text + "', '" + textBox_account_password.Text + "', '" + textBox_family_key.Text + "', '" + Convert.ToString(comboBox_account_permissions.SelectedItem) + "');";
                                        try
                                        {
                                            conn.Open();
                                            cmd = new MySqlCommand(createTableQuery, conn);
                                            cmd.ExecuteNonQuery();
                                            cmd = new MySqlCommand(selectTableQuery, conn);
                                            dataReader = cmd.ExecuteReader();
                                            //Luetaan käyttäjät tietokannasta
                                            query_users = new List<string>();
                                            while (dataReader.Read())
                                            {
                                                if (dataReader["username"] != null)
                                                {
                                                    query_users.Add(Convert.ToString(dataReader["username"]));
                                                }
                                            }
                                            dataReader.Close();
                                            //Tarkistaa, ettei käyttäjänimeä ole vielä olemassa
                                            if (!query_users.Contains(textBox_account_name.Text))
                                            {
                                                query_family_keys = new List<string>();
                                                dataReader = cmd.ExecuteReader();
                                                //Luetaan perheavaimet tietokannasta
                                                while (dataReader.Read())
                                                {
                                                    if (dataReader["family_key"] != null)
                                                    {
                                                        query_family_keys.Add(Convert.ToString(dataReader["family_key"]));
                                                    }
                                                }
                                                dataReader.Close();
                                                //Tarkistaa, onko perheavainta jo olemassa
                                                if (query_family_keys.Contains(textBox_family_key.Text))
                                                {
                                                    //Ehdottaa perheeseen liittymistä
                                                    dialogResult = MessageBox.Show("Tämä perheavain on jo olemassa.\nHaluatko liittyä perheeseen " + textBox_family_key.Text + "?", "Rekisteröityminen", MessageBoxButtons.YesNo);
                                                    if (dialogResult == DialogResult.Yes)
                                                    {
                                                        //Käyttäjä rekisteröidään ja siirrytään takaisin Kirjautumiseen
                                                        cmd = new MySqlCommand(insertTableQuery, conn);
                                                        cmd.ExecuteNonQuery();
                                                        MessageBox.Show("Uusi " + comboBox_account_permissions.SelectedItem + " " + textBox_account_name.Text + " lisätty perheeseen " + textBox_family_key.Text, "Rekisteröityminen");
                                                        groupBox_login.Location = new Point(13, 65);
                                                        groupBox_login.Size = new Size(478, 337);
                                                        groupBox_login.Enabled = true;
                                                        groupBox_login.Visible = true;
                                                        groupBox_registration.Enabled = false;
                                                        groupBox_registration.Visible = false;
                                                    }
                                                }
                                                else
                                                {
                                                    //Ehdottaa uuden luomista
                                                    dialogResult = MessageBox.Show("Tätä perheavainta ei vielä ole olemassa.\nHaluatko luoda uuden " + textBox_family_key.Text + "?", "Rekisteröityminen", MessageBoxButtons.YesNo);
                                                    if (dialogResult == DialogResult.Yes)
                                                    {
                                                        //Käyttäjä rekisteröidään ja siirrytään takaisin Kirjautumiseen
                                                        cmd = new MySqlCommand(insertTableQuery, conn);
                                                        cmd.ExecuteNonQuery();
                                                        MessageBox.Show("Uusi " + comboBox_account_permissions.SelectedItem + " " + textBox_account_name.Text + " lisätty perheeseen " + textBox_family_key.Text, "Rekisteröityminen");
                                                        groupBox_login.Location = new Point(13, 65);
                                                        groupBox_login.Size = new Size(478, 337);
                                                        groupBox_login.Enabled = true;
                                                        groupBox_login.Visible = true;
                                                        groupBox_registration.Enabled = false;
                                                        groupBox_registration.Visible = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Käyttäjänimi on jo olemassa", "Rekisteröityminen");
                                            }
                                            conn.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(Convert.ToString(ex));
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Valitse käyttäjälle oikeudet", "Rekisteröityminen");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Perheavain ei saa sisältää erikoismerkkejä\nVain a-z, A-Z, 0-9 ja _", "Kirjautuminen");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Syötä vähintään neljä merkkiä pitkä perheavain", "Rekisteröityminen");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Syötä vähintään neljä merkkiä pitkä salasana", "Rekisteröityminen");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Käyttäjänimi ei saa sisältää erikoismerkkejä\nVain a-z, A-Z, 0-9 ja _", "Kirjautuminen");
                    }
                }
                else
                {
                    MessageBox.Show("Syötä vähintään neljä merkkiä pitkä käyttäjänimi", "Rekisteröityminen");
                }
            }
            else
            {
                MessageBox.Show("Tietokantaan ei saatu yhteyttä\nYritä korjata yhteytesi, jotta voit jatkaa\nTarkista, että sinulla on XAMPP yhteys päällä", "Tietokanta");
            }
        }

        private void button_return_Click(object sender, EventArgs e)
        {
            //Näkymäksi astetaan Kirjautuminen
            groupBox_login.Location = new Point(13, 65);
            groupBox_login.Size = new Size(478, 337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            //Piilotetaan Rekisteröityminen
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
            textBox_username.Select();
        }
    }
}
