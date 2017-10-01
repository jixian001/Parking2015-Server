using System;
using DatabaseManagementLib;
using TypedefLib;

namespace BillingManagementLib
{
    public abstract class CBillingStandard
    {
        public abstract void CalcParkingFees(ref struBillInfo struBillInfo, CTariffDto tariffTBL);
        public virtual DateTime CalcDeadlineOfFix(DateTime datdtPaymentTime)
        {
            return datdtPaymentTime;
        }
    }
}
