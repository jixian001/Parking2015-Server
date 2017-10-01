namespace KernelGuideLib
{
    partial class CSettingForm
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
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("CommonSettings");
            DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(global::KernelGuideLib.CSplashScreenForm), true, true);
            this.TvwSetting = new System.Windows.Forms.TreeView();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ParaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // TvwSetting
            // 
            this.TvwSetting.Location = new System.Drawing.Point(0, -1);
            this.TvwSetting.Name = "TvwSetting";
            treeNode4.Name = "CommonSettings";
            treeNode4.Text = "CommonSettings";
            this.TvwSetting.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4});
            this.TvwSetting.Size = new System.Drawing.Size(196, 472);
            this.TvwSetting.TabIndex = 0;
            this.TvwSetting.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TvwSetting_NodeMouseClick);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ParaName,
            this.Value});
            this.dataGridView1.Location = new System.Drawing.Point(202, -1);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(504, 472);
            this.dataGridView1.TabIndex = 1;
            // 
            // ParaName
            // 
            this.ParaName.HeaderText = "参数";
            this.ParaName.Name = "ParaName";
            this.ParaName.Width = 230;
            // 
            // Value
            // 
            this.Value.HeaderText = "参数值";
            this.Value.Name = "Value";
            this.Value.Width = 230;
            // 
            // CSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(706, 471);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.TvwSetting);
            this.Name = "CSettingForm";
            this.Text = "Setting";
            this.Load += new System.EventHandler(this.Setting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView TvwSetting;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParaName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;

    }
}