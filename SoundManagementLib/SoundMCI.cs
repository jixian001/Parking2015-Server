using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace SoundManagementLib
{
    public class CSoundMCI : CSound
    {
        public CSoundMCI(){ }

        /// <summary>
        /// 播放语音
        /// </summary>
        /// <param name="soundText"></param>
        public override void ReadSound(string soundText)
        {
            // 方法二 用MCI，通过API直接传递给MCI设备一个播放命令：（自我感觉该种方式不能重复播放）+ "from 10 to 2000"
            StringBuilder shortpath = new StringBuilder(80);            
            int result = Common.GetShortPathName(soundText, shortpath, shortpath.Capacity);
            StringBuilder buf = new StringBuilder();
            MCIOpen(shortpath.ToString());
            int nres = Common.mciSendString("play " + shortpath, buf, buf.Length, 0); //播放
            Thread.Sleep(getasfTime(soundText));
            Thread.Sleep(100);
            //bool f = Common.PlaySound(shortpath.ToString(), IntPtr.Zero);//播放
            int wavcount = Common.waveOutGetNumDevs();
            int midicount = Common.midiOutGetNumDevs();
            struWaveOutCaps wavOut = new struWaveOutCaps();
            int error = Common.waveOutGetDevCaps(0, ref wavOut, 20); 
        }
       
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        private int MCIOpen(string strPath)
        {
            MCI_OPEN_PARMS mciOP = new MCI_OPEN_PARMS();
            int dwReturn;

            // 由mci判断文件类型
            mciOP.lpstrElementName = strPath;
            GCHandle handle = GCHandle.Alloc(mciOP);
            IntPtr ptr = GCHandle.ToIntPtr(handle);
            dwReturn = Common.mciSendCommand(0, Common.MCI_OPEN, 0, ptr);
            return dwReturn;
        } 
    }
}
