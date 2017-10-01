using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommunicationLib;
using ICCardManagementLib;
using System.Drawing;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CommonMethodLib
{
    /// <summary>
    /// 公共成员类
    /// </summary>
    public class CCommonMethods
    {

        private int m_nReservedMaxCount = CConfigManagement.myPara.ReservedMaxCount;// 最大可定制的定期卡个数值(读取配置文件)

        private static object m_objDeviceStatusLocker = new object();
        private static object m_objCarLocationLocker = new object();
        private static object m_objICCardLocker = new object();
        private static object m_objWorkQueueLocker = new object();
        private static object m_objCarCustomerLocker = new object();
        private static object m_objOperatorLocker = new object();
        private static object m_objTariffLocker = new object();
        private static object m_objLedContentLocker = new object();
        private static object m_objDeviceFaultLocker = new object();
        private static object m_objSoundLocker = new object();

        private static List<CDeviceStatusDto> m_lstDeviceStatusInfo;
        private static List<CCarLocationDto> m_lstCarLocationInfo;
        private static List<CICCardDto> m_lstICCardInfo;
        private static List<CWorkQueueDto> m_lstWorkQueueInfo;
        private static List<CCarCustomerDto> m_lstCarCustomerInfo;
        private static List<COperatorDto> m_lstOperatorInfo;
        private static List<CTariffDto> m_lstTariffInfo;
        private static List<CLedContentDto> m_lstLedContentInfo;
        private static List<CDeviceFaultDto> m_lstDeviceFaultInfo;
        private static List<CSoundDto> m_lstSoundInfo;

        private CCommonMethods()
        {
            CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceFaultInfo);
            CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
        }

        #region 单一实例模式
        public static CMainCallback m_mainCallback = null;// add by wanxiaona20150805
        private static readonly CCommonMethods m_commonMehtods = new CCommonMethods();

        /// <summary>
        /// 全局静态CCommonMethods
        /// </summary>
        public static CCommonMethods myMethods
        {
            get
            {
                return m_commonMehtods;
            }
        }
        #endregion 单一实例模式

        #region 定期卡或固定卡允许门外刷卡取车
        /// <summary>
        /// 获取定期卡或固定卡允许门外刷卡取车标志
        /// </summary>
        public string GetFixCard_AllowOutGet() 
        {
            return CConfigManagement.myPara.AllowUserSetFixGetCarLimit;           
        }

        /// <summary>
        ///  设置定期卡或固定卡允许门外刷卡取车
        /// </summary>
        /// <param name="value"></param>
        public int SetFixCard_AllowOutGet(string value) 
        {
            return CConfigManagement.myPara.ModifyUserGetFixCarLimit(value);
        } 
        #endregion

        #region 基本公共函数
        /// <summary>
        /// SQL数据表类转换为字符串string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SQLDtoToString(object data)
        {
            string slog = "";
            if (null == data)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.SQLDtoToString", "数据表为空");
                return slog;
            }
            switch (data.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    {
                        CICCardDto dto = (CICCardDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.iccode + "]";
                        slog += "[" + dto.oldiccode + "]";
                        slog += "[" + dto.phycode + "]";
                        slog += "[" + dto.ictype + "]";
                        slog += "[" + dto.icstatus.ToString() + "]";
                        slog += "[" + dto.icnewtime.ToString() + "]";
                        slog += "[" + dto.iclosstime.ToString() + "]";
                        slog += "[" + dto.iclogouttime.ToString() + "]";
                        slog += "[" + dto.icstarttime.ToString() + "]";
                        slog += "[" + dto.icdeadline.ToString() + "]";
                        slog += "[" + dto.carlocaddr + "]";
                        slog += "[" + dto.warehouse.ToString() + "]";
                        slog += "[" + dto.userid.ToString() + "]";
                        slog += "[" + dto.tariffid.ToString() + "]";
                        slog += "[" + dto.priorityid.ToString() + "]";
                    }
                    break;
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    {
                        COperatorDto dto = (COperatorDto)data;
                        slog += "[" + dto.optcode + "]";
                        slog += "[" + dto.optname + "]";
                        slog += "[" + dto.optpassword + "]";
                        slog += "[" + dto.optphone + "]";
                        slog += "[" + dto.optaddr + "]";
                        slog += "[" + dto.opttype.ToString() + "]";
                        slog += "[" + dto.optpermission.ToString() + "]";
                    }
                    break;
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    {
                        CCarCustomerDto dto = (CCarCustomerDto)data;
                        slog += "[" + dto.userid + "]";
                        slog += "[" + dto.username + "]";
                        slog += "[" + dto.useraddr + "]";
                        slog += "[" + dto.userphone + "]";
                        slog += "[" + dto.usermobile + "]";
                        slog += "[" + dto.carnumber + "]";
                    }
                    break;
                // 车位信息carlocation
                case "DatabaseManagementLib.CCarLocationDto":
                    {
                        CCarLocationDto dto = (CCarLocationDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.carlocaddr + "]";
                        slog += "[" + dto.warehouse.ToString() + "]";
                        slog += "[" + dto.carlocside.ToString() + "]";
                        slog += "[" + dto.carloccolumn.ToString() + "]";
                        slog += "[" + dto.carloclayer.ToString() + "]";
                        slog += "[" + dto.carlocstatus.ToString() + "]";
                        slog += "[" + dto.carloctype.ToString() + "]";
                        slog += "[" + dto.carlocsize + "]";
                        slog += "[" + dto.iccode + "]";
                        slog += "[" + dto.carwheelbase.ToString() + "]";
                        slog += "[" + dto.carsize + "]";
                        slog += "[" + dto.isbackup + "]";
                        slog += "[" + dto.carintime + "]";
                        slog += "[" + dto.priorityid + "]";
                        slog += "[" + dto.region + "]";
                        slog += "[" + dto.carnumber + "]";
                        slog += "[" + dto.carpicture + "]";
                        slog += "[" + dto.direction + "]";
                    }
                    break;
                // 设备状态devicestatus
                case "DatabaseManagementLib.CDeviceStatusDto":
                    {
                        CDeviceStatusDto dto = (CDeviceStatusDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.devicecode + "]";
                        slog += "[" + dto.warehouse + "]";
                        slog += "[" + dto.iccode + "]";
                        slog += "[" + dto.tasktype + "]";
                        slog += "[" + dto.isable.ToString() + "]";
                        slog += "[" + dto.isavailable.ToString() + "]";
                        slog += "[" + dto.devicemode.ToString() + "]";
                        slog += "[" + dto.deviceaddr + "]";
                        slog += "[" + dto.devicetype.ToString() + "]";
                        slog += "[" + dto.halltype.ToString() + "]";
                        slog += "[" + dto.instep + "]";
                        slog += "[" + dto.outstep + "]";
                        slog += "[" + dto.runstep + "]";
                        slog += "[" + dto.warehousechange.ToString() + "]";
                        slog += "[" + dto.devicelayer.ToString() + "]";
                        slog += "[" + dto.region.ToString() + "]";
                        slog += "[" + dto.prevnode.ToString() + "]";
                        slog += "[" + dto.currentnode.ToString() + "]";
                        slog += "[" + dto.queueprevnode.ToString() + "]";
                        slog += "[" + dto.queuecurrentnode.ToString() + "]";                     
                    }
                    break;
                // 系统日志systemlog
                case "DatabaseManagementLib.CSystemLogDto":
                    {
                        CSystemLogDto dto = (CSystemLogDto)data;
                        slog += "[" + dto.logid + "]";
                        slog += "[" + dto.logdescp + "]";
                        slog += "[" + dto.curtime.ToString() + "]";
                        slog += "[" + dto.optcode + "]";
                        slog += "[" + dto.optname + "]";
                    }
                    break;
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    {
                        CTariffDto dto = (CTariffDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.iccardtype.ToString() + "]";
                        slog += "[" + dto.feetype.ToString() + "]";
                        slog += "[" + dto.tariffdescp + "]";
                        slog += "[" + dto.isworkday.ToString() + "]";
                        slog += "[" + dto.workpeakperiod + "]";
                        slog += "[" + dto.workdayquotafee.ToString() + "]";
                        slog += "[" + dto.workpeakfirstunit.ToString() + "]";
                        slog += "[" + dto.worknonpeakfirstunit.ToString() + "]";
                        slog += "[" + dto.workpeakinunitfee.ToString() + "]";
                        slog += "[" + dto.workpeakoutunitfee.ToString() + "]";
                        slog += "[" + dto.worknonpeakinunitfee.ToString() + "]";
                        slog += "[" + dto.worknonpeakoutunitfee.ToString() + "]";
                        slog += "[" + dto.nonworkpeakperiod + "]";
                        slog += "[" + dto.nonworkdayquotafee.ToString() + "]";
                        slog += "[" + dto.nonworkpeakfirstunit.ToString() + "]";
                        slog += "[" + dto.nonworknonpeakfirstunit.ToString() + "]";
                        slog += "[" + dto.nonworkpeakinunitfee.ToString() + "]";
                        slog += "[" + dto.nonworkpeakoutunitfee.ToString() + "]";
                        slog += "[" + dto.nonworknonpeakinunitfee.ToString() + "]";
                        slog += "[" + dto.nonworknonpeakoutunitfee.ToString() + "]";
                        slog += "[" + dto.fee.ToString() + "]";
                    }
                    break;
                // 设备故障devicefault
                case "DatabaseManagementLib.CDeviceFaultDto":
                    {
                        CDeviceFaultDto dto = (CDeviceFaultDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.faultdescp + "]";
                        slog += "[" + dto.faultaddress + "]";
                        slog += "[" + dto.color.ToString() + "]";
                        slog += "[" + dto.isable.ToString() + "]";
                        slog += "[" + dto.devicecode.ToString() + "]";
                        slog += "[" + dto.warehouse.ToString() + "]";
                    }
                    break;
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    {
                        CSoundDto dto = (CSoundDto)data;
                        slog += "[" + dto.soundcode + "]";
                        slog += "[" + dto.soundname + "]";
                        slog += "[" + dto.soundcontent + "]";
                        slog += "[" + dto.soundishand + "]";
                    }
                    break;
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    {
                        CWorkQueueDto dto = (CWorkQueueDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.devicecode + "]";
                        slog += "[" + dto.warehouse + "]";
                        slog += "[" + dto.iccode + "]";
                        slog += "[" + dto.swipecount + "]";
                        slog += "[" + dto.waitsendtelegram + "]";
                        slog += "[" + dto.priorityid + "]";
                    }
                    break;
                // 故障日志devicefaultlog
                case "DatabaseManagementLib.CDeviceFaultLogDto":
                    {
                        CDeviceFaultLogDto dto = (CDeviceFaultLogDto)data;
                        slog += "[" + dto.id + "]";
                        slog += "[" + dto.devicecode + "]";
                        slog += "[" + dto.warehouse + "]";
                        slog += "[" + dto.faultdescp + "]";
                        slog += "[" + dto.color + "]";
                        slog += "[" + dto.optcode + "]";
                        slog += "[" + dto.time + "]";
                    }
                    break;
                default:
                    slog = data.ToString();
                    break;
            }

            return slog;
        }

        /// <summary>
        /// 写入报文日志表
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        public void InsertTelegramLog(object log, int type)
        {
            try
            {
                if (null == log)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertTelegramLog", "数据表为空");
                    return;
                }
                string strType = "其他日志";
                switch (type)
                {
                    case 0:
                        strType = "报文接收";
                        break;
                    case 1:
                        strType = "报文发送";
                        break;
                    case 2:
                        strType = "语音";
                        break;
                    case 3:
                        strType = "刷卡";
                        break;
                    case 4:
                        strType = "报警位";
                        break;
                    case 5:
                        strType = "状态位";
                        break;
                    case 6:
                        strType = "设备";
                        break;
                    default:
                        strType = "其他日志";
                        break;
                }

                Int16[] data = (Int16[])log;
                StringBuilder sb = new StringBuilder();
                int nWareHouseID = 0;
                for (int i = 0; i < data.Length && i < 5; i++)
                {
                    if (i < 2)
                    {
                        if (0 != data[i])
                        {
                            nWareHouseID = data[i];
                        }
                        continue;
                    }
                    if (i < 4)
                    {
                        sb.Append(data[i] + "|");
                    }
                    else
                    {
                        sb.Append(data[i]);
                    }
                }

                CTelegramLogDto telegramLog = new CTelegramLogDto
                {
                    time = DateTime.Now,
                    logtype = strType,
                    warehouse = nWareHouseID,
                    telegramhead = sb.ToString(),
                    devicecode = data[6],
                    telegramid = data[48]
                };

                if (log.GetType() == typeof(Int16[]))
                {
                    CTelegramParse telegramParser = new CTelegramParse();
                    telegramLog.iccode = telegramParser.Int16sToStringByASCII(data, 11, 22, 4);

                    telegramLog.iccode = data[11].ToString();
                    //外形|轴距—后轮距巷道|偏移量-全车长度|前轮距巷道
                    telegramLog.carinfo = data[23] + "|" + data[25] + "-" + data[28] + "|" + data[24] + "-" + data[45] + "|" + data[46];
                    telegramLog.destdirection = data[28];
                    telegramLog.direction = data[29];
                    telegramLog.srcaddr = data[30] + "边" + data[31] + "列" + data[32] + "层";
                    telegramLog.destaddr = data[35] + "边" + data[36] + "列" + data[37] + "层";
                }
                else if (log.GetType() == typeof(Byte[]))
                {
                    telegramLog.carinfo = data[12].ToString();
                    telegramLog.slotinfo = data[17] + "|" + data[18] + "|" + data[19];
                }

                CSQLAccess.mySQL.InsertOnTable(telegramLog);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertTelegramLog 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return;
            }
        }

        /// <summary>
        /// 获取通信模块类
        /// </summary>
        /// <returns></returns>
        public CCommModule GetCommModule(int nPLCID)
        {
            CCommModule commModule = new CCommModule();

            string strClassName = string.Empty;
            object[] arrPara = null;
            //获取通信模块类型的对应类名及相应参数
            if (!CConfigManagement.myPara.DicCommClassName.ContainsKey(nPLCID) || !CConfigManagement.myPara.DicCommClassPara.ContainsKey(nPLCID))
            {
                return null;
            }
            strClassName = CConfigManagement.myPara.DicCommClassName[nPLCID];//.GetCommModulePara(ref strClassName, ref arrPara);
            arrPara = CConfigManagement.myPara.DicCommClassPara[nPLCID];
            if (string.IsNullOrWhiteSpace(strClassName))
            {
                CLOGException.Trace("GetCommModule", "获取通信模式块失败，请查看配置文件是否正确");
                return null; //获取通信模式块失败，请查看配置文件是否正确
            }

            commModule.SetCommAbstact(strClassName, arrPara);// 读取配置文件
            return commModule;
        }

        /// <summary>
        /// 获取刷卡器对象
        /// </summary>
        /// <returns></returns>
        public CICCardReaderObj GetReaderObj(int nICCardComID)
        {
            CICCardReaderObj readerObj = new CICCardReaderObj(nICCardComID);

            string strClassName = CConfigManagement.myPara.ReaderClassName;
            object[] arrPara = CConfigManagement.myPara.ReaderPara;
            if (string.IsNullOrWhiteSpace(strClassName))
            {
                CLOGException.Trace("GetReaderObj", "获取刷卡器对象失败，请查看配置文件是否正确");
                return null; //获取通信模式块失败，请查看配置文件是否正确
            }

            readerObj.SetParameter(strClassName, arrPara);// 读取配置文件
            return readerObj;
        }

        /// <summary>
        /// 获取刷卡器对象
        /// </summary>
        /// <returns></returns>
        public CICCardReaderObj GetReaderObj(int nHallID, int nICCardComID)
        {
            CICCardReaderObj ReaderObj = new CICCardReaderObj(nICCardComID);

            string strClassName = CConfigManagement.myPara.ReaderClassName;
            object[] arrPara = CConfigManagement.myPara.ReaderPara;
            if (string.IsNullOrWhiteSpace(strClassName))
            {
                CLOGException.Trace("GetReaderObj", "获取刷卡器对象失败，请查看配置文件是否正确");
                return null; //获取通信模式块失败，请查看配置文件是否正确
            }

            ReaderObj.SetParameter(nHallID, strClassName, arrPara);// 读取配置文件
            return ReaderObj;
        }

        /// <summary>
        /// 将IC卡数据块中的数据解析到结构体中
        /// </summary>
        /// <param name="enmCardDataType"></param>
        /// <param name="strDataIn"></param>
        /// <param name="struDataOut"></param>
        public struICCardData ConvertICCardData(string strBlock0, string strBlock1, string strBlock2)
        {
            struICCardData struDataReturn = new struICCardData();
            try
            {
                if (null != strBlock0 && 1 < strBlock0.Length)
                {
                    struDataReturn.enmFeeType = (EnmFeeType)Convert.ToInt32(strBlock0.Substring(0, 1));
                    struDataReturn.strICCardID = strBlock0.Substring(1, strBlock0.Length - 1).Trim();
                }
                if (null != strBlock1)
                {
                    CBaseMethods.MyBase.StringToDateTime(strBlock1, out struDataReturn.dtFeeStartTime, "yyyy/MM/dd HH:mm:ss");
                }
                if (null != strBlock2)
                {
                    CBaseMethods.MyBase.StringToDateTime(strBlock2, out struDataReturn.dtFeeEndTime, "yyyy/MM/dd HH:mm:ss");
                }
                return struDataReturn;
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("ConvertICCardData(string strBlock0, string strBlock1, string strBlock2) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return struDataReturn;
            }
        }

        /// <summary>
        /// 将结构体中的数据转换成IC卡中的数据
        /// </summary>
        /// <param name="enmCardDataType"></param>
        /// <param name="strDataIn"></param>
        /// <param name="struDataOut"></param>
        public void ConvertICCardData(struICCardData struDataIn, out string strBlock0, out string strBlock1, out string strBlock2)
        {
            try
            {
                strBlock0 = (int)struDataIn.enmFeeType + struDataIn.strICCardID;
                CBaseMethods.MyBase.DateTimeToString(struDataIn.dtFeeStartTime, out strBlock1, "yyyy/MM/dd HH:mm:ss");
                CBaseMethods.MyBase.DateTimeToString(struDataIn.dtFeeEndTime, out strBlock2, "yyyy/MM/dd HH:mm:ss");
            }
            catch (System.Exception ex)
            {
                strBlock0 = string.Empty;
                strBlock1 = string.Empty;
                strBlock2 = string.Empty;
                CLOGException.Trace("ConvertICCardData(struICCardData struDataIn, out string strBlock0, out string strBlock1, out string strBlock2) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="obj"></param>
        public bool InsertOnTable(object obj)
        {
            if (obj == null)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertOnTable", "要插入的数据表为空");
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    {
                        return InsertICCardInfo((CICCardDto)obj);
                    }
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    {
                        return InsertOperatorInfo((COperatorDto)obj);
                    }
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    {
                        return InsertCarCustomerInfo((CCarCustomerDto)obj);
                    }
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    {
                        return InsertTariffInfo((CTariffDto)obj);
                    }
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    {
                        return InsertSoundInfo((CSoundDto)obj);
                    }
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    {
                        return InsertWorkQueueInfo((CWorkQueueDto)obj);
                    }
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    {
                        return InsertLedContentInfo((CLedContentDto)obj);
                    }
                // IC卡缴费日志iccardlog
                case "DatabaseManagementLib.CICCardLogDto":
                    {
                        return CSQLAccess.mySQL.InsertOnTable((CICCardLogDto)obj);
                    }
                // 系统日志systemlog
                case "DatabaseManagementLib.CSystemLogDto":
                    {
                        return CSQLAccess.mySQL.InsertOnTable((CSystemLogDto)obj);
                    }
                // 报文日志telegramlog
                case "DatabaseManagementLib.CTelegramLogDto":
                    {
                        return CSQLAccess.mySQL.InsertOnTable((CTelegramLogDto)obj);
                    }
                // 设备状态日志devicestatuslog
                case "DatabaseManagementLib.CDevicesStatusLogDto":
                    {
                        return CSQLAccess.mySQL.InsertOnTable((CDeviceStatusLogDto)obj);
                    }
                // 设备故障日志devicefaultlog
                case "DatabaseManagementLib.CDeviceFaultLogDto":
                    {
                        return CSQLAccess.mySQL.InsertOnTable((CDeviceFaultLogDto)obj);
                    }
                default:
                    {
                        break;
                    }
                //// 车位信息carlocation
                //case "DatabaseManagementLib.CCarLocationDto":
                //    {
                //        return InsertCarLocationInfo((CCarLocationDto)obj);
                //    }
                //// 设备状态devicestatus
                //case "DatabaseManagementLib.CDeviceStatusDto":
                //    {
                //        return InsertDeviceStatusInfo((CDeviceStatusDto)obj);
                //    }
                //// 设备故障devicefault
                //case "DatabaseManagementLib.CDeviceFaultDto":
                //    {
                //        return InsertDeviceFaultInfo((CDeviceFaultDto)obj);
                //    }
            }
            return false;
        }

        /// <summary>
        /// 更新修改数据
        /// </summary>
        /// <param name="obj"></param>
        public bool UpdateTable(object obj)
        {
            if (obj == null)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.updateTable", "要更新的数据表为空");
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    {
                        return UpdateICCardInfo((CICCardDto)obj);
                    }
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    {
                        return UpdateOperatorInfo((COperatorDto)obj);
                    }
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    {
                        return UpdateCarCustomerInfo((CCarCustomerDto)obj);
                    }
                // 车位信息carlocation
                case "DatabaseManagementLib.CCarLocationDto":
                    {
                        return UpdateCarLocationInfo((CCarLocationDto)obj);
                    }
                // 设备状态devicestatus
                case "DatabaseManagementLib.CDeviceStatusDto":
                    {
                        return UpdateDeviceStatusInfo((CDeviceStatusDto)obj);
                    }
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    {
                        return UpdateTariffInfo((CTariffDto)obj);
                    }
                // 设备故障devicefault
                case "DatabaseManagementLib.CDeviceFaultDto":
                    {
                        return UpdateDeviceFaultInfo((CDeviceFaultDto)obj);
                    }
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    {
                        return UpdateSoundTable((CSoundDto)obj);
                    }
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    {
                        return UpdateWorkQueueInfo((CWorkQueueDto)obj);
                    }
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    {
                        return UpdateLedContentInfo((CLedContentDto)obj);
                    }
                default:
                    {
                        break;
                    }
                //// IC卡缴费日志iccardlog
                //case "DatabaseManagementLib.CICCardLogDto":
                //    {
                //        return CSQLAccess.mySQL.updateTable((CICCardLogDto)obj);
                //    }
                //// 系统日志systemlog
                //case "DatabaseManagementLib.CSystemLogDto":
                //    {
                //        return CSQLAccess.mySQL.updateTable((CSystemLogDto)obj);
                //    }
                //// 报文日志telegramlog
                //case "DatabaseManagementLib.CTelegramLogDto":
                //    {
                //        return CSQLAccess.mySQL.updateTable((CTelegramLogDto)obj);
                //    }
                //// 设备状态日志devicestatuslog
                //case "DatabaseManagementLib.CDevicesStatusLogDto":
                //    {
                //        return CSQLAccess.mySQL.updateTable((CDeviceStatusLogDto)obj);
                //    }
                //// 设备故障日志devicefaultlog
                //case "DatabaseManagementLib.CDeviceFaultLogDto":
                //    {
                //        return CSQLAccess.mySQL.updateTable((CDeviceFaultLogDto)obj);
                //    }
            }

            return false;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="obj"></param>
        public bool DeleteOnTable(object obj)
        {
            if (obj == null)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteOnTable", "要删除的数据表为空");
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    {
                        return DeleteICCardInfo((CICCardDto)obj);
                    }
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    {
                        return DeleteOperatorInfo((COperatorDto)obj);
                    }
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    {
                        return DeleteCarCustomerInfo((CCarCustomerDto)obj);
                    }
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    {
                        return DeleteTariffInfo((CTariffDto)obj);
                    }
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    {
                        return DeleteSoundInfo((CSoundDto)obj);
                    }
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    {
                        return DeleteWorkQueueInfo((CWorkQueueDto)obj);
                    }
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    {
                        return DeleteLedContentInfo((CLedContentDto)obj);
                    }
                default:
                    {
                        break;
                    }
                //// 车位信息carlocation
                //case "DatabaseManagementLib.CCarLocationDto":
                //    {
                //        return DeleteCarLocationInfo((CCarLocationDto)obj);
                //    }
                //// 设备状态devicestatus
                //case "DatabaseManagementLib.CDeviceStatusDto":
                //    {
                //        return DeleteDeviceStatusInfo((CDeviceStatusDto)obj);
                //    }
                //// 设备故障devicefault
                //case "DatabaseManagementLib.CDeviceFaultDto":
                //    {
                //        return DeleteDeviceFaultInfo((CDeviceFaultDto)obj);
                //    }
                //// IC卡缴费日志iccardlog
                //case "DatabaseManagementLib.CICCardLogDto":
                //    {
                //        return CSQLAccess.mySQL.deleteOnTable((CICCardLogDto)obj);
                //    }
                //// 系统日志systemlog
                //case "DatabaseManagementLib.CSystemLogDto":
                //    {
                //        return CSQLAccess.mySQL.deleteOnTable((CSystemLogDto)obj);
                //    }
                //// 报文日志telegrammlog
                //case "DatabaseManagementLib.CTelegramLogDto":
                //    {
                //        return CSQLAccess.mySQL.deleteOnTable((CTelegramLogDto)obj);
                //    }
                //// 设备状态日志devicestatuslog
                //case "DatabaseManagementLib.CDevicesStatusLogDto":
                //    {
                //        return CSQLAccess.mySQL.deleteOnTable((CDeviceStatusLogDto)obj);
                //    }
                //// 设备故障日志devicefaultlog
                //case "DatabaseManagementLib.CDeviceFaultLogDto":
                //    {
                //        return CSQLAccess.mySQL.deleteOnTable((CDeviceFaultLogDto)obj);
                //    }

            }

            return false;
        }

        /// <summary>
        /// 客户端操作PLC通信连接
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.InvalidWareHouseID;//无效库区号
        ///       EnmFaultType.Fail; //获取通信模式块失败，请查看配置文件是否正确
        ///       EnmFaultType.FailConnection;//连接PLC通信失败
        /// added by wanxiaona20150728
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public EnmFaultType ClientPLCConnect(ref CCommModule commModule, int nWareHouse)
        {
            commModule = GetCommModule(nWareHouse);
            if (null == commModule)
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.ClientPLCConnect", "获取通信模式块失败");
                return EnmFaultType.Fail; //获取通信模式块失败，请查看配置文件是否正确
            }
            if (!CConfigManagement.myPara.HashAllPLCItemAdress.ContainsKey(nWareHouse))
            {
                return EnmFaultType.InvalidWareHouseID;//无效库区号
            }
            List<string> lstPLCItemAdress = (List<string>)CConfigManagement.myPara.HashAllPLCItemAdress[nWareHouse];//.GetAllPLCItemAdress()[nWareHouse];
            commModule.Initialize(lstPLCItemAdress, false);

            if (!commModule.IsConnected)
            {
                // PLC通信未连接上，重连一次
                commModule.ReConnectServer();
                if (!commModule.IsConnected)
                {// PLC通信还未连接上
                    return EnmFaultType.FailConnection;//连接PLC通信失败
                }
            }

            return EnmFaultType.Success;
        }

        /// <summary>
        /// 获取数据库dataset集-数据
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetSQLDataSet()
        {
            return CBaseMethods.MyBase.CompressDataSet(CSQLAccess.mySQL.GetSQLDataSet());
        }
        #endregion 基本公共函数

        #region 对设备状态表devicestatus的操作
        /// <summary>
        /// 获取设备状态具体实例(根据库号和设备号)
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceCode"></param>
        public CDeviceStatusDto GetDeviceStatus(int nWareHouse, int nDeviceCode)
        {
            //lock (m_objDeviceStatusLocker)
            //{                  
                CDeviceStatusDto tableReturn = null;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if ((table.warehouse == 1) && (table.devicecode == nDeviceCode))
                    {
                        tableReturn = table;
                        break;
                    }
                }
                return tableReturn;
            //}
        }

        /// <summary>
        /// 根据逻辑卡号获取设备状态
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public CDeviceStatusDto GetDeviceStatus(string strICCardID)
        {
            //lock (m_objDeviceStatusLocker)
            //{
                CDeviceStatusDto tableReturn = null;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if (table.iccode == strICCardID)
                    {
                        tableReturn = table;
                        break;
                    }
                }
                return tableReturn;
            //}
        }

        /// <summary>
        /// 获取车厅设备状态具体实例(根据库号和设备地址)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        public CDeviceStatusDto GetDeviceStatus(int nWareHouse, string nDeviceAddr, EnmSMGType enmSMGType)
        {
            //lock (m_objDeviceStatusLocker)
            //{
                CDeviceStatusDto tableReturn = null;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.deviceaddr == nDeviceAddr) && table.devicetype == (int)enmSMGType)
                    {
                        tableReturn = table;
                        break;
                    }
                }
                return tableReturn;
            //}
        }

        /// <summary>
        /// 获取可用的ETV或TV的个数(根据库号)
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <param name="nWareHouse"></param>
        public int GetAvailableETVCount(int nWareHouse)
        {
            //lock (m_objDeviceStatusLocker)
            //{
                int nReturn = 0;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicetype == (int)EnmSMGType.ETV) && (table.devicemode == (int)EnmModel.Automatic))
                    {
                        nReturn++;
                    }
                }
                return nReturn;
            //}
        }
        
        /// <summary>
        /// 获取设备状态具体实例(根据库号、设备号和设备类型)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceCode"></param>
        /// <param name="enmSMGType"></param>
        public CDeviceStatusDto GetDeviceStatus(int nWareHouse, int nDeviceCode, EnmSMGType enmSMGType)
        {
            //lock (m_objDeviceStatusLocker)
            //{
                CDeviceStatusDto tableReturn = null;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nDeviceCode) && table.devicetype == (int)enmSMGType)
                    {
                        tableReturn = table;
                        break;
                    }
                }
                return tableReturn;
            //}
        }

        /// <summary>
        /// 获取设备状态具体实例(根据库号和设备类型)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="enmSMGType"></param>
        public CDeviceStatusDto GetDeviceStatus(int nWareHouse, EnmSMGType enmSMGType)
        {
            //lock (m_objDeviceStatusLocker)
            //{
                CDeviceStatusDto tableReturn = null;
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if ((table.warehouse == nWareHouse) && table.devicetype == (int)enmSMGType)
                    {
                        tableReturn = table;
                        break;
                    }
                }
                return tableReturn;
            //}
        }

        /// <summary>
        /// 初始化设备状态，成功返回true，失败返回false
        /// </summary>
        /// <param name="deviceStatusTable"></param>
        /// <returns></returns>
        public bool InitDeviceStatus(ref CDeviceStatusDto deviceStatusTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == deviceStatusTable)
                {
                    return false;
                }

                lock (m_objDeviceStatusLocker)
                {
                    foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                    {
                        if (table.id == deviceStatusTable.id)
                        {
                            table.iccode = null;
                            table.tasktype = (int)EnmTaskType.Init;
                            // 车厅作业流程节点类型清空
                            table.prevnode = null;
                            table.currentnode = null;
                            table.queueprevnode = null;
                            table.queuecurrentnode = null;

                            table.scrloc = null;
                            table.desccloc = null;

                            deviceStatusTable = table;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(ref CDeviceStatusDto deviceStatusTable) 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.UpdateTable(deviceStatusTable))
                {
                    lock (m_objDeviceStatusLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(ref CDeviceStatusDto deviceStatusTable) 数据库操作失败");
                    return false;//更新数据库失败
                }
                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(deviceStatusTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objDeviceStatusLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(ref CDeviceStatusDto deviceStatusTable) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 设备IC卡所有信息都初始化
        /// </summary>
        /// <param name="strICCardID"></param>
        public List<CDeviceStatusDto> InitDeviceStatus(string strICCardID)
        {
            List<CDeviceStatusDto> lstDeviceStatusTBL = new List<CDeviceStatusDto>();
            try
            {
                lock (m_objDeviceStatusLocker)
                {
                    foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                    {
                        if (table.iccode == strICCardID)
                        {
                            table.iccode = null;
                            table.tasktype = (int)EnmTaskType.Init;
                            // 车厅作业流程节点类型清空
                            table.prevnode = null;
                            table.currentnode = null;
                            table.queueprevnode = null;
                            table.queuecurrentnode = null;

                            table.scrloc = null;
                            table.desccloc = null;

                            lstDeviceStatusTBL.Add(table);
                        }
                    }
                }
                if (0 == lstDeviceStatusTBL.Count)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(string strICCardID) 链表操作失败");
                    return lstDeviceStatusTBL;
                }

                if (!CSQLAccess.mySQL.UpdateTable(lstDeviceStatusTBL))
                {
                    lock (m_objDeviceStatusLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(string strICCardID) 数据库操作失败");
                }
                if (null != m_mainCallback)
                {
                    foreach (CDeviceStatusDto table in lstDeviceStatusTBL)
                    {
                        m_mainCallback.PushMessageToWCF(table);
                    }
                }
                return lstDeviceStatusTBL;
            }
            catch (Exception ex)
            {
                lock (m_objDeviceStatusLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InitDeviceStatus(string strICCardID) 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return lstDeviceStatusTBL;
            }
        }

        /// <summary>
        /// 获取nWareHouse库的所有设备状态列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetDeviceStatusList()
        {
            return m_lstDeviceStatusInfo;
        }

        /// <summary>
        /// 获取nWareHouse库的所有设备状态列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetDeviceStatusList(int nWareHouse)
        {
            List<CDeviceStatusDto> lstDeviceStatusTBL = new List<CDeviceStatusDto>();
            //lock (m_objDeviceStatusLocker)
            //{
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if (table.warehouse == nWareHouse)
                    {
                        lstDeviceStatusTBL.Add(table);
                    }
                }
            //}
            return lstDeviceStatusTBL;
        }

        /// <summary>
        /// 获取nWareHouse库的所有指定设备状态列表
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetDeviceStatusList(int nWareHouse, EnmSMGType enmSMGType)
        {
            List<CDeviceStatusDto> lstDeviceStatusTBL = new List<CDeviceStatusDto>();
            //lock (m_objDeviceStatusLocker)
            //{
                foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                {
                    if (table.warehouse == nWareHouse && table.devicetype == (int)enmSMGType)
                    {
                        lstDeviceStatusTBL.Add(table);
                    }
                }
            //}
            return lstDeviceStatusTBL;
        }

        /// <summary>
        /// 更新修改设备状态devicestatus数据列表（根据关键字索引号id）
        /// </summary>
        /// <param name="deviceStatusTable"></param>
        /// <returns></returns>
        public bool UpdateDeviceStatusInfo(CDeviceStatusDto deviceStatusTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == deviceStatusTable)
                {
                    return false;
                }
                lock (m_objDeviceStatusLocker)
                {
                    foreach (CDeviceStatusDto table in m_lstDeviceStatusInfo)
                    {
                        if (table.id == deviceStatusTable.id)
                        {
                            // 修改设备状态的属性
                            table.iccode = deviceStatusTable.iccode;
                            table.tasktype = deviceStatusTable.tasktype;
                            table.isable = deviceStatusTable.isable;
                            table.isavailable = deviceStatusTable.isavailable;
                            table.devicemode = deviceStatusTable.devicemode;
                            table.deviceaddr = deviceStatusTable.deviceaddr;                           
                            table.halltype = deviceStatusTable.halltype;
                            table.instep = deviceStatusTable.instep;
                            table.outstep = deviceStatusTable.outstep;
                            table.runstep = deviceStatusTable.runstep;
                            table.warehousechange = deviceStatusTable.warehousechange;                         
                            table.prevnode = deviceStatusTable.prevnode;
                            table.currentnode = deviceStatusTable.currentnode;
                            table.queueprevnode = deviceStatusTable.queueprevnode;
                            table.queuecurrentnode = deviceStatusTable.queuecurrentnode;
                            table.scrloc = deviceStatusTable.scrloc;
                            table.desccloc = deviceStatusTable.desccloc;
                            if (table.devicecode > 10) 
                            {
                                table.devicelayer = deviceStatusTable.devicelayer;
                            }
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceStatusInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(deviceStatusTable))
                {
                    lock (m_objDeviceStatusLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceStatusInfo 数据库操作失败");
                    return false;
                }
                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(deviceStatusTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objDeviceStatusLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceStatusInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceStatusInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 依层数获取层中的ETV
        /// </summary>
        /// <param name="warehouse">库区</param>
        /// <param name="layer">层数</param>
        /// <returns></returns>
        public List<CDeviceStatusDto> GetEtvsByLayer(int warehouse, int layer) 
        {
            List<CDeviceStatusDto> devLst = new List<CDeviceStatusDto>();
            foreach (CDeviceStatusDto smg in m_lstDeviceStatusInfo) 
            {
                if (smg.warehouse == warehouse && 
                    smg.devicelayer == layer&&
                    smg.devicetype==(int)EnmSMGType.ETV) 
                {
                    devLst.Add(smg);
                }
            }
            return devLst;
        }

        #endregion 对设备状态表devicestatus的操作

        #region 对IC卡信息表iccard的操作
        /// <summary>
        /// 获取IC卡具体实例(根据IC卡逻辑卡号)
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public CICCardDto GetICCard(string strICCode)
        {
            CICCardDto tableReturn = null;
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.iccode == strICCode)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 根据库号和车位地址,查找固定车位卡信息
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CICCardDto GetICCard(int nWareHouse, string strCarLocAddr)
        {
            CICCardDto tableReturn = null;
            if (0 >= nWareHouse || string.Empty == strCarLocAddr)
            {
                return tableReturn;
            }

            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.carlocaddr == strCarLocAddr))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 根据用户ID获取当前用户IC卡信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public CICCardDto GetICCard(int nUserID)
        {
            CICCardDto tableReturn = null;
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.userid == nUserID)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 获取所有的IC卡信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public List<CICCardDto> GetICCardList()
        {
            //lock (m_objICCardLocker)
            //{
                return m_lstICCardInfo;
            //}
        }

        /// <summary>
        /// 根据用户ID获取当前用户所有的IC卡信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public List<CICCardDto> GetICCardList(int nUserID)
        {
            List<CICCardDto> lstICCardTBL = new List<CICCardDto>();
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.userid == nUserID)
                    {
                        lstICCardTBL.Add(table);
                    }
                }
            //}
            return lstICCardTBL;
        }

        /// <summary>
        /// 根据IC卡号获取当前用户所有的IC卡信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public List<CICCardDto> GetICCardListICCardID(string strICCardID)
        {
            List<CICCardDto> lstICCardTBL = new List<CICCardDto>();
            if (string.Empty == strICCardID)
            {
                return lstICCardTBL;
            }
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.iccode == strICCardID)
                    {
                        lstICCardTBL.Add(table);
                    }
                }
            //}
            return lstICCardTBL;
        }

        /// <summary>
        /// 根据车位地址获取IC卡信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public List<CICCardDto> GetICCardListByCarPOSN(string strCarPOSN)
        {
            List<CICCardDto> lstICCardTBL = new List<CICCardDto>();
            if (string.Empty == strCarPOSN)
            {
                return lstICCardTBL;
            }
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.carlocaddr == strCarPOSN)
                    {
                        lstICCardTBL.Add(table);
                    }
                }
            //}
            return lstICCardTBL;
        }

        /// <summary>
        /// 根据物理卡号获取IC卡信息
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CICCardDto GetICCardByPhysCardID(string strPhysicalCardID)
        {
            CICCardDto tableReturn = null;
            if (string.Empty == strPhysicalCardID)
            {
                return tableReturn;
            }
            foreach (CICCardDto table in m_lstICCardInfo)
            {
                if (table.phycode == strPhysicalCardID)
                {
                    tableReturn = table;
                    break;
                }
            }
            return tableReturn;
        }

        /// <summary>
        /// 获取IC卡具体实例(根据IC卡绑定的车位地址)
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public CICCardDto GetICCardByCarPOSN(string strCarPOSN)
        {
            if (string.IsNullOrWhiteSpace(strCarPOSN))
            {
                return null;
            }
            foreach (CICCardDto table in m_lstICCardInfo)
            {
                if (table.carlocaddr == strCarPOSN)
                {
                    return table;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查IC卡是否制卡、是否注销或挂失、IC卡类型是否正确
        /// 成功：EnmFaultType.Success;
        /// 失败：EnmFaultType.Null;//传入的参数卡号为空
        ///       EnmFaultType.NoICCardInfo;//没有制卡
        ///       EnmFaultType.LossORCancel;//IC卡注销或挂失
        ///       EnmFaultType.NotMatch;//IC卡类型不正确
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public EnmFaultType CheckICCardID(string strICCode)
        {
            if (string.IsNullOrWhiteSpace(strICCode))
            {
                return EnmFaultType.Null;//传入的参数卡号为空
            }

            strICCode = strICCode.Trim();
            CICCardDto ICCardTable = GetICCard(strICCode);
            if (null == ICCardTable)
            {
                return EnmFaultType.NoICCardInfo;//没有制卡
            }
            //注销或挂失
            if ((int)EnmICCardStatus.Lost == ICCardTable.icstatus || (int)EnmICCardStatus.Disposed == ICCardTable.icstatus)
            {
                return EnmFaultType.LossORCancel;//IC卡注销或挂失
            }

            //IC卡类型不正确
            if ((int)EnmICCardType.Temp != ICCardTable.ictype && (int)EnmICCardType.FixedLocation != ICCardTable.ictype
                && (int)EnmICCardType.Fixed != ICCardTable.ictype)
            {
                return EnmFaultType.NotMatch;//IC卡类型不正确
            }
            return EnmFaultType.Success;
        }

        /// <summary>
        /// 初始化IC卡信息，成功返回true，失败返回false
        /// </summary>
        /// <param name="carPOSNTable"></param>
        /// <returns></returns>
        public bool InitICCardInfo(CICCardDto ICCardTable)
        {
            try
            {
                bool bIsHandleDB = false;

                if (null == ICCardTable)
                {
                    return false;
                }
                int? nWareHouse = ICCardTable.warehouse;
                string strCarLoc = ICCardTable.carlocaddr;
                lock (m_objICCardLocker)
                {
                    foreach (CICCardDto table in m_lstICCardInfo)
                    {
                        if (table.id == ICCardTable.id)
                        {
                            table.ictype = (int)EnmICCardType.Temp;
                            table.icstatus = (int)EnmICCardStatus.Normal;
                            table.iclosstime = null;
                            table.iclogouttime = null;
                            table.icstarttime = null;
                            table.icdeadline = null;
                            table.carlocaddr = string.Empty;
                            table.warehouse = null;
                            table.userid = null;
                            table.tariffid = null;
                            ICCardTable = table;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }

                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitICCardInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(ICCardTable))
                {
                    lock (m_objICCardLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitICCardInfo 数据库操作失败");
                    return false;
                }
                // add by wanxiaona20151123
                if (null != m_mainCallback && !CBaseMethods.MyBase.IsEmpty(nWareHouse))
                {
                    CCarLocationDto carLoc = GetCarLocation((int)nWareHouse, strCarLoc);
                    if (null != carLoc)
                    {
                        m_mainCallback.PushMessageToWCF(carLoc);
                    }
                }
                // end
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objICCardLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InitICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取当前剩余定期卡预留车位的个数（可定制的定期卡）
        /// </summary>
        /// <returns></returns>
        public int GetFixReservedSizeCount()
        {
            int nCount = 0;
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.ictype == (int)EnmICCardType.Fixed && table.icstatus == (int)EnmICCardStatus.Normal
                        && !CBaseMethods.MyBase.IsUIntNumWithoutZero(table.carlocaddr))
                    {
                        nCount++;
                        continue;
                    }
                }
            //}
            int nReservedSizeCount = m_nReservedMaxCount - nCount;
            return nReservedSizeCount;
        }

        /// <summary>
        /// 更新修改IC卡iccard数据（根据关键字IC卡号码id）
        /// </summary>
        /// <param name="icCardTable"></param>
        /// <returns></returns>
        public bool UpdateICCardInfo(CICCardDto icCardTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == icCardTable)
                {
                    return false;
                }
                int? nWareHouse = icCardTable.warehouse;
                string strCarLoc = icCardTable.carlocaddr;
                lock (m_objICCardLocker)
                {
                    foreach (CICCardDto table in m_lstICCardInfo)
                    {
                        if (table.id == icCardTable.id)
                        {
                            if (!CBaseMethods.MyBase.IsEmpty(table.warehouse))
                            {
                                if (CBaseMethods.MyBase.IsEmpty(nWareHouse))
                                {
                                    nWareHouse = table.warehouse;
                                    strCarLoc = table.carlocaddr;
                                }
                                else
                                {
                                    nWareHouse = 0;
                                    strCarLoc = string.Empty;
                                }
                            }
                            // 修改IC卡的属性 
                            table.iccode = icCardTable.iccode;
                            table.oldiccode = icCardTable.oldiccode;
                            table.phycode = icCardTable.phycode;
                            table.ictype = icCardTable.ictype;
                            table.icstatus = icCardTable.icstatus;
                            table.icnewtime = icCardTable.icnewtime;
                            table.iclosstime = icCardTable.iclosstime;
                            table.iclogouttime = icCardTable.iclogouttime;
                            table.icstarttime = icCardTable.icstarttime;
                            table.icdeadline = icCardTable.icdeadline;
                            table.carlocaddr = icCardTable.carlocaddr;
                            table.warehouse = icCardTable.warehouse;
                            table.userid = icCardTable.userid;
                            table.tariffid = icCardTable.tariffid;
                            table.priorityid = icCardTable.priorityid;

                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateICCardInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(icCardTable))
                {
                    lock (m_objICCardLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateICCardInfo 数据库操作失败");
                    return false;
                }
                // add by wanxiaona20151123
                if (null != m_mainCallback && !CBaseMethods.MyBase.IsEmpty(nWareHouse))
                {
                    CCarLocationDto carLoc = GetCarLocation((int)nWareHouse, strCarLoc);
                    if (null != carLoc)
                    {
                        m_mainCallback.PushMessageToWCF(carLoc);
                    }
                }
                // end
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objICCardLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 将新IC卡的信息插入表中，
        /// 成功:
        /// EnmFaultType.Success;
        /// 失败:
        ///  EnmFaultType.Null;//传入的物理卡号或逻辑卡号为空
        ///  EnmFaultType.HasICCardInfo;//当前IC卡已经制卡
        ///  EnmFaultType.FailToInsert; //插入数据库失败
        ///  EnmFaultType.ICCardIDNotAllowed;  //输入的逻辑卡号已经存在
        ///  EnmFaultType.ModifyICCardID;   //逻辑卡号不一致确认是否修改逻辑卡号
        /// 插入信息包括：逻辑卡号，物理卡号，IC卡类型(默认临时卡)，制卡时间
        /// </summary>
        /// <param name="table"></param>
        public bool InsertICCardInfo(CICCardDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objICCardLocker)
                {
                    table.id = m_lstICCardInfo.Count + 1;
                    m_lstICCardInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertICCardInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objICCardLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertICCardInfo 数据库操作失败");
                    return false; //插入数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objICCardLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除IC卡信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool DeleteICCardInfo(CICCardDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objICCardLocker)
                {
                    CICCardDto tableTmp = m_lstICCardInfo.Find(s => s.id == table.id);
                    bIsHandleDB = m_lstICCardInfo.Remove(tableTmp);
                    if (bIsHandleDB)
                    {
                        foreach (CICCardDto dto in m_lstICCardInfo)
                        {
                            if (null != dto && dto.id > table.id)
                            {
                                dto.id -= 1;
                            }
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteICCardInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objICCardLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteICCardInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objICCardLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstICCardInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteICCardInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对IC卡信息表iccard的操作

        #region 对车位信息表的操作
        /// <summary>
        /// 获取车位信息具体实例(根据库号和车位地址)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CCarLocationDto GetCarLocation(int nWareHouse, string strCarLocAddr)
        {
            CCarLocationDto tableReturn = null;
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.carlocaddr == strCarLocAddr))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 获取车位信息具体实例(根据库IC卡)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CCarLocationDto GetCarLocation(string strICCode)
        {
            CCarLocationDto tableReturn = null;
            if (string.IsNullOrWhiteSpace(strICCode))
            {
                return tableReturn;
            }

            foreach (CCarLocationDto table in m_lstCarLocationInfo)
            {
                if (table.iccode == strICCode && table.carloctype != (int)EnmLocationType.Hall)
                {
                    tableReturn = table;
                    break;
                }
            }
            return tableReturn;
        }
      
        /// <summary>
        /// 获取车位信息具体实例(根据车牌号)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CCarLocationDto GetCarLocationByLicPlteNum(string strLicPlteNbr)
        {
            CCarLocationDto tableReturn = null;
            if (string.IsNullOrWhiteSpace(strLicPlteNbr))
            {
                return tableReturn;
            }
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if (table.iccode == strLicPlteNbr || table.carnumber == strLicPlteNbr)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 获取车位信息具体实例(根据库获取朝向2边的车位)
        /// add by wanxiaona20150806
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="strCarLocAddr"></param>
        /// <returns></returns>
        public CCarLocationDto GetCarLocation(int nWareHouse)
        {
            CCarLocationDto tableReturn = null;
            //lock (m_objCarLocationLocker)
            //{
                tableReturn = m_lstCarLocationInfo.Find(s => s.warehouse == nWareHouse && s.carloctype == (int)EnmLocationType.Normal && s.carlocstatus == (int)EnmLocationStatus.Occupy && s.direction == 2);
            //}
            return tableReturn;
        }

        /// <summary>
        /// 根据卡号和车位状态获取车位信息链表
        /// </summary>
        /// <param name="strICCode"></param>
        /// <param name="nCarPOSNStatus"></param>
        /// <returns></returns>
        public List<CCarLocationDto> GetCarLocationList(string strICCode, int nCarPOSNStatus)
        {
            List<CCarLocationDto> lstTableReturn = new List<CCarLocationDto>();
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if ((table.iccode == strICCode) && (table.carlocstatus == nCarPOSNStatus))
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 获取车位信息具体实例(根据库区号和车位类型)
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public List<CCarLocationDto> GetCarLocationList(int nWareHouse, int nCarPOSNType)
        {
            List<CCarLocationDto> lstTableReturn = new List<CCarLocationDto>();
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.carloctype == nCarPOSNType))
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 获取车位信息具体实例(根据库区号)
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public List<CCarLocationDto> GetCarLocationList(int nWareHouse)
        {
            List<CCarLocationDto> lstTableReturn = new List<CCarLocationDto>();
            foreach (CCarLocationDto table in m_lstCarLocationInfo)
            {
                if (table.warehouse == nWareHouse)
                {
                    lstTableReturn.Add(table);
                }
            }
            return lstTableReturn;
        }

        /// <summary>
        /// 获取车位信息具体实例(根据库区号、车位类型和车位状态)
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public List<CCarLocationDto> GetCarLocationList(int nWareHouse, int nCarPOSNType, int nCarPOSNStatus)
        {
            List<CCarLocationDto> lstTableReturn = new List<CCarLocationDto>();
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.carloctype == nCarPOSNType) && (table.carlocstatus == nCarPOSNStatus))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 获取所有车位信息链表
        /// </summary>
        /// <param name="strICCode"></param>
        /// <returns></returns>
        public List<CCarLocationDto> GetCarLocationList()
        {
            //lock (m_objCarLocationLocker)
            //{
                return m_lstCarLocationInfo;
            //}
        }

        /// <summary>
        /// 获取空闲车位个数
        /// </summary>
        /// <returns></returns>
        public int GetFreeCarPOSN(int nWareHouse)
        {
            int nCount = 0;            
            foreach (CCarLocationDto table in m_lstCarLocationInfo)
            {
                if ((table.warehouse == nWareHouse) &&
                    (table.carloctype == (int)EnmLocationType.Normal) &&
                    (table.carlocstatus == (int)EnmLocationStatus.Space) &&
                     GetICCard((int)table.warehouse, table.carlocaddr) == null)
                {
                    nCount++;                  
                }
            }           
            return nCount;
        }

        /// <summary>
        /// 获取空闲车位个数
        /// </summary>
        /// <returns></returns>
        public int GetFreeBigCarLoc(int nWareHouse)
        {
            int nCount = 0;
            foreach (CCarLocationDto table in m_lstCarLocationInfo)
            {
                if ((table.warehouse == nWareHouse) &&
                    (table.carloctype == (int)EnmLocationType.Normal) &&
                    (table.carlocstatus == (int)EnmLocationStatus.Space) &&
                     GetICCard((int)table.warehouse, table.carlocaddr) == null&&
                    table.carlocsize == CConfigManagement.myPara.CarMaxSize)
                {
                    nCount++;
                }
            }
            return nCount;
        }

        /// <summary>
        /// 初始化车位，成功返回true，失败返回false
        /// </summary>
        /// <param name="carPOSNTable"></param>
        /// <returns></returns>
        public bool InitCarPOSN(ref CCarLocationDto carPOSNTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == carPOSNTable)
                {
                    return false;
                }
                lock (m_objCarLocationLocker)
                {
                    foreach (CCarLocationDto table in m_lstCarLocationInfo)
                    {
                        if (table.id == carPOSNTable.id)
                        {
                            // 车位信息清空
                            table.carlocstatus = (int)EnmLocationStatus.Space;
                            table.iccode = null;
                            table.carwheelbase = 0;
                            table.direction = 0;
                            table.carsize = "";
                            table.carintime = null;
                            table.carnumber = string.Empty;
                            table.carpicture = string.Empty;
                            table.isbackup = null;

                            table.overallLg = 0;
                            table.overhang = 0;
                            table.offcenter = 0;
                            table.rearwheeldis = 0;
                            table.carweight = 0;

                            carPOSNTable = table;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitCarPOSN 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(carPOSNTable))
                {
                    lock (m_objCarLocationLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InitCarPOSN 数据库操作失败");
                    return false;
                }
                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(carPOSNTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarLocationLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InitCarPOSN 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取当前库的剩余临时车位数(除固定车位和预留尺寸的定期卡车位)
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public int GetTempSpaceCount(int nWareHouse)
        {
            int nCount = 0;
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if (table.warehouse == nWareHouse && table.carlocstatus == (int)EnmLocationStatus.Space &&
                        table.carloctype == (int)EnmLocationType.Normal &&
                        GetICCard((int)table.warehouse, table.carlocaddr) == null)
                    {
                        nCount++;
                        continue;
                    }
                }
            //}
            int nFixCount = GetFixCardCount();
            return nCount - nFixCount;
        }

        /// <summary>
        /// 获取当前库的剩余临时车位数(除固定车位和预留尺寸的定期卡车位),nMaxCarSizeCount为空闲临时卡大车位
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nMaxCarSizeCount"></param>
        /// <returns></returns>
        public int GetTempSpaceCount(int nWareHouse, out int nMaxCarSizeCount)
        {
            int nCount = 0;
            nMaxCarSizeCount = 0;
            //lock (m_objCarLocationLocker)
            //{
                foreach (CCarLocationDto table in m_lstCarLocationInfo)
                {
                    if (table.warehouse == nWareHouse && table.carlocstatus == (int)EnmLocationStatus.Space &&
                        table.carloctype == (int)EnmLocationType.Normal &&
                        GetICCard((int)table.warehouse, table.carlocaddr) == null)
                    {
                        nCount++;
                        if (null != table.carlocsize && table.carlocsize == CConfigManagement.myPara.CarMaxSize)
                        {// 空余大车个数
                            nMaxCarSizeCount++;
                        }
                        continue;
                    } 
                }
            //}
            return nCount;
        }

        /// <summary>
        /// 获取当前库的剩余固定车位数
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <returns></returns>
        public int GetFixedLocationSpaceCount(int nWareHouse)
        {
            int nCount = 0;
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.warehouse == nWareHouse && table.ictype == (int)EnmICCardType.FixedLocation
                        && table.icstatus == (int)EnmICCardStatus.Normal && GetCarLocation(table.iccode) == null)
                    {
                        nCount++;
                        continue;
                    }
                }
            //}
            return nCount;
        }

        /// <summary>
        /// 一键禁用/启用
        /// </summary>
        /// <param name="enmCarPOSNType"></param>
        /// <returns></returns>
        public bool ModifyAllCarPOSNType(int nWareHouse, EnmLocationType enmCarPOSNType, out List<CCarLocationDto> lstCarLocation)
        {
            try
            {
                List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
                lock (m_objCarLocationLocker)
                {
                    foreach (CCarLocationDto table in m_lstCarLocationInfo)
                    {
                        if (table.warehouse == nWareHouse)
                        {
                            if (null == table)
                            {
                                continue; //没有找到指定车位
                            }

                            if (enmCarPOSNType == EnmLocationType.Disable &&
                                (table.carloctype != (int)EnmLocationType.Normal || table.carlocstatus != (int)EnmLocationStatus.Space))
                            {
                                continue;//不允许修改车位类型为禁用车位,车位非空闲正常车位
                            }

                            if (enmCarPOSNType == EnmLocationType.Normal && table.carloctype != (int)EnmLocationType.Disable)
                            {
                                continue;//不允许修改车位类型为正常车位,车位非禁用 车位
                            }

                            table.carloctype = (int)enmCarPOSNType;
                            lstCarLocationTBL.Add(table);
                            continue;
                        }
                    }
                }
                if (0 == lstCarLocationTBL.Count)
                {
                    lstCarLocation = null;
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifyAllCarPOSNType 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.UpdateTable(lstCarLocationTBL))
                {
                    lock (m_objCarLocationLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                    }
                    lstCarLocation = null;
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifyAllCarPOSNType 数据库操作失败");
                    return false;
                }
                lstCarLocation = lstCarLocationTBL;
                if (null != m_mainCallback && 0 < lstCarLocation.Count)
                {
                    m_mainCallback.PushMessageToWCF(lstCarLocation[0]);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarLocationLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                }
                lstCarLocation = null;
                CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifyAllCarPOSNType 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 更新修改车位信息carlocation数据列表（根据关键字索引号id）
        /// </summary>
        /// <param name="carLocationTable"></param>
        /// <returns></returns>
        public bool UpdateCarLocationInfo(CCarLocationDto carLocationTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == carLocationTable)
                {
                    return false;
                }
                lock (m_objCarLocationLocker)
                {
                    foreach (CCarLocationDto table in m_lstCarLocationInfo)
                    {
                        if (table.id == carLocationTable.id)
                        {
                            // 修改车位信息的属性 
                            //table.carlocaddr = carLocationTable.carlocaddr;
                            //table.warehouse = carLocationTable.warehouse;
                            //table.carlocside = carLocationTable.carlocside;
                            //table.carloccolumn = carLocationTable.carloccolumn;
                            //table.carloclayer = carLocationTable.carloclayer;
                            table.carlocstatus = carLocationTable.carlocstatus;
                            table.carloctype = carLocationTable.carloctype;
                            table.carlocsize = carLocationTable.carlocsize;
                            table.iccode = carLocationTable.iccode;
                            table.carwheelbase = carLocationTable.carwheelbase;
                            table.carsize = carLocationTable.carsize;
                            table.isbackup = carLocationTable.isbackup;
                            table.carintime = carLocationTable.carintime;
                            table.priorityid = carLocationTable.priorityid;
                            table.region = carLocationTable.region;
                            table.carnumber = carLocationTable.carnumber;
                            table.carpicture = carLocationTable.carpicture;
                            table.direction = carLocationTable.direction;

                            table.offcenter = carLocationTable.offcenter;
                            table.rearwheeldis = carLocationTable.rearwheeldis;
                            table.carweight = carLocationTable.carweight;

                            table.overallLg = carLocationTable.overallLg;
                            table.overhang = carLocationTable.overhang;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarLocationInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(carLocationTable))
                {
                    lock (m_objCarLocationLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarLocationInfo 数据库操作失败");
                    return false;
                }
                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(carLocationTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarLocationLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarLocationInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarLocationInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 获取某库区的总车位数、空闲、占用、空余大车位值
        /// </summary>
        /// <returns></returns>
        public void GetCarPOSNFreeOccupyCount(int nWareHouse, ref Rectangle rectVehCount)
        {
            rectVehCount = new Rectangle();
            List<CCarLocationDto> lstCarLocationInfo = m_lstCarLocationInfo.FindAll(s => s.warehouse == nWareHouse && (s.carloctype == (int)EnmLocationType.Normal || s.carloctype == (int)EnmLocationType.Disable));
            if (null == lstCarLocationInfo)
            {
                return;
            }

            rectVehCount.X = lstCarLocationInfo.Count;// 总车位数
            foreach (CCarLocationDto table in lstCarLocationInfo)
            {
                if (table.carloctype != (int)EnmLocationType.Normal)
                {
                    continue;
                }

                if (table.carlocstatus == (int)EnmLocationStatus.Occupy)
                {
                    rectVehCount.Y++;// 占用车位数
                }
                else if (table.carlocstatus == (int)EnmLocationStatus.Space)
                {
                    if (!string.IsNullOrEmpty(table.carlocaddr))
                    {
                        if (this.GetICCardByCarPOSN(table.carlocaddr) != null)
                        {
                            continue;
                        }
                    }
                    rectVehCount.Width++;// 空闲车位数
                    if (table.carlocsize == CConfigManagement.myPara.CarMaxSize)
                    {
                        // 空余大车个数
                        rectVehCount.Height++;
                    }
                }
            }
        }
        #endregion 对车位信息表的操作

        #region 对设备故障警报表的操作
        /// <summary>
        /// 获取设备故障列表（根据库号和设备号）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public List<CDeviceFaultDto> GetDeviceFaultList(int nWareHouse, int nEquipID)
        {
            List<CDeviceFaultDto> lstTableReturn = new List<CDeviceFaultDto>();
            //lock (m_objDeviceFaultLocker)
            //{
                foreach (CDeviceFaultDto table in m_lstDeviceFaultInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 获取设备故障列表（获取与指定据库号和设备号相同，且于指定故障显示内容不同的故障信息）
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="strFaultDesc">故障显示内容</param>
        /// <returns>设备故障列表</returns>
        public List<CDeviceFaultDto> GetDeviceFaultList(int nWareHouse, int nEquipID, string strFaultDesc)
        {
            List<CDeviceFaultDto> lstTableReturn = new List<CDeviceFaultDto>();
            //lock (m_objDeviceFaultLocker)
            //{
                foreach (CDeviceFaultDto table in m_lstDeviceFaultInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.faultdescp != strFaultDesc))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 获取设备故障信息（获取与指定据库号和设备号相同，且于指定故障地址的故障信息）
        /// </summary>
        /// <param name="lstDeviceFault">设备故障列表</param>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="strFaultDesc">故障地址</param>
        public CDeviceFaultDto GetDeviceFault(int nWareHouse, int nEquipID, int nAddress)
        {
            CDeviceFaultDto tableReturn = null;
            //lock (m_objDeviceFaultLocker)
            //{
                foreach (CDeviceFaultDto table in m_lstDeviceFaultInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.faultaddress == nAddress))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 更新修改设备故障devicefault数据（根据关键字设备故障索引id）
        /// </summary>
        /// <param name="deviceFaultTable"></param>
        /// <returns></returns>
        public bool UpdateDeviceFaultInfo(CDeviceFaultDto deviceFaultTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == deviceFaultTable)
                {
                    return false;
                }
                lock (m_objDeviceFaultLocker)
                {
                    foreach (CDeviceFaultDto table in m_lstDeviceFaultInfo)
                    {
                        if (table.id == deviceFaultTable.id)
                        {
                            // 修改设备故障的属性 
                            table.faultdescp = deviceFaultTable.faultdescp;
                            table.color = deviceFaultTable.color;
                            table.isable = deviceFaultTable.isable;
                            table.devicecode = deviceFaultTable.devicecode;
                            table.warehouse = deviceFaultTable.warehouse;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceFaultInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(deviceFaultTable))
                {
                    lock (m_objDeviceFaultLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceFaultInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceFaultInfo 数据库操作失败");
                    return false;
                }
                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(deviceFaultTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objDeviceFaultLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstDeviceFaultInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateDeviceFaultInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 判断搬动器上是否有车
        /// 10:搬运器上有车，20：搬运器上无车
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="code"></param>
        /// <returns>10:搬运器上有车，20：搬运器上无车</returns>
        public int JudgeHasCar(int warehouse, int code) 
        {
            List<CDeviceFaultDto> tvFault = m_lstDeviceFaultInfo.FindAll(ff => ff.warehouse == warehouse && ff.devicecode == code);
            #region
            if (tvFault == null || tvFault.Count == 0) 
            {
                return 201; //查找故障表错误
            }
            //搬运器上有车
            CDeviceFaultDto devHas = tvFault.Find(ff => ff.faultaddress == 181);
            if (devHas == null) 
            {
                return 202; //找不到181这个报警状态
            }
            //a夹持到位
            CDeviceFaultDto devA = tvFault.Find(ff => ff.faultaddress == 182);
            if (devA == null)
            {
                return 203; //找不到182这个报警状态
            }
            //a松开到位
            CDeviceFaultDto devA_r = tvFault.Find(ff => ff.faultaddress == 183);
            if (devA_r == null)
            {
                return 204; //找不到183这个报警状态
            }
            //b夹持到位
            CDeviceFaultDto devB= tvFault.Find(ff => ff.faultaddress == 184);
            if (devB == null)
            {
                return 205; //找不到184这个报警状态
            }
            //b松开到位
            CDeviceFaultDto devB_r = tvFault.Find(ff => ff.faultaddress == 185);
            if (devB_r == null)
            {
                return 206; //找不到185这个报警状态
            }

            if (devHas.isable == 1 && devA.isable == 1 && devB.isable == 1) 
            {
                return 10; //搬运器上有车
            }

            if (devHas.isable == 0 && devA_r.isable == 1 && devB_r.isable == 1)
            {
                return 20; //搬运器上无车
            }
            #endregion

            return 200;
        }




        #endregion 对设备故障警报表的操作

        #region 对工作队列信息表的操作

        /// <summary>
        /// 获取所有队列,将有报文内容的清空
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> QueryAllQueueForDisp(int warehouse) 
        {
            List<CWorkQueueDto> allQueue = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto wq in m_lstWorkQueueInfo) 
            {
                if (wq.warehouse == warehouse) 
                {
                    CWorkQueueDto dto = new CWorkQueueDto 
                    { 
                        id=wq.id,
                        warehouse=wq.warehouse,
                        devicecode=wq.devicecode,
                        iccode=wq.iccode,
                        swipecount=wq.swipecount,
                        waitsendtelegram=null,
                        priorityid=wq.priorityid,
                        tasktype=wq.tasktype
                    };
                    allQueue.Add(dto);
                }
            }
            return allQueue;
        }

        /// <summary>
        /// 获取所有队列
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetAllTeleQueue(int warehouse)
        {
            List<CWorkQueueDto> listQueue = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto dt in m_lstWorkQueueInfo)
            {
                if (dt.warehouse == warehouse && dt.tasktype != (int)EnmTaskType.Init)
                {
                    listQueue.Add(dt);
                }
            }
            return listQueue;
        }

        /// <summary>
        /// 获取取车排队队列
        /// </summary>
        /// <param name="nWarehouse">库区</param>
        /// <param name="nEquip">设备号</param>
        /// <returns></returns>
        public List<CWorkQueueDto> TakeCarQueueNoTelegram(int nWarehouse)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if (table.warehouse == nWarehouse && table.waitsendtelegram == null &&
                    (table.tasktype == (int)EnmTaskType.ExitTask || table.tasktype == (int)EnmTaskType.TmpFetch))
                {
                    lstTableReturn.Add(table);
                }
            }
            return lstTableReturn;
        }

        /// <summary>
        /// 获取关于车厅所有的队列，处于下发或已经下发的
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetHallsWorkQueueHasTelegram(int warehouse) 
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if (table.warehouse == warehouse && table.devicecode > 10 &&
                    (table.tasktype == (int)EnmTaskType.ExitTask ||
                     table.tasktype == (int)EnmTaskType.TmpFetch))
                {
                    lstTableReturn.Add(table);
                }
            }
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号和设备号获取当前刷卡器的刷卡信息
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSwipeCardQueue(int nWareHouse, int nEquipID)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号获取当前刷卡器的刷卡信息
        /// add by wanxiaona20150806
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetWorkQueueList(int nWareHouse)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if (table.warehouse == nWareHouse)
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }
        
        /// <summary>
        /// 根据库区号、车厅号及取车刷卡标识获取对应读卡器的取车刷卡队列
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="nSwipeCount">刷卡标识，0：取车，1：第1次存车刷卡，2：第2次存车刷卡，3：第3次存车刷卡</param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSwipeCardQueue(int nWareHouse, int nEquipID, int nSwipeCount)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.swipecount == nSwipeCount))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号、车厅号及IC卡卡号获取对应读卡器的取车刷卡队列
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="nSwipeCount">IC卡卡号</param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSwipeCardQueue(string strICCardID)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if (table.iccode == strICCardID)
                {
                    lstTableReturn.Add(table);
                    continue;
                }
            }
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号、车厅号及取车刷卡标识获取对应读卡器的取车刷卡队列
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="nSwipeCount">刷卡标识，0：取车，1：第1次存车刷卡，2：第2次存车刷卡，3：第3次存车刷卡</param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSwipeCardQueue(int nWareHouse, int nEquipID, int nSwipeCountA, int nSwipeCountB)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID)
                        && (table.swipecount == nSwipeCountA || table.swipecount == nSwipeCountB))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 从队列中找出是该车厅的队列
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="hallID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetWorkQueueByHallCode(int warehouse, int hallID) 
        {
            List<CWorkQueueDto> hallQueue = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto wq in m_lstWorkQueueInfo) 
            {
                if (wq.warehouse == warehouse && wq.devicecode == hallID &&
                    wq.waitsendtelegram == null && wq.tasktype != (int)EnmTaskType.Init) 
                {
                    hallQueue.Add(wq);
                }
            }
            return hallQueue;
        }

        /// <summary>
        /// 根据库区号、车厅号及取车刷卡标识获取对应读卡器的取车刷卡队列
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="nSwipeCount">刷卡标识，0：取车，1：第1次存车刷卡，2：第2次存车刷卡，3：临时取物，4：车体超限，5：没有合适尺寸</param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSwipeCardQueue(int nWareHouse, int nEquipID, int nSwipeCountA, int nSwipeCountB, int nSwipeCountC)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID)
                        && (table.swipecount == nSwipeCountA || table.swipecount == nSwipeCountB || table.swipecount == nSwipeCountC))
                    {
                        lstTableReturn.Add(table);
                        continue;
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号、车厅号及取车刷卡标识获取对应读卡器的取车刷卡信息
        /// </summary>
        /// <param name="nWareHouse">库区号</param>
        /// <param name="nEquipID">设备号</param>
        /// <param name="nSwipeCount">刷卡标识，0：取车，1：第1次存车刷卡，2：第2次存车刷卡，3：第3次存车刷卡</param>
        /// <returns></returns>
        public CWorkQueueDto GetSwipeCardInfo(int nWareHouse, int nEquipID, int nSwipeCount)
        {
            CWorkQueueDto tableReturn = null;
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.swipecount == nSwipeCount))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 根据库区号、设备号和IC卡卡号获取当前IC卡的刷卡信息
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public CWorkQueueDto GetSwipeCardInfo(int nWareHouse, int nEquipID, string strICCardID)
        {
            CWorkQueueDto tableReturn = null;
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.iccode == strICCardID))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nWareHouse">库区</param>
        /// <param name="nEquipID">车厅号</param>
        /// <returns></returns>
        public CWorkQueueDto GetSwipeCardInfoOfExit(int nWareHouse, int nEquipID)
        {
            List<CWorkQueueDto> lstWorkQueue = new List<CWorkQueueDto>();          

            CWorkQueueDto workQueueReturn = null;

            #region 这里也应不执行-priorityid，是不用的
            List<CWorkQueueDto> lstWorkQueueTmp = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID)
                    && (table.swipecount == (int)EnmSwipeCount.GetCar || table.swipecount == (int)EnmSwipeCount.TmpFetch)
                    && (!CBaseMethods.MyBase.IsEmpty(table.priorityid)))
                {
                    lstWorkQueueTmp.Add(table);
                    break;
                }
                lstWorkQueue.Add(table);
            }

            if (null != lstWorkQueueTmp && 0 < lstWorkQueueTmp.Count)
            {                
                var query = from lct in lstWorkQueueTmp
                            orderby lct.priorityid
                            select lct;
                lstWorkQueueTmp = query.ToList();
            }
            else
            {
                lstWorkQueueTmp = lstWorkQueue;
            }
            #endregion

            workQueueReturn = lstWorkQueueTmp.Find(delegate(CWorkQueueDto dto)
            {
                return (dto.warehouse == nWareHouse) && (dto.devicecode == nEquipID)
                    && (dto.swipecount == (int)EnmSwipeCount.GetCar || dto.swipecount == (int)EnmSwipeCount.TmpFetch);
            });
            return workQueueReturn;
        }

        /// <summary>
        /// 获取指定库区的取车队列中第一个取车或临时取物
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public CWorkQueueDto GetSwipeCardInfoOfExit(int nWareHouse)
        {           
            List<CWorkQueueDto> lstWorkQueue = new List<CWorkQueueDto>();         
            List<CWorkQueueDto> lstWorkQueueTmp = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if (table.warehouse == nWareHouse && table.tasktype != (int)EnmTaskType.Init && table.waitsendtelegram == null)
                {
                    if (!CBaseMethods.MyBase.IsEmpty(table.priorityid))
                    {
                        lstWorkQueueTmp.Add(table);                       
                    }
                    else
                    {
                        lstWorkQueue.Add(table);
                    }
                }
            }

            List<CWorkQueueDto> works = new List<CWorkQueueDto>();
            if (0 < lstWorkQueueTmp.Count)
            {
                var query = from lct in lstWorkQueueTmp
                            orderby lct.priorityid
                            select lct;

                works = query.ToList();
            }
            works.AddRange(lstWorkQueue);

            if (works.Count > 0) 
            {
                return works[0];
            }
            return null;
        }

        /// <summary>
        /// 根据库区号、设备号和等待发送的报文获取当前IC卡的发送报文队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public CWorkQueueDto GetSendTelegramInfo(int nWareHouse, int nEquipID, string strWaitTelegram)
        {
            CWorkQueueDto tableReturn = null;
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.waitsendtelegram == strWaitTelegram))
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 获取发送报文队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSendTelegramQueue()
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            //lock (m_objWorkQueueLocker)
            //{
                foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                {
                    if (table.waitsendtelegram != null)
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据库区号和设备号获取当前发送报文队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <returns></returns>
        public List<CWorkQueueDto> GetSendTelegramQueue(int nWareHouse, int nEquipID)
        {
            List<CWorkQueueDto> lstTableReturn = new List<CWorkQueueDto>();
            foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
            {
                if (table.warehouse == nWareHouse && 
                    table.devicecode == nEquipID && 
                    table.waitsendtelegram != null)
                {
                    lstTableReturn.Add(table);
                }
            }
            return lstTableReturn;
        }

        /// <summary>
        /// 删除所有队列
        /// </summary>
        /// <returns></returns>
        public bool DeleteAllWorkQueue() 
        {
            try 
            {
                lock (m_lstWorkQueueInfo)
                {
                    CSQLAccess.mySQL.DeleteAllQueueDto();
                    m_lstWorkQueueInfo.RemoveAll(wk => wk.id > 0);                   
                }
                return true;
            }
            catch (Exception ex) 
            {
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteAllWorkQueue 异常", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除队列信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool DeleteWorkQueueInfo(CWorkQueueDto table)
        {
            lock (m_objWorkQueueLocker)
            {
                try
                {
                    bool bIsHandleDB = false;
                    CWorkQueueDto tableTmp = m_lstWorkQueueInfo.Find(s => s.id == table.id);
                    bIsHandleDB = m_lstWorkQueueInfo.Remove(tableTmp);
                    if (bIsHandleDB)
                    {
                        foreach (CWorkQueueDto dto in m_lstWorkQueueInfo)
                        {
                            if (null != dto && dto.id > table.id)
                            {
                                dto.id -= 1;
                            }
                        }
                    }

                    if (!bIsHandleDB)
                    {
                        CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueInfo(CWorkQueueDto table)链表操作失败");
                        return false;
                    }
                    if (!CSQLAccess.mySQL.DeleteOnTable(table))
                    {

                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                        CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueInfo(CWorkQueueDto table)数据库操作失败");
                        return false;//删除数据库失败
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    lock (m_objWorkQueueLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                    return false;
                }
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
        /// <param name="nWareHouse"></param>
        /// <param name="nEquipID"></param>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public EnmFaultType DeleteWorkQueueInfo(int nWareHouse, int nEquipID, string strICCardID)
        {
            lock (m_objWorkQueueLocker)
            {
                try
                {
                    CWorkQueueDto workQueueTBL = null;
                    foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                    {
                        if ((table.warehouse == nWareHouse) && (table.devicecode == nEquipID) && (table.iccode == strICCardID))
                        {
                            CWorkQueueDto tableTmp = m_lstWorkQueueInfo.Find(s => s.id == table.id);
                            if (m_lstWorkQueueInfo.Remove(tableTmp))
                            {
                                workQueueTBL = table;
                                foreach (CWorkQueueDto dto in m_lstWorkQueueInfo)
                                {
                                    if (null != dto && dto.id > table.id)
                                    {
                                        dto.id -= 1;
                                    }
                                }
                            }
                            break;
                        }
                    }

                    if (null == workQueueTBL)
                    {
                        CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueInfo(int nWareHouse, int nEquipID, string strICCardID) 链表操作失败");
                        return EnmFaultType.NoICCardInfo;//没有刷卡信息
                    }

                    if (!CSQLAccess.mySQL.DeleteOnTable(workQueueTBL))
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                        CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueInfo(int nWareHouse, int nEquipID, string strICCardID) 数据库操作失败");
                        return EnmFaultType.FailToDelete;//删除数据库失败
                    }
                    return EnmFaultType.Success;
                }
                catch (Exception ex)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                    return EnmFaultType.Exception;
                }
            }
        }

        /// <summary>
        /// 删除当前IC卡号在刷卡队列中的所有刷卡信息(包括报文)
        /// </summary>
        /// <param name="strICCardID"></param>
        public List<CWorkQueueDto> DeleteWorkQueueList(string strICCardID)
        {
            List<CWorkQueueDto> lstSwipeCardInfo = new List<CWorkQueueDto>();
            lock (m_objWorkQueueLocker)
            {
                try
                {
                    int nMinID = m_lstWorkQueueInfo.Count;
                    for (int nCounter = 0; nCounter < m_lstWorkQueueInfo.Count; nCounter++)
                    {
                        CWorkQueueDto table = (CWorkQueueDto)m_lstWorkQueueInfo[nCounter];
                        if ((table.iccode == strICCardID))
                        {
                            if (m_lstWorkQueueInfo.Remove(table))
                            {
                                lstSwipeCardInfo.Add(table);
                                nMinID = Math.Min(nMinID, table.id);
                                nCounter--;
                            }
                        }

                        int nTmp = nMinID;
                        // 大于该id的所有行实例的id从nMinID开始计算
                        foreach (CWorkQueueDto dto in m_lstWorkQueueInfo)
                        {
                            if (dto.id > nMinID)
                            {
                                dto.id = nTmp++;
                            }
                        }
                    }
                    if (0 == lstSwipeCardInfo.Count)
                    {
                        return lstSwipeCardInfo;
                    }
                    if (!CSQLAccess.mySQL.DeleteOnTable(lstSwipeCardInfo))
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                        CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteWorkQueueList 数据库操作失败");
                        lstSwipeCardInfo = new List<CWorkQueueDto>();
                    }
                    return lstSwipeCardInfo;
                }
                catch (Exception ex)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteSwipeInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                    lstSwipeCardInfo = new List<CWorkQueueDto>();
                    return lstSwipeCardInfo;
                }
            }
        }

        /// <summary>
        /// 更新修改工作队列workqueue数据（根据关键字索引号id）
        /// </summary>
        /// <param name="workQueueTabel"></param>
        /// <returns></returns>
        public bool UpdateWorkQueueInfo(CWorkQueueDto workQueueTabel)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == workQueueTabel)
                {
                    return false;
                }
                lock (m_objWorkQueueLocker)
                {
                    foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                    {
                        if (table.id == workQueueTabel.id)
                        {
                            // 修改工作队列的属性 
                            table.devicecode = workQueueTabel.devicecode;
                            table.warehouse = workQueueTabel.warehouse;
                            table.iccode = workQueueTabel.iccode;
                            table.swipecount = workQueueTabel.swipecount;
                            table.waitsendtelegram = workQueueTabel.waitsendtelegram;
                            table.priorityid = workQueueTabel.priorityid;
                            table.tasktype = workQueueTabel.tasktype;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateWorkQueueInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(workQueueTabel))
                {
                    lock (m_objWorkQueueLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateWorkQueueInfo 数据库操作失败");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objWorkQueueLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateWorkQueueInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 插入排队信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertWorkQueueInfo(CWorkQueueDto table)
        {
            lock (m_objWorkQueueLocker)
            {
                try
                {
                    table.id = m_lstWorkQueueInfo.Count + 1;
                    m_lstWorkQueueInfo.Add(table);

                    if (!CSQLAccess.mySQL.InsertOnTable(table))
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);

                        CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertWorkQueueInfo 数据库操作失败");
                        return false; //插入数据库失败
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        table.id = m_lstWorkQueueInfo.Count + 100;
                        m_lstWorkQueueInfo.Add(table);
                        CSQLAccess.mySQL.InsertOnTable(table);
                        return true;
                    }
                    catch
                    {
                        lock (m_objWorkQueueLocker)
                        {
                            CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                        }
                        CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertWorkQueueInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// 修改取车次序
        /// </summary>
        /// <param name="strSrcICCode"></param>
        /// <param name="strDestICCode"></param>
        /// <returns></returns>
        public bool ModifWorkQueue(string strSrcICCode, string strDestICCode)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objWorkQueueLocker)
                {
                    if (string.IsNullOrWhiteSpace(strSrcICCode) || string.IsNullOrWhiteSpace(strDestICCode))
                    {
                        return false;
                    }

                    int nSrcID = -1;
                    int nDestID = -1;
                    foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                    {
                        if (table.iccode == strDestICCode)
                        {
                            nDestID = table.id;
                            if (-1 != nSrcID && -1 != nDestID)
                            {
                                break;
                            }
                            bIsHandleDB = true;
                            continue;
                        }
                        if (table.iccode == strSrcICCode)
                        {
                            nSrcID = table.id;
                            if (-1 != nSrcID && -1 != nDestID)
                            {
                                break;
                            }
                            bIsHandleDB = true;
                            continue;
                        }
                    }
                    foreach (CWorkQueueDto table in m_lstWorkQueueInfo)
                    {
                        if (null != table && table.id >= nDestID && table.id < nSrcID)
                        {
                            table.id += 1;
                            bIsHandleDB = true;
                            continue;
                        }
                        if (null != table && table.id == nSrcID)
                        {
                            table.id = nDestID;
                            bIsHandleDB = true;
                            continue;
                        }
                        if (null != table && table.id > nSrcID)
                        {
                            continue;
                        }
                    }
                    var query = from table in m_lstWorkQueueInfo
                                orderby table.id
                                select table;
                    m_lstWorkQueueInfo = query.ToList();

                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifWorkQueue 链表岁操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.ModifWorkQueue(strSrcICCode, strDestICCode))
                {
                    lock (m_objWorkQueueLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifWorkQueue 数据库操作失败");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objWorkQueueLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstWorkQueueInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.ModifWorkQueue 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        /// <summary>
        /// 依ID获取工作队列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CWorkQueueDto GetWorkQueueByID(int id) 
        {
            foreach (CWorkQueueDto wk in m_lstWorkQueueInfo) 
            {
                if (wk.id == id) 
                {
                    return wk;
                }
            }
            return null;
        }
        #endregion 对工作队列信息表的操作

        #region 对操作管理员信息表的操作
        /// <summary>
        /// 获取操作员所有信息列表
        /// </summary>
        /// <param name="lstOperatorDto"></param>
        /// <returns></returns>
        public List<COperatorDto> GetOperatorList()
        {
            //lock (m_objOperatorLocker)
            //{
                return m_lstOperatorInfo;
            //}
        }

        /// <summary>
        /// 根据账号获取操作管理员信息
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassWord"></param>
        /// <returns></returns>
        public COperatorDto GetOperatorInfo(string strUserName)
        {
            COperatorDto tableReturn = null;
            //lock (m_objOperatorLocker)
            //{
                foreach (COperatorDto table in m_lstOperatorInfo)
                {
                    if (table.optcode == strUserName)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 修改操作管理员账号密码
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassWord"></param>
        public void UpdatePassword(ref COperatorDto operatorTBL, string strPassWord)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objOperatorLocker)
                {
                    foreach (COperatorDto table in m_lstOperatorInfo)
                    {
                        if (table.optcode == operatorTBL.optcode)
                        {
                            table.optpassword = strPassWord;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                operatorTBL.optpassword = strPassWord;
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdatePassword 链表操作失败");
                    return;
                }
                if (!CSQLAccess.mySQL.UpdateTable(operatorTBL))
                {
                    lock (m_objOperatorLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdatePassword 数据库操作失败");
                    return;
                }
            }
            catch (Exception ex)
            {
                lock (m_objOperatorLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdatePassword 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 更新修改操作员operator数据（根据关键字操作员号码optcode）
        /// </summary>
        /// <param name="operatorTable"></param>
        /// <returns></returns>
        public bool UpdateOperatorInfo(COperatorDto operatorTable)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == operatorTable)
                {
                    return false;
                }
                lock (m_objOperatorLocker)
                {
                    foreach (COperatorDto table in m_lstOperatorInfo)
                    {
                        if (table.optcode == operatorTable.optcode)
                        {
                            // 修改操作员的属性 
                            table.optname = operatorTable.optname;
                            table.optpassword = operatorTable.optpassword;
                            table.optphone = operatorTable.optphone;
                            table.optaddr = operatorTable.optaddr;
                            table.opttype = operatorTable.opttype;
                            table.optpermission = operatorTable.optpermission;
                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateOperatorInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(operatorTable))
                {
                    lock (m_objOperatorLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateOperatorInfo 数据库操作失败");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objOperatorLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateOperatorInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 插入操作员信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertOperatorInfo(COperatorDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objOperatorLocker)
                {
                    m_lstOperatorInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertOperatorInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objOperatorLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertOperatorInfo 数据库操作失败");
                    return false; //插入数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objOperatorLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertOperatorInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除操作员信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool DeleteOperatorInfo(COperatorDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objOperatorLocker)
                {
                    COperatorDto tableTmp = m_lstOperatorInfo.Find(s => s.optcode == table.optcode);
                    bIsHandleDB = m_lstOperatorInfo.Remove(tableTmp);
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteOperatorInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objOperatorLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteOperatorInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objOperatorLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstOperatorInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteOperatorInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对操作管理员信息表的操作

        #region 对车主信息表的操作
        /// <summary>
        /// 无条件查询所有车主信息，返回所有车主信息列表
        /// </summary>
        /// <param name="lstStruCUSTInfo"></param>
        /// <returns></returns>
        public List<CCarCustomerDto> GetCUSTInfoList()
        {
            //lock (m_objCarCustomerLocker)
            //{
                return m_lstCarCustomerInfo;
            //}
        }

        /// <summary>
        /// 根据用户ID获取车主信息
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public CCarCustomerDto GetCUSTByID(int nUserID)
        {
            CCarCustomerDto tableReturn = null;
            //lock (m_objCarCustomerLocker)
            //{
                foreach (CCarCustomerDto table in m_lstCarCustomerInfo)
                {
                    if (table.userid == nUserID)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 根据用户姓名获取车主信息
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public List<CCarCustomerDto> GetCUSTByName(string strUserName)
        {
            List<CCarCustomerDto> lstTableReturn = new List<CCarCustomerDto>();
            //lock (m_objCarCustomerLocker)
            //{
                foreach (CCarCustomerDto table in m_lstCarCustomerInfo)
                {
                    if (table.username == strUserName)
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据用户移动电话获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        public List<CCarCustomerDto> GetCUSTByMobile(string strMobile)
        {
            List<CCarCustomerDto> lstTableReturn = new List<CCarCustomerDto>();
            //lock (m_objCarCustomerLocker)
            //{
                foreach (CCarCustomerDto table in m_lstCarCustomerInfo)
                {
                    if (table.usermobile == strMobile)
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 根据车牌号获取车主信息
        /// </summary>
        /// <param name="strMobile"></param>
        /// <returns></returns>
        public List<CCarCustomerDto> GetCUSTByLicPlteNum(string strLicPlteNbr)
        {
            List<CCarCustomerDto> lstTableReturn = new List<CCarCustomerDto>();
            //lock (m_objCarCustomerLocker)
            //{
                foreach (CCarCustomerDto table in m_lstCarCustomerInfo)
                {
                    if (table.carnumber == strLicPlteNbr)
                    {
                        lstTableReturn.Add(table);
                    }
                }
            //}
            return lstTableReturn;
        }

        /// <summary>
        /// 更新修改车主信息carcustomer数据（根据关键字用户ID userid）
        /// </summary>
        /// <param name="carCustomerTabel"></param>
        /// <returns></returns>
        public bool UpdateCarCustomerInfo(CCarCustomerDto carCustomerTabel)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == carCustomerTabel)
                {
                    return false;
                }
                lock (m_objCarCustomerLocker)
                {
                    foreach (CCarCustomerDto table in m_lstCarCustomerInfo)
                    {
                        if (table.userid == carCustomerTabel.userid)
                        {
                            // 修改车主信息的属性 
                            table.username = carCustomerTabel.username;
                            table.useraddr = carCustomerTabel.useraddr;
                            table.userphone = carCustomerTabel.userphone;
                            table.usermobile = carCustomerTabel.usermobile;
                            table.carnumber = carCustomerTabel.carnumber;

                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarCustomerInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(carCustomerTabel))
                {
                    lock (m_objCarCustomerLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarCustomerInfo 数据库操作失败");
                    return false;
                }

                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(carCustomerTabel);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarCustomerLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateCarCustomerInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 插入车主信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertCarCustomerInfo(CCarCustomerDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objCarCustomerLocker)
                {
                    m_lstCarCustomerInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertCarCustomerInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objCarCustomerLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertCarCustomerInfo 数据库操作失败");
                    return false; //插入数据库失败
                }

                if (null != m_mainCallback)
                {
                    m_mainCallback.PushMessageToWCF(table);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarCustomerLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertCarCustomerInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除车主信息
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public bool DeleteCarCustomerInfo(CCarCustomerDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objCarCustomerLocker)
                {
                    CCarCustomerDto tableTmp = m_lstCarCustomerInfo.Find(s => s.userid == table.userid);
                    bIsHandleDB = m_lstCarCustomerInfo.Remove(tableTmp);
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteCarCustomerInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objCarCustomerLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteCarCustomerInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objCarCustomerLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstCarCustomerInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteCarCustomerInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对用车主信息表的操作

        #region 对计费信息表的操作
        /// <summary>
        /// 根据ID获取计费信息
        /// </summary>
        /// <param name="nID"></param>
        /// <returns></returns>
        public CTariffDto GetTariffByID(int nID)
        {
            CTariffDto tableReturn = null;
            //lock (m_objTariffLocker)
            //{
                foreach (CTariffDto table in m_lstTariffInfo)
                {
                    if (table.id == nID)
                    {
                        tableReturn = table;
                        break;
                    }
                }
            //}
            return tableReturn;
        }

        /// <summary>
        /// 获取所有收费标准列表
        /// </summary>
        /// <returns></returns>
        public List<CTariffDto> GetTariffList()
        {
            //lock (m_objTariffLocker)
            //{
                return m_lstTariffInfo;
            //}
        }

        /// <summary>
        /// 更新修改计费tariff数据（根据关键字计费ID id）
        /// </summary>
        /// <param name="tariffTabel"></param>
        /// <returns></returns>
        public bool UpdateTariffInfo(CTariffDto tariffTabel)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == tariffTabel)
                {
                    return false;
                }
                lock (m_objTariffLocker)
                {
                    foreach (CTariffDto table in m_lstTariffInfo)
                    {
                        if (table.id == tariffTabel.id)
                        {
                            // 修改计费的属性 
                            table.iccardtype = tariffTabel.iccardtype;
                            table.feetype = tariffTabel.feetype;
                            table.tariffdescp = tariffTabel.tariffdescp;
                            table.isworkday = tariffTabel.isworkday;
                            table.workpeakperiod = tariffTabel.workpeakperiod;
                            table.workdayquotafee = tariffTabel.workdayquotafee;
                            table.workpeakfirstunit = tariffTabel.workpeakfirstunit;
                            table.worknonpeakfirstunit = tariffTabel.worknonpeakfirstunit;
                            table.workpeakinunitfee = tariffTabel.workpeakinunitfee;
                            table.workpeakoutunitfee = tariffTabel.workpeakoutunitfee;
                            table.worknonpeakinunitfee = tariffTabel.worknonpeakinunitfee;
                            table.worknonpeakoutunitfee = tariffTabel.worknonpeakoutunitfee;
                            table.nonworkpeakperiod = tariffTabel.nonworkpeakperiod;
                            table.nonworkdayquotafee = tariffTabel.nonworkdayquotafee;
                            table.nonworkpeakfirstunit = tariffTabel.nonworkpeakfirstunit;
                            table.nonworknonpeakfirstunit = tariffTabel.nonworknonpeakfirstunit;
                            table.nonworkpeakinunitfee = tariffTabel.nonworkpeakinunitfee;
                            table.nonworkpeakoutunitfee = tariffTabel.nonworkpeakoutunitfee;
                            table.nonworknonpeakinunitfee = tariffTabel.nonworknonpeakinunitfee;
                            table.nonworknonpeakoutunitfee = tariffTabel.nonworknonpeakoutunitfee;
                            table.fee = tariffTabel.fee;

                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateTariffInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(tariffTabel))
                {
                    lock (m_objTariffLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateTariffInfo 数据库操作失败");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objTariffLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateTariffInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 插入计费信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertTariffInfo(CTariffDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objTariffLocker)
                {
                    table.id = m_lstTariffInfo.Count + 1;
                    m_lstTariffInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertTariffInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objTariffLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertTariffInfo 数据库操作失败");
                    return false; //插入数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objTariffLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertTariffInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除计费信息
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public bool DeleteTariffInfo(CTariffDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objTariffLocker)
                {
                    if (0 < m_lstTariffInfo.RemoveAll(s => s.id == table.id))
                    {
                        bIsHandleDB = true;
                    }
                    //bIsHandleDB = m_lstTariffInfo.Remove(table);
                    if (bIsHandleDB)
                    {
                        foreach (CTariffDto dto in m_lstTariffInfo)
                        {
                            if (null != dto && dto.id > table.id)
                            {
                                dto.id -= 1;
                            }
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteTariffInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objTariffLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteTariffInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objTariffLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstTariffInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteTariffInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对计费信息表的操作

        #region LED显示内容信息表的操作
        /// <summary>
        /// 根据ID获取LED显示的内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetLEDContent(int id)
        {
            string strReturn = string.Empty;
            //lock (m_objLedContentLocker)
            //{
                foreach (CLedContentDto table in m_lstLedContentInfo)
                {
                    if (table.id == id)
                    {
                        strReturn = table.ledcontent;
                        break;
                    }
                }
            //}
            return strReturn;
        }

        /// <summary>
        /// 获取LED显示内容列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CLedContentDto> GetLEDContentList()
        {
            //lock (m_objLedContentLocker)
            //{
                return m_lstLedContentInfo;
            //}
        }

        /// <summary>
        /// 更新修改LED文本ledcontent数据（根据关键字索引号id）
        /// </summary>
        /// <param name="ledContentTabel"></param>
        /// <returns></returns>
        public bool UpdateLedContentInfo(CLedContentDto ledContentTabel)
        {
            try
            {
                bool bIsHandleDB = false;
                if (null == ledContentTabel)
                {
                    return false;
                }
                lock (m_objLedContentLocker)
                {
                    foreach (CLedContentDto table in m_lstLedContentInfo)
                    {
                        if (table.id == ledContentTabel.id)
                        {
                            // 修改工作队列的属性 
                            table.ledcontent = ledContentTabel.ledcontent;

                            bIsHandleDB = true;
                            break;
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateLedContentInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(ledContentTabel))
                {
                    lock (m_objLedContentLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateLedContentInfo 数据库操作失败");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objLedContentLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateLedContentInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 插入LED信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertLedContentInfo(CLedContentDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objLedContentLocker)
                {
                    table.id = m_lstLedContentInfo.Count + 1;
                    m_lstLedContentInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertLedContentInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objLedContentLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertLedContentInfo 数据库操作失败");
                    return false; //插入数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objLedContentLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertLedContentInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除LED信息
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public bool DeleteLedContentInfo(CLedContentDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objLedContentLocker)
                {
                    CLedContentDto tableTmp = m_lstLedContentInfo.Find(s => s.id == table.id);
                    bIsHandleDB = m_lstLedContentInfo.Remove(tableTmp);
                    if (bIsHandleDB)
                    {
                        foreach (CLedContentDto dto in m_lstLedContentInfo)
                        {
                            if (null != dto && dto.id > table.id)
                            {
                                dto.id -= 1;
                            }
                        }
                    }
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteLedContentInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objLedContentLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteLedContentInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objLedContentLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstLedContentInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteLedContentInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion LED显示内容信息表的操作

        #region 对语音信息的管理
        /// <summary>
        /// 获取所有语音列表
        /// </summary>
        /// <returns></returns>
        public List<CSoundDto> GetSoundList()
        {
            //lock (m_objSoundLocker)
            //{
                return m_lstSoundInfo;
            //}
        }

        /// <summary>
        /// 更新修改语音sound数据（根据关键字声音号码soundcode）
        /// </summary>
        /// <param name="soundTable"></param>
        /// <returns></returns>
        public bool UpdateSoundTable(CSoundDto soundTable)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objSoundLocker)
                {
                    foreach (CSoundDto table in m_lstSoundInfo)
                    {
                        if (table.soundcode == soundTable.soundcode)
                        {
                            // 修改语音的属性 
                            table.soundname = soundTable.soundname;
                            table.soundcontent = soundTable.soundcontent;
                            table.soundishand = soundTable.soundishand;

                            bIsHandleDB = true;
                            break;
                        }
                    }
                }

                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateSoundTable 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.UpdateTable(soundTable))
                {
                    lock (m_objSoundLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateSoundTable 数据库操作失败");
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                lock (m_objSoundLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.UpdateSoundTable 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        
        /// <summary>
        /// 根据语音名称获取“语音内容”属性
        /// </summary>
        /// <param name="strSoundCode"></param>
        /// <returns></returns>
        public string GetSoundContent(string strSoundCode)
        {
            string strReturn = string.Empty;
            //lock (m_objSoundLocker)
            //{
                foreach (CSoundDto table in m_lstSoundInfo)
                {
                    if (table.soundcode == strSoundCode)
                    {
                        strReturn = table.soundcontent;
                        break;
                    }
                }
            //}
            return strReturn;
        }

        /// <summary>
        /// 插入LED信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool InsertSoundInfo(CSoundDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objSoundLocker)
                {
                    m_lstSoundInfo.Add(table);
                    bIsHandleDB = true;
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertSoundInfo 链表操作失败");
                    return false;
                }

                if (!CSQLAccess.mySQL.InsertOnTable(table))
                {
                    lock (m_objSoundLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertSoundInfo 数据库操作失败");
                    return false; //插入数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objSoundLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.InsertSoundInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 删除LED信息
        /// </summary>
        /// <param name="strICCardID"></param>
        /// <returns></returns>
        public bool DeleteSoundInfo(CSoundDto table)
        {
            try
            {
                bool bIsHandleDB = false;
                lock (m_objSoundLocker)
                {
                    CSoundDto tableTmp = m_lstSoundInfo.Find(s => s.soundcode == table.soundcode);
                    bIsHandleDB = m_lstSoundInfo.Remove(tableTmp);
                }
                if (!bIsHandleDB)
                {
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteSoundInfo 链表操作失败");
                    return false;
                }
                if (!CSQLAccess.mySQL.DeleteOnTable(table))
                {
                    lock (m_objSoundLocker)
                    {
                        CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                    }
                    CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteSoundInfo 数据库操作失败");
                    return false;//删除数据库失败
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (m_objSoundLocker)
                {
                    CSQLAccess.mySQL.GetTableDtoList(out m_lstSoundInfo);
                }
                CLOGException.Trace("CommonMethodLib.CCommonMethods.DeleteSoundInfo 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        #endregion 对语音信息的管理

        #region 对日志信息的管理
        /// <summary>
        /// 写设备故障日志
        /// </summary>
        /// <param name="logTBL"></param>
        /// <returns></returns>
        public bool InsertDeviceFaultLog(CDeviceFaultLogDto logTBL)
        {
            return CSQLAccess.mySQL.InsertOnTable(logTBL);
        }
        
        /// <summary>
        /// 写系统日志
        /// </summary>
        /// <param name="systemLogTBL"></param>
        /// <returns></returns>
        public bool InsertSystemLogInfo(CSystemLogDto systemLogTBL)
        {
            return CSQLAccess.mySQL.InsertOnTable(systemLogTBL);
        }
        
        /// <summary>
        /// 写IC卡日志
        /// </summary>
        /// <param name="logTBL"></param>
        /// <returns></returns>
        public bool InsertICCardLogInfo(CICCardLogDto logTBL)
        {
            return CSQLAccess.mySQL.InsertOnTable(logTBL);
        }

        /// <summary>
        /// 获取所有设备故障日志
        /// </summary>
        /// <returns></returns>
        public List<CDeviceFaultLogDto> GetDeviceFaultLogList()
        {
            List<CDeviceFaultLogDto> lstLogDto = new List<CDeviceFaultLogDto>();
            CSQLAccess.mySQL.GetTableDtoList(out lstLogDto);
            return lstLogDto;
        }

        /// <summary>
        /// 获取所有设备状态日志
        /// </summary>
        /// <returns></returns>
        public List<CDeviceStatusLogDto> GetDeviceStatusLogList()
        {
            List<CDeviceStatusLogDto> lstLogDto = new List<CDeviceStatusLogDto>();
            CSQLAccess.mySQL.GetTableDtoList(out lstLogDto);
            return lstLogDto;
        }

        /// <summary>
        /// 获取所有系统日志
        /// </summary>
        /// <returns></returns>
        public List<CSystemLogDto> GetSystemLogList()
        {
            List<CSystemLogDto> lstSystemLog = new List<CSystemLogDto>();
            CSQLAccess.mySQL.GetTableDtoList(out lstSystemLog);
            return lstSystemLog;
        }

        /// <summary>
        /// 获取所有报文日志
        /// </summary>
        /// <returns></returns>
        public List<CTelegramLogDto> GetTelegramLogList()
        {
            List<CTelegramLogDto> lstLogDto = new List<CTelegramLogDto>();
            CSQLAccess.mySQL.GetTableDtoList(out lstLogDto);
            return lstLogDto;
        }
        
        /// <summary>
        /// 获取所有IC卡缴费日志
        /// </summary>
        /// <returns></returns>
        public List<CICCardLogDto> GetICCardLogList()
        {
            List<CICCardLogDto> lstLogDto = new List<CICCardLogDto>();
            CSQLAccess.mySQL.GetTableDtoList(out lstLogDto);
            return lstLogDto;
        }

        /// <summary>
        /// 删除日志信息，删除数据库中所有日志表中时间小于指定时间的所有日志
        /// </summary>
        /// <param name="dtDeadLine"></param>
        public void CleanLogTable(DateTime dtDeadLine)
        {
            CSQLAccess.mySQL.CleanLogTable(dtDeadLine);
        }
        #endregion 对日志信息的管理
        
        #region 私有函数
        /// <summary>
        /// 获取未被使用IC卡的预留车位尺寸的定期卡数目
        /// </summary>
        /// <returns></returns>
        private int GetFixCardCount()
        {
            int nCount = 0;
            //lock (m_objICCardLocker)
            //{
                foreach (CICCardDto table in m_lstICCardInfo)
                {
                    if (table.ictype == (int)EnmICCardType.Fixed && table.icstatus == (int)EnmICCardStatus.Normal
                        && !CBaseMethods.MyBase.IsUIntNumWithoutZero(table.carlocaddr) && GetCarLocation(table.iccode) == null)
                    {
                        nCount++;
                        continue;
                    }
                }
            //}
            return nCount;
        }
        #endregion 私有函数
    }
}
