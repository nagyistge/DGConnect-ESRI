namespace Gbdx.Aggregations
{
    partial class Aggregations
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
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.QuerySelectionComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.detailLevelComboBox = new System.Windows.Forms.ComboBox();
            this.specifyDateCheckbox = new UIToolbox.CheckGroupBox();
            this.dateRangeTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.changeDetectionTabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.mainTableLayout.SuspendLayout();
            this.specifyDateCheckbox.SuspendLayout();
            this.dateRangeTableLayout.SuspendLayout();
            this.changeDetectionTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayout
            // 
            this.mainTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.ColumnCount = 3;
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.mainTableLayout.Controls.Add(this.label1, 0, 0);
            this.mainTableLayout.Controls.Add(this.QuerySelectionComboBox, 0, 1);
            this.mainTableLayout.Controls.Add(this.label2, 0, 2);
            this.mainTableLayout.Controls.Add(this.detailLevelComboBox, 1, 2);
            this.mainTableLayout.Controls.Add(this.specifyDateCheckbox, 0, 3);
            this.mainTableLayout.Controls.Add(this.label5, 0, 5);
            this.mainTableLayout.Controls.Add(this.textBox1, 0, 6);
            this.mainTableLayout.Controls.Add(this.changeDetectionTabControl, 0, 7);
            this.mainTableLayout.Location = new System.Drawing.Point(3, 3);
            this.mainTableLayout.Name = "mainTableLayout";
            this.mainTableLayout.RowCount = 10;
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 107F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 183F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 89F));
            this.mainTableLayout.Size = new System.Drawing.Size(326, 698);
            this.mainTableLayout.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.mainTableLayout.SetColumnSpan(this.label1, 3);
            this.label1.Location = new System.Drawing.Point(77, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "What are you looking for?";
            // 
            // QuerySelectionComboBox
            // 
            this.QuerySelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.SetColumnSpan(this.QuerySelectionComboBox, 3);
            this.QuerySelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QuerySelectionComboBox.FormattingEnabled = true;
            this.QuerySelectionComboBox.Items.AddRange(new object[] {
            "What data is available in the region?",
            "What is the twitter sentiment in the region?",
            "What type of data is available in the region?"});
            this.QuerySelectionComboBox.Location = new System.Drawing.Point(3, 30);
            this.QuerySelectionComboBox.Name = "QuerySelectionComboBox";
            this.QuerySelectionComboBox.Size = new System.Drawing.Size(320, 24);
            this.QuerySelectionComboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Detail Level";
            // 
            // detailLevelComboBox
            // 
            this.detailLevelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.SetColumnSpan(this.detailLevelComboBox, 2);
            this.detailLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.detailLevelComboBox.FormattingEnabled = true;
            this.detailLevelComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.detailLevelComboBox.Location = new System.Drawing.Point(111, 73);
            this.detailLevelComboBox.Name = "detailLevelComboBox";
            this.detailLevelComboBox.Size = new System.Drawing.Size(212, 24);
            this.detailLevelComboBox.TabIndex = 3;
            // 
            // specifyDateCheckbox
            // 
            this.specifyDateCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.SetColumnSpan(this.specifyDateCheckbox, 3);
            this.specifyDateCheckbox.Controls.Add(this.dateRangeTableLayout);
            this.specifyDateCheckbox.Location = new System.Drawing.Point(3, 114);
            this.specifyDateCheckbox.Name = "specifyDateCheckbox";
            this.specifyDateCheckbox.Size = new System.Drawing.Size(320, 104);
            this.specifyDateCheckbox.TabIndex = 4;
            this.specifyDateCheckbox.TabStop = false;
            this.specifyDateCheckbox.Text = "Date Range";
            // 
            // dateRangeTableLayout
            // 
            this.dateRangeTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateRangeTableLayout.ColumnCount = 2;
            this.dateRangeTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.61017F));
            this.dateRangeTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63.38983F));
            this.dateRangeTableLayout.Controls.Add(this.dateTimePicker2, 1, 1);
            this.dateRangeTableLayout.Controls.Add(this.label3, 0, 0);
            this.dateRangeTableLayout.Controls.Add(this.dateTimePicker1, 1, 0);
            this.dateRangeTableLayout.Controls.Add(this.label4, 0, 1);
            this.dateRangeTableLayout.Location = new System.Drawing.Point(11, 21);
            this.dateRangeTableLayout.Name = "dateRangeTableLayout";
            this.dateRangeTableLayout.RowCount = 2;
            this.dateRangeTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.dateRangeTableLayout.Size = new System.Drawing.Size(303, 77);
            this.dateRangeTableLayout.TabIndex = 1;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePicker2.CustomFormat = "";
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker2.Location = new System.Drawing.Point(113, 46);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(187, 22);
            this.dateTimePicker2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Starting Date";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePicker1.CustomFormat = "";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(113, 8);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(187, 22);
            this.dateTimePicker1.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 17);
            this.label4.TabIndex = 1;
            this.label4.Text = "Ending Date";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.mainTableLayout.SetColumnSpan(this.label5, 2);
            this.label5.Location = new System.Drawing.Point(3, 245);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 17);
            this.label5.TabIndex = 5;
            this.label5.Text = "Query Filter";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.SetColumnSpan(this.textBox1, 3);
            this.textBox1.Location = new System.Drawing.Point(3, 272);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(320, 101);
            this.textBox1.TabIndex = 6;
            // 
            // changeDetectionTabControl
            // 
            this.changeDetectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayout.SetColumnSpan(this.changeDetectionTabControl, 3);
            this.changeDetectionTabControl.Controls.Add(this.tabPage1);
            this.changeDetectionTabControl.Controls.Add(this.tabPage2);
            this.changeDetectionTabControl.Controls.Add(this.tabPage3);
            this.changeDetectionTabControl.Location = new System.Drawing.Point(3, 379);
            this.changeDetectionTabControl.Name = "changeDetectionTabControl";
            this.changeDetectionTabControl.SelectedIndex = 0;
            this.changeDetectionTabControl.Size = new System.Drawing.Size(320, 177);
            this.changeDetectionTabControl.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(312, 148);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Change Detection";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(246, 148);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "MLTC";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(246, 148);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Multi-Change Detection";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // Aggregations
            // 
            this.Controls.Add(this.mainTableLayout);
            this.Name = "Aggregations";
            this.Size = new System.Drawing.Size(332, 704);
            this.mainTableLayout.ResumeLayout(false);
            this.mainTableLayout.PerformLayout();
            this.specifyDateCheckbox.ResumeLayout(false);
            this.specifyDateCheckbox.PerformLayout();
            this.dateRangeTableLayout.ResumeLayout(false);
            this.dateRangeTableLayout.PerformLayout();
            this.changeDetectionTabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox QuerySelectionComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox detailLevelComboBox;
        private UIToolbox.CheckGroupBox specifyDateCheckbox;
        private System.Windows.Forms.TableLayoutPanel dateRangeTableLayout;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabControl changeDetectionTabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;

    }
}
