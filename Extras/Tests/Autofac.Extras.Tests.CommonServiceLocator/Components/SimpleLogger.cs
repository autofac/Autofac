using System;

namespace AutofacContrib.Tests.CommonServiceLocator.Components
{
	public class SimpleLogger : ILogger
	{
		public void Log(string msg)
		{
			Console.WriteLine(msg);
		}
	}
}