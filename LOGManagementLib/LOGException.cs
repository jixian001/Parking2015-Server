using System;
using System.IO;
using System.Text;

namespace LOGManagementLib
{
    /// <summary>
    /// 日志管理类（业务、异常及数据库日志记录）
    /// </summary>
    public class CLOGException
    {
        private static readonly string m_lpath = System.AppDomain.CurrentDomain.BaseDirectory + "BusinessLog";
        private static readonly string m_epath = System.AppDomain.CurrentDomain.BaseDirectory + "ErrorLog";
        private static readonly string m_spath = System.AppDomain.CurrentDomain.BaseDirectory + "SQLlog";
        private static readonly bool m_isWriteSQLLog = true;// 写入数据库日志开关（true->写入）读取配置文件
        public static int m_logFileMaxSize = 10485760;// 日志文件大小最大值默认值：10MB = 10 * 1024 * 1024  = 10485760byte

        #region 写入数据库日志
        /// <summary>
        /// 写入数据库日志文本
        /// </summary>
        /// <param name="data"></param>
        /// <param name="title"></param>
        public static void WriteSQLLog(object data, string title)
        {
            if (!m_isWriteSQLLog || null == data)
            {
                return;
            }

            // 打开写入数据库日志开关才写入日志
            try
            {
                string slog = string.Empty;
                if (data.GetType() == typeof(string))
                {
                    slog = (string)data;
                }
                else
                {
                    slog = data.ToString();
                }

                string datatxt = "\\" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
                string logPath = m_spath + datatxt + ".csv";

                bool isExist = false;
                if (File.Exists(logPath))
                {
                    isExist = true;
                }

                using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    if (m_logFileMaxSize <= fs.Length)
                    {
                        if (File.Exists(logPath))
                        {
                            fs.Flush();
                            fs.Close();
                            File.Delete(logPath);
                            return;
                        }
                    }

                    using (StreamWriter swFromFileStream = new StreamWriter(fs,Encoding.GetEncoding("GB2312")))
                    {
                        if (swFromFileStream != null)
                        {
                            if (!isExist)
                            {
                                swFromFileStream.WriteLine("Date,TableName,TableData");
                            }

                            swFromFileStream.WriteLine(string.Format("{0:^yyyy/MM/dd HH:mm:ss.fff},{1},{2}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), title, slog));
                            swFromFileStream.Flush();
                            fs.Flush();
                            swFromFileStream.Close();
                            fs.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    if (System.IO.Directory.Exists(m_spath) == false)//如果不存在就创建file文件夹
                    {
                        System.IO.Directory.CreateDirectory(m_spath);
                    }

                    //throw new Exception("写入数据库日志:" + e1.ToString());
                }
                catch (Exception)
                { 
                    //throw e2;
                }
            }
        }
        #endregion

        #region  trace
        /// <summary>
        /// lua脚本日志打印函数
        /// </summary>
        /// <param name="objErrorInfo">要打印的字符串</param>
        public static void Trace(object objErrorInfo)
        {
            try
            {
                string strErrorInfo = string.Empty;

                if (null == objErrorInfo)
                {
                    strErrorInfo = "要打印的日志为空，无法打印。";
                }
                else if (objErrorInfo.GetType() == typeof(long))
                {
                    strErrorInfo = objErrorInfo.ToString();
                }
                else if (objErrorInfo.GetType() == typeof(string))
                {
                    strErrorInfo = (string)objErrorInfo;
                }
                else
                {
                    return;
                }


                string strTraceFileDir = string.Format("./Trace");
                if (!Directory.Exists(strTraceFileDir))
                {
                    Directory.CreateDirectory(strTraceFileDir);
                }

                string strSearchPattern = string.Format("trace_common*");
                string[] strAllTraceFiles = Directory.GetFiles(strTraceFileDir, strSearchPattern, SearchOption.TopDirectoryOnly);
                if (0 == strAllTraceFiles.Length)
                {
                    string strTraceFileName1 = string.Format("{0}/trace_common_{1}.txt", strTraceFileDir, 1);
                    using (FileStream fs = new FileStream(strTraceFileName1, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        if (m_logFileMaxSize <= fs.Length)
                        {
                            if (File.Exists(strTraceFileName1))
                            {
                                fs.Flush();
                                fs.Close();
                                return;
                            }
                        }

                        using (StreamWriter swFromFileStream = new StreamWriter(fs))
                        {
                            if (strErrorInfo.Contains("Start programme"))
                            {
                                swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            else if (strErrorInfo.Contains("Stop programme"))
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                            }
                            else
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            swFromFileStream.Flush();
                            fs.Flush();
                            swFromFileStream.Close();
                            fs.Close();
                        }
                    }
                    return;
                }
                string strOldestFileName = string.Empty;
                DateTime dtOldestFileTime = DateTime.Now;

                string strOlderFileName = string.Empty;
                DateTime dtOlderFileTime = DateTime.Now;

                string strNewestFileName = string.Empty;
                DateTime dtNewestFileTime = new DateTime();
                int nTraceFileNo = 0;
                foreach (string strFileName in strAllTraceFiles)
                {
                    nTraceFileNo++;

                    string strTmpFileName = string.Empty;
                    DateTime dtTmpTime = new DateTime();

                    dtTmpTime = File.GetLastWriteTime(strFileName);
                    strTmpFileName = strFileName;

                    //当前文件的修改时间早于最老的文件则将当前文件的信息赋值给最老文件
                    if (dtOldestFileTime > dtTmpTime)
                    {
                        strOldestFileName = strTmpFileName;
                        dtOldestFileTime = dtTmpTime;
                    }
                    if (dtTmpTime > dtNewestFileTime)//当前文件的修改时间晚于最新的文件则将当前文件的信息赋值给最新文件
                    {
                        strNewestFileName = strTmpFileName;
                        dtNewestFileTime = dtTmpTime;
                    }

                }

                using (FileStream fs = new FileStream(strNewestFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    if (m_logFileMaxSize <= fs.Length)
                    {
                        if (File.Exists(strNewestFileName))
                        {
                            fs.Flush();
                            fs.Close();
                            if (nTraceFileNo >= 5)
                            {
                                //string strNewFileName = strTraceFileDir + "/" + strOldestFileName;
                                File.Delete(strOldestFileName);
                                File.AppendAllText(strOldestFileName, DateTime.Now + " start" + Environment.NewLine);
                            }
                            else
                            {
                                string strIndex = ".txt";//"trace_common" + "_";
                                int nStartIndex = strNewestFileName.IndexOf(strIndex);
                                int nFileNo = Convert.ToInt32(strNewestFileName.Substring(nStartIndex - 1, 1));
                                strNewestFileName = strNewestFileName.Replace(nFileNo + ".txt", nFileNo + 1 + ".txt");

                                File.AppendAllText(strNewestFileName, DateTime.Now + "start");
                            }
                            return;
                        }
                    }

                    using (StreamWriter swFromFileStream = new StreamWriter(fs))
                    {
                        if (strErrorInfo.Contains("Start programme"))
                        {
                            swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        else if (strErrorInfo.Contains("Stop programme"))
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                        }
                        else
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        swFromFileStream.Flush();
                        fs.Flush();
                        swFromFileStream.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Trace(string objModuleName, object objErrorInfo)
        {
            try
            {
                string strErrorInfo = string.Empty;

                if (null == objErrorInfo)
                {
                    strErrorInfo = string.Format("({0})：要打印的日志为空，无法打印。", objModuleName);
                }
                else if (objErrorInfo.GetType() == typeof(long) || objErrorInfo.GetType() == typeof(int))
                {
                    strErrorInfo = "(" + objModuleName + ")：" + objErrorInfo.ToString();
                }
                else if (objErrorInfo.GetType() == typeof(string))
                {
                    strErrorInfo = string.Format("({0})：{1}", objModuleName, (string)objErrorInfo);
                }
                else
                {
                    return;
                }

                string strTraceFileDir = string.Format("./Trace");
                if (!Directory.Exists(strTraceFileDir))
                {
                    Directory.CreateDirectory(strTraceFileDir);
                }

                string strSearchPattern = string.Format("trace_common*");
                string[] strAllTraceFiles = Directory.GetFiles(strTraceFileDir, strSearchPattern, SearchOption.TopDirectoryOnly);
                if (0 == strAllTraceFiles.Length)
                {
                    string strTraceFileName1 = string.Format("{0}/trace_common_{1}.txt", strTraceFileDir, 1);
                    using (FileStream fs = new FileStream(strTraceFileName1, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        if (m_logFileMaxSize <= fs.Length)
                        {
                            if (File.Exists(strTraceFileName1))
                            {
                                fs.Flush();
                                fs.Close();
                                return;
                            }
                        }

                        using (StreamWriter swFromFileStream = new StreamWriter(fs))
                        {
                            if (strErrorInfo.Contains("Start programme"))
                            {
                                swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            else if (strErrorInfo.Contains("Stop programme"))
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                            }
                            else
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            swFromFileStream.Flush();
                            fs.Flush();
                            swFromFileStream.Close();
                            fs.Close();
                        }
                    }
                    return;
                }
                string strOldestFileName = string.Empty;
                DateTime dtOldestFileTime = DateTime.Now;

                string strOlderFileName = string.Empty;
                DateTime dtOlderFileTime = DateTime.Now;

                string strNewestFileName = string.Empty;
                DateTime dtNewestFileTime = new DateTime();
                int nTraceFileNo = 0;
                foreach (string strFileName in strAllTraceFiles)
                {
                    nTraceFileNo++;

                    string strTmpFileName = string.Empty;
                    DateTime dtTmpTime = new DateTime();

                    dtTmpTime = File.GetLastWriteTime(strFileName);
                    strTmpFileName = strFileName;

                    //当前文件的修改时间早于最老的文件则将当前文件的信息赋值给最老文件
                    if (dtOldestFileTime > dtTmpTime)
                    {
                        strOldestFileName = strTmpFileName;
                        dtOldestFileTime = dtTmpTime;
                    }
                    if (dtTmpTime > dtNewestFileTime)//当前文件的修改时间晚于最新的文件则将当前文件的信息赋值给最新文件
                    {
                        strNewestFileName = strTmpFileName;
                        dtNewestFileTime = dtTmpTime;
                    }

                }

                using (FileStream fs = new FileStream(strNewestFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    if (m_logFileMaxSize <= fs.Length)
                    {
                        if (File.Exists(strNewestFileName))
                        {
                            fs.Flush();
                            fs.Close();
                            if (nTraceFileNo >= 5)
                            {
                                //string strNewFileName = strTraceFileDir + "/" + strOldestFileName;
                                File.Delete(strOldestFileName);
                                File.AppendAllText(strOldestFileName, DateTime.Now + " start" + Environment.NewLine);
                            }
                            else
                            {
                                string strIndex = ".txt";//"trace_common" + "_";
                                int nStartIndex = strNewestFileName.IndexOf(strIndex);
                                int nFileNo = Convert.ToInt32(strNewestFileName.Substring(nStartIndex - 1, 1));
                                strNewestFileName = strNewestFileName.Replace(nFileNo + ".txt", nFileNo + 1 + ".txt");

                                File.AppendAllText(strNewestFileName, DateTime.Now + "start");
                            }
                            return;
                        }
                    }

                    using (StreamWriter swFromFileStream = new StreamWriter(fs))
                    {
                        if (strErrorInfo.Contains("Start programme"))
                        {
                            swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        else if (strErrorInfo.Contains("Stop programme"))
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                        }
                        else
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        swFromFileStream.Flush();
                        fs.Flush();
                        swFromFileStream.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Trace(int nPLCID, string objModuleName, object objErrorInfo)
        {
            try
            {
                string strErrorInfo = string.Empty;
                if (null == objErrorInfo)
                {
                    strErrorInfo = string.Format("({0})：要打印的日志为空，无法打印。", objModuleName);
                }
                else if (objErrorInfo.GetType() == typeof(long) || objErrorInfo.GetType() == typeof(int))
                {
                    strErrorInfo = "(" + objModuleName + ")：" + objErrorInfo.ToString();
                }
                else if (objErrorInfo.GetType() == typeof(string))
                {
                    strErrorInfo = string.Format("({0})：{1}", objModuleName, (string)objErrorInfo);
                }
                else
                {
                    return;
                }
                //modify by suhan 20150729
                string strTraceFileDir = string.Format("./Trace");
                //string strTraceFileDir = string.Format("./Trace/库区{0}", nPLCID);
                //end
                if (!Directory.Exists(strTraceFileDir))
                {
                    Directory.CreateDirectory(strTraceFileDir);
                }
                string strSearchPattern = string.Format("ap_trace_PLC{0}*", nPLCID);
                string[] strAllTraceFiles = Directory.GetFiles(strTraceFileDir, strSearchPattern, SearchOption.TopDirectoryOnly);
                if (0 == strAllTraceFiles.Length)
                {
                    string strTraceFileName1 = string.Format("{0}/ap_trace_PLC{1}_{2}.txt", strTraceFileDir, nPLCID, 1);
                    using (FileStream fs = new FileStream(strTraceFileName1, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        if (m_logFileMaxSize <= fs.Length)
                        {
                            if (File.Exists(strTraceFileName1))
                            {
                                fs.Flush();
                                fs.Close();
                                return;
                            }
                        }

                        using (StreamWriter swFromFileStream = new StreamWriter(fs))
                        {
                            if (strErrorInfo.Contains("Start programme"))
                            {
                                swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            else if (strErrorInfo.Contains("Stop programme"))
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                            }
                            else
                            {
                                swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                            }
                            swFromFileStream.Flush();
                            fs.Flush();
                            swFromFileStream.Close();
                            fs.Close();
                        }
                    }
                    return;
                }
                string strOldestFileName = string.Empty;
                DateTime dtOldestFileTime = DateTime.Now;

                string strOlderFileName = string.Empty;
                DateTime dtOlderFileTime = DateTime.Now;

                string strNewestFileName = string.Empty;
                DateTime dtNewestFileTime = new DateTime();
                int nTraceFileNo = 0;
                foreach (string strFileName in strAllTraceFiles)
                {
                    nTraceFileNo++;

                    string strTmpFileName = string.Empty;
                    DateTime dtTmpTime = new DateTime();

                    dtTmpTime = File.GetLastWriteTime(strFileName);
                    strTmpFileName = strFileName;

                    //当前文件的修改时间早于最老的文件则将当前文件的信息赋值给最老文件
                    if (dtOldestFileTime > dtTmpTime)
                    {
                        strOldestFileName = strTmpFileName;
                        dtOldestFileTime = dtTmpTime;
                    }
                    if (dtTmpTime > dtNewestFileTime)//当前文件的修改时间晚于最新的文件则将当前文件的信息赋值给最新文件
                    {
                        strNewestFileName = strTmpFileName;
                        dtNewestFileTime = dtTmpTime;
                    }

                }
                //             string strTraceFileName = strTraceFileDir + "/" + strNewestFileName;
                using (FileStream fs = new FileStream(strNewestFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    if (m_logFileMaxSize <= fs.Length)
                    {
                        if (File.Exists(strNewestFileName))
                        {
                            fs.Flush();
                            fs.Close();
                            if (nTraceFileNo >= 5)
                            {
                                //string strNewFileName = strTraceFileDir + "/" + strOldestFileName;
                                File.Delete(strOldestFileName);
                                File.AppendAllText(strOldestFileName, DateTime.Now + " start" + Environment.NewLine);
                            }
                            else
                            {
                                string strIndex = ".txt";//"ap_trace_PLC" + nPLCID + "_";
                                int nStartIndex = strNewestFileName.IndexOf(strIndex);
                                int nFileNo = Convert.ToInt32(strNewestFileName.Substring(nStartIndex - 1, 1));
                                strNewestFileName = strNewestFileName.Replace(nFileNo + ".txt", nFileNo + 1 + ".txt");

                                File.AppendAllText(strNewestFileName, DateTime.Now + "start");
                            }
                            return;
                        }
                    }

                    using (StreamWriter swFromFileStream = new StreamWriter(fs))
                    {
                        if (strErrorInfo.Contains("Start programme"))
                        {
                            swFromFileStream.Write(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        else if (strErrorInfo.Contains("Stop programme"))
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine + Environment.NewLine);
                        }
                        else
                        {
                            swFromFileStream.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + strErrorInfo + Environment.NewLine);
                        }
                        swFromFileStream.Flush();
                        fs.Flush();
                        swFromFileStream.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
