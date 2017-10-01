using System;
using DatabaseManagementLib;
using TypedefLib;
using BaseMethodLib;

namespace BillingManagementLib
{
    /// <summary>
    /// 年卡
    /// </summary>
    public class CAnnualCard : CBillingStandard
    {
        private int m_dMonths = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strRebate"></param>
        public CAnnualCard(string strMonths)
        {
            CBaseMethods.MyBase.StringToUInt32(strMonths, out this.m_dMonths);
        }

        public override void CalcParkingFees(ref struBillInfo struBillInfo, CTariffDto tariffTBL)
        {
            //struBillInfo.dtStartTime = struBillInfo.dtEndTime.AddMonths(-m_dMonths);
            struBillInfo.strCalculateDays = (struBillInfo.dtEndTime - DateTime.Now).Days.ToString();

            if (0 > struBillInfo.fPayableFee && struBillInfo.dtEndTime < DateTime.Now)
            {// 当前卡已存车且IC卡已过期，请补交费用
                int nDays = Math.Max(struBillInfo.dtEndTime.Subtract(struBillInfo.dtStartTime).Days, 1);
                float fDayFee = struBillInfo.fTariffNorm / nDays;
                int nCalculateDays = Math.Max(DateTime.Now.Subtract(struBillInfo.dtEndTime).Days, 0);
                struBillInfo.fPayableFee = nCalculateDays * fDayFee;
            }
        }

        public override DateTime CalcDeadlineOfFix(DateTime dtPaymentTime)
        {
            return dtPaymentTime.AddMonths(m_dMonths);
        }
    }
}
