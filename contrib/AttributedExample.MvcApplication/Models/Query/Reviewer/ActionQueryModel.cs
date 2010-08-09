using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttributedExample.MvcApplication.Models.Query.Reviewer
{
    [QueryModelMetadata(RoleType.Reviewer, QueryType.Monitor)]
    public class ActionQueryModel : IQueryModel
    {
    }
}