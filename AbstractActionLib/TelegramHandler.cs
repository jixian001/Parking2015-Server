using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using LOGManagementLib;
using ConfigManagmtLib;
using CommunicationLib;
using CommonMethodLib;
using BaseMethodLib;

namespace AbstractActionLib
{
    public class CTelegramHandler
    {
        private int m_nWareHouse = 0;
        private CCommModule m_PLCServer = new CCommModule();// OPC服务器
        private object m_objData = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="m_commModule"></param>
        /// <param name="data"></param>
        /// <param name="?"></param>
        public CTelegramHandler(int nWareHouse, object data)
        {
            m_nWareHouse = nWareHouse;
            //             m_commModule = commModule;
            m_objData = data;
        }

        public void Init()
        {
            try
            {
                //             System.Timers.Timer tmpTimer = new System.Timers.Timer(5000); //Timer的时间间隔为1000毫秒 = 1s
                //             tmpTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
                //             tmpTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
                //             tmpTimer.Elapsed += new System.Timers.ElapsedEventHandler(tmpTimer_Elapsed);
                // 
                m_PLCServer = CCommonMethods.myMethods.GetCommModule(m_nWareHouse);
                if (null == m_PLCServer)
                {
                    CLOGException.Trace("WorkFlowLib.CWorkFlow.InitialTelegramMgmt", "获取通信模式块失败");
                    return; //获取通信模式块失败，请查看配置文件是否正确
                }
                // add by wanxiaona20150728
                if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(m_nWareHouse))
                {
                    CLOGException.Trace("WorkFlowLib.CWorkFlow.InitialTelegramMgmt", "无效库区号");
                    return;
                }
                // add by wanxiaona20150728
                //    lock (m_commModule)
                //{
                m_PLCServer.Initialize((List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[m_nWareHouse], false);
                bool bIsSended = false;
                bool bIsACKReceived = false;
                while (!bIsSended || !bIsACKReceived)
                {
                    bIsSended = SendData();
                    Int16[] data = (Int16[])m_objData;
                    if (bIsSended)
                    {
                        int nMillisecond = 0;
                        while (nMillisecond < 10000)
                        {
                            short recvBuffer = 0;
                            bool isOK = m_PLCServer.Read(ref recvBuffer, 1);
                            if (isOK)
                            {
                                if (recvBuffer == 9999)
                                {
                                    Int16[] ackTelegram = null;
                                    m_PLCServer.Read(ref ackTelegram, 0);// 读取PLC回应ACK报文

                                    bIsACKReceived = CBaseMethods.MyBase.IsPLCAck((Int16[])data, ackTelegram);
                                    if (bIsACKReceived)
                                    {
                                        //更新PLC发送缓冲区标志位为0
                                        if (m_PLCServer.Write(0, 1))
                                        {
                                            ThreadPool.QueueUserWorkItem(new WaitCallback(ReWritePLCFlag));
                                        }

                                        //接收报文ack成功
                                        CCommonMethods.myMethods.InsertTelegramLog(ackTelegram, 0);
                                        break;
                                    }
                                    else
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        Int16[] dataTmp = (Int16[])ackTelegram;
                                        sb.Append("EMS:" + dataTmp[0] + "|");
                                        sb.Append("PLC:" + dataTmp[1] + "|");
                                        sb.Append("TeleType:" + dataTmp[2] + "|");
                                        sb.Append("EMSSubType:" + dataTmp[3] + "|");
                                        sb.Append("PLCSubType:" + dataTmp[4] + "|");
                                        sb.Append("DeviceID:" + dataTmp[6] + "|");
                                        sb.Append("ID:" + dataTmp[48]);
                                        string telegram = sb.ToString();
                                        CLOGException.Trace(m_nWareHouse, "WorkFlowLib.TelegramHandler", "Send telegram failed：" + telegram);
                                    }
                                }
                            }
                            Thread.Sleep(1000);//4000
                            nMillisecond += 1000;
                        }
                        if (bIsACKReceived)
                        {
                            break;
                        }
                    }
                }
                CLOGException.Trace(m_nWareHouse, "WorkFlowLib.TelegramHandler", "***************************WorkFlowLib.CTelegramHandler.Init*****************************");
                m_PLCServer.Dispose();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CTelegramHandler.Init 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        public void Init(object stateInfo)
        {
            try
            {             
                m_PLCServer = CCommonMethods.myMethods.GetCommModule(m_nWareHouse);
                if (null == m_PLCServer)
                {
                    CLOGException.Trace("WorkFlowLib.CWorkFlow.InitialTelegramMgmt", "获取通信模式块失败");
                    return; //获取通信模式块失败，请查看配置文件是否正确
                }
               
                if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(m_nWareHouse))
                {
                    return;
                }
           
                m_PLCServer.Initialize((List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[m_nWareHouse], false);
                bool bIsSended = false;
                bool bIsACKReceived = false;
                while (!bIsSended || !bIsACKReceived)
                {
                    bIsSended = SendData();
                    Int16[] data = (Int16[])m_objData;
                    if (bIsSended)
                    {
                        int nMillisecond = 0;
                        while (nMillisecond < 10000)
                        {
                            short recvBuffer = 0;
                            bool isOK = m_PLCServer.Read(ref recvBuffer, 1);
                            if (isOK)
                            {
                                if (recvBuffer == 9999)
                                {
                                    Int16[] ackTelegram = null;
                                    m_PLCServer.Read(ref ackTelegram, 0);// 读取PLC回应ACK报文

                                    bIsACKReceived = CBaseMethods.MyBase.IsPLCAck((Int16[])data, ackTelegram);
                                    if (bIsACKReceived)
                                    {
                                        //更新PLC发送缓冲区标志位为0
                                        if (m_PLCServer.Write(0, 1))
                                        {
                                            ThreadPool.QueueUserWorkItem(new WaitCallback(ReWritePLCFlag));
                                        }

                                        //接收报文ack成功
                                        CCommonMethods.myMethods.InsertTelegramLog(ackTelegram, 0);
                                        break;
                                    }
                                    else
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        Int16[] dataTmp = (Int16[])ackTelegram;
                                        sb.Append("EMS:" + dataTmp[0] + "|");
                                        sb.Append("PLC:" + dataTmp[1] + "|");
                                        sb.Append("TeleType:" + dataTmp[2] + "|");
                                        sb.Append("EMSSubType:" + dataTmp[3] + "|");
                                        sb.Append("PLCSubType:" + dataTmp[4] + "|");
                                        sb.Append("DeviceID:" + dataTmp[6] + "|");
                                        sb.Append("ID:" + dataTmp[48]);
                                        string telegram = sb.ToString();
                                        CLOGException.Trace(m_nWareHouse, "WorkFlowLib.TelegramHandler", "Send telegram failed：" + telegram);
                                    }
                                }
                            }
                            Thread.Sleep(1000);
                            nMillisecond += 1000;
                        }
                        if (bIsACKReceived)
                        {
                            break;
                        }
                    }
                }
                CLOGException.Trace(m_nWareHouse, "WorkFlowLib.TelegramHandler", "***************************WorkFlowLib.CTelegramHandler.Init*****************************");
                m_PLCServer.Dispose();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CTelegramHandler.Init(object stateInfo) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        private void tmpTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CLOGException.Trace("***************************播放报文发送失败语音*****************************");
        }
        /// <summary>
        /// 分库发送报文
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wareHouse"></param>
        /// <returns></returns>
        private bool SendData()
        {
            if (null == m_PLCServer)
            {
                return false;
            }
            bool isok = false;
            if (m_PLCServer.IsConnected)
            {
                #region add 20150229 在发送报文前，先判断PLC发送缓冲区是否为空，如果是ACK，没有办法再读取了，则强制清0,以让PLC发送缓冲区处于空闲状态
                bool isACK = false;
                Int16 recvBuffer = 0;
                bool isExit = m_PLCServer.Read(ref recvBuffer, 1);// 读取PLC发送缓冲区标志位
                if (isExit)
                {
                    if (recvBuffer == 9999)
                    {
                        short[] recvTele = new short[50];
                        bool isRecvOK = m_PLCServer.Read(ref recvTele, 0);
                        if (isRecvOK)
                        {
                            if (recvTele[4] == 9999)
                            {
                                isACK = true;
                                Thread.Sleep(400);
                            }
                        }
                    }
                    if (isACK)
                    {
                        recvBuffer = 0;
                        isExit = m_PLCServer.Read(ref recvBuffer, 1);
                        if (isExit)
                        {
                            if (recvBuffer == 9999)
                            {
                                m_PLCServer.Write(0, 1);
                            }
                        }
                    }
                }
                #endregion
                Int16 wflag = 0;
                bool wisgood = m_PLCServer.Read(ref wflag, 3);// 读取PLC接收缓冲区标志位

                if (wisgood)// 读取数据有效Good
                {
                    if ((Int16)wflag == 0)// 数据已经被PLC处理（PLC数据为空，此时可以填充数据）
                    {
                        wisgood = m_PLCServer.Write(m_objData, 2);// 向OPC写数据                                    

                        if (wisgood)// 写入数据正常完成
                        {
                            wflag = 9999;
                            wisgood = m_PLCServer.Write(wflag, 3);// 更新PLC接收缓冲区标志位为9999
                            if (!wisgood)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ReWriteEMSFlag));
                            }
                            isok = wisgood;
                        }
                    }
                }
                if (isok)
                {
                    //发送报文成功
                    CCommonMethods.myMethods.InsertTelegramLog(m_objData, 1);
                }
            }
            else
            {
                m_PLCServer.ReConnectServer();
            }

            return isok;
        }

        /// <summary>
        /// 重写PLC将要向EMS发送信息缓冲区标志位为0
        /// </summary>
        private void ReWritePLCFlag(object obj)
        {
            try
            {
                if (null == m_PLCServer)
                {
                    return;
                }
                bool bFlag = false;
                while (!bFlag)
                {
                    Thread.Sleep(50);//4000
                    bFlag = m_PLCServer.Write(0, 1);//更新PLC发送缓冲区标志位为0
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CTelegramHandler.ReWritePLCFlag 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 重写PLC接收EMS信息缓冲区标志位为9999
        /// </summary>
        private void ReWriteEMSFlag(object obj)
        {
            try
            {
                if (null == m_PLCServer)
                {
                    return;
                }
                bool bFlag = false;
                while (!bFlag)
                {
                    Thread.Sleep(50);//4000
                    bFlag = m_PLCServer.Write(9999, 3);// 更新PLC接收缓冲区标志位为9999
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CTelegramHandler.ReWritePLCFlag 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
