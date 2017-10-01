using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MaintainProc
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isStart = false;
            while (isStart)
            {
                DateTime timeNow = DateTime.Now;
                if (timeNow.Hour == 2 && timeNow.Minute == 30)
                {
                    CFileManagement dbDataManagement = new CFileManagement();
                    dbDataManagement.DeleteLog();
                }
                Thread.Sleep(59000);
            }
        }
    }
}
