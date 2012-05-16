using System;

namespace AutofacContrib.Tests.AggregateService
{
    public class MyServiceImpl : IMyService
    {
        public MyServiceImpl()
        {
        }

        public MyServiceImpl(int someIntValue)
        {
            SomeIntValue = someIntValue;
        }

        public MyServiceImpl(string someStringValue, ISomeDependency someDependency)
        {
            SomeStringValue = someStringValue;
            SomeDependency = someDependency;
        }

        public MyServiceImpl(DateTime someDate, int someInt, ISomeDependency someDependency)
        {
            SomeDateValue = someDate;
            SomeIntValue = someInt;
            SomeDependency = someDependency;
        }

        public int SomeIntValue
        {
            get;
            private set;
        }

        public string SomeStringValue
        {
            get;
            private set;
        }

        public DateTime SomeDateValue
        {
            get; private set;
        }

        public ISomeDependency SomeDependency { get; private set; }
    }
}