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

        private void button_exit_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

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
            string str = " CREATE DATABASE My_Database_Test";
            SqlConnection cnn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            SqlCommand myCommand = new SqlCommand(str, cnn);
            try
            {
                cnn.Open();
                MessageBox.Show("Connection Open ! ");
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! ");
            }
            try
            {
                cnn.Open();
                myCommand.ExecuteNonQuery();
                MessageBox.Show("Database has been created successfully!",
                                  "Create Database", MessageBoxButtons.OK,
                                              MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Create Database",
                                            MessageBoxButtons.OK,
                                     MessageBoxIcon.Information);
            }
            finally
            {
                cnn.Close();
            }
            return;
        }
    }
}
