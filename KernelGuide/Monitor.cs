using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using LOGManagementLib;
using BaseMethodLib;

namespace KernelGuideLib
{
    public class CMonitor : IDisposable
    {
        private Thread OMThread;
        private Process OMProcesses;

        /// <summary>
        /// 初始化函数
        /// </summary>
        public void StartMonitor()
        {
            OMThread = new Thread(new ThreadStart(RunProcess))
            { 
                IsBackground = true /*后台线程在主线程关闭时，子线程随之关闭，速度很快。*/ 
            };
            OMThread.Start();
            CLOGException.Trace("Monitor", "Initial MaintainProc successfully");
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public void Dispose()
        {
            if (null != OMThread && OMThread.ThreadState == System.Threading.ThreadState.Running)
            {
                OMThread.Abort();
            }
        }

        /// <summary>
        /// 启动进程
        /// </summary>
        private void RunProcess()
        {
            try
            {
                Process[] localByName = Process.GetProcessesByName("MaintainProc");//MaintainProc.exe是想要执行程序的进程名称。

                //localByName得到的是进程中所有名称为"MaintainProc.exe"的进程。
                if (localByName.Length == 0) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
                {
                    string strFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "MaintainProc.exe";
                    OMProcesses = new System.Diagnostics.Process();
                    OMProcesses.StartInfo.FileName = strFilePath;
                    OMProcesses.Start();
                    CLOGException.Trace("Monitor", "Create process(MaintainProc)");

                    OMProcesses.EnableRaisingEvents = true;
                    OMProcesses.Exited += new EventHandler(MonitorProcess);
                    if (!OMProcesses.HasExited)
                    {
                        CLOGException.Trace("Monitor", "Create process(MaintainProc) successfully");
                    }
                    while (true)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.CMonitor.InitProcess 异常 ", CBaseMethods.MyBase.GetExceptionInfo(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 监控进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorProcess(object sender, EventArgs e)
        {
            try
            {
                CLOGException.Trace("Monitor", "MaintainProc Abnormal exit!");
                //获取参数配置
                //创建新的 Process 实例，并将其与本地计算机上的进程资源关联
                //与类似的 GetProcessById、 GetProcessesByName 和 GetProcesses 三种方法一样，GetCurrentProcess 将现有资源与新的 Process 组件关联。
                Process[] localByName = Process.GetProcessesByName("MaintainProc");//MaintainProc.exe是想要执行程序的进程名称。

                //localByName得到的是进程中所有名称为"MaintainProc.exe"的进程。
                if (localByName.Length == 0) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
                {
                    string strFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "MaintainProc.exe";
                    OMProcesses = new System.Diagnostics.Process();
                    OMProcesses.StartInfo.FileName = strFilePath;
                    OMProcesses.Close();
                    OMProcesses.Start();
                    OMProcesses.EnableRaisingEvents = true;
                    OMProcesses.Exited += new EventHandler(MonitorProcess);
                    if (!OMProcesses.HasExited)
                    {
                        CLOGException.Trace("Monitor", "Restart process(MaintainProc) successfully");
                    }
                }
                else
                {
                    //如果程序已经启动，则执行这一部分代码
                }

            }
            catch (Exception ex)
            {
                CLOGException.Trace("KernelGuideLib.CMonitor.RunProcess 异常 ", CBaseMethods.MyBase.GetExceptionInfo(ex));
                throw ex;
            }
        }

    }
}
