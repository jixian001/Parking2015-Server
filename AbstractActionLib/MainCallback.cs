using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseManagementLib;
using ConfigManagmtLib;
using BaseMethodLib;
using LOGManagementLib;

namespace AbstractActionLib
{
    public delegate void MainEventHandler(object sender, CMainEventArgs e);

    public delegate void StatEventHandler(object sender, CStatEventArgs e);

    /// <summary>
    /// 回调事件参数类EventArgs
    /// </summary>
    public class CMainEventArgs : EventArgs
    {
        private object m_objTable;
        /// <summary>
        /// 回调事件参数
        /// </summary>
        public object ObjTable
        {
            get
            {
                return m_objTable;
            }
            set
            {
                m_objTable = value;
            }
        }
    }

    public class CStatEventArgs : EventArgs
    {
        private bool m_bPLCStat;
        /// <summary>
        /// 回调事件参数
        /// </summary>
        public bool PLCStat
        {
            get
            {
                return m_bPLCStat;
            }
            set
            {
                m_bPLCStat = value;
            }
        }

        private bool[] m_bICCardReaderStat;
        /// <summary>
        /// 回调事件参数
        /// </summary>
        public bool[] ICCardReaderStat
        {
            get
            {
                return m_bICCardReaderStat;
            }
            set
            {
                m_bICCardReaderStat = value;
            }
        }

    }

    /// <summary>
    /// 回调类
    /// </summary>
    public class CMainCallback
    {
        public event MainEventHandler MainCallbackEvent;//回调事件
        /// <summary>
        /// 推送消息到WCF服务
        /// </summary>
        /// <param name="text"></param>
        public void PushMessageToWCF(object objTable)
        {
            try
            {
                if (MainCallbackEvent != null)
                {
                    if (objTable == null)
                    {
                        return;
                    }
                    CMainEventArgs e = new CMainEventArgs();
                    switch (objTable.GetType().ToString())
                    {
                        // 设备状态devicestatus
                        case "DatabaseManagementLib.CDeviceStatusDto":
                            {
                                e.ObjTable = objTable;
                                break;
                            }
                        // 车位信息carlocation
                        case "DatabaseManagementLib.CCarLocationDto":
                            {
                                e.ObjTable = objTable;
                                break;
                            }
                        // 设备故障devicefault
                        case "DatabaseManagementLib.CDeviceFaultDto":
                            {
                                e.ObjTable = objTable;
                                break;
                            }
                        #region 注释
                        //// IC卡iccard
                        //case "DatabaseManagementLib.CICCardDto":
                        //    {
                        //        e.ObjTable = (CICCardDto)objTable;
                        //        break;
                        //    }
                        //// 操作员operator
                        //case "DatabaseManagementLib.COperatorDto":
                        //    {
                        //        e.ObjTable = (COperatorDto)objTable;
                        //        break;
                        //    }
                        //// 车主信息carcustomer
                        //case "DatabaseManagementLib.CCarCustomerDto":
                        //    {
                        //        e.ObjTable = (CCarCustomerDto)objTable;
                        //        break;
                        //    }
                        //// 计费tariff
                        //case "DatabaseManagementLib.CTariffDto":
                        //    {
                        //        e.ObjTable = (CTariffDto)objTable;
                        //        break;
                        //    }
                        //// 语音sound
                        //case "DatabaseManagementLib.CSoundDto":
                        //    {
                        //        e.ObjTable = (CSoundDto)objTable;
                        //        break;
                        //    }
                        //// 工作队列workqueue
                        //case "DatabaseManagementLib.CWorkQueueDto":
                        //    {
                        //        e.ObjTable = (CWorkQueueDto)objTable;
                        //        break;
                        //    }
                        //// LED文本ledcontent
                        //case "DatabaseManagementLib.CLedContentDto)":
                        //    {
                        //        e.ObjTable = (CLedContentDto)objTable;
                        //        break;
                        //    }
                        //// IC卡缴费日志iccardlog
                        //case "DatabaseManagementLib.CICCardLogDto":
                        //    {
                        //        e.ObjTable = (CICCardLogDto)objTable;
                        //        break;
                        //    }
                        //// 系统日志systemlog
                        //case "DatabaseManagementLib.CSystemLogDto":
                        //    {
                        //        e.ObjTable = (CSystemLogDto)objTable;
                        //        break;
                        //    }
                        //// 报文日志telegramlog
                        //case "DatabaseManagementLib.CTelegramLogDto":
                        //    {
                        //        e.ObjTable = (CTelegramLogDto)objTable;
                        //        break;
                        //    }
                        //// 设备状态日志devicestatuslog
                        //case "DatabaseManagementLib.CDevicesStatusLogDto":
                        //    {
                        //        e.ObjTable = (CDeviceStatusLogDto)objTable;
                        //        break;
                        //    }
                        //// 设备故障日志devicefaultlog
                        //case "DatabaseManagementLib.CDeviceFaultLogDto":
                        //    {
                        //        e.ObjTable = (CDeviceFaultLogDto)objTable;
                        //        break;
                        //    }
                        #endregion 注释
                        default:
                            {
                                break;
                            }
                    }
                    MainCallbackEvent(this, e);
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CMainCallback.PushMessageToWCF 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        public event StatEventHandler StatCallbackEvent;//回调事件
        public void PushStatToNotify(bool bStat)
        {
            try
            {
                if (StatCallbackEvent != null)
                {
                    CStatEventArgs e = new CStatEventArgs();
                    e.PLCStat = bStat;
                    StatCallbackEvent(this, e);
                }
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CMainCallback.PushStatToNotify 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }

    }
}
