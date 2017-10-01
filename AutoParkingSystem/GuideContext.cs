using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KernelGuideLib;
using CommonMethodLib;
using LOGManagementLib;
using BaseMethodLib;

namespace AutoParkingSystem
{
    public class CGuideContext : ApplicationContext
    {
        private CMainCallback m_mainCallback;
        private COptionForm optionForm;
        public CGuideContext()
        {
            try
            {               
                m_mainCallback = new CMainCallback();
                optionForm = new COptionForm(m_mainCallback);
                optionForm.Run();
            }
            catch (System.Exception ex)
            {
                CLOGException.Trace("AutoParkingSystem.CGuideContext.CGuideContext  异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
