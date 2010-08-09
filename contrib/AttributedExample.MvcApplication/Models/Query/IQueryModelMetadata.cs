using System.ComponentModel.Composition;

namespace AttributedExample.MvcApplication.Models.Query
{
    public interface IQueryModelMetadata
    {
        RoleType RoleType { get; }
        QueryType QueryType { get; }

    }

    public class QueryModelMetadataAttribute : ExportAttribute, IQueryModelMetadata
    {
        public QueryModelMetadataAttribute(RoleType roleType, QueryType queryType) : base(typeof(IQueryModel))
        {
            RoleType = roleType;
            QueryType = queryType;
        }

        #region IQueryModelMetadata Members

        public RoleType RoleType { get; private set; }
        public QueryType QueryType { get; private set; }

        #endregion
    }
}