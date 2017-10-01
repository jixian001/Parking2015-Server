using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.ServiceModel;
using WCFServiceLib;
using System.ServiceModel.Description;
using WCFQueryLib;
using CommonMethodLib;
using LOGManagementLib;
using BaseMethodLib;

namespace KernelGuideLib
{
    /// <summary>
    /// WCF服务管理类
    /// </summary>
    public class CWCFManagement : IDisposable
    {
        private CMainCallback m_mainCallback;
        private Thread m_queryServiceThread;//查询服务线程
        private Thread m_pushServiceThread; //推送服务线程
      
        ServiceHost m_queryHost;
        ServiceHost m_pushHost;
       
        /// <summary>
        /// 构造函数
        /// </summary>
        public CWCFManagement(CMainCallback mainCallback)
        {
            //CLOGException.Trace("——————CWCFManagement");
            m_mainCallback = mainCallback;
        }

        /// <summary>
        /// 启动线程
        /// </summary>
        public void StartThreads()
        {
            //CLOGException.Trace("——————StartThreads");
            m_queryServiceThread = new Thread(new ThreadStart(OpenQueryService)) 
            { 
                IsBackground = true /*后台线程在主线程关闭时，子线程随之关闭，速度很快。*/
            };
            m_queryServiceThread.Start();

            m_pushServiceThread = new Thread(new ThreadStart(OpenPushService)) 
            { 
                IsBackground = true /*后台线程在主线程关闭时，子线程随之关闭，速度很快。*/
            };
            m_pushServiceThread.Start();

            //m_appServiceThread = new Thread(new ThreadStart(OpenAppService)) { IsBackground = true /*后台线程在主线程关闭时，子线程随之关闭，速度很快。*/ };
            //m_appServiceThread.Start();
        }

        /// <summary>
        /// 注销并释放分配的资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                CloseQueryService();
                ClosePushService();
                //CloseAppService();

           
                if (null != m_queryServiceThread && m_queryServiceThread.ThreadState == ThreadState.Running)
                {
                    m_queryServiceThread.Abort();
                }
                if (null != m_pushServiceThread && m_pushServiceThread.ThreadState == ThreadState.Running)
                {
                    m_pushServiceThread.Abort();
                }
               
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuide.CWCFManagement.Dispose 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }


        /// <summary>
        /// 打开查询服务
        /// </summary>
        private void OpenQueryService()
        {
            try
            {           
                using (m_queryHost = new ServiceHost(typeof(CQueryService)))
                {
                    m_queryHost.Opened += delegate
                    {
                        CLOGException.Trace("QueryService开启中...");
                    };

                    //ServiceMetadataBehavior smb = m_queryHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                    //if (smb == null)
                    //    m_queryHost.Description.Behaviors.Add(new ServiceMetadataBehavior());

                    m_queryHost.Open();
                    while (CommunicationState.Opened == m_queryHost.State)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
            catch (System.Exception ex)
            {             
                CLOGException.Trace("KernelGuide.CWCFManagement.OpenQueryService 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 关边查询服务
        /// </summary>
        private void CloseQueryService()
        {
            try
            {            
                m_queryHost.Abort();
                m_queryHost.Close();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuide.CWCFManagement.CloseQueryService 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 打开推送服务
        /// </summary>
        private void OpenPushService()
        {
            try
            {              
                using (m_pushHost = new ServiceHost(typeof(CPushService)))
                {
                    m_pushHost.Opened += delegate 
                    {
                        CLOGException.Trace("PushService开启中...");
                    };

                    //ServiceMetadataBehavior smb = m_pushHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                    //if (smb == null)
                    //    m_pushHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
                    //m_pushHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

                    m_pushHost.Open();//开启提供服务的主机的通信
                    while (CommunicationState.Opened == m_pushHost.State)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
            catch (System.Exception ex)
            {              
                CLOGException.Trace("KernelGuide.CWCFManagement.OpenPushService 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        /// <summary>
        /// 关边推送服务
        /// </summary>
        private void ClosePushService()
        {
            try
            {        
                m_pushHost.Abort();
                m_pushHost.Close();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuide.CWCFManagement.ClosePushService 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
      

        /// <summary>
        /// 查询服务
        /// </summary>
        private void CloseAppService()
        {
            try
            {
              
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("KernelGuide.CWCFManagement.CloseAppService 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
        ///// <summary>
        ///// 打开提供服务的主机的通信
        ///// </summary>
        //void host_Closed(object sender, EventArgs e)
        //{

        //}

        ///// <summary>
        ///// 关闭提供服务的主机的通信
        ///// </summary>
        //void host_Opened(object sender, EventArgs e)
        //{

        //}
    }
}
