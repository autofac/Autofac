using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Defines a filter provider for filter attributes that performs property injection.
    /// </summary>
    public class AutofacFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacFilterAttributeFilterProvider"/> class.
        /// </summary>
        /// <remarks>
        /// The <c>false</c> constructor parameter passed to base here ensures that attribute instances are not cached.
        /// </remarks>
        public AutofacFilterAttributeFilterProvider() : base(false)
        {
        }

        /// <summary>
        /// Aggregates the filters from all of the filter providers into one collection.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>
        /// The collection filters from all of the filter providers with properties injected.
        /// </returns>
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();
            var lifetimeScope = AutofacDependencyResolver.Current.RequestLifetimeScope;

            if (lifetimeScope != null)
                foreach (var filter in filters)
                    lifetimeScope.InjectProperties(filter.Instance);

            return filters;
        }
    }
}
