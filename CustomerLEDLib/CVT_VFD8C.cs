using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CustomerLEDLib
{
    public class CVT_VFD8C
    {

        private int m_nComID;//连接串口号

        public CVT_VFD8C(int nComID)
        {
            m_nComID = nComID;
        }

        public bool ConnectCOM()
        {
            return VFD8C_LED8C_OpenCom(m_nComID);
        }

        public bool disConnectCOM()
        {
            return VFD8C_LED8C_CloseCom();
        }

        public bool VFD_Clear()
        {
            return VFD8C_LED8C_Clear();
        }

        public bool VFD_Can()
        {
            return VFD8C_LED8C_CAN();
        }

        public bool VFD_Reset()
        {
            return VFD8C_LED8C_Reset();
        }

        public bool VFD_DisplayNum(string buf)
        {
            return VFD8C_LED8C_DisplayNum(buf);
        }

        public bool VFD_DisplayChar(int TransType)
        {
            return VFD8C_LED8C_DisplayChar(TransType);
        }

        public bool VFD_DisplayThanksLine(int ThanksType, int Line)
        {
            return VFD8C_LED8C_DisplayThanksLine(ThanksType, Line);
        }

        public bool VFD_DisplayCurrency(int Currency)
        {
            return VFD8C_LED8C_DisplayCurrency(Currency);
        }

        public bool VFD_DisplayPOS(int POS)
        {
            return VFD8C_LED8C_DisplayPOS(POS);
        }

        public bool VFD_SpeakOther(int Other)
        {
            return VFD8C_LED8C_SpeakOther(Other);
        }

        public bool VFD_SpeakSetup(int n, int m, int k1, int k2, int k3, int k4)
        {
            return VFD8C_LED8C_SpeakSetup(n, m, k1, k2, k3, k4);
        }

        public bool VFD_OpenBox()
        {
            return VFD8C_LED8C_OpenBox();
        }

        public bool VFD_DisplayNumChar(string buf, int TransType)
        {
            return VFD8C_LED8C_DisplayNumChar(buf, TransType);
        }

        public bool VFD_DisplayCharNum(int TransType, string buf)
        {
            return VFD8C_LED8C_DisplayCharNum(TransType, buf);
        }

        public bool VFD_Sendc(string buf)
        {
            return VFD8C_LED8C_Sendc(buf);
        }

        #region 动态库链接函数
        //BOOL WINAPI VFD8C_LED8C_OpenCom(uchar port);
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_OpenCom", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_OpenCom(int port);

        //BOOL WINAPI VFD8C_LED8C_CloseCom(void);
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_CloseCom", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_CloseCom();

        //BOOL WINAPI VFD8C_LED8C_Clear(void);
        /// <summary>
        /// 清屏命令
        /// </summary>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_Clear", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_Clear();

        //BOOL WINAPI VFD8C_LED8C_CAN(void);
        /// <summary>
        /// 清除光标命令
        /// </summary>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_CAN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_CAN();

        //BOOL WINAPI VFD8C_LED8C_Reset(void);
        /// <summary>
        /// 初始化命令
        /// </summary>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_Reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_Reset();

        //BOOL WINAPI VFD8C_LED8C_DisplayNum(uchar *sData);
        /// <summary>
        /// 发送显示数据命令
        /// </summary>
        /// <param name="buf">数据行字符串（位数不超过8位数值+7位小数点）</param>
        /// <returns></returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayNum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayNum(string buf);

        //BOOL WINAPI VFD8C_LED8C_DisplayChar(uchar iTransType);
        /// <summary>
        /// 设置 [单价][总计][收款][找零]字符显示状态
        /// </summary>
        /// <param name="TransType">0-四种字符 全暗；1-[单价]亮；2-[总计]亮；3-[收款]亮;4-[找零]亮</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayChar", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayChar(int TransType);

        //BOOL WINAPI VFD8C_LED8C_DisplayThanksLine(uchar iThanksType,uchar iLine);
        /// <summary>
        /// 设置[多谢惠顾]字符、[动态线]显示状态
        /// </summary>
        /// <param name="ThanksType">0-[多谢惠顾]暗；1-亮；2-转动；3-闪烁</param>
        /// <param name="Line">0-[动态线]暗；1-亮</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayThanksLine", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayThanksLine(int ThanksType, int Line);

        //BOOL WINAPI VFD8C_LED8C_DisplayCurrency(uchar iCurrency);
        /// <summary>
        /// 设置币种字符显示状态
        /// </summary>
        /// <param name="Currency">0-三种字符 全暗；1-[$]亮；2-[￥]亮；3-[£]亮；4-三种字符 全亮；5-三种字符 转动</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayCurrency", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayCurrency(int Currency);

        //BOOL WINAPI VFD8C_LED8C_DisplayPOS(uchar iPOS);
        /// <summary>
        /// 设置POS SYSTEM字符显示状态命令
        /// </summary>
        /// <param name="POS">0-[POS SYSTEM]字符 暗；1-亮</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayPOS", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayPOS(int POS);

        //BOOL WINAPI VFD8C_LED8C_SpeakOther(uchar iOther);
        /// <summary>
        /// 报读特殊语音内容
        /// </summary>
        /// <param name="Other">1-语音“请出示医保卡”；2-语音“对不起，您要买的车票已售完”</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_SpeakOther", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_SpeakOther(int Other);

        //BOOL WINAPI VFD8C_LED8C_SpeakSetup(uchar n,uchar m,uchar k1,uchar k2,uchar k3,uchar k4);
        /// <summary>
        /// 设置语音参数
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
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_SpeakSetup", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_SpeakSetup(int n, int m, int k1, int k2, int k3, int k4);

        //BOOL WINAPI VFD8C_LED8C_OpenBox(void);
        /// <summary>
        /// 开钱箱命令
        /// </summary>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_OpenBox", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_OpenBox();

        //BOOL WINAPI VFD8C_LED8C_DisplayNumChar(char *sData,uchar iTransType);
        /// <summary>
        /// 送显示数据，再设置 “单价”、“总计”、“收款”、“找零”字符显示状态命令
        /// </summary>
        /// <param name="buf">数据行字符串（8位数值+7位小数点）</param>
        /// <param name="TransType">0：四种字符 全暗；
        ///1：“单价”字符 亮，其它三种 全暗；
        ///2：“总计”字符 亮，其它三种 全暗；
        ///3：“收款”字符 亮，其它三种 全暗；
        ///4：“找零”字符 亮，其它三种 全暗。
        ///</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayNumChar", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayNumChar(string buf, int TransType);

        //BOOL WINAPI VFD8C_LED8C_DisplayCharNum(uchar iTransType,char *sData);
        /// <summary>
        /// 设置“单价”、“总计”、“收款”、“找零”字符显示状态，再送显示数据
        /// </summary>
        /// <param name="TransType">
        /// 1：“单价”字符 亮，其它三种 全暗；
        ///2：“总计”字符 亮，其它三种 全暗；
        ///3：“收款”字符 亮，其它三种 全暗；
        ///4：“找零”字符 亮，其它三种 全暗。
        /// </param>
        /// <param name="buf">数据行字符串（8位数值+7位小数点）</param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_DisplayCharNum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_DisplayCharNum(int TransType, string buf);

        //BOOL WINAPI VFD8C_LED8C_Sendc(char *sData);
        /// <summary>
        /// 控制状态灯相应位的亮灭
        /// </summary>
        /// <param name="buf"> 4位字符串（d1d2d3d4）
        /// 当d1=0时，单价字符 灭；d1=1时，单价字符 亮；
        ///当d2=0时，总计字符 灭；d2=1时，总计字符 亮；
        ///当d3=0时，收款字符 灭；d3=1时，收款字符 亮；
        ///当d4=0时，找零字符 灭；d4=1时，找零字符 亮。
        /// </param>
        /// <returns>整型值 1-成功；0-失败</returns>
        [DllImport("VFD8C_LED8C.dll", EntryPoint = "VFD8C_LED8C_Sendc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool VFD8C_LED8C_Sendc(string buf);

        #endregion

    }
}
