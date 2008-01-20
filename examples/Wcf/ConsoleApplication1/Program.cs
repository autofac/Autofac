using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceReference1.IService1 s = new ServiceReference1.Service1Client();

            string r = s.GetData(1);

            Console.WriteLine(r);

            ((IDisposable)s).Dispose();

            Console.ReadLine();
        }
    }
}
