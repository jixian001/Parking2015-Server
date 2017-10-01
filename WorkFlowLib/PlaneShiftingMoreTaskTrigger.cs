using System.Collections.Generic;
using System.Linq;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommonMethodLib;
using AbstractActionLib;
using System;

namespace WorkFlowLib
{
    /// <summary>
    /// 平面移动类停车设备Plane Shifting Mechanical Parking System More一层2台TV
    /// </summary>
    public class CPlaneShiftingMoreTaskTrigger : CTaskTrigger
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CPlaneShiftingMoreTaskTrigger(int nPLCID)
        {
            if (CConfigManagement.myPara.HashGarageType.ContainsKey(nPLCID) && (0 == string.Compare("PlaneShiftingMore", (string)CConfigManagement.myPara.HashGarageType[nPLCID])))
            {
                base.m_nColorAddrHall = CConfigManagement.myPara.ColorAddrHallID[nPLCID];//车厅故障红色与绿色界限（小于nColorAddr为红色，否则绿色）
                base.m_nColorAddrETVorTV = CConfigManagement.myPara.ColorAddrETVorTVID[nPLCID];//ETV或TV故障红色与绿色界限（小于nColorAddr为红色，否则绿色）
                CLOGException.Trace(nPLCID, "CPlaneShiftingMoreTaskTrigger", "Initial CPlaneShiftingMoreTaskTrigger successfully, nColorAddrHall: " + base.m_nColorAddrHall + " ,nColorAddrETVorTV: " + base.m_nColorAddrETVorTV);
            }
        }

        #region 抽象方法重载
        /// <summary>
        /// 车厅确认有车入库，入库指令接受(1001,54)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        protected override void EnterCommandAccept(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus)
        {
            nActionFlag = 1;
            if (deviceStatus.iccode == struTelegram.strICCardID && deviceStatus.currentnode == (int)EnmFlowNodeDescp.EnterCommandAccept)
            {
                requestAction.telegram = "1-54";
                return;
            }
            nActionFlag = 5;
            requestAction.telegram = "1-54";
            // 车厅作业流程节点类型赋值
            deviceStatus.prevnode = deviceStatus.currentnode;
            deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCommandAccept;
            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
        }
     
        /// <summary>
        /// 设备空闲时,下发队列中有报文的队列
        /// </summary>       
        public override void SendQueueHasTelegram(ref int nActionFlag, ref CRequestAction requestAction, CDeviceStatusDto eqp, CWorkQueueDto queue, bool isUnLoad) 
        {
            try 
            {
                nActionFlag = 0;
                #region 解析报文
                int m_nTelegramLen = 50;
                CTelegramParse m_telegramParser = new CTelegramParse();
                
                Int16[] objTelegram = new Int16[m_nTelegramLen];
                m_telegramParser.StringToInt16s(ref objTelegram, 0, m_nTelegramLen - 1, 4, queue.waitsendtelegram);  //解析出存的报文
                
                struTelegram struSendTelegram;
                m_telegramParser.UnpackTelegram(out struSendTelegram, objTelegram);

                #endregion

                if (queue.tasktype == (int)EnmTaskType.RetrySend)
                {
                    #region
                    struSendTelegram.nPLCSendFlag = eqp.warehouse;
                    struSendTelegram.nMasterType = (int)queue.tasktype;
                    requestAction.structTelegram = struSendTelegram;

                    requestAction.telegram = string.Format("{0}-{1}", struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS);
                    //删除队列
                    requestAction.lstTableDto.Add(new struTableDto(queue, 3));

                    if (1 != (nActionFlag & 1))
                    {
                        nActionFlag += 1;
                    }

                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    //记录
                    CWException.WriteLog(string.Format("RetrySendTelegram will send ({0},{1}),devicecode:{2},iccode:{3}",
                        struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS, eqp.devicecode, queue.iccode), 4);
                    return;
                    #endregion
                }

                if (eqp.devicetype == (int)EnmSMGType.Hall)
                {
                    #region 车厅报文时，直接下发
                    struSendTelegram.nPLCSendFlag = eqp.warehouse;
                    struSendTelegram.nMasterType = (int)queue.tasktype;
                    requestAction.structTelegram = struSendTelegram;

                    requestAction.telegram = string.Format("{0}-{1}", struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS);
                    //绑定设备
                    eqp.tasktype = (int)queue.tasktype;
                    eqp.iccode = queue.iccode;
                    eqp.prevnode = null;
                    eqp.currentnode = null;
                    eqp.scrloc = struSendTelegram.strSrcLocAddr;
                    eqp.desccloc = struSendTelegram.strDestLocAddr;
                    requestAction.lstTableDto.Add(new struTableDto(eqp, 2));
                    //删除队列
                    requestAction.lstTableDto.Add(new struTableDto(queue, 3));

                    if (1 != (nActionFlag & 1))
                    {
                        nActionFlag += 1;
                    }

                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    //记录
                    CWException.WriteLog(string.Format("queueHasTelegram will send ({0},{1}),devicecode:{2},iccode:{3}",
                        struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS, eqp.devicecode, queue.iccode), 4);
                    #endregion
                }
                else if (eqp.devicetype == (int)EnmSMGType.ETV)
                {                   
                    if (queue.tasktype == (int)EnmTaskType.AvoidMove)
                    {
                        #region 如果是避让作业，则直接下发
                        struSendTelegram.nPLCSendFlag = eqp.warehouse;
                        struSendTelegram.nMasterType = (int)queue.tasktype;
                        requestAction.structTelegram = struSendTelegram;

                        requestAction.telegram = string.Format("{0}-{1}", struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS);
                        //绑定设备,只有作业为空才允许再写入，以防是等待卸载时执行的避让
                        if (eqp.tasktype == (int)EnmTaskType.Init)
                        {
                            eqp.tasktype = (int)queue.tasktype;
                            eqp.iccode = queue.iccode;
                            eqp.prevnode = null;
                            eqp.currentnode = null;
                            eqp.scrloc = struSendTelegram.strSrcLocAddr;
                            eqp.desccloc = struSendTelegram.strDestLocAddr;
                            requestAction.lstTableDto.Add(new struTableDto(eqp, 2));
                        }
                        //删除队列
                        requestAction.lstTableDto.Add(new struTableDto(queue, 3));

                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }

                        if (4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //记录
                        CWException.WriteLog(string.Format("queueHasTelegram will send ({0},{1}),devicecode:{2},iccode:{3}",
                            struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS, eqp.devicecode, queue.iccode), 4);
                        #endregion
                    }
                    else //(13,1) or (14,1)
                    {
                        #region
                        //判断避让
                        CTelegramSender teleSender = new CTelegramSender(requestAction.PLCServer);
                        string toAddress = "";
                        if (isUnLoad)
                        {
                            toAddress = struSendTelegram.strDestLocAddr;
                        }
                        else
                        {
                            toAddress = struSendTelegram.strSrcLocAddr;
                        }

                        if (teleSender.DealAvoid(eqp, toAddress, queue.iccode))
                        {
                            struSendTelegram.nPLCSendFlag = eqp.warehouse;
                            struSendTelegram.nMasterType = (int)queue.tasktype;
                            requestAction.structTelegram = struSendTelegram;

                            requestAction.telegram = string.Format("{0}-{1}", struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS);

                            //是卸载时，不更新设备信息
                            if (isUnLoad == false)
                            {
                                eqp.tasktype = (int)queue.tasktype;
                                eqp.iccode = queue.iccode;
                                eqp.prevnode = null;
                                eqp.currentnode = (int)EnmFlowNodeDescp.Load;
                                eqp.scrloc = struSendTelegram.strSrcLocAddr;
                                eqp.desccloc = struSendTelegram.strDestLocAddr;
                                requestAction.lstTableDto.Add(new struTableDto(eqp, 2));                               
                            }
                            else 
                            {
                                eqp.prevnode = eqp.currentnode;
                                eqp.currentnode = (int)EnmFlowNodeDescp.UnLoad;
                                requestAction.lstTableDto.Add(new struTableDto(eqp, 2));
                            }
                            
                            //删除队列
                            requestAction.lstTableDto.Add(new struTableDto(queue, 3));

                            if (1 != (nActionFlag & 1))
                            {
                                nActionFlag += 1;
                            }

                            if (4 != (nActionFlag & 4))
                            {
                                nActionFlag += 4;
                            }

                            CWException.WriteLog(string.Format("queueHasTelegram will send ({0},{1}),devicecode:{2},iccode:{3}",
                                struSendTelegram.nTelegramType, struSendTelegram.nSubTypeEMS, eqp.devicecode, queue.iccode), 4);
                        }
                        #endregion
                    }
                }

            }
            catch (Exception ex) 
            {
                CWException.WriteError(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 车厅空闲时,执行队列,下发车厅报文，组装TV报文
        /// </summary>    
        public override void SendHallTelegramAndBuildTvTele(ref int nActionFlag, ref CRequestAction requestAction, CDeviceStatusDto hall, CWorkQueueDto commitQueue, CCarLocationDto lctn) 
        {
            try 
            {
                nActionFlag = 0;
                #region 发送车厅报文，组装装载报文
                CDeviceStatusDto equip = this.allocateEtvDevice(lctn,hall);
                if (equip == null)
                {
                    CWException.WriteLog(string.Format("异常 队列-卡号：{0},类型：{1}，车厅：{2}，出库车位：{3} 在执行队列时找不到TV.",
                        commitQueue.iccode, commitQueue.tasktype, commitQueue.devicecode, lctn.carlocaddr), 4);
                    return;
                }

                if (equip.devicemode != (int)EnmModel.Automatic) 
                {
                    CWException.WriteLog(string.Format("队列-卡号：{0},类型：{1}，车厅：{2}，出库车位：{3} 在执行队列时TV不处于全自动，作业无法下发.",
                       commitQueue.iccode, commitQueue.tasktype, commitQueue.devicecode, lctn.carlocaddr), 4);
                    return;
                }

                if (commitQueue.tasktype == (int)EnmTaskType.TmpFetch)
                {
                    requestAction.telegram = string.Format("2-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, equip.devicecode);
                    lctn.carlocstatus = (int)EnmLocationStatus.TmpFetch;
                }
                else
                {
                    requestAction.telegram = string.Format("3-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, equip.devicecode);
                    lctn.carlocstatus = (int)EnmLocationStatus.Outing;
                }
                //更新车位状态           
                requestAction.lstTableDto.Add(new struTableDto(lctn, 2));
                //建立报文
                struTelegram struTelegram = new struTelegram();
                struTelegram.nPLCSendFlag = (int)hall.warehouse;
                struTelegram.nEquipID = hall.devicecode;
                struTelegram.strICCardID = commitQueue.iccode;

                struTelegram.strCarSize = lctn.carsize;
                struTelegram.nWheelbase = (int)lctn.carwheelbase;
                struTelegram.overallLg = (int)lctn.overallLg;
                struTelegram.overHang = (int)lctn.overhang;

                struTelegram.strSrcLocAddr = lctn.carlocaddr;
                struTelegram.strDestLocAddr = hall.deviceaddr;
                struTelegram.nEndFlag = 9999;
                struTelegram.nMasterType = (int)commitQueue.tasktype;
                requestAction.structTelegram = struTelegram;

                try
                {
                    Dictionary<int, struHallEquips> m_dictHallID = (Dictionary<int, struHallEquips>)CConfigManagement.myPara.DicHallIDDictionary[(int)lctn.warehouse];
                    if (m_dictHallID.ContainsKey(hall.devicecode))
                    {
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        requestAction.soundName = "28.wav";
                        requestAction.struHallEquip = m_dictHallID[hall.devicecode];
                    }
                }
                catch (Exception ex)
                {
                    CLOGException.Trace("DeviceTelegramQueue 车厅查找异常 ", CBaseMethods.MyBase.GetExceptionInfo(ex));
                }

                //绑定设备
                hall.tasktype = commitQueue.tasktype;
                hall.iccode = commitQueue.iccode;
                hall.prevnode = null;
                hall.currentnode = null;
                hall.scrloc = lctn.carlocaddr;
                hall.desccloc = hall.deviceaddr;
                requestAction.lstTableDto.Add(new struTableDto(hall, 2));

                //删除队列
                requestAction.lstTableDto.Add(new struTableDto(commitQueue, 3));

                if (1 != (nActionFlag & 1))
                {
                    nActionFlag += 1;
                }            
                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }

                CWException.WriteLog(string.Format("workQueue has been dealed,devicecode:{0},iccode:{1},devicetype:{2}.",
                    commitQueue.devicecode, commitQueue.iccode, commitQueue.tasktype), 4);
                #endregion
            }
            catch (Exception ex) 
            {
                CWException.WriteError(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 车厅允许TV提前装载，判断是否允许下发，并下发相应报文
        /// </summary>  
        public override void AheadSendTvTeleAndBuildHallTele(ref int nFlag, ref CRequestAction requestAct, CDeviceStatusDto hall, CWorkQueueDto commitQueue, CCarLocationDto lctn) 
        {
            nFlag = 0;
            #region
            try 
            {
                CDeviceStatusDto equip = this.allocateEtvDevice(lctn, hall);
                if (equip == null)
                {
                    CWException.WriteLog(string.Format("异常 队列(提前装载)-卡号：{0},类型：{1}，车厅：{2}，出库车位：{3} 在执行队列时找不到TV.",
                        commitQueue.iccode, commitQueue.tasktype, commitQueue.devicecode, lctn.carlocaddr), 4);
                    return;
                }
                if (equip.isable == 1 && 
                    equip.isavailable == 1 && 
                    equip.tasktype == (int)EnmTaskType.Init) 
                {
                    CTelegramSender teleSender = new CTelegramSender(requestAct.PLCServer);
                    //判断是否允许下发
                    if (teleSender.DealAvoid(equip, lctn.carlocaddr, commitQueue.iccode)) 
                    {
                        if (commitQueue.tasktype == (int)EnmTaskType.TmpFetch)
                        {
                            requestAct.telegram = string.Format("13-1,2-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, hall.devicecode);
                        }
                        else
                        {
                            requestAct.telegram = string.Format("13-1,3-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, hall.devicecode);
                        }
                        //建立报文
                        struTelegram struTelegram = new struTelegram();
                        struTelegram.nPLCSendFlag = (int)lctn.warehouse;
                        struTelegram.nEquipID = equip.devicecode;
                        struTelegram.strICCardID = lctn.iccode;

                        struTelegram.strCarSize = lctn.carsize;
                        struTelegram.nWheelbase = (int)lctn.carwheelbase;
                        struTelegram.overallLg = (int)lctn.overallLg;
                        struTelegram.overHang = (int)lctn.overhang;

                        struTelegram.strSrcLocAddr = lctn.carlocaddr;
                        struTelegram.strDestLocAddr = hall.deviceaddr;
                        struTelegram.nEndFlag = 9999;
                        struTelegram.nMasterType = (int)commitQueue.tasktype;
                        requestAct.structTelegram = struTelegram;

                        //更改车位状态
                        lctn.carlocstatus = (int)EnmLocationStatus.Outing;
                        requestAct.lstTableDto.Add(new struTableDto(lctn, 2));

                        //绑定设备
                        equip.tasktype = commitQueue.tasktype;
                        equip.iccode = commitQueue.iccode;
                        equip.prevnode = null;
                        equip.currentnode = (int)EnmFlowNodeDescp.Load;
                        equip.scrloc = lctn.carlocaddr;
                        equip.desccloc = hall.deviceaddr;
                        requestAct.lstTableDto.Add(new struTableDto(equip, 2));

                        //删除队列
                        requestAct.lstTableDto.Add(new struTableDto(commitQueue, 3));

                        if (1 != (nFlag & 1))
                        {
                            nFlag += 1;
                        }

                        if (4 != (nFlag & 4))
                        {
                            nFlag += 4;
                        }                               
                    }
                }
            }
            catch (Exception ex) 
            {
                CWException.WriteError(ex.ToString());
                throw ex;
            }
            #endregion
        }

        #endregion

        #region 虚方法重载
        /// <summary>
        /// 入库时检测车辆处理(1001,101)
        /// 特殊处理：分配车位和TV，并发送（13，1）报文
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        /// <param name="deviceStatus"></param>
        protected override void EnterCarChecked(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus)
        {
            nActionFlag = 0;
            string strCarSize = struTelegram.strCarSize;// 车辆尺寸
            // 清空当前卡号存取车排队信息
            ClearWorkQueueSwipe(ref requestAction, struTelegram.strICCardID);
            // 清空当前车厅存车刷一次卡信息
            ClearWorkQueueSwipe(ref requestAction);

            struTelegram struTelegramTemp = requestAction.structTelegram;
            struTelegramTemp.nMasterType = (int)EnmTaskType.EntryTask;           

            if (null == deviceStatus)
            {
                CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "deviceStatus为空");
                return;
            }

            if (deviceStatus.iccode == struTelegram.strICCardID && deviceStatus.currentnode == (int)EnmFlowNodeDescp.EnterCarCheck)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(strCarSize) || 3 != strCarSize.Length || !CBaseMethods.MyBase.IsUIntNumWithoutZero(strCarSize))
            {
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                requestAction.soundName = "30.wav";// 无效车辆尺寸，请退出30.wav               
                return;
            }

            //判断允许存入最大车辆尺寸参数的有效性
            if (string.IsNullOrWhiteSpace(CConfigManagement.myPara.CarMaxSize))
            {
                nActionFlag = 2;
                requestAction.soundName = "31.wav";// 允许存入最大车辆尺寸参数配置错误，请联系管理员处理31.wav                
                return;
            }

            // 判断车辆尺寸是否超长 "122"为车辆最大尺寸
            if (false == CBaseMethods.MyBase.CheckCarSize(strCarSize, CConfigManagement.myPara.CarMaxSize))
            {
                #region
                nActionFlag = 3;
                requestAction.telegram = "1-2"; //string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "04.wav";// 车体超限，请退出04.wav

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.Oversize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.Oversize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }
                return;
                #endregion
            }

            // “目的地址”---车位
            CCarLocationDto carLocationDest = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr);

            if (((int)EnmTaskType.TmpFetch == deviceStatus.tasktype || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struTelegram.nPLCSendFlag)
                            && CConfigManagement.myPara.DicCIMCTmpFetch[struTelegram.nPLCSendFlag]))
                && null != carLocationDest)
            {
                #region 临时取物处理
                nActionFlag = 3;
                CDeviceStatusDto etv = this.allocateEtvDevice(carLocationDest, deviceStatus);
                if (etv == null)
                {
                    //转存找不到ETV时，先报语音，暂不做退出处理
                    nActionFlag = 2;
                    requestAction.soundName = "42.wav";
                    CWException.WriteLog(string.Format("取物转存时，找不到可用的TV，Iccard:{0},hallID:{1}", struTelegram.strICCardID, deviceStatus.devicecode), 4);
                    return;
                }
                requestAction.structTelegram = struTelegramTemp;
                // 【报文类型2 （1），报文子类型EMS 3（1）】
                requestAction.telegram = string.Format("1-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, etv.devicecode);
                requestAction.soundName = "26.wav";

                carLocationDest.carlocstatus = (int)EnmLocationStatus.Entering;               
                //轴距，外形
                carLocationDest.carwheelbase = struTelegram.nWheelbase;
                //中心偏移，后轮距巷道，车重
                carLocationDest.offcenter = struTelegram.offCenter;
                carLocationDest.rearwheeldis = struTelegram.rearWheelDis;
                carLocationDest.carweight = struTelegram.carWeight;
                requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));

                // 车厅作业流程节点类型赋值
                deviceStatus.tasktype = (int)EnmTaskType.EntryTask;
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                deviceStatus.scrloc = deviceStatus.scrloc;
                deviceStatus.desccloc = carLocationDest.carlocaddr;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                return;
                #endregion
            }

            // 分配车位算法
            int eqpID;
            string strError = string.Empty;
            CCarLocationDto carLocation = this.AllocateCarLocation(struTelegram, ref strError, out eqpID);          

            //没找到合适的车位
            if (null == carLocation)
            {
                #region
                if (!string.IsNullOrWhiteSpace(strError))
                {
                    nActionFlag = 2;
                    requestAction.soundName = strError;// 该卡异常，请联系管理人员处理32.wav
                    CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "该卡异常，请联系管理人员处理32.wav");// trace
                    return;
                }

                nActionFlag = 3;
                requestAction.telegram = "1-2";// string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "27.wav";// 已无合适车位，请退出27.wav
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.NoSuitableSize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.NoSuitableSize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }
                return;
                #endregion
            }
            //找不到可用的移动设备
            if (eqpID == 0)
            {
                #region
                nActionFlag = 3;
                requestAction.telegram = "1-2";
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "42.wav";// 已无合适车位，请退出27.wav
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.NoSuitableSize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.NoSuitableSize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }
                return;
                #endregion
            }           

