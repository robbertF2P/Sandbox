namespace ActiveDirectoryTester
{
    partial class FloorganiseForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.serviceUserTb = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ldapStringTb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ldapServerTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ldapPortTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.servicePasswordTb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.userTb = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.passwordTb = new System.Windows.Forms.TextBox();
            this.consoleLb = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Service gebruiker";
            // 
            // serviceUserTb
            // 
            this.serviceUserTb.Location = new System.Drawing.Point(143, 23);
            this.serviceUserTb.Name = "serviceUserTb";
            this.serviceUserTb.Size = new System.Drawing.Size(150, 20);
            this.serviceUserTb.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ldapStringTb);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.ldapServerTb);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.ldapPortTb);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.servicePasswordTb);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.serviceUserTb);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(309, 167);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // ldapStringTb
            // 
            this.ldapStringTb.Location = new System.Drawing.Point(143, 127);
            this.ldapStringTb.Name = "ldapStringTb";
            this.ldapStringTb.Size = new System.Drawing.Size(150, 20);
            this.ldapStringTb.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 130);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Ldap connection string";
            // 
            // ldapServerTb
            // 
            this.ldapServerTb.Location = new System.Drawing.Point(143, 101);
            this.ldapServerTb.Name = "ldapServerTb";
            this.ldapServerTb.Size = new System.Drawing.Size(150, 20);
            this.ldapServerTb.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Ldap server";
            // 
            // ldapPortTb
            // 
            this.ldapPortTb.Location = new System.Drawing.Point(143, 75);
            this.ldapPortTb.Name = "ldapPortTb";
            this.ldapPortTb.Size = new System.Drawing.Size(150, 20);
            this.ldapPortTb.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Ldap poort";
            // 
            // servicePasswordTb
            // 
            this.servicePasswordTb.Location = new System.Drawing.Point(143, 49);
            this.servicePasswordTb.Name = "servicePasswordTb";
            this.servicePasswordTb.Size = new System.Drawing.Size(150, 20);
            this.servicePasswordTb.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Service wachtwoord";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.userTb);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.passwordTb);
            this.groupBox2.Location = new System.Drawing.Point(327, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(309, 167);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Test";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 78);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(284, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Controleer";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // userTb
            // 
            this.userTb.Location = new System.Drawing.Point(153, 23);
            this.userTb.Name = "userTb";
            this.userTb.Size = new System.Drawing.Size(150, 20);
            this.userTb.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Gebruiker";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 52);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Wachtwoord";
            // 
            // passwordTb
            // 
            this.passwordTb.Location = new System.Drawing.Point(153, 49);
            this.passwordTb.Name = "passwordTb";
            this.passwordTb.Size = new System.Drawing.Size(150, 20);
            this.passwordTb.TabIndex = 1;
            // 
            // consoleLb
            // 
            this.consoleLb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.consoleLb.FormattingEnabled = true;
            this.consoleLb.Location = new System.Drawing.Point(12, 197);
            this.consoleLb.Name = "consoleLb";
            this.consoleLb.Size = new System.Drawing.Size(623, 108);
            this.consoleLb.TabIndex = 4;
            // 
            // FloorganiseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 319);
            this.Controls.Add(this.consoleLb);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FloorganiseForm";
            this.Text = "Active directory tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serviceUserTb;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox ldapStringTb;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ldapServerTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox ldapPortTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox servicePasswordTb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox userTb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox passwordTb;
        private System.Windows.Forms.ListBox consoleLb;
    }
}

