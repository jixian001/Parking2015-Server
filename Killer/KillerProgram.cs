using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LOGManagementLib;

namespace IEGKiller
{
    class CKillerProgram
    {
        static void Main(string[] args)
        {
            Process[] localByNameKnl = Process.GetProcessesByName("AutoParkingSystem");//想要执行程序的进程名称。

            //localByName得到的是进程中所有名称为"knl_proc.exe"的进程。
            if (localByNameKnl.Length != 0) //如果得到的进程数是0, 那么说明程序未启动，需要启动程序
            {
                foreach (Process process in localByNameKnl)
                {
                    if (process.ProcessName == "AutoParkingSystem")
                    {
                        CLOGException.Trace("(Killer): Stop programme AutoParkingSystem...");
                        process.Kill();
                    }
                }
            }
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

        }
    }
}
