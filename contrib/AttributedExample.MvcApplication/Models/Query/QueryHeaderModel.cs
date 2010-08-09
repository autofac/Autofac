using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttributedExample.MvcApplication.Models.Query
{
    public class QueryHeaderModel
    {
        public string Title { get; private set; }
        public QueryHeaderModel(QueryType queryType)
        {
            Title = queryType.ToString();
        }
    }
}