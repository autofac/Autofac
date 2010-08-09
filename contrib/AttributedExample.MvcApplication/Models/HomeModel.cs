using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AttributedExample.MvcApplication.Models.Query;

namespace AttributedExample.MvcApplication.Models
{

    public class RunningState
    {
        public RoleType RoleType { get; set; }
    }

    public interface IHomeModel
    {
        IEnumerable<QueryHeaderModel> QueryHeaders { get; }
        IEnumerable<IQueryPanelModel> QueryPanels { get; }
    }

    public class HomeModel : IHomeModel
    {
        public HomeModel(IEnumerable<Lazy<IQueryModel, IQueryModelMetadata>> queryModels, RunningState runningState,
            Func<QueryType, QueryHeaderModel> headerFactory, Func<IQueryModel, IQueryPanelModel> panelFactory)
        {
            QueryHeaders = from i in queryModels where(runningState.RoleType == i.Metadata.RoleType) select headerFactory(i.Metadata.QueryType);
            QueryPanels = from i in queryModels where(runningState.RoleType == i.Metadata.RoleType) select panelFactory(i.Value);
        }

        public IEnumerable<QueryHeaderModel> QueryHeaders { get; private set; }

        public IEnumerable<IQueryPanelModel> QueryPanels { get; private set; }
    }

}