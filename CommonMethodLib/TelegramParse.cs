using System;
using System.Linq;
using BaseMethodLib;

namespace CommonMethodLib
{
    public struct struTelegram
    {
        /// <summary>
        /// 库区号,EMS用
        /// </summary>
        public int nEMSSendFlag;//EMS发送报文标识，当该标识为1说明当前报文是由EMS发送的
        /// <summary>
        /// 库区号,PLC标记
        /// </summary>
        public int nPLCSendFlag;//PLC发送报文标识，当该标识为1说明当前报文是由PLC发送的
        public int nTelegramType;//报文类型
        public int nSubTypeEMS;//报文子类型（EMS），EMS发送报文的报文子类型，PLC发送报文时不使用该字节
        public int nSubTypePLC;//报文子类型（PLC），PLC发送报文的报文子类型，EMS发送报文时不使用该字节
        public int nEquipID;//设备ID
        public string strICCardID;//IC卡卡号（卡上面可见的逻辑卡号）
        public string strCarSize;//车辆外形尺寸（111,112等）
        public int nWheelbase;//车辆轴距（单位：mm）
        public string strSrcLocAddr;//车辆移动的源位置（车厅地址或车位地址）
        public string strDestLocAddr;//车辆移动的目的位置（车厅地址或车位地址）
        public int nTelegramID;//报文ID：EMS->PLC（1~4999）；PLC->EMS(5001~9999)
        public int nEndFlag;//报文结束标识，值固定为：9999

        public int nDeviceByteBit;//设备故障文件字节位(ETV可接受指令或者车厅可接受刷取车卡)
        public int nSMGValidity;//状态命令SMG有效性标志
        public int nDirection;//车辆车头朝向
        public int nDestDirection;//目标朝向

        public int offCenter; //中心偏移距离
        public int rearWheelDis; //后轮距巷道

        public int overallLg; //全车长度
        public int overHang;  //前悬长度
        public int carWeight; //车辆重量

        public int nMasterType; //作业类型
    }

    public class CTelegramParse
    {
        /// <summary>
        /// 50
        /// </summary>
        private int m_nTelegramLen = 50;//读取配置文件
        /// <summary>
        /// 1074
        /// </summary>
        private int m_nStatusCommandType = 1074;//读取配置文件
        public Int16[] TelegramSend;//打包后发送的报文
        public struTelegram StruRecvTelegram;//解析后收到的报文
        public struTelegram StruSendTelegram;//解析后收到的报文

        public CTelegramParse()
        {
            TelegramSend = new Int16[m_nTelegramLen];
            StruRecvTelegram = new struTelegram();
        }
        /// <summary>
        /// 解析报文
        /// </summary>
        /// <param name="struReceiveTelegram"></param>
        /// <param name="objTelegram"></param>
        public void UnpackTelegram(out struTelegram struReceiveTelegram, Int16[] objTelegram)
        {
            if (objTelegram.Count() < m_nTelegramLen)
            {
                struReceiveTelegram = new struTelegram();
                return;
            }
            
            if (objTelegram[2] != m_nStatusCommandType)
            {
                // 动作命令
                UnpackTelegramMove(out struReceiveTelegram, objTelegram);
            }
            else
            {
                // 状态命令
                UnpackTelegramStatus(out struReceiveTelegram, objTelegram);
            }
         
            StruRecvTelegram = struReceiveTelegram;           
        }

        /// <summary>
        /// 打包报文
        /// </summary>
        /// <param name="objTelegram"></param>
        /// <param name="struSendTelegram"></param>
        public void PackTelegram(out Int16[] objTelegram, struTelegram struSendTelegram)
        {            
            StruSendTelegram = struSendTelegram;
            //m_nStatusCommandType---1074
            if (struSendTelegram.nTelegramType != m_nStatusCommandType - 1000)
            {
                // 动作命令
                PackTelegramMove(out objTelegram, struSendTelegram);
            }
            else
            {
                // 状态命令
                PackTelegramStatus(out objTelegram, struSendTelegram);
            }
           
            TelegramSend.Initialize();
            objTelegram.CopyTo(TelegramSend, 0);          
        }

