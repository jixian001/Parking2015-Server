using System;
using DatabaseManagementLib;
using TypedefLib;

namespace BillingManagementLib
{
    public class CBillingManagement
    {
        private CBillingStandard m_objStandard;

        public void SetBillingStandard(CBillingStandard cstandard)
        {
            this.m_objStandard = cstandard;
        }

        public void CalcParkingFees(ref struBillInfo struBillInfo, CTariffDto tariffTBL)
        {
            m_objStandard.CalcParkingFees(ref struBillInfo, tariffTBL);
        }

        public DateTime CalcDeadlineOfFix(DateTime dtPaymentTime)
        {
            return m_objStandard.CalcDeadlineOfFix(dtPaymentTime);

        }
    }
}
