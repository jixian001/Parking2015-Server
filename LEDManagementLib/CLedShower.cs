using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using LOGManagementLib;
using System.Threading;

namespace LEDManagementLib
{
    public class CLedShower
    {
        #region 控制卡接口函数
        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_ComInitial", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_ComInitial(int comport, int baudrate, int timeout, int retrytimes);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_ComClose", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_ComClose();

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_ShowString", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_ShowString(int bank, int xpos, int ypos, int color, byte[] str);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_SetLEDProperty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_SetLEDProperty(int atype, int num, int width, int height, int color, int lockHZ);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_SendPicture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CL2005_SendPicture(int index, byte[] buff);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_SendProgList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CL2005_SendProgList(int count, byte[] Prog);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_SwitchToBank", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_SwitchToBank(int bank);

        [DllImport("CL2005_API_StdCall.dll", EntryPoint = "CL2005_ClearBank", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CL2005_ClearBank(int bank);
        #endregion

        private bool ledConn = false;
        private bool SetLedPropertyisok = false;
        private int LedControlCardType = 1;
        private int Ledwide = 64;
        private int LedHigh = 16;
        //private bool swichtobank = false;
        private int nBaudRate = 9600;
        private int timeout = 1600;
        private int timesRetry = 1000;

        private int mport;
        private int num=0;

        public CLedShower(int com) 
        {
            mport = com;
        }

        public void ConnectLed() 
        {
            int mCount = 0;
            try
            {
                ledConn = false;
                SetLedPropertyisok = false;

                while (!ledConn && mCount < 3)
                {
                    mCount = mCount + 1;
                    ledConn = CL2005_ComInitial(mport,nBaudRate,timeout,timesRetry);//连接串口（两参数串口+频率）
                }
                if (ledConn)
                {
                    SetLedPropertyisok = CL2005_SetLEDProperty(LedControlCardType,num, Ledwide, LedHigh, 0, LedControlCardType);                             
                }
                
            }
            catch (Exception e1)
            {
                CLOGException.Trace("CLedShower.ConnectLed 异常", "端口号：COM" + mport + ", " + e1.ToString());
            }            
        }

        public void disConnectLed()
        {
            try
            {              
                CL2005_ComClose();
            }
            catch (Exception e1)
            {
                CLOGException.Trace("CLedShower.disConnectLed 异常", "端口号：COM" + mport + ", " + e1.ToString());
            }
        }

        public void DisplayMessagge(string msg)
        {
            try
            {
                if (SetLedPropertyisok)
                {
                    byte[] buffer1 = Encoding.UTF8.GetBytes(msg);
                    Encoding utf8 = Encoding.GetEncoding(65001);
                    Encoding gb2312 = Encoding.GetEncoding("gb2312");
                    byte[] buffer = Encoding.Convert(utf8, gb2312, buffer1);

                    if (!CL2005_ShowString(0, 0, 0, 1, buffer))
                    {
                        if (CL2005_ShowString(1, 0, 0, 1, buffer))
                        {
                            CL2005_SwitchToBank(1);
                        }
                        else 
                        {
                            if (!CL2005_ShowString(0, 0, 0, 1, buffer))
                            {
                                CL2005_SwitchToBank(0);
                            }
                        }
                    }
                    else
                    {
                        CL2005_SwitchToBank(0);
                    }
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("CLedShower.DisplayMessagge 异常", "端口号：COM" + mport + ", " + ex.ToString());
            }
        }       

    }
}
