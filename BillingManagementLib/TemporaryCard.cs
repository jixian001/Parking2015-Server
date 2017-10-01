using System;
using DatabaseManagementLib;
using TypedefLib;
using BaseMethodLib;

namespace BillingManagementLib
{
    /// <summary>
    /// 临时卡
    /// </summary>
    public class CTemporaryCard : CBillingStandard
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strRebate"></param>
        public CTemporaryCard()
        {
        }

        public override void CalcParkingFees(ref struBillInfo struBillInfo, CTariffDto tariffTBL)
        {
            CalcTempFeeEnterTimeByUnit(ref struBillInfo, tariffTBL);
        }
              
        /// <summary>
        /// 单位向上取整方式-计算临时卡计时卡费用（0点为跨天节点）
        /// </summary>
        /// <param name="struBillInfo"></param>
        /// <param name="tariffTBL"></param>
        private void CalcTempFeeByUnit(ref struBillInfo struBillInfo, CTariffDto tariffTBL)
        {
            if (null == tariffTBL || null == struBillInfo.dtStartTime || null == struBillInfo.dtEndTime)
            {
                return;
            }

            float fPayableFee = 0;
            // 是否区分工作日
            bool bWorkday = true;
            // 工作日高峰起始时间、工作日高峰截止时间、非工作日高峰起始时间、非工作日高峰截止时间
            TimeSpan tsWorkPeakStart, tsWorkPeakEnd, tsNonworkPeakStart, tsNonworkPeakEnd;
            // 工作日限额、非工作日限额、一天费用
            float fWorkDayFee, fNonworkDayFee, fFee;

            // 工作日高峰首单位内单价、工作日高峰首单位外单价、工作日非高峰首单位内单价、工作日非高峰首单位外单价
            float fWorkPeakInFee, fWorkPeakOutFee, fWorkNonpeakInFee, fWorkNonpeakOutFee;
            // 工作日高峰首单位内单位、工作日高峰首单位外单位、工作日非高峰首单位内单位、工作日非高峰首单位外单位（以分钟为单元）
            int nWorkPeakInUnit, nWorkPeakOutUnit, nWorkNonpeakInUnit, nWorkNonpeakOutUnit;

            // 非工作日高峰首单位内单价、非工作日高峰首单位外单价、非工作日非高峰首单位内单价、非工作日非高峰首单位外单价
            float fNonworkPeakInFee, fNonworkPeakOutFee, fNonworkNonpeakInFee, fNonworkNonpeakOutFee;
            // 非工作日高峰首单位内单位、非工作日高峰首单位外单位、非工作日非高峰首单位内单位、非工作日非高峰首单位外单位（以分钟为单元）
            int nNonworkPeakInUnit, nNonworkPeakOutUnit, nNonworkNonpeakInUnit, nNonworkNonpeakOutUnit;
            TimeSpan ts24End;// 24:00:00时间
            DateTime dsCurrent = struBillInfo.dtStartTime;
            DateTime dsEnd = struBillInfo.dtEndTime;

            #region 解析CTariffDto属性值
            if (0 == tariffTBL.isworkday)
            {
                bWorkday = false;
            }
            CBaseMethods.MyBase.ConvertTimePeriod(tariffTBL.workpeakperiod, out tsWorkPeakStart, out tsWorkPeakEnd);
            CBaseMethods.MyBase.ConvertTimePeriod(tariffTBL.nonworkpeakperiod, out tsNonworkPeakStart, out tsNonworkPeakEnd);
            fWorkDayFee = null == tariffTBL.workdayquotafee ? 0.0f : (float)tariffTBL.workdayquotafee;
            fNonworkDayFee = null == tariffTBL.nonworkdayquotafee ? 0.0f : (float)tariffTBL.nonworkdayquotafee;
            fFee = null == tariffTBL.fee ? 0.0f : (float)tariffTBL.fee;
            fWorkPeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.workpeakinunitfee, out nWorkPeakInUnit);
            fWorkPeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.workpeakoutunitfee, out nWorkPeakOutUnit);
            fWorkNonpeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.worknonpeakinunitfee, out nWorkNonpeakInUnit);
            fWorkNonpeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.worknonpeakoutunitfee, out nWorkNonpeakOutUnit);
            fNonworkPeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworkpeakinunitfee, out nNonworkPeakInUnit);
            fNonworkPeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworkpeakoutunitfee, out nNonworkPeakOutUnit);
            fNonworkNonpeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworknonpeakinunitfee, out nNonworkNonpeakInUnit);
            fNonworkNonpeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworknonpeakoutunitfee, out nNonworkNonpeakOutUnit);
            TimeSpan.TryParse("24:00:00", out ts24End);
            #endregion

            #region 首单位内计算
            if (bWorkday && IsNonworkdays(dsCurrent))
            {// 区分工作日，判断当前日期是否为非工作日
                string strNonworkFirstUnit = tariffTBL.nonworkpeakfirstunit;
                if (0 > dsCurrent.TimeOfDay.CompareTo(tsNonworkPeakStart) || 0 < dsCurrent.TimeOfDay.CompareTo(tsNonworkPeakEnd))
                {// 起始时间在非工作日非高峰时段内
                    strNonworkFirstUnit = tariffTBL.nonworknonpeakfirstunit;
                }

                if (!CBaseMethods.MyBase.IsEmpty(strNonworkFirstUnit))
                {// 区分首单位（以分钟为单元）
                    int nNonworkFirstUnit;
                    CBaseMethods.MyBase.ConvertUnitToInt(strNonworkFirstUnit, out nNonworkFirstUnit);
                    dsCurrent = dsCurrent.AddMinutes(nNonworkFirstUnit);

                    if (struBillInfo.dtStartTime.Date == dsCurrent.Date)
                    {
                        fPayableFee += CalculateOneDayFee(struBillInfo.dtStartTime.TimeOfDay, dsCurrent.TimeOfDay, tsNonworkPeakStart, tsNonworkPeakEnd, nNonworkPeakInUnit, nNonworkNonpeakInUnit, fNonworkPeakInFee, fNonworkNonpeakInFee);
                    }
                    else
                    {// 跨天情况
                        fPayableFee += CalculateOneDayFee(struBillInfo.dtStartTime.TimeOfDay, ts24End, tsNonworkPeakStart, tsNonworkPeakEnd, nNonworkPeakInUnit, nNonworkNonpeakInUnit, fNonworkPeakInFee, fNonworkNonpeakInFee);
                        fPayableFee += CalculateOneDayFee(new TimeSpan(0, 0, 0), dsCurrent.TimeOfDay, tsNonworkPeakStart, tsNonworkPeakEnd, nNonworkPeakInUnit, nNonworkNonpeakInUnit, fNonworkPeakInFee, fNonworkNonpeakInFee);
                    }
                }
            }
            else
            {// 工作日
                string strWorkFirstUnit = tariffTBL.workpeakfirstunit;
                if (0 > dsCurrent.TimeOfDay.CompareTo(tsWorkPeakStart) || 0 < dsCurrent.TimeOfDay.CompareTo(tsWorkPeakEnd))
                {// 起始时间在工作日非高峰时段内
                    strWorkFirstUnit = tariffTBL.worknonpeakfirstunit;
                }

                if (!CBaseMethods.MyBase.IsEmpty(strWorkFirstUnit))
                {// 区分首单位
                    int nWorkFirstUnit;
                    CBaseMethods.MyBase.ConvertUnitToInt(strWorkFirstUnit, out nWorkFirstUnit);
                    dsCurrent = dsCurrent.AddMinutes(nWorkFirstUnit);

                    if (struBillInfo.dtStartTime.Date == dsCurrent.Date)
                    {
                        fPayableFee += CalculateOneDayFee(struBillInfo.dtStartTime.TimeOfDay, dsCurrent.TimeOfDay, tsWorkPeakStart, tsWorkPeakEnd, nWorkPeakInUnit, nWorkNonpeakInUnit, fWorkPeakInFee, fWorkNonpeakInFee);
                    }
                    else
                    {// 跨天情况
                        fPayableFee += CalculateOneDayFee(struBillInfo.dtStartTime.TimeOfDay, ts24End, tsWorkPeakStart, tsWorkPeakEnd, nWorkPeakInUnit, nWorkNonpeakInUnit, fWorkPeakInFee, fWorkNonpeakInFee);
                        fPayableFee += CalculateOneDayFee(new TimeSpan(0, 0, 0), dsCurrent.TimeOfDay, tsWorkPeakStart, tsWorkPeakEnd, nWorkPeakInUnit, nWorkNonpeakInUnit, fWorkPeakInFee, fWorkNonpeakInFee);
                    }
                }
            }
            #endregion

            #region 首单位外计算
            while (dsCurrent < dsEnd)
            {
                TimeSpan tsPeakStart, tsPeakEnd;
                int nPeakUnit, nNonpeakUnit;
                float fPeakFee, fNonpeakFee, fDayFee, fCalulateFee;

                if (bWorkday && IsNonworkdays(dsCurrent))
                {// 区分工作日，判断当前日期是否为非工作日
                    tsPeakStart = tsNonworkPeakStart;
                    tsPeakEnd = tsNonworkPeakEnd;
                    nPeakUnit = nNonworkPeakOutUnit;
                    nNonpeakUnit = nNonworkNonpeakOutUnit;
                    fPeakFee = fNonworkPeakOutFee;
                    fNonpeakFee = fNonworkNonpeakOutFee;
                    float dPeakMinutes = (float)(tsPeakEnd.Subtract(tsPeakStart)).TotalMinutes;
                    fDayFee = dPeakMinutes / nPeakUnit * fPeakFee + (1440 - dPeakMinutes) / nNonpeakUnit * fNonpeakFee;
                    // 无限额时，按照一天费用计算
                    fDayFee = fNonworkDayFee < 0.1 ? fDayFee : fNonworkDayFee;
                }
                else
                {// 工作日
                    tsPeakStart = tsWorkPeakStart;
                    tsPeakEnd = tsWorkPeakEnd;
                    nPeakUnit = nWorkPeakOutUnit;
                    nNonpeakUnit = nWorkNonpeakOutUnit;
                    fPeakFee = fWorkPeakOutFee;
                    fNonpeakFee = fWorkNonpeakOutFee;
                    // 无限额时，按照一天费用计算
                    fDayFee = fWorkDayFee < 0.1 ? fFee : fWorkDayFee;
                }

                if (dsCurrent.Date == dsEnd.Date && 0 > dsCurrent.TimeOfDay.CompareTo(dsEnd.TimeOfDay))
                {// 一天内情况
                    fCalulateFee = CalculateOneDayFee(dsCurrent.TimeOfDay, dsEnd.TimeOfDay, tsPeakStart, tsPeakEnd, nPeakUnit, nNonpeakUnit, fPeakFee, fNonpeakFee);
                    fCalulateFee = fCalulateFee < fDayFee ? fCalulateFee : fDayFee;
                    fPayableFee += fCalulateFee;
                    break;
                }
                else if (dsCurrent.Date < dsEnd.Date)
                {// 跨天情况
                    if (0 == dsCurrent.TimeOfDay.TotalSeconds)
                    {// 一整天时间
                        fPayableFee += fDayFee;
                    }
                    else
                    {// 计算当天
                        fCalulateFee = CalculateOneDayFee(dsCurrent.TimeOfDay, ts24End, tsPeakStart, tsPeakEnd, nPeakUnit, nNonpeakUnit, fPeakFee, fNonpeakFee);
                        fCalulateFee = fCalulateFee < fDayFee ? fCalulateFee : fDayFee;
                        fPayableFee += fCalulateFee;
                    }
                    // 累积天数向截止日期靠拢
                    dsCurrent = dsCurrent.AddDays(1).Date;
                }
                else
                {
                    break;
                }
            }
            #endregion

            struBillInfo.fPayableFee = fPayableFee;
            TimeSpan ts = struBillInfo.dtEndTime - struBillInfo.dtStartTime;
            int hours = ts.Hours + (ts.Minutes > 0 ? 1 : 0);// 停车总时间
            struBillInfo.strCalculateDays = ts.Days + "天" + hours + "小时";
        }

        /// <summary>
        /// 单位向上取整方式-计算临时卡计时卡费用（存车时间点为跨天节点）
        /// </summary>
        /// <param name="struBillInfo"></param>
        /// <param name="tariffTBL"></param>
        private void CalcTempFeeEnterTimeByUnit(ref struBillInfo struBillInfo, CTariffDto tariffTBL)
        {
            if (null == tariffTBL || null == struBillInfo.dtStartTime || null == struBillInfo.dtEndTime)
            {
                return;
            }

            float fPayableFee = 0;
            // 是否区分工作日
            bool bWorkday = true;
            // 工作日高峰起始时间、工作日高峰截止时间、非工作日高峰起始时间、非工作日高峰截止时间
            TimeSpan tsWorkPeakStart, tsWorkPeakEnd, tsNonworkPeakStart, tsNonworkPeakEnd;
            // 工作日限额、非工作日限额、一天费用
            float fWorkDayFee, fNonworkDayFee, fFee;

            // 工作日高峰首单位内单价、工作日高峰首单位外单价、工作日非高峰首单位内单价、工作日非高峰首单位外单价
            float fWorkPeakInFee, fWorkPeakOutFee, fWorkNonpeakInFee, fWorkNonpeakOutFee;
            // 工作日高峰首单位内单位、工作日高峰首单位外单位、工作日非高峰首单位内单位、工作日非高峰首单位外单位（以分钟为单元）
            int nWorkPeakInUnit, nWorkPeakOutUnit, nWorkNonpeakInUnit, nWorkNonpeakOutUnit;

            // 非工作日高峰首单位内单价、非工作日高峰首单位外单价、非工作日非高峰首单位内单价、非工作日非高峰首单位外单价
            float fNonworkPeakInFee, fNonworkPeakOutFee, fNonworkNonpeakInFee, fNonworkNonpeakOutFee;
            // 非工作日高峰首单位内单位、非工作日高峰首单位外单位、非工作日非高峰首单位内单位、非工作日非高峰首单位外单位（以分钟为单元）
            int nNonworkPeakInUnit, nNonworkPeakOutUnit, nNonworkNonpeakInUnit, nNonworkNonpeakOutUnit;
            // 首单位内价格、首单位非工作日否
            float fFirstUnitFee = 0f;
            bool bFirstUnitNonworkday = false;
            DateTime dsCurrent = struBillInfo.dtStartTime;
            DateTime dsEnd = struBillInfo.dtEndTime;

            #region 解析CTariffDto属性值
            if (0 == tariffTBL.isworkday)
            {
                bWorkday = false;
            }
            CBaseMethods.MyBase.ConvertTimePeriod(tariffTBL.workpeakperiod, out tsWorkPeakStart, out tsWorkPeakEnd);
            CBaseMethods.MyBase.ConvertTimePeriod(tariffTBL.nonworkpeakperiod, out tsNonworkPeakStart, out tsNonworkPeakEnd);
            fWorkDayFee = null == tariffTBL.workdayquotafee ? 0.0f : (float)tariffTBL.workdayquotafee;
            fNonworkDayFee = null == tariffTBL.nonworkdayquotafee ? 0.0f : (float)tariffTBL.nonworkdayquotafee;
            fFee = null == tariffTBL.fee ? 0.0f : (float)tariffTBL.fee;
            fWorkPeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.workpeakinunitfee, out nWorkPeakInUnit);
            fWorkPeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.workpeakoutunitfee, out nWorkPeakOutUnit);
            fWorkNonpeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.worknonpeakinunitfee, out nWorkNonpeakInUnit);
            fWorkNonpeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.worknonpeakoutunitfee, out nWorkNonpeakOutUnit);
            fNonworkPeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworkpeakinunitfee, out nNonworkPeakInUnit);
            fNonworkPeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworkpeakoutunitfee, out nNonworkPeakOutUnit);
            fNonworkNonpeakInFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworknonpeakinunitfee, out nNonworkNonpeakInUnit);
            fNonworkNonpeakOutFee = CBaseMethods.MyBase.ConvertUnitToInt(tariffTBL.nonworknonpeakoutunitfee, out nNonworkNonpeakOutUnit);
            #endregion

            #region 首单位内计算
            if (bWorkday && IsNonworkdays(dsCurrent))
            {// 区分工作日，判断当前日期是否为非工作日
                string strNonworkFirstUnit = tariffTBL.nonworkpeakfirstunit;
                if (0 > dsCurrent.TimeOfDay.CompareTo(tsNonworkPeakStart) || 0 < dsCurrent.TimeOfDay.CompareTo(tsNonworkPeakEnd))
                {// 起始时间在非工作日非高峰时段内
                    strNonworkFirstUnit = tariffTBL.nonworknonpeakfirstunit;
                }

                if (!CBaseMethods.MyBase.IsEmpty(strNonworkFirstUnit))
                {// 区分首单位（以分钟为单元）
                    int nNonworkFirstUnit;
                    CBaseMethods.MyBase.ConvertUnitToInt(strNonworkFirstUnit, out nNonworkFirstUnit);
                    dsCurrent = dsCurrent.AddMinutes(nNonworkFirstUnit);

                    fPayableFee += CalculateOneDayFeeEnterTime(struBillInfo.dtStartTime, dsCurrent, tsNonworkPeakStart, tsNonworkPeakEnd, nNonworkPeakInUnit, nNonworkNonpeakInUnit, fNonworkPeakInFee, fNonworkNonpeakInFee);
                }
                bFirstUnitNonworkday = true;
            }
            else
            {// 工作日
                string strWorkFirstUnit = tariffTBL.workpeakfirstunit;
                if (0 > dsCurrent.TimeOfDay.CompareTo(tsWorkPeakStart) || 0 < dsCurrent.TimeOfDay.CompareTo(tsWorkPeakEnd))
                {// 起始时间在工作日非高峰时段内
                    strWorkFirstUnit = tariffTBL.worknonpeakfirstunit;
                }

                if (!CBaseMethods.MyBase.IsEmpty(strWorkFirstUnit))
                {// 区分首单位
                    int nWorkFirstUnit;
                    CBaseMethods.MyBase.ConvertUnitToInt(strWorkFirstUnit, out nWorkFirstUnit);
                    dsCurrent = dsCurrent.AddMinutes(nWorkFirstUnit);
                    fPayableFee += CalculateOneDayFeeEnterTime(struBillInfo.dtStartTime, dsCurrent, tsWorkPeakStart, tsWorkPeakEnd, nWorkPeakInUnit, nWorkNonpeakInUnit, fWorkPeakInFee, fWorkNonpeakInFee);
                }
            }
            fFirstUnitFee = fPayableFee;
            #endregion

            #region 首单位外计算
            while (dsCurrent < dsEnd)
            {
                TimeSpan tsPeakStart, tsPeakEnd;
                int nPeakUnit, nNonpeakUnit;
                float fPeakFee, fNonpeakFee, fDayFee, fCalulateFee;
                bool bNonworkday = false;

                if (bWorkday && IsNonworkdays(dsCurrent))
                {// 区分工作日，判断当前日期是否为非工作日
                    tsPeakStart = tsNonworkPeakStart;
                    tsPeakEnd = tsNonworkPeakEnd;
                    nPeakUnit = nNonworkPeakOutUnit;
                    nNonpeakUnit = nNonworkNonpeakOutUnit;
                    fPeakFee = fNonworkPeakOutFee;
                    fNonpeakFee = fNonworkNonpeakOutFee;
                    float dPeakMinutes = (float)(tsPeakEnd.Subtract(tsPeakStart)).TotalMinutes;
                    fDayFee = dPeakMinutes / nPeakUnit * fPeakFee + (1440 - dPeakMinutes) / nNonpeakUnit * fNonpeakFee;
                    // 无限额时，按照一天费用计算
                    fDayFee = fNonworkDayFee < 0.1 ? fDayFee : fNonworkDayFee;
                    bNonworkday = true;
                }
                else
                {// 工作日
                    tsPeakStart = tsWorkPeakStart;
                    tsPeakEnd = tsWorkPeakEnd;
                    nPeakUnit = nWorkPeakOutUnit;
                    nNonpeakUnit = nWorkNonpeakOutUnit;
                    fPeakFee = fWorkPeakOutFee;
                    fNonpeakFee = fWorkNonpeakOutFee;
                    // 无限额时，按照一天费用计算
                    fDayFee = fWorkDayFee < 0.1 ? fFee : fWorkDayFee;
                }

                if (0 >= dsEnd.CompareTo(dsCurrent))
                {
                    break;
                }
                else if (0 >= dsEnd.CompareTo(dsCurrent.AddDays(1)))
                {
                    // 一天内情况
                    fCalulateFee = CalculateOneDayFeeEnterTime(dsCurrent, dsEnd, tsPeakStart, tsPeakEnd, nPeakUnit, nNonpeakUnit, fPeakFee, fNonpeakFee);
                    if (bFirstUnitNonworkday == bNonworkday && 13 < struBillInfo.dtEndTime.Subtract(struBillInfo.dtStartTime).Hours)
                    {
                        fCalulateFee = (fCalulateFee + fFirstUnitFee) < fDayFee ? fCalulateFee : fDayFee - fFirstUnitFee;
                    }
                    else
                    {
                        fCalulateFee = fCalulateFee < fDayFee ? fCalulateFee : fDayFee;
                    }
                    fPayableFee += fCalulateFee;
                    break;
                }
                else
                {
                    // 跨天情况
                    fPayableFee += fDayFee;
                    // 累积天数向截止日期靠拢
                    dsCurrent = dsCurrent.AddDays(1);
                }
            }
            #endregion

            struBillInfo.fPayableFee = fPayableFee;
            TimeSpan ts = struBillInfo.dtEndTime - struBillInfo.dtStartTime;
            int hours = ts.Hours + (ts.Minutes > 0 ? 1 : 0);// 停车总时间
            struBillInfo.strCalculateDays = ts.Days + "天" + hours + "小时";
        }

        #region 私有函数
        /// <summary>
        /// 判断当前日期是否是非工作日(true->非工作日  false->工作日)
        /// </summary>
        /// <param name="date"></param>
        /// <returns>true->非工作日  false->工作日</returns>
        private bool IsNonworkdays(DateTime date)
        {
            if (DayOfWeek.Saturday == date.DayOfWeek || DayOfWeek.Sunday == date.DayOfWeek)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 计算一天中某段时间的费用
        /// </summary>
        /// <param name="tsStart"></param>
        /// <param name="tsEnd"></param>
        /// <param name="tsPeakStart"></param>
        /// <param name="tsPeakEnd"></param>
        /// <param name="nPeakUnit"></param>
        /// <param name="nNonpeakUnit"></param>
        /// <param name="fPeakFee"></param>
        /// <param name="fNonpeakFee"></param>
        /// <returns></returns>
        private float CalculateOneDayFee(TimeSpan tsStart, TimeSpan tsEnd, TimeSpan tsPeakStart, TimeSpan tsPeakEnd, 
                                       int nPeakUnit, int nNonpeakUnit, float fPeakFee, float fNonpeakFee)
        {
            float nFee = 0;
            if (0 <= tsStart.CompareTo(tsEnd))
            {
                return nFee;
            }

            // end > start
            int nMinutesPeak = 0;   // 高峰时段分钟数
            int nMinutesNonpeak = 0;   // 非高峰时段分钟数
            int nMinutes = tsEnd.Subtract(tsStart).Hours * 60 + tsEnd.Subtract(tsStart).Minutes;// 总分钟

            // 值说明-1此实例短于 value。0此实例等于 value。1此实例长于 value。- 或 -value 为 null。
            if (0 < tsStart.CompareTo(tsPeakStart) && 0 > tsStart.CompareTo(tsPeakEnd))
            {// 起始在高峰时段
                if (0 < tsEnd.CompareTo(tsPeakStart) && 0 > tsEnd.CompareTo(tsPeakEnd))
                {// 截止在高峰时段 
                    nMinutesPeak = nMinutes;
                }
                else
                {// 截止在非高峰时段
                    nMinutesPeak = tsPeakEnd.Subtract(tsStart).Hours * 60 + tsPeakEnd.Subtract(tsStart).Minutes;// 总分钟
                }
            }
            else
            {// 起始在非高峰时段
                if (0 < tsEnd.CompareTo(tsPeakStart) && 0 > tsEnd.CompareTo(tsPeakEnd))
                {// 截止在高峰时段
                    nMinutesPeak = tsEnd.Subtract(tsPeakStart).Hours * 60 + tsEnd.Subtract(tsPeakStart).Minutes;// 总分钟
                }
                else if (0 > tsStart.CompareTo(tsPeakStart) && 0 < tsEnd.CompareTo(tsPeakEnd))
                {//  起始在非高峰时段的前段, 截止在非高峰时段后段(也即中间隔着高峰时段)
                    nMinutesPeak = tsPeakEnd.Subtract(tsPeakStart).Hours * 60 + tsPeakEnd.Subtract(tsPeakStart).Minutes;// 总分钟
                }
                else
                {// 截止在非高峰时段
                    nMinutesPeak = 0;
                }
            }

            if (0 > nMinutesPeak)
            {
                nMinutesPeak += 1440;// 24 * 60;
            }

            nMinutesPeak = Math.Min(nMinutesPeak, nMinutes);
            nMinutesNonpeak = nMinutes - nMinutesPeak;
            // 单位向上取整
            int nUnitPeak = (nMinutesPeak / nPeakUnit) + (nMinutesPeak % nPeakUnit > 0 ? 1 : 0);
            int nUnitNoneak = (nMinutesNonpeak / nNonpeakUnit) + (nMinutesNonpeak % nNonpeakUnit > 0 ? 1 : 0);
            nFee = nUnitPeak * fPeakFee + nUnitNoneak * fNonpeakFee;
            return nFee;
        }

        /// <summary>
        /// 计算一天中某段时间的费用
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="tsPeakStart"></param>
        /// <param name="tsPeakEnd"></param>
        /// <param name="nPeakUnit"></param>
        /// <param name="nNonpeakUnit"></param>
        /// <param name="fPeakFee"></param>
        /// <param name="fNonpeakFee"></param>
        /// <returns></returns>
        private float CalculateOneDayFeeEnterTime(DateTime dtStart, DateTime dtEnd, TimeSpan tsPeakStart, TimeSpan tsPeakEnd,
                                       int nPeakUnit, int nNonpeakUnit, float fPeakFee, float fNonpeakFee)
        {
            float nFee = 0;
            if (0 <= dtStart.CompareTo(dtEnd))
            {
                return nFee;
            }

            // end > start
            int nMinutesPeak = 0;   // 高峰时段分钟数
            int nMinutesNonpeak = 0;   // 非高峰时段分钟数
            int nMinutes = dtEnd.Subtract(dtStart).Hours * 60 + dtEnd.Subtract(dtStart).Minutes;// 总分钟
            DateTime dtPeakStart = dtStart.Date.Add(tsPeakStart);
            DateTime dtPeakEnd = dtStart.Date.Add(tsPeakEnd);

            // 值说明-1此实例短于 value。0此实例等于 value。1此实例长于 value。- 或 -value 为 null。
            if (0 < dtStart.CompareTo(dtPeakStart) && 0 > dtStart.CompareTo(dtPeakEnd))
            {// 起始在高峰时段
                if (0 < dtEnd.CompareTo(dtPeakStart) && 0 > dtEnd.CompareTo(dtPeakEnd))
                {// 截止在高峰时段 
                    nMinutesPeak = nMinutes;
                }
                else if (1 <= dtEnd.Subtract(dtPeakStart).Days)
                {//  起始在非高峰时段的后段, 截止在高峰时段（跨天）
                    nMinutesNonpeak = 1440 - dtPeakEnd.Subtract(dtPeakStart).Hours * 60 - dtPeakEnd.Subtract(dtPeakStart).Minutes;// 总分钟
                    nMinutesPeak = nMinutes - nMinutesNonpeak;// 总分钟
                }
                else
                {// 截止在非高峰时段
                    nMinutesPeak = dtPeakEnd.Subtract(dtStart).Hours * 60 + dtPeakEnd.Subtract(dtStart).Minutes;// 总分钟
                }
            }
            else
            {// 起始在非高峰时段
                if (0 < dtEnd.CompareTo(dtPeakStart) && 0 > dtEnd.CompareTo(dtPeakEnd))
                {// 截止在高峰时段
                    nMinutesPeak = dtEnd.Subtract(dtPeakStart).Hours * 60 + dtEnd.Subtract(dtPeakStart).Minutes;// 总分钟
                }
                else if (0 > dtStart.CompareTo(dtPeakStart) && 0 < dtEnd.CompareTo(dtPeakEnd))
                {//  起始在非高峰时段的前段, 截止在非高峰时段后段(也即中间隔着高峰时段)
                    nMinutesPeak = dtPeakEnd.Subtract(dtPeakStart).Hours * 60 + dtPeakEnd.Subtract(dtPeakStart).Minutes;// 总分钟
                }
                else if (0 < dtStart.CompareTo(dtPeakEnd) && 0 < dtEnd.CompareTo(dtPeakEnd) && 1 <= dtEnd.Subtract(dtPeakEnd).Days)
                {//  起始在非高峰时段的后段, 截止在非高峰时段后段(也即中间隔着高峰时段)
                    nMinutesPeak = dtPeakEnd.Subtract(dtPeakStart).Hours * 60 + dtPeakEnd.Subtract(dtPeakStart).Minutes;// 总分钟
                }
                else if (0 < dtStart.CompareTo(dtPeakEnd) && 0 < dtEnd.CompareTo(dtPeakEnd) && 1 <= dtEnd.Subtract(dtPeakStart).Days)
                {//  起始在非高峰时段的后段, 截止在高峰时段（跨天）
                    nMinutesPeak = dtEnd.TimeOfDay.Subtract(tsPeakStart).Hours * 60 + dtEnd.TimeOfDay.Subtract(tsPeakStart).Minutes;// 总分钟
                }
                else
                {// 截止在非高峰时段
                    nMinutesPeak = 0;
                }
            }

            if (0 > nMinutesPeak)
            {
                nMinutesPeak += 1440;// 24 * 60;
            }

            nMinutesPeak = Math.Min(nMinutesPeak, nMinutes);
            nMinutesNonpeak = nMinutes - nMinutesPeak;
            // 单位向上取整
            int nUnitPeak = (nMinutesPeak / nPeakUnit) + (nMinutesPeak % nPeakUnit > 0 ? 1 : 0);
            int nUnitNoneak = (nMinutesNonpeak / nNonpeakUnit) + (nMinutesNonpeak % nNonpeakUnit > 0 ? 1 : 0);
            nFee = nUnitPeak * fPeakFee + nUnitNoneak * fNonpeakFee;
            return nFee;
        }
        #endregion
    }
}
