using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommunicationLib;
using CommonMethodLib;
using ICCardManagementLib;
using SoundManagementLib;
using TypedefLib;
using AbstractActionLib;
using LEDManagementLib;

namespace WorkFlowLib
{
    public class CWorkFlow : IDisposable
    {
        private bool m_isDisposed = false;//保证不重复释放资源
        private bool isStart = false;

        protected int m_nPLCID;//PLC ID
        private CMainCallback m_mainCallback;//回调对象
        private CTelegramParse m_telegramParser; //报文解析对象
        private struTelegram m_struTelegram;
        //准备动作
        private CAbstractAction m_soundAction;
        private CAbstractAction m_modifyDBAction;
        private CAbstractAction m_sendTelegramAction;       
        private CTaskTriggerMgmt m_taskTrigger;
        private CICCardReaderObj[] m_ICCardReader;
        protected CCommModule m_commModule;
        Dictionary<int, struHallEquips> m_dictHallID;       
        private Dictionary<int, CSound> m_dicSound = null;
        private Dictionary<int, CLedShower> m_dicLEDDevice;

        private Thread[] m_SwiperThreads = null;
        private Thread queueThread = null;

        private Thread LedThread = null;       
        private bool isStartLed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <param name="mainCallback"></param>
        public CWorkFlow(int nPLCID, CMainCallback mainCallback)
        {           
            m_nPLCID = nPLCID;
            m_mainCallback = mainCallback;
            if (!CConfigManagement.myPara.HashGarageType.ContainsKey(m_nPLCID))
            {
                CLOGException.Trace("WorkFlowLib", "无效PLCID = " + m_nPLCID);
                return;
            }
        }        
       
