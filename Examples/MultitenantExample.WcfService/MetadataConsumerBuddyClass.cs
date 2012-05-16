using System;
using System.ServiceModel;

namespace MultitenantExample.WcfService
{
    // Specifying a ConfigurationName on a metadata buddy class allows you to
    // set configuration in web.config (or app.config) and have a nice, friendly,
    // predictable configuration name for your service element.
    [ServiceBehavior(ConfigurationName = "MultitenantExample.WcfService.IMetadataConsumer")]
    public class MetadataConsumerBuddyClass
    {
    }
}