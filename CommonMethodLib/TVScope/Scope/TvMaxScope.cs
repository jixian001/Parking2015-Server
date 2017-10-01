using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseManagementLib;

namespace CommonMethodLib
{
    public class TvMaxScope
    {
        private readonly int safeDistCol = 3;  //安全列

        private CDeviceStatusDto cETV = null;
        private IList<CDeviceStatusDto> etvList = null;
        private CScope mPhysicWorkScope = null;

        public TvMaxScope(CDeviceStatusDto pTV, IList<CDeviceStatusDto> pTvList,CScope physScope) 
        {
            cETV = pTV;
            etvList = pTvList;
            mPhysicWorkScope = physScope;
        }

        /// <summary>
        /// 获取最大可达的左侧范围
        /// </summary>
        /// <returns></returns>
        public int GetMaxLeftScope()
        {
            try
            {
                string eAddrs = cETV.deviceaddr;
                if (eAddrs == null || eAddrs.Length < 3)
                {
                    return 0;
                }
                int etvCol = Convert.ToInt32(eAddrs.Substring(1, 2));
                int disableCol = GetPhysicLeftCol(mPhysicWorkScope.LeftCol, etvCol);
                if (disableCol != mPhysicWorkScope.LeftCol)
                {                 
                    if ((disableCol + safeDistCol) < 40)
                    {
                        return disableCol + safeDistCol;
                    }
                    else 
                    {
                        return disableCol;
                    }
                }               
                return mPhysicWorkScope.LeftCol;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最大可达的右侧范围
        /// </summary>
        /// <returns></returns>
        public int GetMaxRightScope()
        {
            try
            {
                string eAddrs = cETV.deviceaddr;
                if (eAddrs == null)
                {
                    return 0;
                }
                int etvCol = Convert.ToInt32(eAddrs.Substring(1,2));
                int disableCol = GetPhysicRightCol(etvCol, mPhysicWorkScope.RightCol);
                if (disableCol != mPhysicWorkScope.RightCol)
                {
                    if (disableCol > safeDistCol && disableCol > 4)
                    {
                        return disableCol - safeDistCol;
                    }
                    else
                    {
                        if (disableCol == 3 || disableCol == 4) 
                        {
                            return 2;
                        }
                        return disableCol;
                    }
                }
                return mPhysicWorkScope.RightCol;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 物理左范围
        /// </summary>
        /// <param name="pLeft"></param>
        /// <param name="pRight"></param>
        /// <returns></returns>
        private int GetPhysicLeftCol(int pLeft, int pRight)
        {
            int leftCol = pLeft;
            foreach (CDeviceStatusDto smg in etvList)
            {
                if (smg.devicecode != cETV.devicecode)
                {
                    string eAddrs = smg.deviceaddr;
                    if (eAddrs == null || eAddrs.Length < 3)
                    {
                        return 0;
                    }
                    bool isAuto = smg.isable == 1 ? true : false;
                    int etvCol = Convert.ToInt32(eAddrs.Substring(1, 2));
                    if (!isAuto)
                    {
                        if (etvCol > pLeft && etvCol < pRight)
                        {
                            leftCol = etvCol;
                        }
                        //当TV4在2#厅时，TV3的范围应向前一个，使TV3不能到2#厅进行存取车
                        if (etvCol == pLeft) 
                        {
                            leftCol = pLeft + 1;
                        }
                    }
                }
            }
            return leftCol;
        }

        /// <summary>
        /// 物理右范围
        /// </summary>
        /// <param name="pLeft"></param>
        /// <param name="pRight"></param>
        /// <returns></returns>
        private int GetPhysicRightCol(int pLeft, int pRight)
        {
            int rightCol = pRight;
            foreach (CDeviceStatusDto smg in etvList)
            {
                if (smg.devicecode != cETV.devicecode)
                {
                    string eAddrs = smg.deviceaddr;
                    if (eAddrs == null || eAddrs.Length < 3)
                    {
                        return 0;
                    }
                    bool isAuto = smg.isable == 1 ? true : false;
                    int etvCol = Convert.ToInt32(eAddrs.Substring(1, 2));
                    if (!isAuto && (etvCol > pLeft && etvCol < pRight))
                    {
                        rightCol = etvCol;
                    }
                }
            }
            return rightCol;
        }

    }
}
