using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using Opc;
using Opc.Da;
using BaseMethodLib;
using LOGManagementLib;

namespace CommunicationLib
{
    /// <summary>
    /// OPC访问数据类
    /// </summary>
    public class COPCAccessTimer : CCommAbstact
    {
        private Opc.Da.Server m_server = null;// 定义数据存取服务器
        private Opc.Da.Subscription m_subscription = null;// 定义组对象（发送订阅者）
        private Opc.Da.SubscriptionState m_sendState = null;// 定义组（发送订阅者）状态，相当于OPC规范中组的参数
        private string m_groupName = "IEG";// 默认组名
        private string m_ipOrHostName = string.Empty;// 主机名
        private int m_updateRate = 1000;// 默认刷新频率为1秒。
        private Opc.IDiscovery m_discovery;// 发送定义枚举基于COM服务器的接口，用来搜索所有的此类服务器。
        private Timer m_timer = null;
        private List<ItemValueResult> m_lstItemValueResult = new List<ItemValueResult>();
        private List<Int16[]> m_lstAckTelegram = new List<Int16[]>();
        private bool m_isAckHandler = false;

        /// <summary>
        /// OPC访问数据类构造函数
        /// </summary>
        public COPCAccessTimer()
        {
            m_discovery = new OpcCom.ServerEnumerator();
        }

        /// <summary>
        /// OPC访问数据类构造函数
        /// </summary>
        public COPCAccessTimer(string ipOrHostName):this()
        {
            m_ipOrHostName = ipOrHostName;
        }

        /// <summary>
        /// OPC访问数据类构造函数
        /// </summary>
        public COPCAccessTimer(int rate):this()
        {
            m_updateRate = rate;
        }

        /// <summary>
        /// OPC访问数据类构造函数
        /// </summary>
        public COPCAccessTimer(string ipOrHostName, string rate):this()
        {
            m_ipOrHostName = ipOrHostName;
            CBaseMethods.MyBase.StringToUInt32(rate, out this.m_updateRate);
        }

