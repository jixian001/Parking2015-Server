using System;
using System.Collections.Generic;
using System.Reflection;
using Opc.Da;

namespace CommunicationLib
{
    /// <summary>
    /// OPC委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CallbackTelegramEventHandler(object sender, CTelegramEventArgs e);

    /// <summary>
    /// OPC回调事件参数类EventArgs
    /// </summary>
    public class CTelegramEventArgs : EventArgs
    {
        private object telegram;
        private string itemName;

        /// <summary>
        /// OPC回调事件参数-报文内容
        /// </summary>
        public object Telegram
        {
            get
            {
                return telegram;
            }
            set
            {
                telegram = value;
            }
        }

        /// <summary>
        /// OPC回调事件参数-报文item对应名称
        /// </summary>
        public string ItemName
        {
            get
            {
                return itemName;
            }
            set
            {
                itemName = value;
            }
        }
    }

    /// <summary>
    /// 管理通信模块类
    /// </summary>
    public class CCommModule
    {
        /// <summary>
        /// 外部回调连接事件
        /// </summary>
        public event CallbackTelegramEventHandler CallbackTelegramEvent;
        private CCommAbstact m_objStandard = null;

        /// <summary>
        ///从此程序集中查找指定的类型，并使用系统激活器创建它的实例，包括可选的区分大小写搜索并具有指定的区域性、参数和绑定及激活属性。
        ///
        /// 参数:
        ///   typeName:
        ///     要查找的类型的 System.Type.FullName。
        ///   args:
        ///     Object 类型的数组，包含要传递给构造函数的参数。此参数数组在数量、顺序和类型方面必须与要调用的构造函数的参数匹配。如果需要默认的构造函数，则
        ///     args 必须是空数组或 null。
        /// 返回结果:
        ///     表示此类型且匹配指定条件的 Object 的实例；如果没有找到 typeName，则为 null。
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="args"></param>
        public void SetCommAbstact(string typeName, object[] args)
        {
            m_objStandard = (CCommAbstact)Assembly.Load("CommunicationLib").CreateInstance("CommunicationLib." + typeName, false, BindingFlags.Default, null, args, null, null);

            if (null == m_objStandard)
            {
                return;
            }

            m_objStandard.SetCommModule(this);
        }
     
        /// <summary>
        /// PLC通信服务器连接与否属性
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return m_objStandard.IsConnected;
            }
        }

        /// <summary>
        /// 用于发送报文后，将IsAckAllowClear置为false,
        /// 当时间到后，不再接收ACK，这时再有ACK，则将其清0
        /// </summary>
        public bool AckAllowClear
        {
            get
            {
                return m_objStandard.IsAckAllowClear;
            }
            set
            {
                m_objStandard.IsAckAllowClear = value;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(List<string> lstConnect)
        {
            if (null == m_objStandard)
            {
                return;
            }

            m_objStandard.Initialize(lstConnect);
        }

        /// <summary>
        /// 初始化（是否添加定时器）
        /// </summary>
        public void Initialize(List<string> lstConnect, bool isTimer)
        {
            if (null == m_objStandard)
            {
                return;
            }

            m_objStandard.Initialize(lstConnect, isTimer);
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public void Dispose()
        {
            if (null == m_objStandard)
            {
                return;
            }

            m_objStandard.Dispose();
        }

        /// <summary>
        /// 重新连接PLC通信服务器
        /// </summary>
        public void ReConnectServer()
        {
            if (null == m_objStandard)
            {
                return;
            }

            m_objStandard.ReConnectServer();
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16型数据
        /// </summary>
        /// <param name="iData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Read(ref Int16 iData, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Read(ref iData, itemNum);
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16[]型数据
        /// </summary>
        /// <param name="intsData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Read(ref Int16[] intsData, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Read(ref intsData, itemNum);
        }

        /// <summary>
        /// 读PLC字数据,返回一个byte[]型数据
        /// </summary>
        /// <param name="bData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Read(ref byte[] bData, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Read(ref bData, itemNum);
        }

        /// <summary>
        /// 写PLC数据，写入1个object数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Write(object msg, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Write(msg, itemNum);
        }

        /// <summary>
        /// 写PLC数据，写入1个object其中的一个字DBW30,DBW32,DBW34数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Write(int nWord, int msg, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Write(nWord, msg, itemNum);
        }
       
        /// <summary>
        /// 写PLC数据，写入1个object其中的2个字DBW30,DBW32数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public bool Write(int nStartWord, int msg30, int msg32, int itemNum)
        {
            if (null == m_objStandard)
            {
                return false;
            }

            return m_objStandard.Write(nStartWord, msg30, msg32, itemNum);
        }

        /// <summary>
        /// 判断发送的报文与PLC回应的ACK是否一致,返回值：true-收到ACK，false-未收到ACK
        /// </summary>
        /// <param name="sendTelegram"></param>
        /// <param name="ackTelegram"></param>
        /// <returns></returns>
        public bool IsEMSSendAck(Int16[] sendTelegram)
        {
            if (null == m_objStandard)
            {
                return true;
            }

            return m_objStandard.IsEMSSendAck(sendTelegram);
        }

        /// <summary>
        /// 触发订阅subscribe数据更改事件
        /// </summary>
        public void OnDataChange(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {
            foreach (ItemValueResult item in values)
            {
                if (CallbackTelegramEvent != null)
                {
                    CTelegramEventArgs e = new CTelegramEventArgs();
                    e.Telegram = item.Value;
                    e.ItemName = item.ItemName;
                    CallbackTelegramEvent(this, e);
                }
            }
        }
    }
}
