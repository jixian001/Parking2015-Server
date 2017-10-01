using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KernelGuideLib
{
    public partial class FrmSplash : Form
    {

        private Timer time1;
        private int cnt = 0;

        public FrmSplash()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            Bitmap bitmap = new Bitmap(KernelGuideLib.Properties.Resources.splashScrn);
            this.ClientSize = bitmap.Size;
            this.BackgroundImage = bitmap;
            this.BackgroundImageLayout = ImageLayout.Center;
            this.WindowState = FormWindowState.Normal;

            time1 = new Timer();
            time1.Interval = 15;           
            time1.Tick += new EventHandler(time1_Tick);
            time1.Enabled = true;
        }

        void time1_Tick(object sender, EventArgs e)
        {
            time1.Enabled = false;
            if (cnt > 1000)
            {
                this.Close();
                this.Dispose();
            }
            else
            {
                cnt += 10;
                this.Opacity = 1 - Convert.ToDouble(cnt) / 1000;
                time1.Enabled = true;
            }
        }

        public int Count
        {
            get
            {
                return cnt;
            }
            set
            {
                value = cnt;
            }
        }
    }
}
