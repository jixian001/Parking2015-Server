using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseManagementLib;

namespace CommonMethodLib
{
    public class TvServiceScope
    {
        private readonly int Etv1LefPhysCol = 2;
        private readonly int Etv1RightPhysCol = 40;
        private readonly int Etv2LefPhysCol = 1;
        private readonly int Etv2RightPhysCol = 38;

        private IList<CDeviceStatusDto> mEtvList;

        public TvServiceScope(IList<CDeviceStatusDto> etvs) 
        {
            mEtvList = etvs;
        }

        /// <summary>
        /// 获取ETV最大作业范围
        /// </summary>
        /// <param name="smg"></param>
        /// <returns></returns>
        public CScope GetTvScope(CDeviceStatusDto smg) 
        {
            foreach (KeyValuePair<CDeviceStatusDto, CScope> pair in DicMaxWork) 
            {
                if (pair.Key.devicecode == smg.devicecode) 
                {
                    return pair.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取最大的物理工作范围
        /// </summary>
        public Dictionary<CDeviceStatusDto, CScope> DicMaxWork 
        {
            get 
            {
                Dictionary<CDeviceStatusDto, CScope> maxwork = new Dictionary<CDeviceStatusDto, CScope>();
                foreach (CDeviceStatusDto smg in mEtvList) 
                {
                    Dictionary<CDeviceStatusDto,CScope> physcScope=this.GetPhyscScope();
                    TvMaxScope maxscope = new TvMaxScope(smg, mEtvList, physcScope[smg]);
                    CScope cs = new CScope();
                    cs.LeftCol = maxscope.GetMaxLeftScope();
                    cs.RightCol = maxscope.GetMaxRightScope();

                    maxwork.Add(smg, cs);
                }
                return maxwork;
            }
        }

        /// <summary>
        /// 原始物理工作范围
        /// </summary>
        /// <returns></returns>
        private Dictionary<CDeviceStatusDto, CScope> GetPhyscScope()
        {
            Dictionary<CDeviceStatusDto, CScope> dicPhyscScope = new Dictionary<CDeviceStatusDto, CScope>();
            foreach (CDeviceStatusDto smg in mEtvList)
            {
                CScope physScope;
                if (smg.region == 1)  //TV4、5、6
                {
                    physScope = new CScope(Etv2LefPhysCol, Etv2RightPhysCol);
                }
                else  //TV1、2、3
                {
                    physScope = new CScope(Etv1LefPhysCol, Etv1RightPhysCol);
                }
                dicPhyscScope.Add(smg, physScope);
            }
            return dicPhyscScope;
        }
    }
}
