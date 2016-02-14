using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dwnldmanager
{
    public partial class InfoLoader : Form
    {
        public InfoLoader()
        {
            InitializeComponent();
        }

        public delegate void _SetBarProperties(int max, int value, bool closeForm = false);
        public void SetBarProperties(int max, int value, bool closeForm = false)
        { 
            if (this.InvokeRequired)
            {
                Invoke(new _SetBarProperties(SetBarProperties),max,value,closeForm);
            }
            else
            {
                progressBar.Maximum = max;
                progressBar.Value = value;

                if (closeForm)
                {
                    this.Close();
                }
            }
        
        }

        private void InfoLoader_Load(object sender, EventArgs e)
        {

        }
    }
}
