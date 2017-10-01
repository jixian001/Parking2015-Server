using System;
using System.Collections.Generic;
using Opc.Da;

namespace CommunicationLib
{
    public abstract class CCommAbstact : IDisposable
    {
        private CCommModule m_commModule = null;
        /// <summary>
        /// 设置通信管理模块
        /// </summary>
        /// <param name="commModule"></param>
        public void SetCommModule(CCommModule commModule)
        {
            m_commModule = commModule;
        }

        /// <summary>
        /// PLC通信服务器连接与否属性
        /// </summary>
        public virtual bool IsConnected
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsAckAllowClear { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Initialize(List<string> lstConnect);
        
        /// <summary>
        /// 初始化（是否添加定时器）
        /// </summary>
        public virtual void Initialize(List<string> lstConnect, bool isTimer)
        {}

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// 连接PLC通信服务器连接
        /// </summary>
        public abstract void ReConnectServer();

        /// <summary>
        /// 读PLC字数据,返回一个Int16型数据
        /// </summary>
        /// <param name="iData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Read(ref Int16 iData, int itemNum)
        {
            return false;
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16[]型数据
        /// </summary>
        /// <param name="intsData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Read(ref Int16[] intsData, int itemNum)
        {
            return false;
        }

        /// <summary>
        /// 读PLC字数据,返回一个byte[]型数据
        /// </summary>
        /// <param name="bData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Read(ref byte[] bData, int itemNum)
        {
            return false;
        }

        /// <summary>
        /// 写PLC数据，写入1个object数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Write(object msg, int itemNum)
        {
            return false;
        }
        
        /// <summary>
        /// 写PLC数据，写入1个object其中的一个字DBW30,DBW32,DBW34数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Write(int nWord, int msg, int itemNum)
        {
            return false;
        }
       
        /// <summary>
        /// 写PLC数据，写入1个object其中的2个字DBW30,DBW32数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public virtual bool Write(int nStartWord, int msg30, int msg32, int itemNum)
        {
            return false;
        }

        /// <summary>
        /// 判断发送的报文与PLC回应的ACK是否一致,返回值：true-收到ACK，false-未收到ACK
        /// </summary>
        /// <param name="sendTelegram"></param>
        /// <param name="ackTelegram"></param>
        /// <returns></returns>
        public virtual bool IsEMSSendAck(Int16[] sendTelegram)
        {
            return true;
        }

        /// <summary>
        /// 触发订阅subscribe数据更改事件
        /// </summary>
        public virtual void OnDataChange(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {
            try
            {
                if (null == m_commModule)
                {
                    return;
                }

                m_commModule.OnDataChange(subscriptionHandle, requestHandle, values);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
