using System;
using System.Collections.Generic;
using System.Linq;
using TypedefLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommunicationLib;
using CommonMethodLib;
using LOGManagementLib;
using SoundManagementLib;
using BaseMethodLib;
using LEDManagementLib;

namespace AbstractActionLib
{
    /// <summary>
    /// 请求动作类
    /// </summary>
    public class CRequestAction
    {
        private static int m_nSendID = 1;// 发送报文ID(1-4999循环)
        private int m_nSendIDMax;//读取配置文件
        private List<CAbstractAction> lstAction;
        private CCommModule m_PLCServer;
        private object m_telegram;
        private struTelegram m_struTelegram;
        private List<struTelegram> m_lstTelegram;// 发送报文列表
        private string m_soundName;
        private List<struTableDto> m_lstTableDto;// SQL操作表格数据
        private string m_ledContent;// LED显示文本内容      
        private CCarLocationDto m_carLocationTBL;
        private CDeviceStatusDto m_deviceStatusTBL;
        private struHallEquips m_struHallEquip;
        private string m_soundUpdateContent;
        private Dictionary<int, CSound> m_dicSound;//语音播放对象  
      
        public CRequestAction()
        {           
            m_nSendIDMax = CConfigManagement.myPara.MaxTelegramID;//读取配置文件
            lstAction = new List<CAbstractAction>();
            m_PLCServer = null;
            m_telegram = null;
            m_lstTelegram = new List<struTelegram>();// 发送报文列表
            m_soundName = string.Empty;
            m_lstTableDto = new List<struTableDto>();// SQL操作表格数据
            m_ledContent = string.Empty;// LED显示文本内容
            m_struTelegram = new struTelegram();
            m_carLocationTBL = new CCarLocationDto();
            m_deviceStatusTBL = new CDeviceStatusDto();
            m_struHallEquip = new struHallEquips();
            m_soundUpdateContent = string.Empty; 
            m_dicSound = new Dictionary<int, CSound>();
          
        }

        #region 属性
        /// <summary>
        /// OPC服务器
        /// </summary>
        public CCommModule PLCServer
        {
            get
            {
                return m_PLCServer;
            }
            set
            {
                m_PLCServer = value;
            }
        }

        /// <summary>
        /// 发送报文内容
        /// </summary>
        public object telegram
        {
            get
            {
                return m_telegram;
            }
            set
            {
                m_telegram = value;
            }
        }

        /// <summary>
        /// 发送报文列表
        /// </summary>
        public List<struTelegram> lstTelegram
        {
            get
            {
                return m_lstTelegram;
            }
            set
            {
                m_lstTelegram = value;
            }
        }

        /// <summary>
        /// 语音名称
        /// </summary>
        public string soundName
        {
            get
            {
                return m_soundName;
            }
            set
            {
                m_soundName = value;
            }
        }

        /// <summary>
        /// SQL表格数据
        /// </summary>
        public List<struTableDto> lstTableDto
        {
            get
            {
                return m_lstTableDto;
            }
            set
            {
                m_lstTableDto = value;
            }
        }

        /// <summary>
        /// 报文结构体
        /// </summary>
        public struTelegram structTelegram
        {
            get
            {
                return m_struTelegram;
            }
            set
            {
                m_struTelegram = value;
            }
        }

        /// <summary>
        /// 设备状态实例
        /// </summary>
        public CDeviceStatusDto deviceStatusTBL
        {
            get
            {
                return m_deviceStatusTBL;
            }
            set
            {
                m_deviceStatusTBL = value;
            }
        }

        /// <summary>
        /// 设备状态实例
        /// </summary>
        public CCarLocationDto carLocationTBL
        {
            get
            {
                return m_carLocationTBL;
            }
            set
            {
                m_carLocationTBL = value;
            }
        }

        /// <summary>
        /// 车厅对应设备参数结构体实例
        /// </summary>
        public struHallEquips struHallEquip
        {
            get
            {
                return m_struHallEquip;
            }
            set
            {
                m_struHallEquip = value;
            }
        }

        /// <summary>
        /// 语音更新内容
        /// </summary>
        public string soundUpdateContent
        {
            get
            {
                return m_soundUpdateContent;
            }
            set
            {
                m_soundUpdateContent = value;
            }
        }

        /// <summary>
        /// 语音播放对象
        /// </summary>
        public Dictionary<int, CSound> DicSound
        {
            get
            {
                return m_dicSound;
            }
            set
            {
                m_dicSound = value;
            }
        }
        
        /// <summary>
        /// 报文ID
        /// </summary>
        public int MessageID
        {
            get
            {
                if (m_nSendID == 1) //开机时随机一个数
                {
                    m_nSendID = new Random().Next(10, 4000);
                }
                else if (m_nSendID > 4999)
                {
                    m_nSendID = 2;
                }
                else
                {
                    m_nSendID++;
                }
                return m_nSendID;
            }
        }
        #endregion

