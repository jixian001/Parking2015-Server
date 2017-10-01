using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Xml;
using System.Drawing;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using System.Reflection;
using System.Configuration;
using System.IO;

namespace ConfigManagmtLib
{
    /// <summary>
    /// 配置信息类
    /// </summary>
    [Serializable]
    public sealed class CConfigManagement
    {
        private static readonly CConfigManagement m_configMgmt = new CConfigManagement();
        /// <summary>
        /// 全局静态CConfigManagement成员
        /// </summary>
        public static CConfigManagement myPara
        {
            get
            {
                return m_configMgmt;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private CConfigManagement()
        {
            Init();
            userLimitInit();            
        }
        
        #region 属性
        private int m_nPLCCount = 0;//PLC个数
        /// <summary>
        /// PLC个数
        /// </summary>
        public int PLCCount
        {
            get { return m_nPLCCount; }
        }

        private string m_strDBConnectionInfo;//GetDBConnectionInfo()
        /// <summary>
        /// 数据库链接信息
        /// </summary>
        public string DBConnectionInfo
        {
            get { return m_strDBConnectionInfo; }
        }

        private string m_strCarMaxSize = string.Empty;
        /// <summary>
        /// 本车库允许存放的最大车辆尺寸
        /// </summary>
        public string CarMaxSize
        {
            get { return m_strCarMaxSize; }
        }

        private int m_nGetCarExitQueueMaxCount = 0;
        /// <summary>
        /// 最大取车排队人数
        /// </summary>
        public int CarExitQueueMaxCount
        {
            get { return m_nGetCarExitQueueMaxCount; }
        }

        private int m_nTelegramResendTime = 0;
        /// <summary>
        /// 报文重复次数
        /// </summary>
        public int TelegramResendTime
        {
            get { return m_nTelegramResendTime; }
        }

        private int m_nLogFileMaxSize = LOG_FILE_MAXSIZE_DEFAULT;
        /// <summary>
        /// 关闭日志文件阈值，单位：MB
        /// </summary>
        public int LogFileMaxSize
        {
            get { return m_nLogFileMaxSize; }
        }

        private int m_nLogFileSaveDays = LOG_FILE_SAVE_DAYS_DEFAULT;
        /// <summary>
        /// 数据库日志保存时间，单位：天
        /// </summary>
        public int LogFileSaveDays
        {
            get { return m_nLogFileSaveDays; }
        }

        private bool m_bBillingFlag = false;
        /// <summary>
        /// 是否有计费功能标志，true:有计费返回，false:无计费返回
        /// </summary>
        public bool BillingFlag
        {
            get { return m_bBillingFlag; }
        }

        private int m_nHallCanGetCarID = 0;
        /// <summary>
        /// 车厅可接受刷取车卡状态PLC地址
        /// </summary>
        public int HallCanGetCarID
        {
            get { return m_nHallCanGetCarID; }
        }

        private int m_nEquipIsAviliableID = 0;
        /// <summary>
        /// 设备可接受新指令状态PLC地址
        /// </summary>
        public int EquipIsAviliableID
        {
            get { return m_nEquipIsAviliableID; }
        }

        private Dictionary<int, int> m_dicCarDislocationID = new Dictionary<int, int>();
        /// <summary>
        /// 车辆跑位状态PLC地址
        /// </summary>
        public Dictionary<int, int> CarDislocationID
        {
            get { return m_dicCarDislocationID; }
        }

        private Dictionary<int, int> m_dicCarOverLimitID = new Dictionary<int, int>();
        /// <summary>
        /// 车辆外形超限指令状态PLC地址
        /// </summary>
        public Dictionary<int, int> CarOverLimitID
        {
            get { return m_dicCarOverLimitID; }
        }

        private Dictionary<int, int> m_dicColorAddrHallID = new Dictionary<int, int>();
        /// <summary>
        /// 车厅故障红色与绿色界限,key-PLC ID，value-故障分界地址
        /// </summary>
        public Dictionary<int, int> ColorAddrHallID
        {
            get { return m_dicColorAddrHallID; }
        }

        private Dictionary<int, int> m_dicColorAddrETVorTVID = new Dictionary<int, int>();
        /// <summary>
        /// ETV或TV故障红色与绿色界限,key-PLC ID，value-故障分界地址
        /// </summary>
        public Dictionary<int, int> ColorAddrETVorTVID
        {
            get { return m_dicColorAddrETVorTVID; }
        }

        private int m_nBillingComID = 0;
        /// <summary>
        /// 收费处刷卡器串口号
        /// </summary>
        public int BillingComID
        {
            get { return m_nBillingComID; }
        }
        
        private int m_nEquipMaxCount = 0;//协议规定的所有设备的最大个数
        /// <summary>
        /// 协议规定的所有设备的最大个数
        /// </summary>
        public int EquipMaxCount
        {
            get { return m_nEquipMaxCount; }
        }

        private int m_nETVorTVMaxCount = 0;//协议规定的ETV或TV最大个数
        /// <summary>
        /// 协议规定的ETV或TV最大个数
        /// </summary>
        public int ETVorTVMaxCount
        {
            get { return m_nETVorTVMaxCount; }//GetETVorTVMaxCount()
        }

        private int m_nHallMaxCount = 0;//协议规定的车厅最大个数
        /// <summary>
        /// 协议规定的车厅最大个数
        /// </summary>
        public int HallMaxCount
        {
            get { return m_nHallMaxCount; }
        }

        private int m_nElevatorMaxCount = 0;//协议规定的升降设备最大个数
        /// <summary>
        /// 协议规定的升降设备最大个数
        /// </summary>
        public int ElevatorMaxCount
        {
            get { return m_nElevatorMaxCount; }
        }

        private List<int> m_lstPLCID;//协议规定的升降设备最大个数
        /// <summary>
        /// 包含所有PLC的ID的链表
        /// </summary>
        public List<int> LstPLCID
        {
            get { return m_lstPLCID; }
        }

        private Hashtable m_hashAllPLCItemAdress = new Hashtable();
        /// <summary>
        /// 所有PLC的数据项地址hashAllPLCItemAdress[PLCID,lstPLCItemAdress]
        /// key是PLC设置节点：PLC1和PLC2等，value是当前PLC的所有数据项地址链表，lstPLCItemAdress节点如下：
        /// S7:[S7 connection_1]DB1001,INT0,50 
        /// S7:[S7 connection_1]DB1001,INT100,1 
        /// S7:[S7 connection_1]DB1002,INT0,50 
        /// S7:[S7 connection_1]DB1002,INT100,1 
        /// S7:[S7 connection_1]DB1003,B0,54 
        /// S7:[S7 connection_1]DB1005,B0,54
        /// </summary>
        public Hashtable HashAllPLCItemAdress
        {
            get { return m_hashAllPLCItemAdress; }
        }
        
        /// <summary>
        /// 通信模块标志。0：OPC模块；1：Modbus模块
        /// </summary>
        private Dictionary<int, int> m_dicCommModuleFlag = new Dictionary<int, int>();
        
        private string m_strStartDBAddress = string.Empty;
        /// <summary>
        /// 第一个数据项的数据块起始地址
        /// </summary>
        public string StartDBAddress
        {
            get { return m_strStartDBAddress; }
        }

        private string m_strSQLBroker = string.Empty;
        /// <summary>
        /// 本地数据库中启动Service Broker命令行
        /// </summary>
        public string SQLBroker
        {
            get { return m_strSQLBroker; }
        }
        
        private Hashtable m_hashGarageType = new Hashtable();
        /// <summary>
        /// 获取车库类型:键值对为PLC ID（int）和车库类型（string）的哈希表
        /// </summary>
        public Hashtable HashGarageType
        {
            get { return m_hashGarageType; }
        }

        private Hashtable m_hashETVOrTVDeviceID = new Hashtable();
        /// <summary>
        /// 获取所有ETV或TV设备号:键值对为PLC ID（int）和所有ETV或TV设备号链表（List 《int》 ）的哈希表
        /// </summary>
        public Hashtable HashETVOrTVDeviceID
        {
            get { return m_hashETVOrTVDeviceID; }
        }

        private Hashtable m_hashHallDeviceID = new Hashtable();
        /// <summary>
        /// 获取所有车厅设备号：键值对为PLC ID（int）和所有车厅ID链表（List《int》 ）的哈希表
        /// </summary>
        public Hashtable HashHallDeviceID
        {
            get { return m_hashHallDeviceID; }
        }
        
        private Dictionary<int, Dictionary<int, struHallEquips>> m_dicHallIDDictionary = new Dictionary<int, Dictionary<int, struHallEquips>>();
        /// <summary>
        /// 获取所有车厅配置：键值对为PLC ID（int）和所有车厅配置（Dictionary《int, struHallEquips》 键：车厅ID，值：车厅下参数struHallEquips）的字典
        /// </summary>
        public Dictionary<int, Dictionary<int, struHallEquips>> DicHallIDDictionary
        {
            get { return m_dicHallIDDictionary; }
        }

        private Hashtable m_hashDeviceIDItemAdressIndex = new Hashtable();
        /// <summary>
        /// 根据设备ID获取所有设备的数据项地址坐标：键值对为PLC ID（int）和所有设备的号（int）的数据项地址坐标哈希表
        /// hashReturn[PLCID, hashDeviceIDItemAdressIndex[nDeviceID,ItemAdressIndex]];
        /// </summary>
        public Hashtable HashDeviceIDItemAdressIndex
        {
            get { return m_hashDeviceIDItemAdressIndex; }
        }

        private Hashtable m_hashItemAdressIndexDeviceID = new Hashtable();
        /// <summary>
        /// 根据数据项地址坐标获取所有设备的设备ID：键值对为PLC ID（int）和所有数据项地址坐标（string）哈希表
        /// hashReturn[PLCID, hashItemAdressIndexDeviceID[ItemAdress,DeviceID]]
        /// </summary>
        public Hashtable HashItemAdressIndexDeviceID
        {
            get { return m_hashItemAdressIndexDeviceID; }
        }

        private Dictionary<int, string> m_dicCommClassName = new Dictionary<int, string>();
        /// <summary>
        /// 通信模块类名
        /// </summary>
        public Dictionary<int, string> DicCommClassName
        {
            get { return m_dicCommClassName; }
        }
        
        private Dictionary<int, object[]> m_dicCommClassPara = new Dictionary<int, object[]>();
        /// <summary>
        /// 通信模块类参数
        /// </summary>
        public Dictionary<int, object[]> DicCommClassPara
        {
            get { return m_dicCommClassPara; }
        }

        private Hashtable m_hashBillingPara = new Hashtable();
        /// <summary>
        /// 获取计费参数哈希表,键值对：hashBillingPara[int nFeeType，ArrayList lstBillingPara(strClassName，objPara)]--lstBillingPara类型:ArrayList
        /// </summary>
        public Hashtable HashBillingPara
        {
            get { return m_hashBillingPara; }
        }

        private int m_nReceiveAndSendBufferLen = 0;
        /// <summary>
        /// 接收和发送报文的长度
        /// </summary>
        public int ReceiveAndSendBufferLen
        {
            get { return m_nReceiveAndSendBufferLen; }
        }
        private int m_nEquipStatusAndAlarmsLen = 0;
        /// <summary>
        /// 设备状态和告警报文的长度
        /// </summary>
        public int EquipStatusAndAlarmsLen
        {
            get { return m_nEquipStatusAndAlarmsLen; }
        }

        private int m_nEquipStatusLen = 0;
        /// <summary>
        /// 设备状态的长度
        /// </summary>
        public int EquipStatusLen
        {
            get { return m_nEquipStatusLen; }
        }

        private int m_nReservedMaxCount = 0;
        /// <summary>
        /// 定期卡预留车位最大个数
        /// </summary>
        public int ReservedMaxCount
        {
            get { return m_nReservedMaxCount; }
        }

        private string m_strAudioAPI = string.Empty;
        /// <summary>
        /// 音频播放接口
        /// </summary>
        public string AudioAPI
        {
            get { return m_strAudioAPI; }
        }

        private bool m_bCarImageFlag = false;
        /// <summary>
        /// 车辆图片和车牌号标志,(默认值：false
        ///                       有效值：false-客户端界面不显示车辆图片和车牌号；
        ///                               true-客户端界面显示车辆图片和车牌号；）
        /// </summary>
        public bool CarImageFlag
        {
            get { return m_bCarImageFlag; }
        }

        private List<string> m_lstCarSizes = new List<string>();
        /// <summary>
        /// 车辆尺寸链表
        /// </summary>
        public List<string> LstCarSizes
        {
            get { return m_lstCarSizes; }
        }

        private string m_strClientTitle = string.Empty;
        /// <summary>
        /// 客户端界面显示的系统名称
        /// </summary>
        public string ClientTitle
        {
            get { return m_strClientTitle; }
        }

        private string m_strBillingTitle = string.Empty;
        /// <summary>
        /// 计费系统界面显示的系统名称
        /// </summary>
        public string BillingTitle
        {
            get { return m_strBillingTitle; }
        }

        private string m_strSQLQueryNOTECarPOSN = string.Empty;
        /// <summary>
        /// 车位信息表的数据库订阅查询通知语句
        /// </summary>
        public string SQLQueryNOTECarPOSN
        {
            get { return m_strSQLQueryNOTECarPOSN; }
        }

        private string m_strSQLQueryNOTEDeviceFault = string.Empty;
        /// <summary>
        /// 设备故障信息表的数据库订阅查询通知语句
        /// </summary>
        public string SQLQueryNOTEDeviceFault
        {
            get { return m_strSQLQueryNOTEDeviceFault; }
        }

        private string m_strSQLQueryNOTEDeviceStatus = string.Empty;
        /// <summary>
        /// 设备状态信息表的数据库订阅查询通知语句
        /// </summary>
        public string SQLQueryNOTEDeviceStatus
        {
            get { return m_strSQLQueryNOTEDeviceStatus; }
        }

        private string m_strSQLQueryNOTELEDContent = string.Empty;
        /// <summary>
        /// 设备LED显示内容表的数据库订阅查询通知语句
        /// </summary>
        public string SQLQueryNOTELEDContent
        {
            get { return m_strSQLQueryNOTELEDContent; }
        }
        
        private string m_strAllOptPermission = string.Empty;
        /// <summary>
        /// 所有权限集合
        /// </summary>
        public string AllOptPermission
        {
            get { return m_strAllOptPermission; }
        }

        private int m_nManagerTopPRMSN = MANAGER_TOP_PRMSN_DEFAULT;
        /// <summary>
        /// 管理员最高权限。成功：返回配置的值，失败：返回默认值255
        /// </summary>
        public int ManagerTopPRMSN
        {
            get { return m_nManagerTopPRMSN; }
        }

        private int m_nOperatorTopPRMSN = OPERATOR_TOP_PRMSN_DEFAULT;
        /// <summary>
        /// 操作员最高权限。成功：返回配置的值，失败：返回默认值103
        /// </summary>
        public int OperatorTopPRMSN
        {
            get { return m_nOperatorTopPRMSN; }
        }

        private int m_nTollKeeperTopPRMSN = TOLL_KEEPER_TOP_PRMSN_DEFAULT;
        /// <summary>
        /// 计费人员最高权限。成功：返回配置的值，失败：返回默认值111
        /// </summary>
        public int TollKeeperTopPRMSN
        {
            get { return m_nTollKeeperTopPRMSN; }
        }

        private int m_nOtherTopPRMSN = OTHER_TOP_PRMSN_DEFAULT;
        /// <summary>
        /// 其他最高权限。成功：返回配置的值，失败：返回默认值103
        /// </summary>
        public int OtherTopPRMSN
        {
            get { return m_nOtherTopPRMSN; }
        }

        private int m_nCIMCWorkerTopPRMSN = CIMC_WORKER_TOP_PRMSN_DEFAULT;
        /// <summary>
        /// 中集工作人员最高权限。成功：返回配置的值，失败：返回默认值239
        /// </summary>
        public int CIMCWorkerTopPRMSN
        {
            get { return m_nCIMCWorkerTopPRMSN; }
        }

        private List<struCarPSONLayoutInfo> m_lstPanelLayoutInfo = new List<struCarPSONLayoutInfo>();
        /// <summary>
        /// 获取所有库区车位布局信息：Rectangle:边排列信息；Width:当前库总列数；Height:当前库总层数；PanelType：车位布局类型
        /// </summary>
        public List<struCarPSONLayoutInfo> LstPanelLayoutInfo
        {
            get { return m_lstPanelLayoutInfo; }
        }

        private int m_nMaintainComID = 0;
        /// <summary>
        /// 控制室串口号，默认值：0（有效值：0-USB刷卡器，1……N-串口刷卡器的串口号）
        /// </summary>
        public int MaintainComID
        {
            get { return m_nMaintainComID; }
        }

        private Dictionary<int, bool> m_dicCIMCTmpFetch = new Dictionary<int, bool>();
        /// <summary>
        /// 中集天达新厂临时取物在车厅直接处理需求标识
        /// </summary>
        public Dictionary<int, bool> DicCIMCTmpFetch
        {
            get { return m_dicCIMCTmpFetch; }
        }

        private Hashtable m_hashSharingLEDFlag = new Hashtable();
        /// <summary>
        /// 车厅之间共享一个LED显示屏标识：哈希表：key-库区号（int），value-共享标识（bool：false-不共享；true-共享，默认值：false）
        /// </summary>
        public Hashtable HashSharingLEDFlag
        {
            get { return m_hashSharingLEDFlag; }
        }

        private Dictionary<int, bool> m_dicVEHExitQueueByWH = new Dictionary<int, bool>();
        /// <summary>
        /// 按库区排队取车标识
        /// </summary>
        public Dictionary<int, bool> DicVEHExitQueueByWH
        {
            get { return m_dicVEHExitQueueByWH; }
        }

        private Dictionary<int, int> m_dicParkingDuration = new Dictionary<int, int>();
        /// <summary>
        /// 存一辆车用时
        /// </summary>
        public Dictionary<int, int> DicParkingDuration
        {
            get { return m_dicParkingDuration; }
        }

        private Dictionary<int, int> m_dicVEHExitDuration = new Dictionary<int, int>();
        /// <summary>
        /// 取一辆车用时
        /// </summary>
        public Dictionary<int, int> DicVEHExitDuration
        {
            get { return m_dicVEHExitDuration; }
        }

        private Dictionary<int, int> m_dicTmpFetchDuration = new Dictionary<int, int>();
        /// <summary>
        /// 临时取物用时
        /// </summary>
        public Dictionary<int, int> DicTmpFetchDuration
        {
            get { return m_dicTmpFetchDuration; }
        }
        
        private Dictionary<int, List<int>> m_dicAllDeviceID = new Dictionary<int, List<int>>();
        /// <summary>
        /// 所有设备的设备号，键为PLC ID（int），值为所有设备的号（int），哈希表结构hashReturn[PLCID, lstDeviceID];
        /// </summary>
        public Dictionary<int, List<int>> DicAllDeviceID
        {
            get { return m_dicAllDeviceID; }
        }

        private string m_strReaderClassName = string.Empty;
        /// <summary>
        /// 读卡器类名
        /// </summary>
        public string ReaderClassName
        {
            get { return m_strReaderClassName; }
        }

        private object[] m_arrReaderPara = null;
        public object[] ReaderPara
        {
            get { return m_arrReaderPara; }
        }
        
        private int m_nMaxTelegramID = MAX_TTELEGRAM_ID_DEFAULT;
        /// <summary>
        /// 最大发送报文ID，报文ID超过该值后从1开始，默认值：4999
        /// </summary>
        public int MaxTelegramID
        {
            get { return m_nMaxTelegramID; }
        }
        
        private int m_nShowInfoFlag = SHOW_INFO_FLAG_DEFAULT;
        /// <summary>
        /// 读取IC卡频率，单位：毫秒（默认值：1毫秒）
        /// </summary>
        public int ShowInfoFlag
        {
            get { return m_nShowInfoFlag; }
        }

        private bool m_bVehicleRotationFlag = false;
        /// <summary>
        /// 是否有计费功能标志，true:有计费返回，false:无计费返回
        /// </summary>
        public bool VehicleRotationFlag
        {
            get { return m_bVehicleRotationFlag; }
        }

        private bool m_bReplacePhyCardIDFlag = false;
        /// <summary>
        /// 6113定制需求:替换IC卡但是逻辑卡号不变仍然使用工号，默认值：0
        /// 有效值：0-关闭该功能新卡与旧卡的逻辑卡号不同；1-打开该功能，新卡与旧卡的逻辑卡号相同
        /// </summary>
        public bool ReplacePhyCardIDFlag
        {
            get { return m_bReplacePhyCardIDFlag; }
        }

        private bool m_bFixAllotTempcarlocFlag = false;
        /// <summary>
        /// 6113定制需求:固定车位卡车主无合适车位时是否分配临时车位
        /// 有效值：0-固定车主开一辆大于绑定车辆时，不分配临时车位；1-打开该功能，分配临时车位
        /// </summary>
        public bool FixAllotTempcarlocFlag
        {
            get { return m_bFixAllotTempcarlocFlag; }
        }

        private Dictionary<int, int> m_dicVIPSoundFlag = new Dictionary<int, int>();
        /// <summary>
        /// 6113定制需求:某库无临时车位时，在入库前是否提示“只剩VIP车位”语音，入库判断条件范围为(0,该值) 默认值：0
        /// 有效值：0-不提示该语音；30-打开该功能，提示该语音,30表示入库前存车自动步进值
        /// </summary>
        public Dictionary<int, int> DicVIPSoundFlag
        {
            get { return m_dicVIPSoundFlag; }
        }

        private int m_nAPPPriorityId = DEFAULT_VALUE;
        /// <summary>
        /// 移动APP平板应用取车优先级（默认值：0无优先级）
        /// </summary>
        public int APPPriorityId
        {
            get { return m_nAPPPriorityId; }
        }

        private string m_strProjectName = string.Empty;
        /// <summary>
        /// APP开发需求：每个车库管理系统项目名称（唯一性）（默认值：""）
        /// </summary>
        public string ProjectName
        {
            get { return m_strProjectName; }
        }

        private string m_strDBPathInfo = string.Empty;
        /// <summary>
        /// APP开发需求：数据中心数据库路径地址
        /// </summary>
        public string DBPathInfo
        {
            get { return m_strDBPathInfo; }
        }

        /// <summary>
        /// 显示器（LED）模块标志。1：LED串口；2：LED网口
        /// </summary>
        private Dictionary<int, int> m_dicDisplayerFlag = new Dictionary<int, int>();

        private Dictionary<int, string> m_dicDisplayerClassName = new Dictionary<int, string>();
        /// <summary>
        /// 通信模块类名
        /// </summary>
        public Dictionary<int, string> DicDisplayerClassName
        {
            get { return m_dicDisplayerClassName; }
        }

        private Dictionary<int, object[]> m_dicDisplayerClassPara = new Dictionary<int, object[]>();
        /// <summary>
        /// 通信模块类参数
        /// </summary>
        public Dictionary<int, object[]> DicDisplayerClassPara
        {
            get { return m_dicDisplayerClassPara; }
        }
        #endregion 属性

        #region 私有变量
        //配置文件
        private string m_strXmlFileName;
        private XmlDocument m_xmlData;

        //节点的属性
        private string m_strMinNodeAttribute;
        private string m_strMaxNodeAttribute;
        private string m_strDeviceIDNodeAttribute;
        private string m_strCountNodeAttribute;

        //节点
        private string m_strXmlFileNameNode;
        private string m_strPLCSettingsNode;
        private string m_strCommonSettingsNode;
        private string m_strPLCCountNode;
        private string m_strDBConnectionInfoNode;
        private string m_strCarMaxSizeNode;
        private string m_strGetCarQueueMaxCountNode;
        private string m_strTelegramResendTimeNode;
        private string m_strLogFileMaxSizeNode;
        private string m_strLogFileSaveDaysNode;
        private string m_strBillingFlagNode;
        private string m_strBillingParaNode;
        private string m_strCarDislocationNode;
        private string m_strHallCanGetCarNode;
        private string m_strEquipIsAviliableNode;
        private string m_strCarOverLimitNode;
        private string m_strColorAddrHallNode;
        private string m_strColorAddrETVorTVNode;
        private string m_strBillingComIDNode;
        private string m_strDBAddressNode;
        private string m_strEquipInfoNode;
        private string m_strPLCConnectIDNode;
        private string m_strRecvAndSendItemCountNode;
        private string m_strGarageTypeNode;
        private string m_strHallSettingsNode;
        private string m_strElevatorSettingsNode;
        private string m_strETVOrTVSettingsNode;
        private string m_strCommModuleFlagNode;
        private string m_strCommModuleParaNode;
        private string m_strStartDBAddressNode;
        private string m_strReservedMaxCountNode;
        private string m_strAudioAPINode;
        private string m_strCarImageFlagNode;
        private string m_strCarSizeSettingsNode;
        private string m_strClientTitleNode;
        private string m_strBillingTitleNode;
        private string m_strSQLQueryNOTECarPOSNNode;
        private string m_strSQLQueryNOTEDeviceFaultNode;
        private string m_strSQLQueryNOTEDeviceStatusNode;
        private string m_strSQLQueryNOTELEDContentNode;
        private string m_strAllOptPermissionNode;
        private string m_strManagerTopPRMSNNode;
        private string m_strOperatorTopPRMSNNode;
        private string m_strTollKeeperTopPRMSNNode;
        private string m_strOtherTopPRMSNNode;
        private string m_strCIMCWorkerTopPRMSNNode;
        private string m_strCarPSONLayoutInfoNode;
        private string m_strMaintainComIDNode;
        private string m_strCIMCTmpFetchNode;
        private string m_strSharingLEDFlagNode;
        private string m_strLEDComIDNode;
        private string m_strCustomerLEDComIDNode;
        private string m_strVEHExitQueueByWHNode;
        private string m_strParkingDurationNode;
        private string m_strVEHExitDurationNode;
        private string m_strTmpFetchDurationNode;
        private string m_strReaderParaNode;
        private string m_strReaderFlagNode;
        private string m_strPLCDataUpdateRateNode;
        private string m_strMaxTelegramIDNode;
        private string m_strReadICCardRateNode;
        private string m_strShowInfoFlagNode;
        private string m_strVehicleRotationFlagNode;
        private string m_strReplacePhyCardIDFlagNode;
        private string m_strFixAllotTempcarlocFlagNode;
        private string m_strVIPSoundFlagNode;
        private string m_strAPPPriorityIdNode;
        private string m_strProjectNameNode;
        private string m_strDBPathInfoNode;
        private string m_strDisplayerFlagNode;
        private string m_strDisplayerParaNode;

        #endregion 私有变量

        #region 默认值
        /// <summary>
        /// 默认值：0
        /// </summary>
        private const int DEFAULT_VALUE = 0;

        /// <summary>
        /// PLC个数默认值:1
        /// </summary>
        private const int PLC_COUNT_DEFAULT = 1;

        /// <summary>
        ///最大取车排队人数默认值:10
        /// </summary>
        private const int CAR_QUEUE_MAXCOUNT_DEFAULT = 10;

        /// <summary>
        ///报文重复发送次数默认值:3
        /// </summary>
        private const int TELEGRAM_RESEND_TIME_DEFAULT = 3;

        /// <summary>
        ///日志文件最大容量，默认值:10MB = 10 * 1024 * 1024  = 10485760byte
        /// </summary>
        private const int LOG_FILE_MAXSIZE_DEFAULT = 10485760;

        /// <summary>
        ///日志文件保存天数，默认值:7天
        /// </summary>
        private const int LOG_FILE_SAVE_DAYS_DEFAULT = 7;

        /// <summary>
        ///车辆跑位状态PLC地址默认值:122
        /// </summary>
        private const int CAR_DISLOCATION_DEFAULT = 122;

        /// <summary>
        ///车厅可接受刷取车卡状态PLC地址默认值:296
        /// </summary>
        private const int HALL_CAN_GET_CAR_DEFAULT = 296;

        /// <summary>
        ///车厅可接受新指令状态PLC地址默认值:297
        /// </summary>
        private const int EQUIP_IS_AVILIABLE_DEFAULT = 297;

        /// <summary>
        ///车辆外形超限指令状态PLC地址默认值:6
        /// </summary>
        private const int CAR_OVERLIMIT_DEFAULT = 6;

        /// <summary>
        ///刷卡器刷新频率默认值:9600
        /// </summary>
        private const int BAUD_RATE_DEFAULT = 115200;

        /// <summary>
        ///刷卡器访卡的模式默认值:1
        /// </summary>
        private const int REQUEST_CARD_MODE_DEFAULT = 1;

        /// <summary>
        ///刷卡器授权模式默认值:0
        /// </summary>
        private const Int16 AUTH_KEY_MODE_DEFAULT = 0;

        /// <summary>
        /// 刷卡器类名默认值：CQingTongReader
        /// </summary>
        private const string READER_CLASS_NAME_DEFAULT = "CQingTongReader";
        /// <summary>
        ///车辆外形超限指令状态PLC地址默认值:30
        /// </summary>
        private const int EQUIP_MAX_COUNT_DEFAULT = 30;

        /// <summary>
        ///车辆外形超限指令状态PLC地址默认值:10
        /// </summary>
        private const int ETVORTV_MAX_COUNT_DEFAULT = 10;

        /// <summary>
        ///车辆外形超限指令状态PLC地址默认值:10
        /// </summary>
        private const int HALL_MAX_COUNT_DEFAULT = 10;

        /// <summary>
        ///车辆外形超限指令状态PLC地址默认值:10
        /// </summary>
        private const int ELEVATOR_MAX_COUNT_DEFAULT = 10;

        /// <summary>
        /// 车库类型默认值:AisleStack(巷道堆垛类停车设备Aisle-stack Mechanical Parking System)
        /// </summary>
        private const string GARAGE_TYPE_DEFAULT = "AisleStack"; //巷道堆垛：AisleStack

        /// <summary>
        /// 通信模块标志默认值:0
        /// </summary>
        private const int COMM_MODULE_FLAG_DEFAULT = 1;

        /// <summary>
        ///定期卡预留车位最大个数默认值:10
        /// </summary>
        private const int RESERVED_MAX_COUNT_DEFAULT = 10;

        /// <summary>
        /// 车辆图片和车牌号标志默认值：false
        /// </summary>
        private const bool CAR_IMAGE_FLAG_DEFAULT = false;

        /// <summary>
        /// 计费标识默认值：false
        /// </summary>
        private const bool BILLING_FLAG_DEFAULT = false;

        /// <summary>
        /// 语音播放模式标识默认值：DirectX
        /// </summary>
        private const string AUDIO_API_DEFAULT = "DirectX";

        /// <summary>
        /// 管理员最高权限默认值：255
        /// </summary>
        private const int MANAGER_TOP_PRMSN_DEFAULT = 255;

        /// <summary>
        /// 操作员最高权限默认值：103
        /// </summary>
        private const int OPERATOR_TOP_PRMSN_DEFAULT = 103;

        /// <summary>
        /// 计费人员最高权限默认值：111
        /// </summary>
        private const int TOLL_KEEPER_TOP_PRMSN_DEFAULT = 111;

        /// <summary>
        /// 其他最高权限默认值：103
        /// </summary>
        private const int OTHER_TOP_PRMSN_DEFAULT = 103;

        /// <summary>
        /// 中集工作人员最高权限默认值：239
        /// </summary>
        private const int CIMC_WORKER_TOP_PRMSN_DEFAULT = 239;

        /// <summary>
        /// 最大发送报文ID默认值：4999
        /// </summary>
        private const int MAX_TTELEGRAM_ID_DEFAULT = 4999;

        /// <summary>
        /// 读取IC卡频率默认值：4999
        /// </summary>
        private const int READ_ICCARD_RATE_DEFAULT = 1;

        /// <summary>
        /// 客户端每个已停车的车位或固定卡车位上显示的内容默认值：1
        /// </summary>
        private const int SHOW_INFO_FLAG_DEFAULT = 1;
        #endregion 默认值

        /// <summary>
        /// 初始化函数
        /// </summary>
        private void Init()
        {
            try
            {
                #region 加载配置文件
                //加载配置文件
                m_strXmlFileNameNode = "/configuration/applicationSettings/ConfigManagmtLib.Properties.Settings/setting";

                string strConfigFileName = Assembly.GetAssembly(this.GetType()).GetName().Name + ".dll.config";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(strConfigFileName);
                XmlNodeList xmlNodeList = xmlDoc.SelectNodes(m_strXmlFileNameNode);
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode.Name == "setting")
                    {
                        if ("XmlFileName" == xmlNode.Attributes.GetNamedItem("name").Value)
                        {
                            m_strXmlFileName = xmlNode.FirstChild.InnerText.Trim();
                            break;
                        }
                    }
                }

                XmlReaderSettings settings = new XmlReaderSettings() { IgnoreComments = true /*忽略文档里面的注释*/ };
                XmlReader reader = XmlReader.Create(@"Config/" + m_strXmlFileName, settings);
                m_xmlData = new XmlDocument();
                m_xmlData.Load(reader);
                #endregion 加载配置文件

                #region 初始化配置文件节点地址
                //节点地址
                m_strPLCSettingsNode = "/configuration/PLCSettings";
                m_strCommonSettingsNode = "/configuration/CommonSettings";
                m_strPLCCountNode = m_strCommonSettingsNode + "/PLCCount";
                m_strDBConnectionInfoNode = m_strCommonSettingsNode + "/DBConnectionInfo";
                m_strCarMaxSizeNode = m_strCommonSettingsNode + "/CarMaxSize";
                m_strGetCarQueueMaxCountNode = m_strCommonSettingsNode + "/GetCarQueueMaxCount";
                m_strTelegramResendTimeNode = m_strCommonSettingsNode + "/TelegramResendTime";
                m_strLogFileMaxSizeNode = m_strCommonSettingsNode + "/LogFileMaxSize"; //日志文件关闭大小
                m_strLogFileSaveDaysNode = m_strCommonSettingsNode + "/LogFileSaveDays"; //日志文件保存天数
                m_strBillingFlagNode = m_strCommonSettingsNode + "/BillingFlag"; //计费标志
                m_strBillingParaNode = m_strCommonSettingsNode + "/BillingPara"; //计费参数
                m_strHallCanGetCarNode = m_strCommonSettingsNode + "/HallCanGetCar";
                m_strEquipIsAviliableNode = m_strCommonSettingsNode + "/EquipIsAviliable";
                m_strCarDislocationNode = "/CarDislocation";
                m_strCarOverLimitNode = "/CarOverLimit";
                m_strColorAddrHallNode = "/ColorAddrHall";
                m_strColorAddrETVorTVNode = "/ColorAddrETVorTV";
                m_strBillingComIDNode = m_strCommonSettingsNode + "/BillingComID";
                m_strDBAddressNode = m_strCommonSettingsNode + "/DBAddress";
                m_strEquipInfoNode = m_strCommonSettingsNode + "/EquipInfo";
                m_strPLCConnectIDNode = "/PLCConnectID";
                m_strRecvAndSendItemCountNode = "RecvAndSendItemCount";
                m_strGarageTypeNode = "/GarageType";
                m_strHallSettingsNode = "/HallSettings";
                m_strElevatorSettingsNode = "/ElevatorSettings";
                m_strETVOrTVSettingsNode = "/ETVOrTVSettings";
                m_strCountNodeAttribute = "Count";
                m_strStartDBAddressNode = "StartDBAddress";
                m_strCommModuleFlagNode = "/CommModuleFlag"; // 通信模块标志
                m_strCommModuleParaNode = "/CommModulePara"; // 通信模块参数
                m_strReservedMaxCountNode = m_strCommonSettingsNode + "/ReservedMaxCount";// 定期卡预留车位最大个数
                m_strMinNodeAttribute = "Min";
                m_strMaxNodeAttribute = "Max";
                m_strDeviceIDNodeAttribute = "/DeviceID";
                m_strAudioAPINode = m_strCommonSettingsNode + "/AudioAPI";
                m_strCarImageFlagNode = m_strCommonSettingsNode + "/CarImageFlag";
                m_strCarSizeSettingsNode = m_strCommonSettingsNode + "/CarSizeSettings";
                m_strClientTitleNode = m_strCommonSettingsNode + "/ClientTitle";
                m_strBillingTitleNode = m_strCommonSettingsNode + "/BillingTitle";
                m_strSQLQueryNOTECarPOSNNode = m_strCommonSettingsNode + "/SQLQueryNOTECarPOSN";
                m_strSQLQueryNOTEDeviceFaultNode = m_strCommonSettingsNode + "/SQLQueryNOTEDeviceFault";
                m_strSQLQueryNOTEDeviceStatusNode = m_strCommonSettingsNode + "/SQLQueryNOTEDeviceStatus";
                m_strSQLQueryNOTELEDContentNode = m_strCommonSettingsNode + "/SQLQueryNOTELEDContent";
                m_strAllOptPermissionNode = m_strCommonSettingsNode + "/AllOptPermission";
                m_strManagerTopPRMSNNode = m_strCommonSettingsNode + "/ManagerTopPRMSN";
                m_strOperatorTopPRMSNNode = m_strCommonSettingsNode + "/OperatorTopPRMSN";
                m_strTollKeeperTopPRMSNNode = m_strCommonSettingsNode + "/TollKeeperTopPRMSN";
                m_strOtherTopPRMSNNode = m_strCommonSettingsNode + "/OtherTopPRMSN";
                m_strCIMCWorkerTopPRMSNNode = m_strCommonSettingsNode + "/CIMCWorkerTopPRMSN";
                m_strCarPSONLayoutInfoNode = "/CarPSONLayoutInfo";
                m_strMaintainComIDNode = m_strCommonSettingsNode + "/MaintainComID";
                m_strCIMCTmpFetchNode = "/CIMCTmpFetch";
                m_strSharingLEDFlagNode = "/SharingLEDFlag";
                m_strLEDComIDNode = "/LEDComID";
                m_strCustomerLEDComIDNode = "/CustomerLEDComID";
                m_strVEHExitQueueByWHNode = "/VEHExitQueueByWH";
                m_strParkingDurationNode = "/ParkingDuration";
                m_strVEHExitDurationNode = "/VEHExitDuration";
                m_strTmpFetchDurationNode = "/TmpFetchDuration";
                m_strReaderParaNode = m_strCommonSettingsNode + "/ReaderPara";
                m_strReaderFlagNode = m_strCommonSettingsNode + "/ReaderFlag";
                m_strPLCDataUpdateRateNode = m_strCommonSettingsNode + "/PLCDataUpdateRate";
                m_strMaxTelegramIDNode = m_strCommonSettingsNode + "/MaxTelegramID";
                m_strReadICCardRateNode = m_strCommonSettingsNode + "/ReadICCardRate";
                m_strShowInfoFlagNode = m_strCommonSettingsNode + "/ShowInfoFlag";
                m_strVehicleRotationFlagNode = m_strCommonSettingsNode + "/VehicleRotationFlag";
                m_strReplacePhyCardIDFlagNode = m_strCommonSettingsNode + "/ReplacePhyCardIDFlag";
                m_strFixAllotTempcarlocFlagNode = m_strCommonSettingsNode + "/FixAllotTempcarlocFlag";
                m_strVIPSoundFlagNode = "/VIPSoundFlag";
                m_strAPPPriorityIdNode = m_strCommonSettingsNode + "/APPPriorityId";
                m_strProjectNameNode = m_strCommonSettingsNode + "/ProjectName";
                m_strDBPathInfoNode = m_strCommonSettingsNode + "/DBPathInfo";
                m_strDisplayerFlagNode = "/DisplayerFlag"; // 显示器（LED）模块标志
                m_strDisplayerParaNode = "/DisplayerPara"; // 显示器（LED）模块参数
                #endregion 初始化配置文件节点地址

                #region 读取配置
                m_nPLCCount = GetPLCCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nPLCCount = " + m_nPLCCount);

                m_strDBConnectionInfo = GetDBConnectionInfo();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strDBConnectionInfo = " + m_strDBConnectionInfo);

                m_nEquipMaxCount = GetEquipMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipMaxCount = " + m_nEquipMaxCount);

                m_nETVorTVMaxCount = GetETVorTVMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nETVorTVMaxCount = " + m_nETVorTVMaxCount);

                m_nHallMaxCount = GetHallMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nHallMaxCount = " + m_nHallMaxCount);

                m_nElevatorMaxCount = GetElevatorMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nElevatorMaxCount = " + m_nElevatorMaxCount);

                m_lstPLCID = GetPLCID();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: lstPLCID = " + m_lstPLCID.ToArray());

                m_strCarMaxSize = GetCarMaxSize();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strCarMaxSize = " + m_strCarMaxSize);

                m_nGetCarExitQueueMaxCount = GetCarExitQueueMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nGetCarExitQueueMaxCount = " + m_nGetCarExitQueueMaxCount);

                m_nTelegramResendTime = GetTelegramResendTime();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nTelegramResendTime = " + m_nTelegramResendTime);

                m_nLogFileMaxSize = GetLogFileMaxSize();
                CLOGException.m_logFileMaxSize = m_nLogFileMaxSize;
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nLogFileMaxSize = " + m_nLogFileMaxSize);

                m_nLogFileSaveDays = GetLogFileSaveDays();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: LogFileSaveDays = " + m_nLogFileSaveDays);

                m_bBillingFlag = GetBillingFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: bBillingFlag = " + m_bBillingFlag);

                m_dicCarDislocationID = GetCarDislocation();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nCarDislocationID = " + m_dicCarDislocationID);

                m_nHallCanGetCarID = GetHallCanGetCar();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nHallCanGetCarID = " + m_nHallCanGetCarID);

                m_nEquipIsAviliableID = GetEquipIsAviliable();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipIsAviliableID = " + m_nEquipIsAviliableID);

                m_dicCarOverLimitID = GetCarOverLimit();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nCarOverLimitID = " + m_dicCarOverLimitID);

                m_dicColorAddrHallID = GetColorAddrHall();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nColorAddrHallID = " + m_dicColorAddrHallID);

                m_dicColorAddrETVorTVID = GetColorAddrETVorTV();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nColorAddrETVorTVID = " + m_dicColorAddrETVorTVID);

                m_nBillingComID = GetBillingComID();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nBillingComID = " + m_nBillingComID);
                
                m_hashAllPLCItemAdress = GetAllPLCItemAdress();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashAllPLCItemAdress = " + m_hashAllPLCItemAdress);

                m_dicCommModuleFlag = GetCommModuleFlag();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: nCommModuleFlag = " + m_dicCommModuleFlag);

                m_strStartDBAddress = GetStartDBAddress();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strStartDBAddress = " + m_strStartDBAddress);

                m_strSQLBroker = GetSQLBroker();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strSQLBroker = " + m_strSQLBroker);
                
                m_hashGarageType = GetGarageType();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashGarageType = " + m_hashGarageType);

                m_hashETVOrTVDeviceID = GetETVOrTVDeviceID();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashETVOrTVDeviceID = " + m_hashETVOrTVDeviceID);

                m_hashHallDeviceID = GetHallDeviceID();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashHallDeviceID = " + m_hashHallDeviceID);
                
                m_dicHallIDDictionary = GetHallIDDictionary();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashHallIDDictionary = " + m_hashHallIDDictionary);

                m_hashDeviceIDItemAdressIndex = GetItemAdressIndexByDeviceID();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashDeviceIDItemAdressIndex = " + m_hashDeviceIDItemAdressIndex);

                m_hashItemAdressIndexDeviceID = GetDeviceIDByItemAdressIndex();
                //             CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipMaxCount = " + m_nEquipMaxCount);

                m_hashBillingPara = GetBillingPara();
                //             CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipMaxCount = " + m_nEquipMaxCount);

                m_nReceiveAndSendBufferLen = GetDBAddressATTRInt("ReceiveAndSendBufferLen");
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nReceiveAndSendBufferLen = " + m_nReceiveAndSendBufferLen);

                m_nEquipStatusAndAlarmsLen = GetDBAddressATTRInt("EquipStatusAndAlarmsLen");
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipStatusAndAlarmsLen = " + m_nEquipStatusAndAlarmsLen);

                m_nEquipStatusLen = GetDBAddressATTRInt("EquipStatusLen");
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nEquipStatusLen = " + m_nEquipStatusLen);
                
                m_lstCarSizes = GetCarSizes();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: lstCarSizes = " + m_lstCarSizes.ToArray());

                GetCommModulePara(ref m_dicCommClassName, ref m_dicCommClassPara);
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: strCommClassName = " + m_dicCommClassName);
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: objCommClassPara = " + m_objCommClassPara.ToString());
                
                m_nReservedMaxCount = GetReservedMaxCount();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nReservedMaxCount = " + m_nReservedMaxCount);

                m_strAudioAPI = GetAudioAPI();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strAudioAPI = " + m_strAudioAPI);

                m_bCarImageFlag = GetCarImageFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: bCarImageFlag = " + m_bCarImageFlag);

                m_strClientTitle = GetClientTitle();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strClientTitle = " + m_strClientTitle);

                m_strBillingTitle = GetBillingTitle();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strBillingTitle = " + m_strBillingTitle);

                m_strSQLQueryNOTECarPOSN = GetSQLQueryNOTECarPOSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strSQLQueryNOTECarPOSN = " + m_strSQLQueryNOTECarPOSN);

                m_strSQLQueryNOTEDeviceFault = GetSQLQueryNOTEDeviceFault();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strSQLQueryNOTEDeviceFault = " + m_strSQLQueryNOTEDeviceFault);

                m_strSQLQueryNOTEDeviceStatus = GetSQLQueryNOTEDeviceStatus();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strSQLQueryNOTEDeviceStatus = " + m_strSQLQueryNOTEDeviceStatus);

                m_strSQLQueryNOTELEDContent = GetSQLQueryNOTELEDContent();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strSQLQueryNOTELEDContent = " + m_strSQLQueryNOTELEDContent);

                m_strAllOptPermission = GetAllOptPermission();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: strAllOptPermission = " + m_strAllOptPermission);

                m_nManagerTopPRMSN = GetManagerTopPRMSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nManagerTopPRMSN = " + m_nManagerTopPRMSN);

                m_nOperatorTopPRMSN = GetOperatorTopPRMSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nOperatorTopPRMSN = " + m_nOperatorTopPRMSN);

                m_nTollKeeperTopPRMSN = GetTollKeeperTopPRMSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nTollKeeperTopPRMSN = " + m_nTollKeeperTopPRMSN);

                m_nOtherTopPRMSN = GetOtherTopPRMSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nOtherTopPRMSN = " + m_nOtherTopPRMSN);

                m_nCIMCWorkerTopPRMSN = GetCIMCWorkerTopPRMSN();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nCIMCWorkerTopPRMSN = " + m_nCIMCWorkerTopPRMSN);

                m_lstPanelLayoutInfo = GetCarPSONLayoutInfo();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: lstPanelLayoutInfo = " + m_lstPanelLayoutInfo);

                m_nMaintainComID = GetMaintainComID();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: nMaintainComID = " + m_nMaintainComID);

                m_dicCIMCTmpFetch = GetCIMCTmpFetch();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: dicCIMCTmpFetch = " + m_dicCIMCTmpFetch);

                m_hashSharingLEDFlag = GetSharingLEDFlag();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: hashSharingLEDFlag = " + m_hashSharingLEDFlag);

                m_dicVEHExitQueueByWH = GetVEHExitQueueByWH();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: dicVEHExitQueueByWH = " + m_dicVEHExitQueueByWH);

                m_dicParkingDuration = GetParkingDuration();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: dicParkingDuration = " + m_dicParkingDuration);

                m_dicVEHExitDuration = GetVEHExitDuration();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: dicVEHExitDuration = " + m_dicVEHExitDuration);

                m_dicTmpFetchDuration = GetTmpFetchDuration();
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: dicTmpFetchDuration = " + m_dicTmpFetchDuration);

                m_dicAllDeviceID = GetAllDeviceID();

                GetReaderPara(ref m_strReaderClassName, ref m_arrReaderPara);
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: m_strReaderClassName = " + m_strReaderClassName);
                //CLOGException.Trace("ConfigManagementLib", "Get value from CM: objCommClassPara = " + m_objCommClassPara.ToString());
                
                m_nMaxTelegramID = GetMaxTelegramID();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: MaxTelegramID = " + m_nMaxTelegramID);
                
                m_nShowInfoFlag = GetShowInfoFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: ShowInfoFlag = " + m_nShowInfoFlag);

                m_bVehicleRotationFlag = GetVehicleRotationFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: VehicleRotationFlag = " + m_bVehicleRotationFlag);

                m_bReplacePhyCardIDFlag = GetReplacePhyCardIDFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: ReplacePhyCardIDFlag = " + m_bReplacePhyCardIDFlag);

                m_bFixAllotTempcarlocFlag = GetFixAllotTempcarlocFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: FixAllotTempcarlocFlag = " + m_bFixAllotTempcarlocFlag);

                m_dicVIPSoundFlag = GetVIPSoundFlag();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: VIPSoundFlag = " + m_dicVIPSoundFlag);

                m_nAPPPriorityId = GetAPPPriorityId();
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: APPPriorityId = " + m_nAPPPriorityId);

                m_strProjectName = GetNodeValueStr(m_strProjectNameNode);
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: ProjectName = " + m_strProjectName);

                m_strDBPathInfo = GetNodeValueStr(m_strDBPathInfoNode); ;
                CLOGException.Trace("ConfigManagementLib", "Get value from CM: DBPathInfo = " + m_strDBPathInfo);

                m_dicDisplayerFlag = GetDisplayerFlag();
                GetDisplayerModulePara(ref m_dicDisplayerClassName, ref m_dicDisplayerClassPara);
                #endregion 读取配置
                reader.Close();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ConfigManagmtLib.CConfigManagement.Init 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        #region 私有方法
        /// <summary>
        /// 获取PLC个数，如果没有配置则取默认值
        /// </summary>
        /// <returns></returns>
        private int GetPLCCount()
        {
            int nReturn = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strPLCSettingsNode);
            if (null == xmlNode)
            {
                return nReturn;
            }
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            nReturn = GetNodeValueInt(m_strPLCCountNode);
            if (nReturn > childNodesList.Count)
            {
                nReturn = childNodesList.Count;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取数据库链接信息
        /// </summary>
        /// <returns>
        /// 成功：返回数据库链接信息
        /// 失败：返回空字符串
        /// </returns>
        private string GetDBConnectionInfo()
        {
            string strConnectionInfo = GetNodeValueStr(m_strDBConnectionInfoNode);
            string strHostName = Dns.GetHostName();
            strConnectionInfo = strConnectionInfo.Replace("LocalHostName", strHostName).Trim();
            return strConnectionInfo;
        }

        /// <summary>
        /// 本车库允许存放的最大车辆尺寸
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：配置的值得长度不是3或不是1-9的数字则返回null
        /// </returns>
        private string GetCarMaxSize()
        {
            string strCarMaxSize = string.Empty;
            strCarMaxSize = GetNodeValueStr(m_strCarMaxSizeNode);
            if (3 != strCarMaxSize.Length || !CBaseMethods.MyBase.IsUIntNumWithoutZero(strCarMaxSize))
            {
                strCarMaxSize = string.Empty;
            }
            return strCarMaxSize;

        }

        /// <summary>
        /// 获取最大取车排队人数
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetCarExitQueueMaxCount()
        {
            int nReturn = GetNodeValueInt(m_strGetCarQueueMaxCountNode);
            if (0 == nReturn)
            {
                return CAR_QUEUE_MAXCOUNT_DEFAULT;
            }
            return nReturn;

        }

        /// <summary>
        /// 获取报文重复次数
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetTelegramResendTime()
        {
            int nReturn = GetNodeValueInt(m_strTelegramResendTimeNode);
            if (0 == nReturn)
            {
                return TELEGRAM_RESEND_TIME_DEFAULT;
            }
            return nReturn;

        }

        /// <summary>
        /// 获取关闭日志文件阈值（单位：MB，默认值：10MB）
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetLogFileMaxSize()
        {
            int nLogFileMaxSize = GetNodeValueInt(m_strLogFileMaxSizeNode);
            nLogFileMaxSize = nLogFileMaxSize * 1024 * 1024;
            if (0 > int.MaxValue.CompareTo(nLogFileMaxSize) || 0 >= nLogFileMaxSize)
            {
                return LOG_FILE_MAXSIZE_DEFAULT;//默认值：10MB = 10 * 1024 * 1024  = 10485760byte
            }
            return nLogFileMaxSize;
        }

        /// <summary>
        /// 获取日志文件文件保存天数（单位：天，默认值：7天）
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetLogFileSaveDays()
        {
            int nLogFileSaveDays = GetNodeValueInt(m_strLogFileSaveDaysNode);
            if (0 > int.MaxValue.CompareTo(nLogFileSaveDays) || 0 > nLogFileSaveDays)
            {
                return LOG_FILE_SAVE_DAYS_DEFAULT;//默认值：7天
            }
            return nLogFileSaveDays;
        }

        /// <summary>
        /// 获取是否有计费功能标志，有计费返回1，无计费返回0
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private bool GetBillingFlag()
        {
            return GetBoolValue(m_strBillingFlagNode);
            //             int nBillingFlag = 0;
            //             nBillingFlag = GetNodeValueInt(m_strBillingFlagNode);
            //             if (1 == nBillingFlag)
            //             {
            //                 return true;
            //             }
            //             return BILLING_FLAG_DEFAULT;
        }

        /// <summary>
        /// 获取计费参数哈希表,键值对：hashBillingPara[strFeeType，lstBillingPara(strClassName，objPara)]--lstBillingPara类型:ArrayList
        /// </summary>
        /// <returns></returns>
        private Hashtable GetBillingPara()
        {
            Hashtable hashBillingPara = new Hashtable();
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strBillingParaNode);
            if (null != xmlNode)
            {
                XmlNodeList childNodesList = xmlNode.ChildNodes;
                foreach (XmlNode childNode in childNodesList)
                {
                    if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                    {
                        break;
                    }
                    ArrayList lstBillingPara = new ArrayList();
                    string strClassName = string.Empty;
                    object[] objPara;

                    string strFeeType = childNode.SelectSingleNode("ID").InnerText.Trim();

                    if (CBaseMethods.MyBase.IsUnsignedNumber(strFeeType))
                    {
                        strClassName = childNode.SelectSingleNode("class").InnerText.Trim();
                        lstBillingPara.Add(strClassName);
                        //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                        if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                        {
                            objPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                            lstBillingPara.Add(objPara);
                        }
                    }
                    hashBillingPara.Add(Convert.ToInt32(strFeeType), lstBillingPara);
                }
            }
            return hashBillingPara;
        }
        
        /// <summary>
        /// 车厅可接受刷取车卡状态PLC地址
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetHallCanGetCar()
        {
            int nReturn = GetNodeValueInt(m_strHallCanGetCarNode);
            if (0 == nReturn)
            {
                return HALL_CAN_GET_CAR_DEFAULT;
            }
            return nReturn;

        }

        /// <summary>
        /// 设备可接受新指令状态PLC地址
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetEquipIsAviliable()
        {
            int nReturn = GetNodeValueInt(m_strEquipIsAviliableNode);
            if (0 == nReturn)
            {
                return EQUIP_IS_AVILIABLE_DEFAULT;
            }
            return nReturn;

        }

        /// <summary>
        /// 车辆跑位状态PLC地址
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private Dictionary<int, int> GetCarDislocation()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strCarDislocationNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 车辆外形超限指令状态PLC地址
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private Dictionary<int, int> GetCarOverLimit()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strCarOverLimitNode));
            }
            return dictReturn;

        }

        /// <summary>
        /// 获取车厅故障红色与绿色界限（小于nColorAddr为红色，否则绿色）Dictionary（key-车厅ID, value-界限地址）
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, int> GetColorAddrHall()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strColorAddrHallNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 获取ETV或TV故障红色与绿色界限（小于nColorAddr为红色，否则绿色）Dictionary（key-车厅ID, value-界限地址）
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, int> GetColorAddrETVorTV()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strColorAddrETVorTVNode));
            }
            return dictReturn;
        }
        
        /// <summary>
        /// 获取收费处刷卡器串口号COMID
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回0
        /// </returns>
        private int GetBillingComID()
        {
            return GetNodeValueInt(m_strBillingComIDNode);
        }
        
        /// <summary>
        /// 获取协议规定的所有设备的最大个数
        /// </summary>
        /// <returns>
        /// 成功：返回整型数据类型的所有设备的最大个数
        /// 失败：返回0
        /// </returns>
        private int GetEquipMaxCount()
        {
            int nCount = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strEquipInfoNode);
            if (null == xmlNode)
            {
                return EQUIP_MAX_COUNT_DEFAULT;
            }
            XmlElement xmlElement = (XmlElement)xmlNode;
            if (null == xmlElement)
            {
                return EQUIP_MAX_COUNT_DEFAULT;
            }
            XmlAttributeCollection xmlAttributesList = xmlElement.Attributes;//当前节点下所有的属性链表
            //             XmlAttributeCollection xmlAttributesList = xmlNode.Attributes;
            //获取所有无符号数字属性值的和
            foreach (XmlNode xmlAttribut in xmlAttributesList)
            {
                string strCount = xmlElement.GetAttribute(xmlAttribut.Name);
                int nCountTmp = 0;
                CBaseMethods.MyBase.StringToUInt32(strCount, out nCountTmp);
                nCount += nCountTmp;
            }
            return nCount;
        }

        /// <summary>
        /// 获取协议规定的ETV或TV最大个数
        /// </summary>
        /// <returns>
        /// 成功：返回整型数据类型的ETV或TV最大个数
        /// 失败：返回0
        /// </returns>
        private int GetETVorTVMaxCount()
        {
            int nReturn = GetEquipInfoATTRInt("ETVorTVMaxCount");
            if (0 == nReturn)
            {
                return ETVORTV_MAX_COUNT_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取协议规定的车厅最大个数
        /// </summary>
        /// <returns>
        /// 成功：返回整型数据类型的车厅最大个数
        /// 失败：返回0
        /// </returns>
        private int GetHallMaxCount()
        {
            int nReturn = GetEquipInfoATTRInt("HallMaxCount");
            if (0 == nReturn)
            {
                return HALL_MAX_COUNT_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取协议规定的升降设备最大个数
        /// </summary>
        /// <returns>
        /// 成功：返回整型数据类型的升降设备最大个数
        /// 失败：返回0
        /// </returns>
        private int GetElevatorMaxCount()
        {
            int nReturn = GetEquipInfoATTRInt("ElevatorMaxCount");
            if (0 == nReturn)
            {
                return ELEVATOR_MAX_COUNT_DEFAULT;
            }
            return nReturn;
        }
        
        /// <summary>
        /// 获取键值对为PLC ID（int）和车库类型（string）的哈希表
        /// </summary>
        /// <returns></returns>
        private Hashtable GetGarageType()
        {
            Hashtable hashReturn = new Hashtable();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (hashReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                hashReturn.Add(nCounter, GetGarageType(nCounter));
            }
            return hashReturn;
        }

        /// <summary>
        /// 获取车库类型，如果没有配置则取默认值1
        /// </summary>
        /// <param name="nPLCID">所要查询的PLC ID</param>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回null
        /// </returns>
        private string GetGarageType(int nPLCID)
        {
            string strReturn = string.Empty;
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strGarageTypeNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strGarageTypeNode;
            strReturn = GetNodeValueStr(strGarageTypeNode);
            if (string.IsNullOrWhiteSpace(strReturn))
            {
                return GARAGE_TYPE_DEFAULT;
            }
            return strReturn;
        }

        /// <summary>
        /// 获取键值对为PLC ID（int）和所有ETV或TV设备号链表（List 《int》 ）的哈希表
        /// </summary>
        /// <returns></returns>
        private Hashtable GetETVOrTVDeviceID()
        {
            Hashtable hashReturn = new Hashtable();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (hashReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                hashReturn.Add(nCounter, GetETVOrTVDeviceID(nCounter));
            }
            return hashReturn;
        }

        /// <summary>
        /// 获取所有ETV或TV设备号链表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        private List<int> GetETVOrTVDeviceID(int nWareHouse)
        {
            List<int> lstDeviceID = new List<int>();
            int nDeviceCount = GetETVorTVCount(nWareHouse);
            if (0 == nDeviceCount)
            {
                return lstDeviceID;
            }
            string strPLCSettingNode = "PLC" + nWareHouse.ToString();
            string strDeviceIDNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strETVOrTVSettingsNode + m_strDeviceIDNodeAttribute;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceIDNode);
            if (null == xmlNode)
            {
                return lstDeviceID;
            }
            XmlNodeList childNodesList = xmlNode.ChildNodes;


            int nCounter = 0;
            foreach (XmlNode childNode in childNodesList)
            {
                if (nCounter >= nDeviceCount)
                {
                    break;
                }
                int nDeviceID = 0;
                if (CBaseMethods.MyBase.StringToUInt32(childNode.InnerText.Trim(), out nDeviceID))
                {
                    lstDeviceID.Add(nDeviceID);
                }
                nCounter++;
            }

            return lstDeviceID;
        }

        /// <summary>
        /// 获取键值对为PLC ID（int）和所有车厅设备号链表（List《int》 ）的哈希表
        /// </summary>
        /// <returns></returns>
        private Hashtable GetHallDeviceID()
        {
            Hashtable hashReturn = new Hashtable();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (hashReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                hashReturn.Add(nCounter, GetHallDeviceID(nCounter));
            }
            return hashReturn;
        }

        /// <summary>
        /// 获取所有车厅设备号链表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        private List<int> GetHallDeviceID(int nWareHouse)
        {
            List<int> lstDeviceID = new List<int>();
            int nDeviceCount = GetHallCount(nWareHouse);
            if (0 == nDeviceCount)
            {
                return lstDeviceID;
            }
            string strPLCSettingNode = "PLC" + nWareHouse.ToString();
            string strDeviceSettingsNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strHallSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceSettingsNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;

            int nCounter = 0;
            foreach (XmlNode childNode in childNodesList)
            {
                if (nCounter >= nDeviceCount)
                {
                    break;
                }
                foreach (XmlNode child in childNode.ChildNodes)
                {
                    if (child.Name == "HallID")
                    {
                        int nDeviceID = 0;
                        if (CBaseMethods.MyBase.StringToUInt32(child.InnerText, out nDeviceID))
                        {
                            lstDeviceID.Add(nDeviceID);
                        }
                        //lstDeviceID.Add(Convert.ToInt32(child.InnerText));
                        break;
                    }
                }
                nCounter++;
            }
            return lstDeviceID;
        }

        /// <summary>
        /// 获取键值对为PLC ID（int）和所有升降机设备号链表（List《int》 ）的哈希表
        /// </summary>
        /// <returns></returns>
        private Hashtable GetElevatorDeviceID()
        {
            Hashtable hashReturn = new Hashtable();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (hashReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                hashReturn.Add(nCounter, GetElevatorDeviceID(nCounter));
            }
            return hashReturn;
        }

        /// <summary>
        /// 获取所有升降机设备号链表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        private List<int> GetElevatorDeviceID(int nWareHouse)
        {
            List<int> lstDeviceID = new List<int>();
            int nDeviceCount = GetElevatorCount(nWareHouse);
            if (0 == nDeviceCount)
            {
                return lstDeviceID;
            }
            string strPLCSettingNode = "PLC" + nWareHouse.ToString();
            string strDeviceIDNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strElevatorSettingsNode + "/DeviceID";
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceIDNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            int nCounter = 0;
            foreach (XmlNode childNode in childNodesList)
            {
                if (nCounter >= nDeviceCount)
                {
                    break;
                }

                int nDeviceID = 0;
                if (CBaseMethods.MyBase.StringToUInt32(childNode.InnerText.Trim(), out nDeviceID))
                {
                    lstDeviceID.Add(nDeviceID);
                }
                nCounter++;
            }
            return lstDeviceID;
        }
        
        /// <summary>
        /// 获取ETV或TV个数，如果没有配置则取默认值
        /// </summary>
        /// <param name="nPLCID">所要查询的PLC ID</param>
        /// <returns>
        /// 成功：返回整型数据类型的ETV或TV个数
        /// 失败：返回0
        /// </returns>
        private int GetETVorTVCount(int nPLCID)
        {
            if (0 >= nPLCID)
            {
                return DEFAULT_VALUE;
            }
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strDeviceNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strETVOrTVSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }
            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }
            return nCount;
        }
        private int GetETVorTVCount(string strPLCSettingNode)
        {
            string strDeviceNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strETVOrTVSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }
            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }

            return nCount;
        }
        
        /// <summary>
        /// 获取车厅个数，如果没有配置则取默认值
        /// </summary>
        /// <param name="nPLCID">所要查询的PLC ID</param>
        /// <returns>
        /// 成功：返回整型数据类型的车厅个数
        /// 失败：返回0
        /// </returns>
        private int GetHallCount(int nPLCID)
        {
            if (0 >= nPLCID)
            {
                return DEFAULT_VALUE;
            }
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strDeviceNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strHallSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }
            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }

            return nCount;
        }
        private int GetHallCount(string strPLCSettingNode)
        {
            string strDeviceNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strHallSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDeviceNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }
            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }
            return nCount;
        }
        
        /// <summary>
        /// 获取升降机个数，如果没有配置则取默认值
        /// </summary>
        /// <param name="nPLCID">所要查询的PLC ID</param>
        /// <returns>
        /// 成功：返回整型数据类型的升降机个数
        /// 失败：返回0
        /// </returns>
        private int GetElevatorCount(int nPLCID)
        {
            if (0 >= nPLCID)
            {
                return DEFAULT_VALUE;
            }
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strElevatorCountValueNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strElevatorSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strElevatorCountValueNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }

            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }
            return nCount;
        }
        private int GetElevatorCount(string strPLCSettingNode)
        {
            string strElevatorCountValueNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strElevatorSettingsNode;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strElevatorCountValueNode);
            if (null == xmlNode)
            {
                return DEFAULT_VALUE;
            }
            XmlElement element = (XmlElement)xmlNode;
            if (null == element)
            {
                return DEFAULT_VALUE;
            }

            string strCount = element.GetAttribute(m_strCountNodeAttribute);
            string strMin = element.GetAttribute(m_strMinNodeAttribute);
            string strMax = element.GetAttribute(m_strMaxNodeAttribute);
            int nCount = DEFAULT_VALUE;
            int nMin = DEFAULT_VALUE;
            int nMax = DEFAULT_VALUE;

            CBaseMethods.MyBase.StringToUInt32(strCount, out nCount);
            CBaseMethods.MyBase.StringToUInt32(strMin, out nMin);
            CBaseMethods.MyBase.StringToUInt32(strMax, out nMax);
            if (nCount < nMin)
            {
                nCount = nMin;
            }
            if (nCount > nMax)
            {
                nCount = nMax;
            }
            return nCount;
        }

        /// <summary>
        /// 获取包含所有PLC的ID的链表
        /// </summary>
        /// <returns>lstPLCID</returns>
        private List<int> GetPLCID()
        {
            List<int> lstPLCID = new List<int>();
            if (0 >= GetPLCCount())
            {
                return lstPLCID;
            }
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strPLCSettingsNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            int nPLCCounter = 1;//PLC个数计数器
            foreach (XmlNode childNode in childNodesList)
            {
                string strPLCSettingNode = m_strPLCSettingsNode + "/" + childNode.Name;
                lstPLCID.Add(GetNodeATTRInt(strPLCSettingNode, "ID"));

                if (nPLCCounter >= PLCCount)
                {
                    return lstPLCID;
                }
                nPLCCounter++;
            }
            return lstPLCID;
        }

        /// <summary>
        /// 根据节点名称获取PLC的ID
        /// </summary>
        /// <returns>lstPLCID</returns>
        private int GetPLCID(string strPLCSettingNodeName)
        {
            string strPLCSettingNode = m_strPLCSettingsNode + "/" + strPLCSettingNodeName;
            return GetNodeATTRInt(strPLCSettingNode, "ID");
        }

        /// <summary>
        /// 获取所有车厅配置：键值对为PLC ID（int）和所有车厅配置（Dictionary《int, struHallEquips》 键：车厅ID，值：车厅下参数struHallEquips）的哈希表
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, Dictionary<int, struHallEquips>> GetHallIDDictionary()
        {
            Dictionary<int, Dictionary<int, struHallEquips>> dicReturn = new Dictionary<int, Dictionary<int, struHallEquips>>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {

                Dictionary<int, struHallEquips> dictHallEquips = GetHallIDDictionary(nCounter);
                if (dicReturn.ContainsKey(nCounter) || null == dictHallEquips)
                {
                    continue;
                }
                dicReturn.Add(nCounter, dictHallEquips);
            }
            return dicReturn;
        }

        /// <summary>
        /// 获取所有PLC下车厅读卡器串口号和（车厅ID，车厅下参数struHallEquips）对应关系表
        /// </summary>
        /// <returns>返回对应关系表</returns>
        private Dictionary<int, struHallEquips> GetHallIDDictionary(int nPLCID)
        {
            if (0 == nPLCID)
            {
                return null;
            }
            string strPLCSettingNode = "PLC" + nPLCID.ToString();

            Dictionary<int, struHallEquips> dictHallEquips = new Dictionary<int, struHallEquips>();
            struHallEquips struHallEquip = new struHallEquips();
            int nWareHouseID = nPLCID;
            string strHallSettingsNode = string.Format("{0}/{1}{2}", m_strPLCSettingsNode, strPLCSettingNode, m_strHallSettingsNode);
            XmlNode xmlHallNode = m_xmlData.SelectSingleNode(strHallSettingsNode);
            if (null == xmlHallNode)
            {
                return null;
            }
            XmlNodeList hallChildNodesList = xmlHallNode.ChildNodes;
            foreach (XmlNode hallChildNode in hallChildNodesList)
            {
                XmlNodeList lstHallNodes = hallChildNode.ChildNodes;
                foreach (XmlElement xmlNote in lstHallNodes)
                {
                    int nReturn = DEFAULT_VALUE;
                    switch (xmlNote.Name)
                    {
                        case "HallID":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nHallID = nReturn;//车厅节点下的参数值,HallID
                                }
                                break;
                            }
                        case "HallName":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strHallName = xmlNote.InnerText.Trim();//车厅节点下的参数值,HallName
                                }
                                else
                                {
                                    struHallEquip.strHallName = string.Empty;
                                }
                                break;
                            }
                        case "ICCardComID":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nICCardComID = nReturn;//车厅节点下的参数值,ICCardComID
                                }
                                break;
                            }
                        case "LEDAddress":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nLEDAddress = nReturn;//车厅节点下的参数值,LEDAddress
                                }
                                break;
                            }
                        case "LEDTotalStay":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nLEDTotalStay = nReturn;//车厅节点下的参数值,LEDTotalStay
                                }
                                break;
                            }
                        case "AudioDeviceDesc":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strAudioDeviceDesc = xmlNote.InnerText.Trim();//车厅节点下的参数值,AudioDeviceDesc
                                }
                                break;
                            }
                        case "LEDComID":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strLEDComID = xmlNote.InnerText.Trim();//车厅节点下的参数值，LEDComID
                                }
                                else
                                {
                                    string strLED = GetLEDComIDOfPLC(nWareHouseID); // PLC节点下的参数值
                                    if (!string.IsNullOrWhiteSpace(strLED))
                                    {
                                        struHallEquip.strLEDComID = strLED;//车厅节点下的参数值，LEDComID
                                    }
                                    else
                                    {
                                        struHallEquip.strLEDComID = GetLEDComIDOfComm();//CommonSettings节点下的参数值
                                    }
                                }
                                break;
                            }
                        case "CustomerLEDComID":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nCustomerLEDComID = nReturn;//车厅节点下的参数值，CustomerLEDComID
                                }
                                else if (0 != GetCusLEDComIDOfPLC(nWareHouseID))
                                {
                                    struHallEquip.nCustomerLEDComID = GetCusLEDComIDOfPLC(nWareHouseID);//PLC节点下的参数值
                                }
                                else
                                {
                                    struHallEquip.nCustomerLEDComID = GetCusLEDComIDOfComm();//CommonSettings节点下的参数值
                                }
                                break;
                            }
                        case "PlateIP":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strPlateIP = xmlNote.InnerText.Trim();//车厅节点下的参数值,车牌识别IP地址PlateIP
                                }
                                break;
                            }
                        case "PlatePort":
                            {
                                if (CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nReturn))
                                {
                                    struHallEquip.nPlatePort = nReturn;//车厅节点下的参数值,车牌识别端口号PlatePort
                                }
                                break;
                            }
                        case "PlateUserName":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strPlateUserName = xmlNote.InnerText.Trim();//车厅节点下的参数值,车牌识别所需用户名PlateUserName
                                }
                                break;
                            }
                        case "PlatePassword":
                            {
                                if (!string.IsNullOrWhiteSpace(xmlNote.InnerText))
                                {
                                    struHallEquip.strPlatePassword = xmlNote.InnerText.Trim();//车厅节点下的参数值,车牌识别所需密码PlatePassword
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                struHallEquip.nWareHouseID = nWareHouseID;
                if (dictHallEquips.ContainsKey(struHallEquip.nHallID))
                {
                    continue;
                }
                dictHallEquips[struHallEquip.nHallID] = struHallEquip;
            }
            return dictHallEquips;
        }

        /// <summary>
        /// 获取所有设备的设备号，键为PLC ID（int），值为所有设备的号（int），哈希表结构hashReturn[PLCID, lstDeviceID];
        /// </summary>
        /// <returns>hashReturn[PLCID, lstDeviceID]</returns>
        private Dictionary<int, List<int>> GetAllDeviceID()
        {
            //Hashtable hashReturn = GetDeviceIDHashtable();
            Dictionary<int, List<int>> dicReturn = new Dictionary<int, List<int>>();
            Hashtable hashETVOrTVDeviceID = GetETVOrTVDeviceID();
            Hashtable hashHallDeviceID = GetHallDeviceID();
            Hashtable hashElevatorDeviceID = GetElevatorDeviceID();

            //List<int> lstDeviceItemAdressIndex = new List<int>();
            for (int nPLCID = 1; nPLCID <= PLCCount; nPLCID++)
            {
                List<int> lstDeviceID = new List<int>();
                if ((nPLCID <= hashETVOrTVDeviceID.Count) && (nPLCID <= hashHallDeviceID.Count) && (nPLCID <= hashElevatorDeviceID.Count))
                {
                    //ETVOrTV
                    if (hashETVOrTVDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashETVOrTVDeviceID[nPLCID]);
                    }

                    //Hall
                    if (hashETVOrTVDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashHallDeviceID[nPLCID]);
                    }

                    //Elevator
                    if (hashETVOrTVDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashElevatorDeviceID[nPLCID]);
                    }
                    if (dicReturn.ContainsKey(nPLCID))
                    {
                        continue;
                    }
                    dicReturn.Add(nPLCID, lstDeviceID);
                }
            }
            return dicReturn;
        }

        /// <summary>
        /// 获取键值对为PLC ID（int）和所有设备的号（int）的数据项地址坐标哈希表hashReturn[PLCID, hashDeviceIDItemAdressIndex[nDeviceID,ItemAdressIndex]];
        /// </summary>
        /// <returns>hashReturn[PLCID, hashDeviceIDItemAdressIndex[nDeviceID,ItemAdressIndex]]</returns>
        private Hashtable GetItemAdressIndexByDeviceID()
        {
            //Hashtable hashReturn = GetDeviceIDHashtable();
            Hashtable hashReturn = new Hashtable();
            Hashtable hashETVOrTVDeviceID = GetETVOrTVDeviceID();
            Hashtable hashHallDeviceID = GetHallDeviceID();
            Hashtable hashElevatorDeviceID = GetElevatorDeviceID();

            //List<int> lstDeviceItemAdressIndex = new List<int>();
            for (int nPLCID = 1; nPLCID <= PLCCount; nPLCID++)
            {
                List<int> lstDeviceID = new List<int>();
                Hashtable hashDeviceIDItemAdressIndex = new Hashtable();
                if ((nPLCID <= hashETVOrTVDeviceID.Count) && (nPLCID <= hashHallDeviceID.Count) && (nPLCID <= hashElevatorDeviceID.Count))
                {
                    //ETVOrTV
                    if (hashETVOrTVDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashETVOrTVDeviceID[nPLCID]);
                    }

                    //Hall
                    if (hashHallDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashHallDeviceID[nPLCID]);
                    }

                    //Elevator
                    if (hashElevatorDeviceID.ContainsKey(nPLCID))
                    {
                        lstDeviceID.AddRange((List<int>)hashElevatorDeviceID[nPLCID]);
                    }

                    foreach (int nDeviceID in lstDeviceID)
                    {
                        if (hashDeviceIDItemAdressIndex.ContainsKey(nDeviceID))
                        {
                            continue;
                        }
                        hashDeviceIDItemAdressIndex.Add(nDeviceID, GetItemAdressIndexByDeviceID(nPLCID, nDeviceID));
                    }
                    if (hashReturn.ContainsKey(nPLCID))
                    {
                        continue;
                    }
                    hashReturn.Add(nPLCID, hashDeviceIDItemAdressIndex);
                }
            }
            return hashReturn;
        }

        /// <summary>
        /// 根据PLC ID和设备ID获取设备的数据项地址坐标
        /// </summary>
        /// <param name="nPLCID">PLC ID</param>
        /// <param name="nDeviceID">设备ID</param>
        /// <returns>
        /// 成功：返回数据项地址
        /// 失败：返回空字符串
        /// </returns>
        private int GetItemAdressIndexByDeviceID(int nPLCID, int nDeviceID)
        {
            if (0 >= nPLCID || 0 >= nDeviceID)
            {
                return -1;
            }
            int nIndex = 0;
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            //             string strItemAdress = string.Empty;
            Hashtable hashPLCItemAdresses = new Hashtable();//所有PLC的数据项地址的哈希表
            hashPLCItemAdresses = GetAllPLCItemAdress();
            int nETVorTVCount = GetETVorTVCount(nPLCID);
            int nHallCount = GetHallCount(nPLCID);
            List<string> lstPLCItemAdress = new List<string>();
            if (!hashPLCItemAdresses.ContainsKey(nPLCID))
            {
                return -1;
            }
            lstPLCItemAdress = (List<string>)hashPLCItemAdresses[nPLCID];
            int nRecvAndSendItemCount = GetNodeATTRInt(m_strDBAddressNode, m_strRecvAndSendItemCountNode);
            if (nRecvAndSendItemCount >= lstPLCItemAdress.Count || 0 == nRecvAndSendItemCount)
            {
                return -1;
            }
            lstPLCItemAdress.RemoveRange(0, nRecvAndSendItemCount);//移除非设备的数据项地址（0~4）
            nIndex += nRecvAndSendItemCount;
            int nItemAdressCounter = 0;
            int nFlag = nDeviceID / 10;
            if (0 == nFlag)
            {
                nItemAdressCounter = 1;
            }
            else if (1 == nFlag)
            {
                if (nETVorTVCount >= lstPLCItemAdress.Count || 0 > nETVorTVCount)
                {
                    return -1;
                }
                lstPLCItemAdress.RemoveRange(0, nETVorTVCount);
                nIndex += nETVorTVCount;
                nItemAdressCounter = 11;
            }
            else if (2 == nFlag)
            {
                if ((nETVorTVCount + nHallCount) > lstPLCItemAdress.Count || 0 > (nETVorTVCount + nHallCount))
                {
                    return -1;
                }
                lstPLCItemAdress.RemoveRange(0, nETVorTVCount + nHallCount);
                nIndex += (nETVorTVCount + nHallCount);
                nItemAdressCounter = 21;
            }
            foreach (string strItemAdressTmp in lstPLCItemAdress)
            {
                if (nItemAdressCounter == nDeviceID)
                {
                    break;
                }
                nIndex++;
                nItemAdressCounter++;
            }
            return nIndex;
        }

        /// <summary>
        /// 获取键值对为PLC ID（int）和所有数据项名称（string）的数据项地址坐标哈希表hashReturn[PLCID, hashItemAdressIndexDeviceID[ItemAdress,DeviceID]]
        /// </summary>
        /// <returns>hashReturn[PLCID, hashItemAdressIndexDeviceID[ItemAdress,DeviceID]]</returns>
        private Hashtable GetDeviceIDByItemAdressIndex()
        {
            //Hashtable hashReturn = GetDeviceIDHashtable();
            Hashtable hashReturn = new Hashtable();
            Hashtable hashPLCItemAdresses = new Hashtable();//所有PLC的数据项地址的哈希表
            hashPLCItemAdresses = GetAllPLCItemAdress();

            for (int nPLCID = 1; nPLCID <= PLCCount; nPLCID++)
            {
                if ((nPLCID <= hashPLCItemAdresses.Count))
                {
                    if (!hashPLCItemAdresses.ContainsKey(nPLCID) || hashReturn.ContainsKey(nPLCID))
                    {
                        continue;
                    }
                    Hashtable hashItemAdressIndexDeviceID = new Hashtable();
                    foreach (string ItemAdress in (List<string>)hashPLCItemAdresses[nPLCID])
                    {
                        if (hashItemAdressIndexDeviceID.ContainsKey(ItemAdress))
                        {
                            continue;
                        }
                        hashItemAdressIndexDeviceID.Add(ItemAdress, GetDeviceIDByItemAdressIndex(nPLCID, ItemAdress));
                    }
                    hashReturn.Add(nPLCID, hashItemAdressIndexDeviceID);
                }
            }
            return hashReturn;
        }

        /// <summary>
        /// 根据PLC ID和数据项名称获取设备号
        /// </summary>
        /// <param name="nPLCID">PLC ID</param>
        /// <param name="strItemName">S7:[S7 connection_1]DB1007,B0,54</param>
        /// <returns></returns>
        private int GetDeviceIDByItemAdressIndex(int nPLCID, string strItemName)
        {
            int nDeviceID = 0;
            Hashtable hashPLCItemAdresses = new Hashtable();//所有PLC的数据项地址的哈希表
            hashPLCItemAdresses = GetAllPLCItemAdress();
            XmlNode xmlPLCSettingsNode = m_xmlData.SelectSingleNode(m_strPLCSettingsNode);
            XmlNodeList PLCNodesList = xmlPLCSettingsNode.ChildNodes;

            foreach (XmlNode PLCNode in PLCNodesList)
            {
                if (GetPLCID(PLCNode.Name) != nPLCID || (!hashPLCItemAdresses.ContainsKey(nPLCID)))
                {
                    continue;
                }
                string strPLCSettingNode = PLCNode.Name;
                int nETVorTVCount = GetETVorTVCount(strPLCSettingNode);
                int nHallCount = GetHallCount(strPLCSettingNode);
                int nElevatorCount = GetElevatorCount(nPLCID);
                List<string> lstPLCItemAdress = new List<string>();
                lstPLCItemAdress = (List<string>)hashPLCItemAdresses[nPLCID];
                int nRecvAndSendItemCount = GetNodeATTRInt(m_strDBAddressNode, m_strRecvAndSendItemCountNode);
                if (nRecvAndSendItemCount >= lstPLCItemAdress.Count || 0 == nRecvAndSendItemCount)
                {
                    return nDeviceID;
                }
                int nCounter = 1;
                foreach (string strItem in lstPLCItemAdress)
                {
                    //获取需要的数据项名称对应的设备
                    if (strItemName.Equals(strItem))
                    {

                        if (0 >= (nCounter - nRecvAndSendItemCount - nETVorTVCount))//设备是ETV或TV
                        {
                            nDeviceID = nCounter - nRecvAndSendItemCount;//ETV或TV的设备号命名规则是：1、2、3……
                            break;
                        }
                        if (0 >= (nCounter - nRecvAndSendItemCount - nETVorTVCount - nHallCount))//设备是车厅
                        {
                            nDeviceID = nCounter - nRecvAndSendItemCount - nETVorTVCount + 10;//车厅的设备号命名规则是：11、12、13……
                            break;
                        }
                        if (0 >= (nCounter - nRecvAndSendItemCount - nETVorTVCount - nHallCount - nElevatorCount))//设备是升降机
                        {
                            nDeviceID = nCounter - nRecvAndSendItemCount - nETVorTVCount - nHallCount + 20;//升降机的设备号命名规则是：21、22、23……
                            break;
                        }
                    }
                    nCounter++;
                }
            }
            return nDeviceID;
        }

        /// <summary>
        /// 获取所有PLC的数据项地址
        /// key是PLC设置节点：PLC1和PLC2等，value是当前PLC的所有数据项地址链表，lstPLCItemAdress节点如下：
        /// S7:[S7 connection_1]DB1001,INT0,50 
        /// S7:[S7 connection_1]DB1001,INT100,1 
        /// S7:[S7 connection_1]DB1002,INT0,50 
        /// S7:[S7 connection_1]DB1002,INT100,1 
        /// S7:[S7 connection_1]DB1003,B0,54 
        /// S7:[S7 connection_1]DB1005,B0,54
        /// </summary>
        /// <returns>
        /// 成功：返回包含所有PLC的数据项地址的哈希表
        /// 失败：返回空
        /// </returns>
        private Hashtable GetAllPLCItemAdress()
        {
            Hashtable hashPLCItemAdresses = new Hashtable();//所有PLC的数据项地址的哈希表
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strPLCSettingsNode);
            if (null == xmlNode)
            {
                return hashPLCItemAdresses;//异常分支需要打印日志
            }
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            int nPLCCounter = 1;//PLC个数计数器
            foreach (XmlNode childNode in childNodesList)
            {
                string strPLCSettingNode = childNode.Name;
                //拼接数据项地址头部//S7:[S7 connection_1]
                string strPLCConnectID = GetPLCConnectID(strPLCSettingNode);
                if (string.IsNullOrWhiteSpace(strPLCConnectID))
                {
                    break;//异常分支需要打印日志
                }

                //获取接收和发送缓存及标识位数据项地址
                List<string> lstPLCItemAdress = new List<string>(strPLCConnectID.Split(';')); 
                int nPLCID = GetPLCID(strPLCSettingNode);
               
                if (hashPLCItemAdresses.ContainsKey(nPLCID))
                {
                    continue;
                }

                hashPLCItemAdresses.Add(nPLCID, lstPLCItemAdress);
                if (nPLCCounter >= PLCCount)
                {
                    return hashPLCItemAdresses;
                }
                nPLCCounter++;
            }
            return hashPLCItemAdresses;
        }

        /// <summary>
        /// 获取通信模块标志。
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回0
        /// </returns>
        private Dictionary<int, int> GetCommModuleFlag()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                int nCommModuleFlag = 0;
                nCommModuleFlag = GetNodeValueInt(nCounter, m_strCommModuleFlagNode);
                if (0 == nCommModuleFlag)
                {
                    nCommModuleFlag = GetCommModuleFlag_Common();
                }
                dictReturn.Add(nCounter, nCommModuleFlag);
            }
            return dictReturn;
        }
        
        /// <summary>
        /// 获取通信模块标志。0：OPC模块；1：Modbus模块
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回0
        /// </returns>
        private int GetCommModuleFlag_Common()
        {
            int nReturn = GetNodeValueInt(m_strCommonSettingsNode + m_strCommModuleFlagNode);
            if (0 == nReturn)
            {
                return COMM_MODULE_FLAG_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取通信模块参数链表
        /// </summary>
        /// <returns></returns>
        private void GetCommModulePara(ref Dictionary<int, string> dicClassName, ref Dictionary<int, object[]> dicPara)
        {
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dicClassName.ContainsKey(nCounter) || dicPara.ContainsKey(nCounter))
                {
                    continue;
                }

                string strClassName = string.Empty;
                object[] arrPara = null;
                bool nCommModuleFlag = false;
                nCommModuleFlag = GetCommModulePara_Common(nCounter, ref strClassName, ref arrPara);
                if (false == nCommModuleFlag)
                {
                    GetCommModulePara_Common(ref strClassName, ref arrPara);
                }
                dicClassName.Add(nCounter, strClassName);
                dicPara.Add(nCounter, arrPara);
            }
        }

        /// <summary>
        /// 获取通信模块参数链表
        /// </summary>
        /// <returns></returns>
        private void GetCommModulePara_Common(ref string strClassName, ref object[] arrPara)
        {
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strCommonSettingsNode + m_strCommModuleParaNode);

            if (null == xmlNode)
            {
                return;
            }

            int nCommModuleFlag = GetNodeValueInt(m_strCommonSettingsNode + m_strCommModuleFlagNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            foreach (XmlNode childNode in childNodesList)
            {
                if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                {
                    continue;
                }

                string strCommType = childNode.SelectSingleNode("ID").InnerText.Trim();
                int nCommType = 0;
                bool bIsUnsignedNumber = CBaseMethods.MyBase.StringToUInt32(strCommType, out nCommType);
                if (bIsUnsignedNumber && nCommModuleFlag == nCommType)
                {
                    strClassName = childNode.SelectSingleNode("class").InnerText.Trim();

                    //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                    if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                    {
                        arrPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 获取通信模块参数链表
        /// </summary>
        /// <returns></returns>
        private bool GetCommModulePara_Common(int nPLCID, ref string strClassName, ref object[] arrPara)
        {
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strCommModuleParaNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strCommModuleParaNode;
            string strCommModuleFlagNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strCommModuleFlagNode;

            XmlNode xmlNode = m_xmlData.SelectSingleNode(strCommModuleParaNode);

            if (null == xmlNode)
            {
                return false;
            }
            int nCommModuleFlag = 0;
            if (!m_dicCommModuleFlag.ContainsKey(nPLCID))
            {
                return false;
            }
            nCommModuleFlag = m_dicCommModuleFlag[nPLCID];
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            foreach (XmlNode childNode in childNodesList)
            {
                if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                {
                    continue;
                }

                string strCommType = childNode.SelectSingleNode("ID").InnerText.Trim();
                int nCommType = 0;
                bool bIsUnsignedNumber = CBaseMethods.MyBase.StringToUInt32(strCommType, out nCommType);
                if (bIsUnsignedNumber && nCommModuleFlag == nCommType)
                {
                    strClassName = childNode.SelectSingleNode("class").InnerText.Trim();

                    //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                    if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                    {
                        arrPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 第一个数据项的数据块起始地址
        /// </summary>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 null
        /// </returns>
        private string GetStartDBAddress()
        {
            return GetDBAddressATTRStr(m_strStartDBAddressNode);
        }

        /// <summary>
        /// 根据PLC ID获取车辆尺寸列表
        /// </summary>
        /// <param name="nPLCID">PLC ID</param>
        /// <returns></returns>
        private List<string> GetCarSizes()
        {
            List<string> lstCarSizes = new List<string>();

            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strCarSizeSettingsNode);
            if (null == xmlNode)
            {
                return lstCarSizes;
            }
            XmlNodeList childNodesList = xmlNode.ChildNodes;

            foreach (XmlNode childNode in childNodesList)
            {
                if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                {
                    lstCarSizes.Add(childNode.InnerText.Trim());
                }
            }
            return lstCarSizes;
        }

        /// <summary>
        /// 获取本地数据库中启动Service Broker命令行
        /// </summary>
        /// <returns></returns>
        private string GetSQLBroker()
        {
            string strDBConnInfo = GetDBConnectionInfo();
            string strInitCatalog = "Initial Catalog";
            int nIndex = strDBConnInfo.IndexOf(strInitCatalog) + strInitCatalog.Length;
            String strSub = strDBConnInfo.Substring(nIndex, strDBConnInfo.Length - nIndex).Trim();
            string strInitCatalogValue = strSub.Substring(1, strSub.IndexOf(";") - 1).Trim();
            string strSQL = string.Format("ALTER DATABASE [{0}] SET NEW_BROKER WITH ROLLBACK IMMEDIATE ALTER DATABASE [{0}] SET ENABLE_BROKER", strInitCatalogValue);
            return strSQL;
        }

        /// <summary>
        /// 获取与PLC的连接ID，如果没有配置则取默认值
        /// </summary>
        /// <param name="strPLCSettingNode">所要查询的PLC节点名称</param>
        /// <returns>
        /// 成功：返回与PLC的连接ID
        /// 失败：返回null
        /// </returns>
        private string GetPLCConnectID(string strPLCSettingNode)
        {
            string strPLCConnectIDNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strPLCConnectIDNode;
            string strPLCConnectID = GetNodeValueStr(strPLCConnectIDNode);
            return strPLCConnectID;
        }
        
        /// <summary>
        /// 获取定期卡预留车位最大个数
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private int GetReservedMaxCount()
        {
            int nReturn = GetNodeValueInt(m_strReservedMaxCountNode);
            if (0 == nReturn)
            {
                return RESERVED_MAX_COUNT_DEFAULT;
            }
            return nReturn;

        }

        /// <summary>
        /// 获取音频播放接口，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetAudioAPI()
        {
            string strReturn = GetNodeValueStr(m_strAudioAPINode);
            if (string.Empty == strReturn)
            {
                return AUDIO_API_DEFAULT;
            }
            return strReturn;
        }

        /// <summary>
        /// 获取车辆图片和车牌号标志,true：有车辆图片和车牌号，false：没有车辆图片和车牌号
        /// </summary>
        /// <returns></returns>
        private bool GetCarImageFlag()
        {
            return GetBoolValue(m_strCarImageFlagNode);
            //             if (1 == GetNodeValueInt(m_strCarImageFlagNode))
            //             {
            //                 return true;
            //             }
            //             return CAR_IMAGE_FLAG_DEFAULT;
        }

        /// <summary>
        /// 获取客户端界面显示的系统名称，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetClientTitle()
        {
            return GetNodeValueStr(m_strClientTitleNode);
        }

        /// <summary>
        /// 获取计费系统界面显示的系统名称，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetBillingTitle()
        {
            return GetNodeValueStr(m_strBillingTitleNode);
        }

        /// <summary>
        /// 获取车位信息表的数据库订阅查询通知语句，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetSQLQueryNOTECarPOSN()
        {
            return GetNodeValueStr(m_strSQLQueryNOTECarPOSNNode);
        }

        /// <summary>
        /// 获取设备故障信息表的数据库订阅查询通知语句，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetSQLQueryNOTEDeviceFault()
        {
            return GetNodeValueStr(m_strSQLQueryNOTEDeviceFaultNode);
        }

        /// <summary>
        /// 获取设备状态信息表的数据库订阅查询通知语句，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetSQLQueryNOTEDeviceStatus()
        {
            return GetNodeValueStr(m_strSQLQueryNOTEDeviceStatusNode);
        }

        /// <summary>
        /// 获取LED显示内容信息表的数据库订阅查询通知语句，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetSQLQueryNOTELEDContent()
        {
            return GetNodeValueStr(m_strSQLQueryNOTELEDContentNode);
        }

        /// <summary>
        /// 获取所有权限集合，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <returns></returns>
        private string GetAllOptPermission()
        {
            return GetNodeValueStr(m_strAllOptPermissionNode);
        }

        /// <summary>
        /// 获取管理员最高权限。成功：返回配置的值，失败：返回默认值255
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值255
        /// </returns>
        private int GetManagerTopPRMSN()
        {
            int nReturn = GetNodeValueInt(m_strManagerTopPRMSNNode);
            if (0 == nReturn)
            {
                return MANAGER_TOP_PRMSN_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取操作员最高权限。成功：返回配置的值，失败：返回默认值103
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值103
        /// </returns>
        private int GetOperatorTopPRMSN()
        {
            int nReturn = GetNodeValueInt(m_strOperatorTopPRMSNNode);
            if (0 == nReturn)
            {
                return OPERATOR_TOP_PRMSN_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取计费人员最高权限。成功：返回配置的值，失败：返回默认值111
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值111
        /// </returns>
        private int GetTollKeeperTopPRMSN()
        {
            int nReturn = GetNodeValueInt(m_strTollKeeperTopPRMSNNode);
            if (0 == nReturn)
            {
                return TOLL_KEEPER_TOP_PRMSN_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取其他最高权限。成功：返回配置的值，失败：返回默认值103
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值103
        /// </returns>
        private int GetOtherTopPRMSN()
        {
            int nReturn = GetNodeValueInt(m_strOtherTopPRMSNNode);
            if (0 == nReturn)
            {
                return OTHER_TOP_PRMSN_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取中集工作人员最高权限。成功：返回配置的值，失败：返回默认值239
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值239
        /// </returns>
        private int GetCIMCWorkerTopPRMSN()
        {
            int nReturn = GetNodeValueInt(m_strCIMCWorkerTopPRMSNNode);
            if (0 == nReturn)
            {
                return CIMC_WORKER_TOP_PRMSN_DEFAULT;
            }
            return nReturn;
        }

        /// <summary>
        /// 获取所有库区车位布局信息：Rectangle:边排列信息；Width:当前库总列数；Height:当前库总层数；PanelType：车位布局类型
        /// </summary>
        /// <returns></returns>
        private List<struCarPSONLayoutInfo> GetCarPSONLayoutInfo()
        {
            List<struCarPSONLayoutInfo> lstReturn = new List<struCarPSONLayoutInfo>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                struCarPSONLayoutInfo struCarPSONLayoutInfoTmp = GetCarPSONLayoutInfo(nCounter);
                if (lstReturn.Contains(struCarPSONLayoutInfoTmp))
                {
                    continue;
                }
                lstReturn.Add(struCarPSONLayoutInfoTmp);
            }
            return lstReturn;
        }

        /// <summary>
        /// 根据库区号获取车位布局信息：Rectangle:边排列信息；Width:当前库总列数；Height:当前库总层数；PanelType：车位布局类型
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <returns></returns>
        private struCarPSONLayoutInfo GetCarPSONLayoutInfo(int nPLCID)
        {
            struCarPSONLayoutInfo struReturn = new struCarPSONLayoutInfo();
            if (0 == nPLCID)
            {
                return struReturn;
            }
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strCarPSONLayoutInfoNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strCarPSONLayoutInfoNode;
            XmlNode xmlHallNode = m_xmlData.SelectSingleNode(strCarPSONLayoutInfoNode);
            if (null == xmlHallNode)
            {
                return struReturn;
            }
            XmlNodeList lstChildNodesList = xmlHallNode.ChildNodes;
            int nRectangle = DEFAULT_VALUE;
            int nWidth = DEFAULT_VALUE;
            int nHeight = DEFAULT_VALUE;
            string strPanelTypes = string.Empty;
            foreach (XmlElement xmlNote in lstChildNodesList)
            {
                switch (xmlNote.Name)
                {
                    case "Rectangle":
                        {
                            if (!CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nRectangle))
                            {
                                return struReturn;
                            }
                            break;
                        }
                    case "Width":
                        {
                            if (!CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nWidth))
                            {
                                return struReturn;
                            }
                            break;
                        }
                    case "Height":
                        {
                            if (!CBaseMethods.MyBase.StringToUInt32(xmlNote.InnerText, out nHeight))
                            {
                                return struReturn;
                            }
                            break;
                        }
                    case "PanelType":
                        {
                            if (string.IsNullOrWhiteSpace(xmlNote.InnerText))
                            {
                                return struReturn;
                            }
                            strPanelTypes = xmlNote.InnerText;
                            break;
                        }
                    default:
                        {
                            return struReturn;
                        }
                }
            }
            struReturn = new struCarPSONLayoutInfo(new Rectangle(nPLCID, nRectangle, nWidth, nHeight), strPanelTypes);
            return struReturn;
        }

        /// <summary>
        /// 获取控制室串口号，成功：返回 配置的值，失败：返回0
        /// </summary>
        /// <returns></returns>
        private int GetMaintainComID()
        {
            return GetNodeValueInt(m_strMaintainComIDNode);
        }

        /// <summary>
        /// 获取中集天达新厂临时取物在车厅直接处理需求标识
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> GetCIMCTmpFetch()
        {
            Dictionary<int, bool> dictReturn = new Dictionary<int, bool>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetBoolValue(nCounter, m_strCIMCTmpFetchNode));
            }
            return dictReturn;
        }
        
        /// <summary>
        /// 获取车厅之间共享一个LED显示屏标识
        /// </summary>
        /// <returns></returns>
        private Hashtable GetSharingLEDFlag()
        {
            Hashtable hashReturn = new Hashtable();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (hashReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                hashReturn.Add(nCounter, GetBoolValue(nCounter, m_strSharingLEDFlagNode));
            }
            return hashReturn;
        }
        
        /// <summary>
        /// 获取CommonSettings下的LED通信串口号，失败返回0
        /// </summary>
        /// <returns></returns>
        private string GetLEDComIDOfComm()
        {
            return GetNodeValueStr(m_strCommonSettingsNode + m_strLEDComIDNode);
        }

        /// <summary>
        /// 获取PLC下的LED通信串口号，失败返回0
        /// </summary>
        /// <returns></returns>
        private string GetLEDComIDOfPLC(int nPLCID)
        {
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strLEDComIDNode;
            string strAttributeValue = string.Empty;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNode);
            if (null != xmlNode)
            {
                strAttributeValue = xmlNode.InnerText.Trim();
            }
            return strAttributeValue;
        }

        /// <summary>
        /// 获取CommonSettings下的顾客显示屏通信串口号，失败返回0
        /// </summary>
        /// <returns></returns>
        private int GetCusLEDComIDOfComm()
        {
            return GetNodeValueInt(m_strCommonSettingsNode + m_strCustomerLEDComIDNode);
        }

        /// <summary>
        /// 获取PLC下的顾客显示屏通信串口号，失败返回0
        /// </summary>
        /// <returns></returns>
        private int GetCusLEDComIDOfPLC(int nPLCID)
        {
            return GetNodeValueInt(nPLCID, m_strCustomerLEDComIDNode);
        }

        /// <summary>
        /// 获取按库区排队取车标识
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> GetVEHExitQueueByWH()
        {
            Dictionary<int, bool> dictReturn = new Dictionary<int, bool>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetBoolValue(nCounter, m_strVEHExitQueueByWHNode));
            }
            return dictReturn;
        }
        
        /// <summary>
        /// 获取存一辆车所用时间
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, int> GetParkingDuration()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strParkingDurationNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 获取取一辆车所用时间
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, int> GetVEHExitDuration()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strVEHExitDurationNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 获取临时取物所用时间
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, int> GetTmpFetchDuration()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strTmpFetchDurationNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 获取IC卡管理模块参数
        /// </summary>
        /// <returns></returns>
        private void GetReaderPara(ref string strClassName, ref object[] arrPara)
        {
            strClassName = READER_CLASS_NAME_DEFAULT;
            string[] strArrPara = new string[4];
            strArrPara[0] = BAUD_RATE_DEFAULT.ToString();
            strArrPara[1] = REQUEST_CARD_MODE_DEFAULT.ToString();
            strArrPara[2] = AUTH_KEY_MODE_DEFAULT.ToString();
            strArrPara[3] = READ_ICCARD_RATE_DEFAULT.ToString();
            arrPara = strArrPara;
            if (null == m_xmlData)
            {
                return;
            }
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strReaderParaNode);

            if (null == xmlNode)
            {
                return;
            }

            int nCommModuleFlag = GetNodeValueInt(m_strReaderFlagNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            foreach (XmlNode childNode in childNodesList)
            {
                if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                {
                    continue;
                }

                string strCommType = childNode.SelectSingleNode("ID").InnerText.Trim();
                int nCommType = 0;
                bool bIsUnsignedNumber = CBaseMethods.MyBase.StringToUInt32(strCommType, out nCommType);
                if (bIsUnsignedNumber && nCommModuleFlag == nCommType)
                {
                    strClassName = childNode.SelectSingleNode("class").InnerText.Trim();

                    //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                    if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                    {
                        arrPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                    }
                    break;
                }
            }
        }
        
        /// <summary>
        /// 最大发送报文ID，报文ID超过该值后从1开始
        /// </summary>
        /// <returns></returns>
        private int GetMaxTelegramID()
        {
            int nReturn = GetNodeValueInt(m_strMaxTelegramIDNode);
            if (0 == nReturn)
            {
                return MAX_TTELEGRAM_ID_DEFAULT;
            }
            return nReturn;
        }
        
        /// <summary>
        /// 客户端每个已停车的车位或固定卡车位上显示的内容（默认值：1-显示卡号）
        /// </summary>
        /// <returns></returns>
        private int GetShowInfoFlag()
        {
            int nReturn = GetNodeValueInt(m_strShowInfoFlagNode);
            if (0 == nReturn)
            {
                return SHOW_INFO_FLAG_DEFAULT;
            }
            return nReturn;
        }
        
        /// <summary>
        /// 6113定制需求：车辆空闲时掉头旋转标识，默认值：0，有效值：0-无旋转功能；1-有旋转功能
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private bool GetVehicleRotationFlag()
        {
            return GetBoolValue(m_strVehicleRotationFlagNode);
        }

        /// <summary>
        /// 6113定制需求:替换IC卡但是逻辑卡号不变仍然使用工号，默认值：0
        /// 有效值：0-关闭该功能新卡与旧卡的逻辑卡号不同；1-打开该功能，新卡与旧卡的逻辑卡号相同
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private bool GetReplacePhyCardIDFlag()
        {
            return GetBoolValue(m_strReplacePhyCardIDFlagNode);
        }

        /// <summary>
        /// add 20151020 by wanxiaona 
        /// 6113定制需求:固定车位卡车主无合适车位时是否分配临时车位
        /// 有效值：0-固定车主开一辆大于绑定车辆时，不分配临时车位；1-打开该功能，分配临时车位
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private bool GetFixAllotTempcarlocFlag()
        {
            return GetBoolValue(m_strFixAllotTempcarlocFlagNode);
        }

        /// <summary>
        /// add 20151020 by wanxiaona 
        /// 6113定制需求:某库无临时车位时，在入库前是否提示“只剩VIP车位”语音，入库判断条件范围为(0,该值) 默认值：0
        /// 有效值：0-不提示该语音；30-打开该功能，提示该语音,30表示入库前存车自动步进值
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回默认值
        /// </returns>
        private Dictionary<int, int> GetVIPSoundFlag()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                dictReturn.Add(nCounter, GetNodeValueInt(nCounter, m_strVIPSoundFlagNode));
            }
            return dictReturn;
        }

        /// <summary>
        /// 获取移动APP平板应用用户取车优先级（默认值：0无优先级）
        /// </summary>
        /// <returns></returns>
        private int GetAPPPriorityId()
        {
            int nReturn = GetNodeValueInt(m_strAPPPriorityIdNode);
            if (0 == nReturn)
            {
                return DEFAULT_VALUE;
            }
            return nReturn;
        }
        
        /// <summary>
        /// 获取显示器（LED）模块参数链表
        /// </summary>
        /// <returns></returns>
        private void GetDisplayerModulePara(ref Dictionary<int, string> dicClassName, ref Dictionary<int, object[]> dicPara)
        {
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dicClassName.ContainsKey(nCounter) || dicPara.ContainsKey(nCounter))
                {
                    continue;
                }

                string strClassName = string.Empty;
                object[] arrPara = null;
                bool nDisplayerFlag = GetDisplayerPara_Common(nCounter, ref strClassName, ref arrPara);
                if (false == nDisplayerFlag)
                {
                    GetDisplayerPara_Common(ref strClassName, ref arrPara);
                }
                dicClassName.Add(nCounter, strClassName);
                dicPara.Add(nCounter, arrPara);
            }
        }
        
        /// <summary>
        /// 获取显示器（LED）模块参数链表
        /// </summary>
        /// <returns></returns>
        private void GetDisplayerPara_Common(ref string strClassName, ref object[] arrPara)
        {
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strCommonSettingsNode + m_strDisplayerParaNode);

            if (null == xmlNode)
            {
                return;
            }

            int nDisplayerFlag = GetNodeValueInt(m_strCommonSettingsNode + m_strDisplayerFlagNode);
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            foreach (XmlNode childNode in childNodesList)
            {
                if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                {
                    continue;
                }

                string strCommType = childNode.SelectSingleNode("ID").InnerText.Trim();
                int nCommType = 0;
                bool bIsUnsignedNumber = CBaseMethods.MyBase.StringToUInt32(strCommType, out nCommType);
                if (bIsUnsignedNumber && nDisplayerFlag == nCommType)
                {
                    strClassName = childNode.SelectSingleNode("class").InnerText.Trim();

                    //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                    if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                    {
                        arrPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 获取显示器（LED）模块参数链表
        /// </summary>
        /// <returns></returns>
        private bool GetDisplayerPara_Common(int nPLCID, ref string strClassName, ref object[] arrPara)
        {
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strDisplayerParaNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strDisplayerParaNode;
            string strDisplayerFlagNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + m_strDisplayerFlagNode;

            XmlNode xmlNode = m_xmlData.SelectSingleNode(strDisplayerParaNode);

            if (null == xmlNode)
            {
                return false;
            }
            int nDisplayerFlag = 0;
            if (!m_dicDisplayerFlag.ContainsKey(nPLCID))
            {
                return false;
            }
            nDisplayerFlag = m_dicDisplayerFlag[nPLCID];
            XmlNodeList childNodesList = xmlNode.ChildNodes;
            foreach (XmlNode childNode in childNodesList)
            {
                if (null == childNode.SelectSingleNode("ID") || null == childNode.SelectSingleNode("class"))
                {
                    continue;
                }

                string strCommType = childNode.SelectSingleNode("ID").InnerText.Trim();
                int nCommType = 0;
                bool bIsUnsignedNumber = CBaseMethods.MyBase.StringToUInt32(strCommType, out nCommType);
                if (bIsUnsignedNumber && nDisplayerFlag == nCommType)
                {
                    strClassName = childNode.SelectSingleNode("class").InnerText.Trim();

                    //若有参数，则将其分割成字符串数组，用于实例化时所用的参数
                    if (null != childNode.SelectSingleNode("para") && !string.IsNullOrWhiteSpace(childNode.SelectSingleNode("para").InnerText))
                    {
                        arrPara = childNode.SelectSingleNode("para").InnerText.Trim().Split(',');
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取显示器（LED）模块标志。
        /// </summary>
        /// <returns>
        /// 成功：返回配置的值
        /// 失败：返回0
        /// </returns>
        private Dictionary<int, int> GetDisplayerFlag()
        {
            Dictionary<int, int> dictReturn = new Dictionary<int, int>();
            for (int nCounter = 1; nCounter <= PLCCount; nCounter++)
            {
                if (dictReturn.ContainsKey(nCounter))
                {
                    continue;
                }
                int nDisplayerFlag = 0;
                nDisplayerFlag = GetNodeValueInt(nCounter, m_strDisplayerFlagNode);
                if (0 == nDisplayerFlag)
                {
                    nDisplayerFlag = GetNodeValueInt(m_strCommonSettingsNode + m_strDisplayerFlagNode);
                }
                dictReturn.Add(nCounter, nDisplayerFlag);
            }
            return dictReturn;
        }

        #region 获取配置文件中参数值的函数集
        /// <summary>
        /// 获取节点下的int类型参数值，成功：返回配置的参数值(值允许配置的范围：0~2147483647)，失败：返回 0
        /// </summary>
        /// <param name="strNodeName">节点名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败或配置的值为空：返回 0
        /// </returns>
        private int GetNodeValueInt(string strNodeName)
        {
            int nAttributeValue = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNodeName);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.InnerText.Trim();
                CBaseMethods.MyBase.StringToUInt32(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }

        /// <summary>
        /// 获取各PLC下各节点下的int类型参数值，成功：返回配置的参数值(值允许配置的范围：0~2147483647)，失败：返回 0
        /// </summary>
        /// <param name="nPLCID">PLC ID</param>
        /// <param name="strNodeName">节点名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败或配置的值为空：返回 0
        /// </returns>
        private int GetNodeValueInt(int nPLCID, string strNodeName)
        {
            string strPLCSettingNode = "PLC" + nPLCID.ToString();
            string strNode = m_strPLCSettingsNode + "/" + strPLCSettingNode + strNodeName;
            int nAttributeValue = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNode);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.InnerText.Trim();
                CBaseMethods.MyBase.StringToUInt32(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }

        /// <summary>
        /// 获取节点下的Int16类型参数值，成功：返回配置的参数值(值允许配置的范围：0~32767)，失败：返回 0
        /// </summary>
        /// <param name="strNodeName">节点名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 0
        /// </returns>
        private Int16 GetNodeValueInt16(string strNodeName)
        {
            Int16 nAttributeValue = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNodeName);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.InnerText.Trim();
                CBaseMethods.MyBase.StringToUInt16(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }
      
        /// <summary>
        /// 获取节点下的string类型参数值，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <param name="strNodeName">节点名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 null
        /// </returns>
        private string GetNodeValueStr(string strNodeName)
        {
            string strAttributeValue = string.Empty;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNodeName);
            if (xmlNode != null)
            {
                strAttributeValue = xmlNode.InnerText.Trim();
            }
            return strAttributeValue;
        }

        /// <summary>
        /// 获取EquipInfo/value节点下指定属性的值，成功：返回配置的参数值，失败：返回 0
        /// </summary>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 0
        /// </returns>
        private int GetEquipInfoATTRInt(string strAttribute)
        {
            int nAttributeValue = 0;
            if (0 == strAttribute.Trim().Length)
            {
                return nAttributeValue;
            }
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strEquipInfoNode);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.Attributes[strAttribute].Value.Trim();
                CBaseMethods.MyBase.StringToUInt32(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }

        /// <summary>
        /// 获取EquipInfo/value节点下指定属性的值，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 null
        /// </returns>
        private string GetEquipInfoATTRStr(string strAttribute)
        {
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strEquipInfoNode);
            if (null == xmlNode)
            {
                return string.Empty;
            }
            return xmlNode.Attributes[strAttribute].Value.Trim();

        }

        /// <summary>
        /// 获取DBAddress/value节点下int类型指定属性的值，成功：返回配置的参数值，失败：返回 0
        /// </summary>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 0
        /// </returns>
        private int GetDBAddressATTRInt(string strAttribute)
        {
            int nAttributeValue = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strDBAddressNode);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.Attributes[strAttribute].Value.Trim();
                CBaseMethods.MyBase.StringToUInt32(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }

        /// <summary>
        /// 获取DBAddress/value节点下string类型指定属性的值，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 null
        /// </returns>
        private string GetDBAddressATTRStr(string strAttribute)
        {
            string strAttributeValue = null;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(m_strDBAddressNode);
            if (null != xmlNode)
            {
                strAttributeValue = xmlNode.Attributes[strAttribute].Value.Trim();
            }
            return strAttributeValue;
        }

        /// <summary>
        /// 获取string类型指定节点指定属性的值，成功：返回配置的参数值，失败：返回 null
        /// </summary>
        /// <param name="strNodeName">节点全路径</param>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 null
        /// </returns>
        private string GetNodeATTRStr(string strNodeName, string strAttribute)
        {
            string strAttributeValue = null;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNodeName);
            if (null != xmlNode)
            {
                strAttributeValue = xmlNode.Attributes[strAttribute].Value.Trim();
            }
            return strAttributeValue;
        }

        /// <summary>
        /// 获取int类型的指定节点指定属性值，成功：返回配置的参数值，失败：返回 0
        /// </summary>
        /// <param name="strNodeName">节点全路径</param>
        /// <param name="strAttribute">属性名称</param>
        /// <returns>
        /// 成功：返回配置的参数值
        /// 失败：返回 0
        /// </returns>
        private int GetNodeATTRInt(string strNodeName, string strAttribute)
        {
            int nAttributeValue = 0;
            XmlNode xmlNode = m_xmlData.SelectSingleNode(strNodeName);
            if (null != xmlNode)
            {
                string strInnerText = xmlNode.Attributes[strAttribute].Value.Trim();
                CBaseMethods.MyBase.StringToUInt32(strInnerText, out nAttributeValue);
            }
            return nAttributeValue;
        }

        /// <summary>
        /// 根据节点全路径获取布尔类型标识，配置值为1返回true，配置值为非1或不配置返回false
        /// </summary>
        /// <param name="strNodePath"></param>
        /// <returns></returns>
        private bool GetBoolValue(string strNodePath)
        {
            int nReturn = GetNodeValueInt(strNodePath);
            if (1 == nReturn)
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 根据PLC ID和节点名字获取布尔类型标识，配置值为1返回true，配置值为非1或不配置返回false
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <param name="strNodeName"></param>
        /// <returns></returns>
        private bool GetBoolValue(int nPLCID, string strNodeName)
        {
            int nReturn = GetNodeValueInt(nPLCID, strNodeName);
            if (1 == nReturn)
            {
                return true;
            }
            return false;
        }
        #endregion

        #endregion 私有方法

        #region 读取及设置用户管理配置

        private static readonly string userXmlPath = AppDomain.CurrentDomain.BaseDirectory + @"\Config";      
        private string allowUserSetFixLimit;
        //如果不存在该文档，则构建;存在，则读取
        private void userLimitInit() 
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                string fileName = userXmlPath + @"\User.xml";
                if (File.Exists(fileName))
                {
                    xmlDoc.Load(fileName);
                    XmlNode root = xmlDoc.SelectSingleNode("User");
                    XmlNodeList xnl = root.ChildNodes;
                    foreach (XmlNode node in xnl)
                    {
                        if (node.Name == "allowOut")
                        {
                            allowUserSetFixLimit = node.InnerText.Trim();
                            break;
                        }
                    }
                }
                else
                {
                    allowUserSetFixLimit = "0";
                    //添加声明结点
                    XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
                    xmlDoc.AppendChild(node);
                    //添加根节点
                    XmlNode root = xmlDoc.CreateElement("User");
                    xmlDoc.AppendChild(root);
                    //添加元术节点
                    XmlNode node1 = xmlDoc.CreateNode(XmlNodeType.Element, "allowOut", null);
                    node1.InnerText = "0";
                    root.AppendChild(node1);
                    try
                    {
                        xmlDoc.Save(fileName);
                    }
                    catch (Exception ex)
                    {
                        CLOGException.Trace("create user xml exception. " + ex.ToString());
                    }

                }
            }
            catch (Exception ec)
            {
                CLOGException.Trace(ec.ToString());
            }
        }
        public string AllowUserSetFixGetCarLimit 
        {
            get 
            {
                if (allowUserSetFixLimit == null) 
                {
                    userLimitInit();
                }
                return allowUserSetFixLimit; 
            }
            private set 
            {                
                allowUserSetFixLimit = value;
            }
        }

        /// <summary>
        /// 修改节点值
        /// </summary>
        /// <param name="value"></param>
        public int ModifyUserGetFixCarLimit(string value) 
        {            
             allowUserSetFixLimit = value.Trim();
             try
             {
                 XmlDocument xmlDoc = new XmlDocument();
                 string fileName = userXmlPath + @"\User.xml";
                 if (File.Exists(fileName))
                 {
                     xmlDoc.Load(fileName);
                     XmlNode root = xmlDoc.SelectSingleNode("User");
                     XmlNodeList xnl = root.ChildNodes;
                     foreach (XmlNode node in xnl)
                     {
                         if (node.Name == "allowOut")
                         {
                             node.InnerXml = value;
                             xmlDoc.Save(fileName);
                             return 1;
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 CLOGException.Trace(ex.ToString());
             }
             return 0;
        }
        #endregion
    }
}
