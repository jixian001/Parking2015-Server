using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Data;
using System.IO.Compression;

namespace BaseMethodLib
{
    public sealed class CBaseMethods
    {

        private static readonly CBaseMethods m_commonMehtods = new CBaseMethods();

        /// <summary>
        /// 全局静态CConfigManagement成员
        /// </summary>
        public static CBaseMethods MyBase
        {
            get
            {
                return m_commonMehtods;
            }
        }

        /// <summary>
        /// 字符串是否是无符号数字
        /// 是  ：返回true
        /// 不是：返回false
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool IsUnsignedNumber(String strNumber)
        {
            if (string.IsNullOrWhiteSpace(strNumber))
            {
                return false;
            }
            string strTmp = strNumber.Trim();
            if (!strTmp.All(c => char.IsNumber(c)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 字符串是否是无符号数字
        /// 是  ：返回true --> nNumber返回的值是配置的无符号数字
        /// 不是：返回false --> nNumber返回的值是0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool StringToUInt32(String strNumber, out int nNumber)
        {
            nNumber = 0;
            if (string.IsNullOrWhiteSpace(strNumber))
            {
                return false;
            }
            string strTmp = strNumber.Trim();
            if (!strTmp.All(c => char.IsNumber(c)))
            {
                return false;
            }
            return int.TryParse(strTmp, out nNumber);
        }

        /// <summary>
        /// 字符串是否是无符号数字
        /// 是  ：返回true --> nNumber返回的值是配置的无符号数字
        /// 不是：返回false --> nNumber返回的值是0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool StringToUInt16(String strNumber, out Int16 nNumber)
        {
            nNumber = 0;
            if (string.IsNullOrWhiteSpace(strNumber))
            {
                return false;
            }
            string strTmp = strNumber.Trim();
            if (!strTmp.All(c => char.IsNumber(c)))
            {
                return false;
            }
            return Int16.TryParse(strTmp, out nNumber);
        }

        /// <summary>
        /// 字符串是否是无符号数字
        /// 是  ：返回true --> nNumber返回的值是配置的无符号数字
        /// 不是：返回false --> nNumber返回的值是0
        /// add by wanxiaona201506
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool StringToDouble(String strNumber, out double dNumber)
        {
            dNumber = 0.0;
            if (string.IsNullOrWhiteSpace(strNumber))
            {
                return false;
            }
            string strTmp = strNumber.Trim();
            for (int i = 0; i < strTmp.Length; i++)
            {
                if (!char.IsNumber(strTmp[i]) && strTmp[i] != '.')
                {// 非数字和小数点时退出
                    return false;
                }
            }
            return double.TryParse(strTmp, out dNumber);
        }

        /// <summary>
        /// 判断字符串是否是非0的数字
        /// 是  ：返回true
        /// 不是：返回false
        /// added by suhan01318154 20141202
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool IsUIntNumWithoutZero(String str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] <= '0' || str[i] > '9')
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 判断字符串是否是非0的数字
        /// 是  ：返回true
        /// 不是：返回false
        /// added by suhan01318154 20141202
        /// </summary>
        /// <param name="nSrc"></param>
        /// <returns></returns>
        public bool IsUIntNumWithoutZero(int nSrc)
        {
            string str = nSrc.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] <= '0' || str[i] > '9')
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断源字符串是否为空，为空返回true，不为空返回false
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        public bool IsEmpty(string strSrc)
        {
            if (null == strSrc || string.Empty == strSrc.Trim())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断int是否为初始
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        public bool IsEmpty(int? enmType)
        {
            if (null == enmType || 0 == enmType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测车辆尺寸算法
        /// 返回值
        /// 成功：true(strCarSize 小于等于strCarLocSize)
        /// 失败：false(strCarSize 大于 strCarLocSize)
        /// </summary>
        /// <param name="strCarSize">车辆尺寸</param>
        /// <param name="strCarLocSize">目的车位尺寸</param>
        /// <returns>返回车辆尺寸是否符合</returns>true表示正确
        public bool CheckCarSize(string strCarSize, string strCarLocSize)
        {
            if (string.IsNullOrWhiteSpace(strCarSize) || string.IsNullOrWhiteSpace(strCarLocSize))
            {
                return false;
            }

            // 车辆型号参数长度不正确或者不为正整数
            if (3 != strCarSize.Length || !IsUIntNumWithoutZero(strCarSize) ||
                3 != strCarLocSize.Length || !IsUIntNumWithoutZero(strCarLocSize))
            {
                return false;
            }

            int nCarLong = Convert.ToInt32(strCarSize.Substring(0, 1));
            int nCarWide = Convert.ToInt32(strCarSize.Substring(1, 1));
            int nCarHigh = Convert.ToInt32(strCarSize.Substring(2, 1));
            if (nCarLong > Convert.ToInt32(strCarLocSize.Substring(0, 1)) ||
                nCarWide > Convert.ToInt32(strCarLocSize.Substring(1, 1)) ||
                nCarHigh > Convert.ToInt32(strCarLocSize.Substring(2, 1)))
            {
                // 车辆长宽高有一个超过最大的尺寸
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取异常文本内容
        /// </summary>
        /// <returns></returns>
        public string GetExceptionInfo(Exception exception)
        {
            return exception.ToString();
        }

        /// <summary>
        /// 根据报文类型确定是否ETV设备
        /// </summary>
        /// <param name="strTelegramType"></param>
        /// <returns></returns>
        public bool IsDeviceETV(string strTelegramType)
        {
            string str = "13-1,11-1,14-1,12-1";
            return str.Contains(strTelegramType);
        }

        #region 时间处理
        /// <summary>
        /// 字符串转换成时间
        /// </summary>
        /// <param name="strSrc"></param>
        /// <param name="dtDest"></param>
        /// <param name="strFormat"></param>
        /// <returns></returns>
        public bool StringToDateTime(string strSrc, out DateTime dtDest, string strFormat)
        {

            dtDest = new DateTime();
            try
            {
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                dtFormat.ShortDatePattern = strFormat;//"yyyy/MM/dd HH:mm:ss";
                dtDest = Convert.ToDateTime(strSrc, dtFormat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DateTimeToString(DateTime dtSrc, out string strDest, string strFormat)
        {
            strDest = string.Empty;
            try
            {
                strDest = dtSrc.ToString(strFormat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断dtCur当前时间是否在stStart开始时间和dtEnd截止时间之间
        /// </summary>
        /// <param name="dtCur"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public bool CompareDateTime(DateTime? dtCur, DateTime dtStart, DateTime dtEnd)
        {
            if (null == dtCur)
            {
                return false;
            }

            // DateTime.CompareTo值说明小于零此实例早于 value。零此实例与 value 相同。大于零此实例晚于 value。
            if (dtCur.Value.CompareTo(dtStart) > 0 && dtCur.Value.CompareTo(dtEnd) < 0)
            {
                return true;
            }

            return false;
        }
        #endregion 时间处理

        #region 报文中函数
        /// <summary>
        /// 对比二个对象数据是否相等
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool IsEqualValue(object objOld, object objNew)
        {
            if (objOld.GetType() != objNew.GetType())
            {
                return false;
            }

            if (typeof(Int16) == objOld.GetType())
            {
                if ((Int16)objOld != (Int16)objNew)
                {
                    return false;
                }

                return true;
            }
            else if (typeof(Int16[]) == objOld.GetType())
            {
                Int16[] intsOld = (Int16[])objOld;
                Int16[] intsNew = (Int16[])objNew;
                if (intsOld.Count() != intsNew.Count())
                {
                    return false;
                }

                for (int i = 0; i < intsOld.Count(); i++)
                {
                    if (intsOld[i] != intsNew[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (typeof(byte[]) == objOld.GetType())
            {
                byte[] bytesOld = (byte[])objOld;
                byte[] bytesNew = (byte[])objNew;
                if (bytesOld.Count() != bytesNew.Count())
                {
                    return false;
                }

                for (int i = 0; i < bytesOld.Count(); i++)
                {
                    if (bytesOld[i] != bytesNew[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断发送的报文与PLC回应的ACK是否一致,返回值：true-收到ACK，false-未收到ACK
        /// </summary>
        /// <param name="sendTelegram"></param>
        /// <param name="ackTelegram"></param>
        /// <returns></returns>
        public bool IsPLCAck(Int16[] sendTelegram, Int16[] ackTelegram)
        {
            if (null == sendTelegram || null == ackTelegram || sendTelegram.Length != ackTelegram.Length || ackTelegram.Length < 5)
            {
                return false;
            }

            if (9999 != ackTelegram[4])
            {// 9999->ack[4]
                return false;
            }

            for (int i = 0; i < sendTelegram.Length; i++)
            {
                if (i != 4 && sendTelegram[i] != ackTelegram[i])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 计费临时卡
        /// <summary>
        /// 转换高峰时段字符串为对应起始和截止时间
        /// </summary>
        /// <param name="strTimePeriod"></param>
        /// <param name="tsPeakStart"></param>
        /// <param name="tsPeakEnd"></param>
        public void ConvertTimePeriod(string strTimePeriod, out TimeSpan tsPeakStart, out TimeSpan tsPeakEnd)
        {
            if (null == strTimePeriod)
            {
                strTimePeriod = string.Empty;
            }

            int index = strTimePeriod.IndexOf("-");
            if (-1 != index)
            {
                string strStart = strTimePeriod.Substring(0, index);
                TimeSpan.TryParse(strTimePeriod.Substring(0, index), out tsPeakStart);
                TimeSpan.TryParse(strTimePeriod.Substring(index + 1), out tsPeakEnd);
            }
            else
            {
                TimeSpan.TryParse("00:00:00", out tsPeakStart);
                TimeSpan.TryParse("23:59:59", out tsPeakEnd);
            }
        }

        /// <summary>
        /// 转换字符串单位为以分钟为单元的具体值
        /// </summary>
        /// <param name="strUnit"></param>
        /// <returns></returns>
        public float ConvertUnitToInt(string strUnit, out int nUnit)
        {
            if (null == strUnit)
            {
                strUnit = string.Empty;
            }

            float nUnitFee = 0;
            nUnit = 30;// 默认30分钟
            int nIndex = strUnit.IndexOf("/");
            string strUnitFee = string.Empty;

            // 获取单价值
            foreach (char c in strUnit)
            {
                if (!char.IsNumber(c) && '.' != c)
                {
                    break;
                }
                // 数字或者小数点
                strUnitFee += c;
            }
            if (!string.IsNullOrWhiteSpace(strUnitFee))
            {
                nUnitFee = (float)Convert.ToDouble(strUnitFee);
            }

            // 获取单位值
            strUnit = strUnit.Substring(nIndex + 1);
            if (strUnit.Contains("分钟"))
            {
                nIndex = strUnit.IndexOf("分钟");
                strUnit = strUnit.Replace("分钟", "");
                if (string.IsNullOrWhiteSpace(strUnit))
                {
                    nUnit = 1;
                }
                else
                {
                    CBaseMethods.MyBase.StringToUInt32(strUnit, out nUnit);
                }
            }
            else if (strUnit.Contains("小时"))
            {
                nIndex = strUnit.IndexOf("小时");
                strUnit = strUnit.Replace("小时", "");
                if (string.IsNullOrWhiteSpace(strUnit))
                {
                    nUnit = 1;
                }
                else
                {
                    CBaseMethods.MyBase.StringToUInt32(strUnit, out nUnit);
                }

                nUnit = nUnit * 60;
            }
            return nUnitFee;
        }
        #endregion

        #region DataSet与二进制byte互转
        /// <summary>
        /// DataSet转二进制byte
        /// MemoryStream: 创建其支持存储区为内存的流。
        /// IFormatter : 提供将序列化对象格式化的功能。
        /// </summary>
        /// <param name="dsOriginal">DataSet数据集对象</param>
        /// <returns>字节数组</returns>
        public byte[] GetBinaryFormatData(DataSet dsOriginal)
        {
            byte[] binaryDataResult = null;
            if (null == dsOriginal)
            {
                return binaryDataResult;
            }

            MemoryStream memStream = new MemoryStream();
            //以二进制格式将对象或整个连接对象图形序列化和反序列化。
            IFormatter brFormatter = new BinaryFormatter();
            //dsOriginal.RemotingFormat 为远程处理期间使用的DataSet 获取或设置 SerializtionFormat        
            //SerializationFormat.Binary      将字符串比较方法设置为使用严格的二进制排序顺序
            dsOriginal.RemotingFormat = SerializationFormat.Binary;
            //把字符串以二进制放进memStream中
            brFormatter.Serialize(memStream, dsOriginal);
            //转为byte数组
            binaryDataResult = memStream.ToArray();
            memStream.Close();
            memStream.Dispose();
            return binaryDataResult;
        }

        /// <summary>
        /// 二进制byte转DataSet
        /// 将字节数组反序列化成DataSet对象
        /// </summary>
        /// <param name="binaryData">字节数组</param>
        /// <returns>DataSet数据集对象</returns>
        public DataSet RetrieveDataSet(byte[] binaryData)
        {
            DataSet ds = null;
            if (null == binaryData || 0 >= binaryData.Length)
            {
                return ds;
            }
            MemoryStream memStream = new MemoryStream(binaryData, true);
            IFormatter brFormatter = new BinaryFormatter();
            ds = (DataSet)brFormatter.Deserialize(memStream);
            return ds;
        }

        /// <summary>
        /// 压缩DataSet
        /// MemoryStream: 创建其支持存储区为内存的流。
        /// IFormatter : 提供将序列化对象格式化的功能。
        /// </summary>
        /// <param name="dsOriginal">DataSet数据集对象</param>
        /// <returns>内存流</returns>
        public MemoryStream CompressDataSet(DataSet dsOriginal)
        {
            MemoryStream outStream = new MemoryStream();
            if (null == dsOriginal)
            {
                return outStream;
            }

            using (MemoryStream intStream = new MemoryStream())
            {
                //以二进制格式将对象或整个连接对象图形序列化和反序列化。
                IFormatter brFormatter = new BinaryFormatter();
                //dsOriginal.RemotingFormat 为远程处理期间使用的DataSet 获取或设置 SerializtionFormat        
                //SerializationFormat.Binary      将字符串比较方法设置为使用严格的二进制排序顺序
                dsOriginal.RemotingFormat = SerializationFormat.Binary;
                //把字符串以二进制放进memStream中
                brFormatter.Serialize(intStream, dsOriginal);
                byte[] binaryDataResult = intStream.ToArray();
                // 压缩
                using (GZipStream Compress =
                    new GZipStream(outStream,
                    CompressionMode.Compress))
                {
                    Compress.Write(binaryDataResult, 0, binaryDataResult.Length);
                }
            }
            return outStream;
        }

        /// <summary>
        /// 解压DataSet
        /// </summary>
        /// <param name="binaryData">内存流</param>
        /// <returns>DataSet数据集对象</returns>
        public DataSet DecompressDataSet(MemoryStream inStream)
        {
            DataSet ds = null;
            if (null == inStream)
            {
                return ds;
            }
            MemoryStream outStream = new MemoryStream();
            using (MemoryStream compressedStream = new MemoryStream(inStream.ToArray()))
            {// 解压
                using (GZipStream Decompress = new GZipStream(compressedStream,
                        CompressionMode.Decompress))
                {
                    Decompress.CopyTo(outStream);
                }
            }
            // 将字节数组反序列化成DataSet对象
            if (null == outStream || 0 >= outStream.Length)
            {
                return ds;
            }
            IFormatter brFormatter = new BinaryFormatter();
            outStream.Position = 0;
            ds = (DataSet)brFormatter.Deserialize(outStream);
            return ds;
        }
        #endregion

        /// <summary>
        /// 依地址获取列数
        /// </summary>
        /// <param name="addrs"></param>
        /// <returns></returns>
        public int GetColumnByAddrs(string addrs)
        {
            string col = addrs.Substring(1, 2);
            int column = Convert.ToInt32(col);
            if (column > 0 && column < 41)
            {
                return column;
            }
            return 0;
        }
    }
}
