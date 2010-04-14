using System;

namespace AutofacContrib.Tests.AggregateService
{
    /// <summary>
    /// Interface illustrating an aggregate service context with supported and unsupported
    /// method signatures.
    /// </summary>
    public interface IMyContext
    {
        // Supported signatures
        IMyService MyService { get; }
        IMyService GetMyService();
        IMyService GetMyService(int someValue);
        IMyService GetMyService(string someOtherValue);
        IMyService GetMyService(DateTime someDate, int someInt);

        // Unsupported signatures
        IMyService PropertyWithSetter { get; set; }
        void MethodWithoutReturnValue();
    }
}