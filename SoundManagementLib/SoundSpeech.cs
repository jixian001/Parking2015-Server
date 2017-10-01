using System.Speech.Synthesis;
using System.IO;

namespace SoundManagementLib
{
    public class CSoundSpeech : CSound
    {        
        private SpeechSynthesizer m_synth = new SpeechSynthesizer();//语音对象
        // 若要配置 SpeechSynthesizer 对象的输出，请使用 SetOutputToAudioStream、SetOutputToDefaultAudioDevice、SetOutputToNull和 SetOutputToWaveFile 方法。

        /// <summary>
        /// 构造函数
        /// </summary>
        public CSoundSpeech()
        {
            // 配置 SpeechSynthesizer 对象以发送输出到默认音频设备。 
            m_synth.SetOutputToDefaultAudioDevice();// 设置播放语音设备，为当前默认
            m_synth.Rate = 0;// 播放语速，-10~10
            m_synth.Volume = 100;// 音量调节：0~100
        }

        /// <summary>
        /// 语音播放 
        /// </summary>
        /// <param name="soundText"></param>
        public override void ReadSound(string soundText)
        {
            //// 获取本机上所安装的所有的Voice的名称
            //ReadOnlyCollection<InstalledVoice> iv = m_synth.GetInstalledVoices();

            //foreach (InstalledVoice s in iv)
            //{
            //    m_synth.SelectVoice(s.VoiceInfo.Name);
                m_synth.SpeakAsync(soundText);
            //}
        }

        /// <summary>
        /// 生成文本语音文件
        /// </summary>
        /// <param name="soundText"></param>
        public override void CreatSoundFile(string strPath, string strFileName, string strText)
        {
            string strFile = strPath + strFileName;
            if (File.Exists(strFile))
            {
                return;
            }
            else
            {
                System.IO.Directory.CreateDirectory(strPath);
            }

            m_synth.SetOutputToWaveFile(strFile);
            m_synth.Speak(strText);
            m_synth.SetOutputToNull();
            m_synth.SetOutputToDefaultAudioDevice();
        }

        /// <summary>
        /// 修改语音文件文本内容
        /// </summary>
        /// <param name="soundText"></param>
        public override void UpdateSoundFileContext(string strPath, string strFileName, string strText)
        {
            strFileName = strPath + strFileName;
            m_synth.SetOutputToWaveFile(strFileName);
            m_synth.Speak(strText);
            m_synth.SetOutputToNull();
            m_synth.SetOutputToDefaultAudioDevice();
        }
    }
}
