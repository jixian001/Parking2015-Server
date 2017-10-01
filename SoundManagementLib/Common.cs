using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SoundManagementLib
{
    /// <summary>
    /// MCI输出设备性能结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct struWaveOutCaps
    {
        public int wMid;
        public int wPid;
        public int vDriverVersion;
        public string szPname;
        public int dwFormats;
        public int wChannels;
        public int dwSupport;
    }

    /// <summary>
    /// MCI打开参数结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MCI_OPEN_PARMS
    {
        public int dwCallback;        // 回调窗口句柄
        public int wDeviceID;         // 设备打开成功，返回的设备号
        public string lpstrDeviceType;  // 设备类型
        public string lpstrElementName; // 复合设备的设备元素，通常为
        public string lpstrAlias;         // 指定的设备别名
    }

    /// <summary>
    /// MCI播放参数结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MCI_PLAY_PARMS
    {
        public int dwCallback;
        public int dwFrom;
        public int dwTo;
    }

    public class Common
    {
        // Constants
        public const int MCI_OPEN = 2066;
        public const int MCI_PLAY = 2;



        [DllImport("Kernel32", CharSet = CharSet.Auto)]
        public static extern Int32 GetShortPathName(String path, StringBuilder shortPath, Int32 shortPathLength);

        /// <summary>
        /// 函数用于向MCI设备发送一个命令消息
        /// </summary>
        /// <param name="IDDevice">接受命令的设备ID,由MCI_OPEN 命令的wDeviceID变量返回</param>
        /// <param name="uMsg">MCI 命令</param>
        /// <param name="fdwCommand">flags 列表区，一般都与Int32 dwParam 中的相关成员配合使用</param>
        /// <param name="dwParam">相应MCI命令的参数区类型</param>
        /// <returns></returns>0 表示成功，否则为出错代码
        [DllImport("winmm.dll")]
        public static extern int mciSendCommand(int IDDevice, Int32 uMsg, int fdwCommand, IntPtr dwParam);

        /// 向媒体控制接口发送控制命令 
        /// </summary> 
        /// <param name="lpszCommand">命令，参见 
        /// _stprintf_s(lpszCommand, TEXT("play %s %s %s"), lpszDeviceID, lpszPlayFlags, lpszFlags); 
        /// play mysound from 1000 to 2000 notify
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd743572(v=vs.85).aspx </param> 
        /// <param name="lpszReturnString">命令返回的信息，如果没有需要返回的信息可以为null</param> 
        /// <param name="cchReturn">指定返回信息的字符串大小</param>
        /// <param name="hwndCallback">回调句柄，如果命令参数中没有指定notify标识，可以为new IntPtr(0)</param> 
        /// <returns>返回命令执行状态的错误代码</returns> 
        [DllImport("winmm.dll")]
        public static extern int mciSendString(string lpszCommand, StringBuilder lpszReturnString, int cchReturn, int hwndCallback);

        [DllImport("winmm.dll")]
        public static extern bool PlaySound(string szSound, IntPtr hMod);

        // 检取系统中存在的波形输出设备的数量
        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        public static extern int waveOutGetNumDevs();

        // 检取系统中存在的MIDI输出设备的数量
        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiOutGetNumDevs();

        // 查询一个指定的波形输出设备以确定其性能
        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        public static extern int waveOutGetDevCaps(int nDeviceID, ref struWaveOutCaps lpCaps, int nSize);
    }
}
