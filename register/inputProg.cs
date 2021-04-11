using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    public partial class inputProg : Form
    {
        public string path;
        public string name;
        public DateTime date;

        public inputProg()
        {
            InitializeComponent();

            this.Load += LoadData;
        }

        private void LoadData(object sender, EventArgs e)
        {
            pathCmb.Text = path;
            nameTxt.Text = name;
            startDateDTP.Value = date;

            nameTxt.TextChanged += NameTxt_TextChanged;
            pathCmb.TextChanged += PathCmb_TextChanged;
        }

        private void PathCmb_TextChanged(object sender, EventArgs e)
        {
            var text = pathCmb.Text;
            if (text.Contains("/")){
                pathCmb.Text = text.Replace("/","-");
            }
        }

        private void NameTxt_TextChanged(object sender, EventArgs e)
        {
            var text = nameTxt.Text;
            if (text.Contains("/")){
                nameTxt.Text = text.Replace("/","-");
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            name = nameTxt.Text;
            path = pathCmb.Text;
            date = startDateDTP.Value;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void pathCmb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