        #region  重载函数
        /// <summary>
        /// OPC服务器连接与否属性
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (m_server != null)
                {
                    return m_server.IsConnected;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize(List<string> lstConnect)
        {
            // 打开OPC服务器
            ReConnectServer();

            if (IsConnected)
            {// 创建OPC订阅
                CreatSubscription(lstConnect);
            }

            m_timer = new Timer();
            m_timer.Interval = m_updateRate;//ms
            //m_timer.Start();
            m_timer.Enabled = true;
            // 计时器启用
            m_timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            // 定义多次循环
            m_timer.AutoReset = true;
            m_timer.Start();
            CLOGException.Trace("CommunicationLib.COPCAccessTimer.Initialize  ", " Initialize(1) successfully");
        }

        /// <summary>
        /// 初始化（是否添加定时器）
        /// </summary>
        public override void Initialize(List<string> lstConnect, bool isTimer)
        {
            // 打开OPC服务器
            ReConnectServer();

            if (IsConnected)
            {// 创建OPC订阅
                CreatSubscription(lstConnect);
            }

            m_isAckHandler = isTimer;
            if (isTimer)
            {
                m_timer = new Timer();
                m_timer.Interval = m_updateRate;//ms
                //m_timer.Start();
                m_timer.Enabled = true;
                // 计时器启用
                m_timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
                // 定义多次循环
                m_timer.AutoReset = true;
                m_timer.Start();
            }
            CLOGException.Trace("CommunicationLib.COPCAccessTimer.Initialize ", " Initialize(2) successfully isTimer:" + isTimer);
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public override void Dispose()
        {
            // 断开OPC服务器
            DisConnect();
            CLOGException.Trace("CommunicationLib.COPCAccessTimer.UnInitialize", "successfully");
        }

        /// <summary>
        /// 连接OPC服务器连接
        /// </summary>
        public override void ReConnectServer()
        {
            try
            {
                // 已经连接OPC服务器，退出
                if (IsConnected)
                {
                    CLOGException.Trace("CommunicationLib.COPCAccessTimer.ReConnectServer ", "successfully:" + IsConnected);
                    return;
                }

                string strIPOrHostName = (string.IsNullOrWhiteSpace(m_ipOrHostName)) ? Dns.GetHostName() : m_ipOrHostName;// 本机主机名

                // 查询服务器             
                Opc.Server[] servers = m_discovery.GetAvailableServers(Specification.COM_DA_20, strIPOrHostName, null);

                if (servers != null)
                {
                    foreach (Opc.Da.Server server in servers)
                    {
                        // server即为需要连接的OPC数据存取服务器。
                        if (String.Compare(server.Name, strIPOrHostName + ".OPC.SimaticNET", true) == 0)// 为true忽略大小写
                        {
                            m_server = server;// 获取OPC服务。
                            break;
                        }
                    }
                }
                // 连接服务器
                if (m_server != null)// 非空连接服务器
                {
                    m_server.Connect();
                }
                CLOGException.Trace("CommunicationLib.COPCAccessTimer.ReConnectServer", "successfully");
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.ReConnectServer 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16型数据
        /// </summary>
        /// <param name="iData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref Int16 iData, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }

                Item[] r_items = new Item[1];
                r_items[0] = m_subscription.Items[itemNum];
                ItemValueResult[] values = m_subscription.Read(r_items);

                if (values[0].Quality == Quality.Good)
                {
                    iData = System.Convert.ToInt16(values[0].Value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Read 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 读PLC字数据,返回一个Int16[]型数据
        /// </summary>
        /// <param name="intsData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref Int16[] intsData, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }

                Item[] r_items = new Item[1];
                r_items[0] = m_subscription.Items[itemNum];
                ItemValueResult[] values = m_subscription.Read(r_items);

                if (values[0].Quality == Quality.Good)
                {
                    intsData = (Int16[])values[0].Value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Read 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 读PLC字数据,返回一个byte[]型数据
        /// </summary>
        /// <param name="bData">返回数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Read(ref byte[] bData, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }

                Item[] r_items = new Item[1];
                r_items[0] = m_subscription.Items[itemNum];
                ItemValueResult[] values = m_subscription.Read(r_items);

                if (values[0].Quality == Quality.Good)
                {
                    bData = (byte[])values[0].Value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Read 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }

        /// <summary>
        /// 写PLC数据，写入1个object数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(object msg, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }

                ItemValue[] itemvalues = new ItemValue[1];
                itemvalues[0] = new ItemValue((Opc.ItemIdentifier)m_subscription.Items[itemNum]);
                itemvalues[0].Value = msg;

                Opc.IdentifiedResult[] idr = m_subscription.Write(itemvalues);
                if (idr != null && idr.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Write 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
       
        /// <summary>
        /// 写PLC数据，写入1个object其中的一个字DBW30,DBW32,DBW34数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(int nWord, int msg, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }
                
                Item[] r_items = new Item[1];
                r_items[0] = m_subscription.Items[itemNum];
                ItemValueResult[] values = m_subscription.Read(r_items);
               
                if (values[0].Quality == Quality.Good)
                {
                    if (typeof(byte[]) != values[0].Value.GetType())
                    {
                        return false;
                    }

                    byte[] bData = (byte[])values[0].Value;

                    if (nWord >= bData.Length || 1 > nWord)
                    {
                        return false;
                    }

                    byte[] bytes = Types.Int.ToByteArray(msg);
                    bData[nWord - 1] = bytes[0];
                    bData[nWord] = bytes[1];
                    ItemValue[] itemvalues = new ItemValue[1];
                    itemvalues[0] = new ItemValue((Opc.ItemIdentifier)m_subscription.Items[itemNum]);
                    itemvalues[0].Value = bData;
                    Opc.IdentifiedResult[] idr = m_subscription.Write(itemvalues);
                    if (idr != null && idr.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Write 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
        }
      
        /// <summary>
        /// 写PLC数据，写入1个object其中的2个字DBW30,DBW32数据
        /// </summary>
        /// <param name="msg">写入数据</param>
        /// <param name="itemNum">操作的Items，0-sendDB,1-sendflag,2-recvDB,3-recvflag,4-TV1alarm,5-TV1ResetAlarm,以此类推</param>
        /// <returns>true表示成功</returns>
        public override bool Write(int nStartWord, int msg30, int msg32, int itemNum)
        {
            try
            {
                if (m_subscription == null || m_subscription.Items == null || itemNum >= m_subscription.Items.Count())
                {
                    return false;
                }

                Item[] r_items = new Item[1];
                r_items[0] = m_subscription.Items[itemNum];
                ItemValueResult[] values = m_subscription.Read(r_items);

                if (values[0].Quality == Quality.Good)
                {
                    if (typeof(byte[]) != values[0].Value.GetType())
                    {
                        return false;
                    }

                    byte[] bData = (byte[])values[0].Value;

                    if (nStartWord >= bData.Length || nStartWord + 2 >= bData.Length || 1 > nStartWord)
                    {
                        return false;
                    }

                    byte[] bytes = Types.Int.ToByteArray(msg30);
                    bData[nStartWord - 1] = bytes[0];
                    bData[nStartWord] = bytes[1];
                    bytes = Types.Int.ToByteArray(msg32);
                    bData[nStartWord + 1] = bytes[0];
                    bData[nStartWord + 2] = bytes[1];
                    ItemValue[] itemvalues = new ItemValue[1];
                    itemvalues[0] = new ItemValue((Opc.ItemIdentifier)m_subscription.Items[itemNum]);
                    itemvalues[0].Value = bData;
                    Opc.IdentifiedResult[] idr = m_subscription.Write(itemvalues);
                    if (idr != null && idr.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Write 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return false;
            }
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
                    m_lstAckTelegram.Remove(ackTelegram);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region 私有函数
        /// <summary>
        /// 断开OPC服务器连接
        /// </summary>
        private void DisConnect()
        {
            if (m_server != null)
            {
                m_subscription.RemoveItems(m_subscription.Items);
                // 结束：释放各资源
                m_server.CancelSubscription(m_subscription);// m_server前文已说明，通知服务器要求删除组。        
                m_subscription.Dispose();// 强制.NET资源回收站回收该subscription的所有资源。         
                m_server.Disconnect();// 断开服务器连接
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
        /// 创建订阅，并添加Items
        /// </summary>
        private void CreatSubscription(List<string> lstConnect)
        {
            try
            {
                if (null == lstConnect)// || null != m_subscription)
                {
                    return;
                }

                if (null != m_subscription)
                {
                    if (0 < m_subscription.Items.Count() && lstConnect.Contains(m_subscription.Items.First().ItemName))
                    {
                        return;
                    }

                    #region 添加items
                    Item[] itemNames1 = new Item[lstConnect.Count];
                    string[] fConnects1 = lstConnect.ToArray();
                    for (int i = 0; i < itemNames1.Length; i++)
                    {
                        itemNames1[i] = new Item();// 创建一个项Item对象。
                        itemNames1[i].ClientHandle = Guid.NewGuid().ToString();// 客户端给该数据项分配的句柄。
                        itemNames1[i].ItemPath = null; // 该数据项在服务器中的路径。
                        itemNames1[i].ItemName = fConnects1[i];   // 该数据项在服务器中定义的名字
                        ItemValueResult itemValue = new ItemValueResult();
                        itemValue.ItemName = fConnects1[i];   // 该数据项在服务器中定义的名字
                        m_lstItemValueResult.Add(itemValue);
                    }
                    #endregion

                    m_subscription.AddItems(itemNames1);
                    return;
                }

                m_sendState = new SubscriptionState();
                m_sendState.Name = m_groupName;// 组名
                m_sendState.ServerHandle = null;// 服务器给该组分配的句柄。
                m_sendState.ClientHandle = Guid.NewGuid().ToString();// 客户端给该组分配的句柄。
                m_sendState.Active = true;// 激活该组。
                m_sendState.UpdateRate = m_updateRate;// 刷新频率
                m_sendState.Deadband = 0;// 死区值，设为0时，服务器端该组内任何数据变化都通知组。
                m_sendState.Locale = null;// 不设置地区值。
                m_subscription = (Opc.Da.Subscription)m_server.CreateSubscription(m_sendState);

                #region 添加items
                Item[] itemNames = new Item[lstConnect.Count];
                string[] fConnects = lstConnect.ToArray();
                for (int i = 0; i < itemNames.Length; i++)
                {
                    itemNames[i] = new Item();// 创建一个项Item对象。
                    itemNames[i].ClientHandle = Guid.NewGuid().ToString();// 客户端给该数据项分配的句柄。
                    itemNames[i].ItemPath = null; // 该数据项在服务器中的路径。
                    itemNames[i].ItemName = fConnects[i];   // 该数据项在服务器中定义的名字
                    ItemValueResult itemValue = new ItemValueResult();
                    itemValue.ItemName = fConnects[i];   // 该数据项在服务器中定义的名字
                    m_lstItemValueResult.Add(itemValue);
                }
                #endregion

                m_subscription.AddItems(itemNames);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.CreatSubscription 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 创建订阅，并添加Items   EventArgs
        /// </summary>
        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                //m_timer.Enabled = false;
                if (null == m_lstItemValueResult)
                {
                    CLOGException.Trace("CommunicationLib.COPCAccessTimer.Timer_Tick", "null == m_lstItemValueResult");
                    return;
                }

                if (!IsConnected)
                {// 重连
                    CLOGException.Trace("CommunicationLib.COPCAccessTimer.Timer_Tick", "重连");
                    // 打开OPC服务器
                    ReConnectServer();
                }

                if (!IsConnected)
                {// 退出
                    CLOGException.Trace("CommunicationLib.COPCAccessTimer.Timer_Tick", "退出");
                    return;
                }

                ItemValueResult item = null;
                string str = null;
                if (m_isAckHandler)
                {
                    // CTelegramHandler--ack
                    if (0 < m_lstItemValueResult.Count && null != m_lstAckTelegram)
                    {
                        item = m_lstItemValueResult[0];
                        str = item.ItemName;
                        int nCount = 0;
                        int.TryParse(str.Split(',').Last(), out nCount);
                        Int16[] obj = new Int16[nCount];
                        Int16 wflag = 0;
                        bool wisgood = Read(ref wflag, 1);// 读取PLC接收缓冲区标志位
                        if (!wisgood || 9999 != wflag || !Read(ref obj, 0))
                        {
                            item = null;
                            str = null;
                            obj = null;
                            return;
                        }

                        if (4 < nCount && 9999 == obj[4] && -1 == m_lstAckTelegram.IndexOf(obj))
                        {// ack
                            m_lstAckTelegram.Add(obj);
                        }
                    }
                    item = null;
                    str = null;
                    return;
                }

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
                                {// ack
                                    obj = null;
                                    //CLOGException.Trace("CommunicationLib.COPCAccessTimer.Timer_Tick ack", obj[0] + "|" + obj[1] + "|" + obj[2] + "|" + obj[3] + "|" + obj[4]);
                                    continue;
                                }
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
								//modify by suhan 20150729
                                //CLOGException.Trace("CommunicationLib.COPCAccessTimer.Timer_Tick i == 0", obj[0] + "|" + obj[1] + "|" + obj[2] + "|" + obj[3] + "|" + obj[4]);
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
                CLOGException.Trace("函数CommunicationLib.COPCAccessTimer.Timer_Tick 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
        #endregion
    }
}
