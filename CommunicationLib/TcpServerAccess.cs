using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using Opc.Da;
using BaseMethodLib;
using LOGManagementLib;

namespace CommunicationLib
{
    public class SocketPacket
    {
        // Constructor which takes a Socket and a client number
        public SocketPacket(System.Net.Sockets.Socket socket, int clientNumber)
        {
            m_currentSocket = socket;
            m_clientNumber = clientNumber;
        }
        public System.Net.Sockets.Socket m_currentSocket;
        public int m_clientNumber;
        // Buffer to store the data sent by the client
        public byte[] dataBuffer = new byte[1024];
    }

    public class CTcpServerAccess : CCommAbstact
    {
        string m_strIP = string.Empty;// 服务器IP地址
        int m_nPort = 0;// 侦听的端口
        public AsyncCallback pfnWorkerCallBack;
        private Socket m_socket = null;
        // An ArrayList is used to keep track of worker sockets that are designed
        // to communicate with each connected client. Make it a synchronized ArrayList
        // For thread safety
        private System.Collections.ArrayList m_workerSocketList = ArrayList.Synchronized(new System.Collections.ArrayList());
        // The following variable will keep track of the cumulative 
        // total number of clients connected at any time. Since multiple threads
        // can access this variable, modifying this variable should be done
        // in a thread safe manner
        private int m_clientCount = 0;

         public CTcpServerAccess()
        {
            m_strIP = "10.34.60.180";
            m_nPort = 2000;
        }

        /// <summary>
         /// TCP服务端类构造函数
        /// </summary>
        public CTcpServerAccess(string strIP, int nPort)
        {
            m_strIP = strIP;
            m_nPort = nPort;
        }

        /// <summary>
        /// TCP服务端类构造函数
        /// </summary>
        public CTcpServerAccess(string strIP, string strPort)
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
            if (null != m_socket)
            {
                return;
            }

            // 创建监听Create the listening socket...
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(m_strIP), m_nPort);// IPAddress.Any
            // 关联Bind to local IP Address...
            m_socket.Bind(ipLocal);
            // 开始侦听Start listening...
            m_socket.Listen(4);
            // 异步连接Create the call back for any client connections...
            m_socket.BeginAccept(new AsyncCallback(ConnectCallback), null);
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

