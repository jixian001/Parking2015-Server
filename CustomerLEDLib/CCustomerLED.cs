using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerLEDLib
{
    public class CCustomerLED : ICustomerLED
    {

        private int mnCommNum;
        private CVT_VFD8C vtVFD8C;

        public CCustomerLED(int com)
        {
            mnCommNum = com;
            vtVFD8C = new CVT_VFD8C(mnCommNum);

        }

        /// <summary>
        /// 连接顾客显示屏
        /// </summary>
        /// <returns></returns>
        public bool ConnectCOM()
        {
            return vtVFD8C.ConnectCOM();
        }

        /// <summary>
        /// 断开顾客显示屏
        /// </summary>
        /// <returns></returns>
        public bool disConnectCOM()
        {
            return vtVFD8C.disConnectCOM();
        }

        /// <summary>
        /// 显示数字到顾客显示屏上
        /// </summary>
        /// <param name="buf">数据行字符串（位数不超过8位数值+7位小数点）</param>
        /// <returns></returns>
        public bool DisplayNum(string buf)
        {
            return vtVFD8C.VFD_DisplayNum(buf);
        }

        /// <summary>
        /// 顾客显示屏播放语音
        /// </summary>
        /// <param name="n">当 n=0，禁止播放语音；当 n=1，使能播放语音。</param>
        /// <param name="m">(2)	当 m=0， 语音播放顺序按照“先发送指示灯，再发送数据”发送命令；
        /// 当 m=1，语音播放顺序按照“先发送数据，再发送指示灯”发送命令。</param>
        /// <param name="k1">(3)	当 k1=0，“单价”字符 亮，显示价格时没有语音报读；k1=1，
        /// “单价”字符 亮，显示价格时语音报读数值。</param>
        /// <param name="k2">(4)	当 k2=0，“总计”字符 亮，显示价格时没有语音报读；
        /// k2=1，“总计”字符亮，显示价格时语音报读“总计”+数值；
        /// k2=2，“总计”字符 亮，显示价格时语音报读“您好，总计”+数值；
        /// k2=3，“总计”字符 亮，显示价格时语音报读“多谢惠顾，您购买的商品总计金额是”+数值。</param>
        /// <param name="k3">(5)	当 k3=0，“收款”字符 亮，显示价格时没有语音报读；
        /// k3=1，“收款”字符亮，显示价格时语音报读“收您”+数值；
        /// k3=2，“收款”字符 亮，显示价格时语音报读“谢谢，收您”+数值。</param>
        /// <param name="k4">(6)	当 k4=0，“找零” 字符 亮，显示价格时没有语音报读；
        /// k4=1，“找零”字符亮，显示价格时语音报读“找您”+数值；
        /// k4=2，“找零”字符 亮，显示价格时语音报读“找您”+数值+“谢谢”；
        /// k4=3，“找零”字符 亮，显示价格时语音报读“找您”+数值+“欢迎您再次光临”；
        /// k4=4，“找零”字符 亮，显示价格时语音报读“找您”+数值+“祝您身体健康”；
        /// k4=5，“找零”字符 亮，显示价格时语音报读“找您”+数值+“祝您旅途愉快”。</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        public bool SpeakSetup(int n, int m, int k1, int k2, int k3, int k4)
        {
            return vtVFD8C.VFD_SpeakSetup(n, m, k1, k2, k3, k4);
        }
    }

}
