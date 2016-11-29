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
        string connetionString = "server=localhost;database=home&life;uid=root;pwd=;", query_user, query_password;
        MySqlConnection conn;
        MySqlCommand cmd;
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

        private void button_return_Click(object sender, EventArgs e)
        {
            groupBox_login.Location = new Point(13, 65);
            groupBox_login.Size = new Size(478, 337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
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
            conn = new MySqlConnection(connetionString);
            if (textBox_username.Text.Length > 0)
            {
                if (textBox_password.Text.Length > 0)
                {
                    query_user = "";
                    createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        password VARCHAR(30) NOT NULL);";
                    selectTableQuery = @"SELECT id, username, password " +
                                        " FROM users ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(createTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        cmd = new MySqlCommand(selectTableQuery, conn);
                        MySqlDataReader dataReader = cmd.ExecuteReader();
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
                        selectTableQuery = @"SELECT id, username, password " +
                                        " FROM users " +
                                        " WHERE username='" + textBox_username.Text + "' ;";
                        try
                        {
                            conn.Open();
                            cmd = new MySqlCommand(createTableQuery, conn);
                            cmd.ExecuteNonQuery();
                            cmd = new MySqlCommand(selectTableQuery, conn);
                            MySqlDataReader dataReader = cmd.ExecuteReader();
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
                            Etusivu Etusivu = new Etusivu();
                            Etusivu.Show();
                            Etusivu.linkLabel_user.Text = textBox_username.Text;
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

        private void Kirjautuminen_Load(object sender, EventArgs e)
        {
            checkDatabaseConnection();
            groupBox_login.Location = new Point(13,65);
            groupBox_login.Size = new Size(478,337);
            groupBox_login.Enabled = true;
            groupBox_login.Visible = true;
            groupBox_registration.Enabled = false;
            groupBox_registration.Visible = false;
        }

        private void button_create_account_Click(object sender, EventArgs e)
        {
            conn = new MySqlConnection(connetionString);
            if (textBox_account_name.Text.Length > 0)
            {
                if (textBox_account_password.Text.Length > 0)
                {
                    query_user = "";
                    createTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                                        id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                        username VARCHAR(30) NOT NULL,
                                        password VARCHAR(30) NOT NULL);";
                    selectTableQuery = @"SELECT id, username, password " +
                                        " FROM users ;";
                    try
                    {
                        conn.Open();
                        cmd = new MySqlCommand(createTableQuery, conn);
                        cmd.ExecuteNonQuery();
                        cmd = new MySqlCommand(selectTableQuery, conn);
                        MySqlDataReader dataReader = cmd.ExecuteReader();
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
                    if (query_user != textBox_account_name.Text)
                    {
                        groupBox_login.Location = new Point(13, 65);
                        groupBox_login.Size = new Size(478, 337);
                        groupBox_login.Enabled = true;
                        groupBox_login.Visible = true;
                        groupBox_registration.Enabled = false;
                        groupBox_registration.Visible = false;
                    }
                    else
                    {
                        MessageBox.Show("Käyttäjänimi on jo olemassa","Rekisteröityminen");
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
        }
    }
}
