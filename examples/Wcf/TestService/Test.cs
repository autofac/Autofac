using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace TestService
{
    public interface ITest
    {
        string Execute();
    }

    public class Test : ITest
    {
        public string Execute()
        {
            return "Test";
        }
    }
}
