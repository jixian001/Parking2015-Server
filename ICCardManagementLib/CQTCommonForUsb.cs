using System;
using System.Runtime.InteropServices;

namespace ICCardManagementLib
{
    /// <summary>
    /// 读卡器操作动态库
    /// </summary>
    public class CQTCommonForUsb
    {
        #region  读卡器操作动态库
        /// <summary>
        /// 初始化读写器
        /// </summary>
        /// <param name="port">通讯端口号</param>
        /// <param name="baud">通信波特率</param>
        /// <returns>返回值</returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_init", SetLastError = true,
         CharSet = CharSet.Auto, ExactSpelling = false,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int rf_init(int port, int baud);

        /// <summary>
        /// 关闭串口，并保存PC机上的设置
        /// </summary>
        /// <param name="icdev">rf_init返回设备描述</param>
        /// <returns>返回值</returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_exit", SetLastError = true,
         CharSet = CharSet.Auto, ExactSpelling = false,
         CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_exit(int icdev);

        /// <summary>
        /// 返回设备当前状态
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_get_status", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     返回设备当前状态
        public static extern Int16 rf_get_status(int icdev, [MarshalAs(UnmanagedType.LPArray)]byte[] state);

        /// <summary>
        ///37.蜂鸣几毫秒。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="_Msec">蜂鸣时间，单位：毫秒</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_beep", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_beep(int icdev, int _Msec);

        /// <summary>
        /// 加载秘钥
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="secnr"></param>
        /// <param name="keybuff"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_load_key", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     加载秘钥
        public static extern Int16 rf_load_key(int icdev, int mode, int secnr, [MarshalAs(UnmanagedType.LPArray)]byte[] keybuff);

        /// <summary>
        /// 加载秘钥,十六进制格式
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="secnr"></param>
        /// <param name="keybuff"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_load_key_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：      加载秘钥,十六进制格式
        public static extern Int16 rf_load_key_hex(int icdev, int mode, int secnr, [MarshalAs(UnmanagedType.LPArray)]byte[] keybuff);

        /// <summary>
        /// 字符数组转十六进制
        /// </summary>
        /// <param name="asc"></param>
        /// <param name="hex"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "a_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     字符数组转十六进制
        public static extern Int16 a_hex([MarshalAs(UnmanagedType.LPArray)]byte[] asc, [MarshalAs(UnmanagedType.LPArray)]byte[] hex, int len);

        /// <summary>
        /// 十六进制转字符数组
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="asc"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "hex_a", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     十六进制转字符数组
        public static extern Int16 hex_a([MarshalAs(UnmanagedType.LPArray)]byte[] hex, [MarshalAs(UnmanagedType.LPArray)]byte[] asc, int len);

