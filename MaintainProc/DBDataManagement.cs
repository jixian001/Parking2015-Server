using System;
using System.Collections.Generic;
using System.Linq;
using ConfigManagmtLib;
using CommonMethodLib;
using LOGManagementLib;
using BaseMethodLib;

namespace MaintainProc
{
    public class CFileManagement
    {
        private int m_nLogFileSaveDays = 0;

        public CFileManagement()
        {
            m_nLogFileSaveDays = CConfigManagement.myPara.LogFileSaveDays + 1;
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        public void DeleteLog()
        {
            try
            {
                DateTime dtDeadLine = DateTime.Now.AddDays(-m_nLogFileSaveDays);
                CCommonMethods.myMethods.CleanLogTable(dtDeadLine);
            }
            catch (Exception ex)
            {
                CLOGException.Trace("MaintainProc.CFileManagement.Timer_Tick 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