            Socket workerSocket = null;
            for (int i = 0; i < m_workerSocketList.Count; i++)
            {
                workerSocket = (Socket)m_workerSocketList[i];
                if (workerSocket != null)
                {
                    workerSocket.Close();
                    workerSocket = null;
                }
            }
        }

        /// <summary>
        /// 重新连接PLC通信服务器连接
        /// </summary>
        public override void ReConnectServer()
        {
            // Send a welcome message to client
            string msg = "Welcome client " + m_clientCount + "\n";
            SendMsgToClient(msg, m_clientCount);

            // Let the worker Socket do the further processing for the 
            // just connected client
            WaitForData(m_socket, m_clientCount);

            // Since the main Socket is now free, it can go back and wait for
            // other clients who are attempting to connect
            m_socket.BeginAccept(new AsyncCallback(ConnectCallback), null);
        }

        /// <summary>
        /// 接收客户端发送过来的数据（用缓存来接收）
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="itemNum"></param>
        public override bool Read(ref byte[] buffer, int itemNum)
        {
            lock (m_socket)// 为了保证数据的完整性以及安全性  锁定数据流
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(ReadCallBack);
                }

                SocketError errorCode;
                // 调用异步方法 BeginReceive 来告知 socket 如何接收数据
                IAsyncResult iar = m_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out errorCode, pfnWorkerCallBack, null);

                return true;
            }
        }

        /// <summary>
        /// 向所有连接的客户端发送数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="itemNum"></param>
        /// <returns></returns>
        public override bool Write(object msg, int itemNum)
        {
            byte[] buffer = new byte[1024];

            if (msg.GetType() == typeof(byte[]))
            {
                buffer = (byte[])msg;
            }
            else if (msg.GetType() == typeof(string))
            {
                string str = (string)msg;
                buffer = Encoding.ASCII.GetBytes(str);
            }
            else if (msg.GetType() == typeof(int))
            {
                string str = "succeed send:" + (int)msg + "\n";
                buffer = Encoding.ASCII.GetBytes(str); ;
            }
            else
            {
                return false;
            }

            bool isSend = false;
            Socket workerSocket = null;
            for (int i = 0; i < m_workerSocketList.Count; i++)
            {
                workerSocket = (Socket)m_workerSocketList[i];
                if (null != workerSocket)
                {
                    if (workerSocket.Connected)
                    {
                        workerSocket.Send(buffer);
                        isSend = true;
                    }
                }
            }

            return isSend;
        }
        #endregion

        #region 私有函数
        /// <summary>
        /// 连接回调This is the call back function, which will be invoked when a client is connected
        /// </summary>
        /// <param name="asyn"></param>
        private void ConnectCallback(IAsyncResult asyn)
        {
            try
            {
                if (null == m_socket)
                {
                    return;
                }
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                Socket workerSocket = m_socket.EndAccept(asyn);

                // Now increment the client count for this client 
                // in a thread safe manner
                Interlocked.Increment(ref m_clientCount);

                // Add the workerSocket reference to our ArrayList
                m_workerSocketList.Add(workerSocket);

                // Send a welcome message to client
                string msg = "Welcome client " + m_clientCount + "\n";
                SendMsgToClient(msg, m_clientCount);

                // Let the worker Socket do the further processing for the 
                // just connected client
                WaitForData(workerSocket, m_clientCount);

                // Since the main Socket is now free, it can go back and wait for
                // other clients who are attempting to connect
                m_socket.BeginAccept(new AsyncCallback(ConnectCallback), null);
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
                CLOGException.Trace("函数CommunicationLib.CTcpServerAccess.ConnectCallback 异常" , CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            catch (SocketException ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpServerAccess.ConnectCallback 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 开始等待客户端数据Start waiting for data from the client
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="clientNumber"></param>
        private void WaitForData(System.Net.Sockets.Socket soc, int clientNumber)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(ReadCallBack);
                }
                SocketPacket theSocPkt = new SocketPacket(soc, clientNumber);

                soc.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpServerAccess.WaitForData 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 侦听到客户端数据回调his the call back function which will be invoked when the socket
        /// detects any client writing of data on the stream
        /// </summary>
        /// <param name="asyn"></param>
        private void ReadCallBack(IAsyncResult asyn)
        {
            SocketPacket socketData = (SocketPacket)asyn.AsyncState;
            try
            {
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                int iRx = socketData.m_currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                // Extract the characters as a buffer
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                
                // 构造事件
                ItemValueResult[] values = new ItemValueResult[1];
                values[0] = new ItemValueResult();
                values[0].Value = socketData.dataBuffer;
                values[0].ItemName = "MODBUS Server:" + socketData.m_currentSocket.LocalEndPoint.ToString();
                base.OnDataChange(null, null, values);

                // Send back the reply to the client
                string replyMsg = "Server Reply:" + szData.ToUpper();
                // Convert the reply to byte array
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(replyMsg);

                Socket workerSocket = (Socket)socketData.m_currentSocket;
                workerSocket.Send(byData);

                // Continue the waiting for data on the Socket
                WaitForData(socketData.m_currentSocket, socketData.m_clientNumber);

            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
                CLOGException.Trace("函数CommunicationLib.CTcpServerAccess.ReadCallBack 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10054) // Error code for Connection reset by peer
                {
                    // Remove the reference to the worker socket of the closed client
                    // so that this object will get garbage collected
                    m_workerSocketList[socketData.m_clientNumber - 1] = null;
                }
                CLOGException.Trace("函数CommunicationLib.CTcpServerAccess.ReadCallBack 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 发送数据到对应clientNumber客户端Send a message to client clientNumber
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="clientNumber"></param>
        private void SendMsgToClient(string msg, int clientNumber)
        {
            // Convert the reply to byte array
            byte[] byData = System.Text.Encoding.ASCII.GetBytes(msg);

            Socket workerSocket = (Socket)m_workerSocketList[clientNumber - 1];
            workerSocket.Send(byData);
        }
        #endregion
    }
}
