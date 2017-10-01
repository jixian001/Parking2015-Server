using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConfigManagmtLib;
using BaseMethodLib;
using LOGManagementLib;
using CommonMethodLib;
using SoundManagementLib;
using DatabaseManagementLib;
using LEDManagementLib;
using TypedefLib;

namespace WorkFlowLib
{
    public class CMainEntrance : IDisposable
    {
        private CMainCallback m_mainCallback;
        private Thread[] m_PLCThreads = null;
        private List<int> m_lstPLCID;
        private CWorkFlow[] m_workFlow = null;
        private Dictionary<int, Dictionary<int, CLedShower>> m_dicLEDdevieLst;

        public CMainEntrance(CMainCallback mainCallback)
        {
            CSound soundDevice = new CSoundSpeech();
            string strPathAuto = System.AppDomain.CurrentDomain.BaseDirectory + @"/soundAuto/";// 自动配好的语音文件夹路径(读取配置文件)

            List<CSoundDto> lstSoundTBL = new List<CSoundDto>();
            lstSoundTBL = CCommonMethods.myMethods.GetSoundList();
            foreach (CSoundDto table in lstSoundTBL)
            {
                soundDevice.CreatSoundFile(strPathAuto, table.soundcode, table.soundcontent);
            }

            m_dicLEDdevieLst = new Dictionary<int, Dictionary<int, CLedShower>>();

            m_mainCallback = mainCallback;
            m_PLCThreads = new Thread[CConfigManagement.myPara.PLCCount];
            m_lstPLCID = CConfigManagement.myPara.LstPLCID;
            m_workFlow = new CWorkFlow[CConfigManagement.myPara.PLCCount];          
            CCommonMethods.m_mainCallback = mainCallback;
        }

        /// <summary>
        /// 初始化函数
        /// </summary>
        public void Init()
        {
            try
            {
                #region Led绑定
                for (int nCounter = 0; nCounter < m_lstPLCID.Count; nCounter++)
                {
                    Dictionary<int, CLedShower> dicHallHasLED = new Dictionary<int, CLedShower>();

                    Dictionary<int, struHallEquips> dictHallID = (Dictionary<int, struHallEquips>)CConfigManagement.myPara.DicHallIDDictionary[m_lstPLCID[nCounter]];//.GetHallIDDictionary(m_nPLCID);                    
                    foreach (KeyValuePair<int, struHallEquips> pair in dictHallID)
                    {
                        struHallEquips hallEquip = pair.Value;
                        if (hallEquip.nLEDAddress != 0)
                        {
                            CLedShower led = new CLedShower(hallEquip.nLEDAddress);
                            dicHallHasLED.Add(pair.Key, led);
                            CLOGException.Trace("AddLEDDevice", string.Format("Info- warehouse:{0},hallID:{1},ledCom:{2}", nCounter + 1, pair.Key, hallEquip.nLEDAddress));
                        }
                    }
                    m_dicLEDdevieLst.Add(m_lstPLCID[nCounter], dicHallHasLED);
                }
                #endregion

                for (int nCounter = 0; nCounter < m_lstPLCID.Count; nCounter++)
                {
                    m_PLCThreads[nCounter] = new Thread(StartThread);//启动业务线程
                    if (null != m_PLCThreads[nCounter])
                    {
                        CLOGException.Trace(m_lstPLCID[nCounter], "IEG_AP_Proc", "Start programme...(Ver:AutoParkingV200R001C01SPC100|AutoParkingV200R001C01SPC100)");
                        m_PLCThreads[nCounter].IsBackground = true;//后台线程在主线程关闭时，子线程随之关闭，速度很快。
                        m_PLCThreads[nCounter].Start(m_lstPLCID[nCounter]);
                    }
                }              
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CMainEntrance.Init 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                for (int nCounter = 0; nCounter < m_lstPLCID.Count; nCounter++)
                {
                    m_workFlow[nCounter].Dispose();
                    if (null != m_PLCThreads[nCounter] && m_PLCThreads[nCounter].ThreadState == ThreadState.Running)
                    {
                        m_PLCThreads[nCounter].Abort();
                    }
                    CLOGException.Trace(m_lstPLCID[nCounter], "IEG_AP_Proc", "Stop programme");
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CMainEntrance.Init 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }

        }

        private void StartThread(object nPLCID)
        {
            try
            {               
                for (int nCounter = 0; nCounter < m_lstPLCID.Count; nCounter++)
                {
                    if ((int)nPLCID == m_lstPLCID[nCounter])
                    {
                        m_workFlow[nCounter] = new CWorkFlow((int)nPLCID, m_mainCallback);                       
                        m_workFlow[nCounter].Init(m_dicLEDdevieLst[m_lstPLCID[nCounter]]);
                        m_workFlow[nCounter].Start();
                    }
                }              
            }
            catch (Exception ex)
            {
                CLOGException.Trace("WorkFlowLib.CMainEntrance.StartThread 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
