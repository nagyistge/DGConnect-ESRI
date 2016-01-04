namespace Gbdx
{
    partial class FormQueryBuilder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormQueryBuilder));
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.rbCustom = new System.Windows.Forms.RadioButton();
            this.rb6Months = new System.Windows.Forms.RadioButton();
            this.rb3Months = new System.Windows.Forms.RadioButton();
            this.rb1Month = new System.Windows.Forms.RadioButton();
            this.rb7Days = new System.Windows.Forms.RadioButton();
            this.rb1Day = new System.Windows.Forms.RadioButton();
            this.rb12Hours = new System.Windows.Forms.RadioButton();
            this.rb6Hours = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.keywords = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioButton10000 = new System.Windows.Forms.RadioButton();
            this.radioButton5000 = new System.Windows.Forms.RadioButton();
            this.radioButton2500 = new System.Windows.Forms.RadioButton();
            this.radioButton1000 = new System.Windows.Forms.RadioButton();
            this.radioButton500 = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.radioButtonDensity = new System.Windows.Forms.RadioButton();
            this.radioButtonPointsMap = new System.Windows.Forms.RadioButton();
            this.radioButtonTable = new System.Windows.Forms.RadioButton();
            this.advancedOptionsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.favoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerStart.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(91, 22);
            this.dateTimePickerStart.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(141, 22);
            this.dateTimePickerStart.TabIndex = 0;
            this.dateTimePickerStart.Value = new System.DateTime(2014, 5, 21, 0, 0, 0, 0);
            this.dateTimePickerStart.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DateTimePickerStartKeyDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Location = new System.Drawing.Point(4, 92);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(413, 210);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Time Interval:";
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.rbCustom);
            this.groupBox6.Controls.Add(this.rb6Months);
            this.groupBox6.Controls.Add(this.rb3Months);
            this.groupBox6.Controls.Add(this.rb1Month);
            this.groupBox6.Controls.Add(this.rb7Days);
            this.groupBox6.Controls.Add(this.rb1Day);
            this.groupBox6.Controls.Add(this.rb12Hours);
            this.groupBox6.Controls.Add(this.rb6Hours);
            this.groupBox6.Location = new System.Drawing.Point(5, 22);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox6.Size = new System.Drawing.Size(397, 89);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Last";
            // 
            // rbCustom
            // 
            this.rbCustom.AutoSize = true;
            this.rbCustom.Location = new System.Drawing.Point(267, 62);
            this.rbCustom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbCustom.Name = "rbCustom";
            this.rbCustom.Size = new System.Drawing.Size(76, 21);
            this.rbCustom.TabIndex = 7;
            this.rbCustom.TabStop = true;
            this.rbCustom.Text = "Custom";
            this.rbCustom.UseVisualStyleBackColor = true;
            this.rbCustom.CheckedChanged += new System.EventHandler(this.RbCustomCheckedChanged);
            // 
            // rb6Months
            // 
            this.rb6Months.AutoSize = true;
            this.rb6Months.Location = new System.Drawing.Point(181, 62);
            this.rb6Months.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb6Months.Name = "rb6Months";
            this.rb6Months.Size = new System.Drawing.Size(87, 21);
            this.rb6Months.TabIndex = 6;
            this.rb6Months.TabStop = true;
            this.rb6Months.Text = "6 Months";
            this.rb6Months.UseVisualStyleBackColor = true;
            this.rb6Months.CheckedChanged += new System.EventHandler(this.Rb6MonthsCheckedChanged);
            // 
            // rb3Months
            // 
            this.rb3Months.AutoSize = true;
            this.rb3Months.Location = new System.Drawing.Point(88, 62);
            this.rb3Months.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb3Months.Name = "rb3Months";
            this.rb3Months.Size = new System.Drawing.Size(87, 21);
            this.rb3Months.TabIndex = 5;
            this.rb3Months.TabStop = true;
            this.rb3Months.Text = "3 Months";
            this.rb3Months.UseVisualStyleBackColor = true;
            this.rb3Months.CheckedChanged += new System.EventHandler(this.Rb3MonthsCheckedChanged);
            // 
            // rb1Month
            // 
            this.rb1Month.AutoSize = true;
            this.rb1Month.Location = new System.Drawing.Point(5, 62);
            this.rb1Month.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb1Month.Name = "rb1Month";
            this.rb1Month.Size = new System.Drawing.Size(80, 21);
            this.rb1Month.TabIndex = 4;
            this.rb1Month.TabStop = true;
            this.rb1Month.Text = "1 Month";
            this.rb1Month.UseVisualStyleBackColor = true;
            this.rb1Month.CheckedChanged += new System.EventHandler(this.Rb1MonthCheckedChanged);
            // 
            // rb7Days
            // 
            this.rb7Days.AutoSize = true;
            this.rb7Days.Location = new System.Drawing.Point(256, 21);
            this.rb7Days.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb7Days.Name = "rb7Days";
            this.rb7Days.Size = new System.Drawing.Size(73, 21);
            this.rb7Days.TabIndex = 3;
            this.rb7Days.TabStop = true;
            this.rb7Days.Text = "7 Days";
            this.rb7Days.UseVisualStyleBackColor = true;
            this.rb7Days.CheckedChanged += new System.EventHandler(this.Rb7DaysCheckedChanged);
            // 
            // rb1Day
            // 
            this.rb1Day.AutoSize = true;
            this.rb1Day.Location = new System.Drawing.Point(184, 21);
            this.rb1Day.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb1Day.Name = "rb1Day";
            this.rb1Day.Size = new System.Drawing.Size(66, 21);
            this.rb1Day.TabIndex = 2;
            this.rb1Day.TabStop = true;
            this.rb1Day.Text = "1 Day";
            this.rb1Day.UseVisualStyleBackColor = true;
            this.rb1Day.CheckedChanged += new System.EventHandler(this.Rb1DayCheckedChanged);
            // 
            // rb12Hours
            // 
            this.rb12Hours.AutoSize = true;
            this.rb12Hours.Location = new System.Drawing.Point(91, 21);
            this.rb12Hours.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb12Hours.Name = "rb12Hours";
            this.rb12Hours.Size = new System.Drawing.Size(87, 21);
            this.rb12Hours.TabIndex = 1;
            this.rb12Hours.TabStop = true;
            this.rb12Hours.Text = "12 Hours";
            this.rb12Hours.UseVisualStyleBackColor = true;
            this.rb12Hours.CheckedChanged += new System.EventHandler(this.Rb12HoursCheckedChanged);
            // 
            // rb6Hours
            // 
            this.rb6Hours.AutoSize = true;
            this.rb6Hours.Location = new System.Drawing.Point(5, 21);
            this.rb6Hours.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rb6Hours.Name = "rb6Hours";
            this.rb6Hours.Size = new System.Drawing.Size(79, 21);
            this.rb6Hours.TabIndex = 0;
            this.rb6Hours.TabStop = true;
            this.rb6Hours.Text = "6 Hours";
            this.rb6Hours.UseVisualStyleBackColor = true;
            this.rb6Hours.CheckedChanged += new System.EventHandler(this.Rb6HoursCheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.dateTimePickerStart);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.dateTimePickerEnd);
            this.groupBox3.Location = new System.Drawing.Point(5, 116);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(397, 87);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Set Custom Time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 57);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "End Time:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Start Time:";
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerEnd.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(91, 57);
            this.dateTimePickerEnd.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new System.Drawing.Size(141, 22);
            this.dateTimePickerEnd.TabIndex = 1;
            this.dateTimePickerEnd.Value = new System.DateTime(2014, 5, 21, 0, 0, 0, 0);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 515);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip.Size = new System.Drawing.Size(445, 25);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "Working ...";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(18, 20);
            this.toolStripStatusLabel.Text = "...";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.keywords);
            this.groupBox2.Location = new System.Drawing.Point(4, 4);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(413, 60);
            this.groupBox2.TabIndex = 2000;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Keyword Search:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Image = global::GbdxSettings.GbdxResources.emptyStar;
            this.button1.Location = new System.Drawing.Point(372, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 30);
            this.button1.TabIndex = 1;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.PictureBoxFavoriteClick);
            // 
            // keywords
            // 
            this.keywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.keywords.Location = new System.Drawing.Point(9, 23);
            this.keywords.Margin = new System.Windows.Forms.Padding(4);
            this.keywords.MaxLength = 256;
            this.keywords.Name = "keywords";
            this.keywords.Size = new System.Drawing.Size(353, 22);
            this.keywords.TabIndex = 0;
            this.keywords.Text = "*";
            this.keywords.TextChanged += new System.EventHandler(this.KeywordsTextChanged);
            this.keywords.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeywordsKeyPress);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox4, 2);
            this.groupBox4.Controls.Add(this.radioButton10000);
            this.groupBox4.Controls.Add(this.radioButton5000);
            this.groupBox4.Controls.Add(this.radioButton2500);
            this.groupBox4.Controls.Add(this.radioButton1000);
            this.groupBox4.Controls.Add(this.radioButton500);
            this.groupBox4.Location = new System.Drawing.Point(4, 312);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(413, 62);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Limit Query:";
            // 
            // radioButton10000
            // 
            this.radioButton10000.AutoSize = true;
            this.radioButton10000.Location = new System.Drawing.Point(309, 26);
            this.radioButton10000.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton10000.Name = "radioButton10000";
            this.radioButton10000.Size = new System.Drawing.Size(44, 21);
            this.radioButton10000.TabIndex = 10;
            this.radioButton10000.Text = "All";
            this.radioButton10000.UseVisualStyleBackColor = true;
            this.radioButton10000.CheckedChanged += new System.EventHandler(this.RadioButton1000CheckedChanged);
            // 
            // radioButton5000
            // 
            this.radioButton5000.AutoSize = true;
            this.radioButton5000.Location = new System.Drawing.Point(236, 26);
            this.radioButton5000.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton5000.Name = "radioButton5000";
            this.radioButton5000.Size = new System.Drawing.Size(61, 21);
            this.radioButton5000.TabIndex = 9;
            this.radioButton5000.Text = "5000";
            this.radioButton5000.UseVisualStyleBackColor = true;
            // 
            // radioButton2500
            // 
            this.radioButton2500.AutoSize = true;
            this.radioButton2500.Checked = true;
            this.radioButton2500.Location = new System.Drawing.Point(157, 26);
            this.radioButton2500.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton2500.Name = "radioButton2500";
            this.radioButton2500.Size = new System.Drawing.Size(61, 21);
            this.radioButton2500.TabIndex = 6;
            this.radioButton2500.TabStop = true;
            this.radioButton2500.Text = "2500";
            this.radioButton2500.UseVisualStyleBackColor = true;
            // 
            // radioButton1000
            // 
            this.radioButton1000.AutoSize = true;
            this.radioButton1000.Location = new System.Drawing.Point(77, 26);
            this.radioButton1000.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1000.Name = "radioButton1000";
            this.radioButton1000.Size = new System.Drawing.Size(61, 21);
            this.radioButton1000.TabIndex = 7;
            this.radioButton1000.Text = "1000";
            this.radioButton1000.UseVisualStyleBackColor = true;
            // 
            // radioButton500
            // 
            this.radioButton500.AutoSize = true;
            this.radioButton500.Location = new System.Drawing.Point(11, 26);
            this.radioButton500.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton500.Name = "radioButton500";
            this.radioButton500.Size = new System.Drawing.Size(53, 21);
            this.radioButton500.TabIndex = 6;
            this.radioButton500.Text = "500";
            this.radioButton500.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(4, 449);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 27);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(317, 449);
            this.buttonRun.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(100, 27);
            this.buttonRun.TabIndex = 3;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.ButtonRunClick);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox5, 2);
            this.groupBox5.Controls.Add(this.radioButtonDensity);
            this.groupBox5.Controls.Add(this.radioButtonPointsMap);
            this.groupBox5.Controls.Add(this.radioButtonTable);
            this.groupBox5.Location = new System.Drawing.Point(4, 383);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox5.Size = new System.Drawing.Size(413, 58);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Results Display:";
            // 
            // radioButtonDensity
            // 
            this.radioButtonDensity.AutoSize = true;
            this.radioButtonDensity.Location = new System.Drawing.Point(276, 31);
            this.radioButtonDensity.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonDensity.Name = "radioButtonDensity";
            this.radioButtonDensity.Size = new System.Drawing.Size(76, 21);
            this.radioButtonDensity.TabIndex = 13;
            this.radioButtonDensity.Text = "Density";
            this.radioButtonDensity.UseVisualStyleBackColor = true;
            this.radioButtonDensity.Visible = false;
            // 
            // radioButtonPointsMap
            // 
            this.radioButtonPointsMap.AutoSize = true;
            this.radioButtonPointsMap.Checked = true;
            this.radioButtonPointsMap.Location = new System.Drawing.Point(16, 31);
            this.radioButtonPointsMap.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonPointsMap.Name = "radioButtonPointsMap";
            this.radioButtonPointsMap.Size = new System.Drawing.Size(119, 21);
            this.radioButtonPointsMap.TabIndex = 7;
            this.radioButtonPointsMap.TabStop = true;
            this.radioButtonPointsMap.Text = "Points on Map";
            this.radioButtonPointsMap.UseVisualStyleBackColor = true;
            // 
            // radioButtonTable
            // 
            this.radioButtonTable.AutoSize = true;
            this.radioButtonTable.Location = new System.Drawing.Point(157, 31);
            this.radioButtonTable.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonTable.Name = "radioButtonTable";
            this.radioButtonTable.Size = new System.Drawing.Size(65, 21);
            this.radioButtonTable.TabIndex = 11;
            this.radioButtonTable.Text = "Table";
            this.radioButtonTable.UseVisualStyleBackColor = true;
            this.radioButtonTable.Visible = false;
            // 
            // advancedOptionsLinkLabel
            // 
            this.advancedOptionsLinkLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.advancedOptionsLinkLabel, 2);
            this.advancedOptionsLinkLabel.Location = new System.Drawing.Point(3, 68);
            this.advancedOptionsLinkLabel.Name = "advancedOptionsLinkLabel";
            this.advancedOptionsLinkLabel.Size = new System.Drawing.Size(162, 17);
            this.advancedOptionsLinkLabel.TabIndex = 2;
            this.advancedOptionsLinkLabel.TabStop = true;
            this.advancedOptionsLinkLabel.Text = "Show Advanced Options";
            this.advancedOptionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AdvancedOptionsLinkLabelLinkClicked);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.35461F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.64539F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.advancedOptionsLinkLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.buttonRun, 1, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 31);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(421, 481);
            this.tableLayoutPanel1.TabIndex = 2001;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.favoritesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(445, 28);
            this.menuStrip1.TabIndex = 2002;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // favoritesToolStripMenuItem
            // 
            this.favoritesToolStripMenuItem.Name = "favoritesToolStripMenuItem";
            this.favoritesToolStripMenuItem.Size = new System.Drawing.Size(80, 24);
            this.favoritesToolStripMenuItem.Text = "Favorites";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(123, 28);
            // 
            // deleteMenuItem
            // 
            this.deleteMenuItem.Name = "deleteMenuItem";
            this.deleteMenuItem.Size = new System.Drawing.Size(122, 24);
            this.deleteMenuItem.Text = "Delete";
            // 
            // FormQueryBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 540);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormQueryBuilder";
            this.Text = "SMA Query Builder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox keywords;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioButton10000;
        private System.Windows.Forms.RadioButton radioButton5000;
        private System.Windows.Forms.RadioButton radioButton2500;
        private System.Windows.Forms.RadioButton radioButton1000;
        private System.Windows.Forms.RadioButton radioButton500;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton radioButtonPointsMap;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.RadioButton radioButtonDensity;
        private System.Windows.Forms.RadioButton radioButtonTable;
        private System.Windows.Forms.LinkLabel advancedOptionsLinkLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton rb6Months;
        private System.Windows.Forms.RadioButton rb3Months;
        private System.Windows.Forms.RadioButton rb1Month;
        private System.Windows.Forms.RadioButton rb7Days;
        private System.Windows.Forms.RadioButton rb1Day;
        private System.Windows.Forms.RadioButton rb12Hours;
        private System.Windows.Forms.RadioButton rb6Hours;
        private System.Windows.Forms.RadioButton rbCustom;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem favoritesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button button1;
    }
}

