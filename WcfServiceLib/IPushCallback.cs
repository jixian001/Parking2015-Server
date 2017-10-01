using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DatabaseManagementLib;

namespace WCFServiceLib
{
    /// <summary>
    /// 推送消息的回调接口
    /// </summary>
    [ServiceContract]
    [ServiceKnownType(typeof(CCarLocationDto))]
    [ServiceKnownType(typeof(CDeviceStatusDto))]
    [ServiceKnownType(typeof(CDeviceFaultDto))]
    public interface IPushCallback
    {
        [OperationContract(IsOneWay = true)]
        void Push(object objTable);
    }
}
