using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using BaseMethodLib;
using CommonMethodLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using ICCardManagementLib;
using LOGManagementLib;
using TypedefLib;
using BillingManagementLib;
using System.Reflection;
using System.Collections;
using SoundManagementLib;
using System.Drawing;
using CustomerLEDLib;
using WorkFlowLib;

namespace WCFQueryLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“QueryService”。
    [Serializable]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class CQueryService : IQueryService, IDisposable
    {
        /// <summary>
        /// 销毁服务实例
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 获取配置文件参数
        /// </summary>
        /// <returns></returns>
        public struClientPara GetConfig()
        {
            //             CConfigManagement paraReturn = new CConfigManagement();
            //             paraReturn = CConfigManagement.myPara;
            struClientPara struPara = new struClientPara();
            try
            {
                Dictionary<int, List<int>> dictETVOrTVDeviceID = new Dictionary<int, List<int>>();
                foreach (int key in CConfigManagement.myPara.HashETVOrTVDeviceID.Keys)
                {
                    dictETVOrTVDeviceID.Add(key, (List<int>)CConfigManagement.myPara.HashETVOrTVDeviceID[key]);
                }
                struPara.dictETVOrTVDeviceID = dictETVOrTVDeviceID;

                Dictionary<int, List<int>> dictHallDeviceID = new Dictionary<int, List<int>>();
                foreach (int key in CConfigManagement.myPara.HashHallDeviceID.Keys)
                {
                    dictHallDeviceID.Add(key, (List<int>)CConfigManagement.myPara.HashHallDeviceID[key]);
                }
                struPara.dictHallDeviceID = dictHallDeviceID;

                struPara.bBillingFlag = CConfigManagement.myPara.BillingFlag;// 是否有计费功能标志，有计费返回1，无计费返回0
                struPara.bCarImageFlag = CConfigManagement.myPara.CarImageFlag;// 是否需要车牌号和车辆图片
                struPara.lstCarSizes = CConfigManagement.myPara.LstCarSizes;// 本项目车位尺寸列表 111(长 * 宽 * 高)
                struPara.strClientTitle = CConfigManagement.myPara.ClientTitle;//"中集天达智能车库管理系统";
                struPara.strBillingTitle = CConfigManagement.myPara.BillingTitle;//"中集天达智能车库收费管理系统";
                struPara.strDBConnectionInfo = CConfigManagement.myPara.DBConnectionInfo;
                struPara.strSQLQueryNOTECarPOSN = CConfigManagement.myPara.SQLQueryNOTECarPOSN;//"SELECT warehouse,carlocaddr,carloctype,carlocstatus,iccode From dbo.carlocation";
                struPara.strSQLQueryNOTEDeviceFault = CConfigManagement.myPara.SQLQueryNOTEDeviceFault;//"SELECT faultdescp,warehouse,devicecode,faultaddress,color,isable From dbo.devicefault";
                struPara.strSQLQueryNOTEDeviceStatus = CConfigManagement.myPara.SQLQueryNOTEDeviceStatus;//"SELECT devicecode,warehouse,deviceaddr,devicemode,devicetype,isavailable From dbo.devicestatus";
                struPara.strAllOptPermission = CConfigManagement.myPara.AllOptPermission; // "系统维护,系统配置,用户管理,缴费管理,操作员管理,查询统计,临时取物,手动指令";//
                struPara.ManagerTopPRMSN = CConfigManagement.myPara.ManagerTopPRMSN;//255//  管理者: 2
                struPara.OperatorTopPRMSN = CConfigManagement.myPara.OperatorTopPRMSN;//103// 操作员： 3
                struPara.TollKeeperTopPRMSN = CConfigManagement.myPara.TollKeeperTopPRMSN;//111// 计费人员: 4
                struPara.OtherTopPRMSN = CConfigManagement.myPara.OtherTopPRMSN;//103// // 其他：5
                struPara.CIMCWorkerTopPRMSN = CConfigManagement.myPara.CIMCWorkerTopPRMSN;//239// 天达维护人员: 1;    按位与结果;1111 1111
                struPara.lstPanelLayoutInfo = CConfigManagement.myPara.LstPanelLayoutInfo;//在各个PLC下配置：(1, 102, 4, 12), "CVerticalPanel")当前项目的库号和车位排列列表

                List<object> lstTmp = new List<object>();
                foreach (int nPLCID in CConfigManagement.myPara.LstPLCID)
                {
                    lstTmp.Add(nPLCID);
                }
                struPara.lstPLCID = lstTmp;//PLC ID链表
                struPara.strCarMaxSize = CConfigManagement.myPara.CarMaxSize; //本车库允许存放的最大车辆尺寸
                struPara.objReaderPara = CConfigManagement.myPara.ReaderPara;//刷卡器参数
                struPara.VehicleRotationFlag = CConfigManagement.myPara.VehicleRotationFlag;//6113定制需求：车辆空闲时掉头旋转标识，默认值：0，有效值：0-无旋转功能；1-有旋转功能
                struPara.ShowInfoFlag = CConfigManagement.myPara.ShowInfoFlag;//客户端每个已停车的车位或固定卡车位上显示的内容：默认值：1，有效值：1-卡号；2-名字
                struPara.DicHallIDDictionary = CConfigManagement.myPara.DicHallIDDictionary;//获取所有车厅配置：键值对为PLC ID（int）和所有车厅配置（Dictionary《int, struHallEquips》
                
                //6113定制需求：替换IC卡但是逻辑卡号不变仍然使用工号，默认值：0 ，有效值：0-关闭该功能新卡与旧卡的逻辑卡号不同；1-打开该功能，新卡与旧卡的逻辑卡号相同
                struPara.ReplacePhyCardIDFlag = CConfigManagement.myPara.ReplacePhyCardIDFlag;
                return struPara;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetConfig 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return struPara;
            }
        }

        #region 对操作管理员账号和密码的操作
        /// <summary>
        /// 获取操作员所有信息列表
        /// </summary>
        /// <param name="lstOperatorDto"></param>
        /// <returns></returns>
        public bool GetOperatorList(ref List<COperatorDto> lstOperatorDto)
        {
            try
            {
                lstOperatorDto = CCommonMethods.myMethods.GetOperatorList();
                if (null == lstOperatorDto || 0 == lstOperatorDto.Count)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetOperatorList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

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
        public EnmFaultType CheckPassword(ref COperatorDto operatorTBL)
        {
            try
            {
                COperatorDto operatorTable = CCommonMethods.myMethods.GetOperatorInfo(operatorTBL.optcode);

                if (null == operatorTable)
                {
                    return EnmFaultType.UserNameError;//用户名错误
                }
                else if (null != operatorTable && operatorTBL.optpassword == operatorTable.optpassword)
                {
                    operatorTBL = operatorTable;
                    return EnmFaultType.Success;//用户名和密码都正确
                }
                else
                {
                    return EnmFaultType.PasswordError;//密码错误
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.CheckPassword 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 修改操作管理员账号密码
        /// 返回结果：
        /// 正确：
        /// EnmFaultType.Success;//用户名和密码都正确
        /// 错误：
        /// EnmFaultType.UserNamerError;//用户名错误
        /// EnmFaultType.PasswordError;//密码错误
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassWord">密码</param>
        public EnmFaultType UpdatePassword(ref COperatorDto operatorTBL, string strNewPassword)
        {
            try
            {
                EnmFaultType enmResult = CheckPassword(ref operatorTBL);
                if (enmResult != EnmFaultType.Success)
                {
                    return enmResult;
                }
                CCommonMethods.myMethods.UpdatePassword(ref operatorTBL, strNewPassword);
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.UpdatePassword 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType AddOrUPDTOperator(COperatorDto operatorTBL)
        {
            try
            {
                if (null != CCommonMethods.myMethods.GetOperatorInfo(operatorTBL.optcode))
                {
                    if (CCommonMethods.myMethods.UpdateOperatorInfo(operatorTBL))
                    {
                        return EnmFaultType.Success;
                    }
                    else
                    {
                        return EnmFaultType.FailToUpdate;//更新数据库失败
                    }
                }
                if (string.IsNullOrWhiteSpace(operatorTBL.optpassword))
                {
                    return EnmFaultType.PasswordNull;// 空密码
                }
                if (null == operatorTBL.opttype)
                {
                    return EnmFaultType.OPRTypeNull; //操作管理员类型为空
                }
                if (!CCommonMethods.myMethods.InsertOperatorInfo(operatorTBL))
                {
                    return EnmFaultType.FailToInsert;//插入数据库失败
                }
                return EnmFaultType.Success;//成功
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.AddOrUPDTperator 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 获取操作管理员类型（根据用户名）
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public EnmOperatorType GetOperatorType(string strUserName)
        {
            try
            {
                COperatorDto operatorTBL = CCommonMethods.myMethods.GetOperatorInfo(strUserName);
                return (EnmOperatorType)operatorTBL.opttype;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetOperatorType 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

        /// <summary>
        /// 删除操作管理员
        /// 返回值：
        /// 成功：true
        /// 失败：false
        /// </summary>
        /// <param name="operatorTBL"></param>
        /// <returns></returns>
        public bool DeleteOperator(COperatorDto operatorTBL)
        {
            try
            {
                return CCommonMethods.myMethods.DeleteOperatorInfo(operatorTBL);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteOperator 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
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
        public EnmFaultType ReadICCardData(out struICCardData struDataOut)
        {
            struDataOut = new struICCardData();
            try
            {
                CICCardReaderObj cardReader = CCommonMethods.myMethods.GetReaderObj(CConfigManagement.myPara.MaintainComID);
                if (null != cardReader)
                {
                    cardReader.ConnectCOM();
                    if (cardReader.isComConnected)
                    {
                        string strBlock0 = string.Empty;
                        string strBlock1 = string.Empty;
                        string strBlock2 = string.Empty;

                        cardReader.ReadDataFromCard(1, 0, out strBlock0);
                        cardReader.ReadDataFromCard(1, 1, out strBlock1);
                        cardReader.ReadDataFromCard(1, 2, out strBlock1);
                        struDataOut = CCommonMethods.myMethods.ConvertICCardData(strBlock0, strBlock1, strBlock2);
                        cardReader.DisConnectCOM();
                        return EnmFaultType.Success;
                    }
                    return EnmFaultType.FailConnection;//连接刷卡器失败
                }
                return EnmFaultType.Fail;//获取刷卡器对象失败
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFQueryLib.CQueryService.ReadICCardData 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
       
        /// <summary>
        /// 读取IC卡卡号(逻辑卡号)---维护系统
        /// 成功返回值为EnmFaultType.LogicCardID    ：获取的是逻辑卡号；
        /// 失败返回值为EnmFaultType.FailToReadICCard;          ：读取IC卡失败
        /// 失败返回值为EnmFaultType.NoICCardInfo;              : 当前IC卡没有制卡
        /// 失败返回值为EnmFaultType.FailConnection ：连接读卡器失败，获取的是空值；
        /// </summary>
        /// <param name="strICCardID">IC卡卡号</param>
        public EnmFaultType ReadICCard(out string strPhysicalCardID, out string strICCardID)
        {
            try
            {
                int nComID = CConfigManagement.myPara.MaintainComID;// 读取配置文件
                return ReadICCardByComID(out strPhysicalCardID, out strICCardID, nComID);
            }
            catch (Exception ex)
            {
                strPhysicalCardID = string.Empty;
                strICCardID = string.Empty;
                CLOGException.Trace("WCFQueryLib.CQueryService.ReadICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 读取IC卡卡号(逻辑卡号)；
        /// 成功返回值为EnmFaultType.LogicCardID    ：获取的是逻辑卡号；
        /// 失败返回值为EnmFaultType.FailToReadICCard;          ：读取IC卡失败
        /// 失败返回值为EnmFaultType.NoICCardInfo;              : 当前IC卡没有制卡
        /// 失败返回值为EnmFaultType.FailConnection ：连接读卡器失败，获取的是空值；
        /// </summary>
        /// <param name="strICCardID">IC卡卡号</param>
        private EnmFaultType ReadICCardByComID(out string strPhysicalCardID, out string strICCardID, int nComID)
        {
            strPhysicalCardID = string.Empty;
            strICCardID = string.Empty;
            try
            {
                //byte[] btICCardData = new byte[16];
                CICCardReaderObj cardReader = CCommonMethods.myMethods.GetReaderObj(nComID);
                cardReader.ConnectCOM();
                if (cardReader.isComConnected)
                {
                    bool bReadOK = cardReader.GetICCardID(out strPhysicalCardID);
                    if (!bReadOK)
                    {
                        cardReader.DisConnectCOM();
                        return EnmFaultType.FailToReadICCard;//读取IC卡失败
                    }
                    //                 string strDataRead = string.Empty;
                    //int nIsGetICCardID = cardReader.ReadDataFromCard(1, 0, ref strDataRead);
                    //                 if (0 == cardReader.ReadDataFromCard(1, 0, ref strDataRead))
                    //                 {
                    //                     strICCardID = strDataRead.Substring(0, strDataRead.Length - 1 - "yyyyMMdd".Length - 2);//卡号 + 类型("1") + 制卡时间("yyyyMMdd") + 收费标准("01")
                    //                     return EnmFaultType.LogicCardID;//逻辑卡号
                    //                 }

                    CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCardByPhysCardID(strPhysicalCardID);
                    if (null != ICCardTable)
                    {
                        strICCardID = ICCardTable.iccode;
                        cardReader.DisConnectCOM();
                        return EnmFaultType.LogicCardID;//逻辑卡号
                    }
                    int nDisConnectTime = 0;
                    do
                    {
                        if (3 <= nDisConnectTime)
                        {
                            break;
                        }
                        cardReader.DisConnectCOM();
                        nDisConnectTime++;
                    } while (cardReader.isComConnected);
                    return EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
                }
                return EnmFaultType.FailConnection;//连接读卡器失败
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ReadICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType AddNewICCard(string strPhysicalCardID, string strICCardID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strPhysicalCardID) || string.IsNullOrWhiteSpace(strICCardID))
                {
                    return EnmFaultType.Fail;//传入的物理卡号或逻辑卡号为空
                }

                CICCardDto icCardTBLByPhys = CCommonMethods.myMethods.GetICCardByPhysCardID(strPhysicalCardID);
                CICCardDto icCardTBLByLogic = CCommonMethods.myMethods.GetICCard(strICCardID);
                if (null != icCardTBLByPhys)
                {
                    if (icCardTBLByPhys.iccode == strICCardID)
                    {
                        return EnmFaultType.HasICCardInfo;// 当前IC卡已经制卡
                    }
                    else
                    {
                        return EnmFaultType.ModifyICCardID;   //逻辑卡号不一致确认是否修改逻辑卡号
                    }
                }
                if (null != icCardTBLByLogic)
                {
                    return EnmFaultType.ICCardIDNotAllowed;  //输入的逻辑卡号已经存在
                }
                CICCardDto ICCardTable = new CICCardDto();
                ICCardTable.iccode = strICCardID;
                ICCardTable.phycode = strPhysicalCardID;
                ICCardTable.ictype = (int)EnmICCardType.Temp;
                ICCardTable.icstatus = (int)EnmICCardStatus.Normal;
                ICCardTable.icnewtime = DateTime.Now;
                if (!CCommonMethods.myMethods.InsertICCardInfo(ICCardTable))
                {
                    return EnmFaultType.FailToInsert;
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.AddNewICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 查询IC卡信息
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        /// </summary>
        /// <param name="icCardTable"></param>
        public EnmFaultType QueryICCardInfo(ref CICCardDto icCardTable)
        {
            try
            {//EnmFaultType enmResult = CCommonMethods.myMethods.CheckICCardID(icCardTable.iccode);
                //if (enmResult != EnmFaultType.Success)
                //{
                //    icCardTable = null;
                //    return enmResult;
                //}
                if (string.Empty != icCardTable.iccode)
                {
                    icCardTable = CCommonMethods.myMethods.GetICCard(icCardTable.iccode);
                    return EnmFaultType.Success;
                }
                else
                {
                    return EnmFaultType.Fail;
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 查询IC卡信息
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;
        /// </summary>
        /// <param name="icCardTable"></param>
        public EnmFaultType QueryICCardInfoByPhyID(ref CICCardDto icCardTable)
        {
            try
            {
                if (string.Empty != icCardTable.phycode)
                {
                    icCardTable = CCommonMethods.myMethods.GetICCardByPhysCardID(icCardTable.phycode);
                    return EnmFaultType.Success;
                }
                else
                {
                    return EnmFaultType.Fail;
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCardInfoByPhyID 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 依物理卡号，查询相应的用户卡号
        /// </summary>
        /// <param name="physic"></param>
        /// <returns></returns>
        public string QueryICCodeByPhysic(string physic) 
        {
            try 
            {
                CICCardDto iccd = CCommonMethods.myMethods.GetICCardByPhysCardID(physic);
                if (iccd != null) 
                {
                    return iccd.iccode;
                }
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCodeByPhysic 异常", ex.ToString());
            }
            return null;
        }
       
        /// <summary>
        /// 查询IC卡信息(根据车位地址)
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;
        /// add by wanxiaona20150731
        /// </summary>
        /// <param name="icCardTable"></param>
        public EnmFaultType QueryICCardInfoByAddress(ref CICCardDto icCardTable)
        {
            try
            {
                //EnmFaultType enmResult = CCommonMethods.myMethods.CheckICCardID(icCardTable.iccode);
                //if (enmResult != EnmFaultType.Success)
                //{
                //    icCardTable = null;
                //    return enmResult;
                //}
                if (!CBaseMethods.MyBase.IsEmpty(icCardTable.warehouse))
                {
                    icCardTable = CCommonMethods.myMethods.GetICCard((int)icCardTable.warehouse, icCardTable.carlocaddr);
                    return EnmFaultType.Success;
                }
                else
                {
                    icCardTable = null;
                    return EnmFaultType.Fail;
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCardInfoByAddress 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 更新IC卡信息
        /// </summary>
        /// <param name="icCardTable"></param>
        public bool UpdateICCardInfo(CICCardDto icCardTable)
        {
            try
            {
                if (!CCommonMethods.myMethods.UpdateICCardInfo(icCardTable))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.UpdateICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取当前剩余定期卡预留车位的个数（可定制的定期卡）
        /// </summary>
        /// <returns></returns>
        public int GetFixReservedSizeCount()
        {
            try
            {
                return CCommonMethods.myMethods.GetFixReservedSizeCount();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetFixReservedSizeCount 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

        /// <summary>
        /// 批量修改固定卡截止日期推迟之后时间
        /// 成功：EnmFaultType.Success;
        /// 失败：
        /// </summary>
        /// <param name="nDelayDays"></param>
        /// <param name="lstICCardID"></param>
        /// <returns></returns>
        public EnmFaultType BatchModifyICCardDeadLine(int nDelayDays, List<string> lstICCardID)
        {
            try
            {
                if (null == lstICCardID || 0 >= lstICCardID.Count)
                {
                    return EnmFaultType.Fail;
                }
                foreach(string strICCardID in lstICCardID)
                {
                    CICCardDto icCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);

                    if (null == icCardTable.icdeadline)
                    {
                        continue;
                    }
                    icCardTable.icdeadline = icCardTable.icdeadline.Value.AddDays(nDelayDays);
                    CCommonMethods.myMethods.UpdateICCardInfo(icCardTable);
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 批量修改计费标准
        /// 成功：EnmFaultType.Success;
        /// 失败：
        /// </summary>
        /// <param name="nDelayDays"></param>
        /// <param name="lstICCardID"></param>
        /// <returns></returns>
        public EnmFaultType BatchModifyICCardTariffID(int nTariffID, List<string> lstICCardID)
        {
            try
            {
                if (null == lstICCardID || 0 >= lstICCardID.Count)
                {
                    return EnmFaultType.Fail;
                }
                foreach (string strICCardID in lstICCardID)
                {
                    CICCardDto icCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);

                    if (null == icCardTable.tariffid)
                    {
                        continue;
                    }
                    icCardTable.tariffid = nTariffID;
                    CCommonMethods.myMethods.UpdateICCardInfo(icCardTable);
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
        #endregion 对IC卡的处理

        #region 对车主的管理
        /// <summary>
        /// 无条件查询所有车主信息，返回所有车主信息列表
        /// </summary>
        /// <param name="lstStruCUSTInfo"></param>
        /// <returns></returns>
        public bool QueryCUSTInfo(ref List<struCustomerInfo> lstStruCUSTInfo)
        {
            try
            {
                List<CCarCustomerDto> lstCarCustomerTBL = CCommonMethods.myMethods.GetCUSTInfoList();

                foreach (CCarCustomerDto carCustomerTBL in lstCarCustomerTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(carCustomerTBL));
                }
                return true;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryCUSTInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// nQueryFlag值含义：
        /// 1：姓名
        /// 2：移动电话
        /// 3：车牌号
        /// 4：IC卡卡号（逻辑卡号）
        /// 5：分配车位
        /// </summary>
        public void QueryCUSTInfo(EnmQueryFlag enmQueryFlag, string strInfo, ref List<struCustomerInfo> lstStruCUSTInfo)
        {
            try
            {
                switch (enmQueryFlag)
                {
                    case EnmQueryFlag.Name://姓名
                        {
                            lstStruCUSTInfo = GetCUSTByName(strInfo);
                            break;
                        }
                    case EnmQueryFlag.Mobile: //移动电话
                        {
                            lstStruCUSTInfo = GetCUSTByMobile(strInfo);
                            break;
                        }
                    case EnmQueryFlag.LicPlteNbr: //车牌号
                        {
                            lstStruCUSTInfo = GetCUSTByLicPlteNum(strInfo);
                            break;
                        }
                    case EnmQueryFlag.ICCardID: //IC卡卡号（逻辑卡号）
                        {
                            lstStruCUSTInfo = GetCUSTByICCardID(strInfo);
                            break;
                        }
                    case EnmQueryFlag.CarPOSN: //分配车位
                        {
                            lstStruCUSTInfo = GetCUSTByCarPOSN(strInfo);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryCUSTInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 为车主绑定IC卡；
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
        /// <param name="customerInfo">用户IC卡对象</param>
        /// <returns></returns>
        public EnmFaultType BoundICCardForCUST(struCustomerInfo customerInfo)
        {
            try
            {//ic卡卡号不允许为空
                if (string.IsNullOrWhiteSpace(customerInfo.strICCardID))
                {
                    return EnmFaultType.Fail;
                }

                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(customerInfo.strICCardID);
                if (null != deviceStatusTBL)
                {
                    return EnmFaultType.TaskOnICCard;//当前卡有作业正在操作
                }

                if (customerInfo.nICCardType == (int)EnmICCardType.FixedLocation)
                {
                    CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(customerInfo.nWareHouse, customerInfo.strCarPOSN);
                    if (null == carPOSNTable)
                    {
                        return EnmFaultType.NotFoundCarPOSN;//没有找到要绑定的车位
                    }

                    CICCardDto ICCardTableByCarPOSN = CCommonMethods.myMethods.GetICCardByCarPOSN(customerInfo.strCarPOSN);
                    if (null != ICCardTableByCarPOSN && ICCardTableByCarPOSN.iccode != customerInfo.strICCardID)
                    {
                        return EnmFaultType.FixedCarPOSN;//指定车位已经绑定其他车主
                    }

                    CCarLocationDto carLOCTable = CCommonMethods.myMethods.GetCarLocation(customerInfo.strICCardID);
                    if (null != carLOCTable)
                    {
                        //有计费功能：有车存在车库就不允许修改绑定的用户信息
                        if (CConfigManagement.myPara.BillingFlag)
                        {
                            return EnmFaultType.CarInGarage;//有车存在车库
                        }
                        //没有计费功能：有车存在车库且车辆存放的车位与要修改的车位不同则不允许修改绑定的用户信息
                        else if (carLOCTable.carlocaddr != customerInfo.strCarPOSN)
                        {
                            return EnmFaultType.CarInGarage;//有车存在车库且要绑定的车位与存车的车位不一致
                        }
                    }
                }

                CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(customerInfo.strICCardID);
                if (null == ICCardTable)
                {
                    string strTmp = string.Format("逻辑卡号{0}在数据库中不存在", customerInfo.strICCardID);
                    //CLOGException.Trace("BoundICCardForCUST", strTmp);
                    return EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
                }

                if (!CBaseMethods.MyBase.IsEmpty(ICCardTable.userid))
                {
                    return EnmFaultType.BoundUser;//当前卡已经绑定车主信息
                }

                if ((int)EnmICCardStatus.Normal != customerInfo.nICCardStatus)
                {
                    return EnmFaultType.LossORCancel;//IC卡挂失或注销
                }

                CCarCustomerDto customer = new CCarCustomerDto();
                customer.useraddr = customerInfo.strAddress;
                customer.carnumber = customerInfo.strLicPlteNbr;
                customer.usermobile = customerInfo.strMobile;
                customer.username = customerInfo.strName;
                customer.userphone = customerInfo.strTelphone;

                int nIDCounter = 1;
                while (null != CCommonMethods.myMethods.GetCUSTByID(nIDCounter))
                {
                    nIDCounter++;
                }
                customer.userid = nIDCounter;

                if (!CCommonMethods.myMethods.InsertCarCustomerInfo(customer))
                {
                    return EnmFaultType.FailToInsert;//插入数据库失败
                }

                ICCardTable.userid = nIDCounter;
                ICCardTable.icstatus = customerInfo.nICCardStatus;
                ICCardTable.ictype = customerInfo.nICCardType;
                ICCardTable.warehouse = customerInfo.nWareHouse;
                ICCardTable.carlocaddr = customerInfo.strCarPOSN;
                ICCardTable.tariffid = customerInfo.nTariffID;
                ICCardTable.priorityid = customerInfo.nPriorityID;
                ICCardTable.icstarttime = customerInfo.dtStartTime;
                ICCardTable.icdeadline = customerInfo.dtDeadLine;
                if (!CCommonMethods.myMethods.UpdateICCardInfo(ICCardTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.BoundICCardForCUST 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifyCUSTInfo(struCustomerInfo customerInfo)
        {
            try
            {//ic卡卡号不允许为空
                if (string.IsNullOrWhiteSpace(customerInfo.strICCardID))
                {
                    return EnmFaultType.Fail;
                }

                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(customerInfo.strICCardID);
                if (null != deviceStatusTBL)
                {
                    return EnmFaultType.TaskOnICCard;//当前卡有作业正在操作
                }

                CCarLocationDto carLOCTable = CCommonMethods.myMethods.GetCarLocation(customerInfo.strICCardID);
                if (customerInfo.nICCardType == (int)EnmICCardType.FixedLocation)
                {
                    CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(customerInfo.nWareHouse, customerInfo.strCarPOSN);
                    if (null == carPOSNTable)
                    {
                        return EnmFaultType.NotFoundCarPOSN;//没有找到要绑定的车位
                    }

                    CICCardDto ICCardTableByCarPOSN = CCommonMethods.myMethods.GetICCardByCarPOSN(customerInfo.strCarPOSN);
                    if (null != ICCardTableByCarPOSN && ICCardTableByCarPOSN.iccode != customerInfo.strICCardID)
                    {
                        return EnmFaultType.FixedCarPOSN;//指定车位已经绑定其他车主
                    }

                    if (null != carLOCTable)
                    {
                        if (carLOCTable.carlocaddr != customerInfo.strCarPOSN)
                        {
                            return EnmFaultType.CarInGarage;//有车存在车库且要绑定的车位与存车的车位不一致
                        }
                    }
                }

                CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(customerInfo.strICCardID);
                if (null == ICCardTable)
                {
                    //CLOGException.Trace("ModifyCUSTInfo", string.Format("逻辑卡号{0}在数据库中不存在", customerInfo.strICCardID));
                    return EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
                }
                if (CBaseMethods.MyBase.IsEmpty(ICCardTable.userid))
                {
                    return EnmFaultType.NoBoundCustomer;//当前卡没有绑定车主信息
                }

                if ((int)EnmICCardStatus.Normal != customerInfo.nICCardStatus)
                {
                    return EnmFaultType.LossORCancel;//IC卡挂失或注销
                }

                //----add by wanxiaona20150609
                //有计费功能：修改绑定
                if (CConfigManagement.myPara.BillingFlag)
                {
                    // 临时卡修改
                    if ((int)EnmICCardType.Temp == ICCardTable.ictype)
                    {// 修改成其他卡类型时且已存车:要求将存车时间到当前时间的临时停车费用缴清再进行修改
                        if (ICCardTable.ictype != customerInfo.nICCardType && null != carLOCTable)
                        {
                            return EnmFaultType.CarInGarage;//有车存在车库
                        }
                    }
                    else
                    {
                        //IC卡重来没有缴过费->第一次绑定
                        if (null == ICCardTable.icdeadline)
                        { }
                        //IC卡已过期
                        else if (ICCardTable.icdeadline < DateTime.Now)
                        { }
                        //IC卡未过期
                        else
                        {

                        }
                    }
                }
                //----add by wanxiaona20150609

                CCarCustomerDto customer = CCommonMethods.myMethods.GetCUSTByID((int)ICCardTable.userid);
                customer.useraddr = customerInfo.strAddress;
                customer.carnumber = customerInfo.strLicPlteNbr;
                customer.usermobile = customerInfo.strMobile;
                customer.username = customerInfo.strName;
                customer.userphone = customerInfo.strTelphone;

                if (!CCommonMethods.myMethods.UpdateCarCustomerInfo(customer))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                ICCardTable.icstatus = customerInfo.nICCardStatus;
                ICCardTable.ictype = customerInfo.nICCardType;
                ICCardTable.warehouse = customerInfo.nWareHouse;
                ICCardTable.carlocaddr = customerInfo.strCarPOSN;
                ICCardTable.tariffid = customerInfo.nTariffID;
                ICCardTable.priorityid = customerInfo.nPriorityID;
                ICCardTable.icstarttime = customerInfo.dtStartTime;
                ICCardTable.icdeadline = customerInfo.dtDeadLine;
                if (!CCommonMethods.myMethods.UpdateICCardInfo(ICCardTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ModifyCUSTInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 删除车主信息
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;
        /// EnmFaultType.NoCustomerInfo;//没有车主信息
        /// EnmFaultType.BoundUser;//解除绑定成功，还有其他的IC卡绑定该车主
        /// 失败：
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.FailToDelete;//删除数据库失败
        /// EnmFaultType.FailToUpdate;//更新数据库失败
        /// EnmFaultType.NoBoundCustomer;//当前卡没有绑定车主
        /// EnmFaultType.CarInGarage;//当前卡有车存在车库
        /// EnmFaultType.TaskOnICCard;//当前卡有作业正在操作
        /// </summary>
        /// <param name="nUserID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public EnmFaultType DeleteCustomer(string strICCardID)
        {
            try
            {
                CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);
                if (null == ICCardTable)
                {
                    return EnmFaultType.NoICCardInfo;//未制卡
                }
                //List<CICCardDto> lstICCardTable = GetAllICCardByUserID(nUserID);
                if (null != CCommonMethods.myMethods.GetCarLocation(strICCardID))
                {
                    return EnmFaultType.CarInGarage;//当前卡有车存在车库
                }

                CDeviceStatusDto deviceStatusTable = CCommonMethods.myMethods.GetDeviceStatus(strICCardID);
                if (null != deviceStatusTable && !CBaseMethods.MyBase.IsEmpty(deviceStatusTable.tasktype))
                {
                    return EnmFaultType.TaskOnICCard;//当前卡有作业正在操作
                }

                //删除车主信息
                //             CICCardDto ICCardTable = GetICCard(strICCardID);
                //             if (null == ICCardTable)
                //             {
                //                 return EnmFaultType.NoICCardInfo;//没有制卡
                //             }
                if (null == ICCardTable.userid)
                {
                    return EnmFaultType.NoBoundCustomer;//当前卡没有绑定车主
                }
                CCarCustomerDto customerTable = CCommonMethods.myMethods.GetCUSTByID((int)ICCardTable.userid);
                if (null == customerTable)
                {
                    return EnmFaultType.NoCustomerInfo;//没有车主信息
                }
                List<CICCardDto> lstICCardTBL = CCommonMethods.myMethods.GetICCardList((int)ICCardTable.userid);

                foreach (CICCardDto ICCardTBL in lstICCardTBL)
                {
                    if (strICCardID == ICCardTBL.iccode)
                    {
                        if (!CCommonMethods.myMethods.InitICCardInfo(ICCardTable))
                        {
                            return EnmFaultType.FailToUpdate;//更新数据库失败
                        }
                    }
                    if (strICCardID != ICCardTBL.iccode)
                    {
                        return EnmFaultType.BoundUser;//解除绑定成功，还有其他的IC卡绑定该车主
                    }
                }

                if (!CCommonMethods.myMethods.DeleteCarCustomerInfo(customerTable))
                {
                    return EnmFaultType.FailToDelete;//删除数据库失败
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteCustomer 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 根据用户姓名获取车主信息
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        private List<struCustomerInfo> GetCUSTByName(string strUserName)
        {
            try
            {
                List<struCustomerInfo> lstStruCUSTInfo = new List<struCustomerInfo>();
                List<CCarCustomerDto> lstCarCustomerTBL = CCommonMethods.myMethods.GetCUSTByName(strUserName);
                foreach (CCarCustomerDto carCustomerTBL in lstCarCustomerTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(carCustomerTBL));
                }
                return lstStruCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCUSTByName 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<struCustomerInfo>();
            }
        }

        /// <summary>
        /// 根据用户移动电话获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        private List<struCustomerInfo> GetCUSTByMobile(string strMobile)
        {
            try
            {
                List<struCustomerInfo> lstStruCUSTInfo = new List<struCustomerInfo>();
                List<CCarCustomerDto> lstCarCustomerTBL = CCommonMethods.myMethods.GetCUSTByMobile(strMobile);
                foreach (CCarCustomerDto carCustomerTBL in lstCarCustomerTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(carCustomerTBL));
                }
                return lstStruCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCUSTByMobile 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<struCustomerInfo>();
            }
        }

        /// <summary>
        /// 根据车牌号获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        public List<struCustomerInfo> GetCUSTByLicPlteNum(string strLicPlteNbr)
        {
            try
            {
                List<struCustomerInfo> lstStruCUSTInfo = new List<struCustomerInfo>();
                List<CCarCustomerDto> lstCarCustomerTBL = CCommonMethods.myMethods.GetCUSTByLicPlteNum(strLicPlteNbr);
                foreach (CCarCustomerDto carCustomerTBL in lstCarCustomerTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(carCustomerTBL));
                }
                return lstStruCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCUSTByLicPlteNum 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<struCustomerInfo>();
            }
        }

        /// <summary>
        /// 根据IC卡卡号获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        private List<struCustomerInfo> GetCUSTByICCardID(string strICCardID)
        {
            try
            {
                List<struCustomerInfo> lstStruCUSTInfo = new List<struCustomerInfo>();
                List<CICCardDto> lstICCardTBL = CCommonMethods.myMethods.GetICCardListICCardID(strICCardID);
                foreach (CICCardDto icCardTBL in lstICCardTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(icCardTBL));
                }
                return lstStruCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCUSTByICCardID 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<struCustomerInfo>();
            }
        }

        /// <summary>
        /// 根据车位地址获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        public List<struCustomerInfo> GetCUSTByCarPOSN(string strCarPOSN)
        {
            try
            {
                List<struCustomerInfo> lstStruCUSTInfo = new List<struCustomerInfo>();
                List<CICCardDto> lstICCardTBL = CCommonMethods.myMethods.GetICCardListByCarPOSN(strCarPOSN);
                foreach (CICCardDto icCardTBL in lstICCardTBL)
                {
                    lstStruCUSTInfo.Add(GetStruCUSTInfo(icCardTBL));
                }
                return lstStruCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCUSTByCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<struCustomerInfo>();
            }
        }

        /// <summary>
        /// 拼装车主信息结构体
        /// </summary>
        /// <param name="carCustomerTBL"></param>
        /// <returns></returns>
        private struCustomerInfo GetStruCUSTInfo(CCarCustomerDto carCustomerTBL)
        {
            struCustomerInfo struCUSTInfo = new struCustomerInfo();
            try
            {
                CICCardDto icCardTBL = CCommonMethods.myMethods.GetICCard(carCustomerTBL.userid);
                if (null != icCardTBL)
                {
                    struCUSTInfo.strICCardID = icCardTBL.iccode;
                    struCUSTInfo.nICCardStatus = (int)icCardTBL.icstatus;
                    struCUSTInfo.nICCardType = (int)icCardTBL.ictype;

                    if (null != icCardTBL.warehouse)
                    {
                        struCUSTInfo.nWareHouse = (int)icCardTBL.warehouse;
                    }

                    if (null != icCardTBL.tariffid)
                    {
                        struCUSTInfo.nTariffID = (int)icCardTBL.tariffid;
                    }

                    struCUSTInfo.strCarPOSN = icCardTBL.carlocaddr;

                    if (null != icCardTBL.priorityid)
                    {
                        struCUSTInfo.nPriorityID = (int)icCardTBL.priorityid;
                    }

                    if (null != icCardTBL.icstarttime)
                    {
                        struCUSTInfo.dtStartTime = icCardTBL.icstarttime.Value;
                    }
                    if (null != icCardTBL.icdeadline)
                    {
                        struCUSTInfo.dtDeadLine = icCardTBL.icdeadline.Value;
                    }
                }
                struCUSTInfo.strName = carCustomerTBL.username;
                struCUSTInfo.strMobile = carCustomerTBL.usermobile;
                struCUSTInfo.strLicPlteNbr = carCustomerTBL.carnumber;
                struCUSTInfo.strTelphone = carCustomerTBL.userphone;
                struCUSTInfo.strAddress = carCustomerTBL.useraddr;
                return struCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetStruCUSTInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return struCUSTInfo;
            }
        }

        /// <summary>
        /// 拼装车主信息结构体
        /// </summary>
        /// <param name="carCustomerTBL"></param>
        /// <returns></returns>
        private struCustomerInfo GetStruCUSTInfo(CICCardDto icCardTBL)
        {
            struCustomerInfo struCUSTInfo = new struCustomerInfo();
            try
            {
                CCarCustomerDto carCustomerTBL = CCommonMethods.myMethods.GetCUSTByID((int)icCardTBL.userid);
                struCUSTInfo.strName = carCustomerTBL.username;
                struCUSTInfo.strICCardID = icCardTBL.iccode;
                struCUSTInfo.nICCardStatus = (int)icCardTBL.icstatus;
                struCUSTInfo.nICCardType = (int)icCardTBL.ictype;
                struCUSTInfo.nWareHouse = (int)icCardTBL.warehouse;
                struCUSTInfo.strCarPOSN = icCardTBL.carlocaddr;
                if (null != icCardTBL.tariffid)
                {
                    struCUSTInfo.nTariffID = (int)icCardTBL.tariffid;
                }

                if (null != icCardTBL.priorityid)
                {
                    struCUSTInfo.nPriorityID = (int)icCardTBL.priorityid;
                }

                if (null != icCardTBL.icstarttime)
                {
                    struCUSTInfo.dtStartTime = icCardTBL.icstarttime.Value;
                }

                if (null != icCardTBL.icdeadline)
                {
                    struCUSTInfo.dtDeadLine = icCardTBL.icdeadline.Value;
                }

                struCUSTInfo.strMobile = carCustomerTBL.usermobile;
                struCUSTInfo.strLicPlteNbr = carCustomerTBL.carnumber;
                struCUSTInfo.strTelphone = carCustomerTBL.userphone;
                struCUSTInfo.strAddress = carCustomerTBL.useraddr;
                return struCUSTInfo;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetStruCUSTInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return struCUSTInfo;
            }
        }
        #endregion 对车主的管理

        #region 对车位信息的管理
        /// <summary>
        /// 查询车位信息(根据库区号和车位地址)
        /// 返回值：
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;
        /// </summary>
        /// <param name="carLOCTable"></param>
        /// <param name="nICCardType"></param>
        /// <returns></returns>
        public EnmFaultType QueryCarPOSN(ref CCarLocationDto carLOCTable, ref CICCardDto icCardTBL)
        {
            try
            {
                if (null == carLOCTable && null == carLOCTable.warehouse)
                {
                    return EnmFaultType.Fail;
                }
                carLOCTable = CCommonMethods.myMethods.GetCarLocation((int)carLOCTable.warehouse, carLOCTable.carlocaddr);
                if (null == carLOCTable)
                {
                    return EnmFaultType.Fail;
                }
                icCardTBL = CCommonMethods.myMethods.GetICCard(carLOCTable.iccode);
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType QueryCarPOSN(string strICCardID, out CCarLocationDto carLocatioTBL)
        {
            carLocatioTBL = null;
            try
            {
                EnmFaultType enmResult = CCommonMethods.myMethods.CheckICCardID(strICCardID);
                if (enmResult != EnmFaultType.Success)
                {
                    return enmResult;
                }
                carLocatioTBL = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if (null == carLocatioTBL)
                {
                    return EnmFaultType.NotFoundCarPOSN;//当前IC卡没有存车
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 查询车位信息（根据库区号）
        /// </summary>
        /// <param name="nWareHouse"></param>
        public List<CCarLocationDto> QueryCarPOSN(int nWareHouse)
        {
            try
            {
                return CCommonMethods.myMethods.GetCarLocationList(nWareHouse);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CCarLocationDto>();
            }
        }

        /// <summary>
        /// 获取车位个数（根据库区号和车位类型）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarPOSNType"></param>
        /// <returns></returns>
        public int GetCarPOSNCount(int nWareHouse, int nCarPOSNType)
        {
            try
            {
                return CCommonMethods.myMethods.GetCarLocationList(nWareHouse, nCarPOSNType).Count;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCarPOSNCount 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

        /// <summary>
        /// 获取车位个数（根据库区号、车位类型和车位状态）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarPOSNType"></param>
        /// <param name="nCarPOSNStatus"></param>
        /// <returns></returns>
        public int GetCarPOSNCount(int nWareHouse, int nCarPOSNType, int nCarPOSNStatus)
        {
            try
            {
                return CCommonMethods.myMethods.GetCarLocationList(nWareHouse, nCarPOSNType, nCarPOSNStatus).Count;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCarPOSNCount 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

        /// <summary>
        /// 获取某库区的总车位数、空闲、占用、空余大车位值
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="rectVehCount"></param>
        public void GetCarPOSNFreeOccupyCount(int nWareHouse, ref Rectangle rectVehCount)
        {
            try
            {
                CCommonMethods.myMethods.GetCarPOSNFreeOccupyCount(nWareHouse, ref rectVehCount);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetCarPOSNCount 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 获取车位状态（根据IC卡卡号）
        /// </summary>
        /// <param name="enmLocationStatus"></param>
        /// <param name="strICCardID"></param>
        public void GetCarPLSNStatus(ref EnmLocationStatus enmLocationStatus, string strICCardID,
            EnmICCardType enmICType, EnmICCardStatus enmICStatus, EnmFeeType enmFeeType,
            EnmTaskType enmTask, EnmSwipeCount enmSwipe, EnmFlowNodeDescp enmFlow)
        {
        }
        #endregion 对车位信息的管理

        #region 对设备信息的管理
        /// <summary>
        /// 获取设备模式
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceID"></param>
        /// <returns></returns>
        public EnmModel GetDeviceMode(int nWareHouse, int nDeviceID)
        {
            try
            {
                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nDeviceID);
                if (null == deviceStatusTBL)
                {
                    return EnmModel.Maintenance;
                }
                return (EnmModel)deviceStatusTBL.devicemode;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceMode 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

        /// <summary>
        /// 获取设备状态具体实例(根据库号和设备号),deviceStatusTBL中的库区号和设备号不能为空
        /// </summary>
        /// <param name="deviceStatusTBL"></param>
        public void GetDeviceStatus(out CDeviceStatusDto deviceStatusTBL, int nWareHouse, int nDeviceCode)
        {
            try
            {
                deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nDeviceCode);
            }
            catch (Exception ex)
            {//打印日志
                deviceStatusTBL = null;
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceStatus 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 获取设备状态具体实例(根据库号和设备地址),deviceStatusTBL中的库区号和设备号不能为空
        /// </summary>
        /// <param name="deviceStatusTBL"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="strDeviceAddr"></param>
        public void GetDeviceStatus(out CDeviceStatusDto deviceStatusTBL, int nWareHouse, string strDeviceAddr, EnmSMGType enmSMGType)
        {
            try
            {
                deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, strDeviceAddr, enmSMGType);
            }
            catch (Exception ex)
            {//打印日志
                deviceStatusTBL = null;
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceStatus 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 获取设备故障列表（获取与指定据库号和设备号相同，且于指定故障显示内容不同的故障信息）
        /// </summary>
        /// <param name="lstDeviceFault">设备故障列表</param>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="strFaultDesc">故障显示内容</param>
        public void GetDeviceFault(out List<CDeviceFaultDto> lstDeviceFault, int nWareHouse, int nEquipID, string strFaultDesc)
        {
            try
            {
                lstDeviceFault = CCommonMethods.myMethods.GetDeviceFaultList(nWareHouse, nEquipID, strFaultDesc);
            }
            catch (Exception ex)
            {//打印日志
                lstDeviceFault = new List<CDeviceFaultDto>();
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceFault 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 获取对应库的所有车厅或者ETV/TV设备号列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="enmSMGType"></param>
        /// <returns></returns>
        public List<object> GetDeviceIDList(int nWareHouse, EnmSMGType enmSMGType)//CDataContract dataContract
        {
            try
            {
                List<object> lstDeviceID = new List<object>();
                switch (enmSMGType)//dataContract.EnmSMGType
                {
                    case EnmSMGType.ETV:
                        {
                            if (!CConfigManagement.myPara.HashETVOrTVDeviceID.ContainsKey(nWareHouse))
                            {
                                //CLOGException.Trace("IEGClientAccessService.CClientAccessService.GetDeviceIDList", "获取ETV设备ID，无效库区号");
                                break;
                            }
                            lstDeviceID = ((List<int>)CConfigManagement.myPara.HashETVOrTVDeviceID[nWareHouse]).Cast<object>().ToList();//.GetETVOrTVDeviceID(nWareHouse).Cast<object>().ToList();
                            break;
                        }
                    case EnmSMGType.Hall:
                        {
                            if (!CConfigManagement.myPara.HashHallDeviceID.ContainsKey(nWareHouse))
                            {
                                //CLOGException.Trace("IEGClientAccessService.CClientAccessService.GetDeviceIDList", "获取Hall设备ID，无效库区号");
                                break;
                            }
                            lstDeviceID = ((List<int>)CConfigManagement.myPara.HashHallDeviceID[nWareHouse]).Cast<object>().ToList();//.Cast<object>().ToList();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                return lstDeviceID;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceFault 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<object>();
            }
        }

        /// <summary>
        /// 获取nWareHouse库的所有设备状态列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetDeviceStatusList(int nWareHouse)
        {
            try
            {
                return CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceStatusList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CDeviceStatusDto>();
            }
        }
        #endregion 对设备信息的管理

        #region 对取车队列的管理
        /// <summary>
        /// 查询对应库区的对应车厅的取车队列
        /// </summary>
        /// <param name="nWareHouse">库区ID</param>
        /// <param name="nEquipID">设备ID</param>
        /// <returns></returns>
        public List<CWorkQueueDto> QueryWorkQueue(int nWareHouse, int nEquipID)
        {
            try
            {
                return CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nEquipID, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CWorkQueueDto>();
            }
        }

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
        public EnmFaultType DeleteWorkQueue(int nWareHouse, int nEquipID, string strICCardID)
        {
            try
            {
                return CCommonMethods.myMethods.DeleteWorkQueueInfo(nWareHouse, nEquipID, strICCardID);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifWorkQueue(string strSrcICCode, string strDestICCode)
        {
            try
            {
                if (!CCommonMethods.myMethods.ModifWorkQueue(strSrcICCode, strDestICCode))
                {
                    return EnmFaultType.FailToUpdate;
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ModifWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
        #endregion 对取车队列的管理

        #region 对查询统计日志信息的管理
        /// <summary>
        /// 获取所有IC卡缴费日志
        /// </summary>
        /// <returns></returns>
        public List<CICCardLogDto> GetICCardLogList()
        {
            try
            {
                return CCommonMethods.myMethods.GetICCardLogList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetICCardLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CICCardLogDto>();
            }
        }

        /// <summary>
        /// 获取所有IC卡缴费日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CICCardLogDto> GetICCardLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent)
        {
            List<CICCardLogDto> lstLogDto = new List<CICCardLogDto>();
            try
            {
                lstLogDto = CCommonMethods.myMethods.GetICCardLogList();
                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据卡号查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.paymenttime, dtStart, dtEnd) && s.iccode == strContent);
                            break;
                        }
                    case 1:// 根据卡类型查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.paymenttime, dtStart, dtEnd) && s.ictype.ToString() == strContent);
                            break;
                        }
                    case 2:// 根据车主姓名查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.paymenttime, dtStart, dtEnd) && s.username == strContent);
                            break;
                        }
                    case 3:// 根据操作员
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.paymenttime, dtStart, dtEnd) && s.optcode == strContent);
                            break;
                        }
                    default:
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.paymenttime, dtStart, dtEnd));
                            break;
                        }
                }
                return lstLogDto;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetICCardLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstLogDto;
            }
        }

        /// <summary>
        /// 写系统日志
        /// </summary>
        /// <param name="systemLogeTBL"></param>
        /// <returns></returns>
        public bool InsertSysLog(CSystemLogDto systemLogeTBL)
        {
            try
            {
                return CCommonMethods.myMethods.InsertSystemLogInfo(systemLogeTBL);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.InsertSysLog 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取所有系统日志
        /// </summary>
        /// <returns></returns>
        public List<CSystemLogDto> GetSystemLogList()
        {
            try
            {
                return CCommonMethods.myMethods.GetSystemLogList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetSystemLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CSystemLogDto>();
            }
        }

        /// <summary>
        /// 获取所有系统日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CSystemLogDto> GetSystemLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent)
        {
            List<CSystemLogDto> lstSystemLog = new List<CSystemLogDto>();
            try
            {
                lstSystemLog = CCommonMethods.myMethods.GetSystemLogList();

                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据描述关键字查询
                        {
                            lstSystemLog = lstSystemLog.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.curtime, dtStart, dtEnd) && s.logdescp.Contains(strContent));
                            break;
                        }
                    case 1:// 根据操作员
                        {
                            lstSystemLog = lstSystemLog.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.curtime, dtStart, dtEnd) && s.optcode == strContent);
                            break;
                        }
                    default:
                        {
                            lstSystemLog = lstSystemLog.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.curtime, dtStart, dtEnd));
                            break;
                        }
                }
                return lstSystemLog;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetSystemLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstSystemLog;
            }
        }

        /// <summary>
        /// 获取所有报文日志
        /// </summary>
        /// <returns></returns>
        public List<CTelegramLogDto> GetTelegramLogList()
        {
            try
            {
                return CCommonMethods.myMethods.GetTelegramLogList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTelegramLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CTelegramLogDto>();
            }
        }

        /// <summary>
        /// 获取所有报文日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CTelegramLogDto> GetTelegramLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent)
        {
            List<CTelegramLogDto> lstLogDto = new List<CTelegramLogDto>();
            try
            {
                //CSQLAccess.mySQL.getTableDtoList(out lstLogDto);
                lstLogDto = CCommonMethods.myMethods.GetTelegramLogList();

                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据日志类型查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.logtype == strContent);
                            break;
                        }
                    case 1:// 根据报文
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.telegramhead.Contains(strContent));
                            break;
                        }
                    case 2:// 根据设备号
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.devicecode.ToString() == strContent);
                            break;
                        }
                    case 3:// 根据IC卡号
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.iccode == strContent);
                            break;
                        }
                    case 4:// 查询存车次数
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.telegramhead == strContent);
                            break;
                        }
                    case 5:// 查询取车次数
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.telegramhead == strContent);
                            break;
                        }
                    default:
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd));
                            break;
                        }
                }

                return lstLogDto;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTelegramLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstLogDto;
            }
        }

        /// <summary>
        /// 获取所有报文日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CTelegramLogDto> GetTelegramLogList(int nIndex, DateTime dtStart, DateTime dtEnd, int nPLCID, string strContent)
        {
            List<CTelegramLogDto> lstLogDto = new List<CTelegramLogDto>();
            try
            {
                //CSQLAccess.mySQL.getTableDtoList(out lstLogDto);
                lstLogDto = CCommonMethods.myMethods.GetTelegramLogList();

                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据日志类型查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.logtype == strContent);
                            break;
                        }
                    case 1:// 根据报文
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.telegramhead.Contains(strContent));
                            break;
                        }
                    case 2:// 根据设备号
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.devicecode.ToString() == strContent);
                            break;
                        }
                    case 3:// 根据IC卡号
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.iccode == strContent);
                            break;
                        }
                    case 4:// 查询存车次数
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.telegramhead == strContent);
                            break;
                        }
                    case 5:// 查询取车次数
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID && s.telegramhead == strContent);
                            break;
                        }
                    case 6:// 根据库区
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID);
                            break;
                        }
                    default:
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse == nPLCID);
                            break;
                        }
                }

                return lstLogDto;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTelegramLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstLogDto;
            }
        }

        /// <summary>
        /// 获取所有设备状态日志
        /// </summary>
        /// <returns></returns>
        public List<CDeviceStatusLogDto> GetDeviceStatusLogList()
        {
            try
            {
                return CCommonMethods.myMethods.GetDeviceStatusLogList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceStatusLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CDeviceStatusLogDto>();
            }
        }

        /// <summary>
        /// 获取所有设备状态日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CDeviceStatusLogDto> GetDeviceStatusLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent)
        {
            List<CDeviceStatusLogDto> lstLogDto = new List<CDeviceStatusLogDto>();
            try
            {
                lstLogDto = CCommonMethods.myMethods.GetDeviceStatusLogList();
                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据库区查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse.ToString() == strContent);
                            break;
                        }
                    case 1:// 根据设备号
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.devicecode.ToString() == strContent);
                            break;
                        }
                    default:
                        lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd));
                        break;
                }

                return lstLogDto;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceStatusLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstLogDto;
            }
        }

        /// <summary>
        /// 写设备故障日志
        /// </summary>
        /// <param name="logTBL"></param>
        /// <returns></returns>
        public bool InsertDeviceFaultLog(CDeviceFaultLogDto logTBL)
        {
            try
            {
                return CCommonMethods.myMethods.InsertDeviceFaultLog(logTBL);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.InsertDeviceFaultLog 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取所有设备故障日志
        /// </summary>
        /// <returns></returns>
        public List<CDeviceFaultLogDto> GetDeviceFaultLogList()
        {
            try
            {
                return CCommonMethods.myMethods.GetDeviceFaultLogList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceFaultLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CDeviceFaultLogDto>();
            }
        }

        /// <summary>
        /// 获取所有设备故障日志(根据客户端条件)
        /// </summary>
        /// <returns></returns>
        public List<CDeviceFaultLogDto> GetDeviceFaultLogList(int nIndex, DateTime dtStart, DateTime dtEnd, string strContent)
        {
            List<CDeviceFaultLogDto> lstLogDto = new List<CDeviceFaultLogDto>();
            try
            {
                lstLogDto = CCommonMethods.myMethods.GetDeviceFaultLogList();
                // 根据查询条件查询
                switch (nIndex)
                {
                    case 0:// 根据库区查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.warehouse.ToString() == strContent);
                            break;
                        }
                    case 1:// 根据设备号查询
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.devicecode.ToString() == strContent);
                            break;
                        }
                    case 2:// 根据故障描述
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.faultdescp.Contains(strContent));
                            break;
                        }
                    case 3:// 根据操作员
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd) && s.optcode == strContent);
                            break;
                        }
                    default:
                        {
                            lstLogDto = lstLogDto.FindAll(s => CBaseMethods.MyBase.CompareDateTime(s.time, dtStart, dtEnd));
                            break;
                        }
                }
                return lstLogDto;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetDeviceFaultLogList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstLogDto;
            }
        }
        #endregion 对系统日志信息的管理

        #region 对故障处理的管理（手动作业）
        /// <summary>
        /// 获取所有作业类型列表
        /// </summary>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetTaskTypeList()
        {
            try
            {
                List<CDeviceStatusDto> lstTaskType = new List<CDeviceStatusDto>();
                lstTaskType = CCommonMethods.myMethods.GetDeviceStatusList();
                lstTaskType = lstTaskType.FindAll(s => !CBaseMethods.MyBase.IsEmpty(s.tasktype));//&& s.devicemode != (int)EnmModel.Automatic);
                return lstTaskType;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTaskTypeList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CDeviceStatusDto>();
            }
        }
        #endregion 对故障处理的管理（手动作业）

        #region 对语音信息的管理
        /// <summary>
        /// 获取所有语音列表
        /// </summary>
        /// <returns></returns>
        public List<CSoundDto> GetSoundList()
        {
            try
            {
                return CCommonMethods.myMethods.GetSoundList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetSoundList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CSoundDto>();
            }
        }

        /// <summary>
        /// 更新语音
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        public bool UpdateSoundDto(CSoundDto sound)
        {
            try
            {
                if (null == sound)
                {
                    return false;
                }

                string strSoundContent = CCommonMethods.myMethods.GetSoundContent(sound.soundcode);
                if (0 != string.Compare(strSoundContent, sound.soundcontent) && CCommonMethods.myMethods.UpdateSoundTable(sound))
                {
                    CSound objSound = new CSoundSpeech();
                    string strPathAuto = System.AppDomain.CurrentDomain.BaseDirectory + @"/soundAuto/";// 自动配好的语音文件夹路径(读取配置文件)
                    objSound.UpdateSoundFileContext(strPathAuto, sound.soundcode, sound.soundcontent);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.UpdateSoundDto 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 更新语音列表
        /// </summary>
        /// <param name="lstSound"></param>
        /// <returns></returns>
        public bool UpdateSoundDtoList(List<CSoundDto> lstSound)
        {
            try
            {
                foreach (CSoundDto sound in lstSound)
                {
                    UpdateSoundDto(sound);
                }

                return true;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.UpdateSoundDtoList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对语音信息的管理

        #region 对计费的管理
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
        public EnmFaultType ReadAndQueryParkingInfoB(ref struBillInfo struBillInfo,int region)
        {
            try
            {
                string strPhysicalCardID;
                string strICCardID;
                int nComID = CConfigManagement.myPara.BillingComID;// 读取配置文件
                EnmFaultType enmReturn = ReadICCardByComID(out strPhysicalCardID, out strICCardID, nComID);
                if (EnmFaultType.LogicCardID != enmReturn)
                {
                    return enmReturn;
                }
                struBillInfo.strICCardID = strICCardID;
                return QueryParkingInfo(ref struBillInfo,region);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ReadAndQueryParkingInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        ///  查询停车费用等信息（临时卡和固定卡等所有卡）---客户端使用
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
        public EnmFaultType ReadAndQueryParkingInfo(ref struBillInfo struBillInfo,int region)
        {
            try
            {
                string strPhysicalCardID;
                string strICCardID;
                int nComID = CConfigManagement.myPara.MaintainComID;// 读取配置文件
                EnmFaultType enmReturn = ReadICCardByComID(out strPhysicalCardID, out strICCardID, nComID);
                if (EnmFaultType.LogicCardID != enmReturn)
                {
                    return enmReturn;
                }
                struBillInfo.strICCardID = strICCardID;
                return QueryParkingInfo(ref struBillInfo,region);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ReadAndQueryParkingInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType QueryParkingInfo(ref struBillInfo struBillInfo,int region)
        {
            try
            {
                CBillingManagement billingMgmt = new CBillingManagement();
                CICCardDto icCardTBL = CCommonMethods.myMethods.GetICCard(struBillInfo.strICCardID);
                if (null == icCardTBL)
                {
                    return EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
                }
                if ((struBillInfo.nICCardType == (int)EnmICCardType.Temp && icCardTBL.ictype != (int)EnmICCardType.Temp)
                    || (0 == struBillInfo.nICCardType && icCardTBL.ictype == (int)EnmICCardType.Temp))
                {
                    return EnmFaultType.NotMatch;//IC卡类型不匹配 
                }

                if (null == icCardTBL.tariffid)
                {
                    return EnmFaultType.NoTariffInfo;// 获取计费标准信息失败
                }

                CTariffDto tariffTBL = CCommonMethods.myMethods.GetTariffByID((int)icCardTBL.tariffid);
                if (null == tariffTBL)
                {
                    return EnmFaultType.NoTariffInfo;// 获取计费标准信息失败
                }

                struBillInfo.nTariffID = tariffTBL.id;
                struBillInfo.nICCardType = (int)icCardTBL.ictype;
                struBillInfo.nFeeType = (int)tariffTBL.feetype;

				////modify by suhan 20150729
                CCarLocationDto carLocationTBL = CCommonMethods.myMethods.GetCarLocation(struBillInfo.strICCardID);
                //end
				
				if (icCardTBL.ictype == (int)EnmICCardType.Temp)//临时卡
                {
                    if (null == carLocationTBL)
                    {
                        return EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
                    }

                    struBillInfo.dtStartTime = (DateTime)carLocationTBL.carintime;
                    struBillInfo.nWareHouse = (int)carLocationTBL.warehouse;
                    CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                    taskTrigger.SetTaskTrigger(struBillInfo.nWareHouse);
                    struBillInfo.nHallID = taskTrigger.ALLOCHallByQueue(struBillInfo.nWareHouse, region, carLocationTBL);
                    struBillInfo.dtEndTime = DateTime.Now;
                }
                else
                {
                    if (null == icCardTBL.icstarttime || null == icCardTBL.icdeadline)
                    {
                        return EnmFaultType.NoBoundCustomer; //当前卡没有绑定车主计费信息
                    }
                  
					if (null != carLocationTBL)
                    {
                        struBillInfo.nWareHouse = (int)carLocationTBL.warehouse;
                        CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                        taskTrigger.SetTaskTrigger(struBillInfo.nWareHouse);
                        struBillInfo.nHallID = taskTrigger.ALLOCHallByQueue(struBillInfo.nWareHouse, region, carLocationTBL);
                    }
					
                    if (icCardTBL.icdeadline < DateTime.Now)
                    {
                        if (null != carLocationTBL)
                        {
                            struBillInfo.fPayableFee = -1;//当前卡已存车且IC卡已过期，请补交费用
                        }
                        else
                        {
                            struBillInfo.fPayableFee = 0;
                        }
                    }
                    // IC卡未过期
                    else
                    {
                        struBillInfo.fPayableFee = 0;
                    }
                    struBillInfo.dtStartTime = icCardTBL.icstarttime.Value;
                    struBillInfo.dtEndTime = icCardTBL.icdeadline.Value;
                }

                // 无限额时，按照一天费用计算
                float fee = null == tariffTBL.fee ? 0.0f : (float)tariffTBL.fee;
                if (null != tariffTBL.workdayquotafee)
                {
                    fee = tariffTBL.workdayquotafee < 0.1 ? (float)tariffTBL.fee : (float)tariffTBL.workdayquotafee;
                }
                struBillInfo.fTariffNorm = fee;

                string strClassName = string.Empty;
                object[] arrPara = null;
                //获取计费类型的对应类名及相应参数
                ArrayList lstBillingPara = (ArrayList)CConfigManagement.myPara.HashBillingPara[(int)tariffTBL.feetype];

                if (0 < lstBillingPara.Count)
                {
                    strClassName = (string)lstBillingPara[0];
                }

                if (1 < lstBillingPara.Count)
                {
                    arrPara = (object[])lstBillingPara[1];
                }

                if (string.IsNullOrWhiteSpace(strClassName))
                {
                    return EnmFaultType.Fail; //计算停车费用失败
                }

                billingMgmt.SetBillingStandard((CBillingStandard)Assembly.Load("BillingManagementLib").CreateInstance("BillingManagementLib." + strClassName, false, BindingFlags.Default, null, arrPara, null, null));
                billingMgmt.CalcParkingFees(ref struBillInfo, tariffTBL);               

                if (null == carLocationTBL)
                {
                    return EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
                }
				//end
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.QueryParkingInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 获取所有收费标准列表
        /// </summary>
        /// <returns></returns>
        public List<CTariffDto> GetTariffList()
        {
            try
            {
                return CCommonMethods.myMethods.GetTariffList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTariffList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CTariffDto>();
            }
        }

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
        public EnmFaultType SaveTariff(CTariffDto tariff)
        {
            try
            {
                List<CTariffDto> lstTariff = new List<CTariffDto>();
                lstTariff = CCommonMethods.myMethods.GetTariffList();
                if (lstTariff.Contains(tariff))
                {
                    return EnmFaultType.Success;
                }

                CTariffDto findDto = lstTariff.Find(s => s.id == tariff.id);

                if (null != findDto)
                {
                    if (CCommonMethods.myMethods.UpdateTariffInfo(tariff))
                    {
                        return EnmFaultType.Success;
                    }

                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                if (CCommonMethods.myMethods.InsertTariffInfo(tariff))
                {
                    return EnmFaultType.Success;
                }

                return EnmFaultType.FailToInsert;//插入数据库失败
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.SaveTariff 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 删除收费标准
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="tariff"></param>
        /// <returns></returns>
        public EnmFaultType DeleteTariff(CTariffDto tariff)
        {
            try
            {
                if (CCommonMethods.myMethods.DeleteTariffInfo(tariff))
                {
                    return EnmFaultType.Success;
                }

                return EnmFaultType.FailToDelete;//删除数据库失败
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteTariff 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 根据ID获取计费信息
        /// </summary>
        /// <param name="nTariffID"></param>
        /// <returns></returns>
        public CTariffDto GetTariffByID(int nTariffID)
        {
            try
            {
                return CCommonMethods.myMethods.GetTariffByID(nTariffID);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetTariffByID 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return null;
            }
        }
        #endregion 对计费的管理

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
        public EnmFaultType ModifyLEDContent(string strLEDOld, string strLEDNew)
        {
            try
            {
                List<CLedContentDto> lstLedContent = CCommonMethods.myMethods.GetLEDContentList();
                // 获取所有包含strLEDOld的LED列表
                lstLedContent = lstLedContent.FindAll(s => s.ledcontent.Contains(strLEDOld));

                if (null == lstLedContent || 1 > lstLedContent.Count)
                {
                    return EnmFaultType.Fail;//LED文本未有当前关键字
                }

                foreach (CLedContentDto dto in lstLedContent)
                {
                    dto.ledcontent = dto.ledcontent.Replace(strLEDOld, strLEDNew);
                    if (!CCommonMethods.myMethods.UpdateLedContentInfo(dto))
                    {
                        return EnmFaultType.FailToUpdate;//更新数据库失败
                    }
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.ModifyLEDContent 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 获取所有LED列表
        /// </summary>
        /// <returns></returns>
        public List<CLedContentDto> GetLEDContentList()
        {
            try
            {
                return CCommonMethods.myMethods.GetLEDContentList();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetLEDContentList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CLedContentDto>();
            }
        }

        /// <summary>
        /// 更新LED列表
        /// </summary>
        /// <param name="lstLedContent"></param>
        /// <returns></returns>
        public bool UpdateLEDContentDtoList(List<CLedContentDto> lstLedContent)
        {
            try
            {
                foreach (CLedContentDto dto in lstLedContent)
                {
                    CCommonMethods.myMethods.UpdateLedContentInfo(dto);
                }
                return true;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.UpdateLEDContentDtoList 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion

        #region 对队列信息的管理
        /// <summary>
        /// 查询所有排队报文
        /// </summary>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSendTelegramQueue()
        {
            try
            {
                return CCommonMethods.myMethods.GetSendTelegramQueue();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetSendTelegramQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CWorkQueueDto>();
            }
        }

        /// <summary>
        /// 根据库区号和设备号获取当前发送报文队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSendTelegramQueue(int nWareHouse, int nEquipID)
        {
            try
            {
                return CCommonMethods.myMethods.GetSendTelegramQueue(nWareHouse, nEquipID);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.GetSendTelegramQueue (int nWareHouse, int nEquipID) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return new List<CWorkQueueDto>();
            }
        }

        /// <summary>
        /// 删除报文队列对象
        /// 成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.FailToDelete;//删除数据库失败
        /// </summary>
        /// <param name="workQueue"></param>
        /// <returns></returns>
        public EnmFaultType DeleteWorkQueueObject(int wid)
        {
            try
            {
                CWorkQueueDto workQueue = CCommonMethods.myMethods.GetWorkQueueByID(wid);
                if (workQueue == null) 
                {
                    return EnmFaultType.Fail;
                }
                if (CCommonMethods.myMethods.DeleteWorkQueueInfo(workQueue))
                {
                    return EnmFaultType.Success;
                }
                else
                {
                    return EnmFaultType.FailToDelete;
                }
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 查询所有的队列信息
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetAllQueueForDisp(int warehouse) 
        {
            try 
            {
                return CCommonMethods.myMethods.QueryAllQueueForDisp(warehouse);
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("WCFQueryLib.CQueryService.GetAllQueueForDisp 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            return null;
        }

        /// <summary>
        /// 删除所有队列
        /// </summary>
        /// <returns></returns>
        public EnmFaultType DeleteAllWorkQueue() 
        {
            try 
            {
                if (CCommonMethods.myMethods.DeleteAllWorkQueue()) 
                {
                    return EnmFaultType.Success;
                }
                return EnmFaultType.Fail;
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("WCFQueryLib.CQueryService.DeleteAllWorkQueue 异常", ex.ToString());
                return EnmFaultType.Exception;
            }
        }

        #endregion 对队列信息的管理

        #region 允许用户设置厅外刷卡取车有效
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUserSetFixCardOutLimit() 
        {
            return CCommonMethods.myMethods.GetFixCard_AllowOutGet();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int SetUserSetFixCardOutLimit(string value) 
        {
            return CCommonMethods.myMethods.SetFixCard_AllowOutGet(value);
        }
        #endregion
    }
}
