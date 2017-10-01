using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DatabaseManagementLib;
using TypedefLib;
using CommonMethodLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using System.Collections;
using AbstractActionLib;
using CommunicationLib;
using WorkFlowLib;
using ICCardManagementLib;

namespace WCFServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“PushService”。
    /// <summary>
    /// 推送消息服务
    /// </summary>
    [Serializable]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class CPushService : IPushService, IDisposable
    {
        #region 私有成员变量
        private int m_nEquipIsAviliable = CConfigManagement.myPara.EquipIsAviliableID;//.GetEquipIsAviliable();
        private CCommModule m_commModule = null;// PLC服务器对象
        private IPushCallback m_pushCallback;
        private static event PushEventHandler m_pushEvent;
        #endregion 私有成员变量

        /// <summary>
        /// 取消注册客户端，销毁服务实例
        /// </summary>
        public void Dispose()
        {
            try
            {
                //m_pushCallback = OperationContext.Current.GetCallbackChannel<IPushCallback>();
                if (null != m_pushEvent)
                {
                    CLOGException.Trace("WCFServiceLib.CPushService.Dispose", m_pushEvent.GetInvocationList().Count());
                    m_pushEvent -= new PushEventHandler(PushMessage);
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("Dispose 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 注册客户端
        /// </summary>
        public void Register(string strHost)
        {
            try
            {
                m_pushCallback = OperationContext.Current.GetCallbackChannel<IPushCallback>();
                m_pushEvent += new PushEventHandler(PushMessage);
                CLOGException.Trace("WCFServiceLib.CPushService.Register", m_pushEvent.GetInvocationList().Count());
            }
            catch (Exception ex)
            {
                CLOGException.Trace("Register 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 心跳包使用
        /// </summary>
        /// <returns></returns>
        public bool CheckService()
        {
            return true;
        }

        /// <summary>
        /// 将消息推送至所有客户端
        /// </summary>
        /// <param name="text"></param>
        public void WCFPushMsgToClient(object objTable)
        {
            try
            {
                if (null != m_pushEvent && null != objTable && typeof(CCarCustomerDto) != objTable.GetType())
                {
                    CPushEventArgs e = new CPushEventArgs();
                    e.ObjTable = objTable;

                    //遍历所有注册的客户端
                    foreach (PushEventHandler handler in m_pushEvent.GetInvocationList())
                    {
                        //调用PushMessage将消息推送到对应的客户端
                        handler.BeginInvoke(this, e, null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("Exception 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 推送消息到客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PushMessage(object sender, CPushEventArgs e)
        {
            try
            {
                if (null == e.ObjTable)
                {
                    return;
                }
                switch (e.ObjTable.GetType().ToString())
                {
                    case "DatabaseManagementLib.CDeviceStatusDto":
                    case "DatabaseManagementLib.CCarLocationDto":
                    case "DatabaseManagementLib.CDeviceFaultDto":
                        {
                            //push到客户端
                            m_pushCallback.Push(e.ObjTable);
                            break;
                        }
                    default:
                        {
                            CLOGException.Trace(string.Format("CPushService.PushMessage推送对象{0},不允许推送！",e.ObjTable.GetType().ToString()));
                            break;
                        }
                }
            }
            catch (TimeoutException ex)
            {
                m_pushEvent -= new PushEventHandler(PushMessage);
                CLOGException.Trace("TimeoutException 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            catch (FaultException ex)
            {
                m_pushEvent -= new PushEventHandler(PushMessage);
                CLOGException.Trace("FaultException 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            catch (CommunicationException ex)
            {
                m_pushEvent -= new PushEventHandler(PushMessage);
                CLOGException.Trace("CommunicationException 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            catch (Exception ex)
            {
                m_pushEvent -= new PushEventHandler(PushMessage);
                CLOGException.Trace("Exception 异常:", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        #region 对IC卡的处理
        /// <summary>
        /// 修改制过卡的逻辑卡号
        /// 成功：
        /// EnmFaultType.Success;//修改成功
        /// 失败：
        /// EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
        /// EnmFaultType.FailToUpdate;//更新数据库失败
        /// EnmFaultType.Null;//传入的参数卡号为空
        /// EnmFaultType.LossORCancel;//IC卡注销或挂失
        /// EnmFaultType.NotMatch;//IC卡类型不正确        
        /// 已经注释掉：EnmFaultType.FailToWriteICCard;//写入IC卡失败
        /// 已经注释掉：EnmFaultType.FailConnection;//连接读卡器失败
        /// </summary>
        /// <param name="strPhysicalCardID"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public EnmFaultType ModifyICCardID(string strPhysicalCardID, string strICCardID)
        {
            try
            {
                //string strICCardID = strData.Substring(0, strData.Length - 1 - "yyyyMMdd".Length - 2);//卡号 + 类型("1") + 制卡时间("yyyyMMdd") + 收费标准("01")
                CICCardDto icCardTBL = CCommonMethods.myMethods.GetICCardByPhysCardID(strPhysicalCardID);
                if (null == icCardTBL)
                {
                    return EnmFaultType.NoICCardInfo;//当前IC卡没有制卡
                }
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(icCardTBL.iccode);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                icCardTBL.oldiccode = icCardTBL.iccode;
                icCardTBL.iccode = strICCardID;
                if (!CCommonMethods.myMethods.UpdateICCardInfo(icCardTBL))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                CICCardReaderObj cardReader = CCommonMethods.myMethods.GetReaderObj(CConfigManagement.myPara.MaintainComID);
                if (null != cardReader)
                {
                    cardReader.ConnectCOM();
                    if (cardReader.isComConnected)
                    {
                        int nReturn = 1;
                        string strBlock1 = string.Empty;
                        nReturn = cardReader.ReadDataFromCard(1, 0, out strBlock1);
                        if (0 == nReturn)
                        {
                            strBlock1 = strBlock1.Replace(strBlock1.Substring(1, strBlock1.Length - 1).Trim(), strICCardID);
                            nReturn = cardReader.WriteDataIntoCard(1, 0, strBlock1);
                        }
                    }
                    cardReader.DisConnectCOM();
                }
                return EnmFaultType.Success;//修改成功
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyICCardID 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifyICCard(string strOldICCardID, string strNewICCardID)
        {
            try
            {//检查新卡和旧卡的基本状态是否满足
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(strOldICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                enmReturn = CCommonMethods.myMethods.CheckICCardID(strNewICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                //判断新卡是否有绑定用户
                CICCardDto oldICCardTBL = CCommonMethods.myMethods.GetICCard(strOldICCardID);
                CICCardDto newICCardTBL = CCommonMethods.myMethods.GetICCard(strNewICCardID);
                if (null != newICCardTBL.userid)
                {
                    return EnmFaultType.BoundUser;//新的IC卡有绑定用户
                }

                //判断新卡和旧卡是否正在作业
                CDeviceStatusDto oldDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(strOldICCardID);
                CDeviceStatusDto newDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(strNewICCardID);
                if (null != oldDeviceStatusTBL || null != newDeviceStatusTBL)
                {
                    return EnmFaultType.TaskOnICCard;//旧卡或新卡正在作业
                }

                //判断新卡和旧卡是否正在排队取车
                List<CWorkQueueDto> oldWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(strOldICCardID);
                List<CWorkQueueDto> newWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(strNewICCardID);
                if (0 < oldWorkQueueTBL.Count || 0 < newWorkQueueTBL.Count)
                {
                    return EnmFaultType.Wait;//旧卡或新卡正在排队取车
                }

                //判断新卡是否有存车
                CCarLocationDto newCarLocationTBL = CCommonMethods.myMethods.GetCarLocation(strNewICCardID);
                if (null != newCarLocationTBL)
                {
                    return EnmFaultType.CarInGarage;//新的IC卡有车存在车库
                }

                //判断旧卡是否有存车,有存车则将车位信息的IC卡卡号修改为新卡的卡号
                CCarLocationDto oldCarLocationTBL = CCommonMethods.myMethods.GetCarLocation(strOldICCardID);
                if (null != oldCarLocationTBL)
                {
                    oldCarLocationTBL.iccode = strNewICCardID;
                    if (!CCommonMethods.myMethods.UpdateCarLocationInfo(oldCarLocationTBL))
                    {
                        return EnmFaultType.FailToUpdate;//更新数据库失败
                    }
                }

                //将旧卡的信息赋给新卡，并将旧卡的卡号填到新卡上，然后将旧卡注销
                newICCardTBL.iccode = strNewICCardID;
                newICCardTBL.oldiccode = strOldICCardID;
                newICCardTBL.carlocaddr = oldICCardTBL.carlocaddr;
                newICCardTBL.icstarttime = oldICCardTBL.icstarttime;
                newICCardTBL.icdeadline = oldICCardTBL.icdeadline;
                newICCardTBL.icstatus = oldICCardTBL.icstatus;
                newICCardTBL.ictype = oldICCardTBL.ictype;
                newICCardTBL.tariffid = oldICCardTBL.tariffid;
                newICCardTBL.userid = oldICCardTBL.userid;
                newICCardTBL.warehouse = oldICCardTBL.warehouse;
                newICCardTBL.priorityid = oldICCardTBL.priorityid;

                oldICCardTBL.icstatus = (int)EnmICCardStatus.Disposed;
                oldICCardTBL.iclogouttime = DateTime.Now;
                if (!CCommonMethods.myMethods.UpdateICCardInfo(oldICCardTBL) || !CCommonMethods.myMethods.UpdateICCardInfo(newICCardTBL))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;//修改成功
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifyPhyCard(string strPhysicalCardID, string strICCardID)
        {
            try
            {//检查新卡和旧卡的基本状态是否满足
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(strICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                //判断新卡和旧卡是否正在作业
                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(strICCardID);
                if (null != deviceStatusTBL)
                {
                    return EnmFaultType.TaskOnICCard;//正在作业
                }

                //判断新卡和旧卡是否正在排队取车
                List<CWorkQueueDto> workQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(strICCardID);
                if (0 < workQueueTBL.Count)
                {
                    return EnmFaultType.Wait;//正在排队取车
                }

                //判断新卡是否有绑定用户
                CICCardDto ICCardTBL = CCommonMethods.myMethods.GetICCard(strICCardID);

                //将旧卡的信息赋给新卡，并将旧卡的卡号填到新卡上，然后将旧卡注销
                ICCardTBL.phycode = strPhysicalCardID;

                if (!CCommonMethods.myMethods.UpdateICCardInfo(ICCardTBL))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;//修改成功
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
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
        public EnmFaultType ManualVEHExit(int nWareHouse, string strCarPOSNAddr)
        {
            try
            {
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strCarPOSNAddr);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.Fail;//获取车位信息失败
                }
                if ((EnmLocationType)carPOSNTable.carloctype == EnmLocationType.Hall)
                {
                    return EnmFaultType.HallEquip;//当前车位是车厅设备
                }
                else if ((EnmLocationType)carPOSNTable.carloctype != EnmLocationType.Normal)
                {
                    return EnmFaultType.NotNormalCarPOSN;//不是正常车位
                }               

                //初始化车位信息
                if (!CCommonMethods.myMethods.InitCarPOSN(ref carPOSNTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ManualVEHExit 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 手动入库
        /// 返回值：
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        ///       EnmFaultType.Fail;//获取车位信息失败
        ///       EnmFaultType.NotNormalCarPOSN;//不是正常车位
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.HallEquip;//当前车位是车厅设备
        ///       EnmFaultType.CarInGarage;//当前车位有车，不允许将车放到该车位
        ///       EnmFaultType.TaskOnICCard; //当前卡有车存在车库
        ///       EnmFaultType.OverCarInSize;// 入库车辆尺寸超限
        /// </summary>
        /// <param name="carPOSNTable"></param>
        public EnmFaultType ManualVEHEntry(CCarLocationDto carPOSNTable)
        {
            try
            {
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(carPOSNTable.iccode);
                if (enmReturn != EnmFaultType.Success)
                {
                    return enmReturn;
                }

                CICCardDto icCardTBL = CCommonMethods.myMethods.GetICCard(carPOSNTable.iccode);

                CCarLocationDto carPOSNTBL = CCommonMethods.myMethods.GetCarLocation(carPOSNTable.iccode);
                if (null != carPOSNTBL)
                {
                    return EnmFaultType.TaskOnICCard;        //当前卡有车存在车库
                }

                CCarLocationDto carPOSNTableTmp = new CCarLocationDto();
                //if (icCardTBL.ictype == (int)EnmICCardType.FixedLocation)
                //{
                //    carPOSNTableTmp = CCommonMethods.myMethods.GetCarLocation((int)carPOSNTable.warehouse, icCardTBL.carlocaddr);
                //}
                //else
                //{
                carPOSNTableTmp = CCommonMethods.myMethods.GetCarLocation((int)carPOSNTable.warehouse, carPOSNTable.carlocaddr);
                //}

                if (null == carPOSNTableTmp)
                {
                    return EnmFaultType.Fail;//获取车位信息失败
                }
                if ((EnmLocationType)carPOSNTableTmp.carloctype != EnmLocationType.Normal)
                {
                    if ((EnmLocationType)carPOSNTableTmp.carloctype == EnmLocationType.Hall)
                    {
                        return EnmFaultType.HallEquip;//当前车位是车厅设备
                    }
                    else
                    {
                        return EnmFaultType.NotNormalCarPOSN;//不是正常车位
                    }
                }
                if ((EnmLocationStatus)carPOSNTableTmp.carlocstatus != EnmLocationStatus.Space)
                {
                    return EnmFaultType.CarInGarage;//当前车位有车，不允许将车放到该车位
                }

                if (!CBaseMethods.MyBase.CheckCarSize(carPOSNTable.carsize, carPOSNTableTmp.carlocsize))
                {
                    return EnmFaultType.OverCarInSize;// 入库车辆尺寸超限
                }
                carPOSNTableTmp.carlocstatus = (int)EnmLocationStatus.Occupy;
                carPOSNTableTmp.iccode = carPOSNTable.iccode;
                carPOSNTableTmp.carwheelbase = carPOSNTable.carwheelbase;
                carPOSNTableTmp.direction = carPOSNTable.direction;
                carPOSNTableTmp.carsize = carPOSNTable.carsize;
                carPOSNTableTmp.carintime = carPOSNTable.carintime;
                carPOSNTableTmp.overallLg = carPOSNTable.overallLg;
                carPOSNTableTmp.overhang = carPOSNTable.overhang;
                //carPOSNTableTmp.isbackup = carPOSNTable.isbackup;
                //carPOSNTableTmp.carnumber = carPOSNTable.carnumber;
                //carPOSNTableTmp.carpicture = carPOSNTable.carpicture;
                if (!CCommonMethods.myMethods.UpdateCarLocationInfo(carPOSNTableTmp))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ManualVEHEntry 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ManualMoveVEH(int nWareHouse, string strSrcCarPOSNAddr, string strDestCarPOSNAddr)
        {
            try
            {
                CCarLocationDto srcCarPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strSrcCarPOSNAddr);
                CCarLocationDto destCarPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strDestCarPOSNAddr);
                if (null == srcCarPOSNTable || null == destCarPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN;//没有找到源车位或目的车位
                }
                if ((int)EnmLocationType.Normal != srcCarPOSNTable.carloctype || (int)EnmLocationType.Normal != destCarPOSNTable.carloctype)
                {
                    return EnmFaultType.NotNormalCarPOSN;//源车位或目的车位不是正常车位
                }
                if ((int)EnmLocationStatus.Occupy != srcCarPOSNTable.carlocstatus)
                {
                    return EnmFaultType.NoCarInGarage;//没有车存在源车位
                }
                if ((int)EnmLocationStatus.Occupy == destCarPOSNTable.carlocstatus)
                {
                    return EnmFaultType.CarInGarage;//有车存在目的车位
                }
                if ((int)EnmLocationStatus.Space != destCarPOSNTable.carlocstatus)
                {
                    return EnmFaultType.TaskOnICCard;//目的车位的车辆正在作业
                }
                CICCardDto ICCardTableSrc = CCommonMethods.myMethods.GetICCard(srcCarPOSNTable.iccode);
                CICCardDto ICCardTableDest = CCommonMethods.myMethods.GetICCard((int)destCarPOSNTable.warehouse, destCarPOSNTable.carlocaddr);
                if (null == ICCardTableSrc)
                {
                    return EnmFaultType.NoICCardInfo;//源车位的IC卡没有制卡
                }

                if ((int)EnmICCardType.FixedLocation != ICCardTableSrc.ictype && null != ICCardTableDest && (int)EnmICCardType.FixedLocation == ICCardTableDest.ictype)
                {
                    return EnmFaultType.FixedCarPOSN;//目的车位是其他车主的固定车位，临时卡或定期卡无法挪移到固定车位卡的车位上
                }
                if ((int)EnmICCardType.FixedLocation == ICCardTableSrc.ictype && null != ICCardTableDest && (int)EnmICCardType.FixedLocation == ICCardTableDest.ictype
                    && ICCardTableSrc.carlocaddr != destCarPOSNTable.carlocaddr)
                {
                    return EnmFaultType.NotMatch;//目的车位是其他车主的固定车位
                }

                if (!CBaseMethods.MyBase.CheckCarSize(srcCarPOSNTable.carsize, destCarPOSNTable.carlocsize))
                {
                    return EnmFaultType.OverCarInSize;//目的车位尺寸不满足车辆尺寸
                }

                destCarPOSNTable.carlocstatus = srcCarPOSNTable.carlocstatus;
                destCarPOSNTable.iccode = srcCarPOSNTable.iccode;

                destCarPOSNTable.carintime = srcCarPOSNTable.carintime;
                destCarPOSNTable.direction = srcCarPOSNTable.direction;

                destCarPOSNTable.carwheelbase = srcCarPOSNTable.carwheelbase;
                destCarPOSNTable.carsize = srcCarPOSNTable.carsize;                

                destCarPOSNTable.overallLg = srcCarPOSNTable.overallLg;
                destCarPOSNTable.overhang = srcCarPOSNTable.overhang;
                destCarPOSNTable.rearwheeldis = srcCarPOSNTable.rearwheeldis;
                destCarPOSNTable.offcenter = srcCarPOSNTable.offcenter;
                destCarPOSNTable.carweight = srcCarPOSNTable.carweight;

                //destCarPOSNTable.isbackup = srcCarPOSNTable.isbackup;
                //destCarPOSNTable.carnumber = srcCarPOSNTable.carnumber;
                //destCarPOSNTable.carpicture = srcCarPOSNTable.carpicture;
               
                if (!CCommonMethods.myMethods.UpdateCarLocationInfo(destCarPOSNTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                //初始化车位信息
                if (!CCommonMethods.myMethods.InitCarPOSN(ref srcCarPOSNTable))
                {
                    CCommonMethods.myMethods.InitCarPOSN(ref destCarPOSNTable);
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ManualMoveVEH 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifyCarPOSNType(int nWareHouse, string strCarPOSNAddr, EnmLocationType enmCarPOSNType)
        {
            try
            {
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strCarPOSNAddr);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN; //没有找到指定车位
                }

                if (enmCarPOSNType == EnmLocationType.Disable &&
                    (carPOSNTable.carloctype != (int)EnmLocationType.Normal || carPOSNTable.carlocstatus != (int)EnmLocationStatus.Space))
                {
                    return EnmFaultType.NotAllowed;//不允许修改车位类型为禁用车位,车位非空闲正常车位
                }

                if (enmCarPOSNType == EnmLocationType.Normal && carPOSNTable.carloctype != (int)EnmLocationType.Disable)
                {
                    return EnmFaultType.NotAllowed;//不允许修改车位类型为正常车位,车位非禁用 车位
                }

                carPOSNTable.carloctype = (int)enmCarPOSNType;
                if (!CCommonMethods.myMethods.UpdateCarLocationInfo(carPOSNTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyCarPOSNType 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType ModifyCarPOSNStatus(int nWareHouse, string strCarPOSNAddr, EnmLocationStatus enmCarPOSNStatus)
        {
            try
            {
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strCarPOSNAddr);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN; //没有找到指定车位
                }

                carPOSNTable.carlocstatus = (int)enmCarPOSNStatus;
                if (!CCommonMethods.myMethods.UpdateCarLocationInfo(carPOSNTable))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyCarPOSNStatus 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 一键出库
        ///  成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.Fail; //一键出库失败
        /// </summary>
        /// <returns></returns>
        public EnmFaultType InitAllCarPOSN(int nWareHouse)
        {
            try
            {
                List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
                lstCarLocationTBL = CCommonMethods.myMethods.GetCarLocationList();
                foreach (CCarLocationDto table in lstCarLocationTBL)
                {
                    CCarLocationDto carLocationTBL = new CCarLocationDto();
                    carLocationTBL = table;
                    if (!CCommonMethods.myMethods.InitCarPOSN(ref carLocationTBL))
                    {
                        continue;
                    }
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.InitAllCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 一键禁用/启用
        ///  成功：
        ///     EnmFaultType.Success;
        /// 失败：
        ///     EnmFaultType.Fail; //一键禁用/启用失败
        /// </summary>
        /// <param name="enmCarPOSNType"></param>
        /// <returns></returns>
        public EnmFaultType ModifyAllCarPOSNType(int nWareHouse, EnmLocationType enmCarPOSNType)
        {
            try
            {
                List<CCarLocationDto> lstCarLocation = new List<CCarLocationDto>();
                if (!CCommonMethods.myMethods.ModifyAllCarPOSNType(nWareHouse, enmCarPOSNType, out lstCarLocation))
                {
                    return EnmFaultType.Fail;
                }
                for (int i = 1; i < lstCarLocation.Count;i++)
                {
                    WCFPushMsgToClient(lstCarLocation[i]);
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.ModifyAllCarPOSNType 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
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
        /// <param name="nIsable"></param>
        public EnmFaultType SetDeviceMode(int nWareHouse, int nDeviceID, int nIsable)
        {
            try
            {
                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nDeviceID);
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;//没有找到指定设备
                }

                if ((int)EnmModel.Automatic != deviceStatusTBL.devicemode && 1 == nIsable)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式不能启用
                }

                if (nIsable != deviceStatusTBL.isable)
                {
                    deviceStatusTBL.isable = nIsable;
                }
                else
                {
                    return EnmFaultType.NotAllowed;//设备当前状态与要修改的状态相同
                }

                if (!CCommonMethods.myMethods.UpdateDeviceStatusInfo(deviceStatusTBL))
                {
                    return EnmFaultType.FailToUpdate;//更新数据库失败
                }

                if (1 == nIsable)
                {
                    return StartWorkQueue(deviceStatusTBL);
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.SetDeviceMode 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }

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
        public EnmFaultType SetHallType(int nWareHouse, int nHallID, EnmHallType enmHallType)
        {
            try
            {
                CDeviceStatusDto hallStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == hallStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;//没有找到指定的车厅
                }

                List<CWorkQueueDto> lstWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nHallID);
                if (null != lstWorkQueueTBL && 0 < lstWorkQueueTBL.Count)
                {
                    return EnmFaultType.WorkQueueNotEmpty;//有人排队取车，不允许修改车厅类型
                }
                hallStatusTBL.halltype = (int)enmHallType;
                if (!CCommonMethods.myMethods.UpdateDeviceStatusInfo(hallStatusTBL))
                {
                    return EnmFaultType.FailToUpdate;//修改数据库失败
                }
                if (!CConfigManagement.myPara.HashDeviceIDItemAdressIndex.ContainsKey(nWareHouse)
                    || !((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse]).ContainsKey(nHallID))
                {
                    return EnmFaultType.InvalidWareHouseID;//无效库区号
                }

                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                int itemNum = (int)((Hashtable)CConfigManagement.myPara.HashDeviceIDItemAdressIndex[nWareHouse])[nHallID];// 订阅Item索引号
                if (!m_commModule.Write(35, (int)enmHallType, itemNum))
                {
                    return EnmFaultType.FailToSendTelegram; //发送报文失败
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.SetHallType 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 0;
            }
        }
       
        /// <summary>
        /// 判断该库区是否有作业
        /// add by wanxiaona20150806
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns>true:有作业  false：无作业</returns>
        public bool IsHasTask(int nWareHouse)
        {
            try
            {
                bool bHas = false;
                List<CWorkQueueDto> lstWorkQueue = CCommonMethods.myMethods.GetWorkQueueList(nWareHouse);

                if (null == lstWorkQueue || 1 > lstWorkQueue.Count)
                {// 无队列
                    List<CDeviceStatusDto> lstDeviceStatus = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse);

                    foreach (CDeviceStatusDto dto in lstDeviceStatus)
                    {
                        if (!CBaseMethods.MyBase.IsEmpty(dto.tasktype) || !string.IsNullOrWhiteSpace(dto.iccode))
                        {
                            bHas = true;
                            break;
                        }
                    }
                }
                else
                {
                    bHas = true;
                }

                return bHas;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.IsHasTask 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
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
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        public EnmFaultType TmpFetch(string strICCardID, int nWareHouse, int nHallID)
        {
            try
            {
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(strICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
                }

                if (null != CCommonMethods.myMethods.GetDeviceStatus(strICCardID))
                {
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                nHallID = nHallID % 100;
                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;       //没有找到指定设备
                }
                if ((int)EnmModel.Automatic != deviceStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }
                if (deviceStatusTBL.devicetype != (int)EnmSMGType.Hall)
                {
                    return EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
                }

                if (deviceStatusTBL.halltype != (int)EnmHallType.EnterOrExit)
                {
                    return EnmFaultType.EquipIncorrect;//车厅不是进出两用车厅
                }

                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);

                return taskTrigger.TmpFetch(deviceStatusTBL, carPOSNTable);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.TmpFetch 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
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
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strHallID">出库车厅</param>
        /// <returns></returns>
        public EnmFaultType VehicleExit(int nWareHouse, string strSrcPOSN, string strHallID)
        {
            try
            {
                int nHallID = 0;
                CBaseMethods.MyBase.StringToUInt32(strHallID, out nHallID);
                nHallID = nHallID % 100;
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strSrcPOSN);
                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN;    //指定的源车位不存在
                }

                if ((int)EnmLocationType.Normal != carPOSNTable.carloctype)
                {
                    return EnmFaultType.NotNormalCarPOSN;//源车位不是正常车位。
                }

                if ((int)EnmLocationStatus.Occupy != carPOSNTable.carlocstatus || string.IsNullOrWhiteSpace(carPOSNTable.iccode))
                {
                    return EnmFaultType.NoCarInGarage;       //源车位没有车
                }

                if (null != CCommonMethods.myMethods.GetDeviceStatus(carPOSNTable.iccode))
                {
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;       //没有找到指定目的车厅
                }

                if ((int)EnmSMGType.Hall != deviceStatusTBL.devicetype)
                {
                    return EnmFaultType.IsNotHallEquip;      //指定的目的地址不是车厅
                }

                if ((int)EnmHallType.Exit != deviceStatusTBL.halltype && (int)EnmHallType.EnterOrExit != deviceStatusTBL.halltype)
                {
                    return EnmFaultType.HallEnter;      //车厅不是出车厅不允许出车
                }

                //已经将您加到取车队列
                List<CWorkQueueDto> lstWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(carPOSNTable.iccode);
                if (lstWorkQueueTBL.Count > 0) 
                {
                    return EnmFaultType.Wait;
                }

                // 清除当前IC卡在刷卡队列中的所有刷卡记录
                List<CWorkQueueDto> lstWorkQueue = CCommonMethods.myMethods.DeleteWorkQueueList(carPOSNTable.iccode);
                foreach (CWorkQueueDto table in lstWorkQueue)
                {
                    WCFPushMsgToClient(table);
                }
                
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();

                taskTrigger.SetTaskTrigger(nWareHouse);

                return taskTrigger.VehicleExit(carPOSNTable.iccode, deviceStatusTBL, carPOSNTable, false);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.VehicleExit 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
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
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strHallID">出库车厅</param>
        /// <returns></returns>
        public EnmFaultType VehicleExitForBilling(string strICCardID, int nHallID)
        {
            try
            {
                CCarLocationDto carLocationTBL = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if (null == carLocationTBL)
                {
                    return EnmFaultType.NoCarInGarage;//源车位没有车
                }
                return VehicleExit((int)carLocationTBL.warehouse, carLocationTBL.carlocaddr, nHallID.ToString());
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.VehicleExit 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
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
        /// <param name="nWareHouse">库区号</param>
        /// <param name="strSrcPOSN">源车位地址</param>
        /// <param name="strDestPOSN">目的车位地址</param>
        /// <returns></returns>
        public EnmFaultType VehicleMove(int nWareHouse, string strSrcPOSN, string strDestPOSN)
        {
            try
            {
                CCarLocationDto carLocationTBLSrc = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strSrcPOSN);
                CCarLocationDto carLocationTBLDest = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strDestPOSN);
                if (null == carLocationTBLSrc || null == carLocationTBLDest)
                {
                    return EnmFaultType.NotFoundCarPOSN; //指定的源或目的车位不存在
                }

                if (carLocationTBLSrc.carloctype != (int)EnmLocationType.Normal || carLocationTBLDest.carloctype != (int)EnmLocationType.Normal)
                {
                    return EnmFaultType.NotNormalCarPOSN; //源车位或目的车位不是正常车位
                }

                if (carLocationTBLSrc.carlocstatus != (int)EnmLocationStatus.Occupy || string.IsNullOrWhiteSpace(carLocationTBLSrc.iccode))
                {
                    return EnmFaultType.NoCarInGarage;//源车位没有车
                }

                if (carLocationTBLDest.carlocstatus == (int)EnmLocationStatus.Occupy || !string.IsNullOrWhiteSpace(carLocationTBLDest.iccode))
                {
                    return EnmFaultType.CarInGarage;//目的车位有车
                }

                if ((int)EnmLocationStatus.Space != carLocationTBLDest.carlocstatus)
                {
                    return EnmFaultType.TaskOnICCard;//目的车位的车辆正在作业
                }
                CICCardDto ICCardTableSrc = CCommonMethods.myMethods.GetICCard(carLocationTBLSrc.iccode);               
                if (null == ICCardTableSrc)
                {
                    return EnmFaultType.NoICCardInfo;//源车位的IC卡没有制卡
                }
                CICCardDto ICCardTableDest = CCommonMethods.myMethods.GetICCard(nWareHouse, strDestPOSN);
                if ((int)EnmICCardType.FixedLocation != ICCardTableSrc.ictype && null != ICCardTableDest && (int)EnmICCardType.FixedLocation == ICCardTableDest.ictype)
                {
                    return EnmFaultType.FixedCarPOSN;//目的车位是其他车主的固定车位，临时卡或定期卡无法挪移到固定车位卡的车位上
                }

                if ((int)EnmICCardType.FixedLocation == ICCardTableSrc.ictype && null != ICCardTableDest && (int)EnmICCardType.FixedLocation == ICCardTableDest.ictype
                    && ICCardTableSrc.carlocaddr != carLocationTBLDest.carlocaddr)
                {
                    return EnmFaultType.NotMatch;//目的车位是其他车主的固定车位
                }

                if (!CBaseMethods.MyBase.CheckCarSize(carLocationTBLSrc.carlocsize, carLocationTBLDest.carlocsize))
                {
                    return EnmFaultType.OverCarInSize; //目的车位尺寸无法满足源车位的车辆
                }

                if (carLocationTBLSrc.carloclayer != carLocationTBLDest.carloclayer)
                {
                    return EnmFaultType.Fail;
                }
                
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);

                return taskTrigger.VehicleMove(carLocationTBLSrc, carLocationTBLDest);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.VehicleMove 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType EquipMove(int nWareHouse, int nEquipID, string strDestPOSN)
        {
            try
            {
                CDeviceStatusDto equipStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nEquipID);
                if (null == equipStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
                }

                if (equipStatusTBL.devicetype != (int)EnmSMGType.ETV)
                {
                    return EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
                }

                if ((int)EnmModel.Automatic != equipStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }

                if (0 == equipStatusTBL.isable)
                {
                    return EnmFaultType.NotAllowed;      //设备不可用;
                }

                if (0 == equipStatusTBL.isavailable || !CBaseMethods.MyBase.IsEmpty(equipStatusTBL.tasktype))
                {
                    return EnmFaultType.NotAvailable;      //设备不可接收指令;
                }

                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strDestPOSN);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
                }
                CRequestAction requestAction = new CRequestAction();

                struTelegram struSendTelegram = new struTelegram();
                struSendTelegram.nPLCSendFlag = nWareHouse;
                struSendTelegram.nEquipID = nEquipID;
                struSendTelegram.strSrcLocAddr = equipStatusTBL.deviceaddr;
                struSendTelegram.strDestLocAddr = strDestPOSN;
                struSendTelegram.nMasterType = (int)EnmTaskType.MoveEquipTask;
                struSendTelegram.nEndFlag = 9999;

                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                requestAction.PLCServer = m_commModule;
               
                requestAction.telegram = string.Format("11-1-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                requestAction.structTelegram = struSendTelegram;               

                int nActionFlag = 5;
                requestAction.RequestAction(nActionFlag);

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.EquipMove 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 处理下一作业
        /// 成功：EnmFaultType.Success;  
        /// 失败：EnmFaultType.Fail;//传入的设备状态表为空
        ///       EnmFaultType.InvalidWareHouseID;//无效库区号
        ///       EnmFaultType.Null;// 下一取车队列的传入的参数卡号为空
        ///       EnmFaultType.WorkQueueNotEmpty;// 启用成功，正在为取车排队的车主取车，请稍后片刻
        ///       EnmFaultType.FailConnection;//连接PLC通信失败
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="deviceStatusTBL"></param>
        /// <returns></returns>
        public EnmFaultType StartWorkQueue(CDeviceStatusDto deviceStatusTBL)
        {
            try
            {
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.Fail;//传入的设备状态表为空
                }
                int nWareHouse = deviceStatusTBL.warehouse;
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);
                CRequestAction requestAction = new CRequestAction();
                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                requestAction.PLCServer = m_commModule;
                //modify by wanxiaona20150728
                bool isHall = false;
                if ((int)EnmSMGType.Hall == deviceStatusTBL.devicetype)
                {
                    isHall = true;
                }
                int nActionFlag = 0;
                bool isGetWorkQueueICCard = taskTrigger.DeviceTelegramQueue(ref nActionFlag, ref requestAction, deviceStatusTBL, isHall);
                requestAction.RequestAction(nActionFlag);
                //end by wanxiaona20150728
                if (isGetWorkQueueICCard)
                {
                    // 执行下一在等待的取车作业
                    return EnmFaultType.WorkQueueNotEmpty;
                }
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.StartWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 预约取车
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
        /// add by wanxiaona201506
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        public EnmFaultType VehicleExitSubscribe(string strICCardID, int nWareHouse, ref int nHallID)
        {
            try
            {
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(strICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
                }

                if (null != CCommonMethods.myMethods.GetDeviceStatus(strICCardID))
                {
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);
                if (CBaseMethods.MyBase.IsEmpty(nHallID))
                {// 用户没选择车厅时，自动分配
                    nHallID = taskTrigger.ALLOCHallByQueue(nWareHouse, 0, carPOSNTable);
                }

                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;       //没有找到指定设备
                }
                if ((int)EnmModel.Automatic != deviceStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }
                if (deviceStatusTBL.devicetype != (int)EnmSMGType.Hall)
                {
                    return EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
                }

                if (deviceStatusTBL.halltype != (int)EnmHallType.EnterOrExit)
                {
                    return EnmFaultType.EquipIncorrect;//车厅不是进出两用车厅
                }
                
                return taskTrigger.VehicleExit(strICCardID, deviceStatusTBL, carPOSNTable, false);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.VehicleExitSubscribe 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType VehicleRotation(int nWareHouse)
        {
            try
            {
                CDeviceStatusDto equipStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, EnmSMGType.ETV);
                if (null == equipStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
                }

                if (equipStatusTBL.devicetype != (int)EnmSMGType.ETV)
                {
                    return EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
                }

                if ((int)EnmModel.Automatic != equipStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }

                if (0 == equipStatusTBL.isable)
                {
                    return EnmFaultType.NotAllowed;      //设备不可用;
                }

                if (0 == equipStatusTBL.isavailable)
                {
                    return EnmFaultType.NotAvailable;    //设备不可接受指令
                }

                // 搜索该库区正常占用车头朝向2边的车位
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
                }

                struTelegram struSendTelegram = new struTelegram();
                struSendTelegram.nPLCSendFlag = nWareHouse;
                struSendTelegram.nEquipID = equipStatusTBL.devicecode;
                struSendTelegram.strICCardID = carPOSNTable.iccode;
                struSendTelegram.strCarSize = carPOSNTable.carsize;
                struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;
                struSendTelegram.nDestDirection = 1;
                struSendTelegram.nDirection = (int)carPOSNTable.direction;
                struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
                struSendTelegram.strDestLocAddr = carPOSNTable.carlocaddr;
                struSendTelegram.nDeviceByteBit = m_nEquipIsAviliable;
                struSendTelegram.nEndFlag = 9999;
                
                #region add by wanxiaona20150728
                CCommModule commModule = new CCommModule();
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion

                CRequestAction requestAction = new CRequestAction();
                int nActionFlag = 5;
                requestAction.telegram = string.Format("12-1-{0}", CConfigManagement.myPara.EquipIsAviliableID); //"12-1";// 报文类型2（12），报文子类型EMS 3（1）;
                requestAction.structTelegram = struSendTelegram;
                requestAction.PLCServer = commModule;

                if ((int)EnmTaskType.VehRotationTask != equipStatusTBL.tasktype)
                {// 修改ETV设备作业类型
                    equipStatusTBL.iccode = carPOSNTable.iccode;
                    equipStatusTBL.tasktype = (int)EnmTaskType.VehRotationTask;
                    //equipStatusTBL.currentnode = (int)EnmFlowNodeDescp.MoveEquipOk;
                    requestAction.lstTableDto.Add(new struTableDto(equipStatusTBL, 2));
                }
                carPOSNTable.carlocstatus = (int)EnmLocationStatus.VehRotation;
                requestAction.lstTableDto.Add(new struTableDto(carPOSNTable, 2));
                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.VehicleRotation 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType VehicleRotation(int nWareHouse, string strCarPOSN)
        {
            try
            {
                CDeviceStatusDto equipStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, EnmSMGType.ETV);
                if (null == equipStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;//没有指定设备，需要确认库区号和设备号无误
                }

                if (equipStatusTBL.devicetype != (int)EnmSMGType.ETV)
                {
                    return EnmFaultType.IsNotETVEquip;//要移动的设备不是ETV或TV设备
                }

                if ((int)EnmModel.Automatic != equipStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }

                if (0 == equipStatusTBL.isable)
                {
                    return EnmFaultType.NotAllowed;      //设备不可用;
                }

                if (0 == equipStatusTBL.isavailable)
                {
                    return EnmFaultType.NotAvailable;    //设备不可接受指令
                }

                // 搜索该库区正常占用车头朝向2边的车位
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(nWareHouse, strCarPOSN);
                if (null == carPOSNTable || 1 == carPOSNTable.direction)
                {
                    return EnmFaultType.NotFoundCarPOSN;//无效目的地址，请选择一个有效的车位地址
                }

                struTelegram struSendTelegram = new struTelegram();
                struSendTelegram.nPLCSendFlag = nWareHouse;
                struSendTelegram.nEquipID = equipStatusTBL.devicecode;
                struSendTelegram.strICCardID = carPOSNTable.iccode;
                struSendTelegram.strCarSize = carPOSNTable.carsize;
                struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;
                struSendTelegram.nDestDirection = 1;
                struSendTelegram.nDirection = (int)carPOSNTable.direction;
                struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
                struSendTelegram.strDestLocAddr = carPOSNTable.carlocaddr;
                struSendTelegram.nDeviceByteBit = m_nEquipIsAviliable;
                struSendTelegram.nEndFlag = 9999;

                #region add by wanxiaona20150728
                CCommModule commModule = new CCommModule();
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion

                CRequestAction requestAction = new CRequestAction();
                int nActionFlag = 5;
                requestAction.telegram = string.Format("12-1-{0}", CConfigManagement.myPara.EquipIsAviliableID); //"12-1";// 报文类型2（12），报文子类型EMS 3（1）;
                requestAction.structTelegram = struSendTelegram;
                requestAction.PLCServer = commModule;

                if ((int)EnmTaskType.VehRotationTask != equipStatusTBL.tasktype)
                {// 修改ETV设备作业类型
                    equipStatusTBL.iccode = carPOSNTable.iccode;
                    equipStatusTBL.tasktype = (int)EnmTaskType.VehRotationTask;
                    //equipStatusTBL.currentnode = (int)EnmFlowNodeDescp.MoveEquipOk;
                    requestAction.lstTableDto.Add(new struTableDto(equipStatusTBL, 2));
                }
                carPOSNTable.carlocstatus = (int)EnmLocationStatus.VehRotation;
                requestAction.lstTableDto.Add(new struTableDto(carPOSNTable, 2));
                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.VehicleRotation 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
        #endregion 对报文的管理

        #region 对故障处理的管理（手动作业）
        /// <summary>
        /// 手动完成作业
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Fail;//手动完成失败
        ///       EnmFaultType.NotFoundCarPOSN;//没有找到合适车位
        ///       EnmFaultType.OverCarInSize;// 入库车辆尺寸超限
        ///       EnmFaultType.FailToDelete;//删除数据库失败
        ///       EnmFaultType.FailToUpdate;//更新数据库失败
        ///       EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
        ///       EnmFaultType.FailToAllocETVorTV;//分配ETV或TV失败
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>       
        public EnmFaultType HandCompleteTask(int warehouse, int code)
        {
            try
            {
                CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(warehouse, code);
                if (null == deviceStatus)
                {
                    return EnmFaultType.Fail;
                }
                if (null == deviceStatus)
                {
                    return EnmFaultType.Fail;
                }
                int nWareHouse = deviceStatus.warehouse;
                int nDeviceID = deviceStatus.devicecode;
                string strICCode = deviceStatus.iccode;
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(strICCode);
                // 作业类型判断
                switch (deviceStatus.tasktype)
                {
                    case (int)EnmTaskType.EntryTask:// 正常存车和临时取物的存车
                        {
                            if (null != carPOSNTable && (EnmLocationStatus)carPOSNTable.carlocstatus == EnmLocationStatus.Entering)
                            {
                                carPOSNTable.carlocstatus = Convert.ToInt32(EnmLocationStatus.Occupy);
                                CCommonMethods.myMethods.UpdateCarLocationInfo(carPOSNTable);
                            }
                            break;
                        }
                    case (int)EnmTaskType.ExitTask:// 取车
                    case (int)EnmTaskType.TmpFetch://临时取物的取车
                        {
                            if (null != carPOSNTable)
                            {
                                CCommonMethods.myMethods.InitCarPOSN(ref carPOSNTable);
                            }
                            break;
                        }
                    case (int)EnmTaskType.MoveCarTask:
                        {
                            if (!string.IsNullOrEmpty(deviceStatus.desccloc) && !string.IsNullOrEmpty(deviceStatus.scrloc))
                            {
                                CCarLocationDto srcLoc = CCommonMethods.myMethods.GetCarLocation(nWareHouse, deviceStatus.scrloc);
                                CCarLocationDto destLoc = CCommonMethods.myMethods.GetCarLocation(nWareHouse, deviceStatus.desccloc);
                                if (srcLoc != null && destLoc != null)
                                {
                                    destLoc.carlocstatus = (int)EnmLocationStatus.Occupy;
                                    destLoc.carwheelbase = srcLoc.carwheelbase;
                                    destLoc.carsize = srcLoc.carsize;
                                    destLoc.carintime = srcLoc.carintime;
                                    destLoc.iccode = srcLoc.iccode;
                                    destLoc.overallLg = srcLoc.overallLg;
                                    destLoc.overhang = srcLoc.overhang;
                                    CCommonMethods.myMethods.UpdateCarLocationInfo(destLoc);

                                    CCommonMethods.myMethods.InitCarPOSN(ref srcLoc);
                                }
                            }
                            break;
                        }
                    case (int)EnmTaskType.MoveEquipTask:// 移动设备
                        {
                            break;
                        }
                }
                CCommonMethods.myMethods.InitDeviceStatus(strICCode);

                CCommonMethods.myMethods.InitDeviceStatus(ref deviceStatus);
                //清除当前IC卡在刷卡队列中的所有刷卡记录
                List<CWorkQueueDto> lstWorkQueue = CCommonMethods.myMethods.DeleteWorkQueueList(strICCode);
                foreach (CWorkQueueDto table in lstWorkQueue)
                {
                    WCFPushMsgToClient(table);
                }

                if ((int)EnmModel.Automatic == deviceStatus.devicemode && (int)EnmSMGType.Hall == deviceStatus.devicetype)
                {// 当前设备全自动模式
                    if (0 == CCommonMethods.myMethods.GetSendTelegramQueue(nWareHouse, nDeviceID).Count &&
                        0 == CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nDeviceID).Count)
                    {// 无下一作业
                        return EnmFaultType.Success;
                    }

                    if (CConfigManagement.myPara.HashGarageType.ContainsKey(nWareHouse)
                       && "VerticalLifting" == (string)CConfigManagement.myPara.HashGarageType[nWareHouse])
                    {
                        return EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
                    }
                    else
                    {
                        return EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
                    }
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CPushService.HandCompleteTask 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

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
        public EnmFaultType HandResetTask(int warehouse, int code)
        {
            try
            {
                CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(warehouse, code);
                if (null == deviceStatus)
                {
                    return EnmFaultType.Fail;
                }
                int nWareHouse = deviceStatus.warehouse;
                int nDeviceID = deviceStatus.devicecode;
                string strICCode = deviceStatus.iccode;
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(strICCode);
                // 作业类型判断
                switch (deviceStatus.tasktype)
                {
                    case (int)EnmTaskType.EntryTask:// 存车
                        {
                            if (null != carPOSNTable)
                            {// 收到（1001,101）报文之后故障处理（未装载卸载，但此时数据库车位信息已有存车信息）
                                //初始化车位信息
                                CCommonMethods.myMethods.InitCarPOSN(ref carPOSNTable);
                            }
                            break;
                        }
                    case (int)EnmTaskType.ExitTask:// 取车
                    case (int)EnmTaskType.TmpFetch://临时取物的取车
                        {
                            if (null != carPOSNTable && (EnmLocationStatus)carPOSNTable.carlocstatus != EnmLocationStatus.Occupy)
                            {// 发送（3,1）报文之后故障处理（未装载卸载，但此时数据库车位信息未清空正在出库）
                                carPOSNTable.carlocstatus = Convert.ToInt32(EnmLocationStatus.Occupy);
                                CCommonMethods.myMethods.UpdateCarLocationInfo(carPOSNTable);
                            }
                            break;
                        }
                    case (int)EnmTaskType.MoveCarTask:
                        {
                            if (!string.IsNullOrEmpty(deviceStatus.desccloc) && !string.IsNullOrEmpty(deviceStatus.scrloc))
                            {
                                CCarLocationDto srcLoc = CCommonMethods.myMethods.GetCarLocation(nWareHouse, deviceStatus.scrloc);
                                CCarLocationDto destLoc = CCommonMethods.myMethods.GetCarLocation(nWareHouse, deviceStatus.desccloc);
                                if (srcLoc != null && destLoc != null)
                                {
                                    CCommonMethods.myMethods.InitCarPOSN(ref destLoc);
                                    srcLoc.carlocstatus = (int)EnmLocationStatus.Occupy;
                                    CCommonMethods.myMethods.UpdateCarLocationInfo(srcLoc);
                                }
                            }
                            break;
                        }
                    case (int)EnmTaskType.MoveEquipTask:// 移动设备
                        {
                            break;
                        }
                }
                CCommonMethods.myMethods.InitDeviceStatus(strICCode);

                CCommonMethods.myMethods.InitDeviceStatus(ref deviceStatus);
                List<CWorkQueueDto> lstWorkQueue = CCommonMethods.myMethods.DeleteWorkQueueList(strICCode);
                foreach (CWorkQueueDto table in lstWorkQueue)
                {
                    WCFPushMsgToClient(table);
                }

                if ((int)EnmModel.Automatic == deviceStatus.devicemode && (int)EnmSMGType.Hall == deviceStatus.devicetype)
                {// 当前设备全自动模式
                    if (0 == CCommonMethods.myMethods.GetSendTelegramQueue(nWareHouse, nDeviceID).Count &&
                        0 == CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nDeviceID).Count)
                    {// 无下一作业
                        return EnmFaultType.Success;
                    }

                    if (CConfigManagement.myPara.HashGarageType.ContainsKey(nWareHouse)
                       && "VerticalLifting" == (string)CConfigManagement.myPara.HashGarageType[nWareHouse])
                    {
                        return EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
                    }
                    else
                    {
                        return EnmFaultType.WorkQueueNotEmpty;// 如果有下一作业，是否启动下一作业
                    }
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {
                //打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.HandResetTask 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        public void InitDeviceFault(CDeviceFaultDto deviceFaultTBL)
        {
          
        }

        /// <summary>
        /// MURO继续，先判断设备是否处于全自动中，
        /// 是否可用，
        /// 是否可接收新指令，
        /// 都OK，则判断中跑车当前状态，是要装载完，还是要卸载
        /// 如果是存车，要执行车厅装载，发送的数据是后轮距巷道、中心偏移距离、轴距
        /// 如果是取车，要执行车位装载，则发送的数据是全车长度，前悬、轴距
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public int MuroDeviceTask(int warehouse, int code) 
        {
            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(warehouse, code);
            if (null == deviceStatus)
            {
                return 101; //找不到相应的移动设备
            }
            try
            {
                #region
                if (deviceStatus.devicemode != (int)EnmModel.Automatic)
                {
                    return 102;//不处于全自动状态
                }
                if (deviceStatus.isable == 0)
                {
                    return 103;//设备不可用
                }
                if (deviceStatus.isavailable == 0)
                {
                    return 104;//设备不可接收新指令
                }
                if (deviceStatus.tasktype == (int)EnmTaskType.Init)
                {
                    return 105;
                }
                if (deviceStatus.iccode == null)
                {
                    return 106;
                }
                int isHasCar = CCommonMethods.myMethods.JudgeHasCar(warehouse, code);
                if (isHasCar > 100)
                {
                    return isHasCar; //查询搬运器上有车信息错误
                }
                //存车故障时
                CCarLocationDto lctn = null;
                lctn = CCommonMethods.myMethods.GetCarLocation(deviceStatus.iccode);
                if (lctn == null)
                {
                    return 109;
                }
                CTaskTriggerMgmt taskTriggerMgt = new CTaskTriggerMgmt();
                taskTriggerMgt.SetTaskTrigger(warehouse);

                if (deviceStatus.tasktype == (int)EnmTaskType.EntryTask)
                {
                    #region 存车
                    //车厅上有故障时，只能将车放到车厅上
                    //如果将车放到搬运器上时，则没有全车长度，前悬数据
                    if (isHasCar == 10)
                    {
                        if (lctn.overallLg == null ||
                            lctn.overallLg == 0 ||
                            lctn.overhang == null ||
                            lctn.overhang == 0)
                        {
                            //提示当前作业为非法作业，只能强制中止
                            return 110;
                        }
                        //可以直接发送卸载指令，数据：全车长度，前悬，轴距，源地址，目的地址
                       return taskTriggerMgt.DealMuroUnloadCmd(deviceStatus, lctn);
                    }
                    //搬运器上无车时，发送车厅装载指令
                    if (isHasCar == 20)
                    {
                        //发送车厅的装载指令
                       return taskTriggerMgt.DealMUROLoadCmd(deviceStatus, lctn, true);
                    }
                    #endregion                   
                }
                else
                {
                    #region 取车或物取
                    //搬运器上无车时，发送装载指令
                    if (isHasCar == 20)
                    {
                        return taskTriggerMgt.DealMUROLoadCmd(deviceStatus, lctn, false);
                    }
                    //搬运器上有车时，发送卸载指令
                    if (isHasCar == 10)
                    {
                        return taskTriggerMgt.DealMuroUnloadCmd(deviceStatus, lctn);
                    }
                    #endregion
                }
                #endregion

                return 0;
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("WCFServiceLib.CPushService.MuroDeviceTask 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return 300;
            }            
        }

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
        public EnmFaultType PayFeesAndTakeCar(struBillInfo struBillInfo)
        {
            try
            {
                //----add by wanxiaona20150609
                EnmFaultType enmResult = CCommonMethods.myMethods.CheckICCardID(struBillInfo.strICCardID);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;//当前IC卡没有制卡
                }
                CICCardDto icCardTBL = CCommonMethods.myMethods.GetICCard(struBillInfo.strICCardID);
                //----add by wanxiaona20150609
                CICCardLogDto icCardLog = new CICCardLogDto();
                EnmFaultType enmVehicleExitReturn = EnmFaultType.Success;
                if (struBillInfo.nICCardType == (int)EnmICCardType.Temp)//临时卡
                {
                    CCarLocationDto carLocationTBL = CCommonMethods.myMethods.GetCarLocation(struBillInfo.strICCardID);
                    if (null == carLocationTBL)
                    {
                        return EnmFaultType.NoCarInGarage;//当前卡没有车存在车库
                    }

                    CDeviceStatusDto hallStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(struBillInfo.nWareHouse, struBillInfo.nHallID);
                    if (null == hallStatusTBL)
                    {
                        return EnmFaultType.NotFoundEquip;//没有找到指定目的车厅
                    }

                    icCardLog.starttime = struBillInfo.dtStartTime;
                    icCardLog.endtime = struBillInfo.dtEndTime;

                    // 车辆出库
                    enmVehicleExitReturn = VehicleExit(struBillInfo.nWareHouse, carLocationTBL.carlocaddr, hallStatusTBL.devicecode.ToString());

                    if (EnmFaultType.Success != enmVehicleExitReturn && 
                        EnmFaultType.Add != enmVehicleExitReturn)
                    {
                        return enmVehicleExitReturn;
                    }
                }
                else if (struBillInfo.nICCardType == (int)EnmICCardType.Fixed || struBillInfo.nICCardType == (int)EnmICCardType.FixedLocation)   //定期卡
                {
                    icCardLog.starttime = struBillInfo.dtStartTime;
                    icCardLog.endtime = struBillInfo.dtEndTime;
                    string strClassName = string.Empty;
                    object[] arrPara = null;
                   
                    if (!CConfigManagement.myPara.HashBillingPara.ContainsKey(struBillInfo.nFeeType))
                    {
                        return EnmFaultType.InvalidFeeType;//无效计费类型
                    }
                    ArrayList lstBillingPara = (ArrayList)CConfigManagement.myPara.HashBillingPara[struBillInfo.nFeeType];
                    strClassName = (string)lstBillingPara[0];
                    arrPara = (object[])lstBillingPara[1];

                    if (string.IsNullOrWhiteSpace(strClassName))
                    {
                        return EnmFaultType.Fail; //计算截止日期失败
                    }

                    if (null == icCardTBL.tariffid || (null != icCardTBL.tariffid && 0 != struBillInfo.nTariffID && icCardTBL.tariffid != struBillInfo.nTariffID))
                    {
                        icCardTBL.tariffid = struBillInfo.nTariffID;
                    }

                    icCardTBL.icstarttime = struBillInfo.dtStartTime;
                    icCardTBL.icdeadline = struBillInfo.dtEndTime;
                    if (!CCommonMethods.myMethods.UpdateICCardInfo(icCardTBL))
                    {
                        return EnmFaultType.FailToUpdate;//更新数据库失败
                    }
                    //m_mainCallback.PushMessageToWCF(icCardTBL);
                    WCFPushMsgToClient(icCardTBL);                 
                }

                icCardLog.iccode = struBillInfo.strICCardID;
                icCardLog.ictype = struBillInfo.nICCardType;
                icCardLog.paymenttime = DateTime.Now;
                icCardLog.calculatedays = struBillInfo.strCalculateDays;
                icCardLog.feetype = (int)struBillInfo.nFeeType;
                icCardLog.tariffnorm = struBillInfo.fTariffNorm;
                icCardLog.payablefee = struBillInfo.fPayableFee;
                icCardLog.actualfee = struBillInfo.fActualFee;
                icCardLog.optcode = struBillInfo.strOptCode;

                if (null != icCardTBL.userid)
                {// 获取用户姓名
                    CCarCustomerDto customer = CCommonMethods.myMethods.GetCUSTByID((int)icCardTBL.userid);

                    if (null != customer)
                    {
                        icCardLog.username = customer.username;
                    }
                }

                // 插入IC卡缴费日志
                if (!CCommonMethods.myMethods.InsertICCardLogInfo(icCardLog))
                {
                    return EnmFaultType.FailToInsert;//插入数据库失败
                }

                return enmVehicleExitReturn;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WCFServiceLib.CPushService.PayFeesAndTakeCar 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
        #endregion 对计费的管理

        #region 对移动APP请求存取车报文的管理
        /// <summary>
        /// 扫描存车_根据IC卡号
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.Null;//传入的参数不正确
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.LossORCancel;//IC卡注销或挂失
        /// EnmFaultType.NotMatch;//IC卡类型不正确或者与第一次刷卡卡号不一致，请重新刷卡
        /// EnmFaultType.CarInGarage;       //当前卡有车存在车库
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.Add; //前面有人正在作业，请稍后存车
        /// EnmFaultType.HallNotCar;// 车厅无车，请将车开到车厅内再进行刷卡存车
        /// EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物
        /// EnmFaultType.OverCarInSize;//车体超限，请退出
        /// EnmFaultType.NotFoundCarPOSN;// 已无合适车位，请退出
        /// EnmFaultType.Fail;// 上位控制系统故障或者获取通信模式块失败，请查看配置文件是否正确
        /// add by wanxiaona20160106
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <returns></returns>
        public EnmFaultType ScanVehicleEnterByICCard(string strICCardID, int nWareHouse, int nHallID)
        {
            try
            {
                CRequestAction requestAction = new CRequestAction();
                int nActionFlag = 0;
                //根据库区号和车厅号获取车厅状态信息
                CDeviceStatusDto HallStatusTable = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (string.IsNullOrWhiteSpace(strICCardID) || null == HallStatusTable)
                {
                    return EnmFaultType.Null;//传入的参数不正确
                }
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(HallStatusTable.warehouse);
                bool bETVStat = taskTrigger.CheckETVStat(HallStatusTable.warehouse, HallStatusTable.devicecode);
                //车厅工作模式不是全自动模式05.wav
                if ((int)EnmModel.Automatic != HallStatusTable.devicemode || !bETVStat)
                {
                    requestAction.soundName = "01.wav";//系统已转为人工作业不能存取车辆01.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }

                CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);
                //未制卡，不是本系统卡
                if (null == ICCardTable)
                {
                    requestAction.soundName = "10.wav";//该卡不是本系统用卡10.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.NoICCardInfo;//没有制卡
                }

                //注销或挂失06.wav
                if ((int)EnmICCardStatus.Lost == ICCardTable.icstatus || (int)EnmICCardStatus.Disposed == ICCardTable.icstatus)
                {
                    requestAction.soundName = "06.wav";//本卡已经挂失或注销，不能存取车辆06.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.LossORCancel;//IC卡注销或挂失
                }

                //IC卡类型不正确
                if ((int)EnmICCardType.Temp != ICCardTable.ictype && (int)EnmICCardType.FixedLocation != ICCardTable.ictype
                    && (int)EnmICCardType.Fixed != ICCardTable.ictype)
                {
                    requestAction.soundName = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.NotMatch;//IC卡类型不正确
                }
                
                CDeviceStatusDto DeviceStatusTable = CCommonMethods.myMethods.GetDeviceStatus(strICCardID);
                //已经在其他库区执行了刷卡
                if (null != DeviceStatusTable && (DeviceStatusTable.warehouse != nWareHouse
                    || DeviceStatusTable.devicecode != nHallID))
                {
                    requestAction.soundName = "08.wav";// 该卡正在作业08.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                CCarLocationDto CarLocation = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if ((null == CarLocation)
                    || (null != CarLocation && (int)EnmLocationStatus.Occupy != CarLocation.carlocstatus
                        && (((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.prevnode))
                             || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(nWareHouse) && CConfigManagement.myPara.DicCIMCTmpFetch[nWareHouse]
                                  && (int)EnmTaskType.ExitTask == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.prevnode)))))
                {
                    //进车厅或进出两用车厅
                    if ((int)EnmHallType.Entance == HallStatusTable.halltype || (int)EnmHallType.EnterOrExit == HallStatusTable.halltype)
                    {
                        //车厅进车检测OK，或取物OK
                        if ((int)EnmTaskType.EntryTask == HallStatusTable.tasktype || (int)EnmTaskType.TmpFetch == HallStatusTable.tasktype
                            || ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype && CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(nWareHouse)
                            && CConfigManagement.myPara.DicCIMCTmpFetch[nWareHouse]))
                        {
                            //处理存车刷卡
                            return taskTrigger.DealParkingScan(ref nActionFlag, ref requestAction, strICCardID, HallStatusTable, CarLocation);
                        }
                        //进出车厅正在执行取车作业
                        else if ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype)
                        {
                            requestAction.soundName = "33.wav";// 前面有人正在取车，请稍后存车33.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.Add; //前面有人正在作业，请稍后存车;
                        }
                        //进出车厅没有存取车作业在执行，且车厅空闲
                        else
                        {
                            requestAction.soundName = "13.wav";// 车厅无车，请将车开到车厅内再进行刷卡存车13.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallNotCar;// 车厅无车，请将车开到车厅内再进行刷卡存车
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
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物
                        }
                        else
                        {
                            requestAction.soundName = "14.wav";//出车厅，不允许存车14.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物;
                        }
                    }
                }
                else if ((int)EnmLocationType.Normal == CarLocation.carloctype && ((int)EnmLocationStatus.Occupy == CarLocation.carlocstatus))
                {
                    return EnmFaultType.CarInGarage;//当前卡有车存在车库
                }
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
                        CLOGException.Trace(nWareHouse, "SwipeICCard", "数据库中IC卡" + strICCardID + "记录的车位类型是车厅，Hall ID:" + nHallID);
                    }
                    else if (null != CarLocation && (int)EnmLocationStatus.TmpFetch == CarLocation.carlocstatus && (int)EnmTaskType.EntryTask == HallStatusTable.tasktype)
                    {
                        requestAction.soundName = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "该卡异常，请联系管理人员处理32.wav, Hall ID:" + struHallEquip.nHallID);
                    }
                    else
                    {
                        CLOGException.Trace(nWareHouse, "SwipeICCard", "新的异常场景，需要增加对当前异常场景的处理，IC卡卡号：" + strICCardID + "，Hall ID:" + nHallID);
                    }

                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.ScanVehicleEnterByICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 扫描存车_根据车牌号
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.Null;//传入的参数不正确
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.LossORCancel;//IC卡注销或挂失
        /// EnmFaultType.NotMatch;//IC卡类型不正确或者与第一次刷卡卡号不一致，请重新刷卡
        /// EnmFaultType.CarInGarage;       //当前卡有车存在车库
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.Add; //前面有人正在作业，请稍后存车
        /// EnmFaultType.HallNotCar;// 车厅无车，请将车开到车厅内再进行刷卡存车
        /// EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物
        /// EnmFaultType.OverCarInSize;//车体超限，请退出
        /// EnmFaultType.NotFoundCarPOSN;// 已无合适车位，请退出
        /// EnmFaultType.Fail;// 上位控制系统故障或者获取通信模式块失败，请查看配置文件是否正确
        /// add by wanxiaona20160106
        /// </summary>
        /// <param name="strLicPlteNbr"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <returns></returns>
        public EnmFaultType ScanVehicleEnterByLicPlteNum(string strLicPlteNbr, int nWareHouse, int nHallID)
        {
            try
            {
                CRequestAction requestAction = new CRequestAction();
                int nActionFlag = 0;
                //根据库区号和车厅号获取车厅状态信息
                CDeviceStatusDto HallStatusTable = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (string.IsNullOrWhiteSpace(strLicPlteNbr) || null == HallStatusTable)
                {
                    return EnmFaultType.Null;//传入的参数不正确
                }
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(HallStatusTable.warehouse);
                bool bETVStat = taskTrigger.CheckETVStat(HallStatusTable.warehouse, HallStatusTable.devicecode);
                //车厅工作模式不是全自动模式05.wav
                if ((int)EnmModel.Automatic != HallStatusTable.devicemode || !bETVStat)
                {
                    requestAction.soundName = "01.wav";//系统已转为人工作业不能存取车辆01.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }
            
                CDeviceStatusDto DeviceStatusTable = CCommonMethods.myMethods.GetDeviceStatus(strLicPlteNbr);
                //已经在其他库区执行了刷卡
                if (null != DeviceStatusTable && (DeviceStatusTable.warehouse != nWareHouse
                    || DeviceStatusTable.devicecode != nHallID))
                {
                    requestAction.soundName = "08.wav";// 该卡正在作业08.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                CCarLocationDto CarLocation = CCommonMethods.myMethods.GetCarLocationByLicPlteNum(strLicPlteNbr);
                if ((null == CarLocation)
                    || (null != CarLocation && (int)EnmLocationStatus.Occupy != CarLocation.carlocstatus
                        && (((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.TmpFetchHasCar == HallStatusTable.prevnode))
                             || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(nWareHouse) && CConfigManagement.myPara.DicCIMCTmpFetch[nWareHouse]
                                  && (int)EnmTaskType.ExitTask == HallStatusTable.tasktype && ((int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.currentnode || (int)EnmFlowNodeDescp.ExitHasCar == HallStatusTable.prevnode)))))
                {
                    //进车厅或进出两用车厅
                    if ((int)EnmHallType.Entance == HallStatusTable.halltype || (int)EnmHallType.EnterOrExit == HallStatusTable.halltype)
                    {
                        //车厅进车检测OK，或取物OK
                        if ((int)EnmTaskType.EntryTask == HallStatusTable.tasktype || (int)EnmTaskType.TmpFetch == HallStatusTable.tasktype
                            || ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype && CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(nWareHouse)
                            && CConfigManagement.myPara.DicCIMCTmpFetch[nWareHouse]))
                        {
                            //处理存车刷卡
                            return taskTrigger.DealParkingScan(ref nActionFlag, ref requestAction, strLicPlteNbr, HallStatusTable, CarLocation);
                        }
                        //进出车厅正在执行取车作业
                        else if ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype)
                        {
                            requestAction.soundName = "33.wav";// 前面有人正在取车，请稍后存车33.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.Add; //前面有人正在作业，请稍后存车;
                        }
                        //进出车厅没有存取车作业在执行，且车厅空闲
                        else
                        {
                            requestAction.soundName = "13.wav";// 车厅无车，请将车开到车厅内再进行刷卡存车13.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallNotCar;// 车厅无车，请将车开到车厅内再进行刷卡存车
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
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物
                        }
                        else
                        {
                            requestAction.soundName = "14.wav";//出车厅，不允许存车14.wav
                            if (2 != (nActionFlag & 2))
                            {
                                nActionFlag += 2;
                            }
                            requestAction.RequestAction(nActionFlag);
                            return EnmFaultType.HallExit;// 出车厅，不允许存车和临时取物;
                        }
                    }
                }
                else if ((int)EnmLocationType.Normal == CarLocation.carloctype && ((int)EnmLocationStatus.Occupy == CarLocation.carlocstatus))
                {
                    return EnmFaultType.CarInGarage;//当前卡有车存在车库
                }
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
                        CLOGException.Trace(nWareHouse, "SwipeICCard", "数据库中车牌号" + strLicPlteNbr + "记录的车位类型是车厅，Hall ID:" + nHallID);
                    }
                    else if (null != CarLocation && (int)EnmLocationStatus.TmpFetch == CarLocation.carlocstatus && (int)EnmTaskType.EntryTask == HallStatusTable.tasktype)
                    {
                        requestAction.soundName = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "SwipeICCard", "该卡异常，请联系管理人员处理32.wav, Hall ID:" + struHallEquip.nHallID);
                    }
                    else
                    {
                        CLOGException.Trace(nWareHouse, "SwipeICCard", "新的异常场景，需要增加对当前异常场景的处理，车牌号：" + strLicPlteNbr + "，Hall ID:" + nHallID);
                    }

                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                return EnmFaultType.Success;
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.ScanVehicleEnterByLicPlteNum 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 预约取车_根据卡号
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
        /// add by wanxiaona201506
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        public EnmFaultType VehicleExitByICCard(string strICCardID, ref string strHallID)
        {
            try
            {
                EnmFaultType enmReturn = CCommonMethods.myMethods.CheckICCardID(strICCardID);
                if (EnmFaultType.Success != enmReturn)
                {
                    return enmReturn;
                }

                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocation(strICCardID);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
                }

                if (null != CCommonMethods.myMethods.GetDeviceStatus(strICCardID))
                {
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                int nWareHouse = (int)carPOSNTable.warehouse;
                int nHallID = 0;
                CBaseMethods.MyBase.StringToUInt32(strHallID, out nHallID);
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);
                if (CBaseMethods.MyBase.IsEmpty(nHallID))
                {// 用户没选择车厅时，自动分配
                    nHallID = taskTrigger.ALLOCHallByQueue(nWareHouse,0,carPOSNTable);
                }

                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;       //没有找到指定设备
                }
                if ((int)EnmModel.Automatic != deviceStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }
                if (deviceStatusTBL.devicetype != (int)EnmSMGType.Hall)
                {
                    return EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
                }

                if (deviceStatusTBL.halltype != (int)EnmHallType.EnterOrExit)
                {
                    return EnmFaultType.EquipIncorrect;//车厅不是进出两用车厅
                }

                if (CConfigManagement.myPara.DicHallIDDictionary.ContainsKey(nWareHouse) && CConfigManagement.myPara.DicHallIDDictionary[nWareHouse].ContainsKey(nHallID))
                {
                    strHallID = CConfigManagement.myPara.DicHallIDDictionary[nWareHouse][nHallID].strHallName;
                }
                
                return taskTrigger.VehicleExit(strICCardID, deviceStatusTBL, carPOSNTable, true);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.VehicleExitByICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }

        /// <summary>
        /// 预约取车_根据车牌号
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
        /// add by wanxiaona201506
        /// </summary>
        /// <param name="strLicPlteNbr"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        public EnmFaultType VehicleExitByLicPlteNum(string strLicPlteNbr, ref string strHallID)
        {
            try
            {
                CCarLocationDto carPOSNTable = CCommonMethods.myMethods.GetCarLocationByLicPlteNum(strLicPlteNbr);
                if (null == carPOSNTable)
                {
                    return EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
                }

                if (null != CCommonMethods.myMethods.GetDeviceStatus(strLicPlteNbr))
                {
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }

                int nWareHouse = (int)carPOSNTable.warehouse;
                int nHallID = 0;
                CBaseMethods.MyBase.StringToUInt32(strHallID, out nHallID);
                CTaskTriggerMgmt taskTrigger = new CTaskTriggerMgmt();
                taskTrigger.SetTaskTrigger(nWareHouse);
                if (CBaseMethods.MyBase.IsEmpty(nHallID))
                {// 用户没选择车厅时，自动分配
                    nHallID = taskTrigger.ALLOCHallByQueue(nWareHouse,0,carPOSNTable);
                }

                CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nHallID);
                if (null == deviceStatusTBL)
                {
                    return EnmFaultType.NotFoundEquip;       //没有找到指定设备
                }
                if ((int)EnmModel.Automatic != deviceStatusTBL.devicemode)
                {
                    return EnmFaultType.NotAutomatic;//非全自动模式
                }
                if (deviceStatusTBL.devicetype != (int)EnmSMGType.Hall)
                {
                    return EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
                }

                if (deviceStatusTBL.halltype != (int)EnmHallType.EnterOrExit)
                {
                    return EnmFaultType.EquipIncorrect;//车厅不是进出两用车厅
                }

                if (CConfigManagement.myPara.DicHallIDDictionary.ContainsKey(nWareHouse) && CConfigManagement.myPara.DicHallIDDictionary[nWareHouse].ContainsKey(nHallID))
                {
                    strHallID = CConfigManagement.myPara.DicHallIDDictionary[nWareHouse][nHallID].strHallName;
                }
                
                return taskTrigger.VehicleExit(strLicPlteNbr, deviceStatusTBL, carPOSNTable, true);
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("CClientAccessService.VehicleExitByLicPlteNum 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return EnmFaultType.Exception;
            }
        }
        #endregion
    }
}