            if (string.IsNullOrWhiteSpace(carLocation.carlocsize))
            {
                CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "行：carLocation.carlocsize为空");
                return;
            }

            // 判断车辆尺寸是否小于车位尺寸
            if (false == CBaseMethods.MyBase.CheckCarSize(strCarSize, carLocation.carlocsize) ||
                carLocation.carloctype != (int)EnmLocationType.Normal ||
                carLocation.carlocstatus != (int)EnmLocationStatus.Space)
            {
                #region
                nActionFlag = 3;
                requestAction.telegram = "1-2";//string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "27.wav";//已无合适车位，请退出27.wav"34.wav";// 车辆尺寸超出固定车位尺寸，请退出34.wav
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.NoSuitableSize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.NoSuitableSize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }
                return;
                #endregion
            }

            nActionFlag = 7;
            requestAction.soundName = "26.wav";// 已接受此次存车,请慢走26.wav        
            // 【报文类型2 （1），报文子类型EMS 3（1）】
            requestAction.telegram = string.Format("1-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, eqpID);
           
            struTelegramTemp.strSrcLocAddr = deviceStatus.deviceaddr;         
            struTelegramTemp.strDestLocAddr = carLocation.carlocaddr;
            requestAction.structTelegram = struTelegramTemp;
              
            carLocation.carlocstatus = (int)EnmLocationStatus.Entering;// 正在入库
            carLocation.iccode = struTelegram.strICCardID;
            //轴距，外形
            carLocation.carwheelbase = struTelegram.nWheelbase;
            carLocation.carsize = struTelegram.strCarSize;
            //中心偏移，后轮距巷道，车重
            carLocation.offcenter = struTelegram.offCenter;
            carLocation.rearwheeldis = struTelegram.rearWheelDis;
            carLocation.carweight = struTelegram.carWeight;

            carLocation.carintime = DateTime.Now;
            requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));

            // 车厅作业流程节点类型赋值
            deviceStatus.tasktype = (int)EnmTaskType.EntryTask;
            deviceStatus.iccode = struTelegram.strICCardID;

            deviceStatus.prevnode = deviceStatus.currentnode;
            deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
            deviceStatus.scrloc = deviceStatus.deviceaddr;
            deviceStatus.desccloc = carLocation.carlocaddr;
            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
        }

        /// <summary>
        /// 处理ETV/TV装载协议(1013,1)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public override bool DealLoadTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            if (1 != struTelegram.nSubTypePLC)
            {
                return true;
            }

            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);
            if (null == deviceStatus)
            {
                return true;
            }

            CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.strICCardID);
            if (null == carLocation)
            {
                #region
                string lctnAddrs = "";
                if (deviceStatus.tasktype == (int)EnmTaskType.EntryTask)
                {
                    lctnAddrs = deviceStatus.desccloc;
                }
                else
                {
                    lctnAddrs = deviceStatus.scrloc;
                }
                if (lctnAddrs != null)
                {
                    carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, lctnAddrs);
                }
                #endregion
            }

            if (carLocation == null)
            {
                return true;
            }
            if (deviceStatus.tasktype == (int)EnmTaskType.Init) 
            {
                return true;
            }

            if (deviceStatus.currentnode == (int)EnmFlowNodeDescp.WillUnLoad||
                deviceStatus.currentnode==(int)EnmFlowNodeDescp.UnLoad)
            {
                return true;
            }
            //取车时(目的地址为车厅地址)
            #region
            string descAddrs = "";
            if (!CBaseMethods.MyBase.IsEmpty(struTelegram.strDestLocAddr))
            {
                descAddrs = struTelegram.strDestLocAddr;
            }
            else
            {
                descAddrs = deviceStatus.desccloc;
            }
            #endregion
            CDeviceStatusDto deviceStatusHallDest = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, descAddrs, EnmSMGType.Hall);
            
            nActionFlag = 1;
            struTelegram struT = requestAction.structTelegram;
            struT.nMasterType = (int)deviceStatus.tasktype;
            requestAction.structTelegram = struT;
            
            requestAction.telegram = string.Format("13-51,14-1-{0}", CConfigManagement.myPara.EquipIsAviliableID);
           
            // ETV设备
            deviceStatus.iccode = struTelegram.strICCardID;
            deviceStatus.prevnode = deviceStatus.currentnode;
            deviceStatus.currentnode = (int)EnmFlowNodeDescp.WillUnLoad;
            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

            #region 更新车位信息
            if (deviceStatus.tasktype == (int)EnmTaskType.EntryTask)
            {
                carLocation.carlocstatus = (int)EnmLocationStatus.Entering;
                carLocation.iccode = deviceStatus.iccode;
                carLocation.carwheelbase = struTelegram.nWheelbase;
                carLocation.overallLg = struTelegram.overallLg;
                carLocation.overhang = struTelegram.overHang;
                requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
            }
            #endregion
           
            #region
            string scrAddrs = "";
            if (!CBaseMethods.MyBase.IsEmpty(struTelegram.strSrcLocAddr))
            {
                scrAddrs = struTelegram.strSrcLocAddr;
            }
            else
            {
                scrAddrs = deviceStatus.scrloc;
            }
            #endregion
            CDeviceStatusDto deviceStatusHall = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, scrAddrs, EnmSMGType.Hall);
            // 车厅设备(存车时)
            if (null != deviceStatusHall)
            {
                if (((int)EnmTaskType.EntryTask == deviceStatusHall.tasktype || (int)EnmTaskType.TmpFetch == deviceStatusHall.tasktype))
                {
                    deviceStatusHall.iccode = null;
                    deviceStatusHall.tasktype = (int)EnmTaskType.Init;
                    // 车厅作业流程节点类型赋值
                    deviceStatusHall.prevnode = deviceStatusHall.currentnode;
                    deviceStatusHall.currentnode = (int)EnmFlowNodeDescp.DealLoad;
                  
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusHall, 2));
                }
            }
            else
            {
                if (null != deviceStatusHallDest)
                {
                    if (deviceStatusHallDest.iccode != struTelegram.strICCardID)
                    {// 车厅作业流程节点类型赋值(取车排队)
                        deviceStatusHallDest.queueprevnode = deviceStatusHallDest.queuecurrentnode;
                        deviceStatusHallDest.queuecurrentnode = (int)EnmFlowNodeDescp.DealLoad;
                    }
                    else
                    {// 车厅作业流程节点类型赋值
                        deviceStatusHallDest.prevnode = deviceStatusHallDest.currentnode;
                        deviceStatusHallDest.currentnode = (int)EnmFlowNodeDescp.DealLoad;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusHallDest, 2));
                }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理取车刷卡+避让处理
        /// 返回值：
        /// 有动作需要处理：true
        /// 无动作需要处理：false
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="ICCardTable"></param>
        /// <param name="HallStatusTable"></param>
        /// <param name="CarLocation"></param>
        /// <param name="struHallEquip"></param>
        /// <returns></returns>
        public override bool DealGetVEHSwipe(ref int nActionFlag, ref CRequestAction requestAction, CICCardDto ICCardTable, CDeviceStatusDto HallStatusTable,
            CCarLocationDto CarLocation, struHallEquips struHallEquip)
        {
            nActionFlag = 0;
            CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(ICCardTable.iccode);
            if (null != deviceStatusTBL)
            {
                requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav              
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                return true;
            }

            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetWorkQueueByHallCode(HallStatusTable.warehouse, HallStatusTable.devicecode);
            if (lstSwipeQueueOut.Count > CConfigManagement.myPara.CarExitQueueMaxCount)
            {
                requestAction.soundName = "40.wav";// 取车排队的人数超过配置的阈值
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                return true;
            }

            //无取车排队
            if (0 == lstSwipeQueueOut.Count)
            {
                //车厅无作业
                if (CBaseMethods.MyBase.IsEmpty(HallStatusTable.tasktype))
                {
                    #region
                    if (!string.IsNullOrWhiteSpace(HallStatusTable.iccode))
                    {
                        #region 有卡号的话就是系统异常
                        //清空当前设备在设备状态表中的IC卡卡号值，提示重新刷卡取车。
                        // 清空当前卡号存取车排队信息
                        ClearWorkQueueSwipe(ref requestAction, ICCardTable.iccode);
                        HallStatusTable.iccode = null;
                        requestAction.soundName = "12.wav";//上位控制系统异常,请重新刷卡取车12.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "上位控制系统异常,数据库表HallStatusTable中的iccode不为空, Hall ID:" + struHallEquip.nHallID);
                        return true;
                        #endregion
                    }
                    else if (1 == HallStatusTable.isavailable) //可接收新指令
                    {
                        #region 可接收新指令,下发车厅报文，同时组装（13，1）
                        CDeviceStatusDto deviceStatusETV = this.allocateEtvDevice(CarLocation, HallStatusTable);
                        if (deviceStatusETV == null)
                        {
                            requestAction.soundName = "42.wav";            
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            return true;
                        }

                        //直接发取车报文
                        requestAction.telegram = string.Format("3-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, deviceStatusETV.devicecode);

                        struTelegram struTelegram = new struTelegram();
                        struTelegram.nPLCSendFlag = struHallEquip.nWareHouseID;
                        struTelegram.nEquipID = HallStatusTable.devicecode;
                        struTelegram.strICCardID = ICCardTable.iccode;
                        struTelegram.strCarSize = CarLocation.carsize;

                        struTelegram.nWheelbase = (int)CarLocation.carwheelbase;
                        struTelegram.overallLg = (int)CarLocation.overallLg;
                        struTelegram.overHang = (int)CarLocation.overhang;

                        struTelegram.strSrcLocAddr = CarLocation.carlocaddr;
                        struTelegram.strDestLocAddr = HallStatusTable.deviceaddr;
                        struTelegram.nEndFlag = 9999;
                        struTelegram.nMasterType = (int)EnmTaskType.ExitTask;
                        requestAction.structTelegram = struTelegram;

                        requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav

                        HallStatusTable.iccode = ICCardTable.iccode;
                        HallStatusTable.tasktype = (int)EnmTaskType.ExitTask;
                        // 车厅作业流程节点类型赋值
                        HallStatusTable.prevnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        HallStatusTable.currentnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        HallStatusTable.scrloc = CarLocation.carlocaddr;
                        HallStatusTable.desccloc = HallStatusTable.deviceaddr;

                        requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));

                        CarLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                        requestAction.lstTableDto.Add(new struTableDto(CarLocation, 2));

                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        return true;
                        #endregion
                    }
                    else
                    {
                        #region 加入取车排队队列
                        requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav

                        CWorkQueueDto getCarQueue = new CWorkQueueDto();
                        getCarQueue.devicecode = struHallEquip.nHallID;
                        getCarQueue.warehouse = struHallEquip.nWareHouseID;
                        getCarQueue.iccode = ICCardTable.iccode;
                        getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                        getCarQueue.priorityid = ICCardTable.priorityid;

                        getCarQueue.tasktype = (int)EnmTaskType.ExitTask;
                        requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1)); //插入

                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag = 2;
                        }
                        if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }

                        return true;
                        #endregion
                    }
                    #endregion
                }
                else //车厅有作业
                {
                    #region 要不要提前装载，如果不要的，则将其加入队列中,如果要下发，先不绑定于设备，还要判断是否要避让呢
                    if (HallStatusTable.tasktype == (int)EnmTaskType.ExitTask)
                    {
                        CDeviceStatusDto deviceStatusETV = this.allocateEtvDevice(CarLocation, HallStatusTable);
                        if (deviceStatusETV == null)
                        {
                            requestAction.soundName = "42.wav";
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            return true;
                        }
                        if (deviceStatusETV.tasktype == (int)EnmTaskType.Init && deviceStatusETV.isavailable == 1) 
                        {
                            requestAction.telegram = string.Format("13-1-{0}-{2},3-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, 
                                HallStatusTable.devicecode,deviceStatusETV.devicecode);

                            struTelegram struTelegram = new struTelegram();
                            struTelegram.nPLCSendFlag = deviceStatusETV.warehouse;
                            struTelegram.nEquipID = deviceStatusETV.devicecode;     //tv ID号
                            struTelegram.strICCardID = ICCardTable.iccode;
                            struTelegram.strCarSize = CarLocation.carsize;

                            struTelegram.nWheelbase = (int)CarLocation.carwheelbase;
                            struTelegram.overallLg = (int)CarLocation.overallLg;
                            struTelegram.overHang = (int)CarLocation.overhang;

                            struTelegram.strSrcLocAddr = CarLocation.carlocaddr;
                            struTelegram.strDestLocAddr = HallStatusTable.deviceaddr;
                            struTelegram.nEndFlag = 9999;
                            struTelegram.nMasterType = (int)EnmTaskType.ExitTask;

                            requestAction.structTelegram = struTelegram;

                            CarLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                            requestAction.lstTableDto.Add(new struTableDto(CarLocation, 2));

                            requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                            if (1 != (nActionFlag & 1))
                            {
                                nActionFlag += 1;
                            }
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            if (4 != (nActionFlag & 4))
                            {
                                nActionFlag += 4;
                            }
                            return true;
                        }
                    }

                    requestAction.soundName = "19.wav";//已经将您加到取车队列，请排队等候出车19.wav
                   
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();
                    getCarQueue.devicecode = struHallEquip.nHallID;
                    getCarQueue.warehouse = struHallEquip.nWareHouseID;
                    getCarQueue.iccode = ICCardTable.iccode;
                    getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                    getCarQueue.priorityid = ICCardTable.priorityid;
                    getCarQueue.tasktype = (int)EnmTaskType.ExitTask;
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));

                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                   
                    return true;
                    #endregion
                }
            }
            else
            {
                #region 存在该车厅的排队作业,则应让车厅队列里面的车辆先出库，刚当前取车应加入队列排队
                //根据库区号、车厅号和IC卡卡号获取刷卡信息
                CWorkQueueDto SwipeCardInfoTable = lstSwipeQueueOut.Find(qu => qu.iccode == ICCardTable.iccode);

                if (null != SwipeCardInfoTable)
                {
                    requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    return true;
                }

                //加入取车队列
                requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav
                //修改数据库，增加取车刷卡到workqueue
                CWorkQueueDto getCarQueue = new CWorkQueueDto();
                getCarQueue.devicecode = struHallEquip.nHallID;
                getCarQueue.warehouse = struHallEquip.nWareHouseID;
                getCarQueue.iccode = ICCardTable.iccode;
                getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                getCarQueue.priorityid = ICCardTable.priorityid;

                getCarQueue.tasktype = (int)EnmTaskType.ExitTask;
                requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));

                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
                return true;

                #endregion
            }

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
        public override EnmFaultType TmpFetch(CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable)
        {
            if (null == carPOSNTable || null == deviceStatusTBL || string.IsNullOrWhiteSpace(carPOSNTable.iccode))
            {
                return EnmFaultType.Fail;
            }

            int nActionFlag = 0;
            string strICCardID = carPOSNTable.iccode;
            int nWareHouse = deviceStatusTBL.warehouse;
            int nHallID = deviceStatusTBL.devicecode;            
            string strDestLocAddr = deviceStatusTBL.deviceaddr;

            //允许取车：车厅可接受指令 && 车厅没有存取作业 && 取车队列为空
            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nHallID, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);

            if (0 == lstSwipeQueueOut.Count)
            {
                if (CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype))
                {
                    if (deviceStatusTBL.isable==1&& deviceStatusTBL.isavailable == 1)
                    {
                        CDeviceStatusDto deviceStatusETV = this.allocateEtvDevice(carPOSNTable, deviceStatusTBL);
                        if (deviceStatusETV == null)
                        {
                            return EnmFaultType.InvalidEquipID;
                        }

                        CRequestAction requestAction = new CRequestAction();

                        struTelegram struSendTelegram = new struTelegram();
                        struSendTelegram.strICCardID = strICCardID;
                        struSendTelegram.nPLCSendFlag = nWareHouse;
                        struSendTelegram.nEquipID = nHallID;

                        struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;
                        struSendTelegram.strCarSize = carPOSNTable.carsize;
                        struSendTelegram.overallLg = (int)carPOSNTable.overallLg;
                        struSendTelegram.overHang = (int)carPOSNTable.overhang;

                        struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
                        struSendTelegram.strDestLocAddr = strDestLocAddr;
                        struSendTelegram.nEndFlag = 9999;
                        struSendTelegram.nMasterType = (int)EnmTaskType.TmpFetch;
                        requestAction.structTelegram = struSendTelegram;

                        requestAction.telegram = string.Format("2-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, deviceStatusETV.devicecode);

                        EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                        if (EnmFaultType.Success != enmResult)
                        {
                            return enmResult;
                        }
                        requestAction.PLCServer = m_commModule;
                        //修改数据库中设备作业状态
                        deviceStatusTBL.iccode = strICCardID;
                        deviceStatusTBL.tasktype = (int)EnmTaskType.TmpFetch;
                        deviceStatusTBL.prevnode = null;
                        deviceStatusTBL.currentnode = null;
                        deviceStatusTBL.scrloc = carPOSNTable.carlocaddr;
                        deviceStatusTBL.desccloc = deviceStatusTBL.deviceaddr;
                        requestAction.lstTableDto.Add(new struTableDto(deviceStatusTBL, 2));

                        carPOSNTable.carlocstatus = (int)EnmLocationStatus.TmpFetch;
                        requestAction.lstTableDto.Add(new struTableDto(carPOSNTable, 2));

                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }
                        if (4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //执行
                        requestAction.RequestAction(nActionFlag);

                        return EnmFaultType.Success;
                    }
                }
            }

            CRequestAction requestAct = new CRequestAction();
            //增加取车刷卡到workqueue
            CWorkQueueDto getCarQueue = new CWorkQueueDto();            
            getCarQueue.devicecode = nHallID;
            getCarQueue.warehouse = nWareHouse;
            getCarQueue.iccode = strICCardID;
            getCarQueue.waitsendtelegram = null;
            getCarQueue.tasktype = (int)EnmTaskType.TmpFetch;
            getCarQueue.swipecount = (int)EnmSwipeCount.TmpFetch;

            if (4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            requestAct.lstTableDto.Add(new struTableDto(getCarQueue, 1));

            requestAct.RequestAction(nActionFlag);

            return EnmFaultType.Wait;
        }

        /// <summary>
        /// 车辆出库（临时卡缴费出车、手动出库）
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
        /// <param name="strICCardID">IC卡号</param>
        /// <param name="deviceStatusTBL">车厅</param>
        /// <param name="carPOSNTable">车位</param>
        /// <param name="bApp">APP手机取车标志 true：手机取车；false：否</param>
        /// <returns></returns>
        public override EnmFaultType VehicleExit(string strICCardID, CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable, bool bApp)
        {
            if (null == carPOSNTable || null == deviceStatusTBL)
            {
                return EnmFaultType.Fail;
            }

            int nActionFlag = 0;
            int nWareHouse = deviceStatusTBL.warehouse;
            int nHallID = deviceStatusTBL.devicecode;
            string strDestLocAddr = deviceStatusTBL.deviceaddr;

            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetWorkQueueByHallCode(nWareHouse, nHallID);
            if (lstSwipeQueueOut.Count > CConfigManagement.myPara.CarExitQueueMaxCount)
            {
                return EnmFaultType.NotAvailable;
            }

            if (0 == lstSwipeQueueOut.Count)
            {
                if (CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype))
                {
                    if (deviceStatusTBL.isavailable == 1)
                    {
                        CDeviceStatusDto deviceStatusEtv = this.allocateEtvDevice(carPOSNTable, deviceStatusTBL);
                        if (deviceStatusEtv == null)
                        {
                            return EnmFaultType.InvalidEquipID;
                        }

                        EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, deviceStatusTBL.warehouse);
                        if (EnmFaultType.Success != enmResult)
                        {
                            return enmResult;
                        }
                        #region
                        CRequestAction requestAction = new CRequestAction();
                        requestAction.PLCServer = m_commModule;

                        struTelegram struSendTelegram = new struTelegram();
                        struSendTelegram.strICCardID = strICCardID;
                        struSendTelegram.nPLCSendFlag = nWareHouse;
                        struSendTelegram.nEquipID = nHallID;

                        struSendTelegram.strCarSize = carPOSNTable.carsize;
                        struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;
                        struSendTelegram.overallLg = (int)carPOSNTable.overallLg;
                        struSendTelegram.overHang = (int)carPOSNTable.overhang;

                        struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
                        struSendTelegram.strDestLocAddr = strDestLocAddr;
                        struSendTelegram.nEndFlag = 9999;
                        struSendTelegram.nMasterType = (int)EnmTaskType.ExitTask;
                        requestAction.structTelegram = struSendTelegram;

                        requestAction.telegram = string.Format("3-1,13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, deviceStatusEtv.devicecode);

                        //修改数据库中设备作业状态
                        deviceStatusTBL.iccode = strICCardID;
                        deviceStatusTBL.tasktype = (int)EnmTaskType.ExitTask;
                        deviceStatusTBL.prevnode = null;
                        deviceStatusTBL.currentnode = null;
                        deviceStatusTBL.scrloc = carPOSNTable.carlocaddr;
                        deviceStatusTBL.desccloc = deviceStatusTBL.deviceaddr;
                        requestAction.lstTableDto.Add(new struTableDto(deviceStatusTBL, 2));

                        carPOSNTable.carlocstatus = (int)EnmLocationStatus.Outing;
                        requestAction.lstTableDto.Add(new struTableDto(carPOSNTable, 2));

                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }
                        if (4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //执行
                        requestAction.RequestAction(nActionFlag);

                        #endregion
                        return EnmFaultType.Success;
                    }
                }
            }

            //修改数据库，增加取车刷卡到workqueue
            CWorkQueueDto getCarQueue = new CWorkQueueDto();
            CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);
            if (null != ICCardTable && 0 < ICCardTable.priorityid)
            {
                if (0 < getCarQueue.priorityid)
                {
                    getCarQueue.priorityid = Math.Min((int)getCarQueue.priorityid, (int)ICCardTable.priorityid);
                }
                else
                {
                    getCarQueue.priorityid = ICCardTable.priorityid;
                }
            }

            getCarQueue.devicecode = nHallID;
            getCarQueue.warehouse = nWareHouse;
            getCarQueue.iccode = strICCardID;
            getCarQueue.waitsendtelegram = null;
            getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
            getCarQueue.tasktype = (int)EnmTaskType.ExitTask;

            CRequestAction requestAct = new CRequestAction();
            requestAct.lstTableDto.Add(new struTableDto(getCarQueue, 1)); //插入队列
            nActionFlag = 4;
            requestAct.RequestAction(nActionFlag);

            return EnmFaultType.Wait;
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
        public override EnmFaultType VehicleMove(CCarLocationDto carLocationTBLSrc, CCarLocationDto carLocationTBLDest)
        {
            #region
            int nWareHouse = (int)carLocationTBLSrc.warehouse;
            CDeviceStatusDto deviceStatus = this.allocateEtvForTrans(carLocationTBLSrc, carLocationTBLDest);
            if (deviceStatus==null)
            {
                return EnmFaultType.NotFoundEquip;//没有空闲设备，分配设备失败
            }
            EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
            if (EnmFaultType.Success != enmResult)
            {
                return enmResult;
            }

            CRequestAction requestAction = new CRequestAction();
            requestAction.PLCServer = m_commModule;

            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.strICCardID = carLocationTBLSrc.iccode;
            struSendTelegram.nPLCSendFlag = nWareHouse;
            struSendTelegram.nEquipID = deviceStatus.devicecode;

            struSendTelegram.overallLg = (int)carLocationTBLSrc.overallLg;
            struSendTelegram.overHang = (int)carLocationTBLSrc.overhang;
            struSendTelegram.nWheelbase = (int)carLocationTBLSrc.carwheelbase;            
            struSendTelegram.strCarSize = carLocationTBLSrc.carsize;

            struSendTelegram.strSrcLocAddr = carLocationTBLSrc.carlocaddr;
            struSendTelegram.strDestLocAddr = carLocationTBLDest.carlocaddr;
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            struSendTelegram.nMasterType = (int)EnmTaskType.MoveCarTask;
            requestAction.structTelegram = struSendTelegram;

            requestAction.telegram = string.Format("13-1-{0}", CConfigManagement.myPara.EquipIsAviliableID); 

            //（13,1）发送挪移装载报文时
            carLocationTBLSrc.carlocstatus = (int)EnmLocationStatus.MovingVEH;
            requestAction.lstTableDto.Add(new struTableDto(carLocationTBLSrc, 2));
            
            carLocationTBLDest.iccode = carLocationTBLSrc.iccode;
            carLocationTBLDest.carlocstatus = (int)EnmLocationStatus.MovingVEH;
            requestAction.lstTableDto.Add(new struTableDto(carLocationTBLDest, 2));

            int nActionFlag = 0;
            if (1!= (nActionFlag & 1))
            {
                nActionFlag += 1;
            }
            if (4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }           
            requestAction.RequestAction(nActionFlag);

            return EnmFaultType.Success;
            #endregion
        }

        /// <summary>
        /// 处理MURO继续时，进行卸载操作--(14,1)
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <returns></returns>
        public override int DealMuroUnloadCmd(CDeviceStatusDto etv, CCarLocationDto car)
        {
            int warehouse = etv.warehouse;
            EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, warehouse);
            if (EnmFaultType.Success != enmResult)
            {
                return 301;
            }
            CRequestAction requestAction = new CRequestAction();
            requestAction.PLCServer = m_commModule;

            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.nPLCSendFlag = warehouse;
            struSendTelegram.strICCardID = etv.iccode;           
            struSendTelegram.nEquipID = etv.devicecode;

            struSendTelegram.overallLg = (int)car.overallLg;
            struSendTelegram.overHang = (int)car.overhang;
            struSendTelegram.nWheelbase = (int)car.carwheelbase;
            struSendTelegram.strCarSize = car.carsize;

            struSendTelegram.strSrcLocAddr = etv.scrloc;
            struSendTelegram.strDestLocAddr = etv.desccloc;
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            struSendTelegram.nMasterType = (int)etv.tasktype;
            requestAction.structTelegram = struSendTelegram;

            requestAction.telegram = string.Format("14-1-{0}", CConfigManagement.myPara.EquipIsAviliableID);

            etv.currentnode = (int)EnmFlowNodeDescp.TMURORecoryHasCar;
            requestAction.lstTableDto.Add(new struTableDto(etv, 2));
            
            int nActionFlag =5;          
            requestAction.RequestAction(nActionFlag);

            return 100;
        }

        /// <summary>
        /// 处理MURO继续时，进行装载操作-(13,1)
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <param name="isHall"></param>
        /// <returns></returns>
        public override int DealMUROLoadCmd(CDeviceStatusDto etv, CCarLocationDto car,bool isHall) 
        {
            int warehouse = etv.warehouse;
            EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, warehouse);
            if (EnmFaultType.Success != enmResult)
            {
                return 301;
            }
            if (etv.prevnode == (int)EnmFlowNodeDescp.WillUnLoad ||
                etv.prevnode == (int)EnmFlowNodeDescp.UnLoad)
            {
                //已经完成装载过的，就不可能再进行装载作业了
                return 302;
            }

            CRequestAction requestAction = new CRequestAction();
            requestAction.PLCServer = m_commModule;

            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.nPLCSendFlag = warehouse;
            struSendTelegram.strICCardID = etv.iccode;          
            struSendTelegram.nEquipID = etv.devicecode;

            if (isHall)
            {
                struSendTelegram.offCenter = (int)car.offcenter;
                struSendTelegram.rearWheelDis = (int)car.rearwheeldis;
            }
            struSendTelegram.overallLg = car.overallLg==null?0:(int)car.overallLg;
            struSendTelegram.overHang = car.overhang==null?0:(int)car.overhang;
            struSendTelegram.nWheelbase = car.carwheelbase==null?0:(int)car.carwheelbase;
            struSendTelegram.strCarSize = car.carsize;

            struSendTelegram.strSrcLocAddr = etv.scrloc;
            struSendTelegram.strDestLocAddr = etv.desccloc;
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            struSendTelegram.nMasterType = (int)etv.tasktype;
            requestAction.structTelegram = struSendTelegram;

            requestAction.telegram = string.Format("13-1", CConfigManagement.myPara.EquipIsAviliableID);

            etv.currentnode = (int)EnmFlowNodeDescp.TMURORecoryNoCar;
            requestAction.lstTableDto.Add(new struTableDto(etv, 2));

            int nActionFlag = 5;
            requestAction.RequestAction(nActionFlag);

            return 100;
        }


        #endregion

        #region 分配算法algorithm
        /// <summary>
        /// 分配车位算法(平面移动)_优先空闲TV层车位
        /// </summary>
        /// <param name="struTelegram"></param>
        /// <param name="strError">卡异常提示</param>
        /// <returns></returns>
        private CCarLocationDto AllocateCarLocation(struTelegram struTelegram, ref string strError,out int eqpID)
        {
            eqpID = 0;
            try
            {
                CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.strICCardID);
                if (null != carLocation)
                {
                    if (struTelegram.nPLCSendFlag != carLocation.warehouse)
                    {
                        strError = "16.wav";// //您的车辆停在其他库区16.wav
                        return null;
                    }
                    return carLocation;
                }
                CICCardDto icCard = CCommonMethods.myMethods.GetICCard(struTelegram.strICCardID);
                if (null == icCard)
                {
                    return null;
                }
                int nWareHouse = struTelegram.nPLCSendFlag;
                string nCarSize =struTelegram.strCarSize;
                int hallID=struTelegram.nEquipID;

                //如果是临时卡的话，为定期卡车主预留车位
                if (icCard.ictype == (int)EnmICCardType.Temp || icCard.ictype == (int)EnmICCardType.Fixed)
                {
                    carLocation =this.allocateLocation(nWareHouse,nCarSize,hallID,out eqpID);
                }             
                else if (icCard.ictype == (int)EnmICCardType.FixedLocation)
                {
                    CDeviceStatusDto hall=CCommonMethods.myMethods.GetDeviceStatus(nWareHouse,hallID);
                    if (hall == null) 
                    {
                        CLOGException.Trace(nWareHouse, "AllocateCarLocation 异常", "找不到车厅,设备号:" + hallID);
                        return null;
                    }

                    carLocation = CCommonMethods.myMethods.GetCarLocation(nWareHouse, icCard.carlocaddr);
                    if (carLocation != null && carLocation.carlocstatus == (int)EnmLocationStatus.Space)
                    {
                        CDeviceStatusDto eqp = this.allocateEtvDevice(carLocation, hall);
                        if (eqp != null)
                        {
                            eqpID = eqp.devicecode;
                        }
                    }                 
                }
                else
                {
                    strError = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                    CLOGException.Trace(struTelegram.nPLCSendFlag, "CPlaneShiftingTaskTrigger.ALLOCCarLocation ", "该卡异常，请联系管理人员处理32.wav，ICCard ID :" + struTelegram.strICCardID);
                }                
                return carLocation;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("CPlaneShiftingTaskTrigger.ALLOCCarLocation 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return null;
            }
        }

        /// <summary>
        /// 分配车厅设备算法——根据车厅排队最少分配
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        public override int ALLOCHallByQueue(int nWareHouse,int region,CCarLocationDto toLctn)
        {
            List<CDeviceStatusDto> etvsLst = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);
            List<CDeviceStatusDto> layerEtvs = etvsLst.FindAll(tt => tt.devicelayer == toLctn.carloclayer);
            List<CDeviceStatusDto> availEtvs = layerEtvs.FindAll(tt => tt.devicemode == (int)EnmModel.Automatic && tt.isable == 1);
            
            List<CDeviceStatusDto> hallsLst = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.Hall);
            var query = from hall in hallsLst
                        where hall.devicemode == (int)EnmModel.Automatic &&
                              (hall.halltype==(int)EnmHallType.EnterOrExit||hall.halltype==(int)EnmHallType.Exit)&&
                              hall.isable==1
                        orderby Math.Abs((int)hall.region - region), hall.devicecode descending
                        select hall;
            List<CDeviceStatusDto> ableHalls = query.ToList();
            if (ableHalls.Count == 0)
            {
                return 0;
            }

            if (availEtvs.Count == 2)
            {
                #region
                if (region == 2)
                {
                    if (toLctn.carloccolumn < 4) 
                    {
                        List<CDeviceStatusDto> side1Halls = ableHalls.FindAll(hh => hh.region != region);
                        if (side1Halls==null|| side1Halls.Count == 0) 
                        {
                            return 0;
                        }
                        return side1Halls[0].devicecode;
                    }
                    return ableHalls[0].devicecode;
                }
                else //region==1
                {
                    List<CDeviceStatusDto> side1Halls = ableHalls.FindAll(hh => hh.region == region);
                    if (side1Halls == null || side1Halls.Count == 0) 
                    {
                        return 0;
                    }
                    else if (side1Halls.Count == 1)
                    {
                        CDeviceStatusDto hallDev=side1Halls[0];
                        if (hallDev.devicecode == 11)
                        {
                            if (toLctn.carloccolumn > 37)
                            {
                                CDeviceStatusDto side2hall = ableHalls.Find(hh => hh.region != region);
                                if (side2hall != null)
                                {
                                    return side2hall.devicecode;
                                }
                                else 
                                {
                                    return 0;
                                }
                            }
                        }
                        return side1Halls[0].devicecode;
                    }
                    else
                    {
                        if (toLctn.carloccolumn > 37)
                        {
                            CDeviceStatusDto hll = side1Halls.Find(hh => hh.devicecode == 12);
                            if (hll == null) 
                            {
                                return 0;
                            }
                            return hll.devicecode;
                        }

                        List<CWorkQueueDto> wks = CCommonMethods.myMethods.GetHallsWorkQueueHasTelegram(nWareHouse);
                        if (wks.Count == 0)
                        {
                            //找出空闲的hall
                            CDeviceStatusDto dev = side1Halls.Find(h => h.tasktype == (int)EnmTaskType.Init);
                            if (dev != null)
                            {
                                return dev.devicecode;
                            }
                            else 
                            {
                                //两个车厅都有作业时，优先作业类型为取车的车厅
                                List<CDeviceStatusDto> devLists = side1Halls.FindAll(h => h.tasktype != (int)EnmTaskType.EntryTask);
                                if (devLists == null || devLists.Count == 0)
                                {
                                    return side1Halls[0].devicecode;
                                }
                                else
                                {
                                    return devLists[0].devicecode;
                                }                               
                            }
                        }
                        else
                        {
                            Dictionary<CDeviceStatusDto, int> dictask = new Dictionary<CDeviceStatusDto, int>();
                            foreach (CDeviceStatusDto dv in side1Halls)
                            {                               
                                List<CWorkQueueDto> wd = wks.Where(tt => tt.warehouse == dv.warehouse && tt.devicecode == dv.devicecode).ToList();
                                if (wd == null)
                                {
                                    dictask.Add(dv, 0);
                                }
                                else
                                {
                                    dictask.Add(dv, wd.Count);
                                }
                            }
                            Dictionary<CDeviceStatusDto, int> dicAsc = dictask.OrderBy(d => d.Value).ToDictionary(o => o.Key, p => p.Value);
                            CDeviceStatusDto smg = dicAsc.FirstOrDefault().Key;
                            if (smg != null)
                            {
                                return smg.devicecode;
                            }
                        }
                    }
                }               
                #endregion
            }
            else if (availEtvs.Count == 1)
            {
                #region
                TvServiceScope svscope = new TvServiceScope(layerEtvs);
                CScope scope = svscope.GetTvScope(availEtvs[0]);
                if (toLctn.carloccolumn >= scope.LeftCol && toLctn.carloccolumn <= scope.RightCol) 
                {
                    foreach (CDeviceStatusDto hallDev in ableHalls) 
                    {
                        int hallColumn=Convert.ToInt32( hallDev.deviceaddr.Substring(1,2));
                        if (scope.LeftCol <= hallColumn && hallColumn <= scope.RightCol) 
                        {
                            return hallDev.devicecode;
                        }
                    }
                }
                #endregion
            }          
            return 0;
        }

        /// <summary>
        /// 检查指定的车厅是否有可用的ETV
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <returns></returns>
        public override bool CheckETVStat(int nWareHouse, int nHallID)
        {
            bool bReturn = false;
            List<CDeviceStatusDto> lstETVStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);
            foreach (CDeviceStatusDto table in lstETVStatusTBL)
            {
                if (1 == table.isable)
                {
                    bReturn = true;
                }
            }

            if (null == lstETVStatusTBL || 1 > lstETVStatusTBL.Count)
            {
                // 无ETV设备时
                return true;
            }
            return bReturn;
        }
       
        /// <summary>
        /// 分配ETV设备算法,返回设备号，分配失败返回：0(平面移动)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        private int ALLOCETVOrTV(int nWareHouse,int layer)
        {
            return 0;
        }

        /// <summary>
        /// 获取车位信息具体实例（分配临时卡车位 根据除定期卡和固定车位卡预留车位之外的空余车位数->临时卡车位数按照优先级分配）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarSize"></param>
        /// <param name="nLayer">-1：非平面移动车库；0：平面移动车库</param>
        /// <returns></returns>
        private CCarLocationDto AllotLocationOfTemp(int nWareHouse, int nCarSize)
        {
            int nLayer = 0;
            List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
            lstCarLocationTBL = CCommonMethods.myMethods.GetCarLocationList();

            var query = from lct in lstCarLocationTBL
                        where lct.warehouse == nWareHouse &&
                        lct.carlocstatus == (int)EnmLocationStatus.Space &&
                        lct.carloctype == (int)EnmLocationType.Normal &&
                        CCommonMethods.myMethods.GetICCard((int)lct.warehouse, lct.carlocaddr) == null
                        orderby lct.priorityid
                        select lct;
            List<CCarLocationDto> locationList = query.ToList();
            // 车厅作为车位处理情况20160309
            foreach (CCarLocationDto lct in locationList)
            {
                CDeviceStatusDto table = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, lct.carlocaddr, EnmSMGType.Hall);
                if (null != table && ((int)EnmHallType.Exit != table.halltype || 0 == table.isavailable))
                {
                    locationList.Remove(lct);
                }
            }

            List<CICCardDto> lstICCardTBL = new List<CICCardDto>();
            lstICCardTBL = CCommonMethods.myMethods.GetICCardList();
            // 当前库正常定期卡列表
            List<CICCardDto> lstFixCard = lstICCardTBL.FindAll(s => s.ictype == (int)EnmICCardType.Fixed
                                                            && s.icstatus == (int)EnmICCardStatus.Normal);

            // 剔除为未存车的定期卡预留的车位
            for (int i = 0; i < lstFixCard.Count; i++)
            {
                CICCardDto card = lstFixCard[i];

                if (!CBaseMethods.MyBase.IsUIntNumWithoutZero(card.carlocaddr) || null != CCommonMethods.myMethods.GetCarLocation(card.iccode))
                {// 无效预留尺寸或者该定期卡已存车
                    lstFixCard.Remove(card);
                    i--;
                    continue;
                }

                var query1 = from lct in locationList
                             orderby lct.carlocsize
                             select lct;// 根据车位尺寸carlocsize排序
                foreach (CCarLocationDto lct in query1.ToList())
                {
                    if (CBaseMethods.MyBase.CheckCarSize(card.carlocaddr, lct.carlocsize))
                    {
                        locationList.Remove(lct);
                        lstFixCard.Remove(card);
                        i--;
                        break;
                    }
                }
            }

            // 平面移动类型库时，判断有无空闲TV，有分配空闲TV那一层的车位
            if (0 <= nLayer)
            {
                List<CDeviceStatusDto> lstDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);

                foreach (CDeviceStatusDto deviceDtatusTBL in lstDeviceStatusTBL)
                {
                    if (1 == deviceDtatusTBL.isable && 1 == deviceDtatusTBL.isavailable && (int)EnmModel.Automatic == deviceDtatusTBL.devicemode
                        && (int)EnmTaskType.Init == deviceDtatusTBL.tasktype)
                    {
                        if (!CBaseMethods.MyBase.IsEmpty(deviceDtatusTBL.devicelayer))
                        {
                            nLayer = (int)deviceDtatusTBL.devicelayer;
                        }
                        break;
                    }
                }
            }
            if (0 < nLayer)
            {  // 找到等于该车辆尺寸的车位且空闲TV那一层的车位
                foreach (CCarLocationDto lct in locationList)
                {
                    if (lct.carlocsize == nCarSize.ToString() && lct.carloclayer == nLayer)
                    {
                        return lct;
                    }
                }
            }

            // 找到等于该车辆尺寸的车位
            CCarLocationDto carLct = locationList.Find(s => s.carlocsize == nCarSize.ToString());

            if (null != carLct)
            {
                return carLct;
            }

            if (0 < nLayer && 0 < locationList.Count)
            {
                // 找到空闲TV那一层的车位
                foreach (CCarLocationDto lct in locationList)
                {
                    if (CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), lct.carlocsize) && lct.carloclayer == nLayer)
                    {
                        return lct;
                    }
                }
            }

            if (0 < locationList.Count)
            {
                return locationList.Find(s => CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), s.carlocsize));
            }

            return null;
        }

        /// <summary>
        /// 临时卡、定期卡车位分配
        ///   1、优先分配空闲的ETV
        ///   2、再分配当前层作业少的
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="checkCode"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        private CCarLocationDto allocateLocation(int warehouse, string checkCode, int hallID,out int eqpID) 
        {
            eqpID = 0;
            #region
            List<CCarLocationDto> allLocations = CCommonMethods.myMethods.GetCarLocationList(warehouse);
            List<CDeviceStatusDto> SMGs = CCommonMethods.myMethods.GetDeviceStatusList();
            CDeviceStatusDto hall = SMGs.Find(h => h.warehouse == warehouse && h.devicecode == hallID);

            List<CDeviceStatusDto> availEtvs = new List<CDeviceStatusDto>();
            var ava = from tt in SMGs
                      where tt.devicemode == (int)EnmModel.Automatic &&
                          tt.isable == 1 &&
                          tt.devicetype == (int)EnmSMGType.ETV
                      orderby tt.devicelayer descending
                      select tt;
            availEtvs.AddRange(ava.ToList());                
         
            //TV4-TV6, hall1-hall2为区域1;  TV1-TV3, hall3为区域2
            List<CDeviceStatusDto> NearTvs = new List<CDeviceStatusDto>();
            var near=from tt in SMGs
                     where tt.region == hall.region && 
                           tt.devicetype == (int)EnmSMGType.ETV
                     orderby tt.devicelayer descending
                     select tt;
            NearTvs.AddRange(near.ToList());

            List<CDeviceStatusDto> FarTvs = new List<CDeviceStatusDto>();
            var far = from tt in SMGs
                       where tt.region != hall.region && 
                             tt.devicetype == (int)EnmSMGType.ETV
                       orderby tt.devicelayer descending
                       select tt;
            FarTvs.AddRange(far.ToList());           
            //空闲的TV集合
            List<CDeviceStatusDto> NearFreeTvs = NearTvs.FindAll(p => p.devicemode == (int)EnmModel.Automatic &&
                                                                p.isable == 1 && p.tasktype == (int)EnmTaskType.Init);
            //暂不用
            List<CDeviceStatusDto> FarFreeTvs = FarTvs.FindAll(p => p.devicemode == (int)EnmModel.Automatic &&
                                                                p.isable == 1 && p.tasktype == (int)EnmTaskType.Init);

            List<CDeviceStatusDto> otherLst = new List<CDeviceStatusDto>();
            #region 如果是2#车厅存车，当同侧的TV被禁用时，考虑另一侧的TV是否可以加入
            if (hall.devicecode == 12)
            {
                foreach (CDeviceStatusDto tt in NearTvs)
                {
                    if (tt.devicemode != (int)EnmModel.Automatic || tt.isable == 0)
                    {
                        if (CBaseMethods.MyBase.GetColumnByAddrs(tt.deviceaddr) == 1) //不可用的TV处于1列时
                        {
                            //同侧的TV不可用，则看另一侧的TV是否可以加入
                            CDeviceStatusDto device = FarTvs.Find(tv => tv.devicelayer == tt.devicelayer &&
                                                                        tv.devicemode == (int)EnmModel.Automatic &&
                                                                        tv.isable == 1);
                            if (device != null)
                            {
                                otherLst.Add(device);
                            }
                        }
                    }                  
                }
            }
            #endregion

            Dictionary<int, int> dicLctnTaskOrderBy = new Dictionary<int, int>();
            #region 查询层中作业的数量
            Dictionary<int, int> locTasksDic = new Dictionary<int, int>();
            List<CWorkQueueDto> lstWork=CCommonMethods.myMethods.TakeCarQueueNoTelegram(warehouse);
            List<CCarLocationDto> lstLctns=new List<CCarLocationDto>();
            foreach (CWorkQueueDto dto in lstWork)
            {
                CCarLocationDto ccar = CCommonMethods.myMethods.GetCarLocation(dto.iccode);
                if (ccar != null) 
                {
                    lstLctns.Add(ccar);
                }
            }
            for (int i = 3; i >0; i--)
            {
                List<CCarLocationDto> lstLC = new List<CCarLocationDto>();
                if (lstLctns.Count > 0)
                {
                    lstLC = lstLctns.FindAll(ll => ll.carloclayer == i);
                }
                var lctnTask = from lc in allLocations
                               where lc.carloclayer == i &&
                                    (lc.carlocstatus == (int)EnmLocationStatus.Entering ||
                                     lc.carlocstatus == (int)EnmLocationStatus.Outing)
                               select lc;
                lstLC.AddRange(lctnTask.ToList());
                locTasksDic.Add(i, lstLC.Count);
            }          
            dicLctnTaskOrderBy = locTasksDic.OrderBy(r => r.Value).ToDictionary(d => d.Key, t => t.Value);
            #endregion

            Dictionary<int, int> dicLctnOppOrderBy = new Dictionary<int, int>();
            #region 查询层中车位占位的数量 
            Dictionary<int, int> dicLctnOppNum = new Dictionary<int, int>();
            for (int i = 3; i >0; i--)
            {
                List<CCarLocationDto> hasOppLcts = allLocations.FindAll(delegate(CCarLocationDto cltndto)
                {
                    return cltndto.carloclayer == i &&
                       cltndto.carloctype == (int)EnmLocationType.Normal &&
                       (cltndto.carlocstatus == (int)EnmLocationStatus.Entering ||
                        cltndto.carlocstatus == (int)EnmLocationStatus.Outing ||
                        cltndto.carlocstatus == (int)EnmLocationStatus.Occupy
                        );
                });
                int count = 0;
                if (hasOppLcts != null)
                {
                    count = hasOppLcts.Count;
                }
                dicLctnOppNum.Add(i, count);  //加入层号、当前层作业数量
            }
            dicLctnOppOrderBy = dicLctnOppNum.OrderBy(r => r.Value).ToDictionary(d => d.Key, t => t.Value);
            #endregion

            List<CCarLocationDto> middleLocations = new List<CCarLocationDto>();
            #region 去除两头的车位，优先分配中间区域的
            var loc = from lc in allLocations
                      where lc.carloctype == (int)EnmLocationType.Normal &&
                            lc.carlocstatus == (int)EnmLocationStatus.Space &&
                            CCommonMethods.myMethods.GetICCard((int)lc.warehouse, lc.carlocaddr) == null &&
                            (lc.carloccolumn > 6 && lc.carloccolumn < 37)
                      select lc;
            middleLocations.AddRange(loc.ToList());
            #endregion

            //车位外形合适的,与车厅列差进行排序
            List<CCarLocationDto> suitable =new List<CCarLocationDto>();
            #region
            //优先分配层中车位占用少的层,以层进行排序
            int layer = 3;
            if (checkCode == "111")
            {
                foreach (KeyValuePair<int, int> pair in dicLctnOppOrderBy) 
                {
                    if (pair.Key == 1 || pair.Key == 2) 
                    {
                        layer = pair.Key;
                        break;
                    }
                }
            }
            var lctns = from lc in middleLocations
                        where string.Compare(lc.carlocsize, checkCode) == 0
                        orderby Math.Abs((int)lc.carloccolumn - CBaseMethods.MyBase.GetColumnByAddrs(hall.deviceaddr)) ascending,
                                lc.carloclayer - layer ascending
                        select lc;
            #endregion
            suitable.AddRange(lctns.ToList()); 

            #region 依空闲的TV，查找可用的车位
            #region 依同侧空闲的TV，查找可用车位
            if (NearFreeTvs.Count > 0) 
            {
                foreach (KeyValuePair<int, int> pair in dicLctnOppOrderBy)
                {
                    foreach (CDeviceStatusDto smg in NearFreeTvs)
                    {
                        if (pair.Key == smg.devicelayer)
                        {
                            #region
                            foreach (CCarLocationDto car in suitable)
                            {
                                if (car.carloclayer == smg.devicelayer)
                                {
                                    CScope scope = this.getTVscope(smg);
                                    if (scope != null)
                                    {
                                        if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                        {
                                            eqpID = smg.devicecode;
                                            return car;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion
            #region 依可加入的不同侧的、空闲的TV，查找可用车位
            foreach (CDeviceStatusDto ot in otherLst) 
            {
                if (ot.tasktype == (int)EnmTaskType.Init) 
                {
                    CScope scope = this.getTVscope(ot);
                    if (scope != null)
                    {
                        int hallColmn = Convert.ToInt32(hall.deviceaddr.Substring(1, 2));

                        if (hallColmn < scope.LeftCol || hallColmn > scope.RightCol)
                        {
                            continue;
                        }
                        foreach (CCarLocationDto car in suitable)
                        {
                            if (car.carloclayer == ot.devicelayer)
                            {
                                if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                {
                                    eqpID = ot.devicecode;
                                    return car;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #endregion
            #region 依层数作业的多少，优先分配层数少的，且要判断TV是不是可用的
            foreach (KeyValuePair<int, int> pair in dicLctnTaskOrderBy)
            {
                foreach (CCarLocationDto car in suitable)
                {
                    if (pair.Key == car.carloclayer)
                    {
                        #region 2016-10-28 add
                        //找出当前车位所在层的TV车                       
                        if (hall.devicecode != 12)
                        {
                            //如果是1\3号车厅，则选择各自区域的TV
                            CDeviceStatusDto dev = availEtvs.Find(ly => ly.region == hall.region && ly.devicelayer == pair.Key);
                            if (dev != null)
                            {
                                CScope scope = this.getTVscope(dev);
                                if (scope != null)
                                {
                                    if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                    {
                                        eqpID = dev.devicecode;
                                        return car;
                                    }
                                }
                            }
                        }
                        else //如果是2#厅存车
                        {
                            //获取TV1
                            List<CDeviceStatusDto> tvOnLayer = availEtvs.FindAll(tt=>tt.devicelayer==pair.Key);
                            if (tvOnLayer.Count == 1)
                            {
                                int hallColumn = Convert.ToInt32(hall.deviceaddr.Substring(1, 2));
                                CScope scope = this.getTVscope(tvOnLayer[0]);
                                if (scope != null)
                                {
                                    if (hallColumn < scope.LeftCol || hallColumn > scope.RightCol)
                                    {
                                        continue;
                                    }
                                    #region

                                    //建立作业范围，判断是否可达                                  
                                    int lctColumn = (int)car.carloccolumn;
                                    if (scope.LeftCol <= lctColumn && scope.RightCol >= lctColumn)
                                    {
                                        eqpID = tvOnLayer[0].devicecode;
                                        return car;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    #endregion
                                }
                            }
                            else if (tvOnLayer.Count == 2)//当前层中存在二个可用的TV
                            {
                                #region
                                CDeviceStatusDto nearDev = tvOnLayer.Find(cc => cc.region == hall.region);
                                CDeviceStatusDto farDev = tvOnLayer.Find(cc => cc.region != hall.region);
                                if (nearDev != null && farDev != null)
                                {
                                    if (nearDev.tasktype == (int)EnmTaskType.EntryTask)
                                    {
                                        if (nearDev.currentnode == (int)EnmFlowNodeDescp.Load)
                                        {
                                            if (farDev.tasktype == (int)EnmTaskType.Init)
                                            {
                                                eqpID = farDev.devicecode;
                                                return car;
                                            }
                                            else
                                            {
                                                eqpID = nearDev.devicecode;
                                                return car;
                                            }
                                        }
                                        else
                                        {
                                            eqpID = nearDev.devicecode;
                                            return car;
                                        }
                                    }
                                    else if (nearDev.tasktype == (int)EnmTaskType.ExitTask)
                                    {
                                        if (nearDev.currentnode != (int)EnmFlowNodeDescp.Load)
                                        {
                                            if (farDev.tasktype == (int)EnmTaskType.Init)
                                            {
                                                eqpID = farDev.devicecode;
                                                return car;
                                            }
                                            else
                                            {
                                                eqpID = nearDev.devicecode;
                                                return car;
                                            }
                                        }
                                        else
                                        {
                                            eqpID = nearDev.devicecode;
                                            return car;
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                break;
                            }
                        }
                        #endregion

                        #region
                        foreach (CDeviceStatusDto smg in NearTvs)
                        {
                            if (smg.devicemode == (int)EnmModel.Automatic &&
                                smg.isable == 1 &&
                                smg.devicelayer == pair.Key)
                            {
                                CScope scope = this.getTVscope(smg);
                                if (scope == null)
                                {
                                    eqpID = smg.devicecode;
                                    return car;
                                }
                                else
                                {
                                    if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                    {
                                        eqpID = smg.devicecode;
                                        return car;
                                    }
                                }
                            }
                        }
                        #endregion
                        #region
                        foreach (CDeviceStatusDto smg in otherLst)
                        {
                            if (smg.devicelayer == pair.Key)
                            {
                                CScope scope = this.getTVscope(smg);
                                if (scope == null)
                                {
                                    eqpID = smg.devicecode;
                                    return car;
                                }
                                else
                                {
                                    if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                    {
                                        eqpID = smg.devicecode;
                                        return car;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }          
            #endregion

            List<CCarLocationDto> sideLocations = new List<CCarLocationDto>();
            #region 两头的车位这里加入
            var locSide = from lc in allLocations
                          where lc.carloctype == (int)EnmLocationType.Normal &&
                                lc.carlocstatus == (int)EnmLocationStatus.Space &&
                                CCommonMethods.myMethods.GetICCard((int)lc.warehouse, lc.carlocaddr) == null &&
                                (lc.carloccolumn < 7 || lc.carloccolumn > 36) &&
                                string.Compare(lc.carlocsize, checkCode) == 0
                          orderby Math.Abs((int)lc.carloccolumn - CBaseMethods.MyBase.GetColumnByAddrs(hall.deviceaddr)) ascending,
                                  lc.carloclayer ascending
                          select lc;
            sideLocations.AddRange(locSide.ToList());
            #endregion
            #region 依空闲的TV，查找可用的车位
            #region 依同侧空闲的TV，查找可用车位
            if (NearFreeTvs.Count > 0)
            {
                foreach (CDeviceStatusDto smg in NearFreeTvs)
                {
                    foreach (CCarLocationDto car in sideLocations)
                    {
                        if (car.carloclayer == smg.devicelayer)
                        {
                            CScope scope = this.getTVscope(smg);
                            if (scope != null)
                            {                               
                                if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                {
                                    eqpID = smg.devicecode;
                                    return car;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region 依可加入的不同侧的TV，先依空闲的TV，查找可用车位
            foreach (CDeviceStatusDto ot in otherLst)
            {
                if (ot.tasktype == (int)EnmTaskType.Init)
                {
                    foreach (CCarLocationDto car in sideLocations)
                    {
                        if (car.carloclayer == ot.devicelayer)
                        {
                             CScope scope = this.getTVscope(ot);
                             if (scope != null)
                             {                                
                                 if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                 {
                                     eqpID = ot.devicecode;
                                     return car;
                                 }
                             }
                        }
                    }
                }
            }
            #endregion
            #endregion
            #region 依层数作业的多少，优先分配层数少的，且要判断TV是不是可用的
            foreach (KeyValuePair<int, int> pair in dicLctnTaskOrderBy)
            {
                foreach (CCarLocationDto car in sideLocations)
                {
                    if (pair.Key == car.carloclayer)
                    {
                        foreach (CDeviceStatusDto smg in NearTvs)
                        {
                            if (smg.devicemode == (int)EnmModel.Automatic &&
                                smg.isable == 1 && smg.devicelayer == pair.Key)
                            {
                                 CScope scope = this.getTVscope(smg);
                                 if (scope == null)
                                 {                                    
                                     if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                     {
                                         eqpID = smg.devicecode;
                                         return car;
                                     }
                                 }
                            }
                        }
                        foreach (CDeviceStatusDto smg in otherLst)
                        {
                            if (smg.devicelayer == pair.Key)
                            {
                                 CScope scope = this.getTVscope(smg);
                                 if (scope == null)
                                 {                                    
                                     if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                                     {
                                         eqpID = smg.devicecode;
                                         return car;
                                     }
                                 }
                            }
                        }
                    }
                }
            }
            #endregion

            List<CCarLocationDto> bigLocations = new List<CCarLocationDto>();
            #region 分配超过车辆尺寸
            #region
            var locBig = from lc in middleLocations
                         where string.Compare(lc.carlocsize, checkCode) > 0
                         orderby Math.Abs((int)lc.carloccolumn - CBaseMethods.MyBase.GetColumnByAddrs(hall.deviceaddr)) ascending,
                                 lc.carloclayer ascending
                         select lc;
            bigLocations.AddRange(locBig.ToList());

            var locSideBig = from lc in allLocations
                              where lc.carloctype == (int)EnmLocationType.Normal &&
                                    lc.carlocstatus == (int)EnmLocationStatus.Space &&
                                    CCommonMethods.myMethods.GetICCard((int)lc.warehouse, lc.carlocaddr) == null &&
                                    (lc.carloccolumn < 7 || lc.carloccolumn > 36) &&
                                    string.Compare(lc.carlocsize, checkCode) > 0
                              orderby Math.Abs((int)lc.carloccolumn - CBaseMethods.MyBase.GetColumnByAddrs(hall.deviceaddr)) ascending,
                                      lc.carloclayer ascending
                              select lc;

            bigLocations.AddRange(locSideBig.ToList());
            #endregion
            #region 查找
            foreach (CCarLocationDto car in bigLocations)
            {
                #region 2016-10-28 add
                //找出当前车位所在层的TV车                       
                if (hall.devicecode != 12)
                {
                    //如果是1\3号车厅，则选择各自区域的TV
                    CDeviceStatusDto dev = availEtvs.Find(ly => ly.region == hall.region && ly.devicelayer == car.carloclayer);
                    if (dev != null)
                    {
                         CScope scope = this.getTVscope(dev);
                         if (scope != null)
                         {
                             if (car.carloccolumn >= scope.LeftCol && car.carloccolumn <= scope.RightCol)
                             {
                                 eqpID = dev.devicecode;
                                 return car;
                             }
                         }
                    }
                }
                else //如果是2#厅存车
                {
                    //获取TV1
                    List<CDeviceStatusDto> tvOnLayer = availEtvs.FindAll(tt => tt.devicelayer == car.carloclayer);
                    if (tvOnLayer.Count == 1)
                    {
                        #region
                        int hallColumn = Convert.ToInt32(hall.deviceaddr.Substring(1, 2));
                        CScope scope = this.getTVscope(tvOnLayer[0]);
                        if (scope != null)
                        {
                            if (hallColumn < scope.LeftCol || hallColumn > scope.RightCol)
                            {
                                continue;
                            }
                            //建立作业范围，判断是否可达                           
                            int lctColumn = (int)car.carloccolumn;
                            if (scope.LeftCol <= lctColumn && scope.RightCol >= lctColumn)
                            {
                                eqpID = tvOnLayer[0].devicecode;
                                return car;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        #endregion
                    }
                    else //当前层中存在二个可用的TV
                    {
                        #region
                        CDeviceStatusDto nearDev = tvOnLayer.Find(cc => cc.region == hall.region);
                        CDeviceStatusDto farDev = tvOnLayer.Find(cc => cc.region != hall.region);
                        if (nearDev.tasktype == (int)EnmTaskType.EntryTask)
                        {
                            if (nearDev.currentnode == (int)EnmFlowNodeDescp.Load)
                            {
                                if (farDev.tasktype == (int)EnmTaskType.Init)
                                {
                                    eqpID = farDev.devicecode;
                                    return car;
                                }
                                else
                                {
                                    eqpID = nearDev.devicecode;
                                    return car;
                                }
                            }
                            else
                            {
                                eqpID = nearDev.devicecode;
                                return car;
                            }
                        }
                        else if (nearDev.tasktype == (int)EnmTaskType.ExitTask)
                        {
                            if (nearDev.currentnode != (int)EnmFlowNodeDescp.Load)
                            {
                                if (farDev.tasktype == (int)EnmTaskType.Init)
                                {
                                    eqpID = farDev.devicecode;
                                    return car;
                                }
                                else
                                {
                                    eqpID = nearDev.devicecode;
                                    return car;
                                }
                            }
                            else
                            {
                                eqpID = nearDev.devicecode;
                                return car;
                            }
                        }
                        else
                        {
                            eqpID = nearDev.devicecode;
                            return car;
                        }
                        #endregion
                    }

                }
                #endregion
            }
            #endregion
            #endregion
            #endregion
            return null;
        }

        /// <summary>
        /// 获取车位信息具体实例（根据车位优先级priorityid排序分配）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarSize"></param>
        /// <param name="nLayer">-1：非平面移动车库；0：平面移动车库</param>
        /// <returns></returns>
        private CCarLocationDto AllocateLocationOfPriorityid(int nWareHouse, int nCarSize, int nLayer = -1)
        {
            // 当前库正常空余车位列表（按车位优先级priorityid排序）
            // 该车位为固定车位卡预留GetICCard((int)lct.warehouse, lct.carlocaddr) != null&&
            List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
            lstCarLocationTBL = CCommonMethods.myMethods.GetCarLocationList();
            var query = from lct in lstCarLocationTBL
                        where lct.warehouse == nWareHouse &&
                        lct.carlocstatus == (int)EnmLocationStatus.Space &&
                        lct.carloctype == (int)EnmLocationType.Normal &&
                        CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), lct.carlocsize) &&
                        CCommonMethods.myMethods.GetICCard((int)lct.warehouse, lct.carlocaddr) == null
                        orderby lct.priorityid
                        select lct;

            List<CCarLocationDto> locationList = query.ToList();
            // 车厅作为车位处理情况20160309
            foreach (CCarLocationDto lct in locationList)
            {
                CDeviceStatusDto table = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, lct.carlocaddr, EnmSMGType.Hall);
                if (null != table && ((int)EnmHallType.Exit != table.halltype || 0 == table.isavailable))
                {
                    locationList.Remove(lct);
                }
            }

            // 平面移动类型库时，判断有无空闲TV，有分配空闲TV那一层的车位
            if (0 <= nLayer)
            {
                List<CDeviceStatusDto> lstDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);

                foreach (CDeviceStatusDto deviceDtatusTBL in lstDeviceStatusTBL)
                {
                    if (1 == deviceDtatusTBL.isable && 1 == deviceDtatusTBL.isavailable && (int)EnmModel.Automatic == deviceDtatusTBL.devicemode
                        && (int)EnmTaskType.Init == deviceDtatusTBL.tasktype)
                    {
                        if (!CBaseMethods.MyBase.IsEmpty(deviceDtatusTBL.devicelayer))
                        {
                            nLayer = (int)deviceDtatusTBL.devicelayer;
                        }
                        break;
                    }
                }
            }
            if (0 < nLayer)
            {  // 找到等于该车辆尺寸的车位且空闲TV那一层的车位
                foreach (CCarLocationDto lct in locationList)
                {
                    if (lct.carlocsize == nCarSize.ToString() && lct.carloclayer == nLayer)
                    {
                        return lct;
                    }
                }
            }
            // 先找到等于该车辆尺寸的车位
            CCarLocationDto carLct = locationList.Find(s => s.carlocsize == nCarSize.ToString());

            if (null != carLct)
            {
                return carLct;
            }

            if (0 < nLayer)
            {  // 找到空闲TV那一层的车位
                foreach (CCarLocationDto lct in locationList)
                {
                    if (lct.carloclayer == nLayer)
                    {
                        return lct;
                    }
                }
            }
            if (0 < locationList.Count)
            {
                return locationList[0];
            }

            return null;
        }
        #endregion  通用算法algorithm

        /// <summary>
        /// 取车时移动设备分配
        /// </summary>
        /// <param name="lct">出库车位</param>
        /// <param name="hall">出库车厅</param>
        /// <returns></returns>
        private CDeviceStatusDto allocateEtvDevice(CCarLocationDto lct, CDeviceStatusDto hall) 
        {
            List<CDeviceStatusDto> cEtvs = CCommonMethods.myMethods.GetEtvsByLayer((int)lct.warehouse, (int)lct.carloclayer);
            TvServiceScope svscope = new TvServiceScope(cEtvs);

            int lctnCol = Convert.ToInt32(lct.carlocaddr.Substring(1, 2));
            int hallCol = Convert.ToInt32(hall.deviceaddr.Substring(1,2));

            List<CDeviceStatusDto> availEtvs = new List<CDeviceStatusDto>();
            #region
            var etvs = from smg in cEtvs
                       where smg.devicemode == (int)EnmModel.Automatic &&
                             smg.isable == 1
                       orderby Math.Abs((int)smg.region-(int)hall.region)
                       select smg;
            availEtvs.AddRange(etvs.ToList());
            #endregion           
            if (availEtvs.Count == 0) 
            {
                return null;
            }

            if (availEtvs.Count == 1)
            {
                #region
                CScope scope = svscope.GetTvScope(availEtvs[0]);
                if (scope == null) 
                {
                    CWException.WriteError("查找TV "+availEtvs[0].devicecode+" 作业范围为NULL! 出库车位："+lct.carlocaddr);
                    return null;
                }
                //可达车位
                if (scope.LeftCol > lctnCol || scope.RightCol < lctnCol) 
                {
                    return null;
                }
                //可达车厅           
                if (hall.region == availEtvs[0].region)
                {
                    return availEtvs[0];
                }
                if (scope.LeftCol <= hallCol && scope.RightCol >= hallCol) 
                {
                    return availEtvs[0];
                }
                #endregion
            }
            else
            {
                #region
                if (hall.devicecode != 12)  //1号车厅，则只有同侧TV可达
                {
                    CDeviceStatusDto devc= availEtvs.Find(cc => cc.region == hall.region);
                    CScope scope = svscope.GetTvScope(devc);
                    if (scope == null)
                    {
                        CWException.WriteError("查找TV " + devc.devicecode + " 作业范围为NULL! 出库车位：" + lct.carlocaddr);
                        return null;
                    }
                    //可达车位
                    if (scope.LeftCol <= lctnCol && scope.RightCol >= lctnCol)
                    {
                        return devc;
                    }
                }
                else //对于2#车厅，两个TV都可以到达的，则选择距离车位区域最近的TV进行作业
                {
                    //获取TV1
                    CDeviceStatusDto nearDev = availEtvs.Find(cc => cc.region == hall.region);
                    CDeviceStatusDto farDev = availEtvs.Find(cc => cc.region != hall.region);
                    if (nearDev == null || farDev == null)
                    {
                        return null;
                    }
                    //判断是否可达车位
                    List<CDeviceStatusDto> reachLocation = new List<CDeviceStatusDto>();
                    #region
                    CScope scopeNear = svscope.GetTvScope(nearDev);
                    if (scopeNear != null)
                    {
                        //可达车位
                        if (scopeNear.LeftCol <= lctnCol && scopeNear.RightCol >= lctnCol)
                        {
                            reachLocation.Add(nearDev);
                        }
                    }
                    CScope scopeFar = svscope.GetTvScope(farDev);
                    if (scopeFar != null)
                    {
                        //可达车位
                        if (scopeFar.LeftCol <= lctnCol && scopeFar.RightCol >= lctnCol)
                        {
                            reachLocation.Add(farDev);
                        }
                    }
                    #endregion
                    if (reachLocation.Count == 1) 
                    {
                        return reachLocation[0];
                    }
                    if (reachLocation.Count > 1)
                    {
                        if (nearDev.tasktype == (int)EnmTaskType.Init) //TV1空闲
                        {
                            return nearDev;
                        }
                        else
                        {
                            EnmTaskType type = (EnmTaskType)nearDev.tasktype;
                            if (type == EnmTaskType.EntryTask)
                            {
                                #region
                                if (nearDev.currentnode == (int)EnmFlowNodeDescp.Load)
                                {
                                    #region
                                    if (string.Compare(nearDev.scrloc.Substring(0, 3), hall.deviceaddr.Substring(0, 3)) != 0)
                                    {
                                        //如果近TV在向1#车厅进行装载作业,如果另一台是空闲的，则选择另一台
                                        if (farDev.tasktype == (int)EnmTaskType.Init)
                                        {
                                            return farDev;
                                        }
                                        else
                                        {
                                            int getColumn = Convert.ToInt32(lct.carlocaddr.Substring(1, 2));
                                            //远TV在作业，如果在另一侧的，则还是选择另一台TV
                                            if (getColumn < 20)
                                            {
                                                return nearDev;
                                            }
                                            else
                                            {
                                                return farDev;
                                            }
                                        }
                                    }
                                    else //如果近TV在2#车厅动作，则选择TV1作业
                                    {
                                        return nearDev;
                                    }
                                    #endregion
                                }
                                else  //近TV在进行卸载
                                {
                                    #region
                                    //建立作业范围，如果取车位在作业范围内，则选取近TV
                                    int TVColumn = Convert.ToInt32(nearDev.deviceaddr.Substring(1, 2));
                                    int toColumn = Convert.ToInt32(nearDev.desccloc.Substring(1, 2));
                                    int getColumn = Convert.ToInt32(lct.carlocaddr.Substring(1, 2));
                                    if (TVColumn < toColumn + 3)
                                    {
                                        if (toColumn + 3 > getColumn) //取车位在范围内
                                        {
                                            return nearDev;
                                        }
                                        else //在范围外
                                        {
                                            if (farDev.tasktype == (int)EnmTaskType.Init)
                                            {
                                                return farDev;
                                            }
                                            else
                                            {
                                                //远TV在作业，如果在另一侧的，则还是选择另一台TV
                                                if (getColumn < 20)
                                                {
                                                    return nearDev;
                                                }
                                                else
                                                {
                                                    return farDev;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else if (type == EnmTaskType.ExitTask || type == EnmTaskType.TmpFetch)
                            {
                                #region
                                if (nearDev.currentnode == (int)EnmFlowNodeDescp.Load)
                                {
                                    #region
                                    //近TV向车位装载
                                    //建立作业范围，如果取车位在作业范围内，则选取近TV
                                    int TVColumn = Convert.ToInt32(nearDev.deviceaddr.Substring(1, 2));
                                    int fromColumn = Convert.ToInt32(nearDev.scrloc.Substring(1, 2));

                                    int getColumn = Convert.ToInt32(lct.carlocaddr.Substring(1, 2));
                                    if (TVColumn < fromColumn + 3)
                                    {
                                        if (fromColumn + 3 > getColumn)
                                        {
                                            //在作业范围内
                                            return nearDev;
                                        }
                                        else
                                        {
                                            if (farDev.tasktype == (int)EnmTaskType.Init)
                                            {
                                                return farDev;
                                            }
                                            else
                                            {
                                                if (getColumn < 20)
                                                {
                                                    return nearDev;
                                                }
                                                else
                                                {
                                                    return farDev;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region
                                    //向车厅卸载,另一台空闲的，则选择空闲的，
                                    if (farDev.tasktype == (int)EnmTaskType.Init)
                                    {
                                        return farDev;
                                    }
                                    //如果都忙，则选择近的
                                    return nearDev;
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                return nearDev;
                            }
                        }
                    }
                    return availEtvs[0];
                }
                #endregion
            }
            return null;
        }

        /// <summary>
        /// 挪移时ETV分配
        /// </summary>
        /// <param name="srcLoc"></param>
        /// <param name="destLoc"></param>
        /// <returns></returns>
        private CDeviceStatusDto allocateEtvForTrans(CCarLocationDto srcLoc, CCarLocationDto destLoc) 
        {
            List<CDeviceStatusDto> cEtvs = CCommonMethods.myMethods.GetEtvsByLayer((int)srcLoc.warehouse, (int)destLoc.carloclayer);
            TvServiceScope svscope = new TvServiceScope(cEtvs);
            int scrCol = Convert.ToInt32(srcLoc.carlocaddr.Substring(1, 2));
            int descCol = Convert.ToInt32(destLoc.carlocaddr.Substring(1, 2));

            List<CDeviceStatusDto> availEtvs = new List<CDeviceStatusDto>();
            #region
            foreach (CDeviceStatusDto smg in cEtvs)
            {
                if (smg.devicemode == (int)EnmModel.Automatic && smg.isable == 1)
                {
                    availEtvs.Add(smg);
                }
            }
            #endregion  
            if (availEtvs.Count == 0) 
            {
                return null;
            }
            if (availEtvs.Count == 1) 
            {
                CScope scope = svscope.GetTvScope(availEtvs[0]);
                if (scope == null)
                {
                    CWException.WriteError("挪移时,查找TV " + availEtvs[0].devicecode + " 作业范围为NULL! ");
                    return null;
                }
                //可达车位
                if ((scope.LeftCol <=scrCol && scope.RightCol >=scrCol)&&
                    (scope.LeftCol <= descCol && scope.RightCol >= descCol))
                {
                    return availEtvs[0];
                }
            }
            if (availEtvs.Count == 2) 
            {
                if (scrCol > 20)
                {
                    return availEtvs.Find(cc => cc.region ==2);
                }
                else 
                {
                    return availEtvs.Find(cc => cc.region == 1);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取TV作业范围
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        private CScope getTVscope(CDeviceStatusDto smg) 
        {
            try
            {
                List<CDeviceStatusDto> cEtvs = CCommonMethods.myMethods.GetEtvsByLayer(smg.warehouse, (int)smg.devicelayer);
                TvServiceScope svscope = new TvServiceScope(cEtvs);
                CScope scope = svscope.GetTvScope(smg);
                if (scope != null)
                {
                    if (scope.LeftCol > 0 && scope.RightCol < 41)
                    {
                        return scope;
                    }
                }             
            }
            catch (Exception ex) 
            {
                CWException.WriteError("存车时获取TV作业范围时异常，devicecode:"+smg.devicecode+"    "+ex.ToString());
            }
            return null;
        }
    }
}
