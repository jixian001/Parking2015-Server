using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DatabaseManagementLib;
using TypedefLib;

namespace WCFServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IPushService”。
    /// <summary>
    /// 推送消息服务接口
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPushCallback), SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(object))]
    [ServiceKnownType(typeof(CCarLocationDto))]
    [ServiceKnownType(typeof(CDeviceStatusDto))]
    [ServiceKnownType(typeof(CDeviceFaultDto))]
    public interface IPushService : IDisposable
    {
        [OperationContract(IsOneWay = true)]
        void Register(string strHost);

        [OperationContract(IsOneWay = true)]
        new void Dispose();

        [OperationContract]
        bool CheckService();

        #region 对IC卡的处理
        /// <summary>
        /// 修改制过卡的逻辑卡号
        /// 成功：
        /// EnmFaultType.Success;//修改成功
        /// 失败：
        /// EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        /// EnmFaultType.FailToUpdate;//更新数据库失败
        /// 已经注释掉：EnmFaultType.FailToWriteICCard;//写入IC卡失败
        /// 已经注释掉：EnmFaultType.FailConnection;//连接读卡器失败
        /// </summary>
        /// <param name="strPhysicalCardID"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyICCardID(string strPhysicalCardID, string strData);


        /// <summary>
        /// 替换IC卡
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        ///       EnmFaultType.BoundUser;//新的IC卡有绑定用户
        ///       EnmFaultType.TaskOnICCard;//旧卡或新卡正在作业
        ///       EnmFaultType.Wait;//旧卡或新卡正在排队取车
        ///       EnmFaultType.CarInGarage;//新的IC卡有车存在车库
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="strPhysicalCardID"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyICCard(string strOldICCardID, string strNewICCardID);

        /// <summary>
        /// 替换IC卡
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        ///       EnmFaultType.TaskOnICCard;//正在作业
        ///       EnmFaultType.Wait;//正在排队取车
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="strPhysicalCardID"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyPhyCard(string strPhysicalCardID, string strICCardID);

        #endregion 对IC卡的处理

        #region 对车位信息的管理
        /// <summary>
        /// 手动出库
        /// 返回值：
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;//获取车位信息失败
        ///       EnmFaultType.HallEquip;//当前车位是车厅设备
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.NotNormalCarPOSN;//不是正常车位
        ///       EnmFaultType.CarInGarage;//当前车位无车，无车出库
        ///       EnmFaultType.Null;//空闲车位
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarPOSNAddr"></param>
        [OperationContract(Name = "ManualVEHExit")]
        EnmFaultType ManualVEHExit(int nWareHouse, string strCarPOSNAddr);

        /// <summary>
        /// 手动入库
        /// 返回值：
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        ///       EnmFaultType.Fail;//获取车位信息失败
        ///       EnmFaultType.NotNormalCarPOSN;//不是正常车位
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.HallEquip;//当前车位是车厅设备
        ///       EnmFaultType.CarInGarage;//当前车位有车，不允许将车放到该车位
        ///       EnmFaultType.TaskOnICCard; //当前卡有车存在车库
        ///       EnmFaultType.OverCarInSize;// 入库车辆尺寸超限
        /// </summary>
        /// <param name="carPOSNTable"></param>
        [OperationContract(Name = "ManualVEHEntry")]
        EnmFaultType ManualVEHEntry(CCarLocationDto carPOSNTable);

        /// <summary>
        /// 手动挪移
        /// 返回值：
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.NotFoundCarPOSN;//没有找到源车位或目的车位
        ///       EnmFaultType.NotNormalCarPOSN;//源车位或目的车位不是正常车位
        ///       EnmFaultType.NoCarInGarage;//没有车存在源车位
        ///       EnmFaultType.CarInGarage;//有车存在目的车位
        ///       EnmFaultType.TaskOnICCard;//目的车位的车辆正在作业
        ///       EnmFaultType.NoICCardInfo;//源车位的IC卡没有制卡
        ///       EnmFaultType.FixedCarPOSN;//目的车位是其他车主的固定车位，临时卡或定期卡无法挪移到固定车位卡的车位上
        ///       EnmFaultType.NotMatch;//目的车位是其他车主的固定车位
        ///       EnmFaultType.OverCarInSize;//目的车位尺寸不满足车辆尺寸
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strSrcCarPOSNAddr"></param>
        /// <param name="strDestCarPOSNAddr"></param>
        [OperationContract(Name = "ManualMoveVEH")]
        EnmFaultType ManualMoveVEH(int nWareHouse, string strSrcCarPOSNAddr, string strDestCarPOSNAddr);

        /// <summary>
        /// 修改车位类型（禁用或启用车位）
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.NotFoundCarPOSN; //没有找到指定车位
        ///     EnmFaultType.NotAllowed;//不允许修改类型
        ///     EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarPOSNAddr"></param>
        /// <param name="enmCarPOSNType"></param>
        [OperationContract(Name = "ModifyCarPOSNType")]
        EnmFaultType ModifyCarPOSNType(int nWareHouse, string strCarPOSNAddr, EnmLocationType enmCarPOSNType);

        /// <summary>
        /// 一键出库
        ///  成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.Fail; //一键出库失败
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType InitAllCarPOSN(int nWareHouse);

        /// <summary>
        /// 一键禁用/启用
        ///  成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.Fail; //一键禁用/启用失败
        /// </summary>
        /// <param name="enmCarPOSNType"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyAllCarPOSNType(int nWareHouse, EnmLocationType enmCarPOSNType);
         
        /// <summary>
        /// 修改车位状态（空闲、占用、入库、出库、挪移、临时取物车位）
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.NotFoundCarPOSN; //没有找到指定车位
        ///     EnmFaultType.NotAllowed;//不允许修改类型
        ///     EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarPOSNAddr"></param>
        /// <param name="enmCarPOSNStatus"></param>
        [OperationContract]
        EnmFaultType ModifyCarPOSNStatus(int nWareHouse, string strCarPOSNAddr, EnmLocationStatus enmCarPOSNStatus);
        #endregion 对车位信息的管理

        #region 对设备信息的管理

        /// <summary>
        /// 设置设备的模式（全自动或停止等模式）
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.NotFoundEquip;//没有找到指定设备
        ///       EnmFaultType.NotAutomatic;//非全自动模式不能启用
        ///       EnmFaultType.NotAllowed;//设备当前状态与要修改的状态相同
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        /// 失败：EnmFaultType.InvalidWareHouseID;//无效库区号
        ///       EnmFaultType.Null;// 下一取车队列的传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;// 下一取车队列的IC卡没有制卡
        ///       EnmFaultType.LossORCancel;// 下一取车队列的IC卡注销或挂失
        ///       EnmFaultType.NotMatch;// 下一取车队列的IC卡类型不正确
        ///       EnmFaultType.NoCarInGarage;// 下一取车队列的用户没有车存在库内
        ///       EnmFaultType.FailToSendTelegram;// 下一取车队列发送报文失败
        ///       EnmFaultType.WorkQueueNotEmpty;// 启用成功，正在为取车排队的车主取车，请稍后片刻
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceID"></param>
        /// <param name="nIsavailable"></param>
        [OperationContract]
        EnmFaultType SetDeviceMode(int nWareHouse, int nDeviceID, int nIsavailable);

        /// <summary>
        ///  设置车厅类型
        ///  成功：EnmFaultType.Success;
        ///  失败：EnmFaultType.NotFoundEquip;//没有找到指定的车厅
        ///        EnmFaultType.WorkQueueNotEmpty;//有人排队取车，不允许修改车厅类型
        ///        EnmFaultType.FailToUpdate;//修改数据库失败
        ///        EnmFaultType.InvalidWareHouseID;//无效库区号
        ///        EnmFaultType.FailToSendTelegram; //发送报文失败
        ///        EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <param name="enmHallType"></param>
        [OperationContract]
        EnmFaultType SetHallType(int nWareHouse, int nHallID, EnmHallType enmHallType);
         
        /// <summary>
        /// 判断该库区是否有作业
        /// add by wanxiaona20150806
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns>true:有作业  false：无作业</returns>
        [OperationContract]
        bool IsHasTask(int nWareHouse);
        #endregion 对设备信息的管理

        #region 对报文的管理
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
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.Wait;//已经将您加到取车队列，请排队等候
        /// EnmFaultType.Add; //前面有人正在作业，已经将您加到取车队列，请排队等候
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        [OperationContract]
        EnmFaultType TmpFetch(string strICCardID, int nWareHouse, int nHallID);

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
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strHallID">出库车厅</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType VehicleExit(int nWareHouse, string strSrcPOSN, string strHallID);

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
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strHallID">出库车厅</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType VehicleExitForBilling(string strICCardID, int nHallID);
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
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strDestPOSN">目的车位地址</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType VehicleMove(int nWareHouse, string strSrcPOSN, string strDestPOSN);

        /// <summary>
        /// 移动设备
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
        /// EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.NotAvailable;      //设备不可接收指令;
        /// EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.NotAllowed;      //设备不可用;
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="strDestPOSN">目的地址</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType EquipMove(int nWareHouse, int nEquipID, string strDestPOSN);


        /// <summary>
        /// 处理下一作业
        /// 成功：EnmFaultType.Success;  
        /// 失败：EnmFaultType.Fail;//传入的设备状态表为空
        ///       EnmFaultType.InvalidWareHouseID;//无效库区号
        ///       EnmFaultType.Null;// 下一取车队列的传入的参数卡号为空
        ///       EnmFaultType.WorkQueueNotEmpty;// 启用成功，正在为取车排队的车主取车，请稍后片刻
        ///       EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType StartWorkQueue(CDeviceStatusDto deviceStatusTBL);

        /// <summary>
        /// 预约取车
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
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        [OperationContract]
        EnmFaultType VehicleExitSubscribe(string strICCardID, int nWareHouse, ref int nHallID);

        /// <summary>
        /// 车辆旋转（掉头）
        /// add by wanxiaona20150806
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
        /// EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.NotAvailable;      //设备不可接收指令;
        /// EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.NotAllowed;      //设备不可用;
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.Exception; //抛出异常
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType VehicleRotation(int nWareHouse);

        /// <summary>
        /// 车辆旋转（掉头）具体车辆
        /// add by wanxiaona20150806
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
        /// EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.NotAvailable;      //设备不可接收指令;
        /// EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.NotAllowed;      //设备不可用;
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.Exception; //抛出异常
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "VehicleRotationByAddr")]
        EnmFaultType VehicleRotation(int nWareHouse, string strCarPOSN);
        #endregion 对报文的管理

        #region 对故障处理的管理（手动作业）

        /// <summary>
        /// 手动完成作业
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;//手动完成失败
        ///       EnmFaultType.NotNormalCarPOSN;//不是正常车位
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.HallEquip;//当前车位是车厅设备
        ///       EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
        ///       EnmFaultType.TaskOnICCard;        //当前卡有车存在车库
        ///       EnmFaultType.FailToAllocETVorTV;//分配ETV或TV失败
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType HandCompleteTask(int warehouse, int code);

        /// <summary>
        /// 手动复位作业
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;//手动复位失败
        ///       EnmFaultType.NotFoundCarPOSN;//没有找到指定车位
        ///       EnmFaultType.HallEquip;//当前车位是车厅设备
        ///       NotNormalCarPOSN; //不是正常车位
        ///       EnmFaultType.OverCarInSize;// 入库车辆尺寸超限
        ///       EnmFaultType.FailToDelete;//删除数据库失败
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
        ///       EnmFaultType.FailToAllocETVorTV;//分配ETV或TV失败
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType HandResetTask(int warehouse, int code);

        [OperationContract]
        void InitDeviceFault(CDeviceFaultDto deviceFaultTBL);

        [OperationContract]
        int MuroDeviceTask(int warehouse, int code);

        #endregion 对故障处理的管理（手动作业）

        #region 对计费的管理

        /// <summary>
        /// 临时卡缴费出车和固定卡缴费
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.Null;//传入的参数卡号为空
        ///     EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        ///     EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///     EnmFaultType.NotMatch;//IC卡类型不正确
        ///     EnmFaultType.NotFoundCarPOSN;    //指定的源或目的车位不存在
        ///     EnmFaultType.NotFoundEquip;       //没有找到指定目的车厅
        ///     EnmFaultType.IsNotHallEquip;      //指定的目的地址不是车厅
        ///     EnmFaultType.HallEnter;         //车厅是进车厅不允许出车
        ///     EnmFaultType.NoCarInGarage;       //源车位没有车
        ///     EnmFaultType.NotAvailable;      //车厅设备不可接收指令
        ///     EnmFaultType.FailToSendTelegram; //发送报文失败
        ///     EnmFaultType.NotSameWareHouse;//刷卡库区与车所在库区不同
        ///     EnmFaultType.FailToInsert;//插入数据库失败
        ///     EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        ///     EnmFaultType.Wait;//已经将您加到取车队列，请排队等候
        ///     EnmFaultType.Add; //前面有人正在取车，已经将您加到取车队列，请排队等候
        ///     EnmFaultType.InvalidEquipID;//无效设备号
        ///     EnmFaultType.InvalidWareHouseID;//无效库区号
        ///     EnmFaultType.NotNormalCarPOSN;//源车位不是正常车位。      
        ///     EnmFaultType.FailToUpdate;//更新数据库失败
        ///     EnmFaultType.FailToInsert;//插入数据库失败
        ///     EnmFaultType.Fail; //计算截止日期失败
        ///     EnmFaultType.InvalidFeeType;//无效计费类型 
        ///     EnmFaultType.FailToFixBill;//当前卡已存车且IC卡已过期，请补交费用
        /// </summary>
        /// <param name="billInfo"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType PayFeesAndTakeCar(struBillInfo billInfo);

        #endregion 对计费的管理
    }
}
