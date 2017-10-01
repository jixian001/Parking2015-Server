using LOGManagementLib;
using System;

namespace ICCardManagementLib
{
    public abstract class CSwiper
    {
        private CICCardReaderObj m_objReader = null;

        protected string m_strPhysicalCardID = null;//读取的IC卡卡号。
        /// <summary>
        /// 物理卡号
        /// </summary>
        public string strPhysicalCardID
        {
            get { return strPhysicalCardID = m_strPhysicalCardID; }
            set { m_strPhysicalCardID = value; }
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
        /// 构造函数
        /// </summary>
        public CSwiper()
        {
        }

        /// <summary>
        /// 设置对象
        /// </summary>
        /// <param name="Obj"></param>
        public void SetICCardReaderObj(CICCardReaderObj Obj)
        {
            m_objReader = Obj;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="nComID"></param>
        public abstract void SetPara(int nComID);

        /// <summary>
        /// 运行刷卡器自动读卡
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// 链接刷卡器
        /// </summary>
        public abstract void ConnectCOM();

        /// <summary>
        /// 断开刷卡器
        /// </summary>
        public abstract void DisConnectCOM();

        /// <summary>
        /// 读取IC卡获取物理卡号,成功返回true，失败返回false
        /// </summary>
        /// <returns></returns>
        public abstract bool GetICCardID(out string strPhysicalCardID);

        /// <summary>
        /// 从感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）读取数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strRead">读取的数据</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public abstract int ReadDataFromCard(Int16 nSector, Int16 nDBNum, out string strRead);

        /// <summary>
        /// 向感应区内所选定卡的指定扇区（0~15）的指定数据块（0~3）写入数据（16字节或者6字节（密码）十六进制）。函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败
        /// </summary>
        /// <param name="nSector">指定扇区</param>
        /// <param name="nDBNum">指定扇区中的指定DB块</param>
        /// <param name="strWrite">写入的数据（16字节或6字节）,十六进制数</param>
        /// <returns>函数返回值，-1表示验证密码失败，0表示操作成功，其他表示读失败</returns>
        public abstract int WriteDataIntoCard(Int16 nSector, Int16 nDBNum, string strWrite);

        /// <summary>
        /// 触发数据更改事件
        /// </summary>
        public void OnDataChange(string strPhysicalCardID)
        {
            if (null == m_objReader)
            {
                CLOGException.Trace("ICCardManagementLib.CSwiper.OnDataChange", "m_objReader为空对象");
            }
            m_objReader.OnDataChange(strPhysicalCardID);
        }

    }
}
