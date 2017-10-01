namespace KernelGuideLib
{
    partial class COptionForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COptionForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.StartService_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopService_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RebootService_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.Exit_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "自动停车系统";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartService_ToolStripMenuItem,
            this.StopService_ToolStripMenuItem,
            this.RebootService_ToolStripMenuItem,
            this.toolStripSeparator2,
            this.Exit_ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 120);
            // 
            // StartService_ToolStripMenuItem
            // 
            this.StartService_ToolStripMenuItem.Enabled = false;
            this.StartService_ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("StartService_ToolStripMenuItem.Image")));
            this.StartService_ToolStripMenuItem.Name = "StartService_ToolStripMenuItem";
            this.StartService_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.StartService_ToolStripMenuItem.Text = "启动服务";
            this.StartService_ToolStripMenuItem.Click += new System.EventHandler(this.StartService_ToolStripMenuItem_Click);
            // 
            // StopService_ToolStripMenuItem
            // 
            this.StopService_ToolStripMenuItem.Enabled = false;
            this.StopService_ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("StopService_ToolStripMenuItem.Image")));
            this.StopService_ToolStripMenuItem.Name = "StopService_ToolStripMenuItem";
            this.StopService_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.StopService_ToolStripMenuItem.Text = "停止服务";
            this.StopService_ToolStripMenuItem.Click += new System.EventHandler(this.StopService_ToolStripMenuItem_Click);
            // 
            // RebootService_ToolStripMenuItem
            // 
            this.RebootService_ToolStripMenuItem.Enabled = false;
            this.RebootService_ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RebootService_ToolStripMenuItem.Image")));
            this.RebootService_ToolStripMenuItem.Name = "RebootService_ToolStripMenuItem";
            this.RebootService_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.RebootService_ToolStripMenuItem.Text = "重启服务";
            this.RebootService_ToolStripMenuItem.Click += new System.EventHandler(this.RebootService_ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // Exit_ToolStripMenuItem
            // 
            this.Exit_ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Exit_ToolStripMenuItem.Image")));
            this.Exit_ToolStripMenuItem.Name = "Exit_ToolStripMenuItem";
            this.Exit_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.Exit_ToolStripMenuItem.Text = "退出";
            this.Exit_ToolStripMenuItem.Click += new System.EventHandler(this.Exit_ToolStripMenuItem_Click);
            // 
            // COptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 76);
            this.Name = "COptionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem StartService_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopService_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RebootService_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem Exit_ToolStripMenuItem;
    }
}

