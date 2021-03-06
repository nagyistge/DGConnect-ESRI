﻿namespace Gbdx.Vector_Index.Forms
{
    partial class VectorIndexDockable
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.selectAreaButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.aoiTypeComboBox = new System.Windows.Forms.ComboBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 148F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxSearch, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.clearButton, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.selectAreaButton, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.aoiTypeComboBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.treeView1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(300, 613);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxSearch, 2);
            this.textBoxSearch.ForeColor = System.Drawing.Color.DarkGray;
            this.textBoxSearch.Location = new System.Drawing.Point(3, 3);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(294, 22);
            this.textBoxSearch.TabIndex = 3;
            this.textBoxSearch.Text = "Enter search terms and select area";
            // 
            // clearButton
            // 
            this.clearButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.clearButton.Location = new System.Drawing.Point(207, 582);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(90, 27);
            this.clearButton.TabIndex = 2;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.EventHandlerClearButtonClick);
            // 
            // selectAreaButton
            // 
            this.selectAreaButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.selectAreaButton.Location = new System.Drawing.Point(3, 583);
            this.selectAreaButton.Name = "selectAreaButton";
            this.selectAreaButton.Size = new System.Drawing.Size(90, 26);
            this.selectAreaButton.TabIndex = 0;
            this.selectAreaButton.Text = "Select Area";
            this.selectAreaButton.UseVisualStyleBackColor = true;
            this.selectAreaButton.Click += new System.EventHandler(this.EventHandlerSelectAreaButtonClick);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 555);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Selection Type";
            // 
            // aoiTypeComboBox
            // 
            this.aoiTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.aoiTypeComboBox.FormattingEnabled = true;
            this.aoiTypeComboBox.Items.AddRange(new object[] {
            "Draw Rectangle",
            "Use Selected Polygon(s)"});
            this.aoiTypeComboBox.Location = new System.Drawing.Point(155, 549);
            this.aoiTypeComboBox.Name = "aoiTypeComboBox";
            this.aoiTypeComboBox.Size = new System.Drawing.Size(142, 24);
            this.aoiTypeComboBox.TabIndex = 4;
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.CheckBoxes = true;
            this.tableLayoutPanel1.SetColumnSpan(this.treeView1, 2);
            this.treeView1.Location = new System.Drawing.Point(3, 48);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(294, 493);
            this.treeView1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Location = new System.Drawing.Point(3, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Case-Sensitive";
            // 
            // VectorIndexDockable
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VectorIndexDockable";
            this.Size = new System.Drawing.Size(306, 616);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button selectAreaButton;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ComboBox aoiTypeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}