        /// <summary>
        /// 将RF（射频）模块的能量释放几毫秒
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="msec">复位时间 ( 0~ 500ms)</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_reset", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     返回设备当前状态
        public static extern Int16 rf_reset(int icdev, int msec);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="_b"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_clr_control_bit", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_clr_control_bit(int icdev, int _b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="_b"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_set_control_bit", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_set_control_bit(int icdev, int _b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="disp"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_disp8", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_disp8(int icdev, short mode, [MarshalAs(UnmanagedType.LPArray)]byte[] disp);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_disp", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_disp(int icdev, short mode, int digit);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ptrsource"></param>
        /// <param name="len"></param>
        /// <param name="ptrdest"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_encrypt", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_encrypt([MarshalAs(UnmanagedType.LPArray)]byte[] key, [MarshalAs(UnmanagedType.LPArray)]byte[] ptrsource, int len, [MarshalAs(UnmanagedType.LPArray)]byte[] ptrdest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ptrsource"></param>
        /// <param name="len"></param>
        /// <param name="ptrdest"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_decrypt", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_decrypt([MarshalAs(UnmanagedType.LPArray)]byte[] key, [MarshalAs(UnmanagedType.LPArray)]byte[] ptrsource, int len, [MarshalAs(UnmanagedType.LPArray)]byte[] ptrdest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="databuff"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_srd_eeprom", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_srd_eeprom(int icdev, int offset, int len, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="databuff"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_swr_eeprom", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_swr_eeprom(int icdev, int offset, int len, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="_byte"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_setport", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_setport(int icdev, byte _byte);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="_byte"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_getport", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_getport(int icdev, out byte _byte);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_gettime", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_gettime(int icdev, [MarshalAs(UnmanagedType.LPArray)]byte[] time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_gettime_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_gettime_hex(int icdev, [MarshalAs(UnmanagedType.LPArray)]byte[] time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_settime_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_settime_hex(int icdev, [MarshalAs(UnmanagedType.LPArray)]byte[] time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_settime", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_settime(int icdev, [MarshalAs(UnmanagedType.LPArray)]byte[] time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="bright"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = " rf_setbright", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_setbright(int icdev, byte bright);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_ctl_mode", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_ctl_mode(int icdev, int mode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_disp_mode", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_disp_mode(int icdev, int mode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "lib_ver", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 lib_ver([MarshalAs(UnmanagedType.LPArray)]byte[] ver);

        /// <summary>
        /// 该函数向卡片发出寻卡命令，开始选择一张新卡片时需要执行该函数。
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode">
        /// 寻卡模式:0 IDLE mode, 只有处在IDLE 状态的卡片才响应读写器的命令。1 ALL mode, 处在 IDLE 状态和HALT 状态的卡片都将响应读写器的命令。
        /// </param>
        /// <param name="tagtype">返回卡片类型</param>
        /// <returns>返回值</returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_request", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_request(int icdev, int mode, out UInt16 tagtype);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="tagtype"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_request_std", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_request_std(int icdev, int mode, out UInt16 tagtype);

        /// <summary>
        /// 激活读写器的防冲突队列。如果有几张MIFARE 卡片在感应区内，将会选择一张卡片，并返回卡片的序列号供将来调用rf_select 函数时使用。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="bcnt">预选卡片使用的位, 标准调用时为bcnt=0.</param>
        /// <param name="snr">返回的卡片序列号</param>
        /// <returns>返回值</returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_anticoll", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_anticoll(int icdev, int bcnt, out uint snr);

        /// <summary>
        /// 用指定的序列号选择卡片，将卡片的容量返回给PC 机。
        /// </summary>
        /// <param name="icdev">返回的设备描述符</param>
        /// <param name="snr">卡片的序列号</param>
        /// <param name="size">卡片容量的地址指针，目前该值不能使用</param>
        /// <returns>返回值</returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_select", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        //说明：     返回设备当前状态
        public static extern Int16 rf_select(int icdev, uint snr, out byte size);

        /// <summary>
        ///7.验证读写器中的密码与需要访问的卡片的同一扇区(0~15)的密码是否一致。如果读写器中选择的密码（可用rf_load_key 函数修改）与卡片的相匹配，密码验证通过，传输的数据将用以下的命令加密
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="mode">验证密码类型：0 — 用KEY A 验证 4 — 用 KEY B 验证</param>
        /// <param name="secnr">将要访问的卡片扇区号(0~15)</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_authentication", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_authentication(int icdev, int mode, int secnr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="mode"></param>
        /// <param name="keynr"></param>
        /// <param name="blocknr"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_authentication_2", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_authentication_2(int icdev, int mode, int keynr, int blocknr);

        /// <summary>
        /// 从一张选定并通过密码验证的卡片读取一块共16个字节的数据。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="blocknr">读取数据的块号(0~63)</param>
        /// <param name="databuff">Data:读取的数据，PC 机上RAM 的地址空间由调用该函数来分配。</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_read", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_read(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 从一张选定并通过密码验证的卡片读取一块共16个字节的数据。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="blocknr">读取数据的块号(0~63)</param>
        /// <param name="databuff">Data:读取的数据，PC 机上RAM 的地址空间由调用该函数来分配。</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_read_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_read_hex(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <param name="databuff"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_write_hex", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_write_hex(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        /// 11.将一块共16字节写入选定并验证通过的卡片中。
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <param name="blocknr">写入数据的块地址 (1~63)</param>
        /// <param name="databuff">写入数据,长度为16字节</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_write", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_write(int icdev, int blocknr, [MarshalAs(UnmanagedType.LPArray)]byte[] databuff);

        /// <summary>
        ///8.将一张选中的卡片设为“Halt”模式，只有当该卡再次复位或用ALL 模式调用request 函数时，读写器才能够再次操作它
        /// </summary>
        /// <param name="icdev">rf_init()返回的设备描述符</param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_halt", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_halt(int icdev);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_initval", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_initval(int icdev, int blocknr, uint val);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_readval", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_readval(int icdev, int blocknr, out uint val);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_increment", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_increment(int icdev, int blocknr, uint val);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_decrement", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_decrement(int icdev, int blocknr, uint val);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_restore", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_restore(int icdev, int blocknr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev"></param>
        /// <param name="blocknr"></param>
        /// <returns></returns>
        [DllImport("mwrf32usb.dll", EntryPoint = "rf_transfer", SetLastError = true,
             CharSet = CharSet.Auto, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 rf_transfer(int icdev, int blocknr);
        #endregion
    }
}
