using System;
using System.Linq;
using Autofac;

namespace CircularCrashTest
{
    public interface IClass1
    {
        void DoClass1();
    }

    public class Class1 : IClass1
    {
        public IClass2 Class2 { get; set; }

        public Class1()
        {
        }

        public void DoClass1()
        {
            Console.WriteLine("DoClass1 called");
            Class2.DoClass2Extra();
        }
    }

    public interface IClass2
    {
        void DoClass2();
        void DoClass2Extra();
    }

    public class Class2 : IClass2
    {
        public IClass1 Class1 { get; set; }

        public Class2()
        {
        }

        public void DoClass2()
        {
            Console.WriteLine("DoClass2 called");
            if (Class1 == null) {
                // This should never happen
                Console.WriteLine("Cannot call Class1 - it should NOT be null!");
            } else {
                Class1.DoClass1();
            }
        }

        public void DoClass2Extra()
        {
            Console.WriteLine("DoClass2Extra called");
        }
    }

    public interface IClass3
    {
        void DoClass3();
    }

    public class Class3 : IClass3
    {
        public Class3(
            IClass2 class2)
        {
            // We SHOULD be able to call into class2 here and FULLY expect that it is completely initialized
            // as this class should NOT have to care how IClass2 is created. It should be fully resolved before
            // this constructor is called.
            class2.DoClass2();
        }

        public void DoClass3()
        {
            Console.WriteLine("DoClass3 called");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Class1>().As<IClass1>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<Class2>().As<IClass2>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<Class3>().As<IClass3>().InstancePerLifetimeScope();

            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope((ContainerBuilder b) => { })) {
                var class3 = scope.Resolve<IClass3>();
                class3.DoClass3();

                Console.WriteLine("Press Enter");
                Console.ReadLine();
            }
        }
    }
}
