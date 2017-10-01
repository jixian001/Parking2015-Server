using System;
using Microsoft.DirectX.DirectSound;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using BaseMethodLib;
using LOGManagementLib;

namespace SoundManagementLib
{
    /// <summary>
    /// 利用DerectX控件
    /// </summary>
    public class CSoundDirectX : CSound
    {
        private Hashtable m_hashDevices = new Hashtable();
        private SecondaryBuffer m_buf;
        private int m_wareHouse = 0;
        private int m_hallID = 0;
        private string m_strDeviceDescription = string.Empty;      

        public CSoundDirectX()
        {
            SearchSoundDevice();
        }

        /// <summary>
        /// 设备哈希列表
        /// </summary>
        public Hashtable HashDevices
        {
            get
            {
                return m_hashDevices;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <param name="nDeviceID"></param>
        public CSoundDirectX(int nWareHouse, int nHallID, string strDeviceDescription)
        {
            m_wareHouse = nWareHouse;
            m_hallID = nHallID;
            m_strDeviceDescription = strDeviceDescription;
            SearchSoundDevice();
            CLOGException.Trace(m_wareHouse, "SoundManagementLib", "Install component(SoundManagementLib) successfully");
        }

        /// <summary>
        /// 语音播放 
        /// </summary>
        public override void ReadSound(string file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(m_strDeviceDescription) ||
                    string.IsNullOrWhiteSpace(file) ||
                    !m_hashDevices.ContainsKey(m_strDeviceDescription))
                {
                    return;
                }

                if (File.Exists(file) == false)
                {
                    return;
                }

                if (null != m_buf)
                {
                    m_buf.Stop();
                    m_buf.Dispose();
                    m_buf = null;
                }
                using (BufferDescription temp = new BufferDescription())
                {
                    temp.Flags = BufferDescriptionFlags.GlobalFocus;
                    m_buf = new SecondaryBuffer(file, temp, (Device)m_hashDevices[m_strDeviceDescription]);
                    m_buf.Play(0, BufferPlayFlags.Default);
                }

            }
            catch (Exception ex)
            {
                CLOGException.Trace("SoundManagementLib.CSoundDirectX.ReadSound 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
              
            }
        }

        /// <summary>
        /// 搜索所有音频设备
        /// </summary>
        public void SearchSoundDevice()
        {
            using (Form test = new Form())
            {
                DevicesCollection DeviceColl = new DevicesCollection();

                for (int i = 1; i < DeviceColl.Count; i++)
                {
                    DeviceInformation info = (DeviceInformation)DeviceColl[i];
                    if (string.IsNullOrEmpty(info.ModuleName)) 
                    {
                        continue;
                    }
                    Guid Dguid = info.DriverGuid;
                    Device dev = new Device(Dguid);
                    dev.SetCooperativeLevel(test, CooperativeLevel.Normal);
                    m_hashDevices.Add(DeviceColl[i].Description, dev);

                    CLOGException.Trace("SoundManagementLib", "Get sound equipment-[ " + DeviceColl[i].Description + " ] successfully");
                }
            }
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <param name="nDeviceID"></param>
        public void SetParams(int nWareHouse, int nHallID, string strDeviceDescription)
        {
            m_wareHouse = nWareHouse;
            m_hallID = nHallID;
            m_strDeviceDescription = strDeviceDescription;
        }
    }
}
