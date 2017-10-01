using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TypedefLib
{
    /// <summary>
    /// 作业状态类型
    /// </summary>
    public enum EnmTaskType
    {
        /// <summary>
        /// 初始:0
        /// </summary>
        Init = 0,

        /// <summary>
        /// 存车作业:1
        /// </summary>
        EntryTask,

        /// <summary>
        /// 取车作业:2
        /// </summary>
        ExitTask,

        /// <summary>
        /// 临时取物:3
        /// </summary>
        TmpFetch,

        /// <summary>
        /// 库内挪移:4
        /// </summary>
        MoveCarTask,

        /// <summary>
        /// 移动设备:5
        /// </summary>
        MoveEquipTask,

        /// <summary>
        /// 车辆旋转:6
        /// </summary>
        VehRotationTask,

        /// <summary>
        /// 避让移动:7
        /// </summary>
        AvoidMove,

        /// <summary>
        /// 报文发送不成功，加入队列，
        /// 需重复发送的: 8
        /// </summary>
        RetrySend,

        /// <summary>
        /// 装载完成后强制避让
        /// </summary>
        ForceMove,
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public enum EnmSMGType
    {
        /// <summary>
        /// 车厅：1
        /// </summary>
        [EnumMember]
        Hall = 1,    

        /// <summary>
        /// ETV：2
        /// </summary>
        [EnumMember]
        ETV
    }
    
    /// <summary>
    /// 车厅类型
    /// </summary>
    public enum EnmHallType
    {
        /// <summary>
        /// 进车厅:1
        /// </summary>
        Entance = 1,

        /// <summary>
        /// 出车厅:2
        /// </summary>
        Exit,

        /// <summary>
        /// 进出两用车厅:3
        /// </summary>
        EnterOrExit,
    }

    /// <summary>
    /// 设备模式
    /// </summary>
    public enum EnmModel
    {
        Init=0,
        /// <summary>
        /// 维修模式:1
        /// </summary>
        Maintenance = 1,   

        /// <summary>
        /// 手动模式:2
        /// </summary>
        Manual,  

        /// <summary>
        /// 单机自动:3
        /// </summary>
        StandAlone,    

        /// <summary>
        /// 在线自动:4
        /// </summary>
        Automatic 
    }

    /// <summary>
    ///  IC卡类型
    /// </summary>
    public enum EnmICCardType
    {
        /// <summary>
        /// 临时:1
        /// </summary>
        Temp = 1,

        /// <summary>
        /// 定期卡:2
        /// </summary>
        Fixed,

        /// <summary>
        /// 固定车位卡:3
        /// </summary>
        FixedLocation
    }
    
    /// <summary>
    /// IC卡状态
    /// </summary>
    public enum EnmICCardStatus
    {
        /// <summary>
        /// 正常:1
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 挂失:2
        /// </summary>
        Lost,

        /// <summary>
        /// 注销:3
        /// </summary>
        Disposed,
    }

    /// <summary>
    /// 车位状态
    /// </summary>
    public enum EnmLocationStatus
    {
        Init=0,

        /// <summary>
        /// 空闲:1
        /// </summary>
        Space = 1,

        /// <summary>
        /// 占用:2
        /// </summary>
        Occupy,

        /// <summary>
        /// 正在入库:3（临时取物存车）
        /// </summary>
        Entering,

        /// <summary>
        /// 正在出库:4
        /// </summary>
        Outing,

        /// <summary>
        /// 临时取物出库:5
        /// </summary>
        TmpFetch,

        /// <summary>
        /// 库内搬移:6
        /// </summary>
        MovingVEH,

        /// <summary>
        /// 车辆旋转:7
        /// </summary>
        VehRotation
    }

    /// <summary>
    /// 车位类型
    /// </summary>
    public enum EnmLocationType
    {
        Init = 0,

        /// <summary>
        /// 正常车位:1
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 车厅:2
        /// </summary>
        Hall,

        /// <summary>
        /// 禁用车位:3
        /// </summary>
        Disable,

        /// <summary>
        /// 无效车位
        /// </summary>
        Invalid,

        /// <summary>
        /// ETV
        /// </summary>
        ETV,

        /// <summary>
        /// 缓存车位
        /// </summary>
        Temp,

        /// <summary>
        /// 禁用缓存
        /// </summary>
        DisTemp
    }

    /// <summary>
    /// 计费类型
    /// </summary>
    public enum EnmFeeType
    {
        /// <summary>
        /// 初始:0
        /// </summary>
        Init = 0,

        /// <summary>
        /// 小时：1
        /// </summary>
        Hour = 1,

        /// <summary>
        /// 月卡：2
        /// </summary>
        Month,

        /// <summary>
        /// 季度卡:3
        /// </summary>
        Season,

        /// <summary>
        /// 年卡:4
        /// </summary>
        Year
    }
    
    /// <summary>
    /// 计费单位
    /// </summary>
    public enum EnmFeeUnit
    {
        /// <summary>
        /// 计费:1
        /// </summary>
        Charging = 1,

        /// <summary>
        /// 限额:2
        /// </summary>
        Limited  
    }
    
    /// <summary>
    /// 操作员类型
    /// </summary>
    public enum EnmOperatorType
    {
        /// <summary>
        /// 天达维护人员: 1
        /// </summary>
        CIMCWorker = 1,

        /// <summary>
        /// 管理者: 2
        /// </summary>
        Manager,

        /// <summary>
        /// 操作员： 3
        /// </summary>
        Operator,

        /// <summary>
        /// 计费人员: 4
        /// </summary>
        TollKeeper,

        /// <summary>
        /// 其他：5
        /// </summary>
        Other
    }
  
    /// <summary>
    /// 刷卡队列作业类型
    /// </summary>
    public enum EnmSwipeCount
    {
        /// <summary>
        /// 取车:0
        /// </summary>
        GetCar = 0,

        /// <summary>
        /// 刷第一次存车卡:1
        /// </summary>
        SaveCarFirst,

        /// <summary>
        /// 刷第二次存车卡:2
        /// </summary>
        SaveCarSecond,

        /// <summary>
        /// 临时取物:3
        /// </summary>
        TmpFetch,

        /// <summary>
        /// 车体超限:4
        /// </summary>
        Oversize,

        /// <summary>
        /// 没有合适尺寸:5
        /// </summary>
        NoSuitableSize,
        
    }

    /// <summary>
    /// 返回值
    /// </summary>
    public enum EnmFaultType
    {
        /// <summary>
        /// 失败
        /// </summary>
        Fail = 0,
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 抛出异常
        /// </summary>
        Exception,
        /// <summary>
        /// 返回空值
        /// </summary>
        Null,
        /// <summary>
        /// 等待
        /// </summary>
        Wait,
        /// <summary>
        /// 出口/退出/车出库
        /// </summary>
        Exit,
        /// <summary>
        /// 增加
        /// </summary>
        Add,
        /// <summary>
        /// 不可用
        /// </summary>
        NotAvailable,
        /// <summary>
        /// 非全自动模式
        /// </summary>
        NotAutomatic,
        /// <summary>
        /// 不允许
        /// </summary>
        NotAllowed,
        /// <summary>
        /// 不匹配
        /// </summary>
        NotMatch,
        /// <summary>
        /// 连接失败
        /// </summary>
        FailConnection,
        /// <summary>
        /// 插入数据库失败
        /// </summary>
        FailToInsert,
        /// <summary>
        /// 更新数据库失败
        /// </summary>
        FailToUpdate,
        /// <summary>
        /// 查询数据库失败
        /// </summary>
        FailToQuery,
        /// <summary>
        /// 删除数据库失败
        /// </summary>
        FailToDelete,
        /// <summary>
        /// 用户名错误
        /// </summary>
        UserNameError,
        /// <summary>
        /// 用户名存在
        /// </summary>
        UserNameExist,
        /// <summary>
        /// 错误密码
        /// </summary>
        PasswordError,
        /// <summary>
        /// 密码为空
        /// </summary>
        PasswordNull,
        /// <summary>
        /// 操作管理员类型为空
        /// </summary>
        OPRTypeNull,
        /// <summary>
        /// 新旧密码不匹配
        /// </summary>
        PWNotMatch,
        /// <summary>
        /// 返回逻辑卡号
        /// </summary>
        LogicCardID,
        /// <summary>
        /// 返回物理卡号
        /// </summary>
        PhysicalCardID,
        /// <summary>
        /// 当前IC卡没有制卡
        /// </summary>
        NoICCardInfo,
        /// <summary>
        /// 当前IC卡已经制卡
        /// </summary>
        HasICCardInfo,
        /// <summary>
        /// 输入的逻辑卡号已经存在
        /// </summary>
        ICCardIDNotAllowed,
        /// <summary>
        /// 逻辑卡号不一致确认是否修改逻辑卡号
        /// </summary>
        ModifyICCardID,
        /// <summary>
        /// 写入IC卡失败
        /// </summary>
        FailToWriteICCard,
        /// <summary>
        /// 读取IC卡失败
        /// </summary>
        FailToReadICCard,
        /// <summary>
        /// 没有用户信息
        /// </summary>
        NoCustomerInfo,
        /// <summary>
        /// 当前卡有车存在车库
        /// </summary>
        CarInGarage,
        /// <summary>
        /// 当前卡没有车存在车库
        /// </summary>
        NoCarInGarage,
        /// <summary>
        /// 正在作业
        /// </summary>
        TaskOnICCard,
        /// <summary>
        /// 已经绑定用户
        /// </summary>
        BoundUser,
        /// <summary>
        /// IC卡挂失或注销
        /// </summary>
        LossORCancel,
        /// <summary>
        /// 当前卡没有绑定用户
        /// </summary>
        NoBoundCustomer,
        /// <summary>
        /// 没有找到设备
        /// </summary>
        NotFoundEquip,
        /// <summary>
        /// 设备不正确
        /// </summary>
        EquipIncorrect,
        /// <summary>
        /// 车厅
        /// </summary>
        HallEquip,
        /// <summary>
        /// 进车厅
        /// </summary>
        HallEnter,
        /// <summary>
        /// 出车厅
        /// </summary>
        HallExit,
        /// <summary>
        /// 进出两用车厅
        /// </summary>
        HallEnterorExit,
        /// <summary>
        /// 不是车厅设备
        /// </summary>
        IsNotHallEquip,
        /// <summary>
        /// 不是ETV设备
        /// </summary>
        IsNotETVEquip,
        /// <summary>
        /// 库区不同
        /// </summary>
        NotSameWareHouse,
        /// <summary>
        /// 不是正常车位
        /// </summary>
        NotNormalCarPOSN,
        /// <summary>
        /// 没有找到指定车位
        /// </summary>
        NotFoundCarPOSN,
        /// <summary>
        /// 固定车位卡车位
        /// </summary>
        FixedCarPOSN,
        /// <summary>
        /// 非固定车位卡车位
        /// </summary>
        NotFixedCarPOSN,
        /// <summary>
        /// 发送报文失败
        /// </summary>
        FailToSendTelegram,
        /// <summary>
        /// 获取计费标准信息失败
        /// </summary>
        NoTariffInfo,
        /// <summary>
        /// 固定卡未缴费
        /// </summary>
        FailToFixBill,
        /// <summary>
        /// 入库车辆尺寸超限
        /// </summary>
        OverCarInSize,
        /// <summary>
        /// 无效设备号
        /// </summary>
        InvalidEquipID,
        /// <summary>
        /// 无效库区号
        /// </summary>
        InvalidWareHouseID,
        /// <summary>
        /// 无效计费类型
        /// </summary>
        InvalidFeeType,
        /// <summary>
        /// 队列不为空
        /// </summary>
        WorkQueueNotEmpty,
        /// <summary>
        /// 分配ETV或TV失败
        /// </summary>
        FailToAllocETVorTV,
        /// <summary>
        /// 车厅无车，请将车开到车厅内再进行刷卡存车
        /// </summary>
        HallNotCar
    }

    /// <summary>
    /// 查询车主信息的查询标识
    /// </summary>
    public enum EnmQueryFlag
    {
        /// <summary>
        /// 姓名
        /// </summary>
        Name = 0,
        /// <summary>
        /// 移动电话
        /// </summary>
        Mobile,
        /// <summary>
        /// 车牌号
        /// </summary>
        LicPlteNbr,
        /// <summary>
        /// IC卡卡号（逻辑卡号）
        /// </summary>
        ICCardID,
        /// <summary>
        /// 分配车位
        /// </summary>
        CarPOSN,
        /// <summary>
        /// Email电子邮箱
        /// </summary>
        Email,
    }

    /// <summary>
    /// LED显示内容
    /// </summary>
    public enum EnmLEDContent
    {
        /// <summary>
        /// 1维修模式
        /// </summary>
        Maintenance = 1,

        /// <summary>
        /// 2手动模式
        /// </summary>
        Manual,

        /// <summary>
        /// 3单机自动模式
        /// </summary>
        StandAlone,

        /// <summary>
        /// 4存车无排队取车情况
        /// </summary>
        CarEntry,

        /// <summary>
        /// 5存车有排队取车情况
        /// </summary>
        CarEntryList,

        /// <summary>
        /// 6取车无排队取车情况
        /// </summary>
        CarExit,

        /// <summary>
        /// 7取车有排队取车情况
        /// </summary>
        CarExitList,

        /// <summary>
        /// 8临时取物无排队取车情况
        /// </summary>
        TmpFetch,

        /// <summary>
        /// 9临时取物有排队取车情况
        /// </summary>
        TmpFetchList,

        /// <summary>
        /// 10进车厅-空闲状态
        /// </summary>
        Entance,

        /// <summary>
        /// 11进出两用车厅-空闲状态
        /// </summary>
        EnterOrExit,

        /// <summary>
        /// 12出车厅-空闲状态
        /// </summary>
        Exit,

        /// <summary>
        /// 13所有ETV都是非全自动模式
        /// </summary>
        Fault,

        /// <summary>
        /// 14空闲状态
        /// </summary>
        Free,

        /// <summary>
        /// 15有排队取车时队列中每个结点的显示格式
        /// </summary>
        QueueFormat,

        /// <summary>
        /// 16空余大车状态
        /// </summary>
        MaxCarSize,
    }

    /// <summary>
    /// 车厅流程图节点描述
    /// </summary>
    public enum EnmFlowNodeDescp
    {
        /// <summary>
        /// 结束：-1
        /// </summary>
        End = -1,

        /// <summary>
        /// 初始：0
        /// </summary>
        Init,

        /// <summary>
        /// 有车入库：1(1001,1)
        /// </summary>
        EnterHasCar,

        /// <summary>
        /// 第一次刷卡:2
        /// </summary>
        EnterSwipeOne,

        /// <summary>
        /// 第二次刷卡:3
        /// </summary>
        EnterSwipeTwo,

        /// <summary>
        /// 车厅对中完毕：4(1001,101)
        /// </summary>
        EnterCarCheck,

        /// <summary>
        /// 车厅确认有车入库：5(1001,54)
        /// </summary>
        EnterCommandAccept,

        /// <summary>
        /// ETV装载完成：6(1013,1)
        /// </summary>
        DealLoad,

        /// <summary>
        /// ETV卸载完成：7(1014,1)
        /// </summary>
        DealUnload,

        /// <summary>
        /// 车体超限：8(1001,104)
        /// </summary>
        EnterCarOverrun,

        /// <summary>
        /// 车辆离开车厅：9(1001,4)
        /// </summary>
        EnterCarLeave,

        /// <summary>
        /// 刷取车卡(3,1)：10
        /// </summary>
        ExitSwipe,

        /// <summary>
        /// 车厅确认出车：11(1003,54)
        /// </summary>
        ExitCommandAccept,

        /// <summary>
        /// 允许取车：12(1003,1)
        /// </summary>
        ExitHasCar,

        /// <summary>
        /// 车辆离开车厅：13(1003,4)
        /// </summary>
        ExitCarLeave,

        /// <summary>
        /// 确定挪移：14
        /// </summary>
        MoveCarOk,

        /// <summary>
        /// 确定移动：15
        /// </summary>
        MoveEquipOk,

        /// <summary>
        /// ETV移动完成：16(1011,1)
        /// </summary>
        MoveEquipLoad,

        /// <summary>
        /// 临时取物确认(2,1)：17
        /// </summary>
        TmpFetchOk,

        /// <summary>
        /// 车厅确认临时取物：18(1002,54)
        /// </summary>
        TmpFetchCommandAccept,

        /// <summary>
        /// 允许取车：19(1002,1)
        /// </summary>
        TmpFetchHasCar,

        /// <summary>
        /// 车辆离开车厅：20(1002,4)
        /// </summary>
        TmpFetchCarLeave,

        /// <summary>
        /// 装载或将要装载：21
        /// </summary>
        Load,

        /// <summary>
        /// 装载完成：22
        /// </summary>
        LoadFinish,

        /// <summary>
        /// 或将要卸载：23
        /// </summary>
        WillUnLoad,

        /// <summary>
        /// 卸载：24
        /// </summary>
        UnLoad,

        /// <summary>
        /// 卸载完成：25
        /// </summary>
        UnLoadFinish,

        /// <summary>
        /// 移动或即将移动：26
        /// </summary>
        Move,

        /// <summary>
        /// 移动完成：27
        /// </summary>
        MoveFinish,
        /// <summary>
        /// 
        /// </summary>
        TMURO,
        /// <summary>
        /// 
        /// </summary>
        TMURORecoryHasCar,
        /// <summary>
        /// 
        /// </summary>
        TMURORecoryNoCar,
    }

    /// <summary>
    /// 数据库操作标识
    /// </summary>
    public enum EnmDBOperations
    {
        /// <summary>
        /// 插入数据表格标志:1
        /// </summary>
        Insert = 1,

        /// <summary>
        /// 更新修改标志:2
        /// </summary>
        Modify,

        /// <summary>
        /// 删除标志:3
        /// </summary>
        Delete,
    }

    public enum EnuCardDataType
    {
        /// <summary>
        /// 计费类型
        /// </summary>
        FeeType,

        /// <summary>
        /// 逻辑卡号
        /// </summary>
        ICCardID,

        /// <summary>
        /// 计费开始时间
        /// </summary>
        FeeStartTime,

        /// <summary>
        /// 计费结束时间
        /// </summary>
        FeeEndTime,
    }

    #region ModBus-PLC枚举类
    public enum CpuType
    {
        S7200 = 0,
        S7300 = 10,
        S7400 = 20,
        S71200 = 30,
    }

    public enum ErrorCode
    {
        NoError = 0,
        WrongCPU_Type = 1,
        ConnectionError = 2,
        IPAddressNotAvailable,

        WrongVarFormat = 10,
        WrongNumberReceivedBytes = 11,

        SendData = 20,
        ReadData = 30,

        WriteData = 50
    }

    public enum DataType
    {
        Input = 129,
        Output = 130,
        Memory = 131,
        DataBlock = 132,
        Timer = 29,
        Counter = 28
    }

    public enum VarType
    {
        Byte = 0,
        Int,
        DInt,
        Word,
        DWord
    }
    #endregion


}
