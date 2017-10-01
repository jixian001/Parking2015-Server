using System;
using System.Collections.Generic;
using System.Linq;
using CommonMethodLib;
using WorkFlowLib;
using LOGManagementLib;
using BaseMethodLib;

namespace WCFServiceLib
{
    public class CWCFSubscription : IDisposable
    {
        private CMainCallback m_mainCallback;
        private CMainEntrance mainEntrance;
        private CPushService pushService;
        public CWCFSubscription(CMainCallback mainCallback)
        {
            //CLOGException.Trace("——————CWCFSubscription");
            m_mainCallback = mainCallback;
            pushService = new CPushService();
        }

        /// <summary>
        /// 启动业务
        /// </summary>
        public void StartSubscript()
        {
            try
            {
                //CLOGException.Trace("——————StartSubscript");
                m_mainCallback.MainCallbackEvent += new MainEventHandler(PushMsgToService);
                mainEntrance = new CMainEntrance(m_mainCallback);
                mainEntrance.Init();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.StartSubscript  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 停止业务
        /// </summary>
        public void Dispose()
        {
            try
            {
                m_mainCallback.MainCallbackEvent -= new MainEventHandler(PushMsgToService);
                mainEntrance.Dispose();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.Dispose  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 推送消息至WCF
        /// </summary>
        /// <param name="text"></param>
        private void PushMsgToService(object sender, CMainEventArgs e)
        {
            try
            {
                pushService.WCFPushMsgToClient(e.ObjTable);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.PushMsgToService  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        #region 响应数据中心云服务器PUSH事件
        /// <summary>
        /// 响应数据中心云服务器PUSH事件_远程/扫描取车根据IC卡号
        /// </summary>
        /// <param name="text"></param>
        public void VehicleExitByICCard(string strICCardID, ref string strHallID)
        {
            try
            {
                pushService.VehicleExitByICCard(strICCardID, ref strHallID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.PushMsgToService  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 响应数据中心云服务器PUSH事件_远程/扫描取车根据车牌号
        /// </summary>
        /// <param name="text"></param>
        public void VehicleExitByLicPlteNum(string strLicPlteNbr, ref string strHallID)
        {
            try
            {
                pushService.VehicleExitByLicPlteNum(strLicPlteNbr, ref strHallID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.PushMsgToService  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 响应数据中心云服务器PUSH事件_扫描存车根据IC卡号
        /// </summary>
        /// <param name="text"></param>
        public void ScanVehicleEnterByICCard(string strICCardID, int nWareHouse, int nHallID)
        {
            try
            {
                pushService.ScanVehicleEnterByICCard(strICCardID, nWareHouse, nHallID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.PushMsgToService  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 响应数据中心云服务器PUSH事件_扫描存车根据IC卡号
        /// </summary>
        /// <param name="text"></param>
        public void ScanVehicleEnterByLicPlteNum(string strICCardID, int nWareHouse, int nHallID)
        {
            try
            {
                pushService.ScanVehicleEnterByLicPlteNum(strICCardID, nWareHouse, nHallID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WCFServiceLib.CWCFSubscription.PushMsgToService  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
        #endregion
    }
}