        /// <summary>
        /// 构造函数
        /// </summary>
        public void Init(Dictionary<int,CLedShower> dicLedDevice)
        {
            try
            {
                //CLOGException.Trace("——————WorkFlowLib.CWorkFlow.Init ");
                if (!CConfigManagement.myPara.DicAllDeviceID.ContainsKey(m_nPLCID))
                {
                    CLOGException.Trace("WorkFlowLib.CWorkFlow.Init", "HashAllDeviceID-无效PLCID");
                    return;//无效PLCID
                }
                m_telegramParser = new CTelegramParse();
                m_struTelegram = new struTelegram();

                m_soundAction = new CSoundAction();
                m_modifyDBAction = new CModifyDBAction();
                m_sendTelegramAction = new CSendTelegramAction();              

                m_taskTrigger = new CTaskTriggerMgmt();
                m_taskTrigger.SetTaskTrigger(m_nPLCID);

                m_dictHallID = new Dictionary<int, struHallEquips>();
                if (!CConfigManagement.myPara.DicHallIDDictionary.ContainsKey(m_nPLCID))
                {
                    CLOGException.Trace("WorkFlowLib.CWorkFlow.Init", "HashHallIDDictionary-无效PLCID");
                    return;//无效PLCID
                }
                m_dictHallID = (Dictionary<int, struHallEquips>)CConfigManagement.myPara.DicHallIDDictionary[m_nPLCID];//.GetHallIDDictionary(m_nPLCID);
                
                if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(m_nPLCID))
                {
                    CLOGException.Trace("WorkFlowLib", "HashAllPLCItemAdress-无效PLC ID");
                    return;//无效PLCID
                }

                m_commModule = CCommonMethods.myMethods.GetCommModule(m_nPLCID);
                if (null == m_commModule)
                {
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Get communication module failed");
                    return; //获取通信模式块失败，请查看配置文件是否正确
                }
                
                m_commModule.Initialize((List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[m_nPLCID]);
                m_commModule.Write(0, 1);//更新PLC发送缓冲区标志位为0               
               
                m_dicSound = new Dictionary<int, CSound>();
                foreach (int hallID in (List<int>)CConfigManagement.myPara.HashHallDeviceID[m_nPLCID])
                {
                    //获取语音模块类型的对应类名及相应参数
                    string strClassName = string.Format("CSound{0}", CConfigManagement.myPara.AudioAPI);
                    CSound sound = new CSound();
                    if (string.IsNullOrWhiteSpace(strClassName))
                    {
                        CLOGException.Trace(m_nPLCID, "IEG_AP_Proc", "Get AudioAPI name failed");
                        return; //获取语音模式块失败，使用默认
                    }

                    // 配置 SpeechSynthesizer 对象以发送输出到默认音频设备。 
                    sound = (CSound)Assembly.Load("SoundManagementLib").CreateInstance("SoundManagementLib." + strClassName, false, BindingFlags.Default, null, null, null, null);
                    if (null == sound)
                    {
                        CLOGException.Trace(m_nPLCID, "IEG_AP_Proc", "Failed to new sound object, strClassName= " + strClassName);
                        return;
                    }                   
                    m_dicSound.Add(hallID, sound);
                }

                m_dicLEDDevice = dicLedDevice;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace(m_nPLCID, "WorkFlowLib.CWorkFlow.Init 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
      
        /// <summary>
        /// 析构函数
        /// 编译器自动生成Finalize()函数由GC自动调用，保证资源被回收。
        /// 最好不要声明空析构函数，造成性能问题
        /// 如果没有引用非托管资源就不需要显示声明析构函数，会造成性能问题，系统会自动生成默认析构函数
        /// </summary>
        ~CWorkFlow()
        {
            // 表示本次调用是隐式调用，由Finalize方法调用，即托管资源释放由GC来完成,此处只需要释放非托管代码即可
            Dispose(false);
        }

        /// <summary>
        /// 注销并释放分配的资源：外部手动调用或者在using中自动调用，同时释放托管资源和非托管资源
        /// </summary>
        public void Dispose()
        {
            // 表示用户显示调用
            Dispose(true);
            // 由于用户是显示调用，所以资源释放不再由GC来完成
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 释放资源，继承IDisposable接口
        /// </summary>
        /// <param name="bDisposing"></param>
        protected void Dispose(bool bDisposing)
        {
            try
            {
                if (!m_isDisposed)
                {
                    if (bDisposing)
                    {
                        isStart = false;
                        //释放托管资源
                        m_commModule.CallbackTelegramEvent -= new CallbackTelegramEventHandler(TelegramMgmt);                       
                        m_commModule.Dispose();
                        DisposeICCardReader();                      

                        if (queueThread != null && queueThread.ThreadState == ThreadState.Running)
                        {
                            queueThread.Abort();
                            CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Stop Thread(TelegramQueue) successfully");
                        }
                    }

                    this.m_isDisposed = true;
                }
            }
            catch 
            { }
        }

        public void Start()
        {
            InitTelegramMgmt();
            InitICCardReader();
            InitLEDManagement();
            startTeleQueueThread();
        }

        /// <summary>
        /// 初始化OPC
        /// 1、链接OPC
        /// 2、完成订阅
        /// 3、监控PLC数据是否有变化
        /// </summary>
        public void InitTelegramMgmt()
        {
            try
            {
                m_commModule.CallbackTelegramEvent += new CallbackTelegramEventHandler(TelegramMgmt);
                CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Init Thread(TelegramMgmt) successfully");
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace(m_nPLCID, "WorkFlowLib.CWorkFlow.InitTelegramMgmt 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 初始化刷卡器
        /// </summary>
        public void InitICCardReader()
        {
            try
            {
                //每个车厅一个刷卡器
                if (null == m_dictHallID)
                {
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib.CWorkFlow.InitICCardReader", "m_dictHallID为空");
                    return;
                }
                m_SwiperThreads = new Thread[m_dictHallID.Count];
                m_ICCardReader = new CICCardReaderObj[m_dictHallID.Count];

                int nHallCounter = 0;
                //键是车厅的ID，初始化所有的刷卡器
                foreach (int HallID in m_dictHallID.Keys)
                {
                    if (0 >= m_dictHallID[HallID].nICCardComID)
                    {
                        CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Invalid COM ID of ICCard reader. COM ID:" + m_dictHallID[HallID].nICCardComID);
                        continue;//打印日志
                    }

                    m_ICCardReader[nHallCounter] = CCommonMethods.myMethods.GetReaderObj(HallID, m_dictHallID[HallID].nICCardComID);
                    m_ICCardReader[nHallCounter].CallbackSwiperEvent += new CallbackSwiperEventHandler(ICCardMgmt);
                    m_SwiperThreads[nHallCounter] = new Thread(m_ICCardReader[nHallCounter].Run);//启动业务线程
                    m_SwiperThreads[nHallCounter].IsBackground = true;//后台线程在主线程关闭时，子线程随之关闭，速度很快。
                    m_SwiperThreads[nHallCounter].Start();
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Init Thread(Swiper) successfully, COM ID = " + m_dictHallID[HallID].nICCardComID);
                    nHallCounter++;
                }

                CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Init Thread(ICCardReader) successfully");
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.InitICCardReader 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 初始化LED
        /// </summary>
        public void InitLEDManagement()
        {
            try
            {
                if (m_nPLCID != 1) 
                {
                    return;
                }
                #region 线程方式
                isStartLed = true;
                LedThread = new Thread(new ThreadStart(LedShowMsg));
                LedThread.IsBackground = true;
                LedThread.Start();
                #endregion

                #region
                m_mainCallback.MainCallbackEvent += new MainEventHandler(LEDManagement);
                //List<CDeviceStatusDto> halls = CCommonMethods.myMethods.GetDeviceStatusList(m_nPLCID, EnmSMGType.Hall);
                //foreach (CDeviceStatusDto hall in halls) 
                //{
                //    CMainEventArgs e = new CMainEventArgs();
                //    e.ObjTable = hall;
                //    LEDManagement(this, e);
                //}
                #endregion

                CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Init Thread(LEDManagement) successfully");
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.InitLEDManagement 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
        /// <summary>
        /// 释放刷卡器资源并停止刷卡器线程
        /// </summary>
        public void DisposeICCardReader()
        {
            try
            {
                //每个车厅一个刷卡器
                if (null == m_dictHallID)
                {
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib", "m_dictHallID为空");
                    return;//打印日志m_SwiperThreads
                }
                int nHallCounter = 0;
                //键是车厅的ID，初始化所有的刷卡器
                foreach (int HallID in m_dictHallID.Keys)
                {
                    if (0 >= m_dictHallID[HallID].nICCardComID)
                    {
                        CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Invalid COM ID of ICCard reader. COM ID:" + m_dictHallID[HallID].nICCardComID);
                        continue;//打印日志
                    }

                    m_ICCardReader[nHallCounter].CallbackSwiperEvent -= new CallbackSwiperEventHandler(ICCardMgmt);
                    m_ICCardReader[nHallCounter].DisConnectCOM();
                    if (null != m_SwiperThreads[nHallCounter] && m_SwiperThreads[nHallCounter].ThreadState == ThreadState.Running)
                    {
                        m_SwiperThreads[nHallCounter].Abort();
                    }
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Uninit Thread(Swiper) successfully, COM ID = " + m_dictHallID[HallID].nICCardComID);
                    nHallCounter++;
                }

                CLOGException.Trace(m_nPLCID, "WorkFlowLib", "dispose Thread(ICCardReader) successfully");
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.UninitICCardReader 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }
       
        /// <summary>
        /// PLC数据处理
        /// PLC数据有变化时触发读取PLC数据
        /// </summary>
        public virtual void TelegramMgmt(object sender, CTelegramEventArgs e)
        {
            try
            {
                if (null == e.Telegram || null == m_commModule)
                {
                    //打印日志
                    return;
                }
                //m_commModule.Write(0, 1);//更新PLC发送缓冲区标志位为0

                if (!CConfigManagement.myPara.HashItemAdressIndexDeviceID.ContainsKey(m_nPLCID) || !((Hashtable)(CConfigManagement.myPara.HashItemAdressIndexDeviceID[m_nPLCID])).ContainsKey(e.ItemName))
                {
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib", "无效PLC ID或数据项地址坐标");
                    return;//无效数据项地址坐标
                }
                int nEquipID = (int)((Hashtable)(CConfigManagement.myPara.HashItemAdressIndexDeviceID[m_nPLCID]))[e.ItemName];

                if (-3 == nEquipID)
                {
                    if (e.ItemName.Contains(CConfigManagement.myPara.StartDBAddress))
                    {
                        //打印接收报文
                        CCommonMethods.myMethods.InsertTelegramLog(e.Telegram, 0);
                    }
                    else
                    {
                        //打印发送报文
                        CCommonMethods.myMethods.InsertTelegramLog(e.Telegram, 1);
                    }

                    //处理消息报文
                    DealTelegramMessage(e.Telegram);
                }
                else if (0 < nEquipID) 
                {                   
                    //处理设备故障文件和状态内容报文
                    DealTelegramMessage(e.Telegram, nEquipID);
                }
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.TelegramMgmt异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 读卡器处理
        /// 读卡器有读取到刷卡时触发读取卡号
        /// </summary>
        /// <param name="nHallID"></param>
        /// <param name="strPhysicalCardID"></param>
        private void ICCardMgmt(object sender, CSwiperEventArgs e)
        {
            try
            {
                int nHallID = e.HallID;
                string strPhysicalCardID = e.PhysicalCardID;
                CLOGException.Trace(m_nPLCID, "WorkFlowLib.CWorkFlow.ICCardMgmt ", string.Format("strPhysicalCardID:{0}", strPhysicalCardID));

                if (!m_dictHallID.ContainsKey(nHallID) || string.IsNullOrWhiteSpace(strPhysicalCardID))
                {
                    CLOGException.Trace(m_nPLCID, "WorkFlowLib.CWorkFlow.ICCardMgmt ", "nHallID or strPhysicalCardID is invalid");
                    return;//打印日志
                }
                struHallEquips struLocation = m_dictHallID[nHallID];
                CRequestAction requestAction = new CRequestAction();
                requestAction.struHallEquip = struLocation;
                int nActionFlag = 0;
                m_taskTrigger.SwipeICCard(ref nActionFlag, ref requestAction, strPhysicalCardID, struLocation);
                requestAction.PLCServer = m_commModule;
                requestAction.DicSound = m_dicSound;               
                requestAction.RequestAction(nActionFlag, m_sendTelegramAction, m_modifyDBAction, m_soundAction);
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.ICCardMgmt异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }      

        /// <summary>
        /// 消息报文处理
        /// </summary>
        private void DealTelegramMessage(object objTelegram)
        {
            try
            {
                if (objTelegram.GetType() != typeof(Int16[]))
                {
                    return;
                }

                //解析报文
                m_telegramParser.UnpackTelegram(out m_struTelegram, (Int16[])objTelegram);
                int nActionFlag = 0;
                CRequestAction requestAction = new CRequestAction();
                if (m_dictHallID.ContainsKey(m_struTelegram.nEquipID))
                {
                    requestAction.struHallEquip = m_dictHallID[m_struTelegram.nEquipID];
                }
                requestAction.structTelegram = m_struTelegram;

                m_taskTrigger.Telegram(ref nActionFlag, ref requestAction, m_struTelegram);
                requestAction.PLCServer = m_commModule;
                requestAction.DicSound = m_dicSound;               
                requestAction.RequestAction(nActionFlag, m_sendTelegramAction, m_modifyDBAction, m_soundAction);
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.DealTelegramMessage异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 设备故障文件和状态内容报文处理
        /// </summary>
        /// <param name="nEquipID"></param>
        private void DealTelegramMessage(object objTelegram, int nEquipID)
        {
            try
            {
                if (objTelegram.GetType() != typeof(Byte[]))
                {
                    return;
                }              
                int nActionFlag = 0;
                CRequestAction requestAction = new CRequestAction();
                if (m_dictHallID.ContainsKey(nEquipID))
                {
                    requestAction.struHallEquip = m_dictHallID[nEquipID];
                }
                struTelegram struTelegramTemp = m_struTelegram;
                struTelegramTemp.nPLCSendFlag = m_nPLCID;
                struTelegramTemp.nEquipID = nEquipID;
                requestAction.structTelegram = struTelegramTemp;

                requestAction.PLCServer = m_commModule;
                requestAction.DicSound = m_dicSound;               

                m_taskTrigger.DeviceTelegram(ref nActionFlag, ref requestAction, m_nPLCID, nEquipID, objTelegram);
                requestAction.RequestAction(nActionFlag, m_sendTelegramAction, m_modifyDBAction, m_soundAction);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CWorkFlow.DealTelegramMessage1异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
      
        /// <summary>
        /// 开启队列线程
        /// </summary>
        public void startTeleQueueThread()
        {
            try
            {
                isStart = true;
                queueThread = new Thread(new ThreadStart(dealTelegramQueue));
                queueThread.IsBackground = true;
                queueThread.Start();
                CLOGException.Trace(m_nPLCID, "WorkFlowLib", "Start Thread(TelegramQueue) successfully ");
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CWorkFlow.InitTelegramQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
   
        /// <summary>
        /// 处理队列
        /// </summary>
        private void dealTelegramQueue() 
        {
            while (isStart) 
            {
                try
                {
                    int nActionFlag = 0;
                    CRequestAction requestAction = new CRequestAction();
                    requestAction.structTelegram = m_struTelegram;
                    requestAction.PLCServer = m_commModule;
                    requestAction.DicSound = m_dicSound;

                    m_taskTrigger.DealTeleQueueInfo(ref nActionFlag, ref requestAction, m_nPLCID);
                    requestAction.RequestAction(nActionFlag, m_sendTelegramAction, m_modifyDBAction, m_soundAction);

                    Thread.Sleep(1000);
                }
                catch (Exception ex) 
                {
                    CWException.WriteError("WorkFlowLib.dealTelegramQueue 异常 " + ex.ToString());
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// led显示处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LEDManagement(object sender, CMainEventArgs e) 
        {
            try
            {
                //CTelegramSender telemSender = new CTelegramSender(m_commModule);
                //telemSender.EMSSetFreeCarLocCount(m_nPLCID);
                #region
                //if (e.ObjTable == null) 
                //{
                //    return;
                //}
                //if (e.ObjTable.GetType().ToString()=="DatabaseManagementLib.CDeviceStatusDto") 
                //{
                //    CDeviceStatusDto hall = (CDeviceStatusDto)e.ObjTable;
                //    #region
                //    int freeLctns = CCommonMethods.myMethods.GetFreeCarPOSN(hall.warehouse);
                //    CLedShower led = null;
                //    if (m_dicLEDDevice.ContainsKey(hall.devicecode))
                //    {
                //        led = m_dicLEDDevice[hall.devicecode];
                //    }
                //    if (led != null)
                //    {
                //        string message = "";
                //        switch ((EnmModel)hall.devicemode)
                //        {
                //            case EnmModel.Automatic:
                //                message = "空余"+freeLctns;
                //                break;
                //            case EnmModel.Manual:
                //                message = "手动模式";
                //                break;
                //            case EnmModel.StandAlone:
                //                message = "单机自动";
                //                break;
                //            case EnmModel.Maintenance:
                //                message = "维修模式";
                //                break;
                //            default:
                //                message = " " + (hall.devicecode - 10).ToString() + "号车厅";
                //                break;
                //        }
                //        if (hall.tasktype != (int)EnmTaskType.Init)
                //        {
                //            switch ((EnmTaskType)hall.tasktype)
                //            {
                //                case EnmTaskType.EntryTask:
                //                    message = "存车";
                //                    break;
                //                case EnmTaskType.ExitTask:
                //                    message = "取车";
                //                    break;
                //                case EnmTaskType.TmpFetch:
                //                    message = "取物";
                //                    break;
                //                default:
                //                    message = "作业";
                //                    break;
                //            }
                //            message += hall.iccode;
                //        }
                //        led.ConnectLed();
                //        led.DisplayMessagge(message);
                //        led.disConnectLed();

                //    }
                //    #endregion
                //}
                #endregion
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("WorkFlowLib.CWorkFlow.LEDManagement 发送空闲车位至PLC异常", ex.ToString());
            }
        }

        #region
        private void LedShowMsg()
        {
            #region
                      
            while (isStartLed)
            {
                List<CDeviceStatusDto> hallDevs = CCommonMethods.myMethods.GetDeviceStatusList(m_nPLCID, EnmSMGType.Hall);
                foreach (CDeviceStatusDto hall in hallDevs)
                {
                    #region
                    CLedShower led = null;
                    if (m_dicLEDDevice.ContainsKey(hall.devicecode))
                    {
                        led = m_dicLEDDevice[hall.devicecode];
                    }
                    if (led != null)
                    {
                        string message = "";
                        switch ((EnmModel)hall.devicemode)
                        {
                            case EnmModel.Automatic:
                                int freeLctns = CCommonMethods.myMethods.GetFreeCarPOSN(hall.warehouse);
                                message = "空余" + freeLctns.ToString().PadRight(5,' ');
                                break;
                            case EnmModel.Manual:
                                message = "手动模式";
                                break;
                            case EnmModel.StandAlone:
                                message = "单机自动";
                                break;
                            case EnmModel.Maintenance:
                                message = "维修模式";
                                break;
                            default:
                                message = " " + (hall.devicecode - 10).ToString() + "号车厅";
                                break;
                        }
                        if (hall.tasktype != (int)EnmTaskType.Init)
                        {
                            switch ((EnmTaskType)hall.tasktype)
                            {
                                case EnmTaskType.EntryTask:
                                    message = "存车   ";
                                    break;
                                case EnmTaskType.ExitTask:
                                    message = "取车   ";
                                    break;
                                case EnmTaskType.TmpFetch:
                                    message = "取物   ";
                                    break;
                                default:
                                    message = "作业   ";
                                    break;
                            }
                            if (!CBaseMethods.MyBase.IsEmpty(hall.iccode))
                            {
                                message = message.Trim();
                                message += hall.iccode;
                            }
                        }
                        led.ConnectLed();
                        led.DisplayMessagge(message);
                        led.disConnectLed();
                    }
                    #endregion
                }               
                Thread.Sleep(1000);
            }
            #endregion
        }
        #endregion
    }
}
