using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICCardManagementLib
{
    /// <summary>
    /// 事件触发时传递的数据
    /// ICCardID：读取的IC卡卡号
    /// </summary>
    public class CICCardReaderEventArgs : EventArgs
    {
        private int m_nComID;
        /// <summary>
        ///  串口号
        /// </summary>
        public int nComID
        {
            get { return m_nComID; }
            set { m_nComID = value; }
        }

        private bool m_isComConnected;
        /// <summary>
        /// 连接标志
        /// </summary>
        public bool isComConnected
        {
            get { return isComConnected = m_isComConnected; }
            set { m_isComConnected = value; }
        }

        private string m_strICCardID;
        /// <summary>
        /// IC卡卡号
        /// </summary>
        public string strICCardID
        {
            get { return m_strICCardID; }
            set { m_strICCardID = value; }
        }
    }
}
