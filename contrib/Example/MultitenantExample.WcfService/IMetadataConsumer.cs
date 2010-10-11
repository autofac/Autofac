using System;
using System.ServiceModel;

namespace MultitenantExample.WcfService
{
	[ServiceContract]
	public interface IMetadataConsumer
	{
		[OperationContract]
		GetServiceInfoResponse GetServiceInfo();
	}
}
