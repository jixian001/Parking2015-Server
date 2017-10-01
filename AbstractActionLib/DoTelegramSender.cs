using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommunicationLib;
using CommonMethodLib;
using System.Text;

namespace AbstractActionLib
{
    /// <summary>
    /// 发送报文动作 1
    /// </summary>
    public class CTelegramSender
    {
        private int m_nTelegramLen = CConfigManagement.myPara.ReceiveAndSendBufferLen;//读取配置文件
        private int m_nByteLen = CConfigManagement.myPara.EquipStatusAndAlarmsLen;//读取配置文件
        private int m_retryCount = CConfigManagement.myPara.TelegramResendTime;// 读取配置文件
        private const Int16 m_nSendAckFlag = 9999;//读取配置文件
        private CCommModule m_PLCServer = new CCommModule();// OPC服务器
        private CTelegramParse m_telegramParser = new CTelegramParse();

        /// <summary>
        /// 构造函数
        /// </summary>
        public CTelegramSender()
        { }
        /// <summary>
        /// 构造函数
        /// </summary>
        public CTelegramSender(CCommModule OPCServer)
        {
            m_PLCServer = OPCServer;
        }

        /// <summary>
        /// 报文下发
        /// </summary>
        /// <param name="objRequestParam"></param>
        public void TelegramSender(object objRequestParam)
        {
            try
            {
                CRequestAction RequestParam = (CRequestAction)objRequestParam;
                m_PLCServer = RequestParam.PLCServer;
                // 发送报文
                foreach (struTelegram struSendTelegram in RequestParam.lstTelegram)
                {
                    CWException.WriteLog(string.Format("Telegram ({0},{1}) will send ,devicecode:{2},iccode:{3}",
                                struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS, struSendTelegram.nEquipID, struSendTelegram.strICCardID), 4);

                    Int16[] objTelegram;
                    m_telegramParser.PackTelegram(out objTelegram, struSendTelegram);

                    int warehouse = struSendTelegram.nEMSSendFlag;
                    int equipID = struSendTelegram.nEquipID;
                    //存在该位(nDeviceByteBit)，表示在下发时要判断设备是否允许下发报文否，如果不存在该位，则可以直接下发当前报文
                    if (0 != struSendTelegram.nDeviceByteBit)
                    {
                        #region
                        CDeviceStatusDto smg = CCommonMethods.myMethods.GetDeviceStatus(warehouse, equipID);
                        #region
                        if (smg == null) 
                        {
                            //如果不存在设备号，则表示异常，先加入队列
                            CWException.WriteLog(string.Format("报文({0},{1})没有设备号,其卡号:{2}",
                                struSendTelegram.nTelegramType,struSendTelegram.nSubTypeEMS,struSendTelegram.strICCardID), 4);
                           //加入队列
                            InsertWorkQueue(objTelegram, struSendTelegram);
                            continue;
                        }
                        #endregion
                        if (smg.devicetype == (int)EnmSMGType.Hall)
                        {
                            #region 允许下发或加入队列
                            if (smg.devicemode == (int)EnmModel.Automatic &&
                               smg.isable == 1 && smg.isavailable == 1&&
                                smg.tasktype==(int)EnmTaskType.Init)
                            {
                                smg.tasktype = struSendTelegram.nMasterType;
                                smg.iccode = struSendTelegram.strICCardID;
                                smg.prevnode = null;
                                smg.currentnode = null;
                                smg.scrloc = struSendTelegram.strSrcLocAddr;
                                smg.desccloc = struSendTelegram.strDestLocAddr;

                                new CDBEditor().updateTableDto(smg);

                                //下发报文
                                DealSendTelegram(objTelegram, warehouse);
                            }
                            else
                            {
                                //加入队列
                                InsertWorkQueue(objTelegram, struSendTelegram);
                            }
                            #endregion
                        }
                        else if (smg.devicetype == (int)EnmSMGType.ETV)
                        {
                            #region 将(14,1)加入队列
                            if (struSendTelegram.nTelegramType == 14 && struSendTelegram.nSubTypeEMS == 1)
                            {
                                CWException.WriteLog(string.Format("warehouse:{0}, devicecode:{1} has telegram (14,1) add workqueue! ",
                                    smg.warehouse, smg.devicecode), 4);

                                InsertWorkQueue(objTelegram, struSendTelegram);
                                continue;
                            }
                            #endregion
                            #region 判断避让, 执行（13,1）（11,1）
                            if (smg.devicemode == (int)EnmModel.Automatic &&
                                smg.isable == 1 && smg.isavailable == 1&&
                                smg.tasktype == (int)EnmTaskType.Init)
                            {
                                string toAddrss = "";
                                if (struSendTelegram.nTelegramType == 11 && struSendTelegram.nSubTypeEMS == 1)
                                {
                                    toAddrss = struSendTelegram.strDestLocAddr;
                                }
                                else 
                                {
                                    toAddrss = struSendTelegram.strSrcLocAddr;
                                }

                                if (this.DealAvoid(smg, toAddrss, struSendTelegram.strICCardID)) 
                                {
                                    //允许下发,绑定设备，下发报文
                                    smg.tasktype = struSendTelegram.nMasterType;
                                    smg.iccode = struSendTelegram.strICCardID;
                                    smg.prevnode = null;
                                    smg.currentnode = null;
                                    if (struSendTelegram.nMasterType != (int)EnmTaskType.MoveEquipTask)
                                    {
                                        smg.currentnode = (int)EnmFlowNodeDescp.Load;
                                    }
                                    
                                    smg.scrloc = struSendTelegram.strSrcLocAddr;
                                    smg.desccloc = struSendTelegram.strDestLocAddr;
                                    new CDBEditor().updateTableDto(smg);
                                    //下发报文
                                    DealSendTelegram(objTelegram, warehouse);
                                    continue;
                                }
                            }

                            //加入队列
                            InsertWorkQueue(objTelegram, struSendTelegram);
                            #endregion
                        }
                        continue;
                        #endregion
                    }

                    // 发送报文
                    DealSendTelegram(objTelegram, warehouse);
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CTelegramSender.TelegramSender 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }     

        /// <summary>
        /// 发送报文并判断是否重发，如果重发次数不超过m_retryCount
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wareHouse"></param>
        private void DealSendTelegram_que(object data, int wareHouse)
        {
            if (null == m_PLCServer)
            {
                return;
            }
            int count = m_retryCount;// 重发次数
            bool bSendResult = false;
            bool bACKResult = false;
            while (0 < count--)
            {
                bSendResult = SendData(data, wareHouse);
                if (bSendResult)
                {
                    int nMillisecond = 0;
                    while (nMillisecond < 3000)
                    {
                        short recvBuffer = 0;
                        bool isOK = m_PLCServer.Read(ref recvBuffer, 1);
                        if (isOK)
                        {
                            if (recvBuffer == 9999)
                            {
                                Int16[] ackTelegram = null;
                                m_PLCServer.Read(ref ackTelegram, 0);// 读取PLC回应ACK报文

                                bACKResult = CBaseMethods.MyBase.IsPLCAck((Int16[])data, ackTelegram);
                                if (bACKResult)
                                {
                                    //更新PLC发送缓冲区标志位为0
                                    if (!m_PLCServer.Write(0, 1))
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
                                    string strTelegram = sb.ToString();
                                    CLOGException.Trace(wareHouse, "AbstractActionLib.CTelegramSender.DealSendTelegram", "ACK错误，报文：" + strTelegram);
                                }
                            }
                        }
                        Thread.Sleep(50);//4000
                        nMillisecond += 50;
                    }
                    if (bACKResult)
                    {
                        break;
                    }
                }
            }

            if (!bSendResult || !bACKResult)
            {
                CTelegramHandler telegramHandler = new CTelegramHandler(wareHouse, data);
            
                ThreadPool.QueueUserWorkItem(new WaitCallback(telegramHandler.Init));
            }
        }

        /// <summary>
        /// 发送报文并判断是否重发，如果重发次数不超过m_retryCount
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wareHouse"></param>
        private bool DealSendTelegram(object data, int wareHouse)
        {
            if (null == m_PLCServer)
            {
                CWException.WriteLog("报文发送时,m_PLCServer为空",4);
                return false;
            }
            if (data.GetType() == typeof(short[]))
            {
                short[] mydt = (short[])data;
                if (mydt[6] == 0 || mydt[2] == 0 || mydt[3] == 0)
                {
                    CWException.WriteLog(data, 1);
                    return true ;
                }
            }
            int count = m_retryCount;//重发次数
            bool bSendResult = false;
            bool bACKResult = false;
            while (0 < count--)
            {
                bSendResult = SendData(data, wareHouse);
                if (bSendResult)
                { 
                    m_PLCServer.AckAllowClear = false;
                    int nMillisecond = 0;
                    while (nMillisecond < 3000)
                    {
                        short recvBuffer = 0;
                        bool isOK = m_PLCServer.Read(ref recvBuffer, 1);
                        if (isOK)
                        {
                            if (recvBuffer == 9999)
                            {
                                Int16[] ackTelegram = null;
                                m_PLCServer.Read(ref ackTelegram, 0);// 读取PLC回应ACK报文

                                bACKResult = CBaseMethods.MyBase.IsPLCAck((Int16[])data, ackTelegram);
                                if (bACKResult)
                                {
                                    //更新PLC发送缓冲区标志位为0
                                    if (!m_PLCServer.Write(0, 1))
                                    {
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(ReWritePLCFlag));
                                    }

                                    //接收报文ack成功
                                    CCommonMethods.myMethods.InsertTelegramLog(ackTelegram, 0);
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(50);//4000
                        nMillisecond += 50;
                    }
                    if (bACKResult)
                    {
                        break;
                    }
                    //重发时更改MessageID
                    if (data.GetType() == typeof(short[]))
                    {
                        short[] mydt = (short[])data;
                        mydt[48] = (short)new CRequestAction().MessageID;
                    }
                }
            }
            m_PLCServer.AckAllowClear = true;

            if (!bSendResult) 
            {
                CWException.WriteLog("以下报文发送不成功,SendResult:"+bSendResult.ToString()+"  bAckResult:"+bACKResult.ToString(),4);
                CWException.WriteLog(data, 1);

                short[] msg = (short[])data;
                string strWaitTelegram = m_telegramParser.Int16sToString(msg, 0, msg.Length - 1, 4);
                //加入队列，将其作业类型改为RetrySend
                CWorkQueueDto workQueueDto = new CWorkQueueDto
                {
                    devicecode = msg[6],
                    warehouse = wareHouse,
                    iccode = msg[11].ToString().PadLeft(4, '0'),
                    waitsendtelegram = strWaitTelegram,
                    tasktype = (int)EnmTaskType.RetrySend
                };
                CWorkQueueDto exitQueue = CCommonMethods.myMethods.GetSendTelegramInfo(wareHouse, (int)workQueueDto.devicecode, strWaitTelegram);
                if (exitQueue == null)
                {
                    new CDBEditor().insertTableDto(workQueueDto);
                }

            }
            return bSendResult;
        }

        /// <summary>
        /// 分库发送报文
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wareHouse"></param>
        /// <returns></returns>
        private bool SendData(object data, int wareHouse)
        {
            if (null == m_PLCServer)
            {
                return false;
            }
            bool isok = false;
            if (m_PLCServer.IsConnected)
            {
                #region add 20160229 在发送报文前，先判断PLC发送缓冲区是否为空，如果是ACK，没有办法再读取了，则强制清0,以让PLC发送缓冲区处于空闲状态
                bool isACK = false;
                Int16 recvBuffer = 0;
                bool isExit = m_PLCServer.Read(ref recvBuffer, 1);// 读取PLC发送缓冲区标志位
                if (isExit) 
                {
                    if (recvBuffer == 9999) 
                    {
                        short[] recvTele=new short[50];
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
                bool wisgood = m_PLCServer.Read(ref wflag, 3);// 20160228-读取PLC接收缓冲区标志位

                if (wisgood)// 读取数据有效Good
                {
                    if ((Int16)wflag == 0)// 数据已经被PLC处理（PLC数据为空，此时可以填充数据）
                    {
                        wisgood = m_PLCServer.Write(data, 2);// 向OPC写数据                                    

                        if (wisgood)// 写入数据正常完成
                        {
                            wflag = m_nSendAckFlag;
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
                    CCommonMethods.myMethods.InsertTelegramLog(data, 1);
                }
            }
            else
            {
                m_PLCServer.ReConnectServer();
            }

            return isok;
        }

        /// <summary>
        /// 读故障文件具体位（车厅、ETV等设备）
        /// </summary>
        /// <param name="itemNum"></param>
        /// <param name="byteNum"></param>
        /// <param name="bitNum"></param>
        /// <returns></returns>
        private bool ReadBit(struTelegram struSendTelegram)
        {
            try
            {
                // 先通过设备表中的可接受指令
                CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struSendTelegram.nEMSSendFlag, struSendTelegram.nEquipID);
                if (null != deviceStatus && 1 == deviceStatus.isavailable)
                {
                    return true;
                }
                // 再通过故障表中的可接受指令
                CDeviceFaultDto deviceFault = CCommonMethods.myMethods.GetDeviceFault(struSendTelegram.nEMSSendFlag, struSendTelegram.nEquipID, struSendTelegram.nDeviceByteBit);
                if (null != deviceFault && 1 == deviceFault.isable)
                {
                    return true;
                }
                // 最后读取PLC中的可接受指令位值
                int itemNum = (int)((Hashtable)(CConfigManagement.myPara.HashDeviceIDItemAdressIndex[struSendTelegram.nEMSSendFlag]))[struSendTelegram.nEquipID];//.GetDeviceItemAdressIndex(struSendTelegram.nEMSSendFlag, struSendTelegram.nEquipID); ;// 订阅Item索引号
                if (-1 == itemNum || null == m_PLCServer)
                {
                    return false;
                }
                byte[] data = new byte[m_nByteLen];
                bool bFlag = m_PLCServer.Read(ref data, itemNum);
                int count = m_retryCount;// 重发次数
                while (!bFlag && 0 < count--)
                {
                    Thread.Sleep(50);//4000
                    bFlag = m_PLCServer.Read(ref data, itemNum);
                }
                if (bFlag)
                {
                    int byteNum = struSendTelegram.nDeviceByteBit / 10;
                    int bitNum = struSendTelegram.nDeviceByteBit % 10;
                    int value = Convert.ToInt32(data[byteNum]);
                    value = value >> bitNum;
                    value = value % 2;

                    if (value == 1)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CSendTelegramAction.ReadBit 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
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
                CLOGException.Trace("WorkFlowLib.CSendTelegramAction.ReWritePLCFlag 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
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
                    bFlag = m_PLCServer.Write(m_nSendAckFlag, 3);// 更新PLC接收缓冲区标志位为9999
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CSendTelegramAction.ReWriteEMSFlag 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 增加排队报文和插入排队的报文到数据库中
        /// </summary>
        /// <param name="objTelegram"></param>
        /// <param name="struSendTelegram"></param>
        private void InsertWorkQueue(Int16[] objTelegram, struTelegram struSendTelegram)
        {
            // 插入数据库对应工作队列数据
            string strWaitTelegram = m_telegramParser.Int16sToString(objTelegram, 0, objTelegram.Length - 1, 4);
            CWorkQueueDto workQueueDto = CCommonMethods.myMethods.GetSendTelegramInfo(struSendTelegram.nEMSSendFlag, struSendTelegram.nEquipID, strWaitTelegram);

            if (workQueueDto == null)
            {
                workQueueDto = new CWorkQueueDto
                {
                    devicecode = struSendTelegram.nEquipID,
                    warehouse = struSendTelegram.nEMSSendFlag,
                    iccode = struSendTelegram.strICCardID,
                    waitsendtelegram = strWaitTelegram,

                    tasktype=struSendTelegram.nMasterType
                };
                CDBEditor dbEditor = new CDBEditor();
                if (null != dbEditor)
                {
                    dbEditor.insertTableDto(workQueueDto);
                }
            }
        }
        
        /// <summary>
        /// DBW30  EMS设定车位个数(DBW30临时车位个数;DBW32固定车位个数)
		///add by suhan 20150729
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// </summary>
        /// <param name="enmHallType"></param>
        /// <returns></returns>
        public EnmFaultType EMSSetSpaceCarLocCount(int nWareHouse)
        {
            if (!CConfigManagement.myPara.HashHallDeviceID.Contains(nWareHouse))
            {
                return EnmFaultType.InvalidWareHouseID;//无效库区号
            }
            foreach (int nHallID in (List<int>)CConfigManagement.myPara.HashHallDeviceID[nWareHouse])
            {

                //m_PLCServer = CCommonMethods.myMethods.GetCommModule();
                //if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(nWareHouse))
                //{
                //    return EnmFaultType.InvalidWareHouseID;//无效库区号
                //}

                //if (!CConfigManagement.myPara.HashDeviceIDItemAdressIndex.ContainsKey(nWareHouse)
                //    || !((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse]).ContainsKey(nHallID))
                //{
                //    return EnmFaultType.InvalidWareHouseID;//无效库区号
                //}
                //List<string> lstPLCItemAdress = (List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[nWareHouse];
                //m_PLCServer.Initialize(lstPLCItemAdress, false);
                int itemNum = (int)((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse])[nHallID];// 订阅Item索引号
                int nTempCount = CCommonMethods.myMethods.GetTempSpaceCount(nWareHouse);
                int nFixedLocationCount = CCommonMethods.myMethods.GetFixedLocationSpaceCount(nWareHouse);
                if (0 == nTempCount)
                {
                    nTempCount = 999;
                }
                if (0 == nFixedLocationCount)
                {
                    nFixedLocationCount = 999;
                }
                if (!m_PLCServer.Write(31, nTempCount, nFixedLocationCount, itemNum))
                {
                    return EnmFaultType.FailToSendTelegram; //发送报文失败
                }
            }
            //m_PLCServer.Fini();
            return EnmFaultType.Success;
        }
        
        /// <summary>
        /// DBW30  EMS设定车位个数(DBW30空闲车位个数)
		///add by suhan 20150729
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// </summary>
        /// <param name="enmHallType"></param>
        /// <returns></returns>
        public EnmFaultType EMSSetFreeCarLocCount(int nWareHouse)
        {
            if (!CConfigManagement.myPara.HashHallDeviceID.Contains(nWareHouse))
            {
                return EnmFaultType.InvalidWareHouseID;//无效库区号
            }
            foreach (int nHallID in (List<int>)CConfigManagement.myPara.HashHallDeviceID[nWareHouse])
            {
                //m_PLCServer = CCommonMethods.myMethods.GetCommModule();
                //if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(nWareHouse))
                //{
                //    return EnmFaultType.InvalidWareHouseID;//无效库区号
                //}

                if (!CConfigManagement.myPara.HashDeviceIDItemAdressIndex.ContainsKey(nWareHouse)
                    || !((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse]).ContainsKey(nHallID))
                {
                    return EnmFaultType.InvalidWareHouseID;//无效库区号
                }
                //List<string> lstPLCItemAdress = (List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[nWareHouse];
                //m_PLCServer.Initialize(lstPLCItemAdress, false);
                int itemNum = (int)((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse])[nHallID];// 订阅Item索引号
                int nFreeCount = CCommonMethods.myMethods.GetFreeCarPOSN(nWareHouse);
                if (0 == nFreeCount)
                {
                    nFreeCount = 999;
                }
                if (!m_PLCServer.Write(31, nFreeCount, itemNum))
                {
                    return EnmFaultType.FailToSendTelegram; //发送报文失败
                }
                //m_PLCServer.Fini();
            }

            return EnmFaultType.Success;
        }

        /// <summary>
        /// 装载、卸载、移动下发时，避让处理
        /// </summary>
        /// <param name="smg"></param>
        /// <param name="toAddrs"></param>
        /// <returns></returns>
        public bool DealAvoid(CDeviceStatusDto smg, string toAddrs,string iccode) 
        {
            List<CDeviceStatusDto> Etvs = CCommonMethods.myMethods.GetEtvsByLayer((int)smg.warehouse, (int)smg.devicelayer);
            int nWarehouse = smg.warehouse;
            int curEtvCol = CBaseMethods.MyBase.GetColumnByAddrs(smg.deviceaddr);
            int curToCol = CBaseMethods.MyBase.GetColumnByAddrs(toAddrs);   //终点列          

            int curMax;
            int curMin;
            #region
            if (curEtvCol > curToCol)
            {
                curMax = curEtvCol;
                curMin = curToCol - 3;
                if (curMin < 1)
                {
                    curMin = 1;
                }
            }
            else
            {
                curMax = curToCol + 3;
                if (curMax > 40)
                {
                    curMax = 40;
                }
                curMin = curEtvCol;
            }
            #endregion
            //对面ETV
            CDeviceStatusDto otherEtv = null;
            #region
            foreach (CDeviceStatusDto et in Etvs)
            {
                if (et.devicecode != smg.devicecode)
                {
                    otherEtv = et;
                    break;
                }
            }
            #endregion
            if (otherEtv == null) 
            {
                return false;
            }
            int otherCol = CBaseMethods.MyBase.GetColumnByAddrs(otherEtv.deviceaddr);

            #region 列数与实际不合的，先不让其去执行
            bool isTrue = true;
            if (smg.devicecode < otherEtv.devicecode)
            {
                if (curEtvCol < otherCol)
                {
                    isTrue = false;
                }
            }
            else 
            {
                if (curEtvCol > otherCol) 
                {
                    isTrue = false;
                }
            }
            if (!isTrue) 
            {
                string msg = String.Format("异常： ETV{0} 当前列{1},ETV{2} 当前列{3}", smg.devicecode, curEtvCol, otherEtv.devicecode, otherCol);
                CWException.WriteLog(msg,4);
                return false;
            }

            #endregion

            if (otherEtv.tasktype == (int)EnmTaskType.Init)
            {
                #region
                if (curMin < otherCol && otherCol <curMax)
                {
                    if (otherEtv.isable == 0)
                    {
                        return false;
                    }
                    #region 生成移动并下发
                    string oLayer = Convert.ToString(otherEtv.devicelayer).PadLeft(2, '0');
                    string oList = "";
                    if (curEtvCol > curToCol)     //需向左避让
                    {
                        oList = curMin.ToString().PadLeft(2, '0');
                    }
                    else
                    {
                        oList = curMax.ToString().PadLeft(2, '0');
                    }
                    string toAddress = string.Concat("1", oList, oLayer);
                   
                    //建立移动作业
                    struTelegram moveTask = new struTelegram();
                    moveTask.nEMSSendFlag = nWarehouse;
                    moveTask.nPLCSendFlag = 0;
                    moveTask.nTelegramType = 11;
                    moveTask.nSubTypeEMS = 1;
                    moveTask.nSubTypePLC = 0;
                    moveTask.nEquipID = otherEtv.devicecode;
                    if (!CBaseMethods.MyBase.IsEmpty(iccode))
                    {
                        moveTask.strICCardID = iccode;
                    }
                    moveTask.strSrcLocAddr = otherEtv.deviceaddr;
                    moveTask.strDestLocAddr = toAddress;
                    moveTask.nTelegramID = new CRequestAction().MessageID;
                    moveTask.nEndFlag = 9999;
                    moveTask.nMasterType = (int)EnmTaskType.AvoidMove;
                    //打包并发送报文
                    Int16[] movTelegram;
                    m_telegramParser.PackTelegram(out movTelegram, moveTask);

                    if (otherEtv.isavailable == 1)
                    {
                        otherEtv.tasktype = (int)EnmTaskType.AvoidMove;
                        otherEtv.iccode = iccode;
                        otherEtv.prevnode = null;
                        otherEtv.currentnode = null;
                        otherEtv.scrloc = otherEtv.deviceaddr;
                        otherEtv.desccloc = toAddress;
                        //更新数据
                        new CDBEditor().updateTableDto(otherEtv);

                        DealSendTelegram(movTelegram, nWarehouse);
                    }
                    else 
                    {
                        //加入队列
                        InsertWorkQueue(movTelegram, moveTask);
                    }

                    #endregion
                }
                return true;
                #endregion
            }
            else //对面的TV在作业，
            {
                #region 对面的TV有作业，则如果有交叉的，当前先不下发，让其先执行完其作业
                string oToAddrs = "";
                #region
                if (otherEtv.currentnode==(int)EnmFlowNodeDescp.Load)
                {
                    oToAddrs = otherEtv.scrloc;
                }
                else
                {
                    oToAddrs = otherEtv.desccloc;
                }
                if (CBaseMethods.MyBase.IsEmpty(oToAddrs))
                {
                    CWException.WriteLog(string.Format("另一TV{0},作业类型：{1},卡号：{2},判断避让时其源/目的地址为空",
                        otherEtv.devicecode, otherEtv.tasktype, otherEtv.iccode == null ? "" : otherEtv.iccode), 4);
                    return true;
                }
                #endregion
                int toColumn = CBaseMethods.MyBase.GetColumnByAddrs(oToAddrs);

                #region 处于等待卸载时，如果路径出现冲突，也可进行避让作业
                if (smg.tasktype != (int)EnmTaskType.Init) //要下发（14，1）时处理
                {
                    if (smg.currentnode == (int)EnmFlowNodeDescp.WillUnLoad&&
                        otherEtv.currentnode == (int)EnmFlowNodeDescp.WillUnLoad) 
                    {
                        //优先1#下发
                        if (smg.region == 2) 
                        {
                            return false;
                        }
                    }

                    if (otherEtv.currentnode == (int)EnmFlowNodeDescp.WillUnLoad)
                    {
                        if (curMin < otherCol && otherCol < curMax)
                        {
                            if (otherEtv.isavailable == 0)
                            {
                                return false;
                            }                          
                            #region 生成移动并下发,直接下发
                            string oLayer = Convert.ToString(otherEtv.devicelayer).PadLeft(2, '0');
                            string oList = "";
                            if (curEtvCol > curToCol)     //需向左避让
                            {
                                oList = curMin.ToString().PadLeft(2, '0');
                            }
                            else
                            {
                                oList = curMax.ToString().PadLeft(2, '0');
                            }
                            string toAddress = string.Concat("1", oList, oLayer);

                            //建立移动作业
                            struTelegram moveTask = new struTelegram();
                            moveTask.nEMSSendFlag = nWarehouse;
                            moveTask.nPLCSendFlag = 0;
                            moveTask.nTelegramType = 11;
                            moveTask.nSubTypeEMS = 1;
                            moveTask.nSubTypePLC = 0;
                            //给假值
                            moveTask.strCarSize = "123";
                            moveTask.nWheelbase = 2600;
                            moveTask.overallLg = 4500;
                            moveTask.overHang = 900;

                            moveTask.nEquipID = otherEtv.devicecode;
                            if (!CBaseMethods.MyBase.IsEmpty(iccode))
                            {
                                moveTask.strICCardID = iccode;
                            }
                            moveTask.strSrcLocAddr = otherEtv.deviceaddr;
                            moveTask.strDestLocAddr = toAddress;
                            moveTask.nTelegramID = new CRequestAction().MessageID;
                            moveTask.nEndFlag = 9999;
                            moveTask.nMasterType = (int)EnmTaskType.ForceMove;
                            //打包并发送报文
                            Int16[] movTelegram;
                            m_telegramParser.PackTelegram(out movTelegram, moveTask);
                            //下发移动避让作业
                            DealSendTelegram(movTelegram, nWarehouse);
                            //更改其TV的可用性
                            otherEtv.isavailable = 0;
                            new CDBEditor().updateTableDto(otherEtv);
                            #endregion                           
                        }
                        return true;
                    }
                }
                #endregion

                #region 如果当前TV还没有进行动作，则这里就限制其下发
                if (otherCol >= curMin && otherCol <= curMax)
                {
                    return false;
                }
                if (toColumn >= curMin && toColumn <= curMax)
                {
                    return false;
                }
                if (otherCol > curMax)
                {
                    if (toColumn < curMax)
                    {
                        return false;
                    }
                }
                if (toColumn > curMax)
                {
                    if (toColumn < curMax)
                    {
                        return false;
                    }
                }
                if (otherCol < curMin)
                {
                    if (toColumn > curMin)
                    {
                        return false;
                    }
                }
                if (toColumn < curMin)
                {
                    if (otherCol > curMin)
                    {
                        return false;
                    }
                }
                #endregion

                #endregion
            }
            return true;
        }     
    }
}
