using PMA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PMAtoXML
{
    public partial class Form1 : Form
    {
        string path;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog x = new OpenFileDialog();
            x.Filter = "PMA file|*.txt;";
            DialogResult result = x.ShowDialog();
            if ((result == DialogResult.OK))
            {
                PMAFile.convertToXML(x.FileName);
            }

            PMAFile.readXML(x.FileName);

            path = x.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PMAFile.convertToTXT(path);
        }
    }
}
