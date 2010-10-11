using System;
using System.ServiceModel;
using AutofacContrib.Multitenant.Wcf;

namespace MultitenantExample.WcfService
{
	[ServiceContract]
	[ServiceMetadataType(typeof(MetadataConsumerBuddyClass))]
	public interface IMetadataConsumer
	{
		[OperationContract]
		GetServiceInfoResponse GetServiceInfo();
	}
}
