namespace Gbdx
{
    partial class FormConfiguration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConfiguration));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.UserNameTextBox = new System.Windows.Forms.TextBox();
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.fileGdbDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(493, 131);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(92, 26);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "Save";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.buttonTest, 2);
            this.buttonTest.Location = new System.Drawing.Point(4, 133);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(4);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(205, 23);
            this.buttonTest.TabIndex = 4;
            this.buttonTest.Text = "Test Connection";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.ButtonTestClick);
            // 
            // UserNameTextBox
            // 
            this.UserNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.UserNameTextBox, 5);
            this.UserNameTextBox.Location = new System.Drawing.Point(104, 5);
            this.UserNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.UserNameTextBox.Name = "UserNameTextBox";
            this.UserNameTextBox.Size = new System.Drawing.Size(481, 22);
            this.UserNameTextBox.TabIndex = 2;
            this.UserNameTextBox.TextChanged += new System.EventHandler(this.TextBoxUrlTextChanged);
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.UsernameLabel.AutoSize = true;
            this.UsernameLabel.Location = new System.Drawing.Point(19, 7);
            this.UsernameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = new System.Drawing.Size(77, 17);
            this.UsernameLabel.TabIndex = 6;
            this.UsernameLabel.Text = "Username:";
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(23, 39);
            this.PasswordLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(73, 17);
            this.PasswordLabel.TabIndex = 8;
            this.PasswordLabel.Text = "Password:";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.PasswordTextBox, 5);
            this.PasswordTextBox.Location = new System.Drawing.Point(104, 36);
            this.PasswordTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(481, 22);
            this.PasswordTextBox.TabIndex = 3;
            this.PasswordTextBox.TextChanged += new System.EventHandler(this.TextBoxUrlTextChanged);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.CancelButton.Location = new System.Drawing.Point(418, 131);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(67, 26);
            this.CancelButton.TabIndex = 6;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 111F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Controls.Add(this.UsernameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.PasswordLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CancelButton, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 5, 4);
            this.tableLayoutPanel1.Controls.Add(this.browseButton, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonTest, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.fileGdbDirectoryTextBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.PasswordTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.UserNameTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.urlTextBox, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 5);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.1462F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.1228F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.5848F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.1462F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(589, 162);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 17);
            this.label2.TabIndex = 9;
            this.label2.Text = "File Location:";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(492, 68);
            this.browseButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(94, 23);
            this.browseButton.TabIndex = 11;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButtonClick);
            // 
            // fileGdbDirectoryTextBox
            // 
            this.fileGdbDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.fileGdbDirectoryTextBox, 4);
            this.fileGdbDirectoryTextBox.Location = new System.Drawing.Point(103, 68);
            this.fileGdbDirectoryTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileGdbDirectoryTextBox.Name = "fileGdbDirectoryTextBox";
            this.fileGdbDirectoryTextBox.Size = new System.Drawing.Size(383, 22);
            this.fileGdbDirectoryTextBox.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "Url";
            // 
            // urlTextBox
            // 
            this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.urlTextBox, 5);
            this.urlTextBox.Location = new System.Drawing.Point(103, 100);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(483, 22);
            this.urlTextBox.TabIndex = 14;
            this.urlTextBox.TextChanged += new System.EventHandler(this.TextBoxUrlTextChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(609, 201);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(601, 172);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Account Setup";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(601, 172);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Experimental Setup";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // FormConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 224);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(799, 798);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(622, 188);
            this.Name = "FormConfiguration";
            this.Text = "Information and GBDX Platform Settings";
            this.TextChanged += new System.EventHandler(this.TextBoxUrlTextChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.TextBox UserNameTextBox;
        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox fileGdbDirectoryTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}