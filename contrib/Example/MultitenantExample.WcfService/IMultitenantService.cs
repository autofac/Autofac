using System;
using System.ServiceModel;

namespace MultitenantExample.WcfService
{
    [ServiceContract]
    public interface IMultitenantService
    {
        [OperationContract]
        GetServiceInfoResponse GetServiceInfo();
    }
}
