using System;
using LOGManagementLib;
using BaseMethodLib;
using System.Reflection;

namespace ICCardManagementLib
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CallbackSwiperEventHandler(object sender, CSwiperEventArgs e);

    /// <summary>
    /// 回调事件参数类EventArgs
    /// </summary>
    public class CSwiperEventArgs : EventArgs
    {
        private string m_strPhysicalCardID;//读取的IC卡卡号。

        /// <summary>
        /// 回调事件参数-物理卡号
        /// </summary>
        public string PhysicalCardID
        {
            get
            {
                return m_strPhysicalCardID;
            }
            set
            {
                m_strPhysicalCardID = value;
            }
        }

        private int m_nHallID;//车厅ID。

        /// <summary>
        /// 回调事件参数-车厅ID
        /// </summary>
        public int HallID
        {
            get
            {
                return m_nHallID;
            }
            set
            {
                m_nHallID = value;
            }
        }

    }

    public class CICCardReaderObj
    {
        /// <summary>
        /// 外部回调连接事件
        /// </summary>
        public event CallbackSwiperEventHandler CallbackSwiperEvent;

        private int m_nHallID = 0;
        private CSwiper m_objSwiper;//读卡器对象
        private int m_nComID = 0;//串口号

        /// <summary>
        /// 构造函数
        /// </summary>
        public CICCardReaderObj(int nComIDTmp)
        {
            m_nComID = nComIDTmp;
        }

        protected bool m_isComConnected = false;
        /// <summary>
        /// 读卡器连接标识
        /// </summary>
        public bool isComConnected
        {
            get { return isComConnected = m_isComConnected; }
            set { m_isComConnected = value; }
        }
        /// <summary>
        /// 设置刷卡器参数
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <param name="nHallID"></param>
        public void SetParameter(string strClassName, object[] argsPara)
        {
            try
            {
                m_objSwiper = (CSwiper)Assembly.Load("ICCardManagementLib").CreateInstance("ICCardManagementLib." + strClassName, false, BindingFlags.Default, null, argsPara, null, null);

                if (null == m_objSwiper)
                {
                    CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.SetParameter ", "m_objReader为空对象");
                }
                m_objSwiper.SetICCardReaderObj(this);
                m_objSwiper.SetPara(m_nComID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.SetParameter1 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 设置刷卡器参数
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <param name="nHallID"></param>
        public void SetParameter(int nHallID, string strClassName, object[] argsPara)
        {
            try
            {
                m_nHallID = nHallID;
                m_objSwiper = (CSwiper)Assembly.Load("ICCardManagementLib").CreateInstance("ICCardManagementLib." + strClassName, false, BindingFlags.Default, null, argsPara, null, null);

                if (null == m_objSwiper)
                {
                    CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.SetParameter ", "m_objReader为空对象");
                    return;
                }
                m_objSwiper.SetICCardReaderObj(this);
                m_objSwiper.SetPara(m_nComID);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.SetParameter 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 连接刷卡器并不停扫描刷卡器
        /// </summary>
        public void Run()
        {
            if (null == m_objSwiper)
            {
                return;
            }
            m_objSwiper.Run();
        }

        /// <summary>
        /// 连接刷卡器
        /// </summary>
        public void ConnectCOM()
        {
            m_objSwiper.ConnectCOM();
            m_isComConnected = m_objSwiper.isComConnected;
        }

        /// <summary>
        /// 断开刷卡器
        /// </summary>
        public void DisConnectCOM()
        {
            m_objSwiper.DisConnectCOM();
            m_isComConnected = m_objSwiper.isComConnected;
        }

        /// <summary>
        /// 读取IC卡获取物理卡号,成功返回true，失败返回false
        /// </summary>
        /// <returns></returns>
        public bool GetICCardID(out string strPhysicalCardID)
        {
            return m_objSwiper.GetICCardID(out strPhysicalCardID);

        }

        /// <summary>
        /// 从感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）读取数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strRead">读取的数据</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public int ReadDataFromCard(Int16 nSector, Int16 nDBNum, out string strRead)
        {
            return m_objSwiper.ReadDataFromCard(nSector, nDBNum, out strRead);
        }

        /// <summary>
        /// 向感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）写入数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strWrite">写入的数据（16字节或6字节）,十六进制数</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public int WriteDataIntoCard(Int16 nSector, Int16 nDBNum, string strWrite)
        {
            return m_objSwiper.WriteDataIntoCard(nSector, nDBNum, strWrite);
        }

        /// <summary>
        /// 将获取的IC卡物理卡号传给订阅者
        /// </summary>
        /// <param name="strPhysicalCardID"></param>
        public void OnDataChange(string strPhysicalCardID)
        {
            if (CallbackSwiperEvent != null)
            {
                CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.OnDataChange ", string.Format("车厅号:{0},物理卡号:{1},CallbackSwiperEvent:{2}", m_nHallID, strPhysicalCardID, CallbackSwiperEvent.Method));
                CSwiperEventArgs e = new CSwiperEventArgs();
                e.PhysicalCardID = strPhysicalCardID;
                e.HallID = m_nHallID;
                CallbackSwiperEvent(this, e);
            }
            else
            {
                CLOGException.Trace("ICCardManagementLib.CICCardReaderObj.OnDataChange ", string.Format("车厅号:{0},物理卡号:{1},CallbackSwiperEvent:{2}", m_nHallID, strPhysicalCardID, CallbackSwiperEvent));
            }
        }
    }
}
