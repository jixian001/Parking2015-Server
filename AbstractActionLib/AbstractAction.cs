using System;
using System.Collections.Generic;
using System.Threading;
using BaseMethodLib;
using LOGManagementLib;

namespace AbstractActionLib
{
    /// <summary>
    /// 执行动作
    /// </summary>
    public abstract class CAbstractAction
    {
        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="RequestParam"></param>
        public abstract void PerformAction(CRequestAction RequestParam);

    }

    #region 发送报文类--1
    /// <summary>
    /// 发送报文动作
    /// </summary>
    public class CSendTelegramAction : CAbstractAction
    {
        private CTelegramSender telegramSender;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CSendTelegramAction()
        {
            telegramSender = new CTelegramSender();
        }

        /// <summary>
        /// 重载执行工作
        /// </summary>
        /// <param name="RequestParam"></param>
        public override void PerformAction(CRequestAction RequestParam)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(telegramSender.TelegramSender), RequestParam);
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CSendTelegramAction.PerformAction 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
    #endregion

    #region 语音类--2
    /// <summary>
    /// 语音工作
    /// </summary>
    public class CSoundAction : CAbstractAction
    {
        private CVoicePlayer m_voicePlayer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nPLCID"></param>
        /// <param name="mainCallback"></param>
        public CSoundAction()
        {
            m_voicePlayer = new CVoicePlayer();
        }

        /// <summary>
        /// 重载执行工作
        /// </summary>
        /// <param name="RequestParam"></param>
        public override void PerformAction(CRequestAction RequestParam)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(m_voicePlayer.VoicePlayer), RequestParam);
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CSoundAction.PerformAction 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
    #endregion

    #region SQL操作类--4
    /// <summary>
    /// SQL操作类
    /// </summary>
    public class CModifyDBAction : CAbstractAction
    {
        private CDBEditor m_dbHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CModifyDBAction()
        {
            m_dbHandler = new CDBEditor();
        }

        /// <summary>
        /// 重载执行工作
        /// </summary>
        /// <param name="RequestParam"></param>
        public override void PerformAction(CRequestAction RequestParam)
        {
            try
            {
                m_dbHandler.DBEditor(RequestParam);
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CModifyDBAction.PerformAction 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
    #endregion  
}