        /// <summary>
        /// 动作命令
        /// </summary>
        /// <param name="struReceiveTelegram"></param>
        /// <param name="objTelegram"></param>
        private void UnpackTelegramMove(out struTelegram struReceiveTelegram, Int16[] objTelegram)
        {
            struReceiveTelegram = new struTelegram();

            struReceiveTelegram.nEMSSendFlag = objTelegram[0];
            struReceiveTelegram.nPLCSendFlag = objTelegram[1];
            struReceiveTelegram.nTelegramType = objTelegram[2];
            struReceiveTelegram.nSubTypeEMS = objTelegram[3];
            struReceiveTelegram.nSubTypePLC = objTelegram[4];
            //CBaseMethods.MyBase.StringToUInt32(Int16sToString(objTelegram, 6, 10, 4), out struReceiveTelegram.nEquipID);
            //struReceiveTelegram.strICCardID = Int16sToStringByASCII(objTelegram, 11, 22, 4);
            struReceiveTelegram.nEquipID=objTelegram[6];
            struReceiveTelegram.strICCardID = (Convert.ToString(objTelegram[11])).PadLeft(4,'0');
            struReceiveTelegram.strCarSize = Convert.ToString(objTelegram[23]);
            struReceiveTelegram.offCenter=objTelegram[24];            
            struReceiveTelegram.nWheelbase = objTelegram[25];
            struReceiveTelegram.carWeight = objTelegram[26];
            struReceiveTelegram.rearWheelDis = objTelegram[28];
            struReceiveTelegram.nDirection = objTelegram[29];
            struReceiveTelegram.strSrcLocAddr = Int16sToString(objTelegram, 30, 34, 2);
            struReceiveTelegram.strDestLocAddr = Int16sToString(objTelegram, 35, 39, 2);
            struReceiveTelegram.overallLg=objTelegram[45];
            struReceiveTelegram.overHang = objTelegram[46];
            struReceiveTelegram.nTelegramID = objTelegram[48];
            struReceiveTelegram.nEndFlag = objTelegram[49];
        }

        /// <summary>
        /// 状态命令
        /// </summary>
        /// <param name="struReceiveTelegram"></param>
        /// <param name="objTelegram"></param>
        private void UnpackTelegramStatus(out struTelegram struReceiveTelegram, Int16[] objTelegram)
        {
            struReceiveTelegram = new struTelegram();

            struReceiveTelegram.nEMSSendFlag = objTelegram[0];
            struReceiveTelegram.nPLCSendFlag = objTelegram[1];
            struReceiveTelegram.nTelegramType = objTelegram[2];
            struReceiveTelegram.nSubTypeEMS = objTelegram[3];
            struReceiveTelegram.nSubTypePLC = objTelegram[4];
            CBaseMethods.MyBase.StringToUInt32(Int16sToString(objTelegram, 6, 10, 4), out struReceiveTelegram.nEquipID);
            CBaseMethods.MyBase.StringToUInt32(Int16sToString(objTelegram, 17, 18, 4), out struReceiveTelegram.nSMGValidity);
            struReceiveTelegram.nTelegramID = objTelegram[48];
            struReceiveTelegram.nEndFlag = objTelegram[49];
        }

        /// <summary>
        /// 动作命令
        /// </summary>
        /// <param name="objTelegram"></param>
        /// <param name="struSendTelegram"></param>
        private void PackTelegramMove(out Int16[] objTelegram, struTelegram struSendTelegram)
        {
            objTelegram = new Int16[m_nTelegramLen];

            objTelegram[0] = (Int16)struSendTelegram.nEMSSendFlag;
            objTelegram[1] = (Int16)struSendTelegram.nPLCSendFlag;
            objTelegram[2] = (Int16)struSendTelegram.nTelegramType;
            objTelegram[3] = (Int16)struSendTelegram.nSubTypeEMS;
            objTelegram[4] = (Int16)struSendTelegram.nSubTypePLC;
         
            objTelegram[6] = (Int16)struSendTelegram.nEquipID;
            if (!string.IsNullOrEmpty(struSendTelegram.strICCardID))
            {
                objTelegram[11] = Convert.ToInt16(struSendTelegram.strICCardID);
            }
            if (!string.IsNullOrEmpty(struSendTelegram.strCarSize))
            {
                objTelegram[23] = Convert.ToInt16(struSendTelegram.strCarSize);
            }
            objTelegram[24] = (Int16)struSendTelegram.offCenter;
            objTelegram[25] = (Int16)struSendTelegram.nWheelbase;
            objTelegram[26] = (Int16)struSendTelegram.carWeight;
            objTelegram[28] = (Int16)struSendTelegram.rearWheelDis;            
         
            if (!string.IsNullOrEmpty(struSendTelegram.strSrcLocAddr)) 
            {
                objTelegram[30] = Convert.ToInt16(struSendTelegram.strSrcLocAddr.Substring(0,1));
                objTelegram[31] = Convert.ToInt16(struSendTelegram.strSrcLocAddr.Substring(1, 2));
                objTelegram[32] = Convert.ToInt16(struSendTelegram.strSrcLocAddr.Substring(3));
            }
            if (!string.IsNullOrEmpty(struSendTelegram.strDestLocAddr)) 
            {
                objTelegram[35] = Convert.ToInt16(struSendTelegram.strDestLocAddr.Substring(0, 1));
                objTelegram[36] = Convert.ToInt16(struSendTelegram.strDestLocAddr.Substring(1, 2));
                objTelegram[37] = Convert.ToInt16(struSendTelegram.strDestLocAddr.Substring(3));
            }

            objTelegram[45] = (Int16)struSendTelegram.overallLg;
            objTelegram[46] = (Int16)struSendTelegram.overHang;

            objTelegram[48] = (Int16)struSendTelegram.nTelegramID;
            objTelegram[49] = (Int16)struSendTelegram.nEndFlag;
        }

