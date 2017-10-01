using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WCFServiceLib;
using CommonMethodLib;
using LOGManagementLib;
using System.Diagnostics;
using BaseMethodLib;

namespace KernelGuideLib
{
    public partial class COptionForm : Form, IDisposable
    {
        private CMainCallback m_mainCallback;
        private CWCFManagement m_wcfManagement;
        private CWCFSubscription m_subscription;
        //private CSettingForm m_setting;
        private CMonitor m_monitor;     
        private bool bRunning;//服务运行标识

        public COptionForm(CMainCallback mainCallback)
        {
            //CLOGException.Trace("——————COptionForm");
            m_mainCallback = mainCallback;
            bRunning = false;
            m_wcfManagement = new CWCFManagement(m_mainCallback);
            m_subscription = new CWCFSubscription(m_mainCallback);
            m_monitor = new CMonitor();
          
            InitializeComponent();
        }
       
        /// <summary>
        /// 每当用户加载窗体时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Run()
        {
            try
            {
                //CLOGException.Trace("——————COptionForm_Load");
                if (bRunning == false)
                {
                    FrmSplash splash = new FrmSplash();
                    splash.Show();
                   
                    ActiveService();                   
                    ActiveSubscript();                   
                    //ActioveMaintainProc();
                    bRunning = true;                  
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.Run 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        ///// <summary>
        ///// 窗体加载时触发
        ///// </summary>
        ///// <param name="e"></param>
        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //}

        /// <summary>
        /// 重载该函数后启动程序后不加载界面，不触发Load事件
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        /// <summary>
        /// 运行WCF接口服务
        /// </summary>
        public void ActiveService()
        {
            m_wcfManagement.StartThreads();
        }

        /// <summary>
        /// 运行自动存取车服务
        /// </summary>
        public void ActiveSubscript()
        {
            m_subscription.StartSubscript();
        }

        /// <summary>
        /// 运行维护进程监控线程
        /// </summary>
        public void ActioveMaintainProc()
        {
            m_monitor.StartMonitor();
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                m_wcfManagement.Dispose();
                m_subscription.Dispose();
                m_monitor.Dispose();
                bRunning = false;
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.Dispose 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        public void Exit()
        {
            try
            {
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();
                bRunning = false;
                Killer();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.Exit 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        ///// <summary>
        ///// 界面关闭是触发
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void COptionForm_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    notifyIcon1.Visible = false;
        //    notifyIcon1.Dispose();
        //    Killer();
        //}

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartService_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (bRunning == false)
                {
                    ActiveService();
                    ActiveSubscript();
                    ActioveMaintainProc();
                    bRunning = true;
                }
                else
                {
                    MessageBox.Show("服务处于启动状态！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.StartService_ToolStripMenuItem_Click 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopService_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (bRunning == true)
                {
                    Dispose();
                    bRunning = false;
                }
                else
                {
                    MessageBox.Show("服务处于停止状态！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.StopService_ToolStripMenuItem_Click 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RebootService_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Dispose();
                bRunning = false;
                ActiveService();
                ActiveSubscript();
                ActioveMaintainProc();
                bRunning = true;
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.RebootService_ToolStripMenuItem_Click 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        ///// <summary>
        ///// 打开参数设置界面
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Setting_ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    //m_setting.Show();
        //}

        /// <summary>
        /// 退出服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                bRunning = false;
                Dispose();
                Exit();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.Exit_ToolStripMenuItem_Click 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 关闭进程
        /// </summary>
        private void Killer()
        {
            try
            {
                Process[] localByNameOM = Process.GetProcessesByName("MaintainProc");//想要执行程序的进程名称。

                //localByName得到的是进程中所有名称为"knl_proc.exe"的进程。
                if (localByNameOM.Length != 0) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
                {
                    foreach (Process process in localByNameOM)
                    {
                        if (process.ProcessName == "MaintainProc")
                        {
                            CLOGException.Trace("(Killer): Stop programme MaintainProc...");
                            process.Kill();
                        }
                    }
                }

                Process[] localByNameAP = Process.GetProcessesByName("AutoParkingSystem");//想要执行程序的进程名称。

                //localByName得到的是进程中所有名称为"knl_proc.exe"的进程。
                if (localByNameAP.Length != 0) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
                {
                    foreach (Process process in localByNameAP)
                    {
                        if (process.ProcessName == "AutoParkingSystem")
                        {
                            CLOGException.Trace("(Killer): Stop programme AutoParkingSystem...");
                            process.Kill();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.COptionForm.Killer 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }
    }
}
