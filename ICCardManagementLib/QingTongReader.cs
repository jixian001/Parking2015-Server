using System;
using LOGManagementLib;
using BaseMethodLib;
using System.Text;
using System.Timers;

namespace ICCardManagementLib
{
    /// <summary>
    /// 读卡器类
    /// </summary>
    public class CQingTongReader : CSwiper
    {
        private bool m_bIs64BitOperatingSystem = Environment.Is64BitOperatingSystem;
        private const int USB_SWIPER = -1;

        /// <summary>
        /// 读卡器所连接的ＣＯＭ口
        /// </summary>
        private int m_nComID = 0;

        /// <summary>
        /// 刷新频率
        /// </summary>
        private int m_nBaudRate;

        /// <summary>
        /// 访卡的模式
        /// </summary>
        private int m_nRequestCardMode;

        /// <summary>
        /// 授权模式
        /// </summary>
        private Int16 m_nAuthKeyMode;

        /// <summary>
        /// 初始化时，返回的设备描述符
        /// </summary>
        private int m_nEquipDesc;

        /// <summary>
        /// 读取IC卡频率 单位：毫秒（默认值：500毫秒）
        /// </summary>
        private int m_nReadICCardRate;

        /// <summary>
        /// 读卡器类构造函数
        /// </summary>
        public CQingTongReader(string nBaudRate, string nRequestCardMode, string nAuthKeyMode)
        {
            CBaseMethods.MyBase.StringToUInt32(nBaudRate, out this.m_nBaudRate);
            CBaseMethods.MyBase.StringToUInt32(nRequestCardMode, out this.m_nRequestCardMode);
            CBaseMethods.MyBase.StringToUInt16(nAuthKeyMode, out this.m_nAuthKeyMode);
        }

        /// <summary>
        /// 读卡器类构造函数
        /// </summary>
        public CQingTongReader(string nBaudRate, string nRequestCardMode, string nAuthKeyMode, string nReadICCardRate)
        {
            CBaseMethods.MyBase.StringToUInt32(nBaudRate, out this.m_nBaudRate);
            CBaseMethods.MyBase.StringToUInt32(nRequestCardMode, out this.m_nRequestCardMode);
            CBaseMethods.MyBase.StringToUInt16(nAuthKeyMode, out this.m_nAuthKeyMode);
            CBaseMethods.MyBase.StringToUInt32(nReadICCardRate, out this.m_nReadICCardRate);
        }
       
        #region 操作函数集

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="nComID"></param>
        public override void SetPara(int nComID)
        {
            m_nComID = nComID - 1;
        }
        
        public override void Run()
        {
            ConnectCOM();
            //m_timer = new Timer() { Interval = m_nReadICCardRate /*ms*/, Enabled = true, /* 定义多次循环*/AutoReset = true };
            //// 计时器启用
            //m_timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            while (true)
            {
                ICCardScaner();
                System.Threading.Thread.Sleep(m_nReadICCardRate);// CConfigManagement.myPara.ReadICCardRate);
            }
        }