        /// <summary>
        /// 状态命令
        /// </summary>
        /// <param name="objTelegram"></param>
        /// <param name="struSendTelegram"></param>
        private void PackTelegramStatus(out Int16[] objTelegram, struTelegram struSendTelegram)
        {
            objTelegram = new Int16[m_nTelegramLen];

            objTelegram[0] = (Int16)struSendTelegram.nEMSSendFlag;
            objTelegram[1] = (Int16)struSendTelegram.nPLCSendFlag;
            objTelegram[2] = (Int16)struSendTelegram.nTelegramType;
            objTelegram[3] = (Int16)struSendTelegram.nSubTypeEMS;
            objTelegram[4] = (Int16)struSendTelegram.nSubTypePLC;
            StringToInt16s(ref objTelegram, 6, 10, 4, struSendTelegram.nEquipID.ToString());
            StringToInt16s(ref objTelegram, 17, 18, 4, struSendTelegram.nSMGValidity.ToString());
            objTelegram[48] = (Int16)struSendTelegram.nTelegramID;
            objTelegram[49] = (Int16)struSendTelegram.nEndFlag;
        }

        /// <summary>
        /// Int16[]数组转为字符串(高-低)
        /// </summary>
        /// <param name="nStart"></param>
        /// <param name="nEnd"></param>
        /// <param name="nAlignNum"></param>左对齐宽度
        /// <returns></returns>
        public string Int16sToString(Int16[] objTelegram, int nStart, int nEnd, int nAlignNum)
        {
            string str = "";
            bool isStart = true;
            if (nStart == 30 || nStart == 35)
            {// 车位地址(低-高  边、列、层)
                for (int i = nStart; i <= nStart + 2; i++)// nEnd
                {
                    if (isStart)
                    {
                        str += objTelegram[i].ToString();
                        isStart = false;
                    }
                    else
                    {
                        str += objTelegram[i].ToString().PadLeft(nAlignNum, '0');
                    }
                }

                if (string.IsNullOrWhiteSpace(str))
                {
                    return "0";
                }
                return str;
            }

            for (int i = nEnd; i >= nStart; i--)
            {
                if (isStart && objTelegram[i] == 0)
                {
                    continue;
                }

                if (isStart)
                {
                    str += objTelegram[i].ToString();
                    isStart = false;
                }
                else
                {
                    str += objTelegram[i].ToString().PadLeft(nAlignNum, '0');
                }
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                return "0";
            }
            return str;
        }

        /// <summary>
        /// 字符串转为Int16[]数组(高-低)
        /// </summary>
        /// <param name="objTelegram"></param>
        /// <param name="nStart"></param>
        /// <param name="nEnd"></param>
        /// <param name="nAlignNum"></param>
        /// <param name="str"></param>
        public void StringToInt16s(ref Int16[] objTelegram, int nStart, int nEnd, int nAlignNum, string str)
         {
             if (string.IsNullOrWhiteSpace(str))
             {
                 return;
             }

             if (nStart == 30 || nStart == 35)
             {// 车位地址(低-高  边、列、层)
                 for (int i = nStart, j = 0; i <= nEnd; i++)
                 {
                     if (j == 0)
                     {
                         objTelegram[i] = Convert.ToInt16(str.Substring(0, 1));
                         j += 1;
                     }
                     else if (j + nAlignNum <= str.Length)
                     {
                         objTelegram[i] = Convert.ToInt16(str.Substring(j, nAlignNum));
                         j += nAlignNum;
                     }
                     else
                     {
                         break;
                     }
                 }

                 return;
             }

             for (int i = nStart, j = str.Length; i <= nEnd; i++)
             {
                 j -= nAlignNum;
                 if (j <= 0)
                 {
                     objTelegram[i] = Convert.ToInt16(str.Substring(0, nAlignNum + j));
                     break;
                 }
                 else
                 {
                     string value = str.Substring(j, nAlignNum);
                     if (value.Contains("-"))
                     {
                         value = value.TrimStart('0');
                     }
                     objTelegram[i] = Convert.ToInt16(value);
                 }
             }
         }
       
        /// <summary>
        /// 数组转字符串(通过ASCII)
        /// </summary>
        /// <param name="strICCard"></param>
        /// <param name="nsTelegram"></param>
        public string Int16sToStringByASCII(Int16[] nsTelegram, int nStart, int nEnd, int nAlignNum)
        {
            string strICCard = string.Empty;
            for (int i = nEnd; i >= nStart; i--)
            {
                if (nsTelegram[i] <= 0)
                {
                    continue;
                }

                strICCard += Convert.ToChar(nsTelegram[i]);
            }

            if (string.IsNullOrWhiteSpace(strICCard))
            {
                return "0";
            }

            return strICCard;
        }

        /// <summary>
        /// 字符串转数组(通过ASCII)
        /// </summary>
        /// <param name="strICCard"></param>
        /// <param name="nsTelegram"></param>
        public void StringToInt16sByASCII(ref Int16[] nsTelegram, int nStart, int nEnd, int nAlignNum, string strICCard)
        {
            if (string.IsNullOrWhiteSpace(strICCard))
            {
                return;
            }

            for (int i = nEnd - nStart, j = nStart; i >= 0; i--)
            {
                if (i < strICCard.Length)
                {
                    nsTelegram[j++] = Convert.ToInt16(strICCard[i]);
                }
            }
        }
    }
}
