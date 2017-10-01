using System;
using System.Collections.Generic;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommonMethodLib;
using AbstractActionLib;
using System.Reflection;

namespace WorkFlowLib
{
    public class CTaskTriggerMgmt
    {
        private CTaskTrigger m_objcTaskTrigger;
        private int m_nPLCID;
        private static int m_nCounter03wav;//车辆未停放到位，请开到位03.wav
        private static int m_nCounter04wav;// 车体超限，请退出04.wav
        private static int m_nCounter22wav;// 请向左22.wav
        private static int m_nCounter23wav;// 请向右23.wav
        private static int m_nCounter41wav;// 只剩VIP车位41.wav

        public CTaskTriggerMgmt()
        {
            m_nCounter03wav = 0;
            m_nCounter04wav = 0;
            m_nCounter22wav = 0;
            m_nCounter23wav = 0;
            m_nCounter41wav = 0;
        }
        public void SetTaskTrigger(int nPLCID)
        {
            if (!CConfigManagement.myPara.HashGarageType.ContainsKey(nPLCID))
            {
                return;
            }

            string strClassName = string.Format("C{0}TaskTrigger", CConfigManagement.myPara.HashGarageType[nPLCID]);
            object[] args = new object[1];
            args[0] = nPLCID;
            m_nPLCID = nPLCID;
           
            CTaskTrigger cTaskTrigger = (CTaskTrigger)Assembly.Load("WorkFlowLib").CreateInstance("WorkFlowLib." + strClassName, false, BindingFlags.Default, null, args, null, null);
            if (null == cTaskTrigger)
            {
                CLOGException.Trace(m_nPLCID, "TaskTriggerMgmt", "Instance class(cTaskTrigger) failed");
            }
            this.m_objcTaskTrigger = cTaskTrigger;
        }
        
