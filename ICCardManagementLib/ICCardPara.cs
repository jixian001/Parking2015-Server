using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICCardManagementLib
{
    /// <summary>
    /// 读卡器连接参数类
    /// </summary>
    class CICCardPara : ICloneable
    {
        /// <summary>
        /// 读卡器所连接的ＣＯＭ口
        /// </summary>
        private int m_nComID;
        public int ComID
        {
            get { return m_nComID; }
            set { m_nComID = value; }
        }
        /// <summary>
        /// 刷新频率
        /// </summary>
        private int m_nBaudRate;
        public int BaudRate
        {
            get { return m_nBaudRate; }
            set { m_nBaudRate = value; }
        }
        /// <summary>
        /// 访卡的模式
        /// </summary>
        private int m_nRequestCardMode;
        public int RequestCardMode
        {
            get { return m_nRequestCardMode; }
            set { m_nRequestCardMode = value; }
        }
        /// <summary>
        /// 授权模式
        /// </summary>
        private Int16 m_nAuthKeyMode;
        public Int16 AuthKeyMode
        {
            get { return m_nAuthKeyMode; }
            set { m_nAuthKeyMode = value; }
        }

        public Object Clone()
        {
            return (Object)this.MemberwiseClone();
        }

    }
}