        /// <summary>
        /// 定时器轮询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                ICCardScaner();
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CQingTongReader.Timer_Tick 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 初始化，建立与COM口的连接
        /// </summary>
        public override void ConnectCOM()
        {
            try
            {
                if (base.isComConnected)
                {
                    return;
                }
                if (m_bIs64BitOperatingSystem)
                {
                    //64位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        m_nEquipDesc = CQTCommonForUsb.rf_init(m_nComID, m_nBaudRate);
                    }
                    else
                    {
                        m_nEquipDesc = CQTCommon.rf_init(Convert.ToInt16(m_nComID), m_nBaudRate);//mnBaudRate改为mnbantnum
                    }
                }
                else
                {
                    //其他位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        m_nEquipDesc = CQTCommonForUsb.rf_init(m_nComID, m_nBaudRate);
                    }
                    else
                    {
                        m_nEquipDesc = CQTCommon.rf_init(Convert.ToInt16(m_nComID), m_nBaudRate);//mnBaudRate改为mnbantnum
                    }

                }
                if (m_nEquipDesc > 0)
                {
                    base.isComConnected = true;
                    CLOGException.Trace("ICCardManagementLib", "Connect ICCardReader successfully, COM ID = " + (m_nComID + 1));
                    CLOGException.Trace("ICCardManagementLib", "Install component(ICCardManagementLib) successfully, COM ID = " + (m_nComID + 1));
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardRW.ConnectCOM 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 断开COM口连接,返回值等于0成功，-1表示COM的连接没有建立。其他表示不成功。
        /// </summary>
        /// <returns>返回值，等于0成功，-1表示COM的连接没有建立。其他表示不成功。</returns>
        public override void DisConnectCOM()
        {
            if (!base.isComConnected)
            {
                return;
            }
            int nback = -1;
            if (m_nEquipDesc >= 0)
            {
                if (m_bIs64BitOperatingSystem)
                {
                    if (USB_SWIPER == m_nComID)
                    {
                        nback = CQTCommonForUsb.rf_exit(m_nEquipDesc);
                        //nback = CommonForUsb.rf_halt(m_nEquipDesc);
                    }
                    else
                    {
                        nback = CQTCommon.rf_exit(m_nEquipDesc);
                        //nback = Common.rf_halt(m_nEquipDesc);
                    }
                }
                else
                {
                    //其他位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        nback = CQTCommonForUsb.rf_exit(m_nEquipDesc);
                        //nback = CommonForUsb.rf_halt(m_nEquipDesc);
                    }
                    else
                    {
                        nback = CQTCommon.rf_exit(m_nEquipDesc);
                        //nback = Common.rf_halt(m_nEquipDesc);
                    }
                }
                if (0 == nback)
                {
                    base.isComConnected = false;
                }
            }
        }

