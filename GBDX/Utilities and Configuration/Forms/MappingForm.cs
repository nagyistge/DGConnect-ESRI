﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gbdx.Utilities_and_Configuration.Forms
{
    public partial class MappingForm : Form
    {

        public MappingForm()
        {
            InitializeComponent();
        }

        public string ItemName
        {
            get
            {
                return this.itemNameTextBox.Text;
            }
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void itemNameTextBox_TextChanged(object sender, EventArgs e)
        {
            this.countLabel.Text = this.itemNameTextBox.Text.Length + "/30";
        }
    }
}
