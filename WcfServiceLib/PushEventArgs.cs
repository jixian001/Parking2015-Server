using System;
using System.Collections.Generic;
using System.Linq;

namespace WCFServiceLib
{
    public delegate void PushEventHandler(object sender, CPushEventArgs e);

    /// <summary>
    /// 推送消息事件的参数类
    /// </summary>
    public class CPushEventArgs : EventArgs
    {
        private object m_objTable;

        public object ObjTable
        {
            get { return m_objTable; }
            set { m_objTable = value; }
        }
    }
}