        /// <summary>
        /// 动作命令和状态命令 报文处理
        /// </summary>
        /// <param name="nActionFlag">返回执行动作的标志</param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public bool Telegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            if (0 == struTelegram.nPLCSendFlag)
            {
                // 非PLC发送的报文时，退出
                CLOGException.Trace("CTaskTriggerMgmt.Telegram 异常", "无效报文，库区号为0");
                return false;
            }
            // 报文类型
            switch (struTelegram.nTelegramType)
            {
                // 入库协议
                case 1001:
                    {
                        return m_objcTaskTrigger.DealEnterTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // 临时取物协议
                case 1002:
                    {
                        return m_objcTaskTrigger.DealTmpFetchTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // 出库协议
                case 1003:
                    {
                        return m_objcTaskTrigger.DealExitTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // 移动ETV或TV
                case 1011:
                    {
                        #region
                        if (1 == struTelegram.nSubTypePLC)
                        {
                            nActionFlag = 1;                         
                            requestAction.telegram = "11-51";

                            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);
                            if (deviceStatus == null) 
                            {
                                return true;
                            }
                            // ETV设备
                            if (deviceStatus.tasktype==(int)EnmTaskType.AvoidMove||
                                deviceStatus.tasktype==(int)EnmTaskType.MoveEquipTask)
                            {
                                deviceStatus.iccode = null;
                                deviceStatus.tasktype = (int)EnmTaskType.Init;
                                // 作业流程节点类型清空
                                deviceStatus.prevnode = null;
                                deviceStatus.currentnode = null;                              
                                deviceStatus.scrloc = null;
                                deviceStatus.desccloc = null;
                                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                            }
                        }
                        #endregion
                        break;                       
                    }
                // 车辆旋转
                case 1012:
                    {
                        return m_objcTaskTrigger.DealVehicleRotationTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // ETV装载
                case 1013:
                    {
                        return m_objcTaskTrigger.DealLoadTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // ETV卸载
                case 1014:
                    {
                        return m_objcTaskTrigger.DealUnloadTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                // 状态命令
                case 1074:
                    {
                        return m_objcTaskTrigger.DealStatusCommandTelegram(ref nActionFlag, ref requestAction, struTelegram);
                    }
                default:
                    {
                        break;
                    }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return false;
        }

        /// <summary>
        /// 设备故障文件和状态内容 报文处理
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="nPLCID"></param>
        /// <param name="nEquipID"></param>
        /// <param name="objTelegram"></param>
        public bool DeviceTelegram(ref int nActionFlag, ref CRequestAction requestAction, int nPLCID, int nEquipID, object objTelegram)
        {
            int nStatusID = CConfigManagement.myPara.EquipStatusLen;// DBW30 设备状态开始位
            int nModeID = nStatusID + 21;// DBW50 设备当前运行模式
            int nStepIn = nStatusID + 9;// DBW38 设备装载自动步
            int nStepOut = nStatusID + 11;// DBW40 设备卸载自动步
            int nColorAddr = m_objcTaskTrigger.nColorAddrETVorTV;// 140;// 设备故障红色与绿色界限（小于nColorAddr为红色，否则绿色）
            bool isHall = false;
            bool isGetWorkQueueICCard = false;
            // 获取库号m_nPLCID和设备号nEquipID的设备状态具体实例
            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(nPLCID, nEquipID);

            if (null == deviceStatus)
            {
                return isGetWorkQueueICCard;
            }

            if (deviceStatus.devicetype == (int)EnmSMGType.Hall)
            {// 判断是否为车厅设备
                nModeID = nStatusID + 9;// DBW38 车厅当前的工作方式
                nStepIn = nStatusID + 11;// DBW40 车厅存车自动步
                nStepOut = nStatusID + 13;// DBW42 车厅取车自动步
                nColorAddr = m_objcTaskTrigger.nColorAddrHall;// 车厅故障红色与绿色界限（小于nColorAddr为红色，否则绿色）
                isHall = true;
            }
            // 获取库号m_nPLCID和设备号nEquipID的设备故障列表
            List<CDeviceFaultDto> lstDeviceFault = CCommonMethods.myMethods.GetDeviceFaultList(nPLCID, nEquipID);
            byte[] bytesTelegram = (byte[])objTelegram;
            bool isDeviceUpDate = false;// 是否修改设备状态

            // 设备故障文件          
            for (int nCounter = lstDeviceFault.Count - 1; nCounter >= 0; nCounter--)
            {
                CDeviceFaultDto deviceFault = lstDeviceFault[nCounter];
                int faultAddr = Convert.ToInt32(deviceFault.faultaddress);
                int byteNum = faultAddr / 10;// 字节
                int bitNum = faultAddr % 10;// 位

                if (byteNum >= nStatusID || byteNum >= bytesTelegram.Length)
                {
                    continue;
                }

                int value = bytesTelegram[byteNum];
                value = value >> bitNum;
                value = value % 2;

                #region 可接收新指令
                if (deviceFault.faultaddress == CConfigManagement.myPara.EquipIsAviliableID) 
                {
                    if (deviceStatus.isavailable != value) 
                    {
                        deviceStatus.isavailable = value;
                        isDeviceUpDate = true;
                    }
                }
                #endregion

                #region 设备故障报警信息处理(车辆跑位+车体超限)
                if (isHall && CConfigManagement.myPara.CarDislocationID.ContainsKey(nPLCID) && deviceFault.faultaddress == CConfigManagement.myPara.CarDislocationID[nPLCID] && 1 == value)
                {// EMS收到“车辆跑位”报警信息后，将进程状态复位至前一状态，并语音提示“车辆未停放到位，请开到位”
                    // 清空当前卡号存取车排队信息
                    m_objcTaskTrigger.ClearWorkQueueSwipe(ref requestAction, deviceStatus.iccode);

                    deviceStatus.iccode = null;
                    deviceStatus.tasktype = (int)EnmTaskType.Init;
                    // 车厅作业流程节点类型清空
                    deviceStatus.prevnode = null;
                    deviceStatus.currentnode = null;
                    deviceStatus.queueprevnode = null;
                    deviceStatus.queuecurrentnode = null;                   
                    if (deviceFault.isable != value || 0 == m_nCounter03wav)
                    {
                        m_nCounter03wav++;
                        requestAction.soundName = "03.wav";// 车辆未停放到位，请开到位03.wav
                        //CLOGException.Trace(nPLCID, "DeviceTelegram", "车辆未停放到位，请开到位03.wav");// trace
                    }
                    else
                    {
                        m_nCounter03wav++;
                        if (5 <= m_nCounter03wav)
                        {
                            m_nCounter03wav = 0;
                        }
                    }
                }

                if (isHall && CBaseMethods.MyBase.IsEmpty(deviceStatus.tasktype) && CConfigManagement.myPara.CarOverLimitID.ContainsKey(nPLCID)
                    && deviceFault.faultaddress == CConfigManagement.myPara.CarOverLimitID[nPLCID] && 1 == value)
                {// EMS收到“车体超限”报警信息后，将进程状态复位至前一状态，并语音提示“车体超限，请退出车厅”
                    if (deviceFault.isable != value || 0 == m_nCounter04wav)
                    {
                        m_nCounter04wav++;
                        requestAction.soundName = "04.wav";// 车体超限，请退出04.wav
                        //CLOGException.Trace(nPLCID, "DeviceTelegram", "车体超限，请退出04.wav");// trace
                    }
                    else
                    {
                        m_nCounter04wav++;
                        if (5 <= m_nCounter04wav)
                        {
                            m_nCounter04wav = 0;
                        }
                    }
                }

                // 读取配置文件
                if ("VerticalLifting" == (string)CConfigManagement.myPara.HashGarageType[nPLCID] && isHall && 1 == value && deviceFault.faultaddress == 294)
                {// 塔库---EMS收到“请向左”报警信息后
                    if (deviceFault.isable != value || 0 == m_nCounter22wav)
                    {
                        m_nCounter22wav++;
                        requestAction.soundName = "22.wav";// 请向左22.wav
                        //CLOGException.Trace(nPLCID, "DeviceTelegram", deviceFault.faultdescp + requestAction.soundName);// trace
                    }
                    else
                    {
                        m_nCounter22wav++;
                        if (5 <= m_nCounter22wav)
                        {
                            m_nCounter22wav = 0;
                        }
                    }
                }

                // 读取配置文件
                if ("VerticalLifting" == (string)CConfigManagement.myPara.HashGarageType[nPLCID] && isHall && 1 == value && deviceFault.faultaddress == 295)
                {// 塔库---EMS收到“请向右”报警信息后
                    if (deviceFault.isable != value || 0 == m_nCounter23wav)
                    {
                        m_nCounter23wav++;
                        requestAction.soundName = "23.wav";// 请向右23.wav
                        //CLOGException.Trace(nPLCID, "DeviceTelegram", deviceFault.faultdescp + requestAction.soundName);// trace
                    }
                    else
                    {
                        m_nCounter23wav++;
                        if (5 <= m_nCounter23wav)
                        {
                            m_nCounter23wav = 0;
                        }
                    }
                }
                #endregion

                #region 设备故障表更新
                if (deviceFault.isable != value)
                {
                    deviceFault.isable = value;

                    if (1 != value)
                    {
                        deviceFault.color = value;
                    }
                    else if (deviceFault.faultaddress < nColorAddr)
                    {
                        deviceFault.color = 1;// 红色
                        CDeviceFaultLogDto logDto = new CDeviceFaultLogDto
                        {
                            warehouse = deviceFault.warehouse,
                            devicecode = deviceFault.devicecode,
                            color = deviceFault.color,
                            time = DateTime.Now,
                            faultdescp = deviceFault.faultdescp
                        };

                        requestAction.lstTableDto.Add(new struTableDto(logDto, 1));
                    }
                    else
                    {
                        deviceFault.color = 3;// 绿色
                    }

                    //lstDeviceFaultDto.Add(deviceFault);
                    requestAction.lstTableDto.Add(new struTableDto(deviceFault, 2));
                }
                #endregion
            }

            #region 设备状态内容更新
            // 设备状态内容
            if (nModeID < bytesTelegram.Length && deviceStatus.devicemode != bytesTelegram[nModeID])
            {// DBW38 车厅当前的工作方式; DBW50 设备当前运行模式
                deviceStatus.devicemode = bytesTelegram[nModeID];
                isDeviceUpDate = true;

                // 设备模式由全自动切换为非自动时
                if (deviceStatus.devicemode != 0)
                {
                    if ((int)EnmModel.Automatic != deviceStatus.devicemode)
                    {
                        deviceStatus.isable = 0;
                        if (deviceStatus.devicetype == (int)EnmSMGType.ETV) 
                        {
                            if (deviceStatus.tasktype == (int)EnmTaskType.EntryTask ||
                                deviceStatus.tasktype == (int)EnmTaskType.ExitTask ||
                                deviceStatus.tasktype == (int)EnmTaskType.TmpFetch)
                            {
                                deviceStatus.prevnode = deviceStatus.currentnode;
                                deviceStatus.currentnode = (int)EnmFlowNodeDescp.TMURO;
                            }
                        }
                    }
                    else if ((int)EnmSMGType.Hall == deviceStatus.devicetype)
                    {
                        // 车厅设备可用与否根据设备模式决定
                        deviceStatus.isable = 1;
                    }
                }

                // 设备模式切换为非自动，且设备上的IC卡号不为空
                if ((int)EnmModel.Automatic != deviceStatus.devicemode && !string.IsNullOrWhiteSpace(deviceStatus.iccode))
                {
                    switch (deviceStatus.tasktype)
                    {
                        case (int)EnmTaskType.EntryTask:
                        case (int)EnmTaskType.ExitTask:
                        case (int)EnmTaskType.TmpFetch:
                            {// 无分配车位之后的车厅故障处理
                                // 获取车位信息具体实例(根据IC卡号码)
                                CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(deviceStatus.iccode);
                                if (null == carLocation)
                                {
                                    // 清空当前卡号存取车排队信息
                                    m_objcTaskTrigger.ClearWorkQueueSwipe(ref requestAction, deviceStatus.iccode);
                                    // 设备IC卡所有信息都初始化
                                    CCommonMethods.myMethods.InitDeviceStatus(deviceStatus.iccode);
                                }
                                break;
                            }
                        case (int)EnmTaskType.MoveEquipTask:
                            {// ETV移动的故障处理
                                deviceStatus.iccode = null;
                                deviceStatus.tasktype = (int)EnmTaskType.Init;
                                // 车厅作业流程节点类型清空
                                deviceStatus.prevnode = null;
                                deviceStatus.currentnode = null;
                                deviceStatus.queueprevnode = null;
                                deviceStatus.queuecurrentnode = null;                               
                                break;
                            }
                    }
                }
            }

            if (!isHall && nStatusID + 7 < bytesTelegram.Length)
            {// DBW36 ETV运行自动步进;
                int nRunStep = CommunicationLib.Types.Int.FromBytes(bytesTelegram[nStatusID + 7], bytesTelegram[nStatusID + 6]);
                if (nRunStep != deviceStatus.runstep)
                {
                    deviceStatus.runstep = nRunStep;
                    isDeviceUpDate = true;
                }
            }
            int hallLayer = nStatusID + 15;
            if (isHall && hallLayer < bytesTelegram.Length)
            {   
                // DBW45
                int nDeviceLayer = CommunicationLib.Types.Int.FromBytes(bytesTelegram[hallLayer], bytesTelegram[hallLayer-1]);
                if (nDeviceLayer != deviceStatus.devicelayer)
                {
                    deviceStatus.devicelayer = nDeviceLayer;
                    isDeviceUpDate = true;
                }
            }
           
            if (nStepIn < bytesTelegram.Length)
            {// DBW38 设备装载自动步;DBW40 车厅存车自动步
                int nInStep = CommunicationLib.Types.Int.FromBytes(bytesTelegram[nStepIn], bytesTelegram[nStepIn - 1]);
                if (nInStep != deviceStatus.instep)
                {
                    deviceStatus.instep = nInStep;
                    isDeviceUpDate = true;
                }

                if (isHall && CConfigManagement.myPara.DicVIPSoundFlag.ContainsKey(nPLCID) 
                    && 0 < nInStep && nInStep <  CConfigManagement.myPara.DicVIPSoundFlag[nPLCID]
                    && 0 >= CCommonMethods.myMethods.GetTempSpaceCount(nPLCID))
                {
                    // 车厅存车自动步进范围在是否VIP语音提示内且临时车位为0时，播放“只剩VIP车位”语音
                    if (0 == m_nCounter41wav)
                    {
                        m_nCounter41wav++;
                        requestAction.soundName = "41.wav";// 只剩VIP车位41.wav
                        //CLOGException.Trace(nPLCID, "DeviceTelegram", deviceFault.faultdescp + requestAction.soundName);// trace
                    }
                    else
                    {
                        m_nCounter41wav++;
                        if (3 <= m_nCounter41wav)
                        {
                            m_nCounter41wav = 0;
                        }
                    }
                }
            }

            if (nStepOut < bytesTelegram.Length)
            {// DBW40 设备卸载自动步; DBW42 车厅取车自动步
                int nOutStep = CommunicationLib.Types.Int.FromBytes(bytesTelegram[nStepOut], bytesTelegram[nStepOut - 1]);
                if (nOutStep != deviceStatus.outstep)
                {
                    deviceStatus.outstep = nOutStep;
                    isDeviceUpDate = true;
                }
            }
            string strDeviceaddr = string.Empty;
            if (!isHall && nStatusID + 1 < bytesTelegram.Length)
            {// DBW30 ETV所在的当前边
                int nSide = CommunicationLib.Types.Int.FromBytes(bytesTelegram[nStatusID + 1], bytesTelegram[nStatusID]);
                strDeviceaddr = nSide.ToString();//.PadLeft(2, '0');
            }
            if (!isHall && nStatusID + 3 < bytesTelegram.Length)
            {// DBW32 ETV所在的当前列
                int nColumn = CommunicationLib.Types.Int.FromBytes(bytesTelegram[nStatusID + 3], bytesTelegram[nStatusID + 2]);
                if (nColumn != 0)
                {
                    strDeviceaddr += nColumn.ToString().PadLeft(2, '0') + (deviceStatus.devicelayer).ToString().PadLeft(2, '0');
                }               
            }
          
            if (!string.IsNullOrWhiteSpace(strDeviceaddr) && strDeviceaddr.Length == 5)
            {// 会影响根据地址查找车厅设备（如果ETV移动到某车厅地址时）
                deviceStatus.deviceaddr = strDeviceaddr;
                isDeviceUpDate = true;
            }
            #endregion

            if (isDeviceUpDate)
            {
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
            }

            if (!string.IsNullOrWhiteSpace(requestAction.soundName) && 2 != (nActionFlag & 2))
            {
                nActionFlag += 2;
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }

            return isGetWorkQueueICCard;
        }      
     
        /// <summary>
        /// 处理队列中信息
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="warehouse"></param>
        /// <param name="nEquipID"></param>
        public void DealTeleQueueInfo(ref int nActionFlag, ref CRequestAction requestAction, int warehouse)
        {
            nActionFlag = 0;
            List<CWorkQueueDto> workQueueLst = CCommonMethods.myMethods.GetAllTeleQueue(warehouse);
            if (workQueueLst.Count == 0)
            {
                return;
            }
            //先处理有报文的队列
            List<CWorkQueueDto> lstWaitTelegram = workQueueLst.FindAll(ls => ls.waitsendtelegram != null);

            //发送原来发送不出去的报文
            CWorkQueueDto retrySendTelegram = lstWaitTelegram.Find(wt => wt.tasktype == (int)EnmTaskType.RetrySend);
            if (retrySendTelegram != null) 
            {
                CDeviceStatusDto smg = CCommonMethods.myMethods.GetDeviceStatus((int)retrySendTelegram.warehouse, (int)retrySendTelegram.devicecode);
                if (smg != null)
                {
                    m_objcTaskTrigger.SendQueueHasTelegram(ref nActionFlag, ref requestAction, smg, retrySendTelegram, false);
                }
                return;
            }

            //优先发送队列中的避让报文
            CWorkQueueDto priorCommit = lstWaitTelegram.Find(wt => wt.tasktype == (int)EnmTaskType.AvoidMove);
            if (priorCommit != null)
            {
                CDeviceStatusDto Tv = CCommonMethods.myMethods.GetDeviceStatus((int)priorCommit.warehouse, (int)priorCommit.devicecode);
                if (Tv != null)
                {
                    if (Tv.isable == 1 && Tv.isavailable == 1)
                    {
                        m_objcTaskTrigger.SendQueueHasTelegram(ref nActionFlag, ref requestAction, Tv, priorCommit, false);
                        return;
                    }
                }
            }
            //再处理其他报文队列
            foreach (CWorkQueueDto queue in lstWaitTelegram)
            {
                CWException.WriteLog(string.Format("queue has telegram,devicecode:{0},iccode:{1},tasktype:{2}.", queue.devicecode, queue.iccode, queue.tasktype), 4);
                #region
                CDeviceStatusDto eqp = CCommonMethods.myMethods.GetDeviceStatus((int)queue.warehouse, (int)queue.devicecode);
                if (eqp != null)
                {
                    if (eqp.isable == 1 && eqp.isavailable == 1)
                    {
                        if (eqp.devicetype == (int)EnmSMGType.Hall)
                        {
                            if (eqp.tasktype == (int)EnmTaskType.Init)
                            {
                                m_objcTaskTrigger.SendQueueHasTelegram(ref nActionFlag, ref requestAction, eqp, queue, false);
                                return;
                            }
                        }
                        else if (eqp.devicetype == (int)EnmSMGType.ETV)
                        {
                            if (eqp.tasktype == (int)EnmTaskType.Init)
                            {
                                m_objcTaskTrigger.SendQueueHasTelegram(ref nActionFlag, ref requestAction, eqp, queue, false);
                                if (nActionFlag != 0)
                                {
                                    return;
                                }
                            }
                            else //处理（14，1）
                            {
                                if (eqp.iccode == queue.iccode)
                                {
                                    m_objcTaskTrigger.SendQueueHasTelegram(ref nActionFlag, ref requestAction, eqp, queue, true);
                                    if (nActionFlag != 0)
                                    {
                                        return;
                                    }
                                }                               
                            }
                        }
                    }
                    //不可用时记录
                    CWException.WriteLog(string.Format("queue of Telegram No send,iccode:{0},because devicecode:{1}( isable:{2},isavailable:{3} ) is not ok!", queue.iccode, eqp.devicecode, eqp.isable, eqp.isavailable), 4);

                }
                #endregion
            }

            //处理处于排队的队列
            List<CWorkQueueDto> getCarQueueList = workQueueLst.FindAll(ls => ls.waitsendtelegram == null &&
                                                  (ls.tasktype == (int)EnmTaskType.ExitTask || ls.tasktype == (int)EnmTaskType.TmpFetch));

            foreach (CWorkQueueDto queue in getCarQueueList)
            {
                CWException.WriteLog(string.Format("workqueue wait to commit,devicecode:{0},iccode:{1},tasktype:{2}.", queue.devicecode, queue.iccode, queue.tasktype), 4);
                #region
                CDeviceStatusDto hall = CCommonMethods.myMethods.GetDeviceStatus((int)queue.warehouse, (int)queue.devicecode);
                if (hall != null)
                {
                    CICCardDto iccd = CCommonMethods.myMethods.GetICCard(queue.iccode);
                    if (iccd == null)
                    {
                        continue;
                    }
                    CCarLocationDto lctn = CCommonMethods.myMethods.GetCarLocation(iccd.iccode);
                    if (lctn == null)
                    {
                        continue;
                    }
                    if (hall.tasktype == (int)EnmTaskType.Init)
                    {
                        if (hall.isable == 1 && hall.isavailable == 1)
                        {
                            //发送车厅报文，组装TV报文,同时删除队列
                            m_objcTaskTrigger.SendHallTelegramAndBuildTvTele(ref nActionFlag, ref requestAction, hall, queue, lctn);
                            if (nActionFlag != 0)
                            {
                                return;
                            }
                        }
                        //不可用时记录
                        //CWException.WriteLog(string.Format("workqueue will send,but hall info is not ok. iccode:{0},devicecode:{1} isable:{2},isavailable:{3}",
                        //                     queue.iccode, hall.devicecode, hall.isable, hall.isavailable), 4);
                    }
                    else //车厅有作业的，则判断是否要提前下发
                    {
                        if (hall.tasktype == (int)EnmTaskType.ExitTask)
                        {
                            if (hall.currentnode == (int)EnmFlowNodeDescp.ExitHasCar ||
                                hall.currentnode == (int)EnmFlowNodeDescp.ExitCarLeave ||
                                hall.currentnode == (int)EnmFlowNodeDescp.DealUnload)
                            {
                                //已将车辆提至车厅，判断是否可以提前下发
                                m_objcTaskTrigger.AheadSendTvTeleAndBuildHallTele(ref nActionFlag, ref requestAction, hall, queue, lctn);
                                if (nActionFlag != 0)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 刷卡处理
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="strPhysicalCardID"></param>
        /// <param name="struHallEquip"></param>
        public void SwipeICCard(ref int nActionFlag, ref CRequestAction requestAction, string strPhysicalCardID, struHallEquips struHallEquip)
        {
            nActionFlag = 0;
            if (0 == struHallEquip.nWareHouseID || 0 == struHallEquip.nHallID)
            {
                CLOGException.Trace("WorkFlowLib.TaskTriggerMgmt.SwipeICCard ", "struHallEquip.nWareHouseID or struHallEquip.nHallID is invalid");
                return;
            }           
            //根据物理卡号获取IC卡信息
            CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCardByPhysCardID(strPhysicalCardID);
            if (null != ICCardTable)
            {
                CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", string.Format("刷卡卡号：{0}，刷卡车厅：{1}", ICCardTable.iccode, struHallEquip.nHallID));
            }
            else
            {
                CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", string.Format("找不到物理卡号：{0}，刷卡车厅：{1}", strPhysicalCardID, struHallEquip.nHallID));
            }
            
            //根据库区号和车厅号获取车厅状态信息
            CDeviceStatusDto HallStatusTable = CCommonMethods.myMethods.GetDeviceStatus(struHallEquip.nWareHouseID, struHallEquip.nHallID);
            if (null == HallStatusTable)
            {
                requestAction.soundName = "20.wav";//上位控制系统故障20.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "上位控制系统故障20.wav，配置文件中库区号WareHouseID或车厅号HallID配置错误, Hall ID:" + struHallEquip.nHallID);
                return;
            }
            //检查IC卡和设备状态
            if (!m_objcTaskTrigger.CheckInfoStatus(ref nActionFlag, ref requestAction, ICCardTable, HallStatusTable))
            {
                return;
            }

            CDeviceStatusDto DeviceStatusTable = CCommonMethods.myMethods.GetDeviceStatus(ICCardTable.iccode);

            //已经在其他库区执行了刷卡
            if ( null != DeviceStatusTable && 
                (DeviceStatusTable.warehouse != struHallEquip.nWareHouseID|| DeviceStatusTable.devicecode != struHallEquip.nHallID)&&
                DeviceStatusTable.tasktype!=(int)EnmTaskType.TmpFetch)
            {
                requestAction.soundName = "08.wav";// 该卡正在作业08.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                return;
            }

            //已经将您加到取车队列
            List<CWorkQueueDto> lstWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(ICCardTable.iccode);
            foreach (CWorkQueueDto workQueueTable in lstWorkQueueTBL)
            {
                if ((int)EnmSwipeCount.GetCar == workQueueTable.swipecount || (int)EnmSwipeCount.TmpFetch == workQueueTable.swipecount||
                    workQueueTable.tasktype == (int)EnmTaskType.EntryTask || workQueueTable.tasktype == (int)EnmTaskType.ExitTask||
                    workQueueTable.tasktype == (int)EnmTaskType.TmpFetch)
                {
                    requestAction.soundName = "19.wav"; // 已经将您加到取车队列，请排队等候出车19.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    return;
                }
            }

            //根据IC卡卡号获取存车的车位信息
            CCarLocationDto CarLocation = CCommonMethods.myMethods.GetCarLocation(ICCardTable.iccode);

            #region 存车或临时取物流程
            if ((null == CarLocation)
                || (null != CarLocation && (int)EnmLocationStatus.Occupy != CarLocation.carlocstatus
                && (((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.prevnode))
                || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struHallEquip.nWareHouseID) && CConfigManagement.myPara.DicCIMCTmpFetch[struHallEquip.nWareHouseID]
                    && (int)EnmTaskType.ExitTask == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.prevnode)))))
            {
                //进车厅或进出两用车厅、或者出车厅临时取物20160314
                if ((int)EnmHallType.Entance == HallStatusTable.halltype || (int)EnmHallType.EnterOrExit == HallStatusTable.halltype
                    || (int)EnmHallType.Exit == HallStatusTable.halltype)
                {
                    //车厅进车检测OK，或取物OK
                    if ((int)EnmTaskType.EntryTask == HallStatusTable.tasktype || (int)EnmTaskType.TmpFetch == HallStatusTable.tasktype
                        || ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype && CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struHallEquip.nWareHouseID)
                        && CConfigManagement.myPara.DicCIMCTmpFetch[struHallEquip.nWareHouseID]))
                    {
                        //处理存车刷卡
                        if (m_objcTaskTrigger.DealParkingSwipe(ref nActionFlag, ref requestAction, ICCardTable, HallStatusTable, CarLocation, struHallEquip))
                        {
                            return;
                        }
                    }
                    //进出车厅正在执行取车作业
                    else if ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype)
                    {
                        requestAction.soundName = "33.wav";// 前面有人正在取车，请稍后存车33.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "前面有人正在取车，请稍后存车33.wav, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                    //进出车厅没有存取车作业在执行，且车厅空闲
                    else //if (1 == HallStatusTable.isavailable)//车厅空闲：车厅可接受指令
                    {
                        requestAction.soundName = "13.wav";// 车厅无车，请将车开到车厅内再进行刷卡存车13.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "车厅无车，请将车开到车厅内再进行刷卡存车13.wav, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                }
                else if ((int)EnmHallType.Exit == HallStatusTable.halltype)//当前刷卡的车厅是出车厅
                {
                    if ((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype)
                    {
                        requestAction.soundName = "35.wav";// 出车厅，不允许临时取物35.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "出车厅，不允许临时取物35.wav, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                    else
                    {
                        requestAction.soundName = "14.wav";//出车厅，不允许存车14.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "出车厅，不允许存车14.wav, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                }
            }
            #endregion 存车或临时取物流程
            #region 取车流程
            else if ((int)EnmLocationType.Normal == CarLocation.carloctype && ((int)EnmLocationStatus.Occupy == CarLocation.carlocstatus))
            {
                #region 有计费功能
                if (CConfigManagement.myPara.BillingFlag)
                {
                    if ((int)EnmICCardType.Temp == ICCardTable.ictype)
                    {
                        requestAction.soundName = "36.wav";// 临时卡请到收费处缴费取车36.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        return;
                    }
                    else if (((int)EnmICCardType.Fixed == ICCardTable.ictype || (int)EnmICCardType.FixedLocation == ICCardTable.ictype))
                    {
                        //IC卡过期
                        if (ICCardTable.icdeadline < DateTime.Now)
                        {
                            requestAction.soundName = "37.wav";// 该卡已到期，请到收费处缴费37.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            return;
                        }

                        //IC卡重来没有缴过费  所有的卡片都在管理室刷卡
                        if (null == ICCardTable.icdeadline)
                        {
                            requestAction.soundName = "25.wav";// 该卡未缴费，请到收费处缴费25.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            return;
                        }
                        //允许用户配置定期卡是否在管理室统一收费
                        if (CCommonMethods.myMethods.GetFixCard_AllowOutGet().Trim() == "0") 
                        {
                            requestAction.soundName = "25.wav";// 该卡未缴费，请到收费处缴费25.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            return;
                        }
                    }
                }
                #endregion 有计费功能
                
                //车厅是出车厅或进出两用车厅
                if ((int)EnmHallType.Exit == HallStatusTable.halltype || (int)EnmHallType.EnterOrExit == HallStatusTable.halltype)
                {
                    //刷卡库区与车所在库区相同
                    if (CarLocation.warehouse == struHallEquip.nWareHouseID)
                    {
                        //处理取车刷卡
                        m_objcTaskTrigger.DealGetVEHSwipe(ref nActionFlag, ref requestAction, ICCardTable, HallStatusTable, CarLocation, struHallEquip);
                    }
                    else
                    {
                        if (CBaseMethods.MyBase.IsEmpty(CarLocation.warehouse))
                        {
                            CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "获取车位对应的库区号失败, Hall ID:" + struHallEquip.nHallID);
                            return;
                        }
                        requestAction.soundName = "16.wav";//您的车辆停在其他库区16.wav
                        nActionFlag = 2;
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "您的车辆停在其他库区16.wav, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                }
                //进车厅不允许取车
                else if ((int)EnmHallType.Entance == HallStatusTable.halltype)
                {
                    if (CBaseMethods.MyBase.IsEmpty(CarLocation.warehouse))
                    {
                        CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "车位对应的库区号为空, Hall ID:" + struHallEquip.nHallID);
                        return;
                    }
                    requestAction.soundName = "15.wav";// 进车厅，不允许取车15.wav
                    nActionFlag = 2;
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "进车厅，不允许取车15.wav, Hall ID:" + struHallEquip.nHallID);
                    return;
                }
                else
                {
                    requestAction.soundName = "20.wav";//上位控制系统故障20.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "上位控制系统故障20.wav，数据库中设备类型devicetype错误, Hall ID:" + struHallEquip.nHallID);
                    return;
                }
            }
            #endregion 取车流程
            #region 异常处理
            else
            {
                if ((int)EnmLocationStatus.Entering == CarLocation.carlocstatus)
                {
                    requestAction.soundName = "17.wav";//您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡), Hall ID:" + struHallEquip.nHallID);

                }
                else if ((int)EnmLocationStatus.Outing == CarLocation.carlocstatus || (int)EnmLocationStatus.TmpFetch == CarLocation.carlocstatus)
                {
                    requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "正在为您出车，请稍后28.wav, Hall ID:" + struHallEquip.nHallID);
                }
                else if ((int)EnmLocationType.Hall == CarLocation.carloctype)
                {
                    //系统故障，把车位信息表中的所有车厅上的IC卡号字段的值清空。
                    //故障恢复之前要判断车厅的车位地址与设备状态中对应的车厅地址一致。如果不一致就是车位，把carloctype改为车位。
                    CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "数据库中IC卡" + ICCardTable.iccode + "记录的车位类型是车厅，Hall ID:" + struHallEquip.nHallID);
                }
                else if (null != CarLocation && (int)EnmLocationStatus.TmpFetch == CarLocation.carlocstatus && (int)EnmTaskType.EntryTask == HallStatusTable.tasktype)
                {
                    requestAction.soundName = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "该卡异常，请联系管理人员处理32.wav, Hall ID:" + struHallEquip.nHallID);
                }
                else
                {
                    CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "新的异常场景，需要增加对当前异常场景的处理，IC卡卡号：" + ICCardTable.iccode + "，Hall ID:" + struHallEquip.nHallID);
                }

                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                return;
            }
            #endregion 异常处理
        }

        #region 客户端请求处理函数
        /// <summary>
        /// 设备可接受新指令取车排队处理
        /// </summary>
        /// <param name="requestAction"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="isHall"></param>
        /// <returns></returns>
        public bool DeviceTelegramQueue(ref int nActionFlag, ref CRequestAction requestAction, CDeviceStatusDto deviceStatus, bool isHall)
        {
            //bool isGetWorkQueueICCard = m_objcTaskTrigger.DeviceTelegramQueue(ref nActionFlag, ref requestAction, ref deviceStatus, isHall);

            //if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            //{
            //    nActionFlag += 4;
            //}
            //return isGetWorkQueueICCard;
            return false;
        }

        /// <summary>
        /// 临时取物
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.Null;//传入的参数卡号为空
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.LossORCancel;//IC卡注销或挂失
        /// EnmFaultType.NotMatch;//IC卡类型不正确
        /// EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
        /// EnmFaultType.NotFoundEquip;       //没有找到指定设备
        /// EnmFaultType.NotAvailable;// 车厅不可接收指令
        /// EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
        /// EnmFaultType.EquipNotCorrect;//车厅不是进出两用车厅
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// </summary>
        /// <param name="deviceStatusTBL">车厅设备</param>
        /// <param name="carPOSNTable">车位对象</param>
        /// <returns></returns>
        public EnmFaultType TmpFetch(CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable)
        {
            return m_objcTaskTrigger.TmpFetch(deviceStatusTBL, carPOSNTable);
        }
        
        /// <summary>
        /// 车辆出库
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundCarPOSN;    //指定的源或目的车位不存在
        /// EnmFaultType.NotFoundEquip;       //没有找到指定目的车厅
        /// EnmFaultType.IsNotHallEquip;      //指定的目的地址不是车厅
        /// EnmFaultType.HallEnter;         //车厅是进车厅不允许出车
        /// EnmFaultType.NoCarInGarage;       //源车位没有车
        /// EnmFaultType.NotAvailable;      //车厅设备不可接收指令
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.NotSameWareHouse;//刷卡库区与车所在库区不同
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.Wait;//已经将您加到取车队列，请排队等候
        /// EnmFaultType.Add; //前面有人正在取车，已经将您加到取车队列，请排队等候
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.NotNormalCarPOSN;//源车位不是正常车位。
        /// </summary>
        /// <param name="strICCardID">IC卡号或者车牌号</param>
        /// <param name="deviceStatusTBL">车厅设备</param>
        /// <param name="carPOSNTable">车位对象</param>
        /// <param name="bApp">APP手机取车标志 true：手机取车；false：否</param>
        /// <returns></returns>
        public EnmFaultType VehicleExit(string strICCardID, CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable, bool bApp)
        {
            return m_objcTaskTrigger.VehicleExit(strICCardID, deviceStatusTBL, carPOSNTable, bApp);
        }

        /// <summary>
        /// 车辆挪移
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundEquip; //没有空闲设备，分配设备失败
        /// EnmFaultType.NotFoundCarPOSN; //指定的源或目的车位不存在
        /// EnmFaultType.NotNormalCarPOSN; //源车位或目的车位不是正常车位
        /// EnmFaultType.NoCarInGarage; //源车位没有车
        /// EnmFaultType.CarInGarage;   //目的车位有车
        /// EnmFaultType.TaskOnICCard;//目的车位的车辆正在作业
        /// EnmFaultType.NoICCardInfo;//源车位的IC卡没有制卡
        /// EnmFaultType.FixedCarPOSN;//目的车位是其他车主的固定车位，临时卡或定期卡无法挪移到固定车位卡的车位上
        /// EnmFaultType.NotMatch;//目的车位是其他车主的固定车位
        /// EnmFaultType.OverCarInSize; //目的车位尺寸无法满足源车位的车辆
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAvailable;  //设备不可接收指令;
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.FailConnection; //连接PLC通信失败
        /// </summary>
        /// <param name="carLocationTBLSrc">源车位</param>
        /// <param name="carLocationTBLDest">目的车位</param>
        /// <returns></returns>
        public EnmFaultType VehicleMove(CCarLocationDto carLocationTBLSrc, CCarLocationDto carLocationTBLDest)
        {
            return m_objcTaskTrigger.VehicleMove(carLocationTBLSrc, carLocationTBLDest);
        }

        /// <summary>
        /// 处理存车扫描
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.Fail; //获取通信模式块失败，请查看配置文件是否正确
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.NotMatch;// 与第一次刷卡卡号不一致，请重新刷卡
        /// EnmFaultType.OverCarInSize;//车体超限，请退出
        /// EnmFaultType.NotFoundCarPOSN;// 已无合适车位，请退出
        /// EnmFaultType.Fail;// 上位控制系统故障
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="strICCard"></param>
        /// <param name="HallStatusTable"></param>
        /// <param name="CarLocation"></param>
        /// <returns></returns>
        public EnmFaultType DealParkingScan(ref int nActionFlag, ref CRequestAction requestAction, string strICCard, CDeviceStatusDto HallStatusTable, CCarLocationDto CarLocation)
        {
            return m_objcTaskTrigger.DealParkingScan(ref nActionFlag, ref requestAction, strICCard, HallStatusTable, CarLocation);
        }

        /// <summary>
        /// 处理MURO继续时，进行装载操作
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <param name="isHall">是进行车厅装载作业</param>
        /// <returns></returns>
        public int DealMUROLoadCmd(CDeviceStatusDto etv, CCarLocationDto car, bool isHall) 
        {
            return m_objcTaskTrigger.DealMUROLoadCmd(etv, car, isHall);
        }

        /// <summary>
        /// 处理MURO继续时，进行卸载操作
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <returns></returns>
        public int DealMuroUnloadCmd(CDeviceStatusDto etv, CCarLocationDto car) 
        {
            return m_objcTaskTrigger.DealMuroUnloadCmd(etv, car);
        }

        #endregion

        #region 分配算法algorithm
        /// <summary>
        /// 分配车厅设备算法——根据车厅排队最少分配
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        public int ALLOCHallByQueue(int nWareHouse,int region,CCarLocationDto loc)
        {
            return m_objcTaskTrigger.ALLOCHallByQueue(nWareHouse,region,loc);
        }

        /// <summary>
        /// 检查指定的车厅是否有可用的ETV
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <returns></returns>
        public bool CheckETVStat(int nWareHouse, int nHallID)
        {
            return m_objcTaskTrigger.CheckETVStat(nWareHouse, nHallID);
        }
        #endregion  通用算法algorithm
    }
}
