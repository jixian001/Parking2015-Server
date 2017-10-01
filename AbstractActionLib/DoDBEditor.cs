using System;
using System.Collections.Generic;
using System.Linq;
using TypedefLib;
using LOGManagementLib;
using CommonMethodLib;
using BaseMethodLib;

namespace AbstractActionLib
{
    public class CDBEditor
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CDBEditor()
        { }

        public void DBEditor(CRequestAction RequestParam)
        {
            try
            {
               
                if (RequestParam.lstTableDto == null)
                {
                    return;
                }
                // 判断是否插入或者更新修改，并返回操作枚举类型值
                foreach (struTableDto obj in RequestParam.lstTableDto)
                {
                    int accessType = obj.accessType;

                    if (accessType == 1)
                    {// 插入数据表操作
                        insertTableDto(obj.tableDto);
                    }
                    else if (accessType == 2)
                    {
                        // 更新修改数据表操作
                        updateTableDto(obj.tableDto);
                    }
                    else if (accessType == 3)
                    {   
                        // 删除数据表操作
                        deleteTableDto(obj.tableDto);
                    }
                }
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CDBEditor.DBEditor 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
        /// <summary>
        /// 插入数据表操作
        /// </summary>
        public void insertTableDto(object obj)
        {
            CCommonMethods.myMethods.InsertOnTable(obj);
            CLOGException.WriteSQLLog(CCommonMethods.myMethods.SQLDtoToString(obj), "插入" + obj.ToString() + "数据");
        }

        /// <summary>
        /// 更新修改数据表操作
        /// </summary>
        public void updateTableDto(object obj)
        {
            CCommonMethods.myMethods.UpdateTable(obj);
            CLOGException.WriteSQLLog(CCommonMethods.myMethods.SQLDtoToString(obj), "更新修改" + obj.ToString() + "数据");
        }

        /// <summary>
        /// 删除数据表操作
        /// </summary>
        public void deleteTableDto(object obj)
        {
            CCommonMethods.myMethods.DeleteOnTable(obj);
            CLOGException.WriteSQLLog(CCommonMethods.myMethods.SQLDtoToString(obj), "删除" + obj.ToString() + "数据");
        }
    }
}
