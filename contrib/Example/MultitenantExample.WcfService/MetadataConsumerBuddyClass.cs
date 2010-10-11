using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace MultitenantExample.WcfService
{
    [ServiceBehavior(Name = "MultitenantExample.WcfService.IMetadataConsumer")]
    public class MetadataConsumerBuddyClass
    {
    }
}