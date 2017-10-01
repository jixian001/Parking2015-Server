using System;
using System.Text;
using System.Threading;
using System.IO;
using BaseMethodLib;

namespace SoundManagementLib
{
    /// <summary>
    /// 语音管理
    /// </summary>
    public class CSound
    {
        public CSound()
        {}

        /// <summary>
        /// 播放语音
        /// </summary>
        /// <param name="soundText"></param>
        public virtual void ReadSound(string soundText)
        {
            // 方法一  SoundPlayer方式：两个声音文件不能同时播放
            System.Media.SoundPlayer startSound = new System.Media.SoundPlayer(soundText);
            if (File.Exists(soundText))
            {
                startSound.Play();
                Thread.Sleep(getasfTime(soundText));
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 生成文本语音文件
        /// </summary>
        /// <param name="soundText"></param>
        public virtual void CreatSoundFile(string strPath, string strFileName, string strText)
        { }

        /// <summary>
        /// 修改语音文件文本内容
        /// </summary>
        /// <param name="soundText"></param>
        public virtual void UpdateSoundFileContext(string strPath, string strFileName, string strText)
        { }

        /// <summary>
        /// 获取播放文件时间间隔
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected TimeSpan getasfTime(string filePath)
        {
            StringBuilder shortpath = new StringBuilder(80);
            Common.GetShortPathName(filePath, shortpath, shortpath.Capacity);
            string name = shortpath.ToString();
            StringBuilder buf = new StringBuilder(80);
            Common.mciSendString("close all", buf, buf.Capacity, 0);
            Common.mciSendString("open " + name + " alias media", buf, buf.Capacity, 0);
            Common.mciSendString("status media length", buf, buf.Capacity, 0);
            double dou = 0.0;
            CBaseMethods.MyBase.StringToDouble(buf.ToString().Trim(), out dou);
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)dou);
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)Convert.ToDouble(buf.ToString().Trim()));
            return ts;
        }
    }


    #region Voice
    // class Voice
    // {
    //     private MemoryStream memstream;//内存流
    //     private SecondaryBuffer secBuffer;//辅助缓冲区
    //     private int iNotifySize = 0;//通知大小
    //     private int iBufferSize = 0;//捕捉缓冲区大小 
    //     private CaptureBuffer capturebuffer;//捕捉缓冲区对象
    //     private AutoResetEvent notifyEvent;//通知事件
    //     private Thread notifyThread;//通知线程
    //     private int iNotifyNum = 16;//通知个数
    //     private Notify myNotify;//通知对象
    //     private Capture capture;//捕捉设备对象
    //     private Device PlayDev;//播放设备对象
    //     private BufferDescription buffDiscript;
    //     private Socket Client;
    //     private EndPoint epServer;
    //     private int iBufferOffset = 0;//捕捉缓冲区位移
    //     private IntPtr intptr;//窗口句柄
           
    //     public IntPtr Intptr
    //     {
    //         set
    //         {
    //             intptr = value;
    //         }
    //     }
         
    //     public int NotifySize
    //     {
    //         set
    //         {
    //             iNotifySize = value;
    //         }
    //     }
         
    //     public int NotifyNum
    //     {
    //         set
    //         {
    //             iNotifyNum = value;
    //         }
    //    }
         
    //     public Socket LocalSocket
    //     {
    //         set
    //         {
    //             Client = value;
    //         }
    //     }
        
    //     public EndPoint RemoteEndPoint
    //     {
    //         set
    //         {
    //             epServer = value;
    //         }
    //     }
        
    //     public void InitVoice()
    //     {//初始化声音相关设置：（1）捕捉缓冲区（2）播放缓冲区
    //         if (!CreateCaputerDevice())
    //         {
    //             throw new Exception();
    //         }//建立设备对象
    //         CreateCaptureBuffer();//建立缓冲区对象
    //         CreateNotification();//设置通知及事件
    //         //======（2）==============
    //         if (!CreatePlayDevice())
    //         {
    //             throw new Exception();
    //         }
    //         CreateSecondaryBuffer();
    //     }
        
    //     public void StartVoiceCapture()
    //     {
    //         capturebuffer.Start(true);//true表示设置缓冲区为循环方式，开始捕捉
    //     }
              
    //     private bool CreatePlayDevice()
    //     {
    //         DevicesCollection dc = new DevicesCollection();
    //         Guid g;
    //         if (dc.Count > 0)
    //         {
    //             g = dc[0].DriverGuid;
    //         }
    //         else
    //         { return false; }
    //         PlayDev = new Device(g);
    //         PlayDev.SetCooperativeLevel(intptr, CooperativeLevel.Normal);
    //         return true;
    //     }
        
    //     private void CreateSecondaryBuffer()
    //     {
    //         buffDiscript = new BufferDescription();
    //         WaveFormat mWavFormat = SetWaveFormat();
    //         buffDiscript.Format = mWavFormat;
    //         //iNotifySize = mWavFormat.AverageBytesPerSecond / iNotifyNum;//设置通知大小
    //         iNotifySize = (1024 > mWavFormat.AverageBytesPerSecond / 8) ? 1024 : (mWavFormat.AverageBytesPerSecond / 8);
    //         iNotifySize -= iNotifySize % mWavFormat.BlockAlign;
    //         iBufferSize = iNotifyNum * iNotifySize;
    //         buffDiscript.BufferBytes = iBufferSize;
    //         buffDiscript.ControlPan = true;
    //         buffDiscript.ControlFrequency = true;
    //         buffDiscript.ControlVolume = true;
    //         buffDiscript.GlobalFocus = true;
    //         secBuffer = new SecondaryBuffer(buffDiscript, PlayDev);
    //         byte[] bytMemory = new byte[100000];
    //         memstream = new MemoryStream(bytMemory, 0, 100000, true, true);
    //         //g729 = new G729();
    //         //g729.InitalizeEncode();
    //         //g729.InitalizeDecode();
    //     }
         
    //     private bool CreateCaputerDevice()
    //     {
    //         //首先要玫举可用的捕捉设备
    //         CaptureDevicesCollection capturedev = new CaptureDevicesCollection();
    //         Guid devguid;
    //         if (capturedev.Count > 0)
    //         {
    //             devguid = capturedev[0].DriverGuid;
    //         }
    //         else
    //         {
    //             System.Windows.Forms.MessageBox.Show("当前没有可用于音频捕捉的设备", "系统提示");
    //             return false;
    //         }
    //         //利用设备GUID来建立一个捕捉设备对象
    //         capture = new Capture(devguid);
    //         return true;
    //     }
         
    //     private void CreateCaptureBuffer()
    //     {
            
    //         WaveFormat mWavFormat = SetWaveFormat();
    //         CaptureBufferDescription bufferdescription = new CaptureBufferDescription();
    //         bufferdescription.Format = mWavFormat;//设置缓冲区要捕捉的数据格式
    //        // iNotifySize = mWavFormat.AverageBytesPerSecond / iNotifyNum;//1秒的数据量/设置的通知数得到的每个通知大小小于0.2s的数据量，话音延迟小于200ms为优质话音
    //         iNotifySize = (1024 > mWavFormat.AverageBytesPerSecond / 8) ? 1024 : (mWavFormat.AverageBytesPerSecond / 8);
    //         iNotifySize -= iNotifySize % mWavFormat.BlockAlign;
    //         iBufferSize = iNotifyNum * iNotifySize;
    //         bufferdescription.BufferBytes = iBufferSize;
    //         bufferdescription.ControlEffects = true;
    //         bufferdescription.WaveMapped = true;
    //         capturebuffer = new CaptureBuffer(bufferdescription, capture);    
    //     }
         
    //     private void CreateNotification()
    //     {
    //         BufferPositionNotify[] bpn = new BufferPositionNotify[iNotifyNum];
    //         //设置通知事件
    //         notifyEvent = new AutoResetEvent(false);
    //         notifyThread = new Thread(RecoData);//通知触发事件
    //         notifyThread.IsBackground = true;
    //         notifyThread.Start();
    //         for (int i = 0; i < iNotifyNum; i++)
    //         {
    //             bpn[i].Offset = iNotifySize + i * iNotifySize - 1;//设置具体每个的位置
    //             bpn[i].EventNotifyHandle = notifyEvent.Handle;
    //         }
    //         myNotify = new Notify(capturebuffer);
    //         myNotify.SetNotificationPositions(bpn);
    //     }

    //     private void RecoData()
    //     {
    //         while (true)
    //         {
    //             // 等待缓冲区的通知消息
    //             notifyEvent.WaitOne(Timeout.Infinite, true);
    //             // 录制数据
    //             RecordCapturedData(Client, epServer);
    //         }
    //     }
  
    //     private void RecordCapturedData(Socket Client, EndPoint epServer)
    //     {
    //         byte[] capturedata = null;
    //         int readpos = 0, capturepos = 0, locksize = 0;
    //         capturebuffer.GetCurrentPosition(out capturepos, out readpos);
    //         locksize = readpos - iBufferOffset;//这个大小就是我们可以安全读取的大小
    //         if (locksize == 0)
    //         {
    //             return;
    //         }
    //         if (locksize < 0)
    //         {
    //             locksize += iBufferSize;
    //         }
    //         capturedata = (byte[])capturebuffer.Read(iBufferOffset, typeof(byte), LockFlag.FromWriteCursor, locksize);
           
    //         try
    //         {
    //             Client.SendTo(capturedata, epServer);//传送语音
    //         }
    //         catch
    //         {
    //             throw new Exception();
    //         }
    //         iBufferOffset += capturedata.Length;
    //         iBufferOffset %= iBufferSize;
    //     }
         
    //     private int intPosWrite = 0;
    //     private int intPosPlay = 0;
    //     private int intNotifySize = 5000;
         
    //     public void GetVoiceData(int intRecv, byte[] bytRecv)
    //     {
            
    //         if (intPosWrite + intRecv <= memstream.Capacity)
    //         {
    //             if ((intPosWrite - intPosPlay >= 0 && intPosWrite - intPosPlay < intNotifySize) || (intPosWrite - intPosPlay < 0 && intPosWrite - intPosPlay + memstream.Capacity < intNotifySize))
    //             {
    //                 memstream.Write(bytRecv, 0, intRecv);
    //                 intPosWrite += intRecv;
    //             }
    //             else if (intPosWrite - intPosPlay >= 0)
    //             {
    //                 buffDiscript.BufferBytes = intPosWrite - intPosPlay;//缓冲区大小为播放指针到写指针之间的距离。
    //                 SecondaryBuffer sec = new SecondaryBuffer(buffDiscript, PlayDev);//建立一个合适的缓冲区用于播放这段数据。
    //                 memstream.Position = intPosPlay;//先将memstream的指针定位到这一次播放开始的位置
    //                 sec.Write(0, memstream, intPosWrite - intPosPlay, LockFlag.FromWriteCursor);
    //                 sec.Play(0, BufferPlayFlags.Default);
    //                 memstream.Position = intPosWrite;//写完后重新将memstream的指针定位到将要写下去的位置。
    //                 intPosPlay = intPosWrite;
    //             }
    //             else if (intPosWrite - intPosPlay < 0)
    //             {
    //                 buffDiscript.BufferBytes = intPosWrite - intPosPlay + memstream.Capacity;//缓冲区大小为播放指针到写指针之间的距离。
    //                 SecondaryBuffer sec = new SecondaryBuffer(buffDiscript, PlayDev);//建立一个合适的缓冲区用于播放这段数据。
    //                 memstream.Position = intPosPlay;
    //                 sec.Write(0, memstream, memstream.Capacity - intPosPlay, LockFlag.FromWriteCursor);
    //                 memstream.Position = 0;
    //                 sec.Write(memstream.Capacity - intPosPlay, memstream, intPosWrite, LockFlag.FromWriteCursor);
    //                 sec.Play(0, BufferPlayFlags.Default);
    //                 memstream.Position = intPosWrite;
    //                 intPosPlay = intPosWrite;
    //             }
    //         }
    //         else
    //         {
    //             int irest = memstream.Capacity - intPosWrite;
    //             memstream.Write(bytRecv, 0, irest);
    //             memstream.Position = 0;                
    //             memstream.Write(bytRecv, irest, intRecv - irest);
    //             intPosWrite = intRecv - irest;
    //         }
    //     }
        
    //     private WaveFormat SetWaveFormat()
    //     {
    //         WaveFormat format = new WaveFormat();
    //         format.FormatTag = WaveFormatTag.Pcm;//设置音频类型
    //         format.SamplesPerSecond = 11025;//采样率（单位：赫兹）典型值：11025、22050、44100Hz
    //         format.BitsPerSample = 16;//采样位数
    //         format.Channels = 1;//声道
    //         format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
    //         format.AverageBytesPerSecond = format.BlockAlign * format.SamplesPerSecond;
 
    //        return format;
        
    //     }
    //}
    #endregion
}
