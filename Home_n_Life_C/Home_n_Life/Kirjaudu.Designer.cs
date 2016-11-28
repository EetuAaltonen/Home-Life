namespace Home_n_Life
{
    partial class Kirjautuminen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.button_exit = new System.Windows.Forms.Button();
            this.button_minimize = new System.Windows.Forms.Button();
            this.groupBox_navigation = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox_login = new System.Windows.Forms.GroupBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_sign_up = new System.Windows.Forms.Button();
            this.button_login = new System.Windows.Forms.Button();
            this.groupBox_registration = new System.Windows.Forms.GroupBox();
            this.textBox_account_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button_create_account = new System.Windows.Forms.Button();
            this.textBox_account_name = new System.Windows.Forms.TextBox();
            this.button_show_password = new System.Windows.Forms.Button();
            this.button_return = new System.Windows.Forms.Button();
            this.groupBox_navigation.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox_login.SuspendLayout();
            this.groupBox_registration.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_username
            // 
            this.textBox_username.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_username.Location = new System.Drawing.Point(99, 96);
            this.textBox_username.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.textBox_username.MaxLength = 20;
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(272, 26);
            this.textBox_username.TabIndex = 9;
            this.textBox_username.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_exit
            // 
            this.button_exit.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_exit.ForeColor = System.Drawing.Color.White;
            this.button_exit.Location = new System.Drawing.Point(214, 8);
            this.button_exit.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(60, 30);
            this.button_exit.TabIndex = 10;
            this.button_exit.Text = "Exit";
            this.button_exit.UseVisualStyleBackColor = false;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // button_minimize
            // 
            this.button_minimize.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_minimize.ForeColor = System.Drawing.Color.White;
            this.button_minimize.Location = new System.Drawing.Point(148, 8);
            this.button_minimize.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_minimize.Name = "button_minimize";
            this.button_minimize.Size = new System.Drawing.Size(60, 30);
            this.button_minimize.TabIndex = 11;
            this.button_minimize.Text = "Pienennä";
            this.button_minimize.UseVisualStyleBackColor = false;
            this.button_minimize.Click += new System.EventHandler(this.button_minimize_Click);
            // 
            // groupBox_navigation
            // 
            this.groupBox_navigation.BackColor = System.Drawing.Color.Black;
            this.groupBox_navigation.Controls.Add(this.flowLayoutPanel1);
            this.groupBox_navigation.Controls.Add(this.label5);
            this.groupBox_navigation.Location = new System.Drawing.Point(-3, 0);
            this.groupBox_navigation.Name = "groupBox_navigation";
            this.groupBox_navigation.Size = new System.Drawing.Size(511, 59);
            this.groupBox_navigation.TabIndex = 12;
            this.groupBox_navigation.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button_exit);
            this.flowLayoutPanel1.Controls.Add(this.button_minimize);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(217, 9);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(277, 44);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(15, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(191, 37);
            this.label5.TabIndex = 12;
            this.label5.Text = "Home\'N\'Life";
            // 
            // groupBox_login
            // 
            this.groupBox_login.BackColor = System.Drawing.Color.Black;
            this.groupBox_login.Controls.Add(this.textBox_password);
            this.groupBox_login.Controls.Add(this.label2);
            this.groupBox_login.Controls.Add(this.button_show_password);
            this.groupBox_login.Controls.Add(this.label1);
            this.groupBox_login.Controls.Add(this.button_sign_up);
            this.groupBox_login.Controls.Add(this.textBox_username);
            this.groupBox_login.Controls.Add(this.button_login);
            this.groupBox_login.Enabled = false;
            this.groupBox_login.Location = new System.Drawing.Point(12, 109);
            this.groupBox_login.Name = "groupBox_login";
            this.groupBox_login.Size = new System.Drawing.Size(34, 29);
            this.groupBox_login.TabIndex = 13;
            this.groupBox_login.TabStop = false;
            this.groupBox_login.Visible = false;
            // 
            // textBox_password
            // 
            this.textBox_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_password.Location = new System.Drawing.Point(99, 179);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.textBox_password.MaxLength = 20;
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(272, 26);
            this.textBox_password.TabIndex = 16;
            this.textBox_password.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(94, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 29);
            this.label2.TabIndex = 15;
            this.label2.Text = "Salasana";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(94, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 29);
            this.label1.TabIndex = 14;
            this.label1.Text = "Käyttäjänimi";
            // 
            // button_sign_up
            // 
            this.button_sign_up.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_sign_up.ForeColor = System.Drawing.Color.White;
            this.button_sign_up.Location = new System.Drawing.Point(124, 274);
            this.button_sign_up.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_sign_up.Name = "button_sign_up";
            this.button_sign_up.Size = new System.Drawing.Size(101, 30);
            this.button_sign_up.TabIndex = 13;
            this.button_sign_up.Text = "Rekisteröidy";
            this.button_sign_up.UseVisualStyleBackColor = false;
            this.button_sign_up.Click += new System.EventHandler(this.button_sign_up_Click);
            // 
            // button_login
            // 
            this.button_login.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_login.ForeColor = System.Drawing.Color.White;
            this.button_login.Location = new System.Drawing.Point(231, 274);
            this.button_login.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(101, 30);
            this.button_login.TabIndex = 12;
            this.button_login.Text = "Kirjaudu";
            this.button_login.UseVisualStyleBackColor = false;
            this.button_login.Click += new System.EventHandler(this.button_login_Click);
            // 
            // groupBox_registration
            // 
            this.groupBox_registration.BackColor = System.Drawing.Color.Black;
            this.groupBox_registration.Controls.Add(this.button_return);
            this.groupBox_registration.Controls.Add(this.textBox_account_password);
            this.groupBox_registration.Controls.Add(this.label3);
            this.groupBox_registration.Controls.Add(this.label4);
            this.groupBox_registration.Controls.Add(this.button_create_account);
            this.groupBox_registration.Controls.Add(this.textBox_account_name);
            this.groupBox_registration.Enabled = false;
            this.groupBox_registration.Location = new System.Drawing.Point(12, 65);
            this.groupBox_registration.Name = "groupBox_registration";
            this.groupBox_registration.Size = new System.Drawing.Size(34, 38);
            this.groupBox_registration.TabIndex = 17;
            this.groupBox_registration.TabStop = false;
            this.groupBox_registration.Visible = false;
            // 
            // textBox_account_password
            // 
            this.textBox_account_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_account_password.Location = new System.Drawing.Point(99, 179);
            this.textBox_account_password.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.textBox_account_password.MaxLength = 20;
            this.textBox_account_password.Name = "textBox_account_password";
            this.textBox_account_password.Size = new System.Drawing.Size(272, 26);
            this.textBox_account_password.TabIndex = 16;
            this.textBox_account_password.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(94, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 29);
            this.label3.TabIndex = 15;
            this.label3.Text = "Salasana";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(94, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 29);
            this.label4.TabIndex = 14;
            this.label4.Text = "Käyttäjänimi";
            // 
            // button_create_account
            // 
            this.button_create_account.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_create_account.ForeColor = System.Drawing.Color.White;
            this.button_create_account.Location = new System.Drawing.Point(231, 274);
            this.button_create_account.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_create_account.Name = "button_create_account";
            this.button_create_account.Size = new System.Drawing.Size(101, 30);
            this.button_create_account.TabIndex = 13;
            this.button_create_account.Text = "Rekisteröidy";
            this.button_create_account.UseVisualStyleBackColor = false;
            this.button_create_account.Click += new System.EventHandler(this.button_create_account_Click);
            // 
            // textBox_account_name
            // 
            this.textBox_account_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_account_name.Location = new System.Drawing.Point(99, 96);
            this.textBox_account_name.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.textBox_account_name.MaxLength = 20;
            this.textBox_account_name.Name = "textBox_account_name";
            this.textBox_account_name.Size = new System.Drawing.Size(272, 26);
            this.textBox_account_name.TabIndex = 9;
            this.textBox_account_name.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_show_password
            // 
            this.button_show_password.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_show_password.ForeColor = System.Drawing.Color.White;
            this.button_show_password.Location = new System.Drawing.Point(377, 179);
            this.button_show_password.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_show_password.Name = "button_show_password";
            this.button_show_password.Size = new System.Drawing.Size(49, 26);
            this.button_show_password.TabIndex = 12;
            this.button_show_password.Text = "Näytä";
            this.button_show_password.UseVisualStyleBackColor = false;
            this.button_show_password.Click += new System.EventHandler(this.button_show_password_Click);
            // 
            // button_return
            // 
            this.button_return.BackColor = System.Drawing.Color.DodgerBlue;
            this.button_return.ForeColor = System.Drawing.Color.White;
            this.button_return.Location = new System.Drawing.Point(124, 274);
            this.button_return.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.button_return.Name = "button_return";
            this.button_return.Size = new System.Drawing.Size(101, 30);
            this.button_return.TabIndex = 17;
            this.button_return.Text = "Takaisin";
            this.button_return.UseVisualStyleBackColor = false;
            this.button_return.Click += new System.EventHandler(this.button_return_Click);
            // 
            // Kirjautuminen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(503, 414);
            this.Controls.Add(this.groupBox_registration);
            this.Controls.Add(this.groupBox_login);
            this.Controls.Add(this.groupBox_navigation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Kirjautuminen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Home&Life - Kirjautuminen";
            this.Load += new System.EventHandler(this.Kirjautuminen_Load);
            this.groupBox_navigation.ResumeLayout(false);
            this.groupBox_navigation.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox_login.ResumeLayout(false);
            this.groupBox_login.PerformLayout();
            this.groupBox_registration.ResumeLayout(false);
            this.groupBox_registration.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.Button button_minimize;
        private System.Windows.Forms.GroupBox groupBox_navigation;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox_login;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_sign_up;
        private System.Windows.Forms.GroupBox groupBox_registration;
        private System.Windows.Forms.TextBox textBox_account_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_create_account;
        private System.Windows.Forms.TextBox textBox_account_name;
        private System.Windows.Forms.Button button_show_password;
        private System.Windows.Forms.Button button_return;
    }
}