        /// <summary>
        /// 增加动作
        /// </summary>
        /// <param name="action"></param>
        public void SetAction(CAbstractAction action)
        {
            lstAction.Add(action);
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        public void SubmitAction()
        {
            foreach (CAbstractAction action in lstAction)
            {
                action.PerformAction(this);
            }
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        public void RequestAction(int nActionFlag, CAbstractAction sendTelegramAction, CAbstractAction modifyDBAction, CAbstractAction soundAction)
        {
            try
            {
                //根据执行动作的标志进行具体动作的准备
                if (0 == nActionFlag)
                {
                    return;
                }

                // 发报文
                if (1 == (nActionFlag & 1))
                {
                    SetAction(sendTelegramAction);
                    ConvertTelegram();
                }
                
                // 语音
                if (2 == (nActionFlag & 2))
                {
                    SetAction(soundAction);
                }

                // 修改数据库
                if (4 == (nActionFlag & 4))
                {
                    SetAction(modifyDBAction);
                }                
                //执行动作
                SubmitAction();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.RequestAction 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return;
            }
        }
   
        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        public void RequestAction(int nActionFlag)
        {
            try
            {
                //根据执行动作的标志进行具体动作的准备
                if (0 == nActionFlag)
                {
                    return;
                }

                // 发报文
                if (1 == (nActionFlag & 1))
                {
                    CAbstractAction sendTelegramAction = new CSendTelegramAction(); 
                    SetAction(sendTelegramAction);
                    ConvertTelegram();
                }               

                // 语音
                if (2 == (nActionFlag & 2))
                {
                    CAbstractAction soundAction = new CSoundAction();
                    SetAction(soundAction);
                }

                // 修改数据库
                if (4 == (nActionFlag & 4))
                {
                    CAbstractAction modifyDBAction = new CModifyDBAction();
                    SetAction(modifyDBAction);
                }
               
                //执行动作
                SubmitAction();
            }
            catch (Exception ex)
            {//打印日志
                CLOGException.Trace("WorkFlowLib.CWorkFlow.RequestAction2 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return;
            }
        }

        /// <summary>
        /// 报文转换
        /// </summary>
        public void ConvertTelegram()
        {
            if (telegram == null)
            {
                return;
            }

            // telegram格式为“1-54,13-1-297-2”
            string[] lstStrTelegram = telegram.ToString().Split(',');// ','分隔不同报文

            foreach (string strTelegram in lstStrTelegram)
            {
                string[] lstTypeTelegram = strTelegram.Split('-');// '-'分隔报文类型、子类型、设备故障字节位297->29.7bool字节

                if (lstTypeTelegram.Count() < 2)
                {
                    continue;
                }

                struTelegram struSendTelegram = m_struTelegram;
                struSendTelegram.nPLCSendFlag = 0;
                struSendTelegram.nEMSSendFlag = m_struTelegram.nPLCSendFlag;
                struSendTelegram.nTelegramType = Convert.ToInt32(lstTypeTelegram[0]);
                struSendTelegram.nSubTypeEMS = Convert.ToInt32(lstTypeTelegram[1]);
                struSendTelegram.nSubTypePLC = 0;

                if (lstTypeTelegram.Count() > 3)
                {// 分配的设备号
                    struSendTelegram.nEquipID = Convert.ToInt32(lstTypeTelegram[3]);
                }
                else
                {   
                    //默认
                    struSendTelegram.nEquipID = m_struTelegram.nEquipID;
                }
                struSendTelegram.strICCardID = m_struTelegram.strICCardID;
                //外形，轴距
                struSendTelegram.strCarSize = m_struTelegram.strCarSize;
                struSendTelegram.nWheelbase = m_struTelegram.nWheelbase;
                //中心偏移，后轮距巷道，车重
                struSendTelegram.offCenter = m_struTelegram.offCenter;
                struSendTelegram.rearWheelDis = m_struTelegram.rearWheelDis;
                struSendTelegram.carWeight = m_struTelegram.carWeight;
                //全车长度，前悬
                struSendTelegram.overallLg = m_struTelegram.overallLg;
                struSendTelegram.overHang = m_struTelegram.overHang;               

                struSendTelegram.strSrcLocAddr = m_struTelegram.strSrcLocAddr;
                struSendTelegram.strDestLocAddr = m_struTelegram.strDestLocAddr;
                struSendTelegram.nTelegramID = MessageID;
                struSendTelegram.nEndFlag = m_struTelegram.nEndFlag;
                //作业类型
                struSendTelegram.nMasterType = m_struTelegram.nMasterType;

                if (lstTypeTelegram.Count() > 2)
                {
                    struSendTelegram.nDeviceByteBit = Convert.ToInt32(lstTypeTelegram[2]);
                }
                else
                {
                    struSendTelegram.nDeviceByteBit = 0;
                }

                lstTelegram.Add(struSendTelegram);
            }
        }
    }
}
