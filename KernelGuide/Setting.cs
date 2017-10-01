using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConfigManagmtLib;

namespace KernelGuideLib
{
    public partial class CSettingForm : Form
    {
        public CSettingForm()
        {
            InitializeComponent();
        }
        private void Setting_Load(object sender, EventArgs e)
        {
            //初始化根节点
            initParent();

            this.Controls.Add(TvwSetting);
        }

        private void TvwSetting_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (null == TvwSetting.SelectedNode)
            {
                if (e.Button == MouseButtons.Left && TvwSetting.GetNodeAt(e.X, e.Y) != null)
                {
                    TvwSetting.SelectedNode = TvwSetting.GetNodeAt(e.X, e.Y);
                }
                TvwSetting.SelectedNode = (TreeNode)e.Node;
            }
            if (null != TvwSetting.SelectedNode && 0 == TvwSetting.SelectedNode.Name.CompareTo("CommonSettings"))
            {

            }
            foreach (TreeNode treeNode in TvwSetting.SelectedNode.Nodes)
            {
                foreach (int nPLCID in CConfigManagement.myPara.LstPLCID)
                {
                    string strPLCID = "PLC" + nPLCID;
                    if (0 == treeNode.Name.CompareTo(strPLCID))
                    {

                    }
                }

            }

        }

        //初始化根节点
        void initParent()
        {
            TreeNode tn = new TreeNode();
            tn.Text = "PLCSettings";
            tn.Tag = "1";
            this.TvwSetting.Nodes.Add(tn);
            //初始化下级节点
            initLeaf(tn);

        }
        //初始化下级节点
        void initLeaf(TreeNode tn)
        {
            foreach (int nPLCID in CConfigManagement.myPara.LstPLCID)
            {
                TreeNode ctn = new TreeNode();
                ctn.Text = "PLC" + nPLCID;
                ctn.Tag = nPLCID.ToString();
                tn.Nodes.Add(ctn);
            }
            //递归调用，不断循环至叶节点
            //initLeaf(ctn);
        }
    }
}
