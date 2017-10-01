using System;
using System.Collections.Generic;
using System.ServiceModel;
using DatabaseManagementLib;
using TypedefLib;
using System.Drawing;

namespace WCFQueryLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IQueryService”。
    [ServiceContract]
    public interface IQueryService
    {
        // TODO: 在此添加您的服务操作

        /// <summary>
        /// 获取配置文件参数
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        struClientPara GetConfig();

        #region 对操作管理员账号和密码的操作

        /// <summary>
        /// 获取操作员所有信息列表
        /// </summary>
        /// <param name="lstOperatorDto"></param>
        /// <returns></returns>
        [OperationContract]
        bool GetOperatorList(ref List<COperatorDto> lstOperatorDto);

        /// <summary>
        /// 检查操作管理员账号和密码是否正确
        /// 返回结果：
        /// 正确：
        /// EnmFaultType.Success;//用户名和密码都正确
        /// 错误：
        /// EnmFaultType.UserNamerError;//用户名错误
        /// EnmFaultType.PasswordError;//密码错误
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassWord"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType CheckPassword(ref COperatorDto operatorTBL);

        /// <summary>
        /// 修改操作管理员账号密码
        /// 返回结果：
        /// 正确：
        /// EnmFaultType.Success;//用户名和密码都正确
        /// 错误：
        /// EnmFaultType.UserNamerError;//用户名错误
        /// EnmFaultType.PasswordError;//密码错误
        /// EnmFaultType.PWNotMatch;//新旧密码不匹配
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassWord">密码</param>
        [OperationContract]
        EnmFaultType UpdatePassword(ref COperatorDto operatorTBL, string strNewPassword);

        /// <summary>
        /// 增加操作管理员
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;//成功
        /// 失败：
        /// EnmFaultType.FailToUpdate;//更新数据库失败
        /// EnmFaultType.PasswordNull;// 空密码
        /// EnmFaultType.OPRTypeNull; //操作管理员类型为空
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// </summary>
        /// <param name="operatorTBL"></param>
        /// <returns></returns>
        [OperationContract(Name = "AddOperator")]
        EnmFaultType AddOrUPDTOperator(COperatorDto operatorTBL);
                
        /// <summary>
        /// 获取操作管理员类型（根据用户名）
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetOperatorType")]
        EnmOperatorType GetOperatorType(string strUserName);

        /// <summary>
        /// 删除操作管理员
        /// 返回值：
        /// 成功：true
        /// 失败：false
        /// </summary>
        /// <param name="operatorTBL"></param>
        /// <returns></returns>
        [OperationContract]
        bool DeleteOperator(COperatorDto operatorTBL);

        #endregion 对操作管理员账号和密码的操作

        #region 对IC卡的处理
                
        /// <summary>
        /// 读取IC卡内存中的数据
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.FailConnection;//连接刷卡器失败
        ///       EnmFaultType.Fail;//获取刷卡器对象失败
        ///       EnmFaultType.Exception;
        /// </summary>
        /// <param name="struDataOut"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ReadICCardData(out struICCardData struDataOut);

        /// <summary>
        /// 读取IC卡卡号(逻辑卡号)；
        /// 成功返回值为EnmFaultType.LogicCardID    ：获取的是逻辑卡号；
        /// 失败返回值为EnmFaultType.FailToReadICCard;          ：读取IC卡失败
        /// 失败返回值为EnmFaultType.NoICCardInfo;              : 当前IC卡没有制卡
        /// 失败返回值为EnmFaultType.FailConnection ：连接读卡器失败，获取的是空值；
        /// </summary>
        /// <param name="strICCardID">IC卡卡号</param>
        [OperationContract]
        EnmFaultType ReadICCard(out string strPhysicalCardID, out string strICCardID);

        /// <summary>
        /// 制卡
        /// 成功：      
        /// EnmFaultType.Success
        /// 失败：
        /// EnmFaultType.Null;//传入的物理卡号或逻辑卡号为空
        /// 已经注释掉：EnmFaultType.FailConnection;//连接读卡器失败
        /// 已经注释掉：EnmFaultType.FailToWriteICCard;//写入IC卡失败
        /// EnmFaultType.HasICCardInfo;//当前IC卡已经制卡
        /// EnmFaultType.FailToInsert; //插入数据库失败
        /// EnmFaultType.ICCardIDNotAllowed;  //输入的逻辑卡号已经存在
        /// EnmFaultType.ModifyICCardID;   //逻辑卡号不一致确认是否修改逻辑卡号
        /// 写入数据格式【总长度不大于30个字符】：
        ///     卡号("1234567890123456789"长度不大于19个字符) + IC卡类型("1"长度=1个字符) + 制卡时间("yyyyMMdd"长度=8个字符) + 收费标准("01"长度=2个字符)
        /// </summary>
        /// <param name="strPhysicalCardID">物理卡号</param>
        /// <param name="strData">
        /// 写入数据格式（最大可写入长度是15个字节）：
        ///     卡号 + 类型("1") + 制卡时间("yyyyMMdd") + 收费标准("01")
        ///     </param>
        [OperationContract]
        EnmFaultType AddNewICCard(string strPhysicalCardID, string strData);

        /// <summary>
        /// 查询IC卡信息
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        /// </summary>
        /// <param name="icCardTable"></param>
        [OperationContract]
        EnmFaultType QueryICCardInfo(ref CICCardDto icCardTable);
       
        /// <summary>
        /// 查询IC卡信息(根据车位地址)
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;
        /// add by wanxiaona20150731
        /// </summary>
        /// <param name="icCardTable"></param>
        [OperationContract]
        EnmFaultType QueryICCardInfoByAddress(ref CICCardDto icCardTable);

        /// <summary>
        /// 查询IC卡信息(根据物理卡号)
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;
        /// </summary>
        /// <param name="icCardTable"></param>
        [OperationContract]
        EnmFaultType QueryICCardInfoByPhyID(ref CICCardDto icCardTable);

        /// <summary>
        /// 依物理卡号，查询相应的用户卡号
        /// </summary>
        /// <param name="physic"></param>
        /// <returns></returns>
        [OperationContract]
        string QueryICCodeByPhysic(string physic);

        /// <summary>
        /// 更新IC卡信息
        /// </summary>
        /// <param name="icCardTable"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateICCardInfo(CICCardDto icCardTable);

        /// <summary>
        /// 获取当前剩余定期卡预留车位的个数（可定制的定期卡）
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        int GetFixReservedSizeCount();

        /// <summary>
        /// 批量修改固定卡截止日期推迟之后时间
        /// 成功：EnmFaultType.Success;
        /// 失败：
        /// </summary>
        /// <param name="nDelayDays"></param>
        /// <param name="lstICCardID"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType BatchModifyICCardDeadLine(int nDelayDays, List<string> lstICCardID);
                
        /// <summary>
        /// 批量修改计费标准
        /// 成功：EnmFaultType.Success;
        /// 失败：
        /// </summary>
        /// <param name="nDelayDays"></param>
        /// <param name="lstICCardID"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType BatchModifyICCardTariffID(int nTariffID, List<string> lstICCardID);

       
        #endregion 对IC卡的处理

        #region 对车主的管理
        
        /// <summary>
        /// 无条件查询所有车主信息，返回所有车主信息列表
        /// </summary>
        /// <param name="lstStruCUSTInfo"></param>
        /// <returns></returns>
        [OperationContract(Name = "QueryCUSTInfo")]
        bool QueryCUSTInfo(ref List<struCustomerInfo> lstStruCUSTInfo);

        /// <summary>
        /// nQueryFlag值含义：
        /// 1：姓名
        /// 2：移动电话
        /// 3：车牌号
        /// 4：IC卡卡号（逻辑卡号）
        /// 5：分配车位
        /// </summary>
        [OperationContract(Name = "QueryCUSTInfoByStr")]
        void QueryCUSTInfo(EnmQueryFlag enmQueryFlag, string strInfo, ref List<struCustomerInfo> lstStruCUSTInfo);

        /// <summary>
        /// 成功：返回EnmFaultType.Success
        /// 失败返回值EnmFaultType.Fail：IC卡卡号为空，绑定卡IC卡卡号不允许为空
        ///           EnmFaultType.NoICCardInfo：没有制卡
        ///           EnmFaultType.CarInGarage：有车存在车库且要绑定的车位与存车的车位不一致
        ///           EnmFaultType.LossORCancel：IC卡挂失或注销
        ///           EnmFaultType.FailToUpdate：更新数据库失败
        ///           EnmFaultType.TaskOnICCard：当前卡有作业正在操作
        ///           EnmFaultType.FailToInsert：插入数据库失败
        ///           EnmFaultType.NotFoundCarPOSN;//没有找到要绑定的车位
        ///           EnmFaultType.FixedCarPOSN;//指定车位已经绑定其他车主
        ///           EnmFaultType.BoundUser;//当前卡已经绑定车主信息
        /// </summary>
        /// <param name="customerInfo">用户对象</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType BoundICCardForCUST(struCustomerInfo customerInfo);
        
        /// <summary>
        /// 修改车主信息；
        /// 成功：返回EnmFaultType.Success
        /// 失败返回值EnmFaultType.Fail：IC卡卡号为空，绑定卡IC卡卡号不允许为空
        ///           EnmFaultType.NoICCardInfo：没有制卡
        ///           EnmFaultType.CarInGarage：有车存在车库且要绑定的车位与存车的车位不一致
        ///           EnmFaultType.LossORCancel：IC卡挂失或注销
        ///           EnmFaultType.FailToUpdate：更新数据库失败
        ///           EnmFaultType.TaskOnICCard：当前卡有作业正在操作
        ///           EnmFaultType.NotFoundCarPOSN;//没有找到要绑定的车位
        ///           EnmFaultType.FixedCarPOSN;//指定车位已经绑定其他车主
        ///           EnmFaultType.NoBoundCustomer;//当前卡没有绑定车主信息
        /// </summary>
        /// <param name="customer">用户对象</param>
        /// <param name="icCard">IC卡对象</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyCUSTInfo(struCustomerInfo customerInfo);

        /// <summary>
        /// 删除车主信息
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;
        /// 失败：
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.CarInGarage;//当前卡有车存在车库
        /// EnmFaultType.TaskOnICCard;//当前卡有作业正在操作
        /// EnmFaultType.NoBoundCustomer;//当前卡没有绑定车主
        /// EnmFaultType.NoCustomerInfo;//没有车主信息
        /// EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="nUserID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType DeleteCustomer(string strICCardID);

        #endregion 对车主的管理

        #region 对车位信息的管理

        /// <summary>
        /// 查询车位信息(根据库区号和车位地址)
        /// </summary>
        /// <param name="carLOCTable"></param>
        /// <param name="nICCardType"></param>
        /// <returns></returns>
        [OperationContract(Name = "QueryCarPOSNByAddr")]
        EnmFaultType QueryCarPOSN(ref CCarLocationDto carLOCTable, ref CICCardDto nICCardType);

        /// <summary>
        /// 查询车位信息(根据库区号)
        /// </summary>
        /// <param name="carLOCTable"></param>
        [OperationContract(Name = "QueryCarPOSNByWareHouse")]
        List<CCarLocationDto> QueryCarPOSN(int nWareHouse);

        /// <summary>
        /// 查询车位信息（根据IC卡卡号）
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        ///       EnmFaultType.NotFoundCarPOSN;//当前IC卡没有存车
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="carLocatioTBL"></param>
        /// <returns></returns>
        [OperationContract(Name = "QueryCarPOSNByCardID")]
        EnmFaultType QueryCarPOSN(string strICCardID, out CCarLocationDto carLocatioTBL);

        /// <summary>
        /// 获取车位个数（根据库区号和车位类型）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarPOSNType"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetCarPOSNCountByType")]
        int GetCarPOSNCount(int nWareHouse, int nCarPOSNType);

        /// <summary>
        /// 获取车位个数（根据库区号、车位类型和车位状态）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarPOSNType"></param>
        /// <param name="nCarPOSNStatus"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetCarPOSNCountByStat")]
        int GetCarPOSNCount(int nWareHouse, int nCarPOSNType, int nCarPOSNStatus);
        
        /// <summary>
        /// 获取某库区的总车位数、空闲、占用、空余大车位值
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="rectVehCount"></param>
        [OperationContract]
        void GetCarPOSNFreeOccupyCount(int nWareHouse, ref Rectangle rectVehCount);

        /// <summary>
        /// 获取车位状态（根据IC卡卡号）
        /// </summary>
        /// <param name="enmLocationStatus"></param>
        /// <param name="strICCardID"></param>
        [OperationContract(Name = "GetCarPLSNStatus")]
        void GetCarPLSNStatus(ref EnmLocationStatus enmLocationStatus, string strICCardID, 
            EnmICCardType enmICType, EnmICCardStatus enmICStatus,
            EnmFeeType enmFeeType, EnmTaskType enmTask, EnmSwipeCount enmSwipe, EnmFlowNodeDescp enmFlow);

        #endregion 对车位信息的管理

        #region 对设备的管理
        /// <summary>
        /// 获取设备模式
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceID"></param>
        /// <returns></returns>
        [OperationContract]
        EnmModel GetDeviceMode(int nWareHouse, int nDeviceID);

        /// <summary>
        /// 获取设备状态信息(根据库号和设备号)
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceCode"></param>
        [OperationContract]
        void GetDeviceStatus(out CDeviceStatusDto deviceStatus, int nWareHouse, int nDeviceCode);

        /// <summary>
        /// 获取设备状态信息(根据库号和设备地址)
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        [OperationContract(Name = "GetDeviceStatusByAddr")]
        void GetDeviceStatus(out CDeviceStatusDto deviceStatus, int nWareHouse, string nDeviceAddr, EnmSMGType enmSMGType);

        /// <summary>
        /// 获取设备故障列表（获取与指定据库号和设备号相同，且于指定故障显示内容不同的故障信息）
        /// </summary>
        /// <param name="lstDeviceFault"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <param name="strFaultDesc"></param>
        [OperationContract]
        void GetDeviceFault(out List<CDeviceFaultDto> lstDeviceFault, int nWareHouse, int nEquipID, string strFaultDesc);

        /// <summary>
        /// 获取对应库的所有车厅或者ETV/TV设备号列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="dataContract"></param>
        /// <returns></returns>
        [OperationContract]
        List<object> GetDeviceIDList(int nWareHouse, EnmSMGType enmSMGType);
        
        /// <summary>
        /// 获取nWareHouse库的所有设备状态列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        [OperationContract]
        List<CDeviceStatusDto> GetDeviceStatusList(int nWareHouse);
        #endregion

        #region 对取车队列的管理
        /// <summary>
        /// 查询对应库区的对应车厅的取车队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        [OperationContract]
        List<CWorkQueueDto> QueryWorkQueue(int nWareHouse, int nEquipID);

        
        /// <summary>
        /// 删除队列中的取车卡
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.NoICCardInfo;//没有刷卡信息
        ///     EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="nWareHouse">库区ID</param>
        /// <param name="nEquipID">设备ID</param>
        /// <param name="strICCardID">IC卡卡号</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType DeleteWorkQueue(int nWareHouse, int nEquipID, string strICCardID);
         
        /// <summary>
        /// 修改作业优先级
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="strSrcICCode"></param>
        /// <param name="strDestICCode"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifWorkQueue(string strSrcICCode, string strDestICCode);
        #endregion

        #region 对系统日志信息的管理
        /// <summary>
        /// 获取所有IC卡缴费日志
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CICCardLogDto> GetICCardLogList();

        /// <summary>
        /// 写系统日志
        /// </summary>
        /// <param name="systemLogeTBL"></param>
        /// <returns></returns>
        [OperationContract]
        bool InsertSysLog(CSystemLogDto systemLogeTBL);
        
        /// <summary>
        /// 获取所有系统日志
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CSystemLogDto> GetSystemLogList();
       
        /// <summary>
        /// 获取所有报文日志
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CTelegramLogDto> GetTelegramLogList();
       
        /// <summary>
        /// 获取所有设备状态日志
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CDeviceStatusLogDto> GetDeviceStatusLogList();

        /// <summary>
        /// 写设备故障日志
        /// </summary>
        /// <param name="logTBL"></param>
        /// <returns></returns>
        [OperationContract]
        bool InsertDeviceFaultLog(CDeviceFaultLogDto logTBL);

        /// <summary>
        /// 获取所有设备故障日志
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CDeviceFaultLogDto> GetDeviceFaultLogList();

        /// <summary>
        /// 获取所有IC卡缴费日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetICCardLogListByContent")]
        List<CICCardLogDto> GetICCardLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent);

        /// <summary>
        /// 获取所有系统日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetSystemLogListByContent")]
        List<CSystemLogDto> GetSystemLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent);

        /// <summary>
        /// 获取所有报文日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetTelegramLogListByContent")]
        List<CTelegramLogDto> GetTelegramLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent);

                /// <summary>
        /// 获取所有报文日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetTelegramLogListByPLCContent")]
        List<CTelegramLogDto> GetTelegramLogList(int nIndex, DateTime dtStart, DateTime dtEnd, int nPLCID, string strContent);

        /// <summary>
        /// 获取所有设备状态日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetDeviceStatusLogListByContent")]
        List<CDeviceStatusLogDto> GetDeviceStatusLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent);

        /// <summary>
        /// 获取所有设备故障日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "GetDeviceFaultLogListByContent")]
        List<CDeviceFaultLogDto> GetDeviceFaultLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent);
        #endregion 对系统日志信息的管理

        #region 对语音信息的管理
        /// <summary>
        /// 获取所有语音列表
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CSoundDto> GetSoundList();

        /// <summary>
        /// 更新语音
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateSoundDto(CSoundDto sound);

        /// <summary>
        /// 更新语音列表
        /// </summary>
        /// <param name="lstSound"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateSoundDtoList(List<CSoundDto> lstSound);
        #endregion

        #region 对故障处理的管理（手动作业）
        /// <summary>
        /// 获取所有作业类型列表
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CDeviceStatusDto> GetTaskTypeList();
        #endregion

        #region 收费管理
        /// <summary>
        /// 获取所有收费标准列表
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CTariffDto> GetTariffList();

        /// <summary>
        /// 保存收费标准实例
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// </summary>
        /// <param name="tariff"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType SaveTariff(CTariffDto tariff);

        /// <summary>
        /// 删除收费标准
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="tariff"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType DeleteTariff(CTariffDto tariff);

                /// <summary>
        ///  查询停车费用等信息（临时卡和固定卡等所有卡）-----计费端使用
        ///  成功：
        ///     EnmFaultType.Success;
        ///  失败：
        ///     EnmFaultType.FailToReadICCard; //读取IC卡失败
        ///     EnmFaultType.FailConnection //连接读卡器失败，获取的是空值；
        ///     EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        ///     EnmFaultType.NoTariffInfo;// 获取计费标准信息失败
        ///     EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
        ///     EnmFaultType.NotMatch;//IC卡类型和不匹配 
        ///     EnmFaultType.NoBoundCustomer; //当前卡没有绑定车主计费信息
        ///     EnmFaultType.Fail; //计算停车费用失败
        /// </summary>
        /// <param name="struBillInfo"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ReadAndQueryParkingInfoB(ref struBillInfo struBillInfo,int region);

        /// <summary>
        ///  查询停车费用等信息（临时卡和固定卡等所有卡）
        ///  成功：
        ///     EnmFaultType.Success;
        ///  失败：
        ///     EnmFaultType.FailToReadICCard; //读取IC卡失败
        ///     EnmFaultType.FailConnection ：连接读卡器失败，获取的是空值；
        ///     EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        ///     EnmFaultType.NoTariffInfo;// 获取计费标准信息失败
        ///     EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
        ///     EnmFaultType.NotMatch;//IC卡类型和不匹配 
        ///     EnmFaultType.NoBoundCustomer; //当前卡没有绑定车主计费信息
        ///     EnmFaultType.Fail; //计算停车费用失败
        /// </summary>
        /// <param name="struBillInfo"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ReadAndQueryParkingInfo(ref struBillInfo struBillInfo,int region);

        /// <summary>
        ///  查询停车费用等信息（临时卡和固定卡等所有卡）
        ///  成功：
        ///     EnmFaultType.Success;
        ///  失败：
        ///     EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        ///     EnmFaultType.NoTariffInfo;// 获取计费标准信息失败
        ///     EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
        ///     EnmFaultType.NotMatch;//IC卡类型不匹配 
        ///     EnmFaultType.NoBoundCustomer; //当前卡没有绑定车主计费信息
        ///     EnmFaultType.Fail; //计算停车费用失败
        /// </summary>
        /// <param name="struBillInfo"></param>
        /// <param name="region">用于区分客户端，所在的区域：1-在南侧，2-北侧</param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType QueryParkingInfo(ref struBillInfo struBillInfo,int region);
        
        /// <summary>
        /// 根据ID获取计费信息
        /// </summary>
        /// <param name="nTariffID"></param>
        /// <returns></returns>
        [OperationContract]
        CTariffDto GetTariffByID(int nTariffID);
        #endregion 收费管理

        #region 对LED的管理
        /// <summary>
        /// 修改LED关键字
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.Fail;//LED文本未有当前关键字
        /// EnmFaultType.FailToUpdate;//更新数据库失败
        /// </summary>
        /// <param name="strLEDOld"></param>
        /// <param name="strLEDNew"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType ModifyLEDContent(string strLEDOld, string strLEDNew);
        
        /// <summary>
        /// 获取所有LED列表
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CLedContentDto> GetLEDContentList();

        /// <summary>
        /// 更新LED列表
        /// </summary>
        /// <param name="lstLedContent"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateLEDContentDtoList(List<CLedContentDto> lstLedContent);

        #endregion 对LED的管理

        #region 对队列信息的管理

        /// <summary>
        /// 查询所有排队报文
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CWorkQueueDto> GetSendTelegramQueue();

        /// <summary>
        /// 根据库区号和设备号获取当前发送报文队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetSendTelegramQueueByEquipID")]
        List<CWorkQueueDto> GetSendTelegramQueue(int nWareHouse, int nEquipID);

        /// <summary>
        /// 删除报文队列对象
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="workQueue"></param>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType DeleteWorkQueueObject(int wid);

        /// <summary>
        /// 查询所有的队列信息(包括有报文队列的)
        /// </summary>
        /// <param name="warehouse">库区</param>
        /// <returns></returns>
        [OperationContract]
        List<CWorkQueueDto> GetAllQueueForDisp(int warehouse);

        /// <summary>
        /// 删除所有队列
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        EnmFaultType DeleteAllWorkQueue();

        #endregion 对队列信息的管理

        #region 允许用户设置厅外刷卡取车有效
        [OperationContract]
        string GetUserSetFixCardOutLimit();

        [OperationContract]
        int SetUserSetFixCardOutLimit(string value);
        #endregion
    }  
}
