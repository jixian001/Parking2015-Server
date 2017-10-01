using System;
using System.Collections.Generic;
using System.Linq;
using BaseMethodLib;
using SoundManagementLib;
using LOGManagementLib;

namespace AbstractActionLib
{
    public class CVoicePlayer
    {
        private string m_strPathAuto = System.AppDomain.CurrentDomain.BaseDirectory + @"/soundAuto/";// 自动配好的语音文件夹路径
        private Dictionary<int, CSound> m_dicSound;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CVoicePlayer()
        {
            m_dicSound = new Dictionary<int, CSound>();
        }

        /// <summary>
        /// 播放语音
        /// </summary>
        public void VoicePlayer(object objRequestParam)
        {
            try
            {
                CRequestAction RequestParam = (CRequestAction)objRequestParam;
                m_dicSound = RequestParam.DicSound;
                int nPLCID = RequestParam.struHallEquip.nWareHouseID;
                int nHallID = RequestParam.struHallEquip.nHallID;
                if (!m_dicSound.ContainsKey(nHallID))
                {
                    CLOGException.Trace("AbstractActionLib.CVoicePlayer.VoicePlayer ", string.Format("无效 PLC ID = {0},或无效 Hall ID = {1}", nPLCID, nHallID));
                    return;
                }
                CSound player = new CSound();
                player = m_dicSound[nHallID];

                if (typeof(CSoundDirectX) == player.GetType())
                {
                    // struHallEquip增加语音设备号参数（读取配置文件）
                    ((CSoundDirectX)player).SetParams(RequestParam.struHallEquip.nWareHouseID, RequestParam.struHallEquip.nHallID, RequestParam.struHallEquip.strAudioDeviceDesc);
                }

                player.ReadSound(m_strPathAuto + RequestParam.soundName);
                player = null;                
            }
            catch (Exception ex)
            {
                CLOGException.Trace("AbstractActionLib.CVoicePlayer.VoicePlayer 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
            }
        }
    }
}
