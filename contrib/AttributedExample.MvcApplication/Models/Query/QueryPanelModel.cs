using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttributedExample.MvcApplication.Models.Query
{
    public interface IQueryPanelModel
    {
        string Id { get; }
        IQueryModel QueryModel { get; }
    }


    public class QueryPanelModel : IQueryPanelModel
    {
        public IQueryModel QueryModel { get; private set; }
        public string Id { get; private set; }
        public QueryPanelModel(IQueryModel queryModel, QueryType queryType)
        {
            QueryModel = queryModel;
            Id = queryType.ToString();
        }
    }
}