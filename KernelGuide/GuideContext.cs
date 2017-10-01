using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KernelGuideLib;
using AbstractActionLib;
using LOGManagementLib;
using BaseMethodLib;
using DevExpress.XtraSplashScreen;

namespace KernelGuideLib
{
    public class CGuideContext : ApplicationContext
    {
        private CMainCallback telegramCallback;
        private COptionForm optionForm;
        //private CSplashScreenForm splashScreen;
        public CGuideContext()
        {
            try
            {
                CLOGException.Trace("——————CGuideContext");
                telegramCallback = new CMainCallback();
                optionForm = new COptionForm(telegramCallback);
                //splashScreen = new CSplashScreenForm();
                Run();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AutoParkingSystem.CGuideContext.CGuideContext  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }

        public void Run()
        {
            optionForm.Show();
            //splashScreen.SetInfo("启动WCF服务", 0, 30, 300);
            //splashScreen.SetInfo("启动自动停车服务", 31, 70, 300);
            //splashScreen.SetInfo("启动文件管理进程", 71, 100, 300);
        }
    }
}
