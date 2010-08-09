using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttributedExample.MvcApplication.Models.Query.Operator
{
    [QueryModelMetadata(RoleType.Operator, QueryType.Monitor)]
    public class MonitorQueryModel : IQueryModel
    {
    }
}