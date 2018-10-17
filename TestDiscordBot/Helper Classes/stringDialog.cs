using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestDiscordBot
{
    public partial class stringDialog : Form
    {
        public string result = "";

        public stringDialog(string Question, string preText)
        {
            InitializeComponent();
            Text = Question;
            textBox1.Text = preText;
        }

        private void done_Click(object sender, EventArgs e)
        {
            result = textBox1.Text;
            Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                done_Click(this, EventArgs.Empty);
        }

        private void stringDialog_Load(object sender, EventArgs e)
        {

        }

        private void stringDialog_Shown(object sender, EventArgs e)
        {
            this.BringToFront();
        }
    }
}
