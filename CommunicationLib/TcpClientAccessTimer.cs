using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Timers;
using Opc.Da;
using TypedefLib;
using System.Text.RegularExpressions;
using System.Net;
using BaseMethodLib;
using LOGManagementLib;

namespace CommunicationLib
{
    public class CTcpClientAccessTimer : CCommAbstact
    {
        private Socket _mSocket; //TCP connection to device
        private Timer m_timer = null;
        private int m_updateRate = 1000;// 默认刷新频率为1秒。
        private bool m_isConnected = false;
        private List<ItemValueResult> m_lstItemValueResult = new List<ItemValueResult>();
        private List<Int16[]> m_lstAckTelegram = new List<Int16[]>();
        private bool m_isAckHandler = false;

        public string IP { get; set; }
        public CpuType CPU { get; set; }
        public Int16 Rack { get; set; }
        public Int16 Slot { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }

        private int ackCount = 0;
        private short teleType = 0;
        private short subTeleType = 0;

        /// <summary>
        /// 如果有ACK要接收时，为真，否则为假
        /// </summary>
        private bool isAllowAckClear = true;

        public bool IsAvailable
        {
            get
            {
                Ping ping = new Ping();
                PingReply result = ping.Send(IP);
                bool bResult = result != null && result.Status == IPStatus.Success;
                if (bResult)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.IsAvailable", IP + result.Status);
                }
                ping = null;
                result = null;
                return bResult;
            }
        }
        public string LastErrorString { get; private set; }
        public ErrorCode LastErrorCode { get; private set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public CTcpClientAccessTimer() : this(CpuType.S7400, "localhost", 0, 2) { }

        /// <summary>
        /// 传入三个参数构造函数（IP地址、rack、slot槽号）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public CTcpClientAccessTimer(string ip, Int16 rack, Int16 slot, string name = "", object tag = null)
        {
            m_isConnected = false;
            IP = ip;
            CPU = CpuType.S7300;
            Rack = rack;
            Slot = slot;
            Name = name;
            Tag = tag;
        }

        /// <summary>
        /// 传入三个参数构造函数（IP地址、rack、slot槽号）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public CTcpClientAccessTimer(string ip, string rack, string slot)
        {
            m_isConnected = false;
            IP = ip;
            CPU = CpuType.S7300;
            short nSlot = 0, nRack = 0;
            CBaseMethods.MyBase.StringToUInt16(rack, out nRack);
            CBaseMethods.MyBase.StringToUInt16(slot, out nSlot);
            Rack = nRack;
            Slot = nSlot;
        }

        /// <summary>
        /// 传入四个参数构造函数（IP地址、rack、slot槽号、PLCDataUpdateRate）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public CTcpClientAccessTimer(string ip, string rack, string slot, string updateRate)
        {
            m_isConnected = false;
            IP = ip;
            CPU = CpuType.S7300;
            short nSlot = 0, nRack = 0 ;
            CBaseMethods.MyBase.StringToUInt16(rack, out nRack);
            CBaseMethods.MyBase.StringToUInt16(slot, out nSlot);
            Rack = nRack;
            Slot = nSlot;
            CBaseMethods.MyBase.StringToUInt32(updateRate, out this.m_updateRate);
        }

        /// <summary>
        /// 传入四个参数构造函数（CPU类型、IP地址、rack、slot槽号）
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        public CTcpClientAccessTimer(CpuType cpu, string ip, Int16 rack, Int16 slot, string name = "", object tag = null)
        {
            m_isConnected = false;
            IP = ip;
            CPU = cpu;
            Rack = rack;
            Slot = slot;
            Name = name;
            Tag = tag;
        }

        #region  重载函数
        public override bool IsAckAllowClear
        {
            get
            {
                return isAllowAckClear;
            }
            set
            {
                isAllowAckClear = value;
            }
        }

        /// <summary>
        /// PLC通信服务器连接与否属性
        /// </summary>
        public override bool IsConnected 
        {
            get 
            {
                return m_isConnected; 
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize(List<string> lstConnect)
        {
            // 打开TCP通信
            Open();

            // 添加items项
            m_lstItemValueResult.Clear();
            string[] fConnects = lstConnect.ToArray();
            for (int i = 0; i < fConnects.Length; i++)
            {
                ItemValueResult itemValue = new ItemValueResult() { ItemName = fConnects[i] /* 该数据项在服务器中定义的名字*/ };
                m_lstItemValueResult.Add(itemValue);
                itemValue = null;
            }
            fConnects = null;

            m_timer = new Timer()
            {
                Interval = m_updateRate,
                Enabled = true
            };
            // 计时器启用
            m_timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Initialize", string.Format("{0}successfully", IP));
          
        }

        /// <summary>
        /// 初始化（是否添加定时器）
        /// </summary>
        public override void Initialize(List<string> lstConnect, bool isTimer)
        {
            // 打开TCP通信
            Open();

            // 添加items项
            m_lstItemValueResult.Clear();
            string[] fConnects = lstConnect.ToArray();
            for (int i = 0; i < fConnects.Length; i++)
            {
                ItemValueResult itemValue = new ItemValueResult() { ItemName = fConnects[i] /* 该数据项在服务器中定义的名字*/ };
                m_lstItemValueResult.Add(itemValue);
                itemValue = null;
            }
            fConnects = null;

            m_isAckHandler = isTimer;
            CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Initialize", string.Format("{0}successfully, isTimer:{1}", IP, isTimer));
            if (isTimer)
            {
                while (true)
                {
                    Timer_Tick(this, null);
                    System.Threading.Thread.Sleep(m_updateRate);// CConfigManagement.myPara.ReadICCardRate);
                }
                //m_timer = new Timer() { Interval = m_updateRate /*ms*/, Enabled = true, /* 定义多次循环*/AutoReset = true };
                //// 计时器启用
                //m_timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            }
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public override void Dispose()
        {
            Close();
            CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.UnInitialize", string.Format("{0}successfully", IP));
        }

        /// <summary>
        /// 连接PLC通信服务器连接
        /// </summary>
        public override void ReConnectServer()
        {
            Open();
            CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.ReConnectServer", string.Format("{0}successfully", IP));
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16型数据
        /// </summary>
        /// <param name="iData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref Int16 iData, int itemNum)
        {            
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1001;
            int nStartAddr = 100;
            int nLen = 1;
            VarType varType = VarType.Int;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);

            object obj = ReadObject(DataType.DataBlock, nDB, nStartAddr, varType, nLen);

            if (null != obj && typeof(Int16) == obj.GetType())
            {
                iData = (Int16)obj;
                obj = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16[]型数据
        /// </summary>
        /// <param name="intsData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref Int16[] intsData, int itemNum)
        {
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1001;
            int nStartAddr = 0;
            int nLen = 50;
            VarType varType = VarType.Int;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);
            object obj = ReadObject(DataType.DataBlock, nDB, nStartAddr, varType, nLen);

            if (null != obj && typeof(Int16[]) == obj.GetType())
            {
                intsData = (Int16[])obj;
                obj = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 读PLC字数据,返回一个byte[]型数据
        /// </summary>
        /// <param name="bData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref byte[] bData, int itemNum)
        {
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1003;
            int nStartAddr = 0;
            int nLen = 54;
            VarType varType = VarType.Byte;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);
            object obj = ReadObject(DataType.DataBlock, nDB, nStartAddr, varType, nLen);

            if (null != obj && typeof(byte[]) == obj.GetType())
            {
                bData = (byte[])obj;
                obj = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 写PLC数据，写入1个object数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(object msg, int itemNum)
        {
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1001;
            int nStartAddr = 0;
            int nLen = 50;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);
            object obj = WriteObject(DataType.DataBlock, nDB, nStartAddr, msg);

            if (null != obj && typeof(ErrorCode) == obj.GetType() && ErrorCode.NoError == (ErrorCode)obj)
            {
                obj = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 写PLC数据，写入1个object其中的一个字DBW30,DBW32,DBW34数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(int nWord, int msg, int itemNum)
        {
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1003;
            int nStartAddr = 0;
            int nLen = 54;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);
            /* VarType varType = VarType.Byte;
            object obj = ReadObject(DataType.DataBlock, nDB, nStartAddr, varType, nLen);// 读取对应itemNum值
            if (null != obj && typeof(byte[]) == obj.GetType())
            {
                byte[] bData = (byte[])obj;
                if (nWord >= bData.Length || 1 > nWord)
                {
                    obj = null;
                    bData = null;
                    return false;
                }

                byte[] bytes = Types.Int.ToByteArray(msg);
                bData[nWord - 1] = bytes[0];
                bData[nWord] = bytes[1];
                obj = WriteObject(DataType.DataBlock, nDB, nStartAddr, bData);
                if (null != obj && typeof(ErrorCode) == obj.GetType() && ErrorCode.NoError == (ErrorCode)obj)
                {
                    obj = null;
                    bData = null;
                    bytes = null;
                    return true;
                }
            }*/
            byte[] bytes = Types.Int.ToByteArray(msg);
            object obj = WriteObject(DataType.DataBlock, nDB, nWord - 1, bytes);
            if (null != obj && typeof(ErrorCode) == obj.GetType() && ErrorCode.NoError == (ErrorCode)obj)
            {
                obj = null;
                bytes = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 写PLC数据，写入1个object其中的2个字DBW30,DBW32数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(int nStartWord, int msg30, int msg32, int itemNum)
        {
            if (null == _mSocket || !_mSocket.Connected)
            {// TCP连接设备未打开
                return false;
            }

            int nDB = 1003;
            int nStartAddr = 0;
            int nLen = 54;
            GetItemDBAddress(itemNum, ref nDB, ref nStartAddr, ref nLen);
            //VarType varType = VarType.Byte;
            /*object obj = ReadObject(DataType.DataBlock, nDB, nStartAddr, varType, nLen);// 读取对应itemNum值
            if (null != obj && typeof(byte[]) == obj.GetType())
            {
                byte[] bData = (byte[])obj;
                if (nStartWord >= bData.Length || nStartWord + 2 >= bData.Length || 1 > nStartWord)
                {
                    obj = null;
                    bData = null;
                    return false;
                }

                byte[] bytes = Types.Int.ToByteArray(msg30);
                bData[nStartWord - 1] = bytes[0];
                bData[nStartWord] = bytes[1];
                bytes = Types.Int.ToByteArray(msg32);
                bData[nStartWord + 1] = bytes[0];
                bData[nStartWord + 2] = bytes[1];
                obj = WriteObject(DataType.DataBlock, nDB, nStartAddr, bData);
                if (null != obj && typeof(ErrorCode) == obj.GetType() && ErrorCode.NoError == (ErrorCode)obj)
                {
                    obj = null;
                    bData = null;
                    bytes = null;
                    return true;
                }
            }*/
            byte[] bytes = Types.Int.ToByteArray(msg30);
            byte[] bData = new byte[4];
            bData[0] = bytes[0];
            bData[1] = bytes[1];
            bytes = Types.Int.ToByteArray(msg32);
            bData[2] = bytes[0];
            bData[3] = bytes[1];
            object obj = WriteObject(DataType.DataBlock, nDB, nStartWord - 1, bData);
            if (null != obj && typeof(ErrorCode) == obj.GetType() && ErrorCode.NoError == (ErrorCode)obj)
            {
                obj = null;
                bData = null;
                bytes = null;
                return true;
            }
            obj = null;
            return false;
        }

        /// <summary>
        /// 判断发送的报文与PLC回应的ACK是否一致,返回值：true-收到ACK，false-未收到ACK
        /// </summary>
        /// <param name="sendTelegram"></param>
        /// <param name="ackTelegram"></param>
        /// <returns></returns>
        public override bool IsEMSSendAck(Int16[] sendTelegram)
        {
            if (null == m_lstAckTelegram)
            {
                return true;
            }

            foreach (Int16[] ackTelegram in m_lstAckTelegram)
            {
                if (CBaseMethods.MyBase.IsPLCAck(sendTelegram, ackTelegram))
                {
                    //m_lstAckTelegram.RemoveAll(s =>s == ackTelegram);
                    m_lstAckTelegram.Remove(ackTelegram);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region 私有函数
        /// <summary>
        /// 打开TCP/IP连接
        /// </summary>
        /// <returns></returns>
        public ErrorCode Open()
        {
            if (_mSocket != null && _mSocket.Connected)
            {
                _mSocket.Close();
                _mSocket.Dispose();
                m_isConnected = false;
            }

            try
            {
                // check if available
                if (!IsAvailable)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Open", IP + "：Ping失败");
                    return ErrorCode.ConnectionError;
                }
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.IPAddressNotAvailable;
                LastErrorString = string.Format("Destination IP-Address '{0}' is not available!", IP);
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.Open 异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                return LastErrorCode;
            }

            byte[] bReceive = new byte[256];
            try
            {
                // open the channel
                _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                _mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);

                IPEndPoint server = new IPEndPoint(IPAddress.Parse(IP), 102);
                _mSocket.Connect(server);
                server = null;
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.ConnectionError;
                LastErrorString = ex.Message;
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.Open Ping异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                return ErrorCode.ConnectionError;
            }

            try
            {
                byte[] bSend1 = { 3, 0, 0, 22, 17, 224, 0, 0, 0, 46, 0, 193, 2, 1, 0, 194, 2, 3, 0, 192, 1, 9 };

                switch (CPU)
                {
                    case CpuType.S7200:
                        //S7200: Chr(193) & Chr(2) & Chr(16) & Chr(0) 'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 16;
                        bSend1[14] = 0;
                        //S7200: Chr(194) & Chr(2) & Chr(16) & Chr(0) 'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 16;
                        bSend1[18] = 0;
                        break;
                    case CpuType.S71200:
                    case CpuType.S7300:
                        //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    case CpuType.S7400:
                        //S7400: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7400: Chr(194) & Chr(2) & Chr(3) & Chr(3)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    default:
                        return ErrorCode.WrongCPU_Type;
                }

                _mSocket.Send(bSend1, 22, SocketFlags.None);
                if (_mSocket.Receive(bReceive, 22, SocketFlags.None) != 22)
                {
                    bSend1 = null;
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Open", IP + "：连接DB块1失败");
                    return ErrorCode.WrongNumberReceivedBytes;
                }

                byte[] bsend2 = { 3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 3, 0, 3, 1, 0 };
                _mSocket.Send(bsend2, 25, SocketFlags.None);

                if (_mSocket.Receive(bReceive, 27, SocketFlags.None) != 27)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Open", IP + "：连接DB块2失败");
                    bSend1 = null;
                    bsend2 = null;
                    return ErrorCode.WrongNumberReceivedBytes;
                }
                m_isConnected = true;
                bSend1 = null;
                bsend2 = null;
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.ConnectionError;
                LastErrorString = string.Format("Couldn't establish the connection to {0}!", IP);
                m_isConnected = false;
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.Open Send异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                return ErrorCode.ConnectionError;
            }

            return ErrorCode.NoError;
        }

        /// <summary>
        /// 关闭TCP/IP连接
        /// </summary>
        public void Close()
        {
            if (_mSocket != null)// && _mSocket.Connected)
            {
                _mSocket.Close();
                _mSocket.Dispose();
                m_isConnected = false;
            }

            if (null != m_timer)
            {
                m_timer.Stop();
                m_timer.Close();
                m_timer.Dispose();
            }

            m_lstItemValueResult = null;
            m_lstAckTelegram = null;
        }

        /// <summary>
        /// 通过TCP/IP从对应PLC中的DB块的对应地址中读取byte[]类型数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="DB"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(DataType dataType, int DB, int startByteAdr, int count)
        {
            byte[] bytes = new byte[count];

            try
            {
                // first create the header
                const int packageSize = 31;
                Types.ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 0x03, 0x00, 0x00 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x04, 0x01, 0x12, 0x0a, 0x10 });
                // package.Add(0x02);  // datenart
                switch (dataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add((byte)dataType);
                        break;
                    default:
                        package.Add(0x02);
                        break;
                }

                package.Add(Types.Word.ToByteArray((ushort)(count)));
                package.Add(Types.Word.ToByteArray((ushort)(DB)));
                package.Add((byte)dataType);
                package.Add((byte)0);
                switch (dataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add(Types.Word.ToByteArray((ushort)(startByteAdr)));
                        break;
                    default:
                        package.Add(Types.Word.ToByteArray((ushort)((startByteAdr) * 8)));
                        break;
                }

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                byte[] bReceive = new byte[512];
                _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.ReadBytes ", IP + " ErrorCode.WrongNumberReceivedBytes");
                    return null;
                }
                /*// -----add by wanxiaona20150910
                string data = string.Empty;
                byte[] temp = new byte[count];
                for (int i = 0; i < bReceive.Length && i < count + 25; i++)
                {
                    data += string.Format("[{0}]", bReceive[i]);
                    if (i< count)
                    {
                        temp[i] = bReceive[i + 25];
                    }
                }
                CLOGException.Trace(0, "CommunicationLib.CTcpClientAccessTimer.ReadBytes ", string.Format("IP:{0} DB:{1} bReceive:{2}", IP, DB, data));
                data = string.Empty;
                Int16[] intemp = Types.Int.ToArray(temp);
                for (int i = 0; i < intemp.Length; i++)
                {
                    data += string.Format("[{0}]", intemp[i]);
                }
                CLOGException.Trace(0, "CommunicationLib.CTcpClientAccessTimer.ReadBytes ", string.Format("IP:{0} DB:{1} bReceiveint:{2}", IP, DB, data));
                 // -----end by wanxiaona20150910*/

                for (int cnt = 0; cnt < count; cnt++)
                    bytes[cnt] = bReceive[cnt + 25];

                return bytes;
            }
            catch (SocketException ex)
            {
                m_isConnected = false;
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = ex.Message;
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.ReadBytes SocketException异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                bytes = null;
                return null;
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = ex.Message;
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.ReadBytes Exception异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                bytes = null;
                return null;
            }
        }

        /// <summary>
        /// 通过TCP/IP从对应PLC中的DB块的对应地址中读取object数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="varType"></param>
        /// <param name="varCount"></param>
        /// <returns></returns>
        public object ReadObject(DataType dataType, int db, int startByteAdr, VarType varType, int varCount)
        {
            try
            {
                byte[] bytes = null;
                int cntBytes = 0;

                switch (varType)
                {
                    case VarType.Byte:
                        {
                            cntBytes = varCount;
                            if (cntBytes < 1) cntBytes = 1;
                            bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                            if (bytes == null) return null;
                            if (varCount == 1)
                                return bytes[0];
                            else
                                return bytes;
                        }
                    case VarType.Word:
                        {
                            cntBytes = varCount * 2;
                            bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                            if (bytes == null) return null;

                            if (varCount == 1)
                                return Types.Word.FromByteArray(bytes);
                            else
                                return Types.Word.ToArray(bytes);
                        }
                    case VarType.Int:
                        {
                            cntBytes = varCount * 2;
                            bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                            if (bytes == null) return null;

                            if (varCount == 1)
                                return Types.Int.FromByteArray(bytes);
                            else
                                return Types.Int.ToArray(bytes);
                        }
                    case VarType.DWord:
                        {
                            cntBytes = varCount * 4;
                            bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                            if (bytes == null) return null;

                            if (varCount == 1)
                                return Types.DWord.FromByteArray(bytes);
                            else
                                return Types.DWord.ToArray(bytes);
                        }
                    case VarType.DInt:
                        {
                            cntBytes = varCount * 4;
                            bytes = ReadBytes(dataType, db, startByteAdr, cntBytes);
                            if (bytes == null) return null;

                            if (varCount == 1)
                                return Types.DInt.FromByteArray(bytes);
                            else
                                return Types.DInt.ToArray(bytes);
                        }
                    default:
                        {
                            return null;
                        }
                }
            }
            catch (Exception ex) 
            {
                m_isConnected = false;
                throw ex;
            }
        }

        /// <summary>
        /// 通过TCP/IP写入byte[]类型数据到对应PLC中的DB块的对应地址里
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ErrorCode WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {
            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                /*// -----add by wanxiaona20150909
                string data = string.Empty;
                for (int i = 0; i < value.Length; i++)
                {
                    data += string.Format("[{0}]", value[i]);
                }
                CLOGException.Trace(0, "CommunicationLib.CTcpClientAccessTimer.WriteBytes ", string.Format("IP:{0} db:{1} startByteAdr:{2} DATA:{3}", IP, db, startByteAdr, data));
                // -----end by wanxiaona20150909*/
                
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                Types.ByteArray package = new Types.ByteArray(packageSize);

                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount - 1)));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Types.Word.ToByteArray((ushort)(varCount + 4)));
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
                package.Add(Types.Word.ToByteArray((ushort)varCount));
                package.Add(Types.Word.ToByteArray((ushort)(db)));
                package.Add((byte)dataType);
                package.Add((byte)0);
                package.Add(Types.Word.ToByteArray((ushort)(startByteAdr * 8)));
                package.Add(new byte[] { 0, 4 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount * 8)));

                // now join the header and the data
                package.Add(value);

                _mSocket.Send(package.array, package.array.Length, SocketFlags.None);

                _mSocket.Receive(bReceive, 512, SocketFlags.None);
                if (bReceive[21] != 0xff)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.WriteBytes ", IP + " ErrorCode.WrongNumberReceivedBytes");
                    return ErrorCode.WrongNumberReceivedBytes;
                }

                return ErrorCode.NoError;
            }
            catch (Exception ex)
            {
                LastErrorCode = ErrorCode.WriteData;
                LastErrorString = "";
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.WriteBytes 异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));
                return LastErrorCode;
            }
        }

        /// <summary>
        /// 通过TCP/IP写入object数据到对应PLC中的DB块的对应地址里
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object WriteObject(DataType dataType, int db, int startByteAdr, object value)
        {
            if (null == value)
            {
                return ErrorCode.WriteData;
            }

            byte[] package = null;

            switch (value.GetType().Name)
            {
                case "Int16":
                    {
                        package = Types.Int.ToByteArray((Int16)value);
                        break;
                    }
                case "UInt16":
                    {
                        package = Types.Word.ToByteArray((UInt16)value);
                        break;
                    }
                case "Int32":
                    {
                        //package = Types.DInt.ToByteArray((Int32)value);
                        package = Types.Int.ToByteArray((int)value);
                        break;
                    }
                case "UInt32":
                    {
                        package = Types.DWord.ToByteArray((UInt32)value);
                        break;
                    }
                case "Byte[]":
                    {
                        package = (byte[])value;
                        break;
                    }
                case "Int16[]":
                    {
                        package = Types.Int.ToByteArray((Int16[])value);
                        break;
                    }
                case "UInt16[]":
                    {
                        package = Types.Word.ToByteArray((UInt16[])value);
                        break;
                    }
                case "Int32[]":
                    {
                        package = Types.DInt.ToByteArray((Int32[])value);
                        break;
                    }
                case "UInt32[]":
                    {
                        package = Types.DWord.ToByteArray((UInt32[])value);
                        break;
                    }
                default:
                    {
                        return ErrorCode.WrongVarFormat;
                    }
            }
            return WriteBytes(dataType, db, startByteAdr, package);
        }

        /// <summary>
        /// 创建订阅，并添加Items   EventArgs
        /// </summary>
        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {               
                if (null == m_lstItemValueResult)
                {
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Timer_Tick", IP + "null == m_lstItemValueResult");                   
                    return;
                }

                if (!m_isConnected)
                {// 重连
                    //CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Timer_Tick", IP + "重连");
                    Open();
                }

                if (!m_isConnected)
                {// 退出
                    CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Timer_Tick", IP + "退出");                   
                    return;
                }

                ItemValueResult item = null;
                string str = null;

                #region
                //if (m_isAckHandler)
                //{
                //    // CTelegramHandler--ack
                //    if (0 < m_lstItemValueResult.Count && null != m_lstAckTelegram)
                //    {
                //        item = m_lstItemValueResult[0];
                //        str = item.ItemName;
                //        int nCount = 0;
                //        int.TryParse(str.Split(',').Last(), out nCount);
                //        Int16[] obj = new Int16[nCount];
                //        Int16 wflag = 0;
                //        bool wisgood = Read(ref wflag, 1);// 读取PLC接收缓冲区标志位
                //        if (!wisgood || 9999 != wflag || !Read(ref obj, 0))
                //        {
                //            item = null;
                //            str = null;
                //            obj = null;
                //            return;
                //        }

                //        if (4 < nCount && 9999 == obj[4] && -1 == m_lstAckTelegram.IndexOf(obj))
                //        {// ack
                //            m_lstAckTelegram.Add(obj);
                //        }
                //    }
                //    item = null;
                //    str = null;
                //    return;
                //}
                #endregion

                Regex regexInts = new Regex(",INT[0-9]{1,},");
                Regex regexBytes = new Regex(",B[0-9]{1,},");
                // 报文解析
                List<ItemValueResult> values = new List<ItemValueResult>();
                for (int i = 0; i < m_lstItemValueResult.Count; i++)
                {
				    if(0 < i && i < 4)
					{
					    continue;
					}					
                    item = m_lstItemValueResult[i];
                    str = item.ItemName;
                    if (0 == i)
                    {
                        // 读取DB1001INT数组需要先判断PLC标志位是否可读  9999
                        Int16 wflag = 0;
                        bool wisgood = Read(ref wflag, 1);// 读取PLC接收缓冲区标志位
                        if (!wisgood || 9999 != wflag)
                        {
                            continue;
                        }
                    }

                    if (regexInts.IsMatch(str))//)str.Contains(",INT0"))
                    {
                        int nCount = 0;
                        int.TryParse(str.Split(',').Last(), out nCount);
                        if (1 < nCount)
                        {
                            Int16[] obj = new Int16[nCount];
                            if (!Read(ref obj, i))
                            {
                                obj = null;
                                continue;
                            }
                            if (0 == i)
                            {
                                if (4 < nCount && 9999 == obj[4])
                                {
                                    //收到ACK，但这个时侯不需要ACK，则直接丢弃
                                    if (isAllowAckClear) 
                                    {
                                        Write(0,1);
                                    }
                                    // ack
                                    if (obj[2] == teleType && obj[3] == subTeleType)
                                    {
                                        ackCount++;
                                    }
                                    else
                                    {
                                        teleType = obj[2];
                                        subTeleType = obj[3];
                                    }
                                    //用于处理ACK不接收，导致PLC发送通道堵塞
                                    if (ackCount == 15)
                                    {
                                        ackCount = 0;
                                        Write(0, 1);//更新PLC发送缓冲区标志位为0
                                        CLOGException.Trace("CommunicationLib.CTcpClientAccessTimer.Timer_Tick ack堵塞PLC发送通道 ", "  报文信息 |" + obj[0] + "|" + obj[1] + "|" + obj[2] + "|" + obj[3] + "|" + obj[4]);
                                    }

                                    obj = null;                                 
                                    continue;
                                }

                                ackCount = 0;
                                teleType = 0;
                                subTeleType = 0;
                                // 读取DB1001INT数组成功后也需要写入PLC标志位 0
                                Write(0, 1);//更新PLC发送缓冲区标志位为0
                            }
                            if (null == item.Value)
                            {
                                item.Value = obj;
                            }
                            else if (!CBaseMethods.MyBase.IsEqualValue(item.Value, obj))
                            {
                                item.Value = obj;
                                values.Add(item);
                            }
                        }
                        else if (1 == nCount)
                        {
                            Int16 obj = new Int16();
                            if (!Read(ref obj, i))
                            {
                                continue;
                            }

                            if (null == item.Value)
                            {
                                item.Value = obj;
                            }
                            else if (!CBaseMethods.MyBase.IsEqualValue(item.Value, obj))
                            {
                                item.Value = obj;
                                values.Add(item);
                            }
                        }
                    }
                    else if (regexBytes.IsMatch(str))//str.Contains(",B0"))
                    {
                        int nCount = 0;
                        int.TryParse(str.Split(',').Last(), out nCount);
                        if (1 > nCount)
                        {
                            continue;
                        }
                        byte[] obj = new byte[nCount];
                        if (!Read(ref obj, i))
                        {
                            obj = null;
                            continue;
                        }

                        if (null == item.Value)
                        {
                            item.Value = obj;
                            values.Add(item);
                        }
                        else if (!CBaseMethods.MyBase.IsEqualValue(item.Value, obj))
                        {
                            item.Value = obj;
                            values.Add(item);
                        }
                    }
                }

                if (0 < values.Count)
                {
                    base.OnDataChange(sender, this, values.ToArray());
                }
                item = null;
                str = null;
                regexInts = null;
                regexBytes = null;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.CTcpClientAccessTimer.Timer_Tick 异常," + IP, CBaseMethods.MyBase.GetExceptionInfo(ex));                
            }
        }

        /// <summary>
        /// 根据Item索引获取数据块DB值、地址、长度
        /// </summary>
        /// <param name="itemNum"></param>
        /// <param name="nDB"></param>
        /// <param name="nStartAddr"></param>
        /// <param name="nLen"></param>
        private void GetItemDBAddress(int itemNum, ref int nDB, ref int nStartAddr, ref int nLen)
        {
            if (itemNum >= m_lstItemValueResult.Count || itemNum < 0)
            {
                return;
            }

            ItemValueResult item = m_lstItemValueResult[itemNum];
            string str = item.ItemName;
            int nIndex = str.IndexOf("DB");
            if (-1 == nIndex)
            {
                return;
            }

            str = str.Substring(nIndex + 2);
            str = str.Replace("INT", "");
            str = str.Replace("B", "");
            string[] strs = str.Split(',');

            if (2 == strs.Count())
            {
                int.TryParse(strs[0], out nDB);
                int.TryParse(strs[1], out nStartAddr);
                nLen = 1;
            }
            else if (3 == strs.Count())
            {
                int.TryParse(strs[0], out nDB);
                int.TryParse(strs[1], out nStartAddr);
                int.TryParse(strs[2], out nLen);
            }
        }
        #endregion
    }
}
