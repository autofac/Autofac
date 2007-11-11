using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using ShareTrader.Model;

namespace ShareTrader
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var portfolio = Global.Container.Resolve<Portfolio>();
            portfolio.Add("GNU", 1200);
            portfolio.Add("MONO", 300);
            portfolio.Add("LINUX", 500);
            ValueLabel.Text = portfolio.Value.ToString("C");
        }
    }
}
