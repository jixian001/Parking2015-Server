using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace TypedefLib
{
    /// <summary>
    /// 车厅中设备参数结构体
    /// </summary>
    public struct struHallEquips
    {
        /// <summary>
        /// 库区号
        /// </summary>
        public int nWareHouseID;

        /// <summary>
        /// 车厅号
        /// </summary>
        public int nHallID;

        /// <summary>
        /// 车厅名字，LED显示时用
        /// </summary>
        public string strHallName;

        /// <summary>
        /// 刷卡器串口号
        /// </summary>
        public int nICCardComID;

        /// <summary>
        /// LED串口地址
        /// </summary>
        public int nLEDAddress;

        /// <summary>
        /// LED即时文本有效时间
        /// </summary>
        public int nLEDTotalStay;

        /// <summary>
        /// 音频设备号
        /// </summary>
        public string strAudioDeviceDesc;

        /// <summary>
        /// 暂不用
        /// </summary>
        public string strLEDComID;

        /// <summary>
        /// 顾客显示屏连接串口号
        /// </summary>
        public int nCustomerLEDComID;

        /// <summary>
        /// 车牌识别IP地址
        /// </summary>
        public string strPlateIP;

        /// <summary>
        /// 车牌识别端口号
        /// </summary>
        public int nPlatePort;

        /// <summary>
        /// 车牌识别所需用户名
        /// </summary>
        public string strPlateUserName;

        /// <summary>
        /// 车牌识别所需密码
        /// </summary>
        public string strPlatePassword;
    }

    /// <summary>
    /// 设备状态信息
    /// </summary>
    public struct struEquipInformation
    {
        /// <summary>
        /// 设备状态表
        /// </summary>
        public List<object> objEquipStatusTBL;
//         /// <summary>
//         /// 1-库区标识
//         /// </summary>
//         public int nPLCID;
//         /// <summary>
//         /// 2-设备标识
//         /// </summary>
//         public int nEquipID;
// 
//         /// <summary>
//         /// 3-IC卡标识
//         /// </summary>
//         public string strICCardID;
// 
//         /// <summary>
//         /// 4-作业类型
//         /// </summary>
//         public int nTaskType;
// 
//         /// <summary>
//         /// 5-设备是否可用
//         /// </summary>
//         public bool bIsAvailable;
// 
//         /// <summary>
//         /// 6-设备的工作模式：1-维修模式，2-手动模式，3-单机自动，4-在线自动
//         /// </summary>
//         public int nDeviceMode;
// 
//         /// <summary>
//         /// 7-设备类型：1-车厅，2-ETV
//         /// </summary>
//         public int nDeviceType;
//         
//         /// <summary>
//         /// 8-车厅类型：1-进车厅，2-出车厅，3-进出车厅
//         /// </summary>
//         public int nHallType;
//         
//         /// <summary>
//         /// 9-当前节点
//         /// </summary>
//         public int nCurrentNode;
// 
//         /// <summary>
//         /// 10-前一节点
//         /// </summary>
//         public int nPrevNode;
// 
//         /// <summary>
//         /// 11-弹出队列的当前节点
//         /// </summary>
//         public int nQueueCurrentNode;
// 
//         /// <summary>
//         /// 12-弹出队列的前一节点
//         /// </summary>
//         public int nQueuePrevNode;
// 
        /// <summary>
        /// 13-当前设备的等待报文
        /// </summary>
        public List<string> lstWaitTelegram;
    }

    /// <summary>
    /// 车位信息
    /// </summary>
    public struct struCarLocInformation
    {
        /// <summary>
        /// 1-库区标识
        /// </summary>
        public int nPLCID;

        /// <summary>
        /// 2-IC卡标识
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// 3-车位地址
        /// </summary>
        public string strCarLocAddr;

        /// <summary>
        /// 4-车位状态：空闲-1，占用-2，正在入库（或临时取物存车）-3，正在出库-4，临时取物出库-5，库内搬移-6
        /// </summary>
        public int nCarLocStatus;

        /// <summary>
        /// 5-车位类型：正常车位-1，车厅-2，禁用车位-3
        /// </summary>
        public int nCarLocType;

        /// <summary>
        /// 6-车位尺寸
        /// </summary>
        public string strCarLocSize;

        /// <summary>
        /// 7-车辆轴距
        /// </summary>
        public int nCarWheelbase;

        /// <summary>
        /// 8-车辆尺寸
        /// </summary>
        public string strCarSize;

        /// <summary>
        /// 9-是否需要倒车
        /// </summary>
        public int nIsBackup;

        /// <summary>
        /// 10-入库时间
        /// </summary>
        public DateTime dtCarInTime;

        /// <summary>
        /// 11-车位优先级
        /// </summary>
        public int nPriorityID;

        /// <summary>
        /// 12-所在区域
        /// </summary>
        public int nRegion;

        /// <summary>
        /// 13-车牌号
        /// </summary>
        public string strPlateNum;

        /// <summary>
        /// 14-车辆图片
        /// </summary>
        public string strCarPicture;

        /// <summary>
        /// 15-车辆朝向
        /// </summary>
        public int nDirection;
    }

    /// <summary>
    /// IC卡信息
    /// </summary>
    public struct struICCardInformation
    {
        /// <summary>
        /// 1-逻辑卡号
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// 2-物理卡号
        /// </summary>
        public string strPhyICCardID;

        /// <summary>
        /// 3-IC卡类型：临时-1，定期卡-2，固定车位卡-3
        /// </summary>
        public int nICType;

        /// <summary>
        /// 4-IC卡状态：正常-1，挂失-2，注销-3
        /// </summary>
        public int nICStatus;

        /// <summary>
        /// 5-分配车位地址
        /// </summary>
        public string strCarLocAddr;

        /// <summary>
        /// 6-库区号
        /// </summary>
        public int nWarehouse;

        /// <summary>
        /// 7-计费类型
        /// </summary>
        public int nTariffID;

        /// <summary>
        /// 8-取车优先级
        /// </summary>
        public int nPriorityID;
    }

    /// <summary>
    /// 取车排队信息
    /// </summary>
    public struct struQueueInformation
    {
        /// <summary>
        /// 1-库区标识
        /// </summary>
        public int nPLCID;

        /// <summary>
        /// 2-设备标识
        /// </summary>
        public int nEquipID;

        /// <summary>
        /// 3-IC卡标识
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// 4-刷卡次数
        /// </summary>
        public int nSwipeCount;

        /// <summary>
        /// 5-取车优先级
        /// </summary>
        public int nPriorityID;
    }
    
    /// <summary>
    /// 车主信息
    /// </summary>
    public struct struCustomerInfo
    {
        /// <summary>
        /// 用户姓名
        /// </summary>
        public string strName;

        /// <summary>
        /// IC卡号
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// IC卡状态
        /// </summary>
        public int nICCardStatus;

        /// <summary>
        /// IC卡类型
        /// </summary>
        public int nICCardType;

        /// <summary>
        /// 库区号
        /// </summary>
        public int nWareHouse;

        /// <summary>
        /// 车位地址
        /// </summary>
        public string strCarPOSN;

        /// <summary>
        /// 移动电话
        /// </summary>
        public string strMobile;

        /// <summary>
        /// 车牌号
        /// </summary>
        public string strLicPlteNbr;

        /// <summary>
        /// 家庭电话
        /// </summary>
        public string strTelphone;

        /// <summary>
        /// 住址
        /// </summary>
        public string strAddress;

        /// <summary>
        /// 收费表ID
        /// </summary>
        public int nTariffID;

        /// <summary>
        /// 优先级ID
        /// </summary>
        public int nPriorityID;

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime dtStartTime;

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime dtDeadLine;
    }

    /// <summary>
    /// 操作数据库表格结构
    /// </summary>
    public struct struTableDto
    {
        /// <summary>
        /// 数据库表格实例
        /// </summary>
        public object tableDto;
        /// <summary>
        /// 数据库操作枚举类型值
        /// </summary>
        public int accessType;

        /// <summary>
        /// 带二参构造函数（_accessType->1：插入数据表格标志；2：更新修改标志；3：删除标志）
        /// </summary>
        /// <param name="_tableDto"></param>
        /// <param name="_accessType">1：插入数据表格标志；2：更新修改标志；3：删除标志</param>
        public struTableDto(object _tableDto, int _accessType)
        {
            tableDto = _tableDto;
            accessType = _accessType;
        }
    }

    /// <summary>
    /// 收费管理信息
    /// </summary>
    public struct struBillInfo
    {
        /// <summary>
        /// IC卡号
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// IC卡类型
        /// </summary>
        public int nICCardType;

        /// <summary>
        /// 库区号
        /// </summary>
        public int nWareHouse;

        /// <summary>
        /// 出车车厅 
        /// </summary>
        public int nHallID;

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime dtStartTime;

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime dtEndTime;

        /// <summary>
        /// 计算时间（天）
        /// </summary>
        public string strCalculateDays;
       
        /// <summary>
        /// 计费ID号
        /// </summary>
        public int nTariffID;

        /// <summary>
        /// 收费类型
        /// </summary>
        public int nFeeType;

        /// <summary>
        /// 收费标准
        /// </summary>
        public float fTariffNorm;

        /// <summary>
        /// 应缴费用
        /// </summary>
        public float fPayableFee;

        /// <summary>
        /// 实收金额
        /// </summary>
        public float fActualFee;

        /// <summary>
        /// 操作员
        /// </summary>
        public string strOptCode;
    }

    /// <summary>
    /// 客户端使用参数
    /// </summary>
    public struct struClientPara
    {
        /// <summary>
        /// // 获取所有ETV或TV设备号:键值对为PLC ID（int）和所有ETV或TV设备号链表（List 《int》 ）的哈希表
        /// </summary>
        public Dictionary<int, List<int>> dictETVOrTVDeviceID;

        /// <summary>
        /// //获取所有车厅设备号：键值对为PLC ID（int）和所有车厅设备号链表（List《int》 ）的哈希表
        /// </summary>
        public Dictionary<int, List<int>> dictHallDeviceID;

        /// <summary>
        /// // 是否有计费功能标志，有计费返回1，无计费返回0
        /// </summary>
        public bool bBillingFlag;

        /// <summary>
        /// // 是否需要车牌号和车辆图片
        /// </summary>
        public bool bCarImageFlag;

        /// <summary>
        /// // 本项目车位尺寸列表 111(长 * 宽 * 高)
        /// </summary>
        public List<string> lstCarSizes;

        /// <summary>
        /// //"中集天达智能车库管理系统";
        /// </summary>
        public string strClientTitle;

        /// <summary>
        /// //"中集天达智能车库收费管理系统";
        /// </summary>
        public string strBillingTitle;

        /// <summary>
        /// 数据库连接标识
        /// </summary>
        public string strDBConnectionInfo;

        /// <summary>
        /// //"SELECT warehouse,carlocaddr,carloctype,carlocstatus,iccode From dbo.carlocation";
        /// </summary>
        public string strSQLQueryNOTECarPOSN;

        /// <summary>
        /// //"SELECT faultdescp,warehouse,devicecode,faultaddress,color,isable From dbo.devicefault";
        /// </summary>
        public string strSQLQueryNOTEDeviceFault;

        /// <summary>
        /// //"SELECT devicecode,warehouse,deviceaddr,devicemode,devicetype,isavailable From dbo.devicestatus";
        /// </summary>
        public string strSQLQueryNOTEDeviceStatus;

        /// <summary>
        /// // "系统维护,系统配置,用户管理,缴费管理,操作员管理,查询统计,临时取物,手动指令";//
        /// </summary>
        public string strAllOptPermission; 

        /// <summary>
        /// //255//  管理者: 2
        /// </summary>
        public int ManagerTopPRMSN;

        /// <summary>
        /// //103// 操作员： 3
        /// </summary>
        public int OperatorTopPRMSN;

        /// <summary>
        /// //111// 计费人员: 4
        /// </summary>
        public int TollKeeperTopPRMSN;

        /// <summary>
        /// //103// // 其他：5
        /// </summary>
        public int OtherTopPRMSN;

        /// <summary>
        /// //239// 天达维护人员: 1;    按位与结果;1111 1111
        /// </summary>
        public int CIMCWorkerTopPRMSN;

        /// <summary>
        /// //在各个PLC下配置：(1, 102, 4, 12), "CVerticalPanel")当前项目的库号和车位排列列表
        /// </summary>
        public List<struCarPSONLayoutInfo> lstPanelLayoutInfo;

        /// <summary>
        /// //当前项目的库号列表
        /// </summary>
        public List<object> lstPLCID;

        /// <summary>
        /// // 最大车辆尺寸
        /// </summary>
        public string strCarMaxSize;

        /// <summary>
        /// 刷卡器参数
        /// </summary>
        public object[] objReaderPara;

        /// <summary>
        /// 车辆空闲时掉头旋转标识
        /// </summary>
        public bool VehicleRotationFlag;

        /// <summary>
        /// 客户端每个已停车的车位或固定卡车位上显示的内容
        /// </summary>
        public int ShowInfoFlag;

        /// <summary>
        /// 获取所有车厅配置：键值对为PLC ID（int）和所有车厅配置（Dictionary《int, struHallEquips》 键：车厅ID，值：车厅下参数struHallEquips）的字典
        /// </summary>
        public Dictionary<int, Dictionary<int, struHallEquips>> DicHallIDDictionary;

        /// <summary>
        /// 6113定制需求：替换IC卡但是逻辑卡号不变仍然使用工号，默认值：0
        /// </summary>
        public bool ReplacePhyCardIDFlag;

    }

    /// <summary>
    /// 操作数据库表格结构
    /// </summary>
    public struct struCarPSONLayoutInfo
    {
        /// <summary>
        /// X:库号；Y:边排列信息；Width:当前库总列数；Height:当前库总层数
        /// </summary>
        public Rectangle rectInfo;
        /// <summary>
        /// 当前库车位状态布局类名称
        /// </summary>
        public string strPanelName;

        /// <summary>
        /// 带二参构造函数(_rectInfo->X:库号；Y:边排列信息；Width:当前库总列数；Height:当前库总层数)
        /// </summary>
        /// <param name="_rectInfo"></param>
        /// <param name="_strPanelName"></param>

        public struCarPSONLayoutInfo(Rectangle _rectInfo, string _strPanelName)
        {
            rectInfo = _rectInfo;
            strPanelName = _strPanelName;
        }
    }

    /// <summary>
    /// IC卡扇区的数据结构
    /// </summary>
    public struct struICCardData
    {
        /// <summary>
        /// 计费类型
        /// </summary>
        public EnmFeeType enmFeeType;

        /// <summary>
        /// 逻辑卡号
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// 计费开始时间
        /// </summary>
        public DateTime dtFeeStartTime;

        /// <summary>
        /// 计费结束时间
        /// </summary>
        public DateTime dtFeeEndTime;
    }

    /// <summary>
    /// 移动APP应用用户输入的数据结构
    /// </summary>
    [System.Runtime.Serialization.KnownType(typeof(struAPPData))]
    public struct struAPPData
    {
        /// <summary>
        /// 作业类型
        /// </summary>
        public EnmTaskType enmTaskType;

        /// <summary>
        /// 用户选择枚举值
        /// </summary>
        public EnmQueryFlag enmQueryType;
     
        /// <summary>
        /// 车库管理系统具体项目名称
        /// </summary>
        public string strProjectName;

        /// <summary>
        /// 逻辑卡号
        /// </summary>
        public string strICCardID;

        /// <summary>
        /// 车牌号
        /// </summary>
        public string strLicPlteNbr;

        /// <summary>
        /// 移动电话
        /// </summary>
        public string strMobile;

        /// <summary>
        /// 库区号
        /// </summary>
        public int nWareHouseID;

        /// <summary>
        /// 车厅号
        /// </summary>
        public int nHallID;
    }
}
