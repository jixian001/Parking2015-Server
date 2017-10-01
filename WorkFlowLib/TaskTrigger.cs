using System;
using System.Linq;
using System.Collections.Generic;
using TypedefLib;
using BaseMethodLib;
using LOGManagementLib;
using ConfigManagmtLib;
using DatabaseManagementLib;
using CommonMethodLib;
using AbstractActionLib;
using CommunicationLib;

namespace WorkFlowLib
{
    /// <summary>
    /// 业务处理
    /// </summary>
    public abstract class CTaskTrigger
    {
        protected CCommModule m_commModule = null;// PLC服务器对象
        protected int m_nColorAddrHall = 0;
        public int nColorAddrHall
        {
            get { return m_nColorAddrHall; }
            set { m_nColorAddrHall = value; }
        }

        protected int m_nColorAddrETVorTV = 0;
        public int nColorAddrETVorTV
        {
            get { return m_nColorAddrETVorTV; }
            set { m_nColorAddrETVorTV = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CTaskTrigger()
        {
        }

        #region  报文处理函数
        /// <summary>
        /// 处理入库协议-1001
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealEnterTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);

            switch (struTelegram.nSubTypePLC)
            {
                case 1:
                    {
                        EnterCarStatus(out nActionFlag, ref deviceStatus, ref requestAction);
                        break;
                    }
                case 4://存车离开 或临时取物转存失败离开
                    {
                        // 清空当前卡号存取车排队信息
                        ClearWorkQueueSwipe(ref requestAction, struTelegram.strICCardID);

                        nActionFlag = 3;
                        requestAction.telegram = "1-55";// 报文类型2（1），报文子类型EMS 3（55）;
                        requestAction.soundName = "02.wav"; // 请慢走，欢迎下次光临02.wav
                        //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealEnterTelegram", "请慢走，欢迎下次光临02.wav");
                        // 车厅设备
                        if (null != deviceStatus)
                        {
                            if (!string.IsNullOrWhiteSpace(deviceStatus.iccode))
                            {
                                // 临时取物转存时出现故障无法存入，车辆退出清除车位信息
                                CCarLocationDto carLocationTBL = CCommonMethods.myMethods.GetCarLocation(deviceStatus.iccode);
                                // 更新车位信息carlocation 为空
                                InitCarPOSN(ref requestAction, carLocationTBL);
                            }
                            deviceStatus.iccode = null;
                            deviceStatus.tasktype = (int)EnmTaskType.Init;
                            // 车厅作业流程节点类型清空
                            deviceStatus.prevnode = null;
                            deviceStatus.currentnode = null;
                            deviceStatus.queueprevnode = null;
                            deviceStatus.queuecurrentnode = null;
                         
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                        }

                        // 存车（目的地址为车位地址）
                        CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr);
                        // 更新车位信息carlocation 为空
                        InitCarPOSN(ref requestAction, carLocation);
                        break;
                    }
                case 54:
                    {
                        EnterCommandAccept(out nActionFlag, ref requestAction, struTelegram, deviceStatus);
                        break;
                    }
                case 101:
                    {
                        EnterCarChecked(out nActionFlag, ref requestAction, struTelegram, deviceStatus);
                        break;
                    }
                case 104:
                    {
                        nActionFlag = 2;
                        requestAction.soundName = "04.wav"; //车体超限，请退出04.wav
                        //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealEnterTelegram", "车体超限，请退出04.wav");

                        // 车厅设备
                        if (null != deviceStatus)
                        {// 车厅作业流程节点类型赋值
                            deviceStatus.prevnode = deviceStatus.currentnode;
                            deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarOverrun;
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                        }

                        CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.SaveCarSecond);//2
                        if (null != SwipeCardInfoTable)
                        {
                            SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.Oversize;
                            requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 2));
                        }
                        else
                        {
                            SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.Oversize);//2
                            if (null == SwipeCardInfoTable)
                            {
                                CWorkQueueDto SwipeCardInfoTableNew = new CWorkQueueDto();
                                SwipeCardInfoTableNew.devicecode = struTelegram.nEquipID;
                                SwipeCardInfoTableNew.warehouse = struTelegram.nPLCSendFlag;
                                SwipeCardInfoTableNew.iccode = struTelegram.strICCardID;
                                SwipeCardInfoTableNew.swipecount = (int)EnmSwipeCount.Oversize;
                                requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTableNew, 1));
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理临时取物协议-1002
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealTmpFetchTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);

            switch (struTelegram.nSubTypePLC)
            {
                case 1:
                    {
                        nActionFlag = 3;
                        requestAction.telegram = "2-2";// 报文类型2 （2），报文子类型EMS 3（2）
                        requestAction.soundName = "11.wav";// 请取物11.wav
                     
                        if (null != deviceStatus)
                        {// 车厅作业流程节点类型赋值
                            deviceStatus.prevnode = deviceStatus.currentnode;
                            deviceStatus.currentnode = (int)EnmFlowNodeDescp.TmpFetchHasCar;
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                        }
                        break;
                    }
                case 4://临时取物离开
                    {
                        // 清空当前卡号存取车排队信息
                        ClearWorkQueueSwipe(ref requestAction, struTelegram.strICCardID);

                        if (null != deviceStatus && deviceStatus.currentnode != (int)EnmFlowNodeDescp.TmpFetchHasCar)
                        {
                            break;
                        }

                        nActionFlag = 3;
                        requestAction.telegram = "2-55";// 报文类型2 （2），报文子类型EMS 3（55）	
                        requestAction.soundName = "02.wav"; // 请慢走，欢迎下次光临02.wav
                        //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealTmpFetchTelegram", "请慢走，欢迎下次光临02.wav");// trace

                        // 车厅设备
                        if (null != deviceStatus)
                        {
                            deviceStatus.iccode = null;
                            deviceStatus.tasktype = (int)EnmTaskType.Init;
                            // 车厅作业流程节点类型清空
                            deviceStatus.prevnode = null;
                            deviceStatus.currentnode = null;
                            deviceStatus.queueprevnode = null;
                            deviceStatus.queuecurrentnode = null;                          
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                        }

                        // 临时取物（源地址为车位地址）
                        CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr);
                        // 更新车位信息carlocation 为空
                        InitCarPOSN(ref requestAction, carLocation);
                        break;
                    }
                case 54:
                    {
                        nActionFlag = 1;
                        // 车厅设备
                        if (null != deviceStatus && (int)EnmFlowNodeDescp.TmpFetchCommandAccept == deviceStatus.currentnode)
                        {
                            break;
                        }
                        requestAction.telegram = "2-54";

                        // 车厅设备
                        if (null != deviceStatus)
                        {
                            deviceStatus.iccode = struTelegram.strICCardID;
                            deviceStatus.tasktype = (int)EnmTaskType.TmpFetch;
                            // 车厅作业流程节点类型赋值
                            deviceStatus.prevnode = deviceStatus.currentnode;
                            deviceStatus.currentnode = (int)EnmFlowNodeDescp.TmpFetchCommandAccept;
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                        }

                        // 临时取物（源地址为车位地址）
                        CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr);
                        // 更新车位信息carlocation 为“临时取物”
                        if (null != carLocation && carLocation.carloctype == (int)EnmLocationType.Normal)
                        {
                            carLocation.carlocstatus = (int)EnmLocationStatus.TmpFetch;
                            requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理出库协议-1003
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealExitTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);

            switch (struTelegram.nSubTypePLC)
            {
                case 1:
                    {
                        AllowGetCar(out nActionFlag, ref requestAction, struTelegram, deviceStatus);
                        break;
                    }
                case 4:
                    {
                        ExitCarLeave(out nActionFlag, ref requestAction, struTelegram, deviceStatus);
                        break;
                    }
                case 54:
                    {
                        ExitCommandAccept(out nActionFlag, ref requestAction, struTelegram, deviceStatus);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理ETV/TV装载协议(1013,1)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealLoadTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            if (1 != struTelegram.nSubTypePLC)
            {
                return true;
            }

            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);
            CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.strICCardID);

            if (null == carLocation || null == deviceStatus)
            {
                return true;
            }
            if (deviceStatus.iccode == struTelegram.strICCardID &&
                deviceStatus.currentnode == (int)EnmFlowNodeDescp.DealLoad)
            {
                return true;
            }

            //取车时(目的地址为车厅地址)
            CDeviceStatusDto deviceStatusHallDest = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr, EnmSMGType.Hall);

            nActionFlag = 1;
            // 报文类型2 （13），报文子类型EMS 3（51）
            // 报文类型2 （14），报文子类型EMS 3（1）
            struTelegram struT = requestAction.structTelegram;
            struT.nDestDirection = requestAction.structTelegram.nDirection;
            requestAction.telegram = string.Format("13-51,14-1-{0}", CConfigManagement.myPara.EquipIsAviliableID);
            //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealLoadTelegram", string.Format("发送报文：{0}，设备ID：{1}，IC卡号：{2}", requestAction.telegram, struTelegram.nEquipID, struTelegram.strICCardID));// trace

            // ETV设备
            deviceStatus.iccode = struTelegram.strICCardID;
            deviceStatus.currentnode = (int)EnmFlowNodeDescp.DealLoad;

            switch ((EnmLocationStatus)carLocation.carlocstatus)
            {
                case EnmLocationStatus.Entering:
                    {// 存车
                        deviceStatus.tasktype = (int)EnmTaskType.EntryTask;
                        #region 增加再次复核尺寸功能 add by wanxiaona20151210
                        // 复核的尺寸小于等于已分配车位的尺寸
                        if (CBaseMethods.MyBase.CheckCarSize(struTelegram.strCarSize, carLocation.carlocsize))
                        {
                            carLocation.carwheelbase = struTelegram.nWheelbase;
                            requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
                        }
                        else
                        {
                            CLOGException.Trace(struTelegram.nPLCSendFlag, "DealLoadTelegram", string.Format("复核尺寸功能原分配车位地址：{0}，复核尺寸：{1}", carLocation.carlocaddr, struTelegram.strCarSize));
                            // 重新分配车位
                            // 更新过去分配的车位信息为空
                            InitCarPOSN(ref requestAction, carLocation);
                            // 分配车位算法
                            string strError = string.Empty;
                            CCarLocationDto carLocationRe = ALLOCCarLocation(struTelegram, ref strError);
                             // 没找到合适的车位
                            if (null == carLocationRe || !CBaseMethods.MyBase.CheckCarSize(struTelegram.strCarSize, carLocationRe.carlocsize))
                            {
                                // 退回源车厅-发送14-1源地址为车厅，3-1车厅报文
                                struT.strDestLocAddr = struTelegram.strSrcLocAddr;
                            }
                            else
                            {
                                struT.strDestLocAddr = carLocationRe.carlocaddr;
                                carLocationRe.carlocstatus = (int)EnmLocationStatus.Entering;// 正在入库
                                carLocationRe.iccode = struTelegram.strICCardID;
                                carLocationRe.carwheelbase = struTelegram.nWheelbase;
                                carLocationRe.direction = struTelegram.nDirection;
                                carLocationRe.carsize = struTelegram.strCarSize;
                                carLocationRe.carintime = DateTime.Now;
                                requestAction.lstTableDto.Add(new struTableDto(carLocationRe, 2));
                            }
                        }
                        #endregion
                        break;
                    }
                case EnmLocationStatus.Outing:
                    {// 取车
                        deviceStatus.tasktype = (int)EnmTaskType.ExitTask;
                        break;
                    }
                case EnmLocationStatus.TmpFetch:
                    {// 临时取物
                        deviceStatus.tasktype = (int)EnmTaskType.TmpFetch;
                        break;
                    }
                case EnmLocationStatus.MovingVEH:
                    {// 库内挪移
                        deviceStatus.tasktype = (int)EnmTaskType.MoveCarTask;
                        break;
                    }
                case EnmLocationStatus.VehRotation:
                    {// 车辆旋转
                        deviceStatus.tasktype = (int)EnmTaskType.VehRotationTask;
                        break;
                    }
                case EnmLocationStatus.Occupy:
                    {
                        //取车时(目的地址为车厅地址)
                        //连续取车时，取第二辆车时先发13-1报文
                        if (null != deviceStatusHallDest)
                        {
                            deviceStatus.tasktype = (int)EnmTaskType.ExitTask;
                            carLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                            requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
                        }

                        break;
                    }
            }

            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
            requestAction.structTelegram = struT;

            // 存车时(源地址为车厅地址)
            CDeviceStatusDto deviceStatusHall = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr, EnmSMGType.Hall);
            // 车厅设备(存车时)
            if (null != deviceStatusHall)
            {
                if (((int)EnmTaskType.EntryTask == deviceStatusHall.tasktype || (int)EnmTaskType.TmpFetch == deviceStatusHall.tasktype))
                {
                    deviceStatusHall.iccode = null;
                    deviceStatusHall.tasktype = (int)EnmTaskType.Init;
                    // 车厅作业流程节点类型赋值
                    deviceStatusHall.prevnode = deviceStatusHall.currentnode;
                    deviceStatusHall.currentnode = (int)EnmFlowNodeDescp.DealLoad;
                 
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusHall, 2));
                }
            }
            else
            {// 取车时(目的地址为车厅地址)
                //CDeviceStatusDto deviceStatusHallDest = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr, EnmSMGType.Hall);
                if (null != deviceStatusHallDest)
                {
                    if (deviceStatusHallDest.iccode != struTelegram.strICCardID)
                    {// 车厅作业流程节点类型赋值(取车排队)
                        deviceStatusHallDest.queueprevnode = deviceStatusHallDest.queuecurrentnode;
                        deviceStatusHallDest.queuecurrentnode = (int)EnmFlowNodeDescp.DealLoad;
                    }
                    else
                    {// 车厅作业流程节点类型赋值
                        deviceStatusHallDest.prevnode = deviceStatusHallDest.currentnode;
                        deviceStatusHallDest.currentnode = (int)EnmFlowNodeDescp.DealLoad;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusHallDest, 2));
                }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理ETV/TV卸载协议(1014,1)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public bool DealUnloadTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            if (1 != struTelegram.nSubTypePLC)
            {
                return true;
            }

            // 清空当前卡号存取车排队信息
            ClearWorkQueueSwipe(ref requestAction, struTelegram.strICCardID);
            nActionFlag = 1;
            requestAction.telegram = "14-51";// 报文类型2 （14），报文子类型EMS 3（51）
            //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealUnloadTelegram", requestAction.telegram);// trace

            CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);
            // ETV设备
            if (null == deviceStatus)
            {
                return true;
            }
            // “源地址”
            CCarLocationDto carLocationSrc = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr);
            if (null == carLocationSrc)
            {
                if (deviceStatus.scrloc != null)
                {
                    carLocationSrc = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, deviceStatus.scrloc);
                }
            }
            // “目的地址”
            CCarLocationDto carLocationDest = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr);
            if (carLocationDest == null) 
            {
                if (deviceStatus.desccloc != null) 
                {
                    carLocationDest = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, deviceStatus.desccloc);
                }
            }

            switch ((EnmTaskType)deviceStatus.tasktype)
            {
                case EnmTaskType.EntryTask:
                    {// 存车---目的车位
                        if (null != carLocationDest)
                        {// 当前车位更新修改为“有车”
                            carLocationDest.carlocstatus = (int)EnmLocationStatus.Occupy;
                            requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));
                        }
                        break;
                    }
                case EnmTaskType.ExitTask:
                    {   
                        // 取车---源车位
                        // 更新当前车位信息为空
                        InitCarPOSN(ref requestAction, carLocationSrc);
                        break;
                    }
                case EnmTaskType.TmpFetch:
                    {// 临时取物---目的车位(存车)
                        if (null != carLocationDest)
                        {// 当前车位更新修改为“有车”
                            carLocationDest.carlocstatus = (int)EnmLocationStatus.Occupy;
                            requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));
                        }
                        break;
                    }
                case EnmTaskType.MoveCarTask:
                    {// 库内挪移---目的车位 = 源车位、 源车位 = null
                        if (null != carLocationSrc && null != carLocationDest)
                        {
                            carLocationDest.carlocstatus = (int)EnmLocationStatus.Occupy;
                            carLocationDest.iccode = carLocationSrc.iccode;
                            carLocationDest.carwheelbase = carLocationSrc.carwheelbase;                            
                            carLocationDest.carsize = carLocationSrc.carsize;

                            carLocationDest.overallLg = carLocationSrc.overallLg;
                            carLocationDest.overhang = carLocationSrc.overhang;
                            carLocationDest.offcenter = carLocationSrc.offcenter;
                            carLocationDest.rearwheeldis = carLocationSrc.rearwheeldis;
                            carLocationDest.carweight = carLocationSrc.carweight;

                            carLocationDest.carintime = carLocationSrc.carintime;
                            //carLocationDest.isbackup = carLocationSrc.isbackup;
                            //carLocationDest.carnumber = carLocationSrc.carnumber;
                            //carLocationDest.carpicture = carLocationSrc.carpicture;
                            requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));

                            // 更新源车位信息为空
                            InitCarPOSN(ref requestAction, carLocationSrc);
                        }
                        break;
                    }
            }

            //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealUnloadTelegram", "telegram:" + requestAction.telegram + ", iccode:" + deviceStatus.iccode
            //    + ", EquipID:" + deviceStatus.devicecode + ", tasktype" + deviceStatus.tasktype + ", isable" + deviceStatus.isable);// trace
            deviceStatus.iccode = null;
            deviceStatus.tasktype = (int)EnmTaskType.Init;          
            deviceStatus.prevnode = null;
            deviceStatus.currentnode = null;

            deviceStatus.scrloc = null;
            deviceStatus.desccloc = null;
           
            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
            //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealUnloadTelegram", "telegram:" + requestAction.telegram + ", iccode:" + deviceStatus.iccode
            //    + ", EquipID:" + deviceStatus.devicecode + ", tasktype:" + deviceStatus.tasktype + ", isable:" + deviceStatus.isable);// trace

            // 存车时(源地址为车厅地址)
            CDeviceStatusDto deviceStatusHallSrc = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr, EnmSMGType.Hall);
            // 车厅设备(存车时)
            if (null != deviceStatusHallSrc)
            { // 车厅作业流程节点类型清空
                deviceStatusHallSrc.prevnode = null;
                deviceStatusHallSrc.currentnode = null;
                deviceStatusHallSrc.queueprevnode = null;
                deviceStatusHallSrc.queuecurrentnode = null;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatusHallSrc, 2));
            }
            else
            {// 取车时(目的地址为车厅地址)
                CDeviceStatusDto deviceStatusHallDest = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr, EnmSMGType.Hall);
                if (null != deviceStatusHallDest)
                {
                    if (deviceStatusHallDest.iccode != struTelegram.strICCardID)
                    {// 车厅作业流程节点类型赋值(取车排队)
                        deviceStatusHallDest.queueprevnode = deviceStatusHallDest.queuecurrentnode;
                        deviceStatusHallDest.queuecurrentnode = (int)EnmFlowNodeDescp.DealUnload;
                    }
                    else
                    {// 车厅作业流程节点类型赋值
                        deviceStatusHallDest.prevnode = deviceStatusHallDest.currentnode;
                        deviceStatusHallDest.currentnode = (int)EnmFlowNodeDescp.DealUnload;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusHallDest, 2));
                }
            }

            if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            return true;
        }

        /// <summary>
        /// 处理状态命令协议(1074,7)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealStatusCommandTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            //CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);

            if (7 == struTelegram.nSubTypePLC && 0 == struTelegram.nSMGValidity)
            {// 改变设备状态为故障
                nActionFlag = 1;
                requestAction.telegram = "74-1";// 报文类型2 （74），报文子类型EMS 3（1）
                //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealStatusCommandTelegram", requestAction.telegram);// trace

                //CCommonMethods.myMethods.DeleteWorkQueueInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, struTelegram.strICCardID);

                // ETV设备
                //if (null != deviceStatus)
                //{
                //    deviceStatus.devicemode = (int)EnmModel.StandAlone;// AutoStopped3
                //    // 作业流程节点类型清空
                //    deviceStatus.iccode = null;
                //    deviceStatus.tasktype = (int)EnmTaskType.Init;
                //    deviceStatus.prevnode = null;
                //    deviceStatus.currentnode = null;
                //    deviceStatus.queueprevnode = null;
                //    deviceStatus.queuecurrentnode = null;
                //    requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                //    CCommonMethods.myMethods.DeleteWorkQueueInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, struTelegram.strICCardID);
                //}
            }

            //if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
            //{
            //    nActionFlag += 4;
            //}
            return true;
        }

        /// <summary>
        /// 处理车辆旋转(1012,1)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        public virtual bool DealVehicleRotationTelegram(ref int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram)
        {
            if (1 == struTelegram.nSubTypePLC)
            {// 改变设备状态为故障
                nActionFlag = 5;
                requestAction.telegram = "12-51";// 报文类型2 （12），报文子类型EMS 3（51）
                //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealVehicleRotationTelegram", requestAction.telegram);// trace
                // “目的地址”
                CCarLocationDto carLocationDest = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr);
                // 当前车位更新修改为“有车”、车头朝向
                carLocationDest.carlocstatus = (int)EnmLocationStatus.Occupy;
                carLocationDest.direction = struTelegram.nDirection;
                requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));
                CDeviceStatusDto deviceStatus = CCommonMethods.myMethods.GetDeviceStatus(struTelegram.nPLCSendFlag, struTelegram.nEquipID);

                // ETV设备
                if (null != deviceStatus)
                {
                    // 作业流程节点类型清空
                    deviceStatus.iccode = null;
                    deviceStatus.tasktype = (int)EnmTaskType.Init;
                    deviceStatus.prevnode = null;
                    deviceStatus.currentnode = null;
                    deviceStatus.queueprevnode = null;
                    deviceStatus.queuecurrentnode = null;
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                }
            }
            return true;
        }

        /// <summary>
        /// 入库时车辆状态判断并处理(1001,1)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="requestAction"></param>
        protected void EnterCarStatus(out int nActionFlag, ref CDeviceStatusDto deviceStatus, ref CRequestAction requestAction)
        {
            nActionFlag = 0;
            if (null == deviceStatus)
            {
                CLOGException.Trace("EnterCarStatus", "deviceStatus为空");//trace
                return;
            }

            // 判断车厅当前的工作方式是否是4-全自动(nAlarms-Hall)
            if ((int)EnmModel.Automatic != deviceStatus.devicemode)
            {
                nActionFlag = 2;
                requestAction.soundName = "01.wav";//系统已转为人工作业不能存取车辆01.wav 
                return;
            }
            if (!CBaseMethods.MyBase.IsEmpty(deviceStatus.tasktype) && !string.IsNullOrWhiteSpace(deviceStatus.iccode) && (int)EnmFlowNodeDescp.EnterSwipeOne != deviceStatus.currentnode)
            {
                nActionFlag = 2;
                requestAction.soundName = "39.wav";//有故障作业未处理，请联系管理员处理39.wav
                //CLOGException.Trace(deviceStatus.warehouse, "EnterCarStatus", "有故障作业未处理，请联系管理员处理39.wav");// trace
                return;
            }
            if ((int)EnmHallType.Exit == deviceStatus.halltype)
            {
                nActionFlag = 2;
                requestAction.soundName = "07.wav";// 出车厅不允许进车07.wav
                return;
            }
            int warehouse = deviceStatus.warehouse;
            int freeCarNum = CCommonMethods.myMethods.GetFreeCarPOSN(warehouse);
            if (freeCarNum == 0) 
            {
                nActionFlag = 2;
                requestAction.soundName = "60.wav";               
            }
            struTelegram tele = requestAction.structTelegram;
            string carSize = tele.strCarSize;
            if (!string.IsNullOrEmpty(carSize))
            {
                if (carSize == CConfigManagement.myPara.CarMaxSize)
                {
                    int freeBigCar = CCommonMethods.myMethods.GetFreeBigCarLoc(warehouse);
                    if (freeBigCar == 0)
                    {
                        nActionFlag = 2;
                        requestAction.soundName = "61.wav";                        
                    }
                }
            }

            if ((int)EnmHallType.Entance == deviceStatus.halltype || (int)EnmHallType.EnterOrExit == deviceStatus.halltype) // 车厅是进车厅或者车厅是进出共用车厅 --	判断车厅是否是进车厅并且占用否
            {
                nActionFlag = 2;
                requestAction.soundName = GetEnterCarSoundName(deviceStatus);//请拉好手刹，关好车门，收起后视镜和天线，贵重物品请带走，刷卡存车18.wav
                if ((int)EnmFlowNodeDescp.EnterSwipeOne != deviceStatus.currentnode)
                {
                    deviceStatus.iccode = null;
                    // 清空当前卡号存取车排队信息
                    ClearWorkQueueSwipe(ref requestAction, deviceStatus.iccode);
                }

                deviceStatus.queueprevnode = null;
                deviceStatus.queuecurrentnode = null;
                deviceStatus.tasktype = (int)EnmTaskType.EntryTask;// 设置该车厅为存车状态2

                if ((int)EnmFlowNodeDescp.EnterSwipeOne != deviceStatus.currentnode && (int)EnmFlowNodeDescp.EnterSwipeTwo != deviceStatus.currentnode)
                {  // 车厅作业流程节点类型赋值
                    deviceStatus.prevnode = (int)EnmFlowNodeDescp.EnterHasCar;
                    deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterHasCar;
                }

                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

            }
            
        }

        /// <summary>
        /// 入库时检测车辆处理(1001,101)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        /// <param name="deviceStatus"></param>
        protected virtual void EnterCarChecked(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus)
        {
            struTelegram struTelegramTemp = requestAction.structTelegram;
            nActionFlag = 0;
            string strCarSize = struTelegram.strCarSize;// 车辆尺寸
            // 清空当前卡号存取车排队信息
            ClearWorkQueueSwipe(ref requestAction, struTelegram.strICCardID);
            // 清空当前车厅存车刷一次卡信息
            ClearWorkQueueSwipe(ref requestAction);

            if (null == deviceStatus)
            {
                CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "deviceStatus为空");
                return;

            }

            if (deviceStatus.iccode == struTelegram.strICCardID && deviceStatus.currentnode == (int)EnmFlowNodeDescp.EnterCarCheck)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(strCarSize) || 3 != strCarSize.Length || !CBaseMethods.MyBase.IsUIntNumWithoutZero(strCarSize))
            {
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                requestAction.soundName = "30.wav";// 无效车辆尺寸，请退出30.wav
                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "无效车辆尺寸，请退出30.wav");
                return;
            }

            //判断允许存入最大车辆尺寸参数的有效性
            if (string.IsNullOrWhiteSpace(CConfigManagement.myPara.CarMaxSize))
            {
                nActionFlag = 2;
                requestAction.soundName = "31.wav";// 允许存入最大车辆尺寸参数配置错误，请联系管理员处理31.wav
                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "允许存入最大车辆尺寸参数配置错误，请联系管理员处理31.wav");// trace
                return;
            }

            // 判断车辆尺寸是否超长 "122"为车辆最大尺寸
            if (false == CBaseMethods.MyBase.CheckCarSize(strCarSize, CConfigManagement.myPara.CarMaxSize))
            {
                nActionFlag = 3;
                requestAction.telegram = "1-2"; //string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "04.wav";// 车体超限，请退出04.wav

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.Oversize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.Oversize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }

                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "车体超限，请退出04.wav");// trace
                return;
            }

            // “目的地址”---车位
            CCarLocationDto carLocationDest = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strDestLocAddr);

            if (((int)EnmTaskType.TmpFetch == deviceStatus.tasktype || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struTelegram.nPLCSendFlag)
                            && CConfigManagement.myPara.DicCIMCTmpFetch[struTelegram.nPLCSendFlag]))
                && null != carLocationDest)//!string.IsNullOrWhiteSpace(struTelegramTemp.strDestLocAddr))
            {
                // 临时取物处理
                nActionFlag = 3;
                // 【报文类型2 （1），报文子类型EMS 3（1）】
                requestAction.telegram = "1-1";
                requestAction.soundName = "26.wav";// 已接受此次存车,请慢走26.wav
                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "已接受此次存车请慢走26.wav");// trace
                if (carLocationDest.carloctype == (int)EnmLocationType.Normal)
                {// 当前车位更新修改为“正在入库”
                    carLocationDest.carlocstatus = (int)EnmLocationStatus.Entering;
                    requestAction.lstTableDto.Add(new struTableDto(carLocationDest, 2));
                }

                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                return;
            }

            // 分配车位算法
            string strError = string.Empty;
            CCarLocationDto carLocation = ALLOCCarLocation(struTelegram, ref strError);

            // 没找到合适的车位
            if (null == carLocation)
            {
                if (!string.IsNullOrWhiteSpace(strError))
                {
                    nActionFlag = 2;
                    requestAction.soundName = strError;// 该卡异常，请联系管理人员处理32.wav
                    CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "该卡异常，请联系管理人员处理32.wav");// trace
                    return;
                }

                nActionFlag = 3;
                requestAction.telegram = "1-2";// string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "27.wav";// 已无合适车位，请退出27.wav
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.NoSuitableSize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.NoSuitableSize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }

                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "已无空余车位，请退出27.wav");// trace
                return;
            }

            if (string.IsNullOrWhiteSpace(carLocation.carlocsize))
            {
                CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "行：carLocation.carlocsize为空");
                return;
            }

            // 判断车辆尺寸是否小于车位尺寸
            if (false == CBaseMethods.MyBase.CheckCarSize(strCarSize, carLocation.carlocsize))
            {
                nActionFlag = 3;
                requestAction.telegram = "1-2";//string.Format("1-2-{0}", CConfigManagement.myPara.EquipIsAviliableID);
                // 目的地址为车厅地址
                struTelegramTemp.strDestLocAddr = deviceStatus.deviceaddr;
                requestAction.structTelegram = struTelegramTemp;
                requestAction.soundName = "27.wav";//已无合适车位，请退出27.wav"34.wav";// 车辆尺寸超出固定车位尺寸，请退出34.wav
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struTelegram.nPLCSendFlag, struTelegram.nEquipID, (int)EnmSwipeCount.NoSuitableSize);
                if (null == SwipeCardInfoTable)
                {
                    SwipeCardInfoTable = new CWorkQueueDto();
                    SwipeCardInfoTable.devicecode = struTelegram.nEquipID;
                    SwipeCardInfoTable.warehouse = struTelegram.nPLCSendFlag;
                    SwipeCardInfoTable.iccode = struTelegram.strICCardID;
                    SwipeCardInfoTable.swipecount = (int)EnmSwipeCount.NoSuitableSize;
                    requestAction.lstTableDto.Add(new struTableDto(SwipeCardInfoTable, 1));
                }

                //CLOGException.Trace(struTelegram.nPLCSendFlag, "EnterCarChecked", "车辆尺寸超出固定车位尺寸，请退出34.wav");// trace
                return;
            }

            nActionFlag = 7;
            // 【报文类型2 （1），报文子类型EMS 3（1）】
            requestAction.telegram = "1-1";
            // 源地址为车厅地址
            struTelegramTemp.strSrcLocAddr = deviceStatus.deviceaddr;
            // 目的地址为车位地址
            struTelegramTemp.strDestLocAddr = carLocation.carlocaddr;
            requestAction.structTelegram = struTelegramTemp;
            requestAction.soundName = "26.wav";// 已接受此次存车,请慢走26.wav
            //CLOGException.Trace(struTelegram.nPLCSendFlag, "DealCheckedCar", "已接受此次存车,请慢走26.wav");// trace
            carLocation.carlocstatus = (int)EnmLocationStatus.Entering;// 正在入库
            carLocation.iccode = struTelegram.strICCardID;
            carLocation.carwheelbase = struTelegram.nWheelbase;
            carLocation.direction = struTelegram.nDirection;
            carLocation.carsize = struTelegram.strCarSize;
            carLocation.carintime = DateTime.Now;
           
            requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
            // 车厅作业流程节点类型赋值
            deviceStatus.tasktype = (int)EnmTaskType.EntryTask;// add by wanxiaona20150729
            deviceStatus.prevnode = deviceStatus.currentnode;
            deviceStatus.currentnode = (int)EnmFlowNodeDescp.EnterCarCheck;
            requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
        }

        /// <summary>
        /// 车厅确认出车(1003,54)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        protected virtual void ExitCommandAccept(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus)
        {
            nActionFlag = 1;
            requestAction.telegram = "3-54";
            if (deviceStatus.iccode == struTelegram.strICCardID &&
                deviceStatus.currentnode == (int)EnmFlowNodeDescp.ExitCommandAccept)
            {
                return;
            }

            // 车厅设备
            if (null != deviceStatus)
            {
                deviceStatus.iccode = struTelegram.strICCardID;
                deviceStatus.tasktype = (int)EnmTaskType.ExitTask;
                // 车厅作业流程节点类型赋值
                deviceStatus.prevnode = deviceStatus.currentnode;
                deviceStatus.currentnode = (int)EnmFlowNodeDescp.ExitCommandAccept;
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
            }

            // 取车（源地址为车位地址）
            CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.nPLCSendFlag, struTelegram.strSrcLocAddr);
            // 更新车位信息carlocation 为“正在出库”
            if (null != carLocation)
            {
                carLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                requestAction.lstTableDto.Add(new struTableDto(carLocation, 2));
            }

            if (4 != (nActionFlag & 4)) 
            {
                nActionFlag += 4;
            }
        }

        /// <summary>
        /// 卡A允许取车(1003,4)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        protected virtual void ExitCarLeave(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus)
        { 
            nActionFlag = 0;
            requestAction.telegram = "3-55";// 报文类型2 （3），报文子类型EMS 3（55）
	
            requestAction.soundName = "02.wav"; // 请慢走，欢迎下次光临02.wav

            if (1 != (nActionFlag & 1))
            {
                nActionFlag += 1;
            }
            if (2 != (nActionFlag & 2))
            {
                nActionFlag += 2;
            }
            // 车厅设备
            if (null != deviceStatus)
            {
                // 清空当前卡号存取车排队信息
                ClearWorkQueueSwipe(ref requestAction, deviceStatus.iccode);

                deviceStatus.iccode = null;
                deviceStatus.tasktype = (int)EnmTaskType.Init;
                // 车厅作业流程节点类型清空
                deviceStatus.prevnode = null;
                deviceStatus.currentnode = null;
                deviceStatus.queueprevnode = null;
                deviceStatus.queuecurrentnode = null;
              
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));

                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
            }
        }

        /// <summary>
        /// (1003,1) 允许取车
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTele"></param>
        /// <param name="deviceStatus"></param>
        protected void AllowGetCar(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTele,CDeviceStatusDto deviceStatus)
        {
            nActionFlag = 0;  
         
            requestAction.telegram = "3-2";// 报文类型2 （3），报文子类型EMS 3（2）

            requestAction.soundName = "05.wav";// 请取车05.wav

            if (1 != (nActionFlag & 1))
            {
                nActionFlag += 1;
            }
            if (2 != (nActionFlag & 2))
            {
                nActionFlag += 2;
            }

            // 车厅设备
            if (null != deviceStatus)
            {
                if (deviceStatus.iccode != struTele.strICCardID)
                {// 车厅作业流程节点类型赋值(取车排队)
                    deviceStatus.queueprevnode = deviceStatus.queuecurrentnode;
                    deviceStatus.queuecurrentnode = (int)EnmFlowNodeDescp.ExitHasCar;
                }
                else
                {// 车厅作业流程节点类型赋值
                    deviceStatus.prevnode = deviceStatus.currentnode;
                    deviceStatus.currentnode = (int)EnmFlowNodeDescp.ExitHasCar;
                }
                requestAction.lstTableDto.Add(new struTableDto(deviceStatus, 2));
                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
            }
        }

        #endregion 报文处理函数

        #region 刷卡处理函数
        /// <summary>
        /// 检查IC卡和车厅状态
        /// 成功：true
        /// 失败：false
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="ICCardTable"></param>
        /// <param name="HallStatusTable"></param>
        /// <returns></returns>
        public virtual bool CheckInfoStatus(ref int nActionFlag, ref CRequestAction requestAction, CICCardDto ICCardTable, CDeviceStatusDto HallStatusTable)
        {
            bool bETVStat = CheckETVStat(HallStatusTable.warehouse, HallStatusTable.devicecode);
            //车厅工作模式不是全自动模式05.wav
            if ((int)EnmModel.Automatic != HallStatusTable.devicemode || !bETVStat)
            {
                requestAction.soundName = "01.wav";//系统已转为人工作业不能存取车辆01.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                //CLOGException.Trace(HallStatusTable.warehouse, "CheckInfoStatus", string.Format("系统已转为人工作业不能存取车辆01.wav，devicemode = {0}", HallStatusTable.devicemode));
                return false;
            }

            //未制卡，不是本系统卡
            if (null == ICCardTable)
            {
                requestAction.soundName = "10.wav";//该卡不是本系统用卡10.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                CLOGException.Trace(HallStatusTable.warehouse, "CheckInfoStatus", "该卡不是本系统用卡10.wav");
                return false;
            }

            //注销或挂失06.wav
            if ((int)EnmICCardStatus.Lost == ICCardTable.icstatus || (int)EnmICCardStatus.Disposed == ICCardTable.icstatus)
            {
                requestAction.soundName = "06.wav";//本卡已经挂失或注销，不能存取车辆06.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                //CLOGException.Trace(HallStatusTable.warehouse, "CheckInfoStatus", string.Format("本卡已经挂失或注销，不能存取车辆06.wav,IC卡卡号 = {0}，icstatus = {1}", ICCardTable.iccode, ICCardTable.icstatus));
                return false;
            }

            //IC卡类型不正确
            if ((int)EnmICCardType.Temp != ICCardTable.ictype && (int)EnmICCardType.FixedLocation != ICCardTable.ictype
                && (int)EnmICCardType.Fixed != ICCardTable.ictype)
            {
                requestAction.soundName = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                //CLOGException.Trace(HallStatusTable.warehouse, "CheckInfoStatus", string.Format("该卡异常，请联系管理人员处理32.wav,IC卡卡号 = {0}，ictype = {1}", ICCardTable.iccode, ICCardTable.ictype));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理存车刷卡
        /// 返回值：
        /// 有动作需要处理：true
        /// 无动作需要处理：false
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="ICCardTable"></param>
        /// <param name="HallStatusTable"></param>
        /// <param name="CarLocation"></param>
        /// <param name="struHallEquip"></param>
        /// <returns></returns>
        public virtual bool DealParkingSwipe(ref int nActionFlag, ref CRequestAction requestAction, CICCardDto ICCardTable, CDeviceStatusDto HallStatusTable,
            CCarLocationDto CarLocation, struHallEquips struHallEquip)
        {
            //根据库区号、车厅号和IC卡卡号获取刷卡信息
            CWorkQueueDto workQueueTable = CCommonMethods.myMethods.GetSwipeCardInfo(struHallEquip.nWareHouseID, struHallEquip.nHallID, ICCardTable.iccode);

            if (null == workQueueTable)
            {
                // 存车
                // 临时取物
                // 中集天达新厂塔库临时取物后直接刷卡存车需求
                if (string.IsNullOrWhiteSpace(HallStatusTable.iccode)
                    || ((int)EnmHallType.EnterOrExit == HallStatusTable.halltype
                         && ICCardTable.iccode == HallStatusTable.iccode
                         && (((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype)
                              || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struHallEquip.nWareHouseID) && CConfigManagement.myPara.DicCIMCTmpFetch[struHallEquip.nWareHouseID]))))
                {                  
                    requestAction.soundName = "09.wav";//确认安全后再次刷卡09.wav(第一次刷卡)

                    HallStatusTable.iccode = ICCardTable.iccode;
                    HallStatusTable.prevnode = HallStatusTable.currentnode;
                    HallStatusTable.currentnode = (int)EnmFlowNodeDescp.EnterSwipeOne;
                    requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));

                    //修改数据库，增加第一次存车刷卡到workqueue表中
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();
                    getCarQueue.devicecode = struHallEquip.nHallID;
                    getCarQueue.warehouse = struHallEquip.nWareHouseID;
                    getCarQueue.iccode = ICCardTable.iccode;
                    getCarQueue.swipecount = (int)EnmSwipeCount.SaveCarFirst;
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                    nActionFlag = 6;

                    return true;
                }
                else
                {
                    if (ICCardTable.iccode == HallStatusTable.iccode)
                    {
                        requestAction.soundName = "17.wav";// 您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡), Hall ID:" + struHallEquip.nHallID);
                        return true;
                    }
                    else
                    {
                        //所有错误的存车卡：与第一次刷卡不一致，只语音提示
                        requestAction.soundName = "24.wav";// 与第一次刷卡卡号不一致，请重新刷卡24.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "与第一次刷卡卡号不一致，请重新刷卡24.wav, Hall ID:" + struHallEquip.nHallID);
                        return true;
                    }
                }
            }
            else
            {
                if ((int)EnmSwipeCount.SaveCarFirst == workQueueTable.swipecount)
                {
                    // 车厅作业流程节点类型赋值
                    HallStatusTable.iccode = ICCardTable.iccode;
                    HallStatusTable.prevnode = HallStatusTable.currentnode;
                    HallStatusTable.currentnode = (int)EnmFlowNodeDescp.EnterSwipeTwo;
                    requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));

                    //第二次刷卡 发报文
                    struTelegram struTelegram = new struTelegram();
                    requestAction.telegram = "1-9";
                    struTelegram.nPLCSendFlag = struHallEquip.nWareHouseID;
                    struTelegram.nEquipID = HallStatusTable.devicecode;
                    struTelegram.strICCardID = ICCardTable.iccode;
                    struTelegram.nEndFlag = 9999;

                    if ((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(struHallEquip.nWareHouseID)
                        && CConfigManagement.myPara.DicCIMCTmpFetch[struHallEquip.nWareHouseID]))
                    {
                        struTelegram.strSrcLocAddr = HallStatusTable.deviceaddr;
                        if (null != CarLocation)
                        {                            
                            struTelegram.nWheelbase = (int)CarLocation.carwheelbase;                            
                            struTelegram.strCarSize = CarLocation.carsize;
                            struTelegram.overallLg = (int)CarLocation.overallLg;
                            struTelegram.overHang = (int)CarLocation.overhang;

                            struTelegram.strSrcLocAddr = HallStatusTable.deviceaddr;
                            struTelegram.strDestLocAddr = CarLocation.carlocaddr;
                            //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "临时取物作业:" + HallStatusTable.tasktype + ", 取物的车位地址：" + CarLocation.carlocaddr);
                        }
                        else
                        {
                            CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "临时取物转存时获取的车辆信息为空:");
                        }
                    }
                    requestAction.structTelegram = struTelegram;
                    requestAction.soundName = "21.wav";// 请稍候，听到提示再离开21.wav(第二次刷卡)
                    workQueueTable.swipecount = (int)EnmSwipeCount.SaveCarSecond;
                    requestAction.lstTableDto.Add(new struTableDto(workQueueTable, 2));// 修改刷卡次数为2
                    nActionFlag = 7;
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "请稍候，听到提示再离开21.wav(第二次刷卡), Hall ID:" + struHallEquip.nHallID);
                    return true;
                }
                else if ((int)EnmSwipeCount.SaveCarSecond == workQueueTable.swipecount)
                {
                    //刷卡第三次或多次，语音提示。
                    requestAction.soundName = "17.wav";//您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡), Hall ID:" + struHallEquip.nHallID);
                    return true;
                }
                else if ((int)EnmSwipeCount.Oversize == workQueueTable.swipecount)
                {
                    //提示对不起，车体超限，请退出
                    requestAction.soundName = "04.wav";// 车体超限，请退出04.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "车体超限，请退出(第3次或更多次刷卡), Hall ID:" + struHallEquip.nHallID);
                    return true;
                }
                else if ((int)EnmSwipeCount.NoSuitableSize == workQueueTable.swipecount)
                {
                    // 已无合适车位，请退出27.wav
                    requestAction.soundName = "27.wav";// 已无合适车位，请退出27.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "已无合适车位，请退出27.wav(第3次或更多次刷卡), Hall ID:" + struHallEquip.nHallID);
                    return true;
                }
                else
                {
                    //异常，实际上不存在
                    //提示控制系统故障
                    requestAction.soundName = "20.wav";//上位控制系统故障20.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    CLOGException.Trace(struHallEquip.nWareHouseID, "DealParkingSwipe", "上位控制系统故障20.wav，数据库表workQueueTable中的刷卡次数异常, Hall ID:" + struHallEquip.nHallID);
                    return true;
                }
            }
        }

        /// <summary>
        /// 处理取车刷卡
        /// 返回值：
        /// 有动作需要处理：true
        /// 无动作需要处理：false
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="ICCardTable"></param>
        /// <param name="HallStatusTable"></param>
        /// <param name="CarLocation"></param>
        /// <param name="struHallEquip"></param>
        /// <returns></returns>
        public virtual bool DealGetVEHSwipe(ref int nActionFlag, ref CRequestAction requestAction, CICCardDto ICCardTable, CDeviceStatusDto HallStatusTable,
            CCarLocationDto CarLocation, struHallEquips struHallEquip)
        {
            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetSwipeCardQueue(struHallEquip.nWareHouseID, struHallEquip.nHallID, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);
            
            CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(ICCardTable.iccode);
            if (null != deviceStatusTBL)
            {
                requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav              
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                return true;
            }
            //无取车排队
            if (0 == lstSwipeQueueOut.Count)
            {
                //车厅无作业
                if (CBaseMethods.MyBase.IsEmpty(HallStatusTable.tasktype))
                {
                    if (!string.IsNullOrWhiteSpace(HallStatusTable.iccode))
                    {
                        //有卡号的话就是系统异常
                        //清空当前设备在设备状态表中的IC卡卡号值，提示重新刷卡取车。
                        // 清空当前卡号存取车排队信息
                        ClearWorkQueueSwipe(ref requestAction, ICCardTable.iccode);
                        HallStatusTable.iccode = null;
                        requestAction.soundName = "12.wav";//上位控制系统异常,请重新刷卡取车12.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "上位控制系统异常,数据库表HallStatusTable中的iccode不为空, Hall ID:" + struHallEquip.nHallID);
                        return true;
                    }
                    //可接受指令
                    else if (1 == HallStatusTable.isavailable)
                    {
                        int nAllotEquipID = ALLOCETVOrTV(struHallEquip.nWareHouseID);
                        CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(struHallEquip.nWareHouseID, nAllotEquipID);
                        if (null != deviceStatusETV)
                        {
                            deviceStatusETV.tasktype = (int)EnmTaskType.ExitTask;
                            deviceStatusETV.iccode = ICCardTable.iccode;
                            // ETV/TV作业流程节点类型清空
                            deviceStatusETV.prevnode = null;
                            deviceStatusETV.currentnode = null;
                            deviceStatusETV.queueprevnode = null;
                            deviceStatusETV.queuecurrentnode = null;
                            if (4 != (nActionFlag & 4))
                            {
                                nActionFlag += 4;
                            }
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                        }

                        //直接发取车报文
                        requestAction.telegram = string.Format("3-1-{0},13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);
                        struTelegram struTelegram = new struTelegram();
                        struTelegram.nPLCSendFlag = struHallEquip.nWareHouseID;
                        struTelegram.nEquipID = HallStatusTable.devicecode;
                        struTelegram.strICCardID = ICCardTable.iccode;
                        struTelegram.strCarSize = CarLocation.carsize;
                        struTelegram.nWheelbase = Convert.ToInt32(CarLocation.carwheelbase);
                        struTelegram.nDirection = Convert.ToInt32(CarLocation.direction);
                        struTelegram.strSrcLocAddr = CarLocation.carlocaddr;
                        struTelegram.strDestLocAddr = HallStatusTable.deviceaddr;
                        struTelegram.nEndFlag = 9999;
                        requestAction.structTelegram = struTelegram;
                        requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                        HallStatusTable.iccode = ICCardTable.iccode;
                        HallStatusTable.tasktype = (int)EnmTaskType.ExitTask;
                        // 车厅作业流程节点类型赋值
                        HallStatusTable.prevnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        HallStatusTable.currentnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));
                        CarLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                        requestAction.lstTableDto.Add(new struTableDto(CarLocation, 2));
                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "正在为您出车，请稍后28.wav, Hall ID:" + struHallEquip.nHallID);
                        return true;
                    }
                    else
                    {
                        //加入取车排队队列
                        requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav
                        //修改数据库，增加取车刷卡到workqueue
                        CWorkQueueDto getCarQueue = new CWorkQueueDto();
                        getCarQueue.devicecode = struHallEquip.nHallID;
                        getCarQueue.warehouse = struHallEquip.nWareHouseID;
                        getCarQueue.iccode = ICCardTable.iccode;
                        getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                        getCarQueue.priorityid = ICCardTable.priorityid;
                        requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "已经将您加到取车队列，请排队等候出车19.wav, Hall ID:" + struHallEquip.nHallID + "无取车排队HallStatusTable.isavailable=" + HallStatusTable.isavailable);
                        return true;
                    }
                }
                else if ((int)EnmTaskType.EntryTask == HallStatusTable.tasktype || (int)EnmTaskType.ExitTask == HallStatusTable.tasktype || (int)EnmTaskType.TmpFetch == HallStatusTable.tasktype)    //车厅有作业
                {
                    int nAllotEquipID = ALLOCETVOrTV(struHallEquip.nWareHouseID);
                    //---add by wanxiaona20150728
                    int nAllotHallID = HallStatusTable.devicecode;
                    string strDestLocAddr = HallStatusTable.deviceaddr;
                    if (CConfigManagement.myPara.DicVEHExitQueueByWH.ContainsKey(struHallEquip.nWareHouseID)
                    && CConfigManagement.myPara.DicVEHExitQueueByWH[struHallEquip.nWareHouseID])
                    {
                        // 按库区排队取车
                        nAllotHallID = ALLOCHallByQueue(struHallEquip.nWareHouseID,(int)HallStatusTable.region,null);
                        CDeviceStatusDto HallTable = CCommonMethods.myMethods.GetDeviceStatus(struHallEquip.nWareHouseID, nAllotHallID);
                        if (null != HallTable)
                        {
                            strDestLocAddr = HallTable.deviceaddr;
                            HallStatusTable = HallTable;
                        }
                    }
                    //---add by wanxiaona20150728
                    if (HallStatusTable.iccode == ICCardTable.iccode)
                    {
                        requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "正在为您出车，请稍后28.wav, Hall ID:" + struHallEquip.nHallID);
                    } 
                    //可接受指令
                    else if (1 == HallStatusTable.isavailable && 0 != nAllotHallID)
                    {
                        CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(struHallEquip.nWareHouseID, nAllotEquipID);
                        if (null != deviceStatusETV)
                        {
                            deviceStatusETV.tasktype = (int)EnmTaskType.ExitTask;
                            deviceStatusETV.iccode = ICCardTable.iccode;
                            // ETV/TV作业流程节点类型清空
                            deviceStatusETV.prevnode = null;
                            deviceStatusETV.currentnode = null;
                            deviceStatusETV.queueprevnode = null;
                            deviceStatusETV.queuecurrentnode = null;
                            if (4 != (nActionFlag & 4))
                            {
                                nActionFlag += 4;
                            }
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                        }

                        //直接发取车报文
                        requestAction.telegram = string.Format("3-1-{0},13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);
                        struTelegram struTelegram = new struTelegram();
                        struTelegram.nPLCSendFlag = struHallEquip.nWareHouseID;
                        struTelegram.nEquipID = HallStatusTable.devicecode;
                        struTelegram.strICCardID = ICCardTable.iccode;
                        struTelegram.strCarSize = CarLocation.carsize;
                        struTelegram.nWheelbase = Convert.ToInt32(CarLocation.carwheelbase);
                        struTelegram.nDirection = Convert.ToInt32(CarLocation.direction);
                        struTelegram.strSrcLocAddr = CarLocation.carlocaddr;
                        struTelegram.strDestLocAddr = HallStatusTable.deviceaddr;
                        struTelegram.nEndFlag = 9999;
                        requestAction.structTelegram = struTelegram;
                        requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                        HallStatusTable.iccode = ICCardTable.iccode;
                        HallStatusTable.tasktype = (int)EnmTaskType.ExitTask;
                        // 车厅作业流程节点类型赋值
                        HallStatusTable.prevnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        HallStatusTable.currentnode = (int)EnmFlowNodeDescp.ExitSwipe;
                        requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));
                        CarLocation.carlocstatus = (int)EnmLocationStatus.Outing;
                        requestAction.lstTableDto.Add(new struTableDto(CarLocation, 2));
                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        if (0 < requestAction.lstTableDto.Count && 4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "正在为您出车，请稍后28.wav, 重新分配Hall ID:" + struHallEquip.nHallID);
                        return true;
                    }
                    //---modify by wanxiaona20150728
                    //else if ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype && 0 != nAllotEquipID)
                    else if ((int)EnmTaskType.ExitTask == HallStatusTable.tasktype && 0 != nAllotEquipID && 0 != nAllotHallID)
                    {
                        CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(struHallEquip.nWareHouseID, nAllotEquipID);
                        if (null != deviceStatusETV)
                        {
                            deviceStatusETV.tasktype = (int)EnmTaskType.ExitTask;
                            deviceStatusETV.iccode = ICCardTable.iccode;
                            // ETV/TV作业流程节点类型清空
                            deviceStatusETV.prevnode = null;
                            deviceStatusETV.currentnode = null;
                            deviceStatusETV.queueprevnode = null;
                            deviceStatusETV.queuecurrentnode = null;
                            if (4 != (nActionFlag & 4))
                            {
                                nActionFlag += 4;
                            }
                            requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                        }

                        //直接发取车报文
                        requestAction.telegram = string.Format("13-1-{0}-{1},3-1-{0}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);
                        struTelegram struTelegram = new struTelegram();
                        struTelegram.nPLCSendFlag = struHallEquip.nWareHouseID;
                        struTelegram.nEquipID = nAllotHallID;// HallStatusTable.devicecode;//---modify by wanxiaona20150728
                        struTelegram.strICCardID = ICCardTable.iccode;
                        struTelegram.strCarSize = CarLocation.carsize;
                        struTelegram.nWheelbase = Convert.ToInt32(CarLocation.carwheelbase);
                        struTelegram.nDirection = Convert.ToInt32(CarLocation.direction);
                        struTelegram.strSrcLocAddr = CarLocation.carlocaddr;
                        struTelegram.strDestLocAddr = strDestLocAddr;// HallStatusTable.deviceaddr;//---modify by wanxiaona20150728
                        struTelegram.nEndFlag = 9999;
                        requestAction.structTelegram = struTelegram;
                        requestAction.soundName = "28.wav";// 正在为您出车，请稍后28.wav
                        if (1 != (nActionFlag & 1))
                        {
                            nActionFlag += 1;
                        }

                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "正在为您出车，请稍后28.wav, Hall ID:" + struHallEquip.nHallID);
                        return true;

                    }
                    else
                    {
                        requestAction.soundName = "19.wav";//已经将您加到取车队列，请排队等候出车19.wav
                        //加入取车排队队列
                        //修改数据库，增加取车刷卡到workqueue
                        CWorkQueueDto getCarQueue = new CWorkQueueDto();
                        getCarQueue.devicecode = struHallEquip.nHallID;
                        getCarQueue.warehouse = struHallEquip.nWareHouseID;
                        getCarQueue.iccode = ICCardTable.iccode;
                        getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                        getCarQueue.priorityid = ICCardTable.priorityid;
                        requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                        if (4 != (nActionFlag & 4))
                        {
                            nActionFlag += 4;
                        }
                        //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "已经将您加到取车队列，请排队等候出车19.wav, Hall ID:" + struHallEquip.nHallID + "HallStatusTable.tasktype=" + HallStatusTable.tasktype + "nAllotEquipID=" + nAllotEquipID);
                    }
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    return true;
                }
                else
                {
                    requestAction.soundName = "20.wav";//上位控制系统故障20.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "上位控制系统故障20.wav，数据库中设备作业状态tasktype错误, Hall ID:" + struHallEquip.nHallID);
                    return true;
                }

            }
            else if (lstSwipeQueueOut.Count <= CConfigManagement.myPara.CarExitQueueMaxCount)
            //有取车排队
            {
                //根据库区号、车厅号和IC卡卡号获取刷卡信息
                CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(struHallEquip.nWareHouseID, struHallEquip.nHallID, ICCardTable.iccode);

                //---modify by wanxiaona20150728
                //if (null != SwipeCardInfoTable && (int)EnmSwipeCount.GetCar == SwipeCardInfoTable.swipecount)
                if (null != SwipeCardInfoTable && ((int)EnmSwipeCount.GetCar == SwipeCardInfoTable.swipecount || (int)EnmSwipeCount.TmpFetch == SwipeCardInfoTable.swipecount))
                {
                    requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "已经将您加到取车队列，请排队等候出车19.wav, Hall ID:" + struHallEquip.nHallID + "SwipeCardInfoTable.swipecount=" + SwipeCardInfoTable.swipecount);
                    return true;
                }
                else
                {
                    //加入取车队列
                    requestAction.soundName = "19.wav";// 已经将您加到取车队列，请排队等候出车19.wav
                    //修改数据库，增加取车刷卡到workqueue
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();
                    getCarQueue.devicecode = struHallEquip.nHallID;
                    getCarQueue.warehouse = struHallEquip.nWareHouseID;
                    getCarQueue.iccode = ICCardTable.iccode;
                    getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                    getCarQueue.priorityid = ICCardTable.priorityid;
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "已经将您加到取车队列，请排队等候出车19.wav, Hall ID:" + struHallEquip.nHallID);
                    return true;

                }
            }
            else
            {
                requestAction.soundName = "40.wav";// 取车排队的人数超过配置的阈值
                if (2 != (nActionFlag & 2))
                {
                    nActionFlag += 2;
                }
                //CLOGException.Trace(struHallEquip.nWareHouseID, "DealGetVEHSwipe", "当前车厅的取车排队人数已经达到阈值，请到其他车厅刷卡取车, Hall ID:" + struHallEquip.nHallID);
                return true;
            }
        }
        #endregion 刷卡处理函数

        #region 客户端请求处理函数
        /// <summary>
        /// 临时取物
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.Null;//传入的参数卡号为空
        /// EnmFaultType.NoICCardInfo;//没有制卡
        /// EnmFaultType.LossORCancel;//IC卡注销或挂失
        /// EnmFaultType.NotMatch;//IC卡类型不正确
        /// EnmFaultType.NoCarInGarage;       //当前卡没有车存在车库
        /// EnmFaultType.NotFoundEquip;       //没有找到指定设备
        /// EnmFaultType.NotAvailable;// 车厅不可接收指令
        /// EnmFaultType.IsNotHallEquip;//指定的设备不是车厅
        /// EnmFaultType.EquipNotCorrect;//车厅不是进出两用车厅
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAutomatic;//非全自动模式
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// </summary>
        /// <param name="deviceStatusTBL">车厅设备</param>
        /// <param name="carPOSNTable">车位对象</param>
        /// <returns></returns>
        public virtual EnmFaultType TmpFetch(CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable)
        {
            if (null == carPOSNTable || null == deviceStatusTBL || string.IsNullOrWhiteSpace(carPOSNTable.iccode))
            {
                return EnmFaultType.Fail;     
            }

            int nActionFlag = 0;
            string strICCardID = carPOSNTable.iccode;
            int nWareHouse = deviceStatusTBL.warehouse;
            int nHallID = deviceStatusTBL.devicecode;
            int nAllotHallID = nHallID;
            string strDestLocAddr = deviceStatusTBL.deviceaddr;
            if (!CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype) && CConfigManagement.myPara.DicVEHExitQueueByWH.ContainsKey(nWareHouse)
            && CConfigManagement.myPara.DicVEHExitQueueByWH[nWareHouse])
            {
                #region
                // 按库区排队取车
                //nAllotHallID = ALLOCHallByQueue(nWareHouse,(int)deviceStatusTBL.region);
                //if (0 == nAllotHallID)
                //{
                //    nAllotHallID = nHallID;
                //}
                //CDeviceStatusDto HallTable = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotHallID);
                //if (null != HallTable)
                //{
                //    strDestLocAddr = HallTable.deviceaddr;
                //    deviceStatusTBL = HallTable;
                //}
                #endregion
            }
            //---add by wanxiaona20150728
            CRequestAction requestAction = new CRequestAction();
            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.strICCardID = strICCardID;
            struSendTelegram.nPLCSendFlag = nWareHouse;
            struSendTelegram.nEquipID = nAllotHallID;

            struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;            
            struSendTelegram.strCarSize = carPOSNTable.carsize;
            struSendTelegram.overallLg = (int)carPOSNTable.overallLg;
            struSendTelegram.overHang = (int)carPOSNTable.overhang;

            struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
            struSendTelegram.strDestLocAddr = strDestLocAddr;// deviceStatusTBL.deviceaddr;//modify by wanxiaona20150730
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            requestAction.structTelegram = struSendTelegram;

            //允许取车：车厅可接受指令 && 车厅没有存取作业 && 取车队列为空
            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nHallID, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);
            bool bETVStat = CheckETVStat(nWareHouse, nHallID);
            int nAllotEquipID = ALLOCETVOrTV(nWareHouse);
            if (1 == deviceStatusTBL.isavailable && CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype) && bETVStat && 0 == lstSwipeQueueOut.Count)
            {
                nActionFlag += 1;
                CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotEquipID);
                if (null != deviceStatusETV)
                {
                    deviceStatusETV.tasktype = (int)EnmTaskType.TmpFetch;
                    deviceStatusETV.iccode = strICCardID;
                    // ETV/TV作业流程节点类型清空
                    deviceStatusETV.prevnode = null;
                    deviceStatusETV.currentnode = null;
                    deviceStatusETV.queueprevnode = null;
                    deviceStatusETV.queuecurrentnode = null;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                }

                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                requestAction.PLCServer = m_commModule;
                requestAction.telegram = string.Format("2-1-{0},13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);

                //修改数据库中设备作业状态
                deviceStatusTBL.iccode = strICCardID;
                deviceStatusTBL.tasktype = (int)EnmTaskType.TmpFetch;

                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
                requestAction.lstTableDto.Add(new struTableDto(deviceStatusTBL, 2));
                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }
            //---modify by wanxiaona20150728
            //else if (0 == lstSwipeQueueOut.Count && bETVStat && 0 != nAllotEquipID && (int)EnmTaskType.EntryTask != deviceStatusTBL.tasktype && (int)EnmTaskType.TmpFetch != deviceStatusTBL.tasktype)
            else if (0 == lstSwipeQueueOut.Count && bETVStat && 0 != nAllotEquipID && 0 != nAllotHallID && 0 == deviceStatusTBL.isavailable && (int)EnmTaskType.ExitTask == deviceStatusTBL.tasktype)
            {
                CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotEquipID);
                if (null != deviceStatusETV)
                {
                    deviceStatusETV.tasktype = (int)EnmTaskType.TmpFetch;
                    deviceStatusETV.iccode = strICCardID;
                    // ETV/TV作业流程节点类型清空
                    deviceStatusETV.prevnode = null;
                    deviceStatusETV.currentnode = null;
                    deviceStatusETV.queueprevnode = null;
                    deviceStatusETV.queuecurrentnode = null;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                }

                nActionFlag += 1;
                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                requestAction.PLCServer = m_commModule;
                requestAction.telegram = string.Format("13-1-{0}-{1},2-1-{0}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);

                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }
            else
            {
                EnmFaultType enmResult = AddWorkQueue(nWareHouse, nHallID, strICCardID, deviceStatusTBL);
                if (EnmFaultType.Add == enmResult)
                {
                    //修改数据库，增加取车刷卡到workqueue
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();

                    CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);
                    if (null != ICCardTable)
                    {
                        getCarQueue.priorityid = ICCardTable.priorityid;
                    }

                    getCarQueue.devicecode = nHallID;
                    getCarQueue.warehouse = nWareHouse;
                    getCarQueue.iccode = strICCardID;
                    getCarQueue.swipecount = (int)EnmSwipeCount.TmpFetch;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                    requestAction.RequestAction(nActionFlag);
                }

                return enmResult;
            }
        }

        /// <summary>
        /// 车辆出库
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundCarPOSN;    //指定的源或目的车位不存在
        /// EnmFaultType.NotFoundEquip;       //没有找到指定目的车厅
        /// EnmFaultType.IsNotHallEquip;      //指定的目的地址不是车厅
        /// EnmFaultType.HallEnter;         //车厅是进车厅不允许出车
        /// EnmFaultType.NoCarInGarage;       //源车位没有车
        /// EnmFaultType.NotAvailable;      //车厅设备不可接收指令
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.NotSameWareHouse;//刷卡库区与车所在库区不同
        /// EnmFaultType.FailToInsert;//插入数据库失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.Wait;//已经将您加到取车队列，请排队等候
        /// EnmFaultType.Add; //前面有人正在取车，已经将您加到取车队列，请排队等候
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.NotNormalCarPOSN;//源车位不是正常车位。
        /// </summary>
        /// <param name="strICCardID">IC卡号或者车牌号</param>
        /// <param name="deviceStatusTBL">车厅设备</param>
        /// <param name="carPOSNTable">车位对象</param>
        /// <param name="bApp">APP手机取车标志 true：手机取车；false：否</param>
        /// <returns></returns>
        public virtual EnmFaultType VehicleExit(string strICCardID, CDeviceStatusDto deviceStatusTBL, CCarLocationDto carPOSNTable,bool bApp)
        {
            if (null == carPOSNTable || null == deviceStatusTBL)
            {
                return EnmFaultType.Fail;
            }

            int nActionFlag = 0;
            int nWareHouse = deviceStatusTBL.warehouse;
            int nHallID = deviceStatusTBL.devicecode;
            int nAllotHallID = nHallID;
            string strDestLocAddr = deviceStatusTBL.deviceaddr;

            if (!CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype) &&
                 CConfigManagement.myPara.DicVEHExitQueueByWH.ContainsKey(nWareHouse) && 
                 CConfigManagement.myPara.DicVEHExitQueueByWH[nWareHouse])
            {
                // 按库区排队取车
                nAllotHallID = ALLOCHallByQueue(nWareHouse, 0, null);
                if (0 == nAllotHallID)
                {
                    nAllotHallID = nHallID;
                }
                CDeviceStatusDto HallTable = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotHallID);
                if (null != HallTable)
                {
                    strDestLocAddr = HallTable.deviceaddr;
                    deviceStatusTBL = HallTable;
                }
            }
            //---add by wanxiaona20150728
            CRequestAction requestAction = new CRequestAction();
            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.strICCardID = strICCardID;
            struSendTelegram.nPLCSendFlag = nWareHouse;
            struSendTelegram.nEquipID = nAllotHallID;// nHallID;//---modify by wanxiaona20150728
            struSendTelegram.nWheelbase = (int)carPOSNTable.carwheelbase;
            struSendTelegram.nDirection = (int)carPOSNTable.direction;
            struSendTelegram.strCarSize = carPOSNTable.carsize;
            struSendTelegram.strSrcLocAddr = carPOSNTable.carlocaddr;
            struSendTelegram.strDestLocAddr = strDestLocAddr;// deviceStatusTBLDest.deviceaddr;//---modify by wanxiaona20150728
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            requestAction.structTelegram = struSendTelegram;

            //允许取车：车厅可接受指令 && 车厅没有存取作业 && 取车队列为空
            List<CWorkQueueDto> lstSwipeQueueOut = CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, nHallID, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);
            bool bETVStat = CheckETVStat(nWareHouse, nHallID);
            int nAllotEquipID = ALLOCETVOrTV(nWareHouse);

            if (bETVStat && 0 == lstSwipeQueueOut.Count && 1 == deviceStatusTBL.isavailable && CBaseMethods.MyBase.IsEmpty(deviceStatusTBL.tasktype))
            {
                CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotEquipID);
                if (null != deviceStatusETV)
                {
                    deviceStatusETV.tasktype = (int)EnmTaskType.ExitTask;
                    deviceStatusETV.iccode = strICCardID;
                    // ETV/TV作业流程节点类型清空
                    deviceStatusETV.prevnode = null;
                    deviceStatusETV.currentnode = null;
                    deviceStatusETV.queueprevnode = null;
                    deviceStatusETV.queuecurrentnode = null;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                }

                nActionFlag += 1;
                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                requestAction.PLCServer = m_commModule;
                requestAction.telegram = string.Format("3-1-{0},13-1-{0}-{1}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);
                //修改数据库中设备作业状态
                deviceStatusTBL.iccode = strICCardID;
                deviceStatusTBL.tasktype = (int)EnmTaskType.ExitTask;
                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
                requestAction.lstTableDto.Add(new struTableDto(deviceStatusTBL, 2));

                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }       
            else if (0 == lstSwipeQueueOut.Count && bETVStat && 0 != nAllotEquipID && 0 != nAllotHallID && 0 == deviceStatusTBL.isavailable && (int)EnmTaskType.ExitTask == deviceStatusTBL.tasktype)
            {
                CDeviceStatusDto deviceStatusETV = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nAllotEquipID);
                if (null != deviceStatusETV)
                {
                    deviceStatusETV.tasktype = (int)EnmTaskType.ExitTask;
                    deviceStatusETV.iccode = strICCardID;
                    // ETV/TV作业流程节点类型清空
                    deviceStatusETV.prevnode = null;
                    deviceStatusETV.currentnode = null;
                    deviceStatusETV.queueprevnode = null;
                    deviceStatusETV.queuecurrentnode = null;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(deviceStatusETV, 2));
                }

                nActionFlag += 1;
                #region add by wanxiaona20150728
                EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
                if (EnmFaultType.Success != enmResult)
                {
                    return enmResult;
                }
                #endregion
                requestAction.PLCServer = m_commModule;
                requestAction.telegram = string.Format("13-1-{0}-{1},3-1-{0}", CConfigManagement.myPara.EquipIsAviliableID, nAllotEquipID);

                requestAction.RequestAction(nActionFlag);
                return EnmFaultType.Success;
            }
            //车厅正在取车 或 取车队列不为空 或 车厅不可接收指令
            else
            {
                EnmFaultType enmResult = AddWorkQueue(nWareHouse, nHallID, strICCardID, deviceStatusTBL);
                if (EnmFaultType.Add == enmResult)
                {
                    //修改数据库，增加取车刷卡到workqueue
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();

                    // bApp取车时优先级
                    if (bApp && 0 < CConfigManagement.myPara.APPPriorityId)
                    {
                        getCarQueue.priorityid = CConfigManagement.myPara.APPPriorityId;
                    }
                    CICCardDto ICCardTable = CCommonMethods.myMethods.GetICCard(strICCardID);
                    if (null != ICCardTable && 0 < ICCardTable.priorityid)
                    {
                        if (0 < getCarQueue.priorityid)
                        {
                            getCarQueue.priorityid = Math.Min((int)getCarQueue.priorityid, (int)ICCardTable.priorityid);
                        }
                        else
                        {
                            getCarQueue.priorityid = ICCardTable.priorityid;
                        }
                    }

                    getCarQueue.devicecode = nHallID;
                    getCarQueue.warehouse = nWareHouse;
                    getCarQueue.iccode = strICCardID;
                    getCarQueue.swipecount = (int)EnmSwipeCount.GetCar;
                    if (4 != (nActionFlag & 4))
                    {
                        nActionFlag += 4;
                    }
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                    requestAction.RequestAction(nActionFlag);
                }
                return enmResult;
            }
        }

        /// <summary>
        /// 车辆挪移
        /// 返回值：
        /// 成功：
        /// EnmFaultType.Success;  
        /// 失败：
        /// EnmFaultType.NotFoundEquip; //没有空闲设备，分配设备失败
        /// EnmFaultType.NotFoundCarPOSN; //指定的源或目的车位不存在
        /// EnmFaultType.NotNormalCarPOSN; //源车位或目的车位不是正常车位
        /// EnmFaultType.NoCarInGarage; //源车位没有车
        /// EnmFaultType.CarInGarage;   //目的车位有车
        /// EnmFaultType.TaskOnICCard;//目的车位的车辆正在作业
        /// EnmFaultType.NoICCardInfo;//源车位的IC卡没有制卡
        /// EnmFaultType.FixedCarPOSN;//目的车位是其他车主的固定车位，临时卡或定期卡无法挪移到固定车位卡的车位上
        /// EnmFaultType.NotMatch;//目的车位是其他车主的固定车位
        /// EnmFaultType.OverCarInSize; //目的车位尺寸无法满足源车位的车辆
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.InvalidEquipID;//无效设备号
        /// EnmFaultType.NotAvailable;  //设备不可接收指令;
        /// EnmFaultType.FailToSendTelegram; //发送报文失败
        /// EnmFaultType.FailToUpdate; //更新数据库失败
        /// EnmFaultType.FailConnection; //连接PLC通信失败
        /// </summary>
        /// <param name="carLocationTBLSrc">源车位</param>
        /// <param name="carLocationTBLDest">目的车位</param>
        /// <returns></returns>
        public virtual EnmFaultType VehicleMove(CCarLocationDto carLocationTBLSrc, CCarLocationDto carLocationTBLDest)
        {
            if (null == carLocationTBLSrc || null == carLocationTBLDest)
            {
                return EnmFaultType.Fail;
            }

            int nWareHouse = (int)carLocationTBLSrc.warehouse;
            int nEquipID = ALLOCETVOrTV(nWareHouse);
            if (0 == nEquipID)
            {
                return EnmFaultType.NotFoundEquip;//没有空闲设备，分配设备失败
            }

            CRequestAction requestAction = new CRequestAction();
            struTelegram struSendTelegram = new struTelegram();
            struSendTelegram.strICCardID = carLocationTBLSrc.iccode;
            struSendTelegram.nPLCSendFlag = nWareHouse;
            struSendTelegram.nEquipID = nEquipID;
            struSendTelegram.nWheelbase = (int)carLocationTBLSrc.carwheelbase;
            struSendTelegram.nDirection = (int)carLocationTBLSrc.direction;
            struSendTelegram.strCarSize = carLocationTBLSrc.carsize;
            struSendTelegram.strSrcLocAddr = carLocationTBLSrc.carlocaddr;
            struSendTelegram.strDestLocAddr = carLocationTBLDest.carlocaddr;
            struSendTelegram.nDeviceByteBit = CConfigManagement.myPara.EquipIsAviliableID;
            struSendTelegram.nEndFlag = 9999;
            #region add by wanxiaona20150728
            EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, nWareHouse);
            if (EnmFaultType.Success != enmResult)
            {
                return enmResult;
            }
            #endregion
            requestAction.PLCServer = m_commModule;

            int nActionFlag = 0;
            nActionFlag += 1;
            requestAction.telegram = string.Format("13-1-{0}", CConfigManagement.myPara.EquipIsAviliableID); //"13-1-297";// 报文类型2（1），报文子类型EMS 3（55）;
            requestAction.structTelegram = struSendTelegram;

            // （13,1）发送挪移装载报文时（表示车位A挪移到车位B），修改A(B).carlocstatus = MoveVeh，B.iccode = A.iccode,其他不变
            carLocationTBLSrc.carlocstatus = (int)EnmLocationStatus.MovingVEH;
            carLocationTBLDest.iccode = carLocationTBLSrc.iccode;
            carLocationTBLDest.carlocstatus = (int)EnmLocationStatus.MovingVEH;

            CDeviceStatusDto deviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, nEquipID);
            if (null != deviceStatusTBL)
            {
                deviceStatusTBL.tasktype = (int)EnmTaskType.MoveCarTask;
                deviceStatusTBL.iccode = carLocationTBLSrc.iccode;
                // 作业流程节点类型清空
                deviceStatusTBL.prevnode = (int)EnmFlowNodeDescp.MoveCarOk;
                deviceStatusTBL.currentnode = (int)EnmFlowNodeDescp.MoveCarOk;
                deviceStatusTBL.queueprevnode = null;
                deviceStatusTBL.queuecurrentnode = null;
                if (4 != (nActionFlag & 4))
                {
                    nActionFlag += 4;
                }
                requestAction.lstTableDto.Add(new struTableDto(deviceStatusTBL, 2));
            }
            if (4 != (nActionFlag & 4))
            {
                nActionFlag += 4;
            }
            requestAction.lstTableDto.Add(new struTableDto(carLocationTBLSrc, 2));
            requestAction.lstTableDto.Add(new struTableDto(carLocationTBLDest, 2));
            requestAction.RequestAction(nActionFlag);
            return EnmFaultType.Success;
        }

        /// <summary>
        /// 处理存车扫描
        /// EnmFaultType.InvalidWareHouseID;//无效库区号
        /// EnmFaultType.Fail; //获取通信模式块失败，请查看配置文件是否正确
        /// EnmFaultType.FailConnection;//连接PLC通信失败
        /// EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
        /// EnmFaultType.NotMatch;// 与第一次刷卡卡号不一致，请重新刷卡
        /// EnmFaultType.OverCarInSize;//车体超限，请退出
        /// EnmFaultType.NotFoundCarPOSN;// 已无合适车位，请退出
        /// EnmFaultType.Fail;// 上位控制系统故障
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="strICCard"></param>
        /// <param name="HallStatusTable"></param>
        /// <param name="CarLocation"></param>
        /// <returns></returns>
        public virtual EnmFaultType DealParkingScan(ref int nActionFlag, ref CRequestAction requestAction, string strICCard, CDeviceStatusDto HallStatusTable, CCarLocationDto CarLocation)
        {
            //根据库区号、车厅号和IC卡卡号获取刷卡信息
            CWorkQueueDto workQueueTable = CCommonMethods.myMethods.GetSwipeCardInfo(HallStatusTable.warehouse, HallStatusTable.devicecode, strICCard);

            if (null == workQueueTable)
            {
                // 存车
                // 临时取物
                // 中集天达新厂塔库临时取物后直接刷卡存车需求
                if (string.IsNullOrWhiteSpace(HallStatusTable.iccode)
                    || ((int)EnmHallType.EnterOrExit == HallStatusTable.halltype
                         && strICCard == HallStatusTable.iccode
                         && (((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype)
                              || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(HallStatusTable.warehouse) && CConfigManagement.myPara.DicCIMCTmpFetch[HallStatusTable.warehouse]))))
                {
                    EnmFaultType enmResult = CCommonMethods.myMethods.ClientPLCConnect(ref m_commModule, HallStatusTable.warehouse);
                    if (EnmFaultType.Success != enmResult)
                    {
                        return enmResult;
                    }

                    // 车厅作业流程节点类型赋值
                    HallStatusTable.iccode = strICCard;//修改数据库
                    HallStatusTable.prevnode = HallStatusTable.currentnode;
                    HallStatusTable.currentnode = (int)EnmFlowNodeDescp.EnterSwipeTwo;
                    requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));
                    //第二次刷卡 发报文
                    struTelegram struTelegram = new struTelegram();
                    requestAction.telegram = "1-9";
                    struTelegram.nPLCSendFlag = HallStatusTable.warehouse;
                    struTelegram.nEquipID = HallStatusTable.devicecode;
                    struTelegram.strICCardID = strICCard;
                    struTelegram.nEndFlag = 9999;

                    if ((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(HallStatusTable.warehouse)
                        && CConfigManagement.myPara.DicCIMCTmpFetch[HallStatusTable.warehouse]))
                    {
                        struTelegram.strSrcLocAddr = HallStatusTable.deviceaddr;
                        if (null != CarLocation)
                        {
                            struTelegram.strDestLocAddr = CarLocation.carlocaddr;
                            struTelegram.nWheelbase = (int)CarLocation.carwheelbase;
                            struTelegram.nDirection = (int)CarLocation.direction;
                            struTelegram.strCarSize = CarLocation.carsize;
                        }
                        else
                        {
                            CLOGException.Trace(HallStatusTable.warehouse, "CAndroidService.DealParkingSwipe", "临时取物转存时获取的车辆信息为空:");
                        }
                    }
                    requestAction.structTelegram = struTelegram;
                    requestAction.PLCServer = m_commModule;
                    requestAction.soundName = "21.wav";// 请稍候，听到提示再离开21.wav(第二次刷卡)
                    //修改数据库，增加第二次存车刷卡到workqueue表中
                    CWorkQueueDto getCarQueue = new CWorkQueueDto();
                    getCarQueue.devicecode = HallStatusTable.devicecode;
                    getCarQueue.warehouse = HallStatusTable.warehouse;
                    getCarQueue.iccode = strICCard;
                    getCarQueue.swipecount = (int)EnmSwipeCount.SaveCarSecond;
                    requestAction.lstTableDto.Add(new struTableDto(getCarQueue, 1));
                    nActionFlag = 7;
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
                }
                else
                {
                    if (strICCard == HallStatusTable.iccode)
                    {
                        requestAction.soundName = "17.wav";// 您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        requestAction.RequestAction(nActionFlag);
                        return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                    }
                    else
                    {
                        //所有错误的存车卡：与第一次刷卡不一致，只语音提示
                        requestAction.soundName = "24.wav";// 与第一次刷卡卡号不一致，请重新刷卡24.wav
                        if (2 != (nActionFlag & 2))
                        {
                            nActionFlag += 2;
                        }
                        requestAction.RequestAction(nActionFlag);
                        return EnmFaultType.NotMatch;// 与第一次刷卡卡号不一致，请重新刷卡
                    }
                }
            }
            else
            {
                if ((int)EnmSwipeCount.SaveCarFirst == workQueueTable.swipecount)
                {
                    // 车厅作业流程节点类型赋值
                    HallStatusTable.prevnode = HallStatusTable.currentnode;
                    HallStatusTable.currentnode = (int)EnmFlowNodeDescp.EnterSwipeTwo;
                    requestAction.lstTableDto.Add(new struTableDto(HallStatusTable, 2));

                    //第二次刷卡 发报文
                    struTelegram struTelegram = new struTelegram();
                    requestAction.telegram = "1-9";
                    struTelegram.nPLCSendFlag = HallStatusTable.warehouse;
                    struTelegram.nEquipID = HallStatusTable.devicecode;
                    struTelegram.strICCardID = strICCard;
                    struTelegram.nEndFlag = 9999;

                    if ((int)EnmTaskType.TmpFetch == HallStatusTable.tasktype || (CConfigManagement.myPara.DicCIMCTmpFetch.ContainsKey(HallStatusTable.warehouse)
                        && CConfigManagement.myPara.DicCIMCTmpFetch[HallStatusTable.warehouse]))
                    {
                        struTelegram.strSrcLocAddr = HallStatusTable.deviceaddr;
                        if (null != CarLocation)
                        {
                            struTelegram.strDestLocAddr = CarLocation.carlocaddr;
                            struTelegram.nWheelbase = (int)CarLocation.carwheelbase;
                            struTelegram.nDirection = (int)CarLocation.direction;
                            struTelegram.strCarSize = CarLocation.carsize;
                        }
                        else
                        {
                            CLOGException.Trace(HallStatusTable.warehouse, "CAndroidService.DealParkingSwipe", "临时取物转存时获取的车辆信息为空:");
                        }
                    }
                    requestAction.structTelegram = struTelegram;
                    requestAction.soundName = "21.wav";// 请稍候，听到提示再离开21.wav(第二次刷卡)
                    workQueueTable.swipecount = (int)EnmSwipeCount.SaveCarSecond;
                    requestAction.lstTableDto.Add(new struTableDto(workQueueTable, 2));// 修改刷卡次数为2
                    nActionFlag = 7;
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }
                else if ((int)EnmSwipeCount.SaveCarSecond == workQueueTable.swipecount)
                {
                    //刷卡第三次或多次，语音提示。
                    requestAction.soundName = "17.wav";//您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后;
                }
                else if ((int)EnmSwipeCount.Oversize == workQueueTable.swipecount)
                {
                    //提示对不起，车体超限，请退出
                    requestAction.soundName = "04.wav";// 车体超限，请退出04.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.OverCarInSize;//车体超限，请退出
                }
                else if ((int)EnmSwipeCount.NoSuitableSize == workQueueTable.swipecount)
                {
                    // 已无合适车位，请退出27.wav
                    requestAction.soundName = "27.wav";// 已无合适车位，请退出27.wav(第3次或更多次刷卡)
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.NotFoundCarPOSN;// 已无合适车位，请退出
                }
                else
                {
                    //异常，实际上不存在
                    //提示控制系统故障
                    requestAction.soundName = "20.wav";//上位控制系统故障20.wav
                    if (2 != (nActionFlag & 2))
                    {
                        nActionFlag += 2;
                    }
                    CLOGException.Trace(HallStatusTable.warehouse, "CAndroidService.DealParkingSwipe", "上位控制系统故障20.wav，数据库表workQueueTable中的刷卡次数异常, Hall ID:" + HallStatusTable.devicecode);
                    requestAction.RequestAction(nActionFlag);
                    return EnmFaultType.Fail;// 上位控制系统故障
                }
            }
        }

        /// <summary>
        /// 处理MURO继续时，进行卸载操作
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <returns></returns>
        public virtual int DealMuroUnloadCmd(CDeviceStatusDto etv, CCarLocationDto car) 
        {
            //在子类中实现
            return 0;
        }

        /// <summary>
        /// 处理MURO继续时，进行装载操作
        /// </summary>
        /// <param name="etv"></param>
        /// <param name="car"></param>
        /// <param name="isHall"></param>
        /// <returns></returns>
        public virtual int DealMUROLoadCmd(CDeviceStatusDto etv, CCarLocationDto car, bool isHall)
        {
            //在子类中实现
            return 0;
        }
        #endregion

        #region 分配算法algorithm
        /// <summary>
        /// 分配车位算法（巷道堆垛）
        /// </summary>
        /// <param name="struTelegram"></param>
        /// <returns></returns>
        public virtual CCarLocationDto ALLOCCarLocation(struTelegram struTelegram, ref string strError)
        {
            try
            {
                CCarLocationDto carLocation = CCommonMethods.myMethods.GetCarLocation(struTelegram.strICCardID);
                if (null != carLocation)
                {
                    if (struTelegram.nPLCSendFlag != carLocation.warehouse)
                    {
                        strError = "16.wav";// //您的车辆停在其他库区16.wav
                        return null;
                    }
                    return carLocation;
                }
                CICCardDto icCard = CCommonMethods.myMethods.GetICCard(struTelegram.strICCardID);
                int nWareHouse = struTelegram.nPLCSendFlag;
                int nCarSize = Convert.ToInt32(struTelegram.strCarSize);

                if (null == icCard)
                {
                    return null;
                }

                //如果是临时卡的话，为定期卡车主预留车位
                if (icCard.ictype == (int)EnmICCardType.Temp)
                {
                    carLocation = AllotLocationOfTemp(nWareHouse, nCarSize);
                }
                else if (icCard.ictype == (int)EnmICCardType.Fixed)
                {
                    if (string.IsNullOrWhiteSpace(icCard.carlocaddr) || !CBaseMethods.MyBase.CheckCarSize(struTelegram.strCarSize, icCard.carlocaddr))
                    {// 预留车位尺寸无效时，作为临时卡处理
                        // 分配车位是根据不超过该预留车位尺寸来分配车位，如果超过只能给临时的车位分配，否则不接收存车
                        carLocation = AllotLocationOfTemp(nWareHouse, nCarSize);
                    }
                    else
                    {
                        carLocation = AllocateLocationOfPriorityid(nWareHouse, nCarSize);
                    }
                }
                else if (icCard.ictype == (int)EnmICCardType.FixedLocation)
                {
                    if (icCard.warehouse != nWareHouse)
                    {// 绑定车位信息不是本车库的车位时，则自由分配车位
                        carLocation = AllocateLocationOfPriorityid(nWareHouse, nCarSize);
                    }
                    else
                    {// 获取绑定的车位信息
                        carLocation = CCommonMethods.myMethods.GetCarLocation(nWareHouse, icCard.carlocaddr);

                        // 绑定车位信息不正确时，则自由分配车位
                        if (carLocation == null || carLocation.carloctype != (int)EnmLocationType.Normal
                            || carLocation.carlocstatus != (int)EnmLocationStatus.Space)
                        {
                            carLocation = AllocateLocationOfPriorityid(nWareHouse, nCarSize);
                        }
                        else if (CConfigManagement.myPara.FixAllotTempcarlocFlag &&
                                 !CBaseMethods.MyBase.CheckCarSize(struTelegram.strCarSize, carLocation.carlocsize))
                        {
                            carLocation = AllocateLocationOfPriorityid(nWareHouse, nCarSize);
                        }
                    }
                }
                else
                {
                    strError = "32.wav";// 该卡异常，请联系管理人员处理32.wav
                    CLOGException.Trace(struTelegram.nPLCSendFlag, "CTaskTrigger.ALLOCCarLocation ", "该卡异常，请联系管理人员处理32.wav，ICCard ID :" + struTelegram.strICCardID);
                }

                return carLocation;
            }
            catch (Exception ex)
            {
                CLOGException.Trace("CTaskTrigger.ALLOCCarLocation 异常", CBaseMethods.MyBase.GetExceptionInfo(ex));
                return null;
            }
        }

        /// <summary>
        /// 分配ETV设备算法,返回设备号，分配失败返回：0
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        public virtual int ALLOCETVOrTV(int nWareHouse)
        {
            List<CDeviceStatusDto> lstDeviceStatusTBL = new List<CDeviceStatusDto>();
            lstDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);

            int nEquipID = 0;//0是无效设备号
            foreach (CDeviceStatusDto deviceDtatusTBL in lstDeviceStatusTBL)
            {
                if (1 == deviceDtatusTBL.isable && 1 == deviceDtatusTBL.isavailable && (int)EnmModel.Automatic == deviceDtatusTBL.devicemode
                    && (int)EnmTaskType.Init == deviceDtatusTBL.tasktype)
                {
                    nEquipID = deviceDtatusTBL.devicecode;
                    break;
                }
            }
            return nEquipID;
        }

        /// <summary>
        /// 分配车厅设备算法——根据车厅排队最少分配
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nDeviceAddr"></param>
        /// <returns></returns>
        public virtual int ALLOCHallByQueue(int nWareHouse,int region,CCarLocationDto loc)
        {
            int nHallIDQueue = 0;//0是无效设备号
            int nMinListCount = 0;
            List<CDeviceStatusDto> lstDeviceStatusTBL = new List<CDeviceStatusDto>();
            lstDeviceStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.Hall);
            // 根据车厅类型升序排列（进车厅1、出车厅2、进出两用车厅3）
            var query = from dto in lstDeviceStatusTBL
                        where (int)EnmHallType.Entance != dto.halltype
                        orderby dto.halltype
                        select dto;
            lstDeviceStatusTBL = query.ToList();

            foreach (CDeviceStatusDto deviceDtatusTBL in lstDeviceStatusTBL)
            {
                if ((int)EnmModel.Automatic == deviceDtatusTBL.devicemode
                    && (int)EnmTaskType.Init == deviceDtatusTBL.tasktype 
                    && 1 == deviceDtatusTBL.isavailable)
                {// 先根据有无空闲车厅分配
                    return deviceDtatusTBL.devicecode;
                }

                // 车厅排队最少分配
                if ((int)EnmModel.Automatic == deviceDtatusTBL.devicemode)
                {
                    List<CWorkQueueDto> lst = CCommonMethods.myMethods.GetSwipeCardQueue(nWareHouse, deviceDtatusTBL.devicecode, (int)EnmSwipeCount.GetCar, (int)EnmSwipeCount.TmpFetch);
                    if (null == lst)
                    {
                        continue;
                    }

                    if (0 == nHallIDQueue || nMinListCount > lst.Count)
                    {
                        nMinListCount = lst.Count;
                        nHallIDQueue = deviceDtatusTBL.devicecode;
                    }
                }
            }

            return nHallIDQueue;
        }

        /// <summary>
        /// 检查指定的车厅是否有可用的ETV
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <returns></returns>
        public virtual bool CheckETVStat(int nWareHouse, int nHallID)
        {
            bool bReturn = false;
            List<CDeviceStatusDto> lstETVStatusTBL = CCommonMethods.myMethods.GetDeviceStatusList(nWareHouse, EnmSMGType.ETV);
            foreach (CDeviceStatusDto table in lstETVStatusTBL)
            {
                if (1 == table.isable)
                {
                    bReturn = true;
                }
            }

            if (null == lstETVStatusTBL || 1 > lstETVStatusTBL.Count)
            {
                // 无ETV设备时
                return true;
            }
            return bReturn;
        }
        #endregion  通用算法algorithm

        #region 基本函数
        /// <summary>
        /// 清除当前卡的所有排队信息
        /// </summary>
        /// <param name="requestAction"></param>
        public void ClearWorkQueueSwipe(ref CRequestAction requestAction, string strICCardID)
        {
            List<CWorkQueueDto> lstSwipeCardInfoTable = new List<CWorkQueueDto>();

            if (string.IsNullOrWhiteSpace(strICCardID))
            {// 删除第一次存车刷卡记录和车辆跑位的刷卡记录
                lstSwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardQueue(requestAction.structTelegram.nPLCSendFlag, requestAction.structTelegram.nEquipID, (int)EnmSwipeCount.SaveCarFirst, (int)EnmSwipeCount.Oversize, (int)EnmSwipeCount.NoSuitableSize);
            }
            else
            {
                lstSwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardQueue(strICCardID);
            }

            foreach (CWorkQueueDto wqdto in lstSwipeCardInfoTable)
            {
                requestAction.lstTableDto.Add(new struTableDto(wqdto, 3));
            }
        }

        /// <summary>
        /// 清空当前车厅存车刷一次卡信息
        /// </summary>
        /// <param name="requestAction"></param>
        public void ClearWorkQueueSwipe(ref CRequestAction requestAction)
        {
            // 删除第一次存车刷卡记录和车辆跑位的刷卡记录
            List<CWorkQueueDto> lstSwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardQueue(requestAction.structTelegram.nPLCSendFlag, requestAction.structTelegram.nEquipID, (int)EnmSwipeCount.SaveCarFirst);
         
            foreach (CWorkQueueDto wqdto in lstSwipeCardInfoTable)
            {
                requestAction.lstTableDto.Add(new struTableDto(wqdto, 3));
            }
        }

        /// <summary>
        /// 初始化车位，成功返回true，失败返回false
        /// </summary>
        /// <param name="carPOSNTable"></param>
        /// <returns></returns>
        public bool InitCarPOSN(ref CRequestAction requestAction, CCarLocationDto carPOSNTable)
        {
            if (null == carPOSNTable || carPOSNTable.carloctype != (int)EnmLocationType.Normal)
            {
                return false;
            }

            // 更新当前车位信息为空
            carPOSNTable.carlocstatus = (int)EnmLocationStatus.Space;
            carPOSNTable.iccode = null;
            carPOSNTable.carwheelbase = null;
            carPOSNTable.direction = null;
            carPOSNTable.carsize = string.Empty;
            carPOSNTable.carintime = null;
            //carPOSNTable.isbackup = null;
            //carPOSNTable.carnumber = string.Empty;
            //carPOSNTable.carpicture = string.Empty;
            carPOSNTable.overallLg = 0;
            carPOSNTable.overhang = 0;
            carPOSNTable.rearwheeldis = 0;
            carPOSNTable.offcenter = 0;
            carPOSNTable.carweight = 0;

            requestAction.lstTableDto.Add(new struTableDto(carPOSNTable, 2));
            return true;
        }

        /// <summary>
        /// 插入临时取物/取车队列
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nHallID"></param>
        /// <param name="strICCardID"></param>
        /// <param name="deviceStatusTBL"></param>
        /// <returns></returns>
        public EnmFaultType AddWorkQueue(int nWareHouse, int nHallID, string strICCardID, CDeviceStatusDto deviceStatusTBL)
        {
            //根据库区号、车厅号和IC卡卡号获取刷卡信息
            CWorkQueueDto SwipeCardInfoTable = CCommonMethods.myMethods.GetSwipeCardInfo(nWareHouse, nHallID, strICCardID);

            //正在为您出车，请稍后
            if (strICCardID == deviceStatusTBL.iccode)
            {
                return EnmFaultType.TaskOnICCard;//正在为您作业，请稍后
            }
            //已经将您加到取车队列，请排队等候出车
            else if (null != SwipeCardInfoTable && ((int)EnmSwipeCount.GetCar == SwipeCardInfoTable.swipecount || (int)EnmSwipeCount.TmpFetch == SwipeCardInfoTable.swipecount))
            {
                return EnmFaultType.Wait;//已经将您加到取车队列，请排队等候出车
            }
            //前面有人正在取车，已经将您加到取车队列，请排队等候出车
            else
            {
                List<CWorkQueueDto> lstWorkQueueTBL = CCommonMethods.myMethods.GetSwipeCardQueue(strICCardID);
                SwipeCardInfoTable = (null == lstWorkQueueTBL) ? null : lstWorkQueueTBL.Find(s => (int)EnmSwipeCount.GetCar == s.swipecount || (int)EnmSwipeCount.TmpFetch == s.swipecount);

                if (null != SwipeCardInfoTable)
                {
                    return EnmFaultType.Wait;//已经将您加到取车队列，请排队等候出车
                }
                return EnmFaultType.Add; //前面有人正在取车，已经将您加到取车队列，请排队等候出车
            }
        }
        #endregion 基本函数

        #region 抽象方法
        /// <summary>
        /// 车厅确认有车入库，入库指令接受(1001,54)
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="struTelegram"></param>
        /// <param name="deviceStatus"></param>
        protected abstract void EnterCommandAccept(out int nActionFlag, ref CRequestAction requestAction, struTelegram struTelegram, CDeviceStatusDto deviceStatus);

        /// <summary>
        /// 设备空闲时,下发队列中有报文的队列
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="eqp"></param>
        /// <param name="queue"></param>
        /// <param name="isUnLoad"></param>
        /// <returns></returns>
        public abstract void SendQueueHasTelegram(ref int nActionFlag, ref CRequestAction requestAction,CDeviceStatusDto eqp,CWorkQueueDto queue,bool isUnLoad);

        /// <summary>
        /// 车厅空闲时，执行队列，下发车厅报文，组装TV报文
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="hall"></param>
        /// <param name="queue"></param>
        /// <param name="lctn"></param>
        /// <returns></returns>
        public abstract void SendHallTelegramAndBuildTvTele(ref int nActionFlag, ref CRequestAction requestAction, CDeviceStatusDto hall, CWorkQueueDto queue, CCarLocationDto lctn);

        /// <summary>
        /// 车厅允许TV提前装载，判断是否允许下发，并下发相应报文
        /// </summary>
        /// <param name="nActionFlag"></param>
        /// <param name="requestAction"></param>
        /// <param name="hall"></param>
        /// <param name="queue"></param>
        /// <param name="lctn"></param>
        /// <returns></returns>
        public abstract void AheadSendTvTeleAndBuildHallTele(ref int nActionFlag, ref CRequestAction requestAction, CDeviceStatusDto hall, CWorkQueueDto queue, CCarLocationDto lctn);
        
        #endregion 抽象方法

        #region 私有函数
        /// <summary>
        /// 获取有车入库语音文件
        /// </summary>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        private string GetEnterCarSoundName(CDeviceStatusDto deviceStatus)
        {
            string strName = "18.wav";// 请拉好手刹，关好车门，收起后视镜和天线，贵重物品请带走，刷卡存车18.wav
            //根据库区号、车厅号和IC卡卡号获取刷卡信息
            CWorkQueueDto workQueueTable = CCommonMethods.myMethods.GetSwipeCardInfo(deviceStatus.warehouse, deviceStatus.devicecode, deviceStatus.iccode);

            //CLOGException.Trace(deviceStatus.warehouse, "WorkFlowLib", "请拉好手刹，关好车门，收起后视镜和天线，贵重物品请带走，刷卡存车18.wav");// trace
            if (null != workQueueTable)
            {
                if ((int)EnmSwipeCount.SaveCarFirst == workQueueTable.swipecount)
                {//2 
                    strName = "09.wav";//确认安全后再次刷卡09.wav(第一次刷卡)
                    //CLOGException.Trace(deviceStatus.warehouse, "WorkFlowLib", "确认安全后再次刷卡09.wav(第一次刷卡)");// trace
                }
                else if ((int)EnmSwipeCount.SaveCarSecond == workQueueTable.swipecount)
                {
                    //刷卡第三次或多次，语音提示。
                    strName = "17.wav";// 您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)
                    //CLOGException.Trace(deviceStatus.warehouse, "WorkFlowLib", "您的存车刷卡已完毕，请不要重复刷卡17.wav(第3次或更多次刷卡)");// trace
                }
            }

            return strName;
        }

        /// <summary>
        /// 获取车位信息具体实例（分配临时卡车位 根据除定期卡和固定车位卡预留车位之外的空余车位数->临时卡车位数按照优先级分配）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarSize"></param>
        /// <returns></returns>
        private CCarLocationDto AllotLocationOfTemp(int nWareHouse, int nCarSize)
        {
            List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
            lstCarLocationTBL = CCommonMethods.myMethods.GetCarLocationList();

            // 当前库正常空余车位列表（按车位优先级priorityid排序）
            // 该车位为固定车位卡预留GetICCard((int)lct.warehouse, lct.carlocaddr) != null&&
            //CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), lct.carlocsize) 
            var query = from lct in lstCarLocationTBL
                        where lct.warehouse == nWareHouse &&
                        lct.carlocstatus == (int)EnmLocationStatus.Space &&
                        lct.carloctype == (int)EnmLocationType.Normal &&
                        CCommonMethods.myMethods.GetICCard((int)lct.warehouse, lct.carlocaddr) == null
                        orderby lct.priorityid
                        select lct;
            List<CCarLocationDto> locationList = query.ToList();
            // 车厅作为车位处理情况20160309
            foreach (CCarLocationDto lct in locationList)
            {
                CDeviceStatusDto table = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, lct.carlocaddr, EnmSMGType.Hall);
                if (null != table && ((int)EnmHallType.Exit != table.halltype || 0 == table.isavailable))
                {
                    locationList.Remove(lct);
                }
            }

            List<CICCardDto> lstICCardTBL = CCommonMethods.myMethods.GetICCardList();
            // 当前库正常定期卡列表
            List<CICCardDto> lstFixCard = lstICCardTBL.FindAll(s => s.ictype == (int)EnmICCardType.Fixed
                                                            && s.icstatus == (int)EnmICCardStatus.Normal);

            // 剔除为未存车的定期卡预留的车位
            for (int i = 0; i < lstFixCard.Count; i++)
            {
                CICCardDto card = lstFixCard[i];

                if (!CBaseMethods.MyBase.IsUIntNumWithoutZero(card.carlocaddr) || 
                    null != CCommonMethods.myMethods.GetCarLocation(card.iccode))
                {// 无效预留尺寸或者该定期卡已存车
                    lstFixCard.Remove(card);
                    i--;
                    continue;
                }

                var query1 = from lct in locationList
                             orderby lct.carlocsize
                             select lct;// 根据车位尺寸carlocsize排序
                foreach (CCarLocationDto lct in query1.ToList())
                {
                    if (CBaseMethods.MyBase.CheckCarSize(card.carlocaddr, lct.carlocsize))
                    {
                        locationList.Remove(lct);
                        lstFixCard.Remove(card);
                        i--;
                        break;
                    }
                }
            }

            // 先找到等于该车辆尺寸的车位
            CCarLocationDto carLct = locationList.Find(s => s.carlocsize == nCarSize.ToString());

            if (null != carLct)
            {
                return carLct;
            }

            if (0 < locationList.Count)
            {
                return locationList.Find(s => CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), s.carlocsize));
            }

            return null;
        }

        /// <summary>
        /// 获取车位信息具体实例（根据车位优先级priorityid排序分配）
        /// </summary>
        /// <param name="nWareHouse"></param>
        /// <param name="nCarSize"></param>
        /// <returns></returns>
        private CCarLocationDto AllocateLocationOfPriorityid(int nWareHouse, int nCarSize)
        {
            // 当前库正常空余车位列表（按车位优先级priorityid排序）
            // 该车位为固定车位卡预留GetICCard((int)lct.warehouse, lct.carlocaddr) != null&&
            List<CCarLocationDto> lstCarLocationTBL = new List<CCarLocationDto>();
            lstCarLocationTBL = CCommonMethods.myMethods.GetCarLocationList();
            var query = from lct in lstCarLocationTBL
                        where lct.warehouse == nWareHouse &&
                        lct.carlocstatus == (int)EnmLocationStatus.Space &&
                        lct.carloctype == (int)EnmLocationType.Normal &&
                        CBaseMethods.MyBase.CheckCarSize(nCarSize.ToString(), lct.carlocsize) &&
                        CCommonMethods.myMethods.GetICCard((int)lct.warehouse, lct.carlocaddr) == null
                        orderby lct.priorityid
                        select lct;

            List<CCarLocationDto> locationList = query.ToList();
            // 车厅作为车位处理情况20160309
            foreach (CCarLocationDto lct in locationList)
            {
                CDeviceStatusDto table = CCommonMethods.myMethods.GetDeviceStatus(nWareHouse, lct.carlocaddr, EnmSMGType.Hall);
                if (null != table && ((int)EnmHallType.Exit != table.halltype || 0 == table.isavailable))
                {
                    locationList.Remove(lct);
                }
            }

            // 先找到等于该车辆尺寸的车位
            CCarLocationDto carLct = locationList.Find(s => s.carlocsize == nCarSize.ToString());

            if (null != carLct)
            {
                return carLct;
            }

            if (0 < locationList.Count)
            {
                return locationList[0];
            }

            return null;
        }
        #endregion
    }
}
