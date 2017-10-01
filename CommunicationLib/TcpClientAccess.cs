using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Opc.Da;
using BaseMethodLib;
using LOGManagementLib;

namespace CommunicationLib
{
    public class CTcpClientAccess : CCommAbstact
    {
        TcpClient m_tcp = new TcpClient();
        string m_strIP = string.Empty;// 服务器IP地址
        int m_nPort = 0;// 侦听的端口
        private Socket m_socket = null;
        public byte[] m_dataBuffer = new byte[1024];

        public CTcpClientAccess()
        {
            m_strIP = "10.34.60.253";
            m_nPort = 2000;
        }

        /// <summary>
        /// TCP客户端类构造函数
        /// </summary>
        public CTcpClientAccess(string strIP, int nPort)
        {
            m_strIP = strIP;
            m_nPort = nPort;
        }

        /// <summary>
        /// TCP客户端类构造函数
        /// </summary>
        public CTcpClientAccess(string strIP, string strPort)
        {
            m_strIP = strIP;

            if (!CBaseMethods.MyBase.StringToUInt32(strPort, out m_nPort))
            {
                m_nPort = 2000;
            }
        }

        #region 重载函数
        /// <summary>
        /// modbus服务器连接与否属性
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (null == m_socket)
                {
                    return false;
                }

                return m_socket.Connected;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize(List<string> lstConnect)
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Connect(new IPEndPoint(IPAddress.Parse(m_strIP), m_nPort));

            if (m_socket.Connected)
            {
                //异步接受服务器回报的字符串
                m_socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, ReadCallBack, m_dataBuffer);
            }
       
            //if (null != m_socket)
            //{
            //    return;
            //}

            ////根据服务器的IP地址和端口号 异步连接服务器
            //m_tcp.BeginConnect(IPAddress.Parse(m_strIP), m_nPort, new AsyncCallback(ConnectCallback), m_tcp);
            //m_socket = m_tcp.Client;
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public override void Dispose()
        {
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket = null;
            }
        }

        /// <summary>
        /// 重新连接PLC通信服务器连接
        /// </summary>
        public override void ReConnectServer()
        {
            //根据服务器的IP地址和端口号 异步连接服务器
            m_tcp.BeginConnect(IPAddress.Parse(m_strIP), m_nPort, new AsyncCallback(ConnectCallback), m_tcp);
            m_socket = m_tcp.Client;
        }

        /// <summary>
        /// 接收服务端发送过来的数据（用缓存来接收）
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="itemNum"></param>
        public override bool Read(ref byte[] buffer, int itemNum)
        {
            lock (m_socket)// 为了保证数据的完整性以及安全性  锁定数据流
            {
                //异步接受服务器回报的字符串
                //m_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReadCallBack, m_dataBuffer);
                m_socket.Receive(buffer);
                return true;
            }
        }

        /// <summary>
        /// 向连接的服务端发送数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="itemNum"></param>
        public override bool Write(object msg, int itemNum)
        {
            if (msg.GetType() == typeof(byte[]))
            {
                m_dataBuffer = (byte[])msg;
            }
            else if (msg.GetType() == typeof(string))
            {
                string str = (string)msg;
                m_dataBuffer = Encoding.ASCII.GetBytes(str);
            }
            else if (msg.GetType() == typeof(int))
            {
                string str = "succeed send:" + (int)msg + "\n";
                m_dataBuffer = Encoding.ASCII.GetBytes(str); ;
            }
            else
            {
                return false;
            }

            lock (m_socket)
            {
                // 异步发送数据
                m_socket.BeginSend(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), m_dataBuffer);
                return true;
            }
        }
        #endregion

        #region 私有函数
        /// <summary>
        /// 连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            TcpClient t = (TcpClient)ar.AsyncState;
            m_tcp = t;
            m_socket = m_tcp.Client;
            try
            {
                if (t.Connected)
                {
                    t.EndConnect(ar);//函数运行到这里就说明连接成功
                    // Send a succeed Connect to Server
                    string msg = "succeed Connect \n";
                    Write(msg, 1);
                    //异步接受服务器回报的字符串
                    m_socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, ReadCallBack, m_dataBuffer);
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccess.ConnectCallback 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 接收服务端数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallBack(IAsyncResult ar)
        {
            //接收到的数据长度
            int receLen = 0;
            SocketError errorCode;

            try
            {
                receLen = m_socket.EndReceive(ar, out errorCode);
                char[] chars = new char[receLen + 1];
                // Extract the characters as a buffer
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(m_dataBuffer, 0, receLen, chars, 0);
                System.String szData = new System.String(chars);
                // 构造事件
                ItemValueResult[] values = new ItemValueResult[1];
                values[0] = new ItemValueResult();
                values[0].Value = m_dataBuffer;
                values[0].ItemName = "MODBUS Client:" + m_socket.LocalEndPoint.ToString();
                base.OnDataChange(null, null, values);

                if (receLen > 0)
                {
                    string strLogin = "succeed recived:" + szData;
                    byte[] data = Encoding.ASCII.GetBytes(strLogin);
                    m_socket.BeginSend(data, 0, data.Length, SocketFlags.None, out errorCode, SendCallback, m_dataBuffer);//异步发送数据
                    //异步接受服务器回报的字符串
                    m_socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, ReadCallBack, m_dataBuffer);
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccess.ReadCallBack 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 发送数据到服务端回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                SocketError errorCode;
                m_socket.EndSend(ar, out errorCode);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccess.SendCallback 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }
        #endregion
    }
}