        /// <summary>
        /// 读取IC卡获取物理卡号,成功返回true，失败返回false
        /// </summary>
        /// <returns></returns>
        private bool ICCardScaner()
        {
            try
            {
                //int nRequestCardFlag = 0;
                //int nSelectCardFlag = 1;
                uint uPhysicalICCardID = 0;
                //当刷卡器连接失败时，会尝试连接刷卡器3次，3次还是连接不上则退出
                for (int nConnectCount = 0; nConnectCount < 3; nConnectCount++)
                {
                    if (base.m_isComConnected) //是否连接
                    {
                        UInt16 nICType = 0;
                        //首先释放，其次发出寻卡命令（参数请求卡片模式）
                        //nRequestCardFlag = RequestICCard(ref nICType); //初始化寻卡功能
                        if (0 == RequestICCard(ref nICType))
                        {
                            //nSelectCardFlag = SelectCard(ref uPhysicalICCardID);//读取卡号
                            if (0 == SelectCard(ref uPhysicalICCardID))
                            {
                                base.m_strPhysicalCardID = uPhysicalICCardID.ToString();
                                base.OnDataChange(base.m_strPhysicalCardID);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        ConnectCOM();
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardRW.ICCardScaner 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
        
        /// <summary>
        /// 读取IC卡获取物理卡号,成功返回true，失败返回false
        /// </summary>
        /// <returns></returns>
        public override bool GetICCardID(out string strPhysicalCardID)
        {
            strPhysicalCardID = string.Empty;
            try
            {
                //int nRequestCardFlag = 0;
                //int nSelectCardFlag = 1;
                uint uPhysicalICCardID = 0;
                //当刷卡器连接失败时，会尝试连接刷卡器3次，3次还是连接不上则退出
                for (int nConnectCount = 0; nConnectCount < 3; nConnectCount++)
                {
                    if (base.m_isComConnected) //是否连接
                    {
                        UInt16 nICType = 0;
                        //首先释放，其次发出寻卡命令（参数请求卡片模式）
                        //nRequestCardFlag = RequestICCard(ref nICType); //初始化寻卡功能
                        if (0 == RequestICCard(ref nICType))
                        {
                            //nSelectCardFlag = SelectCard(ref uPhysicalICCardID);//读取卡号
                            if (0 == SelectCard(ref uPhysicalICCardID))
                            {
                                strPhysicalCardID = uPhysicalICCardID.ToString();
                                return true;
                            }
                        }
                    }
                    else
                    {
                        ConnectCOM();
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardRW.GetICCard 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 向感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）写入数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strWrite">写入的数据（16字节或6字节）,十六进制数</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public override int WriteDataIntoCard(Int16 nSector, Int16 nDBNum, string strWrite)
        {
            strWrite += "FF";
            byte[] bBuff = new byte[32];
            byte[] bWrite = new byte[16];
            int nCounter = 0;
            for (nCounter = 0; nCounter < strWrite.Length; nCounter++)
            {
                if (strWrite[nCounter] >= '0' && strWrite[nCounter] <= '9')
                    continue;
                if (strWrite[nCounter] <= 'a' && strWrite[nCounter] <= 'f')
                    continue;
                if (strWrite[nCounter] <= 'A' && strWrite[nCounter] <= 'F')
                    continue;
            }
            if (nCounter != strWrite.Length)
            {
                //                 lbResult.Text = "data is hex data!";
                return 1;

            }

            bBuff = Encoding.ASCII.GetBytes(strWrite);
            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    CQTCommonForUsb.a_hex(bBuff, bWrite, 32);
                }
                else
                {
                    CQTCommon.a_hex(bBuff, bWrite, 32);
                }
            }
            else
            {
                //其他位系统                
                if (USB_SWIPER == m_nComID)
                {
                    CQTCommonForUsb.a_hex(bBuff, bWrite, 32);
                }
                else
                {
                    CQTCommon.a_hex(bBuff, bWrite, 32);
                }
            }


            int nBack = 1;
            int blocknum = nSector * 4 + nDBNum;

            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    nBack = CQTCommonForUsb.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
                else
                {
                    nBack = CQTCommon.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
            }
            else
            {
                //其他位系统
                if (USB_SWIPER == m_nComID)
                {
                    nBack = CQTCommonForUsb.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
                else
                {
                    nBack = CQTCommon.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
            }

            if (nBack != 0)
            {
                return -1;
            }

            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    nBack = CQTCommonForUsb.rf_write(m_nEquipDesc, (short)blocknum, bWrite);
                    CQTCommonForUsb.rf_beep(m_nEquipDesc, 50);
                }
                else
                {
                    nBack = CQTCommon.rf_write(m_nEquipDesc, (short)blocknum, bWrite);
                    CQTCommon.rf_beep(m_nEquipDesc, 50);
                }
            }
            else
            {
                //其他位系统
                if (USB_SWIPER == m_nComID)
                {
                    nBack = CQTCommonForUsb.rf_write(m_nEquipDesc, (short)blocknum, bWrite);
                    CQTCommonForUsb.rf_beep(m_nEquipDesc, 50);
                }
                else
                {
                    nBack = CQTCommon.rf_write(m_nEquipDesc, (short)blocknum, bWrite);
                    CQTCommon.rf_beep(m_nEquipDesc, 50);
                }
            }

            return nBack;
        }

        /// <summary>
        /// 从感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）读取数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strRead">读取的数据</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public override int ReadDataFromCard(Int16 nSector, Int16 nDBNum, out string strRead)
        {
            strRead = string.Empty;
            int i = 0;
            byte[] data = new byte[16];
            byte[] buff = new byte[32];

            for (i = 0; i < 16; i++)
                data[i] = 0;
            for (i = 0; i < 32; i++)
                buff[i] = 0;
            int nReturn = 1;
            int blocknum = nSector * 4 + nDBNum;
            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    nReturn = CQTCommonForUsb.rf_read(m_nEquipDesc, blocknum, data);
                }
                else
                {
                    nReturn = CQTCommon.rf_read(m_nEquipDesc, blocknum, data);
                }
            }
            else
            {
                //其他位系统                
                if (USB_SWIPER == m_nComID)
                {
                    nReturn = CQTCommonForUsb.rf_read(m_nEquipDesc, blocknum, data);
                }
                else
                {
                    nReturn = CQTCommon.rf_read(m_nEquipDesc, blocknum, data);
                }
            }

            if (nReturn == 0)
            {
                if (m_bIs64BitOperatingSystem)
                {
                    //64位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.hex_a(data, buff, 16);
                    }
                    else
                    {
                        CQTCommon.hex_a(data, buff, 16);
                    }
                }
                else
                {
                    //其他位系统                
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.hex_a(data, buff, 16);
                    }
                    else
                    {
                        CQTCommon.hex_a(data, buff, 16);
                    }
                }
                strRead = System.Text.Encoding.ASCII.GetString(buff);
            }
            return nReturn;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="nSector"></param>
        /// <param name="strKey"></param>
        /// <returns></returns>
        private int AuthKey(int nSector, string strKey)
        {
            int nReturn = -1;
            byte[] szKey1 = new byte[17];
            byte[] szKey2 = new byte[7];
            int i = 0;
            int nKeyLen = strKey.Length;
            if (nKeyLen != 12)
            {
                return -1;
            }

            if (nSector < 1 || nSector > 15)
            {
                return -1;
            }

            for (i = 0; i < nKeyLen; i++)
            {
                if (strKey[i] >= '0' && strKey[i] <= '9')
                    continue;
                if (strKey[i] <= 'a' && strKey[i] <= 'f')
                    continue;
                if (strKey[i] <= 'A' && strKey[i] <= 'F')
                    continue;
            }
            if (i != nKeyLen)
            {
                return -1;

            }
            szKey1 = Encoding.ASCII.GetBytes(strKey);
            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    CQTCommonForUsb.a_hex(szKey1, szKey2, 12);
                    nReturn = CQTCommonForUsb.rf_load_key(m_nEquipDesc, m_nAuthKeyMode, nSector, szKey2);
                    if (nReturn != 0)
                    {
                        return nSector;
                    }
                    nReturn = CQTCommonForUsb.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
                else
                {
                    CQTCommon.a_hex(szKey1, szKey2, 12);
                    nReturn = CQTCommon.rf_load_key(m_nEquipDesc, m_nAuthKeyMode, nSector, szKey2);
                    if (nReturn != 0)
                    {
                        return nSector;
                    }
                    nReturn = CQTCommon.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
            }
            else
            {
                //其他位系统
                if (USB_SWIPER == m_nComID)
                {
                    CQTCommonForUsb.a_hex(szKey1, szKey2, 12);
                    nReturn = CQTCommonForUsb.rf_load_key(m_nEquipDesc, m_nAuthKeyMode, nSector, szKey2);
                    if (nReturn != 0)
                    {
                        return nSector;
                    }
                    nReturn = CQTCommonForUsb.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
                else
                {
                    CQTCommon.a_hex(szKey1, szKey2, 12);
                    nReturn = CQTCommon.rf_load_key(m_nEquipDesc, m_nAuthKeyMode, nSector, szKey2);
                    if (nReturn != 0)
                    {
                        return nSector;
                    }
                    nReturn = CQTCommon.rf_authentication(m_nEquipDesc, m_nAuthKeyMode, nSector);
                }
            }
            return nReturn;
        }
        
        /// <summary>
        ///  寻卡，注意寻卡方式，0表示Idle方式；1表示all方式。返回值为0表成功
        /// </summary>
        /// <param name="nCardType">返回寻到的卡的类型</param>
        /// <returns>返回值为0表成功</returns>
        private int RequestICCard(ref UInt16 nCardType)
        {
            int nback = -1;
            //int ntemp=-1;
            if (m_nEquipDesc >= 0) //设备初始化了
            {
                if (m_bIs64BitOperatingSystem)
                {
                    //64位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.rf_reset(m_nEquipDesc, 3);
                        nback = CQTCommonForUsb.rf_request(m_nEquipDesc, m_nRequestCardMode, out nCardType);
                    }
                    else
                    {
                        CQTCommon.rf_reset(m_nEquipDesc, 3);
                        nback = CQTCommon.rf_request(m_nEquipDesc, m_nRequestCardMode, out nCardType);
                    }
                }
                else
                {
                    //其他位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.rf_reset(m_nEquipDesc, 3);
                        nback = CQTCommonForUsb.rf_request(m_nEquipDesc, m_nRequestCardMode, out nCardType);
                    }
                    else
                    {
                        CQTCommon.rf_reset(m_nEquipDesc, 3);
                        nback = CQTCommon.rf_request(m_nEquipDesc, m_nRequestCardMode, out nCardType);
                    }
                }

            }
            return nback;
        }

        /// <summary>
        /// 防冲突的情况下选卡，并选择其中停车卡。返回值为0表示成功，-1表示防冲突函数失败，否则表示选卡不成功。
        /// </summary>
        /// <param name="nCardSequenceNum">卡号</param>
        /// <returns>返回值为0表示成功，-1表示防冲突函数失败，否则表示选卡不成功。</returns>
        private int SelectCard(ref uint nCardSequenceNum)
        {
            //int nCardSequenceNum;
            //byte strtemp;
            int nback = -1;

            if (m_bIs64BitOperatingSystem)
            {
                //64位系统
                if (USB_SWIPER == m_nComID)
                {
                    nback = CQTCommonForUsb.rf_anticoll(m_nEquipDesc, 0, out nCardSequenceNum);//查找卡片
                }
                else
                {
                    nback = CQTCommon.rf_anticoll(m_nEquipDesc, 0, out nCardSequenceNum);//查找卡片
                }
            }
            else
            {
                //其他位系统
                if (USB_SWIPER == m_nComID)
                {
                    nback = CQTCommonForUsb.rf_anticoll(m_nEquipDesc, 0, out nCardSequenceNum);//查找卡片
                }
                else
                {
                    nback = CQTCommon.rf_anticoll(m_nEquipDesc, 0, out nCardSequenceNum);//查找卡片
                }
            }

            if (nback == 0)
            {
                if (m_bIs64BitOperatingSystem)
                {
                    //64位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.rf_beep(m_nEquipDesc, 50);
                    }
                    else
                    {
                        CQTCommon.rf_beep(m_nEquipDesc, 50);
                    }
                }
                else
                {
                    //其他位系统
                    if (USB_SWIPER == m_nComID)
                    {
                        CQTCommonForUsb.rf_beep(m_nEquipDesc, 50);
                    }
                    else
                    {
                        //CLOGException.Trace(m_nEquipDesc + "beep");
                        CQTCommon.rf_beep(m_nEquipDesc, 50);
                    }
                }
                //rf_halt(m_nEquipDesc);
            }
            return nback;
        }

        #region 注释
        ///// <summary>
        ///// 向读写器装载指定扇区的新密码（不与卡片进行通讯），读写器中有16个扇区的密码（0~15），每个扇区有两个密码(KEY A 和 KEY B)。返回0表示成功，否则表示失败
        ///// 该函数在与读写器建立连接后，初始化时需要写入
        ///// </summary>
        ///// <param name="mode">密码类型0 — KEY A 4 — KEY B</param>
        ///// <param name="SectorNum">须装载密码的扇区号(0～15)</param>
        ///// <param name="csKeyCode">写入读写器的6字节新密码(十六进制字符)</param>
        ///// <returns>返回0表示成功，否则表示失败</returns>
        //public int LoadKeyToEquip(int mode, int SectorNum, byte[] csKeyCode)
        //{
        //    int nback = -1;
        //    if (m_bIs64BitOperatingSystem)
        //    {
        //        //64位系统
        //        nback = CommonForUsb.rf_load_key(m_nEquipDesc, mode, SectorNum, csKeyCode);
        //    }
        //    else
        //    {
        //        //其他位系统
        //        nback = Common.rf_load_key(m_nEquipDesc, mode, SectorNum, csKeyCode);
        //    }
        //    return nback;
        //}


        ///// <summary>
        ///// 读取感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        ///// </summary>
        ///// <param name="nSector">指定扇区</param>
        ///// <param name="nDBNum">指定扇区中的指定DB块</param>
        ///// <param name="strRead">返回的数据（16字节）,十六进制数</param>
        ///// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        //public int ReadDataFromCard(Int16 nSector, Int16 nDBNum, ref string strRead)
        //{
        //    int nCounter = 0;
        //    int nback = -1;
        //    byte[] bData = new byte[16];
        //    byte[] bRead = new byte[32];
        //    for (nCounter = 0; nCounter < 16; nCounter++)
        //    {
        //        bData[nCounter] = 0;
        //    }
        //    for (nCounter = 0; nCounter < 32; nCounter++)
        //    {
        //        bRead[nCounter] = 0;
        //    }
        //    short blocknum = (short)(nSector * 4 + nDBNum);

        //    if (m_bIs64BitOperatingSystem)
        //    {
        //        //64位系统
        //        nback = CommonForUsb.rf_authentication(m_nEquipDesc, ICCardPara.AuthKeyMode, nSector);
        //    }
        //    else
        //    {
        //        //其他位系统
        //        nback = Common.rf_authentication(m_nEquipDesc, ICCardPara.AuthKeyMode, nSector);
        //    }

        //    if (nback != 0)
        //    {
        //        return -1;
        //    }

        //    if (m_bIs64BitOperatingSystem)
        //    {
        //        //64位系统
        //        nback = CommonForUsb.rf_read(m_nEquipDesc, blocknum, bData);
        //        CommonForUsb.hex_a(bData, bRead, 16);
        //    }
        //    else
        //    {
        //        //其他位系统
        //        nback = Common.rf_read(m_nEquipDesc, blocknum, bData);
        //        Common.hex_a(bData, bRead, 16);
        //    }

        //    strRead = System.Text.Encoding.ASCII.GetString(bRead);
        //    int nIndexOfF = strRead.IndexOf('F');
        //    if (0 < nIndexOfF)
        //    {
        //        strRead = strRead.Substring(0, nIndexOfF);
        //    }
        //    else
        //    {
        //        strRead = string.Empty;
        //        return -1;
        //    }

        //    //             if (0 == nback)
        //    //             {
        //    //                 if (m_bIs64BitOperatingSystem)
        //    //                 {
        //    //                     //64位系统
        //    //                     CommonForUsb.rf_beep(m_nEquipDesc, 50);
        //    //                 }
        //    //                 else
        //    //                 {
        //    //                     //其他位系统
        //    //                     Common.rf_beep(m_nEquipDesc, 50);
        //    //                 }
        //    //             }
        //    return nback;
        //}

        //private int AuthenticatCard(int nSector)
        //{
        //    int nback = -1;

        //    if (m_bIs64BitOperatingSystem)
        //    {
        //        //64位系统
        //        nback = CommonForUsb.rf_authentication(m_nEquipDesc, (short)ICCardPara.AuthKeyMode, (short)(nSector));
        //    }
        //    else
        //    {
        //        //其他位系统
        //        nback = Common.rf_authentication(m_nEquipDesc, (short)ICCardPara.AuthKeyMode, (short)(nSector));
        //    }

        //    return nback;
        //}

        #endregion 注释
        
        #endregion
    }
}
