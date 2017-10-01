using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LOGManagementLib;
using BaseMethodLib;
using System.Diagnostics;

namespace AutoParkingSystem
{
    static class MainProgram
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        { 
            try
            {
                CheckProcStat();               
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CGuideContext());
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AutoParkingSystem.CProgram.Main 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        static void CheckProcStat()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess(); 

                Process[] localByName = Process.GetProcessesByName("AutoParkingSystem");//想要执行程序的进程名称。

                //localByName得到的是进程中所有名称为"knl_proc.exe"的进程。
                if (localByName.Length > 1) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
                {
                    foreach (Process process in localByName)
                    {
                        if (process.Id != currentProc.Id && process.ProcessName == "AutoParkingSystem")
                        {
                            MessageBox.Show("服务处于启动状态！请退出后再重新启动。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            //currentProc.Close();
                            //currentProc.Dispose();
                            currentProc.Kill();
                            return;
                        }
                    }
                }
                else
                {
                    CLOGException.Trace("Start programme...(Ver:AutoParkingV100R001C01SPC100|AutoParkingV100R001C01SPC100)");
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AutoParkingSystem.CProgram.Main 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                throw new Exception(CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
