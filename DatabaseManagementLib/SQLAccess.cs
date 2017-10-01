using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Security.Permissions;
using TypedefLib;
using ConfigManagmtLib;
using System.Data;

namespace DatabaseManagementLib
{

    /// <summary>
    /// 数据库访问类
    /// </summary>
    public sealed class CSQLAccess
    {
        private static readonly CSQLAccess mSQL = new CSQLAccess();
        private object m_objLockICCard = new object();
        private object m_objLockOperator = new object();
        private object m_objLockCustomer = new object();
        private object m_objLockCarLocation = new object();
        private object m_objLockDeviceStatus = new object();
        private object m_objLockDeviceFault = new object();
        private object m_objLockTariff = new object();
        private object m_objLockWorkQueue = new object();
        private object m_objLockICCardLog = new object();
        private object m_objLockSystemLog = new object();
        private object m_objLockTelegramLog = new object();
        private object m_objLockDeviceStatusLog = new object();
        private object m_objLockDeviceFaultLog = new object();

        /// <summary>
        /// 全局静态CSQLAccess成员
        /// </summary>
        public static CSQLAccess mySQL
        {
            get
            {
                return mSQL;
            }
        }

        /// <summary>
        /// 数据库连接信息
        /// </summary>
        public string m_connection;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        private CSQLAccess()
        {
            m_connection = CConfigManagement.myPara.DBConnectionInfo;

            // 要使用的每一个数据库都需要启动Service Broker功能
            using (SqlConnection cn = new SqlConnection(m_connection))
            {
                string sql = CConfigManagement.myPara.SQLBroker;
                
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    // 需要授予一些SQL Server权限以允许非管理员帐户能够参与使用查询通知
                    SqlClientPermission permission = new SqlClientPermission(PermissionState.Unrestricted);
                    permission.Demand();
                }
            }
        }

        /// <summary>
        /// 带SQL数据库用户名字参数的构造函数
        /// </summary>
        /// <param name="connection"></param>
        public CSQLAccess(string connection)
        {
            m_connection = connection;
        }
        
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="obj"></param>
        public bool InsertOnTable(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    return InsertOnTable((CICCardDto)obj);
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    return InsertOnTable((COperatorDto)obj);
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    return InsertOnTable((CCarCustomerDto)obj);
                // 车位信息carlocation
                case "DatabaseManagementLib.CCarLocationDto":
                    return InsertOnTable((CCarLocationDto)obj);
                // 设备状态devicestatus
                case "DatabaseManagementLib.CDeviceStatusDto":
                    return InsertOnTable((CDeviceStatusDto)obj);
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    return InsertOnTable((CTariffDto)obj);
                // 设备故障devicefault
                case "DatabaseManagementLib.CDeviceFaultDto":
                    return InsertOnTable((CDeviceFaultDto)obj);
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    return InsertOnTable((CSoundDto)obj);
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    return InsertOnTable((CWorkQueueDto)obj);
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    return InsertOnTable((CLedContentDto)obj);
                // IC卡缴费日志iccardlog
                case "DatabaseManagementLib.CICCardLogDto":
                    return InsertOnTable((CICCardLogDto)obj);
                // 系统日志systemlog
                case "DatabaseManagementLib.CSystemLogDto":
                    return InsertOnTable((CSystemLogDto)obj);
                // 报文日志telegramlog
                case "DatabaseManagementLib.CTelegramLogDto":
                    return InsertOnTable((CTelegramLogDto)obj);
                // 设备状态日志devicestatuslog
                case "DatabaseManagementLib.CDevicesStatusLogDto":
                    return InsertOnTable((CDeviceStatusLogDto)obj);
                // 设备故障日志devicefaultlog
                case "DatabaseManagementLib.CDeviceFaultLogDto":
                    return InsertOnTable((CDeviceFaultLogDto)obj);
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
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    return UpdateTable((CICCardDto)obj);
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    return UpdateTable((COperatorDto)obj);
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    return UpdateTable((CCarCustomerDto)obj);
                // 车位信息carlocation
                case "DatabaseManagementLib.CCarLocationDto":
                    return UpdateTable((CCarLocationDto)obj);
                // 设备状态devicestatus
                case "DatabaseManagementLib.CDeviceStatusDto":
                    return UpdateTable((CDeviceStatusDto)obj);
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    return UpdateTable((CTariffDto)obj);
                // 设备故障devicefault
                case "DatabaseManagementLib.CDeviceFaultDto":
                    return UpdateTable((CDeviceFaultDto)obj);
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    return UpdateTable((CSoundDto)obj);
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    return UpdateTable((CWorkQueueDto)obj);
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    return UpdateTable((CLedContentDto)obj);
                // IC卡缴费日志iccardlog
                case "DatabaseManagementLib.CICCardLogDto":
                    return UpdateTable((CICCardLogDto)obj);
                // 系统日志systemlog
                case "DatabaseManagementLib.CSystemLogDto":
                    return UpdateTable((CSystemLogDto)obj);
                // 报文日志telegramlog
                case "DatabaseManagementLib.CTelegramLogDto":
                    return UpdateTable((CTelegramLogDto)obj);
                // 设备状态日志devicestatuslog
                case "DatabaseManagementLib.CDevicesStatusLogDto":
                    return UpdateTable((CDeviceStatusLogDto)obj);
                // 设备故障日志devicefaultlog
                case "DatabaseManagementLib.CDeviceFaultLogDto":
                    return UpdateTable((CDeviceFaultLogDto)obj);
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
                return false;
            }

            switch (obj.GetType().ToString())
            {
                // IC卡iccard
                case "DatabaseManagementLib.CICCardDto":
                    return DeleteOnTable((CICCardDto)obj);
                // 操作员operator
                case "DatabaseManagementLib.COperatorDto":
                    return DeleteOnTable((COperatorDto)obj);
                // 车主信息carcustomer
                case "DatabaseManagementLib.CCarCustomerDto":
                    return DeleteOnTable((CCarCustomerDto)obj);
                // 车位信息carlocation
                case "DatabaseManagementLib.CCarLocationDto":
                    return DeleteOnTable((CCarLocationDto)obj);
                // 设备状态devicestatus
                case "DatabaseManagementLib.CDeviceStatusDto":
                    return DeleteOnTable((CDeviceStatusDto)obj);
                // 计费tariff
                case "DatabaseManagementLib.CTariffDto":
                    return DeleteOnTable((CTariffDto)obj);
                // 设备故障devicefault
                case "DatabaseManagementLib.CDeviceFaultDto":
                    return DeleteOnTable((CDeviceFaultDto)obj);
                // 语音sound
                case "DatabaseManagementLib.CSoundDto":
                    return DeleteOnTable((CSoundDto)obj);
                // 工作队列workqueue
                case "DatabaseManagementLib.CWorkQueueDto":
                    return DeleteOnTable((CWorkQueueDto)obj);
                // LED文本ledcontent
                case "DatabaseManagementLib.CLedContentDto)":
                    return DeleteOnTable((CLedContentDto)obj);
                // IC卡缴费日志iccardlog
                case "DatabaseManagementLib.CICCardLogDto":
                    return DeleteOnTable((CICCardLogDto)obj);
                // 系统日志systemlog
                case "DatabaseManagementLib.CSystemLogDto":
                    return DeleteOnTable((CSystemLogDto)obj);
                // 报文日志telegrammlog
                case "DatabaseManagementLib.CTelegramLogDto":
                    return DeleteOnTable((CTelegramLogDto)obj);
                // 设备状态日志devicestatuslog
                case "DatabaseManagementLib.CDevicesStatusLogDto":
                    return DeleteOnTable((CDeviceStatusLogDto)obj);
                // 设备故障日志devicefaultlog
                case "DatabaseManagementLib.CDeviceFaultLogDto":
                    return DeleteOnTable((CDeviceFaultLogDto)obj);
            }

