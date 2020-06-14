using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Frogger
{
    public partial class Form2 : Form
    {
        public bool colorlogs = true;
        public bool restart = false;
        public Form2()
        {
            InitializeComponent();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            colorlogs = LogColorCheckBox.Checked; 
            Form1.Instance.Text = colorlogs + "," + restart;
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            button1.Location = new Point(100, this.Height - 50);
            button2.Location = new Point(this.Width - 100, this.Height - 50);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            restart = true;
        }
    }
}
