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

namespace Home_n_Life
{
    public partial class Kirjautuminen : Form
    {
        public Kirjautuminen()
        {
            InitializeComponent();
        }
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;";
        string query_user, query_password, query_family_key;
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader dataReader;
        DialogResult dialogResult;
        string dropTableQuery, createTableQuery, insertTableQuery, selectTableQuery, updateTableQuery, deleteTableQuery;

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
//----- Login --------------------------------------------------------------------------------------
        private void Kirjautuminen_Load(object sender, EventArgs e)
        {
            checkDatabaseConnection();
            groupBox_login.Location = new Point(13, 65);
            groupBox_login.Size = new Size(478, 337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
        }

        private void button_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_show_password_Click(object sender, EventArgs e)
        {
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
            conn = new MySqlConnection(connetionString);
            if (textBox_username.Text.Length > 0)
            {
                if (textBox_password.Text.Length > 0)
                {
                    query_user = "";
                    createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                                username VARCHAR(30) NOT NULL,
                                                password VARCHAR(30) NOT NULL,
                                                family_key VARCHAR(30) NOT NULL,
                                                permissions VARCHAR(30) NOT NULL);";
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
                        while (dataReader.Read())
                        {
                            if (dataReader["username"] != null)
                            {
                                query_user = Convert.ToString(dataReader["username"]);
                            }
                        }
                        dataReader.Close();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Convert.ToString(ex));
                    }
                    if (textBox_username.Text == query_user)
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
                        if (query_password == textBox_password.Text)
                        {
                            Etusivu etusivu = new Etusivu();
                            etusivu.Show();
                            etusivu.checkDatabaseConnection();
                            etusivu.initializeUserData(textBox_username.Text);
                            etusivu.searchFamilyMembers();
                            this.Hide();
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

        private void button_sign_up_Click(object sender, EventArgs e)
        {
            groupBox_registration.Location = new Point(13, 65);
            groupBox_registration.Size = new Size(478, 337);
            groupBox_registration.Enabled = true;
            groupBox_registration.Visible = true;
            groupBox_login.Enabled = false;
            groupBox_login.Visible = false;
            textBox_account_name.Text = "";
            textBox_account_password.Text = "";
            textBox_family_key.Text = "";
            comboBox_account_permissions.SelectedItem = null;
        }

//----- Sign up --------------------------------------------------------------------------------------

        private void button_create_account_Click(object sender, EventArgs e)
        {
            conn = new MySqlConnection(connetionString);
            if (textBox_account_name.Text.Length > 0)
            {
                if (textBox_account_password.Text.Length > 0)
                {
                    if (textBox_family_key.Text.Length >= 4)
                    {
                        if (comboBox_account_permissions.SelectedItem != null)
                        {
                            query_user = "";
                            createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                                username VARCHAR(30) NOT NULL,
                                                password VARCHAR(30) NOT NULL,
                                                family_key VARCHAR(30) NOT NULL,
                                                permissions VARCHAR(30) NOT NULL);";
                            selectTableQuery = @"SELECT id, username, password, family_key, permissions " +
                                                " FROM users ;";
                            insertTableQuery = @"INSERT INTO users (id, username, password, family_key, permissions) " +
                                                "VALUES('null', '" + textBox_account_name.Text + "', '" + textBox_account_password.Text + "', '" + textBox_family_key.Text + "', '" + Convert.ToString(comboBox_account_permissions.SelectedItem) + "');";
                            try
                            {
                                conn.Open();
                                cmd = new MySqlCommand(createTableQuery, conn);
                                cmd.ExecuteNonQuery();
                                cmd = new MySqlCommand(selectTableQuery, conn);
                                dataReader = cmd.ExecuteReader();
                                while (dataReader.Read())
                                {
                                    if (dataReader["username"] != null)
                                    {
                                        query_user = Convert.ToString(dataReader["username"]);
                                    }
                                }
                                dataReader.Close();
                                if (query_user != textBox_account_name.Text)
                                {
                                    query_family_key = "";
                                    dataReader = cmd.ExecuteReader();
                                    while (dataReader.Read())
                                    {
                                        if (dataReader["family_key"] != null)
                                        {
                                            query_family_key = Convert.ToString(dataReader["family_key"]);
                                        }
                                    }
                                    dataReader.Close();
                                    if (query_family_key != "")
                                    {
                                        dialogResult = MessageBox.Show("Tämä perheavainta on jo olemassa.\nHaluatko liittyä perheeseen " + query_family_key + "?", "Rekisteröityminen", MessageBoxButtons.YesNo);
                                        if (dialogResult == DialogResult.Yes)
                                        {
                                            cmd = new MySqlCommand(insertTableQuery, conn);
                                            cmd.ExecuteNonQuery();
                                            MessageBox.Show("Uusi " + comboBox_account_permissions.SelectedItem + " " + textBox_account_name.Text + " lisätty perheeseen " + query_family_key, "Rekisteröityminen");
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
                                        dialogResult = MessageBox.Show("Tätä perheavainta ei ole vielä olemassa.\nHaluatko luoda uuden " + textBox_family_key.Text + "?", "Rekisteröityminen", MessageBoxButtons.YesNo);
                                        if (dialogResult == DialogResult.Yes)
                                        {
                                            cmd = new MySqlCommand(insertTableQuery, conn);
                                            cmd.ExecuteNonQuery();
                                            MessageBox.Show("Uusi " + comboBox_account_permissions.SelectedItem + " " + textBox_account_name.Text + " lisätty perheeseen " + query_family_key, "Rekisteröityminen");
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
                        MessageBox.Show("Syötä minimissään neljä merkkiä pitkä perheavain", "Rekisteröityminen");
                    }
                }
                else
                {
                    MessageBox.Show("Syötä salasana", "Rekisteröityminen");
                }
            }
            else
            {
                MessageBox.Show("Syötä käyttäjänimi", "Rekisteröityminen");
            }
        }

        private void button_return_Click(object sender, EventArgs e)
        {
            groupBox_login.Location = new Point(13, 65);
            groupBox_login.Size = new Size(478, 337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
        }
    }
}