            return false;
        }

        #region 数据列表操作
        /// <summary>
        /// 定时清理数据库日志表
        /// </summary>
        public void CleanLogTable(DateTime dtMinTime)
        {
            // 数据库中记录日志的表需要做定时清理，防止数据过多导致数据库崩溃
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                //string strCmd = "DELETE FROM iccardlog WHERE (paymenttime < '2014/11/24 13:34:20')";
                string strDate = dtMinTime.ToString() + "')";
                string strSQLCom = "DELETE FROM {0} WHERE ({1} < '{2}')\ndeclare @i   int\nselect   @i = 0\nupdate {0} set @i = @i + 1,{3} = @i";
                //"DELETE FROM dbo.iccardlog WHERE (paymenttime < '" + strDate);
                db.ExecuteCommand(string.Format(strSQLCom, "dbo.iccardlog", "paymenttime", dtMinTime.ToString(), "id"));
                // "DELETE FROM dbo.systemlog WHERE (curtime < '" + strDate);
                db.ExecuteCommand(string.Format(strSQLCom, "dbo.systemlog", "curtime", dtMinTime.ToString(), "logid"));
                //"DELETE FROM dbo.telegramlog WHERE (time < '" + strDate);
                db.ExecuteCommand(string.Format(strSQLCom, "dbo.telegramlog", "time", dtMinTime.ToString(), "id"));
                //"DELETE FROM dbo.devicestatuslog WHERE (time < '" + strDate);
                db.ExecuteCommand(string.Format(strSQLCom, "dbo.devicestatuslog", "time", dtMinTime.ToString(), "id"));
                //"DELETE FROM dbo.devicefaultlog WHERE (time < '" + strDate);
                db.ExecuteCommand(string.Format(strSQLCom, "dbo.devicefaultlog", "time", dtMinTime.ToString(), "id"));
             
                #region 注释
                /*
                #region IC卡日志所有行实例的id重新从1编号 
                int nID = 1;
                List<CICCardLogDto> icCardLDtoList = db.CICCardLogDto.ToList<CICCardLogDto>();
                foreach (CICCardLogDto logDto in icCardLDtoList)
                {
                    CICCardLogDto editLog = new CICCardLogDto { };
                    editLog.id = nID++;
                    editLog.iccode = logDto.iccode;
                    editLog.ictype = logDto.ictype;
                    editLog.paymenttime = logDto.paymenttime;
                    editLog.starttime = logDto.starttime;
                    editLog.endtime = logDto.endtime;
                    editLog.calculatedays = logDto.calculatedays;
                    editLog.feetype = logDto.feetype;
                    editLog.tariffnorm = logDto.tariffnorm;
                    editLog.payablefee = logDto.payablefee;
                    editLog.actualfee = logDto.actualfee;
                    editLog.username = logDto.username;
                    editLog.optcode = logDto.optcode;

                    db.CICCardLogDto.InsertOnSubmit(editLog);
                    db.CICCardLogDto.DeleteOnSubmit(logDto);
                }
                #endregion

                #region 系统日志所有行实例的id重新从1编号
                nID = 1;
                List<CSystemLogDto> systemLogDtoList = db.CSystemLogDto.ToList<CSystemLogDto>();
                foreach (CSystemLogDto logDto in systemLogDtoList)
                {
                    CSystemLogDto editLog = new CSystemLogDto { };
                    editLog.logid = nID++;
                    editLog.logdescp = logDto.logdescp;
                    editLog.curtime = logDto.curtime;
                    editLog.optcode = logDto.optcode;
                    editLog.optname = logDto.optname;

                    db.CSystemLogDto.InsertOnSubmit(editLog);
                    db.CSystemLogDto.DeleteOnSubmit(logDto);
                }
                #endregion

                #region 报文日志所有行实例的id重新从1编号
                nID = 1;
                List<CTelegramLogDto> telegramLogDtoList = db.CTelegramLogDto.ToList<CTelegramLogDto>();
                foreach (CTelegramLogDto logDto in telegramLogDtoList)
                {
                    CTelegramLogDto editLog = new CTelegramLogDto { };
                    editLog.id = nID++;
                    editLog.time = logDto.time;
                    editLog.logtype = logDto.logtype;
                    editLog.warehouse = logDto.warehouse;
                    editLog.telegramhead = logDto.telegramhead;
                    editLog.devicecode = logDto.devicecode;
                    editLog.iccode = logDto.iccode;
                    editLog.carinfo = logDto.carinfo;
                    editLog.slotinfo = logDto.slotinfo;
                    editLog.destdirection = logDto.destdirection;
                    editLog.direction = logDto.direction;
                    editLog.srcaddr = logDto.srcaddr;
                    editLog.destaddr = logDto.destaddr;
                    editLog.telegramid = logDto.telegramid;

                    db.CTelegramLogDto.InsertOnSubmit(editLog);
                    db.CTelegramLogDto.DeleteOnSubmit(logDto);
                }
                #endregion

                #region 设备状态日志所有行实例的id重新从1编号
                nID = 1;
                List<CDeviceStatusLogDto> statusLogDtoList = db.CDeviceStatusLogDto.ToList<CDeviceStatusLogDto>();
                foreach (CDeviceStatusLogDto logDto in statusLogDtoList)
                {
                    CDeviceStatusLogDto editLog = new CDeviceStatusLogDto { };
                    editLog.id = nID++;
                    editLog.warehouse = logDto.warehouse;
                    editLog.devicecode = logDto.devicecode;
                    editLog.deviceaddr = logDto.deviceaddr;
                    editLog.time = logDto.time;
                    editLog.devicemode = logDto.devicemode;
                    editLog.devicetype = logDto.devicetype;
                    editLog.halltype = logDto.halltype;
                    editLog.curtasktype = logDto.curtasktype;
                    editLog.isavailable = logDto.isavailable;
                    editLog.isgetoutcar = logDto.isgetoutcar;
                    editLog.instep = logDto.instep;
                    editLog.outstep = logDto.outstep;
                    editLog.isable = logDto.isable;
                    editLog.runstep = logDto.runstep;

                    db.CDeviceStatusLogDto.InsertOnSubmit(editLog);
                    db.CDeviceStatusLogDto.DeleteOnSubmit(logDto);
                }
                #endregion

                #region 报文日志所有行实例的id重新从1编号
                nID = 1;
                List<CDeviceFaultLogDto> faultLogDtoList = db.CDeviceFaultLogDto.ToList<CDeviceFaultLogDto>();
                foreach (CDeviceFaultLogDto logDto in faultLogDtoList)
                {
                    CDeviceFaultLogDto editLog = new CDeviceFaultLogDto { };
                    editLog.id = nID++;
                    editLog.warehouse = logDto.warehouse;
                    editLog.devicecode = logDto.devicecode;
                    editLog.faultdescp = logDto.faultdescp;
                    editLog.time = logDto.time;
                    editLog.optcode = logDto.optcode;
                    editLog.color = logDto.color;

                    db.CDeviceFaultLogDto.InsertOnSubmit(editLog);
                    db.CDeviceFaultLogDto.DeleteOnSubmit(logDto);
                }
                #endregion
                db.SubmitChanges();*/
                #endregion
            }
        }

        /// <summary>
        /// 获取数据库dataset集-数据
        /// </summary>
        public DataSet GetSQLDataSet()
        {
            using (SqlConnection conn = new SqlConnection(m_connection))
            {
                DataSet ds = new DataSet();//新建一个dataset集，用ds表示
                // 打开数据库连接
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                string strProjectName = CConfigManagement.myPara.ProjectName;
                string selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[carcustomer]", strProjectName);
                // 使用sqldataadapter读取数据库中表carcustomer的数据
                SqlDataAdapter da = new SqlDataAdapter(selectCommandText, conn);
                da.Fill(ds, "carcustomer");
                selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[carlocation]", strProjectName);
                // 使用sqldataadapter读取数据库中表carlocation的数据
                da = new SqlDataAdapter(selectCommandText, conn);
                da.Fill(ds, "carlocation");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[devicefault]", strProjectName);
                //// 使用sqldataadapter读取数据库中表devicefault的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "devicefault");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[devicefaultlog]", strProjectName);
                //// 使用sqldataadapter读取数据库中表devicefaultlog的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "devicefaultlog");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[devicestatus]", strProjectName);
                //// 使用sqldataadapter读取数据库中表devicestatus的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "devicestatus");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[devicestatuslog]", strProjectName);
                //// 使用sqldataadapter读取数据库中表devicestatuslog的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "devicestatuslog");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[iccard]", strProjectName);
                //// 使用sqldataadapter读取数据库中表iccard的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "iccard");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[iccardlog]", strProjectName);
                //// 使用sqldataadapter读取数据库中表iccardlog的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "iccardlog");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[ledcontent]", strProjectName);
                //// 使用sqldataadapter读取数据库中表ledcontent的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "ledcontent");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[operator]", strProjectName);
                //// 使用sqldataadapter读取数据库中表operator的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "operator");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[sound]", strProjectName);
                //// 使用sqldataadapter读取数据库中表sound的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "sound");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[systemlog]", strProjectName);
                //// 使用sqldataadapter读取数据库中表systemlog的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "systemlog");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[tariff]", strProjectName);
                //// 使用sqldataadapter读取数据库中表tariff的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "tariff");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[telegramlog]", strProjectName);
                //// 使用sqldataadapter读取数据库中表telegramlog的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "telegramlog");
                //selectCommandText = string.Format("SELECT * FROM [{0}].[dbo].[workqueue]", strProjectName);
                //// 使用sqldataadapter读取数据库中表workqueue的数据
                //da = new SqlDataAdapter(selectCommandText, conn);
                //da.Fill(ds, "workqueue");
                return ds;
            }
        }

        /// <summary>
        /// 插入车主信息carcustomer数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CCarCustomerDto> InsertOnTableCarCustomer(DataTable dt)
        {
            // 车主信息carcustomer
            List<CCarCustomerDto> lstCarCustomerDto = new List<CCarCustomerDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CCarCustomerDto carCustomerDto = new CCarCustomerDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "carnumber":
                            {
                                carCustomerDto.carnumber = dstr[dc].ToString();
                                break;
                            }
                        case "useraddr":
                            {
                                carCustomerDto.useraddr = dstr[dc].ToString();
                                break;
                            }
                        case "userid":
                            {
                                carCustomerDto.userid = (int)dstr[dc];
                                break;
                            }
                        case "usermobile":
                            {
                                carCustomerDto.usermobile = dstr[dc].ToString();
                                break;
                            }
                        case "username":
                            {
                                carCustomerDto.username = dstr[dc].ToString();
                                break;
                            }
                        case "userphone":
                            {
                                carCustomerDto.userphone = dstr[dc].ToString();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstCarCustomerDto.Add(carCustomerDto);
            }

            lock (m_objLockCustomer)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach (CCarCustomerDto carCustomerDto in lstCarCustomerDto)
                    {
                        // 取出车主信息 
                        var editCarCustomer = db.CCarCustomerDto.SingleOrDefault<CCarCustomerDto>(s => s.userid == carCustomerDto.userid);

                        if (null == editCarCustomer)
                        {
                            db.CCarCustomerDto.InsertOnSubmit(carCustomerDto);
                            continue;
                        }

                        // 修改车主信息的属性 
                        editCarCustomer.username = carCustomerDto.username;
                        editCarCustomer.useraddr = carCustomerDto.useraddr;
                        editCarCustomer.userphone = carCustomerDto.userphone;
                        editCarCustomer.usermobile = carCustomerDto.usermobile;
                        editCarCustomer.carnumber = carCustomerDto.carnumber;
                    }
                    db.SubmitChanges();
                    return lstCarCustomerDto;
                }
            }
        }

        /// <summary>
        /// 插入车位信息carlocation数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CCarLocationDto> InsertOnTableCarLocation(DataTable dt)
        {
            // 车位信息carlocation
            List<CCarLocationDto> lstCarLocationDto = new List<CCarLocationDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CCarLocationDto carLocationDto = new CCarLocationDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "carintime":
                            {
                                carLocationDto.carintime = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "carlocaddr":
                            {
                                carLocationDto.carlocaddr = dstr[dc].ToString();
                                break;
                            }
                        case "carloccolumn":
                            {
                                carLocationDto.carloccolumn = (int)dstr[dc];
                                break;
                            }
                        case "carloclayer":
                            {
                                carLocationDto.carloclayer = (int)dstr[dc];
                                break;
                            }
                        case "carlocside":
                            {
                                carLocationDto.carlocside = (int)dstr[dc];
                                break;
                            }
                        case "carlocsize":
                            {
                                carLocationDto.carlocsize = dstr[dc].ToString();
                                break;
                            }
                        case "carlocstatus":
                            {
                                carLocationDto.carlocstatus = (int)dstr[dc];
                                break;
                            }
                        case "carloctype":
                            {
                                carLocationDto.carloctype = (int)dstr[dc];
                                break;
                            }
                        case "carnumber":
                            {
                                carLocationDto.carnumber = dstr[dc].ToString();
                                break;
                            }
                        case "carpicture":
                            {
                                carLocationDto.carpicture = dstr[dc].ToString();
                                break;
                            }
                        case "carsize":
                            {
                                carLocationDto.carsize = dstr[dc].ToString();
                                break;
                            }
                        case "carwheelbase":
                            {
                                carLocationDto.carwheelbase = (int)dstr[dc];
                                break;
                            }
                        case "direction":
                            {
                                carLocationDto.direction = (int)dstr[dc];
                                break;
                            }
                        case "iccode":
                            {
                                carLocationDto.iccode = dstr[dc].ToString();
                                break;
                            }
                        case "id":
                            {
                                carLocationDto.id = (int)dstr[dc];
                                break;
                            }
                        case "isbackup":
                            {
                                carLocationDto.isbackup = (int)dstr[dc];
                                break;
                            }
                        case "priorityid":
                            {
                                carLocationDto.priorityid = (int)dstr[dc];
                                break;
                            }
                        case "region":
                            {
                                carLocationDto.region = (int)dstr[dc];
                                break;
                            }
                        case "warehouse":
                            {
                                carLocationDto.warehouse = (int)dstr[dc];
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstCarLocationDto.Add(carLocationDto);
            }

            lock (m_objLockCarLocation)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach(CCarLocationDto carLocationDto in lstCarLocationDto)
                    {
                        // 取出车位信息 
                        var editCarLocation = db.CCarLocationDto.SingleOrDefault<CCarLocationDto>(s => s.id == carLocationDto.id);

                        if (null == editCarLocation)
                        {
                            db.CCarLocationDto.InsertOnSubmit(carLocationDto);
                            continue;
                        }

                        // 修改车位信息的属性 
                        editCarLocation.carlocaddr = carLocationDto.carlocaddr;
                        editCarLocation.warehouse = carLocationDto.warehouse;
                        editCarLocation.carlocside = carLocationDto.carlocside;
                        editCarLocation.carloccolumn = carLocationDto.carloccolumn;
                        editCarLocation.carloclayer = carLocationDto.carloclayer;
                        editCarLocation.carlocstatus = carLocationDto.carlocstatus;
                        editCarLocation.carloctype = carLocationDto.carloctype;
                        editCarLocation.carlocsize = carLocationDto.carlocsize;
                        editCarLocation.iccode = carLocationDto.iccode;
                        editCarLocation.carwheelbase = carLocationDto.carwheelbase;
                        editCarLocation.carsize = carLocationDto.carsize;
                        editCarLocation.isbackup = carLocationDto.isbackup;
                        editCarLocation.carintime = carLocationDto.carintime;
                        editCarLocation.priorityid = carLocationDto.priorityid;
                        editCarLocation.region = carLocationDto.region;
                        editCarLocation.carnumber = carLocationDto.carnumber;
                        editCarLocation.carpicture = carLocationDto.carpicture;
                        editCarLocation.direction = carLocationDto.direction;
                    }
                    db.SubmitChanges();
                    return lstCarLocationDto;
                }
            }
        }

        /// <summary>
        /// 插入设备故障devicefault数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CDeviceFaultDto> InsertOnTableDeviceFault(DataTable dt)
        {
            // 设备故障devicefault
            List<CDeviceFaultDto> lstDeviceFaultDto = new List<CDeviceFaultDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CDeviceFaultDto deviceFaultDto = new CDeviceFaultDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "color":
                            {
                                deviceFaultDto.color = (int)dstr[dc];
                                break;
                            }
                        case "devicecode":
                            {
                                deviceFaultDto.devicecode = (int)dstr[dc];
                                break;
                            }
                        case "faultaddress":
                            {
                                deviceFaultDto.faultaddress = (int)dstr[dc];
                                break;
                            }
                        case "faultdescp":
                            {
                                deviceFaultDto.faultdescp = dstr[dc].ToString();
                                break;
                            }
                        case "id":
                            {
                                deviceFaultDto.id = (int)dstr[dc];
                                break;
                            }
                        case "isable":
                            {
                                deviceFaultDto.isable = (int)dstr[dc];
                                break;
                            }
                        case "warehouse":
                            {
                                deviceFaultDto.warehouse = (int)dstr[dc];
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstDeviceFaultDto.Add(deviceFaultDto);
            }

            lock (m_objLockDeviceFault)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    foreach (CDeviceFaultDto deviceFaultDto in lstDeviceFaultDto)
                    {// 取出设备故障
                        var editDeviceFault = db.CDeviceFaultDto.SingleOrDefault<CDeviceFaultDto>(s => s.id == deviceFaultDto.id);

                        if (null == editDeviceFault)
                        {
                            db.CDeviceFaultDto.InsertOnSubmit(deviceFaultDto);
                            continue;
                        }

                        // 修改设备故障的属性 
                        editDeviceFault.faultdescp = deviceFaultDto.faultdescp;
                        editDeviceFault.color = deviceFaultDto.color;
                        editDeviceFault.isable = deviceFaultDto.isable;
                        editDeviceFault.devicecode = deviceFaultDto.devicecode;
                        editDeviceFault.warehouse = deviceFaultDto.warehouse;
                    }
                    db.SubmitChanges();
                    return lstDeviceFaultDto;
                }
            }
        }

        /// <summary>
        /// 插入设备状态devicestatus数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CDeviceStatusDto> InsertOnTableDeviceStatus(DataTable dt)
        {
            // 设备状态devicestatus
            List<CDeviceStatusDto> lstDeviceStatusDto = new List<CDeviceStatusDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CDeviceStatusDto deviceStatusDto = new CDeviceStatusDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "currentnode":
                            {
                                deviceStatusDto.currentnode = (int)dstr[dc];
                                break;
                            }
                        case "deviceaddr":
                            {
                                deviceStatusDto.deviceaddr = dstr[dc].ToString();
                                break;
                            }
                        case "devicecode":
                            {
                                deviceStatusDto.devicecode = (int)dstr[dc];
                                break;
                            }
                        case "devicelayer":
                            {
                                deviceStatusDto.devicelayer = (int)dstr[dc];
                                break;
                            }
                        case "devicemode":
                            {
                                deviceStatusDto.devicemode = (int)dstr[dc];
                                break;
                            }
                        case "devicetype":
                            {
                                deviceStatusDto.devicetype = (int)dstr[dc];
                                break;
                            }
                        case "halltype":
                            {
                                deviceStatusDto.halltype = (int)dstr[dc];
                                break;
                            }
                        case "iccode":
                            {
                                deviceStatusDto.iccode = dstr[dc].ToString();
                                break;
                            }
                        case "id":
                            {
                                deviceStatusDto.id = (int)dstr[dc];
                                break;
                            }
                        case "instep":
                            {
                                deviceStatusDto.instep = (int)dstr[dc];
                                break;
                            }
                        case "isable":
                            {
                                deviceStatusDto.isable = (int)dstr[dc];
                                break;
                            }
                        case "isavailable":
                            {
                                deviceStatusDto.isavailable = (int)dstr[dc];
                                break;
                            }
                        case "outstep":
                            {
                                deviceStatusDto.outstep = (int)dstr[dc];
                                break;
                            }
                        case "prevnode":
                            {
                                deviceStatusDto.prevnode = (int)dstr[dc];
                                break;
                            }
                        case "queuecurrentnode":
                            {
                                deviceStatusDto.queuecurrentnode = (int)dstr[dc];
                                break;
                            }
                        case "queueprevnode":
                            {
                                deviceStatusDto.queueprevnode = (int)dstr[dc];
                                break;
                            }
                        case "region":
                            {
                                deviceStatusDto.region = (int)dstr[dc];
                                break;
                            }
                        case "runstep":
                            {
                                deviceStatusDto.runstep = (int)dstr[dc];
                                break;
                            }
                        case "tasktype":
                            {
                                deviceStatusDto.tasktype = (int)dstr[dc];
                                break;
                            }
                        case "warehouse":
                            {
                                deviceStatusDto.warehouse = (int)dstr[dc];
                                break;
                            }
                        case "warehousechange":
                            {
                                deviceStatusDto.warehousechange = (int)dstr[dc];
                                break;
                            }                       
                        default:
                            {
                                break;
                            }
                    }
                }
                lstDeviceStatusDto.Add(deviceStatusDto);
            }

            lock (m_objLockDeviceStatus)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach (CDeviceStatusDto deviceStatusDto in lstDeviceStatusDto)
                    { // 取出设备状态
                        var editDeviceStatus = db.CDeviceStatusDto.SingleOrDefault<CDeviceStatusDto>(s => s.id == deviceStatusDto.id);

                        if (null == editDeviceStatus)
                        {
                            db.CDeviceStatusDto.InsertOnSubmit(deviceStatusDto);
                            continue;
                        }
                        
                        // 修改设备状态的属性 
                        editDeviceStatus.devicecode = deviceStatusDto.devicecode;
                        editDeviceStatus.warehouse = deviceStatusDto.warehouse;
                        editDeviceStatus.iccode = deviceStatusDto.iccode;
                        editDeviceStatus.tasktype = deviceStatusDto.tasktype;
                        editDeviceStatus.isable = deviceStatusDto.isable;
                        editDeviceStatus.isavailable = deviceStatusDto.isavailable;
                        editDeviceStatus.devicemode = deviceStatusDto.devicemode;
                        editDeviceStatus.deviceaddr = deviceStatusDto.deviceaddr;
                        editDeviceStatus.devicetype = deviceStatusDto.devicetype;
                        editDeviceStatus.halltype = deviceStatusDto.halltype;
                        editDeviceStatus.instep = deviceStatusDto.instep;
                        editDeviceStatus.outstep = deviceStatusDto.outstep;
                        editDeviceStatus.runstep = deviceStatusDto.runstep;
                        editDeviceStatus.warehousechange = deviceStatusDto.warehousechange;
                        editDeviceStatus.devicelayer = deviceStatusDto.devicelayer;
                        editDeviceStatus.region = deviceStatusDto.region;
                        editDeviceStatus.prevnode = deviceStatusDto.prevnode;
                        editDeviceStatus.currentnode = deviceStatusDto.currentnode;
                        editDeviceStatus.queueprevnode = deviceStatusDto.queueprevnode;
                        editDeviceStatus.queuecurrentnode = deviceStatusDto.queuecurrentnode;                       
                    }
                    db.SubmitChanges();
                    return lstDeviceStatusDto;
                }
            }
        }

        /// <summary>
        /// 插入IC卡iccard数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CICCardDto> InsertOnTableICCard(DataTable dt)
        {
            // IC卡iccard
            List<CICCardDto> lstICCardDto = new List<CICCardDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CICCardDto icCardDto = new CICCardDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "iccode":
                            {
                                icCardDto.iccode = dstr[dc].ToString();
                                break;
                            }
                        case "carlocaddr":
                            {
                                icCardDto.carlocaddr = dstr[dc].ToString();
                                break;
                            }
                        case "icdeadline":
                            {
                                icCardDto.icdeadline = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "iclogouttime":
                            {
                                icCardDto.iclogouttime = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "iclosstime":
                            {
                                icCardDto.iclosstime = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "icnewtime":
                            {
                                icCardDto.icnewtime = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "icstarttime":
                            {
                                icCardDto.icstarttime = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "icstatus":
                            {
                                icCardDto.icstatus = (int)dstr[dc];
                                break;
                            }
                        case "ictype":
                            {
                                icCardDto.ictype = (int)dstr[dc];
                                break;
                            }
                        case "id":
                            {
                                icCardDto.id = (int)dstr[dc];
                                break;
                            }
                        case "oldiccode":
                            {
                                icCardDto.oldiccode = dstr[dc].ToString();
                                break;
                            }
                        case "phycode":
                            {
                                icCardDto.phycode = dstr[dc].ToString();
                                break;
                            }
                        case "priorityid":
                            {
                                icCardDto.priorityid = (int)dstr[dc];
                                break;
                            }
                        case "tariffid":
                            {
                                icCardDto.tariffid = (int)dstr[dc];
                                break;
                            }
                        case "userid":
                            {
                                icCardDto.userid = (int)dstr[dc];
                                break;
                            }
                        case "warehouse":
                            {
                                icCardDto.warehouse = (int)dstr[dc];
                                break;
                            }
                        default:
                            {
                                break;
                            }

                    }
                }
                lstICCardDto.Add(icCardDto);
            }

            lock (m_objLockICCard)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach(CICCardDto icCardDto in lstICCardDto)
                    {
                        var editICCard = db.CICCardDto.SingleOrDefault<CICCardDto>(s => s.id == icCardDto.id);

                        if (null == editICCard)
                        {
                            db.CICCardDto.InsertOnSubmit(icCardDto);
                            continue;
                        }

                        // 修改IC卡的属性 
                        editICCard.iccode = icCardDto.iccode;
                        editICCard.oldiccode = icCardDto.oldiccode;
                        editICCard.phycode = icCardDto.phycode;
                        editICCard.ictype = icCardDto.ictype;
                        editICCard.icstatus = icCardDto.icstatus;
                        editICCard.icnewtime = icCardDto.icnewtime;
                        editICCard.iclosstime = icCardDto.iclosstime;
                        editICCard.iclogouttime = icCardDto.iclogouttime;
                        editICCard.icstarttime = icCardDto.icstarttime;
                        editICCard.icdeadline = icCardDto.icdeadline;
                        editICCard.carlocaddr = icCardDto.carlocaddr;
                        editICCard.warehouse = icCardDto.warehouse;
                        editICCard.userid = icCardDto.userid;
                        editICCard.tariffid = icCardDto.tariffid;
                        editICCard.priorityid = icCardDto.priorityid;
                    }
                    db.SubmitChanges();
                    return lstICCardDto;
                }
            }
        }

        /// <summary>
        /// 插入LED内容ledcontent数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CLedContentDto> InsertOnTableLedContent(DataTable dt)
        {
            // LED内容ledcontent
            List<CLedContentDto> lstLedContentDto = new List<CLedContentDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CLedContentDto ledContentDto = new CLedContentDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "id":
                            {
                                ledContentDto.id = (int)dstr[dc];
                                break;
                            }
                        case "ledcontent":
                            {
                                ledContentDto.ledcontent = dstr[dc].ToString();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstLedContentDto.Add(ledContentDto);
            }
            
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                foreach (CLedContentDto ledContentDto in lstLedContentDto)
                {
                    // 取出工作队列实例
                    var editLedContent = db.CLedContentDto.SingleOrDefault<CLedContentDto>(s => s.id == ledContentDto.id);

                    if (null == editLedContent)
                    {
                        db.CLedContentDto.InsertOnSubmit(ledContentDto);
                        continue;
                    }

                    // 修改工作队列的属性 
                    editLedContent.ledcontent = ledContentDto.ledcontent;
                }
                db.SubmitChanges();
                return lstLedContentDto;
            }
        }

        /// <summary>
        /// 插入操作员operator数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<COperatorDto> InsertOnTableOperator(DataTable dt)
        {
            // 操作员operator
            List<COperatorDto> lstOperatorDto = new List<COperatorDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                COperatorDto operatorDto = new COperatorDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "optaddr":
                            {
                                operatorDto.optaddr = dstr[dc].ToString();
                                break;
                            }
                        case "optcode":
                            {
                                operatorDto.optcode = dstr[dc].ToString();
                                break;
                            }
                        case "optname":
                            {
                                operatorDto.optname = dstr[dc].ToString();
                                break;
                            }
                        case "optpassword":
                            {
                                operatorDto.optpassword = dstr[dc].ToString();
                                break;
                            }
                        case "optpermission":
                            {
                                operatorDto.optpermission = (int)dstr[dc];
                                break;
                            }
                        case "optphone":
                            {
                                operatorDto.optphone = dstr[dc].ToString();
                                break;
                            }
                        case "opttype":
                            {
                                operatorDto.opttype = (int)dstr[dc];
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstOperatorDto.Add(operatorDto);
            }

            lock (m_objLockOperator)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach(COperatorDto operatorDto in lstOperatorDto)
                    {
                        // 取出操作员 
                        var editOperator = db.COperatorDto.SingleOrDefault<COperatorDto>(s => s.optcode == operatorDto.optcode);

                        if (null == editOperator)
                        {
                            db.COperatorDto.InsertOnSubmit(operatorDto);
                            continue;
                        }

                        // 修改操作员的属性 
                        editOperator.optname = operatorDto.optname;
                        editOperator.optpassword = operatorDto.optpassword;
                        editOperator.optphone = operatorDto.optphone;
                        editOperator.optaddr = operatorDto.optaddr;
                        editOperator.opttype = operatorDto.opttype;
                        editOperator.optpermission = operatorDto.optpermission;
                    }
                    db.SubmitChanges();
                    return lstOperatorDto;
                }
            }
        }

        /// <summary>
        /// 插入语音内容sound数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CSoundDto> InsertOnTableSound(DataTable dt)
        {
            // 语音内容sound
            List<CSoundDto> lstSoundDto = new List<CSoundDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CSoundDto soundDto = new CSoundDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "soundcode":
                            {
                                soundDto.soundcode = dstr[dc].ToString();
                                break;
                            }
                        case "soundcontent":
                            {
                                soundDto.soundcontent = dstr[dc].ToString();
                                break;
                            }
                        case "soundishand":
                            {
                                soundDto.soundishand = (int)dstr[dc];
                                break;
                            }
                        case "soundname":
                            {
                                soundDto.soundname = dstr[dc].ToString();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstSoundDto.Add(soundDto);
            }


            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                foreach (CSoundDto soundDto in lstSoundDto)
                {
                    // 取出语音
                    var editSound = db.CSoundDto.SingleOrDefault<CSoundDto>(s => s.soundcode == soundDto.soundcode);

                    if (null == editSound)
                    {
                        db.CSoundDto.InsertOnSubmit(soundDto);
                        continue;
                    }

                    // 修改语音的属性 
                    editSound.soundname = soundDto.soundname;
                    editSound.soundcontent = soundDto.soundcontent;
                    editSound.soundishand = soundDto.soundishand;
                }
                db.SubmitChanges();
                return lstSoundDto;
            }
        }

        /// <summary>
        /// 插入计费标准tariff数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CTariffDto> InsertOnTableTariff(DataTable dt)
        {
            // 计费标准tariff
            List<CTariffDto> lstTariffDto = new List<CTariffDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CTariffDto tariffDto = new CTariffDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "fee":
                            {
                                tariffDto.fee = float.Parse(dstr[dc].ToString());
                                break;
                            }
                        case "feetype":
                            {
                                tariffDto.feetype = (int)dstr[dc];
                                break;
                            }
                        case "iccardtype":
                            {
                                tariffDto.iccardtype = (int)dstr[dc];
                                break;
                            }
                        case "id":
                            {
                                tariffDto.id = (int)dstr[dc];
                                break;
                            }
                        case "isworkday":
                            {
                                tariffDto.isworkday = (int)dstr[dc];
                                break;
                            }
                        case "nonworkdayquotafee":
                            {
                                tariffDto.nonworkdayquotafee = float.Parse(dstr[dc].ToString());
                                break;
                            }
                        case "nonworknonpeakfirstunit":
                            {
                                tariffDto.nonworknonpeakfirstunit = dstr[dc].ToString();
                                break;
                            }
                        case "nonworknonpeakinunitfee":
                            {
                                tariffDto.nonworknonpeakinunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "nonworknonpeakoutunitfee":
                            {
                                tariffDto.nonworknonpeakoutunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "nonworkpeakfirstunit":
                            {
                                tariffDto.nonworkpeakfirstunit = dstr[dc].ToString();
                                break;
                            }
                        case "nonworkpeakinunitfee":
                            {
                                tariffDto.nonworkpeakinunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "nonworkpeakoutunitfee":
                            {
                                tariffDto.nonworkpeakoutunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "nonworkpeakperiod":
                            {
                                tariffDto.nonworkpeakperiod = dstr[dc].ToString();
                                break;
                            }
                        case "tariffdescp":
                            {
                                tariffDto.tariffdescp = dstr[dc].ToString();
                                break;
                            }
                        case "workdayquotafee":
                            {
                                tariffDto.workdayquotafee = float.Parse(dstr[dc].ToString());
                                break;
                            }
                        case "worknonpeakfirstunit":
                            {
                                tariffDto.worknonpeakfirstunit = dstr[dc].ToString();
                                break;
                            }
                        case "worknonpeakinunitfee":
                            {
                                tariffDto.worknonpeakinunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "worknonpeakoutunitfee":
                            {
                                tariffDto.worknonpeakoutunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "workpeakfirstunit":
                            {
                                tariffDto.workpeakfirstunit = dstr[dc].ToString();
                                break;
                            }
                        case "workpeakinunitfee":
                            {
                                tariffDto.workpeakinunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "workpeakoutunitfee":
                            {
                                tariffDto.workpeakoutunitfee = dstr[dc].ToString();
                                break;
                            }
                        case "workpeakperiod":
                            {
                                tariffDto.workpeakperiod = dstr[dc].ToString();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstTariffDto.Add(tariffDto);
            }

            lock (m_objLockTariff)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    foreach(CTariffDto tariffDto in lstTariffDto)
                    {
                        // 取出计费
                        var editTariff = db.CTariffDto.SingleOrDefault<CTariffDto>(s => s.id == tariffDto.id);

                        if (null == editTariff)
                        {
                            db.CTariffDto.InsertOnSubmit(tariffDto);
                            continue;
                        }

                        // 修改计费的属性 
                        editTariff.iccardtype = tariffDto.iccardtype;
                        editTariff.feetype = tariffDto.feetype;
                        editTariff.tariffdescp = tariffDto.tariffdescp;
                        editTariff.isworkday = tariffDto.isworkday;
                        editTariff.workpeakperiod = tariffDto.workpeakperiod;
                        editTariff.workdayquotafee = tariffDto.workdayquotafee;
                        editTariff.workpeakfirstunit = tariffDto.workpeakfirstunit;
                        editTariff.worknonpeakfirstunit = tariffDto.worknonpeakfirstunit;
                        editTariff.workpeakinunitfee = tariffDto.workpeakinunitfee;
                        editTariff.workpeakoutunitfee = tariffDto.workpeakoutunitfee;
                        editTariff.worknonpeakinunitfee = tariffDto.worknonpeakinunitfee;
                        editTariff.worknonpeakoutunitfee = tariffDto.worknonpeakoutunitfee;
                        editTariff.nonworkpeakperiod = tariffDto.nonworkpeakperiod;
                        editTariff.nonworkdayquotafee = tariffDto.nonworkdayquotafee;
                        editTariff.nonworkpeakfirstunit = tariffDto.nonworkpeakfirstunit;
                        editTariff.nonworknonpeakfirstunit = tariffDto.nonworknonpeakfirstunit;
                        editTariff.nonworkpeakinunitfee = tariffDto.nonworkpeakinunitfee;
                        editTariff.nonworkpeakoutunitfee = tariffDto.nonworkpeakoutunitfee;
                        editTariff.nonworknonpeakinunitfee = tariffDto.nonworknonpeakinunitfee;
                        editTariff.nonworknonpeakoutunitfee = tariffDto.nonworknonpeakoutunitfee;
                        editTariff.fee = tariffDto.fee;
                    }
                    db.SubmitChanges();
                    return lstTariffDto;
                }
            }
        }

        /// <summary>
        /// 插入队列排队信息workqueue数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CWorkQueueDto> InsertOnTableWorkQueue(DataTable dt)
        {
            lock (m_objLockWorkQueue)
            {
                // 队列排队信息workqueue
                List<CWorkQueueDto> lstWorkQueueDto = new List<CWorkQueueDto>();
                foreach (DataRow dstr in dt.Rows)
                {
                    CWorkQueueDto workQueueDto = new CWorkQueueDto();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                        {
                            continue;
                        }

                        switch (dc.ColumnName)
                        {
                            case "devicecode":
                                {
                                    workQueueDto.devicecode = (int)dstr[dc];
                                    break;
                                }
                            case "iccode":
                                {
                                    workQueueDto.iccode = dstr[dc].ToString();
                                    break;
                                }
                            case "id":
                                {
                                    workQueueDto.id = (int)dstr[dc];
                                    break;
                                }
                            case "priorityid":
                                {
                                    workQueueDto.priorityid = (int)dstr[dc];
                                    break;
                                }
                            case "swipecount":
                                {
                                    workQueueDto.swipecount = (int)dstr[dc];
                                    break;
                                }
                            case "waitsendtelegram":
                                {
                                    workQueueDto.waitsendtelegram = dstr[dc].ToString();
                                    break;
                                }
                            case "warehouse":
                                {
                                    workQueueDto.warehouse = (int)dstr[dc];
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    lstWorkQueueDto.Add(workQueueDto);
                }

                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    foreach (CWorkQueueDto workQueueDto in lstWorkQueueDto)
                    {
                        // 取出工作队列实例
                        var editWorkQueue = db.CWorkQueueDto.SingleOrDefault<CWorkQueueDto>(s => s.id == workQueueDto.id);

                        if (null == editWorkQueue)
                        {
                            db.CWorkQueueDto.InsertOnSubmit(workQueueDto);
                            continue;
                        }

                        // 修改工作队列的属性 
                        editWorkQueue.devicecode = workQueueDto.devicecode;
                        editWorkQueue.warehouse = workQueueDto.warehouse;
                        editWorkQueue.iccode = workQueueDto.iccode;
                        editWorkQueue.swipecount = workQueueDto.swipecount;
                        editWorkQueue.waitsendtelegram = workQueueDto.waitsendtelegram;
                        editWorkQueue.priorityid = workQueueDto.priorityid;
                    }
                    db.SubmitChanges();
                    return lstWorkQueueDto;
                }
            }
        }
        
        /// <summary>
        /// 插入报文日志信息telegramlog数据列表
        /// </summary>
        /// <param name="dt"></param>
        public List<CTelegramLogDto> InsertOnTableTelegramLog(DataTable dt)
        {
            // 队列排队信息workqueue
            List<CTelegramLogDto> lstTelegramLogDto = new List<CTelegramLogDto>();
            foreach (DataRow dstr in dt.Rows)
            {
                CTelegramLogDto telegramLogDto = new CTelegramLogDto();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (null == dstr[dc] || string.IsNullOrWhiteSpace(dstr[dc].ToString()))
                    {
                        continue;
                    }

                    switch (dc.ColumnName)
                    {
                        case "carinfo":
                            {
                                telegramLogDto.carinfo = dstr[dc].ToString();
                                break;
                            }
                        case "destaddr":
                            {
                                telegramLogDto.destaddr = dstr[dc].ToString();
                                break;
                            }
                        case "destdirection":
                            {
                                telegramLogDto.destdirection = (int)dstr[dc];
                                break;
                            }
                        case "devicecode":
                            {
                                telegramLogDto.devicecode = (int)dstr[dc];
                                break;
                            }
                        case "direction":
                            {
                                telegramLogDto.direction = (int)dstr[dc];
                                break;
                            }
                        case "iccode":
                            {
                                telegramLogDto.iccode = dstr[dc].ToString();
                                break;
                            }
                        case "id":
                            {
                                telegramLogDto.id = (int)dstr[dc];
                                break;
                            }
                        case "logtype":
                            {
                                telegramLogDto.logtype = dstr[dc].ToString();
                                break;
                            }
                        case "slotinfo":
                            {
                                telegramLogDto.slotinfo = dstr[dc].ToString();
                                break;
                            }
                        case "srcaddr":
                            {
                                telegramLogDto.srcaddr = dstr[dc].ToString();
                                break;
                            }
                        case "telegramhead":
                            {
                                telegramLogDto.telegramhead = dstr[dc].ToString();
                                break;
                            }
                        case "telegramid":
                            {
                                telegramLogDto.telegramid = (int)dstr[dc];
                                break;
                            }
                        case "time":
                            {
                                telegramLogDto.time = Convert.ToDateTime(dstr[dc]);
                                break;
                            }
                        case "warehouse":
                            {
                                telegramLogDto.warehouse = (int)dstr[dc];
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                lstTelegramLogDto.Add(telegramLogDto);
            }

            lock (m_objLockTelegramLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    foreach (CTelegramLogDto telegramLogDto in lstTelegramLogDto)
                    {
                        // 取出工作队列实例
                        var editTelegramLog = db.CTelegramLogDto.SingleOrDefault<CTelegramLogDto>(s => s.id == telegramLogDto.id);

                        if (null == editTelegramLog)
                        {
                            db.CTelegramLogDto.InsertOnSubmit(telegramLogDto);
                            continue;
                        }

                        // 修改报文日志的属性 
                        editTelegramLog.time = telegramLogDto.time;
                        editTelegramLog.logtype = telegramLogDto.logtype;
                        editTelegramLog.warehouse = telegramLogDto.warehouse;
                        editTelegramLog.telegramhead = telegramLogDto.telegramhead;
                        editTelegramLog.devicecode = telegramLogDto.devicecode;
                        editTelegramLog.iccode = telegramLogDto.iccode;
                        editTelegramLog.carinfo = telegramLogDto.carinfo;
                        editTelegramLog.slotinfo = telegramLogDto.slotinfo;
                        editTelegramLog.destdirection = telegramLogDto.destdirection;
                        editTelegramLog.direction = telegramLogDto.direction;
                        editTelegramLog.srcaddr = telegramLogDto.srcaddr;
                        editTelegramLog.destaddr = telegramLogDto.destaddr;
                        editTelegramLog.telegramid = telegramLogDto.telegramid;
                    }
                    db.SubmitChanges();
                    return lstTelegramLogDto;
                }
            }
        }
        #endregion

        #region 获取SQLRelateTable数据表列表
        /// <summary>
        /// 获取IC卡iccard所有数据列表
        /// </summary>
        /// <param name="icCardDtoList"></param>
        public bool GetTableDtoList(out List<CICCardDto> icCardDtoList)
        {
            using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
            {
                icCardDtoList = db.CICCardDto.ToList<CICCardDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取操作员operator所有数据列表
        /// </summary>
        /// <param name="operatorDtoList"></param>
        public bool GetTableDtoList(out List<COperatorDto> operatorDtoList)
        {
            using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
            {
                operatorDtoList = db.COperatorDto.ToList<COperatorDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取车主信息carcustomer所有数据列表
        /// </summary>
        /// <param name="carCustomerDtoList"></param>
        public bool GetTableDtoList(out List<CCarCustomerDto> carCustomerDtoList)
        {
            using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
            {
                carCustomerDtoList = db.CCarCustomerDto.ToList<CCarCustomerDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取车位信息carlocation所有数据列表
        /// </summary>
        /// <param name="carLocationDtoList"></param>
        public bool GetTableDtoList(out List<CCarLocationDto> carLocationDtoList)
        {
            using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
            {
                carLocationDtoList = db.CCarLocationDto.ToList<CCarLocationDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取设备状态devicestatus所有数据列表
        /// </summary>
        /// <param name="deviceStatusDtoList"></param>
        public bool GetTableDtoList(out List<CDeviceStatusDto> deviceStatusDtoList)
        {
            using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
            {
                deviceStatusDtoList = db.CDeviceStatusDto.ToList<CDeviceStatusDto>();
                return true;
            }
        }
        #endregion

        #region 获取SQLOtherTable数据表列表
        /// <summary>
        /// 获取计费tariff所有数据列表
        /// </summary>
        /// <param name="tariffDtoList"></param>
        public bool GetTableDtoList(out List<CTariffDto> tariffDtoList)
        {
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                tariffDtoList = db.CTariffDto.ToList<CTariffDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取设备故障devicefault所有数据列表
        /// </summary>
        /// <param name="deviceFaultDtoList"></param>
        public bool GetTableDtoList(out List<CDeviceFaultDto> deviceFaultDtoList)
        {
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                deviceFaultDtoList = db.CDeviceFaultDto.ToList<CDeviceFaultDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取语音sound所有数据列表
        /// </summary>
        /// <param name="soundDtoList"></param>
        public bool GetTableDtoList(out List<CSoundDto> soundDtoList)
        {
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                soundDtoList = db.CSoundDto.ToList<CSoundDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取工作队列（取车IC卡和等待发送报文队列）workqueue所有数据列表
        /// </summary>
        /// <param name="workQueueDtoList"></param>
        public bool GetTableDtoList(out List<CWorkQueueDto> workQueueDtoList)
        {
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                workQueueDtoList = db.CWorkQueueDto.ToList<CWorkQueueDto>();
                return true;
            }
        }
       
        /// <summary>
        /// 获取LED文本ledcontent所有数据列表
        /// </summary>
        /// <param name="ledContentDtoList"></param>
        public bool GetTableDtoList(out List<CLedContentDto> ledContentDtoList)
        {
            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                ledContentDtoList = db.CLedContentDto.ToList<CLedContentDto>();
                return true;
            }
        }
        #endregion

        #region 获取SQLLogTable数据表列表
        /// <summary>
        /// 获取IC卡缴费日志iccardlog所有数据列表
        /// </summary>
        /// <param name="logDtoList"></param>
        public bool GetTableDtoList(out List<CICCardLogDto> logDtoList)
        {
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                logDtoList = db.CICCardLogDto.ToList<CICCardLogDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取系统日志systemlog所有数据列表
        /// </summary>
        /// <param name="logDtoList"></param>
        public bool GetTableDtoList(out List<CSystemLogDto> logDtoList)
        {
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                logDtoList = db.CSystemLogDto.ToList<CSystemLogDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取报文日志telegramlog所有数据列表
        /// </summary>
        /// <param name="logDtoList"></param>
        public bool GetTableDtoList(out List<CTelegramLogDto> logDtoList)
        {
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                logDtoList = db.CTelegramLogDto.ToList<CTelegramLogDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取设备状态日志devicestatuslog所有数据列表
        /// </summary>
        /// <param name="logDtoList"></param>
        public bool GetTableDtoList(out List<CDeviceStatusLogDto> logDtoList)
        {
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                logDtoList = db.CDeviceStatusLogDto.ToList<CDeviceStatusLogDto>();
                return true;
            }
        }

        /// <summary>
        /// 获取设备故障日志devicefaultlog所有数据列表
        /// </summary>
        /// <param name="logDtoList"></param>
        public bool GetTableDtoList(out List<CDeviceFaultLogDto> logDtoList)
        {
            using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
            {
                logDtoList = db.CDeviceFaultLogDto.ToList<CDeviceFaultLogDto>();
                return true;
            }
        }
        #endregion 获取SQLLogTable数据表列表

        #region 插入SQLRelateTable数据表接口
        /// <summary>
        /// 插入IC卡iccard数据
        /// </summary>
        /// <param name="icCardDto"></param>
        private bool InsertOnTable(CICCardDto icCardDto)
        {
            if (null == icCardDto)
            {
                return false;
            }
            lock (m_objLockICCard)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    icCardDto.iccode = icCardDto.iccode.Trim();
                    db.CICCardDto.InsertOnSubmit(icCardDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入操作员operator数据
        /// </summary>
        /// <param name="operatorDto"></param>
        private bool InsertOnTable(COperatorDto operatorDto)
        {
            if (null == operatorDto)
            {
                return false;
            }
            lock (m_objLockOperator)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    db.COperatorDto.InsertOnSubmit(operatorDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入车主信息carcustomer数据
        /// </summary>
        /// <param name="carCustomerDto"></param>
        private bool InsertOnTable(CCarCustomerDto carCustomerDto)
        {
            if (null == carCustomerDto)
            {
                return false;
            }
            lock (m_objLockCustomer)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    db.CCarCustomerDto.InsertOnSubmit(carCustomerDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入车位信息carlocation数据
        /// </summary>
        /// <param name="carLocationDto"></param>
        private bool InsertOnTable(CCarLocationDto carLocationDto)
        {
            if (null == carLocationDto)
            {
                return false;
            }
            lock (m_objLockCarLocation)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    int nId = db.CCarLocationDto.Count() + 1;
                    while (null != db.CCarLocationDto.SingleOrDefault<CCarLocationDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    carLocationDto.id = nId;
                    db.CCarLocationDto.InsertOnSubmit(carLocationDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入设备状态devicestatus数据
        /// </summary>
        /// <param name="deviceStatusDto"></param>
        private bool InsertOnTable(CDeviceStatusDto deviceStatusDto)
        {
            if (null == deviceStatusDto)
            {
                return false;
            }
            lock (m_objLockDeviceStatus)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    int nId = db.CDeviceStatusDto.Count() + 1;
                    while (null != db.CDeviceStatusDto.SingleOrDefault<CDeviceStatusDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    deviceStatusDto.id = nId;
                    db.CDeviceStatusDto.InsertOnSubmit(deviceStatusDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion

        #region 插入SQLOtherTable数据表接口
        /// <summary>
        /// 插入计费tariff数据
        /// </summary>
        /// <param name="tariffDto"></param>
        private bool InsertOnTable(CTariffDto tariffDto)
        {
            if (null == tariffDto)
            {
                return false;
            }
            lock (m_objLockTariff)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    db.CTariffDto.InsertOnSubmit(tariffDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入设备故障devicefault数据
        /// </summary>
        /// <param name="deviceFaultDto"></param>
        private bool InsertOnTable(CDeviceFaultDto deviceFaultDto)
        {
            if (null == deviceFaultDto)
            {
                return false;
            }
            lock (m_objLockDeviceFault)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    int nId = db.CDeviceFaultDto.Count() + 1;
                    while (null != db.CDeviceFaultDto.SingleOrDefault<CDeviceFaultDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    deviceFaultDto.id = nId;
                    db.CDeviceFaultDto.InsertOnSubmit(deviceFaultDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入语音sound数据
        /// </summary>
        /// <param name="soundDto"></param>
        private bool InsertOnTable(CSoundDto soundDto)
        {
            if (null == soundDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                db.CSoundDto.InsertOnSubmit(soundDto);
                db.SubmitChanges();
                return true;
            }
        }

        /// <summary>
        /// 插入工作队列workqueue数据
        /// </summary>
        /// <param name="workQueueDto"></param>
        private bool InsertOnTable(CWorkQueueDto workQueueDto)
        {
            lock (m_objLockWorkQueue)
            {
                if (null == workQueueDto)
                {
                    return false;
                }

                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    db.CWorkQueueDto.InsertOnSubmit(workQueueDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入LED文本ledcontent数据
        /// </summary>
        /// <param name="ledContentDto"></param>
        private bool InsertOnTable(CLedContentDto ledContentDto)
        {
            if (null == ledContentDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                db.CLedContentDto.InsertOnSubmit(ledContentDto);
                db.SubmitChanges();
                return true;
            }
        }
        #endregion

        #region 插入SQLLogTable数据表接口
        /// <summary>
        /// 插入IC卡缴费日志iccardlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool InsertOnTable(CICCardLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }
            lock (m_objLockICCardLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    int nId = db.CICCardLogDto.Count() + 1;
                    while (null != db.CICCardLogDto.SingleOrDefault<CICCardLogDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    logDto.id = nId;
                    db.CICCardLogDto.InsertOnSubmit(logDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入系统日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool InsertOnTable(CSystemLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }
            lock (m_objLockSystemLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    int nId = db.CSystemLogDto.Count() + 1;
                    while (null != db.CSystemLogDto.SingleOrDefault<CSystemLogDto>(s => s.logid == nId))
                    {
                        nId++;
                    }
                    logDto.logid = nId;
                    db.CSystemLogDto.InsertOnSubmit(logDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入报文日志telegramlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool InsertOnTable(CTelegramLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }
            lock (m_objLockTelegramLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    int nId = db.CTelegramLogDto.Count() + 1;
                    while (null != db.CTelegramLogDto.SingleOrDefault<CTelegramLogDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    logDto.id = nId;
                    db.CTelegramLogDto.InsertOnSubmit(logDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入设备状态日志devicestatuslog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool InsertOnTable(CDeviceStatusLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }
            lock (m_objLockDeviceStatusLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    int nId = db.CDeviceStatusLogDto.Count() + 1;
                    while (null != db.CDeviceStatusLogDto.SingleOrDefault<CDeviceStatusLogDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    logDto.id = nId;
                    db.CDeviceStatusLogDto.InsertOnSubmit(logDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 插入设备故障日志devicefaultlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool InsertOnTable(CDeviceFaultLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }
            lock (m_objLockDeviceFaultLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    int nId = db.CDeviceFaultLogDto.Count() + 1;
                    while (null != db.CDeviceFaultLogDto.SingleOrDefault<CDeviceFaultLogDto>(s => s.id == nId))
                    {
                        nId++;
                    }
                    logDto.id = nId;
                    db.CDeviceFaultLogDto.InsertOnSubmit(logDto);
                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion 插入SQLLogTable数据表接口

        #region 更新修改SQLRelateTable数据表
        /// <summary>
        /// 更新修改IC卡iccard数据（根据关键字id）
        /// </summary>
        /// <param name="icCardDto"></param>
        private bool UpdateTable(CICCardDto icCardDto)
        {
            if (null == icCardDto)
            {
                return false;
            }

            lock (m_objLockICCard)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    //DataAccess.DeviceStatus deviceStatus = context.DeviceStatus.Single(dd => dd.Device == device);
                    //var value = (from ds in context.DeviceStatus where ds.Device == device select new { ds.UnitID, ds.Flight, ds.Weight, ds.Higher }).Single();
                    // 取出IC卡 
                    var editICCard = db.CICCardDto.SingleOrDefault<CICCardDto>(s => s.id == icCardDto.id);

                    if (null == editICCard)
                    {// 弹出警告
                        return false;
                    }

                    // 修改IC卡的属性 
                    editICCard.iccode = icCardDto.iccode;
                    editICCard.oldiccode = icCardDto.oldiccode;
                    editICCard.phycode = icCardDto.phycode;
                    editICCard.ictype = icCardDto.ictype;
                    editICCard.icstatus = icCardDto.icstatus;
                    editICCard.icnewtime = icCardDto.icnewtime;
                    editICCard.iclosstime = icCardDto.iclosstime;
                    editICCard.iclogouttime = icCardDto.iclogouttime;
                    editICCard.icstarttime = icCardDto.icstarttime;
                    editICCard.icdeadline = icCardDto.icdeadline;
                    editICCard.carlocaddr = icCardDto.carlocaddr;
                    editICCard.warehouse = icCardDto.warehouse;
                    editICCard.userid = icCardDto.userid;
                    editICCard.tariffid = icCardDto.tariffid;
                    editICCard.priorityid = icCardDto.priorityid;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改操作员operator数据（根据关键字操作员号码optcode）
        /// </summary>
        /// <param name="operatorDto"></param>
        private bool UpdateTable(COperatorDto operatorDto)
        {
            if (null == operatorDto)
            {
                return false;
            }

            lock (m_objLockOperator)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出操作员 
                    var editOperator = db.COperatorDto.SingleOrDefault<COperatorDto>(s => s.optcode == operatorDto.optcode);

                    if (null == editOperator)
                    {// 弹出警告
                        return false;
                    }

                    // 修改操作员的属性 
                    editOperator.optname = operatorDto.optname;
                    editOperator.optpassword = operatorDto.optpassword;
                    editOperator.optphone = operatorDto.optphone;
                    editOperator.optaddr = operatorDto.optaddr;
                    editOperator.opttype = operatorDto.opttype;
                    editOperator.optpermission = operatorDto.optpermission;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改车主信息carcustomer数据（根据关键字用户ID userid）
        /// </summary>
        /// <param name="carCustomerDto"></param>
        private bool UpdateTable(CCarCustomerDto carCustomerDto)
        {
            if (null == carCustomerDto)
            {
                return false;
            }

            lock (m_objLockCustomer)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出车主信息 
                    var editCarCustomer = db.CCarCustomerDto.SingleOrDefault<CCarCustomerDto>(s => s.userid == carCustomerDto.userid);

                    if (null == editCarCustomer)
                    {// 弹出警告
                        return false;
                    }

                    // 修改车主信息的属性 
                    editCarCustomer.username = carCustomerDto.username;
                    editCarCustomer.useraddr = carCustomerDto.useraddr;
                    editCarCustomer.userphone = carCustomerDto.userphone;
                    editCarCustomer.usermobile = carCustomerDto.usermobile;
                    editCarCustomer.carnumber = carCustomerDto.carnumber;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改车位信息carlocation数据（根据关键字索引号id）
        /// </summary>
        /// <param name="carLocationDto"></param>
        private bool UpdateTable(CCarLocationDto carLocationDto)
        {
            if (null == carLocationDto)
            {
                return false;
            }

            lock (m_objLockCarLocation)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出车位信息 
                    var editCarLocation = db.CCarLocationDto.SingleOrDefault<CCarLocationDto>(s => s.id == carLocationDto.id);

                    if (null == editCarLocation)
                    {// 弹出警告
                        return false;
                    }
                    // 修改车位信息的属性                   
                    editCarLocation.carlocstatus = carLocationDto.carlocstatus;
                    editCarLocation.carloctype = carLocationDto.carloctype;
                    editCarLocation.carlocsize = carLocationDto.carlocsize;
                    editCarLocation.iccode = carLocationDto.iccode;
                    editCarLocation.carwheelbase = carLocationDto.carwheelbase;
                    editCarLocation.carsize = carLocationDto.carsize;
                    editCarLocation.isbackup = carLocationDto.isbackup;
                    editCarLocation.carintime = carLocationDto.carintime;
                    editCarLocation.priorityid = carLocationDto.priorityid;                   
                    editCarLocation.carnumber = carLocationDto.carnumber;
                    editCarLocation.carpicture = carLocationDto.carpicture;
                    editCarLocation.direction = carLocationDto.direction;

                    editCarLocation.offcenter = carLocationDto.offcenter;
                    editCarLocation.rearwheeldis = carLocationDto.rearwheeldis;
                    editCarLocation.carweight = carLocationDto.carweight;

                    editCarLocation.overallLg = carLocationDto.overallLg;
                    editCarLocation.overhang = carLocationDto.overhang;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改设备状态devicestatus数据（根据关键字索引号id）
        /// </summary>
        /// <param name="deviceStatusDto"></param>
        private bool UpdateTable(CDeviceStatusDto deviceStatusDto)
        {
            if (null == deviceStatusDto)
            {
                return false;
            }

            lock (m_objLockDeviceStatus)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出设备状态
                    var editDeviceStatus = db.CDeviceStatusDto.SingleOrDefault<CDeviceStatusDto>(s => s.id == deviceStatusDto.id);

                    if (null == editDeviceStatus)
                    {// 弹出警告
                        return false;
                    }

                    bool isInsertLog = false;
                    if (editDeviceStatus.devicemode != deviceStatusDto.devicemode
                        || editDeviceStatus.halltype != deviceStatusDto.halltype
                        || editDeviceStatus.tasktype != deviceStatusDto.tasktype
                        || editDeviceStatus.isavailable != deviceStatusDto.isavailable
                        || editDeviceStatus.instep != deviceStatusDto.instep
                        || editDeviceStatus.outstep != deviceStatusDto.outstep
                        || editDeviceStatus.isable != deviceStatusDto.isable
                        || editDeviceStatus.runstep != deviceStatusDto.runstep)
                    {// 设备模式、车厅类型、当前作业等其一更新，则写入设备状态日志
                        isInsertLog = true;
                    }

                    // 修改设备状态的属性
                    editDeviceStatus.iccode = deviceStatusDto.iccode;
                    editDeviceStatus.tasktype = deviceStatusDto.tasktype;
                    editDeviceStatus.isable = deviceStatusDto.isable;
                    editDeviceStatus.isavailable = deviceStatusDto.isavailable;
                    editDeviceStatus.devicemode = deviceStatusDto.devicemode;
                    editDeviceStatus.deviceaddr = deviceStatusDto.deviceaddr;                  
                    editDeviceStatus.halltype = deviceStatusDto.halltype;
                    editDeviceStatus.instep = deviceStatusDto.instep;
                    editDeviceStatus.outstep = deviceStatusDto.outstep;
                    editDeviceStatus.runstep = deviceStatusDto.runstep;
                    editDeviceStatus.warehousechange = deviceStatusDto.warehousechange;                 
                    editDeviceStatus.prevnode = deviceStatusDto.prevnode;
                    editDeviceStatus.currentnode = deviceStatusDto.currentnode;
                    editDeviceStatus.queueprevnode = deviceStatusDto.queueprevnode;
                    editDeviceStatus.queuecurrentnode = deviceStatusDto.queuecurrentnode;
                    editDeviceStatus.scrloc = deviceStatusDto.scrloc;
                    editDeviceStatus.desccloc = deviceStatusDto.desccloc;
                    editDeviceStatus.devicelayer = deviceStatusDto.devicelayer;

                    // 执行更新操作 
                    db.SubmitChanges();

                    if (isInsertLog)
                    {
                        CDeviceStatusLogDto logDto = new CDeviceStatusLogDto
                        {
                            warehouse = deviceStatusDto.warehouse,
                            devicecode = deviceStatusDto.devicecode,
                            deviceaddr = deviceStatusDto.deviceaddr,
                            time = DateTime.Now,
                            devicemode = deviceStatusDto.devicemode,
                            devicetype = deviceStatusDto.devicetype,
                            halltype = deviceStatusDto.halltype,
                            curtasktype = deviceStatusDto.tasktype,
                            isavailable = deviceStatusDto.isavailable,
                            instep = deviceStatusDto.instep,
                            outstep = deviceStatusDto.outstep,
                            isable = deviceStatusDto.isable,
                            runstep = deviceStatusDto.runstep
                        };

                        InsertOnTable(logDto);
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改车位信息carlocation数据列表（根据关键字索引号id）
        /// </summary>
        /// <param name="carLocationDto"></param>
        public bool UpdateTable(List<CCarLocationDto> lstCarLocationDto)
        {
            if (null == lstCarLocationDto || 1 > lstCarLocationDto.Count)
            {
                return false;
            }

            lock (m_objLockCarLocation)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach (CCarLocationDto carLocationDto in lstCarLocationDto)
                    {// 取出车位信息 
                        var editCarLocation = db.CCarLocationDto.SingleOrDefault<CCarLocationDto>(s => s.id == carLocationDto.id);

                        if (null == editCarLocation)
                        {// 弹出警告
                            continue;
                        }


                        // 修改车位信息的属性 
                        editCarLocation.carlocaddr = carLocationDto.carlocaddr;
                        editCarLocation.warehouse = carLocationDto.warehouse;
                        editCarLocation.carlocside = carLocationDto.carlocside;
                        editCarLocation.carloccolumn = carLocationDto.carloccolumn;
                        editCarLocation.carloclayer = carLocationDto.carloclayer;
                        editCarLocation.carlocstatus = carLocationDto.carlocstatus;
                        editCarLocation.carloctype = carLocationDto.carloctype;
                        editCarLocation.carlocsize = carLocationDto.carlocsize;
                        editCarLocation.iccode = carLocationDto.iccode;
                        editCarLocation.carwheelbase = carLocationDto.carwheelbase;
                        editCarLocation.carsize = carLocationDto.carsize;
                        editCarLocation.isbackup = carLocationDto.isbackup;
                        editCarLocation.carintime = carLocationDto.carintime;
                        editCarLocation.priorityid = carLocationDto.priorityid;
                        editCarLocation.region = carLocationDto.region;
                        editCarLocation.carnumber = carLocationDto.carnumber;
                        editCarLocation.carpicture = carLocationDto.carpicture;
                        editCarLocation.direction = carLocationDto.direction;
                    }

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改设备状态devicestatus数据列表（根据关键字索引号id）
        /// </summary>
        /// <param name="deviceStatusDto"></param>
        public bool UpdateTable(List<CDeviceStatusDto> lstDeviceStatusDto)
        {
            if (null == lstDeviceStatusDto || 1 > lstDeviceStatusDto.Count)
            {
                return false;
            }

            lock (m_objLockDeviceStatus)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    foreach (CDeviceStatusDto deviceStatusDto in lstDeviceStatusDto)
                    {// 取出设备状态
                        var editDeviceStatus = db.CDeviceStatusDto.SingleOrDefault<CDeviceStatusDto>(s => s.id == deviceStatusDto.id);

                        if (null == editDeviceStatus)
                        {// 弹出警告
                            continue;
                        }

                        if (editDeviceStatus.devicemode != deviceStatusDto.devicemode
                            || editDeviceStatus.halltype != deviceStatusDto.halltype
                            || editDeviceStatus.tasktype != deviceStatusDto.tasktype
                            || editDeviceStatus.isable != deviceStatusDto.isable
                            || editDeviceStatus.isavailable != deviceStatusDto.isavailable
                            || editDeviceStatus.instep != deviceStatusDto.instep
                            || editDeviceStatus.outstep != deviceStatusDto.outstep
                            || editDeviceStatus.runstep != deviceStatusDto.runstep)
                        {// 设备模式、车厅类型、当前作业等其一更新，则写入设备状态日志
                            CDeviceStatusLogDto logDto = new CDeviceStatusLogDto
                            {
                                warehouse = deviceStatusDto.warehouse,
                                devicecode = deviceStatusDto.devicecode,
                                deviceaddr = deviceStatusDto.deviceaddr,
                                time = DateTime.Now,
                                devicemode = deviceStatusDto.devicemode,
                                devicetype = deviceStatusDto.devicetype,
                                halltype = deviceStatusDto.halltype,
                                curtasktype = deviceStatusDto.tasktype,
                                isable = deviceStatusDto.isable,
                                isavailable = deviceStatusDto.isavailable,
                                instep = deviceStatusDto.instep,
                                outstep = deviceStatusDto.outstep,
                                runstep = deviceStatusDto.runstep                                
                            };

                            InsertOnTable(logDto);
                        }

                        // 修改设备状态的属性                       
                        editDeviceStatus.iccode = deviceStatusDto.iccode;
                        editDeviceStatus.tasktype = deviceStatusDto.tasktype;
                        editDeviceStatus.isable = deviceStatusDto.isable;
                        editDeviceStatus.isavailable = deviceStatusDto.isavailable;
                        editDeviceStatus.devicemode = deviceStatusDto.devicemode;
                        editDeviceStatus.deviceaddr = deviceStatusDto.deviceaddr;
                     
                        editDeviceStatus.halltype = deviceStatusDto.halltype;
                        editDeviceStatus.instep = deviceStatusDto.instep;
                        editDeviceStatus.outstep = deviceStatusDto.outstep;
                        editDeviceStatus.runstep = deviceStatusDto.runstep;
                        editDeviceStatus.warehousechange = deviceStatusDto.warehousechange;
                   
                        editDeviceStatus.prevnode = deviceStatusDto.prevnode;
                        editDeviceStatus.currentnode = deviceStatusDto.currentnode;
                        editDeviceStatus.queueprevnode = deviceStatusDto.queueprevnode;
                        editDeviceStatus.queuecurrentnode = deviceStatusDto.queuecurrentnode;

                        editDeviceStatus.scrloc = deviceStatusDto.scrloc;
                        editDeviceStatus.desccloc = deviceStatusDto.desccloc;
                    }

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion

        #region 更新修改SQLOtherTable数据表
        /// <summary>
        /// 更新修改计费tariff数据（根据关键字计费ID id）
        /// </summary>
        /// <param name="tariffDto"></param>
        private bool UpdateTable(CTariffDto tariffDto)
        {
            if (null == tariffDto)
            {
                return false;
            }

            lock (m_objLockTariff)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出计费
                    var editTariff = db.CTariffDto.SingleOrDefault<CTariffDto>(s => s.id == tariffDto.id);

                    if (null == editTariff)
                    {// 弹出警告
                        return false;
                    }

                    // 修改计费的属性 
                    editTariff.iccardtype = tariffDto.iccardtype;
                    editTariff.feetype = tariffDto.feetype;
                    editTariff.tariffdescp = tariffDto.tariffdescp;
                    editTariff.isworkday = tariffDto.isworkday;
                    editTariff.workpeakperiod = tariffDto.workpeakperiod;
                    editTariff.workdayquotafee = tariffDto.workdayquotafee;
                    editTariff.workpeakfirstunit = tariffDto.workpeakfirstunit;
                    editTariff.worknonpeakfirstunit = tariffDto.worknonpeakfirstunit;
                    editTariff.workpeakinunitfee = tariffDto.workpeakinunitfee;
                    editTariff.workpeakoutunitfee = tariffDto.workpeakoutunitfee;
                    editTariff.worknonpeakinunitfee = tariffDto.worknonpeakinunitfee;
                    editTariff.worknonpeakoutunitfee = tariffDto.worknonpeakoutunitfee;
                    editTariff.nonworkpeakperiod = tariffDto.nonworkpeakperiod;
                    editTariff.nonworkdayquotafee = tariffDto.nonworkdayquotafee;
                    editTariff.nonworkpeakfirstunit = tariffDto.nonworkpeakfirstunit;
                    editTariff.nonworknonpeakfirstunit = tariffDto.nonworknonpeakfirstunit;
                    editTariff.nonworkpeakinunitfee = tariffDto.nonworkpeakinunitfee;
                    editTariff.nonworkpeakoutunitfee = tariffDto.nonworkpeakoutunitfee;
                    editTariff.nonworknonpeakinunitfee = tariffDto.nonworknonpeakinunitfee;
                    editTariff.nonworknonpeakoutunitfee = tariffDto.nonworknonpeakoutunitfee;
                    editTariff.fee = tariffDto.fee;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改设备故障devicefault数据（根据关键字设备故障索引id）
        /// </summary>
        /// <param name="deviceFaultDto"></param>
        private bool UpdateTable(CDeviceFaultDto deviceFaultDto)
        {
            if (null == deviceFaultDto)
            {
                return false;
            }

            lock (m_objLockDeviceFault)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出设备故障
                    var editDeviceFault = db.CDeviceFaultDto.SingleOrDefault<CDeviceFaultDto>(s => s.id == deviceFaultDto.id);

                    if (null == editDeviceFault)
                    {// 弹出警告
                        return false;
                    }

                    // 修改设备故障的属性 
                    editDeviceFault.faultdescp = deviceFaultDto.faultdescp;
                    editDeviceFault.color = deviceFaultDto.color;
                    editDeviceFault.isable = deviceFaultDto.isable;
                    editDeviceFault.devicecode = deviceFaultDto.devicecode;
                    editDeviceFault.warehouse = deviceFaultDto.warehouse;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改语音sound数据（根据关键字声音号码soundcode）
        /// </summary>
        /// <param name="soundDto"></param>
        private bool UpdateTable(CSoundDto soundDto)
        {
            if (null == soundDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                // 取出语音
                var editSound = db.CSoundDto.SingleOrDefault<CSoundDto>(s => s.soundcode == soundDto.soundcode);

                if (null == editSound)
                {// 弹出警告
                    return false;
                }

                // 修改语音的属性 
                editSound.soundname = soundDto.soundname;
                editSound.soundcontent = soundDto.soundcontent;
                editSound.soundishand = soundDto.soundishand;

                // 执行更新操作 
                db.SubmitChanges();
                return true;
            }
        }

        /// <summary>
        /// 更新修改工作队列workqueue数据（根据关键字索引号id）
        /// </summary>
        /// <param name="workQueueDto"></param>
        private bool UpdateTable(CWorkQueueDto workQueueDto)
        {
            if (null == workQueueDto)
            {
                return false;
            }

            lock (m_objLockWorkQueue)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出工作队列实例
                    var editWorkQueue = db.CWorkQueueDto.SingleOrDefault<CWorkQueueDto>(s => s.id == workQueueDto.id);

                    if (null == editWorkQueue)
                    {// 弹出警告
                        return false;
                    }

                    // 修改工作队列的属性 
                    editWorkQueue.devicecode = workQueueDto.devicecode;
                    editWorkQueue.warehouse = workQueueDto.warehouse;
                    editWorkQueue.iccode = workQueueDto.iccode;
                    editWorkQueue.swipecount = workQueueDto.swipecount;
                    editWorkQueue.waitsendtelegram = workQueueDto.waitsendtelegram;
                    editWorkQueue.priorityid = workQueueDto.priorityid;
                    editWorkQueue.tasktype = workQueueDto.tasktype;
                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 修改作业优先级
        /// </summary>
        /// <param name="strSrcICCode"></param>
        /// <param name="strDestICCode"></param>
        /// <returns></returns>
        public bool ModifWorkQueue(string strSrcICCode, string strDestICCode)
        {
            if (string.IsNullOrWhiteSpace(strSrcICCode) || string.IsNullOrWhiteSpace(strDestICCode))
            {
                return false;
            }

            lock (m_objLockWorkQueue)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    List<CWorkQueueDto> lst = db.CWorkQueueDto.ToList();
                    // 取出工作队列实例
                    var srcWorkQueue = lst.Find(s => s.iccode == strSrcICCode && (s.swipecount == (int)EnmSwipeCount.GetCar || s.swipecount == (int)EnmSwipeCount.TmpFetch));
                    var destWorkQueue = lst.Find(s => s.iccode == strDestICCode && (s.swipecount == (int)EnmSwipeCount.GetCar || s.swipecount == (int)EnmSwipeCount.TmpFetch));

                    if (null == srcWorkQueue || null == destWorkQueue)
                    {// 弹出警告
                        return false;
                    }

                    int nMinID = srcWorkQueue.id < destWorkQueue.id ? srcWorkQueue.id : destWorkQueue.id;
                    int nMaxID = srcWorkQueue.id > destWorkQueue.id ? srcWorkQueue.id : destWorkQueue.id;
                    int nNum = srcWorkQueue.id > destWorkQueue.id ? 1 : -1;

                    // 修改该实例优先级
                    CWorkQueueDto copyWorkQueue = new CWorkQueueDto
                    {
                        id = destWorkQueue.id,
                        warehouse = srcWorkQueue.warehouse,
                        devicecode = srcWorkQueue.devicecode,
                        iccode = srcWorkQueue.iccode,
                        swipecount = srcWorkQueue.swipecount,
                        waitsendtelegram = srcWorkQueue.waitsendtelegram,
                        priorityid = srcWorkQueue.priorityid
                    };

                    db.CWorkQueueDto.DeleteOnSubmit(srcWorkQueue);
                    lst.Remove(srcWorkQueue);

                    // 大于destWorkQueue->id且小于srcWorkQueue->id的所有行实例的id递增1
                    lst.Reverse();
                    foreach (CWorkQueueDto dto in lst)
                    {
                        if (null != dto && dto.id >= nMinID && dto.id <= nMaxID)
                        {
                            CWorkQueueDto newDto = new CWorkQueueDto
                            {
                                id = dto.id + nNum,
                                devicecode = dto.devicecode,
                                warehouse = dto.warehouse,
                                iccode = dto.iccode,
                                swipecount = dto.swipecount,
                                waitsendtelegram = dto.waitsendtelegram,
                                priorityid = dto.priorityid
                            };

                            db.CWorkQueueDto.InsertOnSubmit(newDto);
                            db.CWorkQueueDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.CWorkQueueDto.InsertOnSubmit(copyWorkQueue);
                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改LED文本ledcontent数据（根据关键字索引号id）
        /// </summary>
        /// <param name="ledContentDto"></param>
        private bool UpdateTable(CLedContentDto ledContentDto)
        {
            if (null == ledContentDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                // 取出工作队列实例
                var editLedContent = db.CLedContentDto.SingleOrDefault<CLedContentDto>(s => s.id == ledContentDto.id);

                if (null == editLedContent)
                {// 弹出警告
                    return false;
                }

                // 修改工作队列的属性 
                editLedContent.ledcontent = ledContentDto.ledcontent;

                // 执行更新操作 
                db.SubmitChanges();
                return true;
            }
        }
        #endregion

        #region 更新修改SQLLogTable数据表
        /// <summary>
        /// 更新修改IC卡缴费日志iccardlog数据（根据关键字索引ID id）
        /// </summary>
        /// <param name="logDto"></param>
        private bool UpdateTable(CICCardLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockICCardLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出IC卡缴费日志
                    var editLog = db.CICCardLogDto.SingleOrDefault<CICCardLogDto>(s => s.id == logDto.id);

                    if (null == editLog)
                    {// 弹出警告
                        return false;
                    }

                    // 修改IC卡缴费日志的属性 
                    editLog.iccode = logDto.iccode;
                    editLog.ictype = logDto.ictype;
                    editLog.paymenttime = logDto.paymenttime;
                    editLog.starttime = logDto.starttime;
                    editLog.endtime = logDto.endtime;
                    editLog.calculatedays = logDto.calculatedays;
                    editLog.feetype = logDto.feetype;
                    editLog.tariffnorm = logDto.tariffnorm;
                    editLog.payablefee = logDto.payablefee;
                    editLog.actualfee = logDto.actualfee;
                    editLog.username = logDto.username;
                    editLog.optcode = logDto.optcode;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改系统日志systemlog数据（根据关键字日志ID logid）
        /// </summary>
        /// <param name="logDto"></param>
        private bool UpdateTable(CSystemLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockSystemLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出系统日志
                    var editLog = db.CSystemLogDto.SingleOrDefault<CSystemLogDto>(s => s.logid == logDto.logid);

                    if (null == editLog)
                    {// 弹出警告
                        return false;
                    }

                    // 修改系统日志的属性 
                    editLog.logdescp = logDto.logdescp;
                    editLog.curtime = logDto.curtime;
                    editLog.optcode = logDto.optcode;
                    editLog.optname = logDto.optname;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改报文日志iccardlog数据（根据关键字索引ID id）
        /// </summary>
        /// <param name="logDto"></param>
        private bool UpdateTable(CTelegramLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockTelegramLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出报文日志
                    var editLog = db.CTelegramLogDto.SingleOrDefault<CTelegramLogDto>(s => s.id == logDto.id);

                    if (null == editLog)
                    {// 弹出警告
                        return false;
                    }

                    // 修改报文日志的属性 
                    editLog.time = logDto.time;
                    editLog.logtype = logDto.logtype;
                    editLog.warehouse = logDto.warehouse;
                    editLog.telegramhead = logDto.telegramhead;
                    editLog.devicecode = logDto.devicecode;
                    editLog.iccode = logDto.iccode;
                    editLog.carinfo = logDto.carinfo;
                    editLog.slotinfo = logDto.slotinfo;
                    editLog.destdirection = logDto.destdirection;
                    editLog.direction = logDto.direction;
                    editLog.srcaddr = logDto.srcaddr;
                    editLog.destaddr = logDto.destaddr;
                    editLog.telegramid = logDto.telegramid;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改设备状态日志iccardlog数据（根据关键字索引ID id）
        /// </summary>
        /// <param name="logDto"></param>
        private bool UpdateTable(CDeviceStatusLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockDeviceStatusLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出设备状态日志
                    var editLog = db.CDeviceStatusLogDto.SingleOrDefault<CDeviceStatusLogDto>(s => s.id == logDto.id);

                    if (null == editLog)
                    {// 弹出警告
                        return false;
                    }

                    // 修改设备状态日志的属性 
                    editLog.warehouse = logDto.warehouse;
                    editLog.devicecode = logDto.devicecode;
                    editLog.deviceaddr = logDto.deviceaddr;
                    editLog.time = logDto.time;
                    editLog.devicemode = logDto.devicemode;
                    editLog.devicetype = logDto.devicetype;
                    editLog.halltype = logDto.halltype;
                    editLog.curtasktype = logDto.curtasktype;
                    editLog.isavailable = logDto.isavailable;
                    editLog.isgetoutcar = logDto.isgetoutcar;
                    editLog.instep = logDto.instep;
                    editLog.outstep = logDto.outstep;
                    editLog.isable = logDto.isable;
                    editLog.runstep = logDto.runstep;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 更新修改设备故障日志iccardlog数据（根据关键字索引ID id）
        /// </summary>
        /// <param name="logDto"></param>
        private bool UpdateTable(CDeviceFaultLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockDeviceFaultLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出设备故障日志
                    var editLog = db.CDeviceFaultLogDto.SingleOrDefault<CDeviceFaultLogDto>(s => s.id == logDto.id);

                    if (null == editLog)
                    {// 弹出警告
                        return false;
                    }

                    // 修改设备故障日志的属性 
                    editLog.warehouse = logDto.warehouse;
                    editLog.devicecode = logDto.devicecode;
                    editLog.faultdescp = logDto.faultdescp;
                    editLog.time = logDto.time;
                    editLog.optcode = logDto.optcode;
                    editLog.color = logDto.color;

                    // 执行更新操作 
                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion

        #region 删除SQLRelateTable数据表接口
        /// <summary>
        /// 删除IC卡iccard数据
        /// </summary>
        /// <param name="icCardDto"></param>
        private bool DeleteOnTable(CICCardDto icCardDto)
        {
            if (null == icCardDto)
            {
                return false;
            }

            lock (m_objLockICCard)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {  // 取出IC卡 
                    var deleteICCard = db.CICCardDto.SingleOrDefault<CICCardDto>(s => s.id == icCardDto.id);

                    if (null == deleteICCard)
                    {
                        return false;
                    }

                    db.CICCardDto.DeleteOnSubmit(deleteICCard);

                    // 大于该id的所有行实例的id递减1
                    List<CICCardDto> lst = db.CICCardDto.ToList();
                    foreach (CICCardDto dto in lst)
                    {
                        if (null != dto && dto.id > icCardDto.id)
                        {
                            CICCardDto newDto = new CICCardDto();
                            newDto.id = dto.id - 1;
                            newDto.iccode = dto.iccode;
                            newDto.oldiccode = dto.oldiccode;
                            newDto.phycode = dto.phycode;
                            newDto.ictype = dto.ictype;
                            newDto.icstatus = dto.icstatus;
                            newDto.icnewtime = dto.icnewtime;
                            newDto.iclosstime = dto.iclosstime;
                            newDto.iclogouttime = dto.iclogouttime;
                            newDto.icstarttime = dto.icstarttime;
                            newDto.icdeadline = dto.icdeadline;
                            newDto.carlocaddr = dto.carlocaddr;
                            newDto.warehouse = dto.warehouse;
                            newDto.userid = dto.userid;
                            newDto.tariffid = dto.tariffid;
                            newDto.priorityid = dto.priorityid;

                            db.CICCardDto.InsertOnSubmit(newDto);
                            db.CICCardDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除操作员operator数据
        /// </summary>
        /// <param name="operatorDto"></param>
        private bool DeleteOnTable(COperatorDto operatorDto)
        {
            if (null == operatorDto)
            {
                return false;
            }

            lock (m_objLockOperator)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出操作员 
                    var deleteOperator = db.COperatorDto.SingleOrDefault<COperatorDto>(s => s.optcode == operatorDto.optcode);

                    if (null == deleteOperator)
                    {
                        return false;
                    }

                    db.COperatorDto.DeleteOnSubmit(deleteOperator);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除车主信息carcustomer数据
        /// </summary>
        /// <param name="carCustomerDto"></param>
        private bool DeleteOnTable(CCarCustomerDto carCustomerDto)
        {
            if (null == carCustomerDto)
            {
                return false;
            }

            lock (m_objLockCustomer)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出车主信息 
                    var deleteCarCustomer = db.CCarCustomerDto.SingleOrDefault<CCarCustomerDto>(s => s.userid == carCustomerDto.userid);

                    if (null == deleteCarCustomer)
                    {
                        return false;
                    }

                    db.CCarCustomerDto.DeleteOnSubmit(deleteCarCustomer);
                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除车位信息carlocation数据
        /// </summary>
        /// <param name="carLocationDto"></param>
        private bool DeleteOnTable(CCarLocationDto carLocationDto)
        {
            if (null == carLocationDto)
            {
                return false;
            }

            lock (m_objLockCarLocation)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出车位信息 
                    var deleteCarLocation = db.CCarLocationDto.SingleOrDefault<CCarLocationDto>(s => s.id == carLocationDto.id);

                    if (null == deleteCarLocation)
                    {
                        return false;
                    }

                    db.CCarLocationDto.DeleteOnSubmit(deleteCarLocation);

                    // 大于该id的所有行实例的id递减1
                    List<CCarLocationDto> lst = db.CCarLocationDto.ToList();
                    foreach (CCarLocationDto dto in lst)
                    {
                        if (null != dto && dto.id > carLocationDto.id)
                        {
                            CCarLocationDto newDto = new CCarLocationDto
                            {
                                id = dto.id - 1,
                                carlocaddr = dto.carlocaddr,
                                warehouse = dto.warehouse,
                                carlocside = dto.carlocside,
                                carloccolumn = dto.carloccolumn,
                                carloclayer = dto.carloclayer,
                                carlocstatus = dto.carlocstatus,
                                carloctype = dto.carloctype,
                                carlocsize = dto.carlocsize,
                                iccode = dto.iccode,
                                carwheelbase = dto.carwheelbase,
                                carsize = dto.carsize,
                                isbackup = dto.isbackup,
                                carintime = dto.carintime,
                                priorityid = dto.priorityid,
                                region = dto.region,
                                carnumber = dto.carnumber,
                                carpicture = dto.carpicture,
                                direction = dto.direction
                            };

                            db.CCarLocationDto.InsertOnSubmit(newDto);
                            db.CCarLocationDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除设备状态devicestatus数据
        /// </summary>
        /// <param name="deviceStatusDto"></param>
        private bool DeleteOnTable(CDeviceStatusDto deviceStatusDto)
        {
            if (null == deviceStatusDto)
            {
                return false;
            }

            lock (m_objLockDeviceStatus)
            {
                using (CSQLRelateTableDataContext db = new CSQLRelateTableDataContext(m_connection))
                {
                    // 取出设备状态
                    var deleteDeviceStatus = db.CDeviceStatusDto.SingleOrDefault<CDeviceStatusDto>(s => s.id == deviceStatusDto.id);

                    if (null == deleteDeviceStatus)
                    {
                        return false;
                    }

                    db.CDeviceStatusDto.DeleteOnSubmit(deleteDeviceStatus);

                    // 大于该id的所有行实例的id递减1
                    List<CDeviceStatusDto> lst = db.CDeviceStatusDto.ToList();
                    foreach (CDeviceStatusDto dto in lst)
                    {
                        if (null != dto && dto.id > deviceStatusDto.id)
                        {
                            CDeviceStatusDto newDto = new CDeviceStatusDto
                            {
                                id = dto.id - 1,
                                devicecode = dto.devicecode,
                                warehouse = dto.warehouse,
                                iccode = dto.iccode,
                                tasktype = dto.tasktype,
                                isable = dto.isable,
                                isavailable = dto.isavailable,
                                devicemode = dto.devicemode,
                                deviceaddr = dto.deviceaddr,
                                devicetype = dto.devicetype,
                                halltype = dto.halltype,
                                instep = dto.instep,
                                outstep = dto.outstep,
                                runstep = dto.runstep,
                                warehousechange = dto.warehousechange,
                                devicelayer = dto.devicelayer,
                                region = dto.region,
                                prevnode = dto.prevnode,
                                currentnode = dto.currentnode,
                                queueprevnode = dto.queueprevnode,
                                queuecurrentnode = dto.queuecurrentnode, 
                                scrloc=dto.scrloc,
                                desccloc=dto.desccloc
                            };

                            db.CDeviceStatusDto.InsertOnSubmit(newDto);
                            db.CDeviceStatusDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion

        #region 删除SQLOtherTable数据表接口
        /// <summary>
        /// 删除计费tariff数据
        /// </summary>
        /// <param name="tariffDto"></param>
        private bool DeleteOnTable(CTariffDto tariffDto)
        {
            if (null == tariffDto)
            {
                return false;
            }

            lock (m_objLockTariff)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出计费
                    var deleteTariff = db.CTariffDto.SingleOrDefault<CTariffDto>(s => s.id == tariffDto.id);

                    if (null == deleteTariff)
                    {
                        return false;
                    }

                    db.CTariffDto.DeleteOnSubmit(deleteTariff);

                    // 大于该id的所有行实例的id递减1
                    List<CTariffDto> lst = db.CTariffDto.ToList();
                    foreach (CTariffDto dto in lst)
                    {
                        if (null != dto && dto.id > tariffDto.id)
                        {
                            CTariffDto newDto = new CTariffDto
                            {
                                id = dto.id - 1,
                                iccardtype = dto.iccardtype,
                                feetype = dto.feetype,
                                tariffdescp = dto.tariffdescp,
                                isworkday = dto.isworkday,
                                workpeakperiod = dto.workpeakperiod,
                                workdayquotafee = dto.workdayquotafee,
                                workpeakfirstunit = dto.workpeakfirstunit,
                                worknonpeakfirstunit = dto.worknonpeakfirstunit,
                                workpeakinunitfee = dto.workpeakinunitfee,
                                workpeakoutunitfee = dto.workpeakoutunitfee,
                                worknonpeakinunitfee = dto.worknonpeakinunitfee,
                                worknonpeakoutunitfee = dto.worknonpeakoutunitfee,
                                nonworkpeakperiod = dto.nonworkpeakperiod,
                                nonworkdayquotafee = dto.nonworkdayquotafee,
                                nonworkpeakfirstunit = dto.nonworkpeakfirstunit,
                                nonworknonpeakfirstunit = dto.nonworknonpeakfirstunit,
                                nonworkpeakinunitfee = dto.nonworkpeakinunitfee,
                                nonworkpeakoutunitfee = dto.nonworkpeakoutunitfee,
                                nonworknonpeakinunitfee = dto.nonworknonpeakinunitfee,
                                nonworknonpeakoutunitfee = dto.nonworknonpeakoutunitfee,
                                fee = dto.fee
                            };

                            db.CTariffDto.InsertOnSubmit(newDto);
                            db.CTariffDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除设备故障devicefault数据
        /// </summary>
        /// <param name="deviceFaultDto"></param>
        private bool DeleteOnTable(CDeviceFaultDto deviceFaultDto)
        {
            if (null == deviceFaultDto)
            {
                return false;
            }

            lock (m_objLockDeviceFault)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出设备故障
                    var deleteDeviceFault = db.CDeviceFaultDto.SingleOrDefault<CDeviceFaultDto>(s => s.id == deviceFaultDto.id);

                    if (null == deleteDeviceFault)
                    {
                        return false;
                    }

                    db.CDeviceFaultDto.DeleteOnSubmit(deleteDeviceFault);

                    // 大于该id的所有行实例的id递减1
                    List<CDeviceFaultDto> lst = db.CDeviceFaultDto.ToList();
                    foreach (CDeviceFaultDto dto in lst)
                    {
                        if (null != dto && dto.id > deviceFaultDto.id)
                        {
                            CDeviceFaultDto newDto = new CDeviceFaultDto
                            {
                                id = dto.id - 1,
                                faultdescp = dto.faultdescp,
                                faultaddress = dto.faultaddress,
                                color = dto.color,
                                isable = dto.isable,
                                devicecode = dto.devicecode,
                                warehouse = dto.warehouse
                            };

                            db.CDeviceFaultDto.InsertOnSubmit(newDto);
                            db.CDeviceFaultDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除语音sound数据
        /// </summary>
        /// <param name="soundDto"></param>
        private bool DeleteOnTable(CSoundDto soundDto)
        {
            if (null == soundDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                // 取出语音
                var deleteSoundDto = db.CSoundDto.SingleOrDefault<CSoundDto>(s => s.soundcode == soundDto.soundcode);

                if (null == deleteSoundDto)
                {
                    return false;
                }

                db.CSoundDto.DeleteOnSubmit(deleteSoundDto);
                db.SubmitChanges();
                return true;
            }
        }

        /// <summary>
        /// 删除工作队列workqueue数据
        /// </summary>
        /// <param name="workQueueDto"></param>
        private bool DeleteOnTable(CWorkQueueDto workQueueDto)
        {
            if (null == workQueueDto)
            {
                return false;
            }

            lock (m_objLockWorkQueue)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    // 取出工作队列实例
                    var deleteWorkQueue = db.CWorkQueueDto.SingleOrDefault<CWorkQueueDto>(s => s.id == workQueueDto.id);

                    if (null == deleteWorkQueue)
                    {
                        return false;
                    }

                    db.CWorkQueueDto.DeleteOnSubmit(deleteWorkQueue);

                    // 大于该id的所有行实例的id递减1
                    List<CWorkQueueDto> lst = db.CWorkQueueDto.ToList();
                    foreach (CWorkQueueDto dto in lst)
                    {
                        if (null != dto && dto.id > workQueueDto.id)
                        {
                            CWorkQueueDto newDto = new CWorkQueueDto
                            {
                                id = dto.id - 1,
                                devicecode = dto.devicecode,
                                warehouse = dto.warehouse,
                                iccode = dto.iccode,
                                swipecount = dto.swipecount,
                                waitsendtelegram = dto.waitsendtelegram,
                                priorityid = dto.priorityid,
                                tasktype=dto.tasktype
                            };

                            db.CWorkQueueDto.InsertOnSubmit(newDto);
                            db.CWorkQueueDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除所有队列
        /// </summary>
        public void DeleteAllQueueDto() 
        {
            lock (m_objLockWorkQueue)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    db.CWorkQueueDto.DeleteAllOnSubmit(db.CWorkQueueDto.Where(a=>true));
                    db.SubmitChanges();
                }
            }
        }

        /// <summary>
        /// 删除LED文本ledcontent数据
        /// </summary>
        /// <param name="ledContentDto"></param>
        private bool DeleteOnTable(CLedContentDto ledContentDto)
        {
            if (null == ledContentDto)
            {
                return false;
            }

            using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
            {
                // 取出语音
                var deleteLedContentDto = db.CLedContentDto.SingleOrDefault<CLedContentDto>(s => s.id == ledContentDto.id);

                if (null == deleteLedContentDto)
                {
                    return false;
                }

                db.CLedContentDto.DeleteOnSubmit(deleteLedContentDto);

                // 大于该id的所有行实例的id递减1
                List<CLedContentDto> lst = db.CLedContentDto.ToList();
                foreach (CLedContentDto dto in lst)
                {
                    if (null != dto && dto.id > ledContentDto.id)
                    {
                        CLedContentDto newDto = new CLedContentDto();
                        newDto.id = dto.id - 1;
                        newDto.ledcontent = dto.ledcontent;

                        db.CLedContentDto.InsertOnSubmit(newDto);
                        db.CLedContentDto.DeleteOnSubmit(dto);
                    }
                }

                db.SubmitChanges();
                return true;
            }
        }

        /// <summary>
        /// 删除工作队列workqueue数据列表
        /// </summary>
        /// <param name="workQueueDto"></param>
        public bool DeleteOnTable(List<CWorkQueueDto> lstWorkQueueDto)
        {
            if (null == lstWorkQueueDto || 1 > lstWorkQueueDto.Count)
            {
                return false;
            }

            lock (m_objLockWorkQueue)
            {
                using (CSQLOtherTableDataContext db = new CSQLOtherTableDataContext(m_connection))
                {
                    List<CWorkQueueDto> lst = db.CWorkQueueDto.ToList();
                    int nMinID = db.CWorkQueueDto.Count();
                    foreach (CWorkQueueDto workQueueDto in lstWorkQueueDto)
                    {// 取出工作队列实例
                        var deleteWorkQueue = db.CWorkQueueDto.SingleOrDefault<CWorkQueueDto>(s => s.id == workQueueDto.id);

                        if (null == deleteWorkQueue)
                        {
                            continue;
                        }
                        nMinID = Math.Min(nMinID, workQueueDto.id);
                        db.CWorkQueueDto.DeleteOnSubmit(deleteWorkQueue);
                        lst.Remove(deleteWorkQueue);
                    }

                    // 大于该id的所有行实例的id从nMinID开始计算
                    lst = lst.FindAll(dto => dto.id > nMinID);
                    foreach (CWorkQueueDto dto in lst)
                    {
                        CWorkQueueDto newDto = new CWorkQueueDto
                        {
                            id = nMinID++,
                            devicecode = dto.devicecode,
                            warehouse = dto.warehouse,
                            iccode = dto.iccode,
                            swipecount = dto.swipecount,
                            waitsendtelegram = dto.waitsendtelegram,
                            priorityid = dto.priorityid
                        };

                        db.CWorkQueueDto.InsertOnSubmit(newDto);
                        db.CWorkQueueDto.DeleteOnSubmit(dto);
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion

        #region 删除SQLLogTable数据表接口
        /// <summary>
        /// 删除IC卡缴费日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool DeleteOnTable(CICCardLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockICCardLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出IC卡缴费日志
                    var deleteLog = db.CICCardLogDto.SingleOrDefault<CICCardLogDto>(s => s.id == logDto.id);

                    if (null == deleteLog)
                    {
                        return false;
                    }

                    db.CICCardLogDto.DeleteOnSubmit(deleteLog);

                    // 大于该id的所有行实例的id递减1
                    List<CICCardLogDto> lst = db.CICCardLogDto.ToList();
                    foreach (CICCardLogDto dto in lst)
                    {
                        if (null != dto && dto.id > logDto.id)
                        {
                            CICCardLogDto newDto = new CICCardLogDto
                            {
                                id = dto.id - 1,
                                iccode = dto.iccode,
                                ictype = dto.ictype,
                                paymenttime = dto.paymenttime,
                                starttime = dto.starttime,
                                endtime = dto.endtime,
                                calculatedays = dto.calculatedays,
                                feetype = dto.feetype,
                                tariffnorm = dto.tariffnorm,
                                payablefee = dto.payablefee,
                                actualfee = dto.actualfee,
                                username = dto.username,
                                optcode = dto.optcode
                            };

                            db.CICCardLogDto.InsertOnSubmit(newDto);
                            db.CICCardLogDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除系统日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool DeleteOnTable(CSystemLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockSystemLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出系统日志
                    var deleteLog = db.CSystemLogDto.SingleOrDefault<CSystemLogDto>(s => s.logid == logDto.logid);

                    if (null == deleteLog)
                    {
                        return false;
                    }

                    db.CSystemLogDto.DeleteOnSubmit(deleteLog);

                    // 大于该id的所有行实例的id递减1
                    List<CSystemLogDto> lst = db.CSystemLogDto.ToList();
                    foreach (CSystemLogDto dto in lst)
                    {
                        if (null != dto && dto.logid > logDto.logid)
                        {
                            CSystemLogDto newDto = new CSystemLogDto
                            {
                                logid = dto.logid - 1,
                                logdescp = dto.logdescp,
                                curtime = logDto.curtime,
                                optcode = dto.optcode,
                                optname = dto.optname
                            };

                            db.CSystemLogDto.InsertOnSubmit(newDto);
                            db.CSystemLogDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除报文日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool DeleteOnTable(CTelegramLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockTelegramLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出报文日志
                    var deleteLog = db.CTelegramLogDto.SingleOrDefault<CTelegramLogDto>(s => s.id == logDto.id);

                    if (null == deleteLog)
                    {
                        return false;
                    }

                    db.CTelegramLogDto.DeleteOnSubmit(deleteLog);

                    // 大于该id的所有行实例的id递减1
                    List<CTelegramLogDto> lst = db.CTelegramLogDto.ToList();
                    foreach (CTelegramLogDto dto in lst)
                    {
                        if (null != dto && dto.id > logDto.id)
                        {
                            CTelegramLogDto newDto = new CTelegramLogDto
                            {
                                id = dto.id - 1,
                                time = dto.time,
                                logtype = dto.logtype,
                                warehouse = dto.warehouse,
                                telegramhead = dto.telegramhead,
                                devicecode = dto.devicecode,
                                iccode = dto.iccode,
                                carinfo = dto.carinfo,
                                slotinfo = dto.slotinfo,
                                destdirection = dto.destdirection,
                                direction = dto.direction,
                                srcaddr = dto.srcaddr,
                                destaddr = dto.destaddr,
                                telegramid = dto.telegramid

                            };

                            db.CTelegramLogDto.InsertOnSubmit(newDto);
                            db.CTelegramLogDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除设备状态日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool DeleteOnTable(CDeviceStatusLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockDeviceStatusLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出设备状态日志
                    var deleteLog = db.CDeviceStatusLogDto.SingleOrDefault<CDeviceStatusLogDto>(s => s.id == logDto.id);

                    if (null == deleteLog)
                    {
                        return false;
                    }

                    db.CDeviceStatusLogDto.DeleteOnSubmit(deleteLog);

                    // 大于该id的所有行实例的id递减1
                    List<CDeviceStatusLogDto> lst = db.CDeviceStatusLogDto.ToList();
                    foreach (CDeviceStatusLogDto dto in lst)
                    {
                        if (null != dto && dto.id > logDto.id)
                        {
                            CDeviceStatusLogDto newDto = new CDeviceStatusLogDto
                            {
                                id = dto.id - 1,
                                warehouse = dto.warehouse,
                                devicecode = dto.devicecode,
                                deviceaddr = dto.deviceaddr,
                                time = dto.time,
                                devicemode = dto.devicemode,
                                devicetype = dto.devicetype,
                                halltype = dto.halltype,
                                curtasktype = dto.curtasktype,
                                isavailable = dto.isavailable,
                                isgetoutcar = dto.isgetoutcar,
                                instep = dto.instep,
                                outstep = dto.outstep,
                                isable = dto.isable,
                                runstep = dto.runstep
                            };

                            db.CDeviceStatusLogDto.InsertOnSubmit(newDto);
                            db.CDeviceStatusLogDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }

        /// <summary>
        /// 删除设备故障日志systemlog数据
        /// </summary>
        /// <param name="logDto"></param>
        private bool DeleteOnTable(CDeviceFaultLogDto logDto)
        {
            if (null == logDto)
            {
                return false;
            }

            lock (m_objLockDeviceFaultLog)
            {
                using (CSQLLogTableDataContext db = new CSQLLogTableDataContext(m_connection))
                {
                    // 取出设备故障日志
                    var deleteLog = db.CDeviceFaultLogDto.SingleOrDefault<CDeviceFaultLogDto>(s => s.id == logDto.id);

                    if (null == deleteLog)
                    {
                        return false;
                    }

                    db.CDeviceFaultLogDto.DeleteOnSubmit(deleteLog);

                    // 大于该id的所有行实例的id递减1
                    List<CDeviceFaultLogDto> lst = db.CDeviceFaultLogDto.ToList();
                    foreach (CDeviceFaultLogDto dto in lst)
                    {
                        if (null != dto && dto.id > logDto.id)
                        {
                            CDeviceFaultLogDto newDto = new CDeviceFaultLogDto
                            {
                                id = dto.id - 1,
                                warehouse = dto.warehouse,
                                devicecode = dto.devicecode,
                                faultdescp = dto.faultdescp,
                                time = dto.time,
                                optcode = dto.optcode,
                                color = dto.color
                            };

                            db.CDeviceFaultLogDto.InsertOnSubmit(newDto);
                            db.CDeviceFaultLogDto.DeleteOnSubmit(dto);
                        }
                    }

                    db.SubmitChanges();
                    return true;
                }
            }
        }
        #endregion
    }
}
