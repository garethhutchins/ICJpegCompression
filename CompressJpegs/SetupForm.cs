using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompressJpegs
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
            
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
            {
                txt_TempPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            //Save the values
            tempPath = txt_TempPath.Text;
            Compression = Convert.ToDouble(ud_compression.Value);
            this.Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            //Load the default values
            txt_TempPath.Text = tempPath;
            ud_compression.Value = Convert.ToDecimal(Compression);
        }
    }
}
