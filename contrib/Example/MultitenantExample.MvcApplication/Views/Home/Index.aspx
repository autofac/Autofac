<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MultitenantExample.MvcApplication.Models.IndexModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Multitenant Example
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Multitenant Example</h2>
    <p>Select a tenant ID from the list to change the tenant and view the results of the tenant change. You should see </p>

    <h2>Expectations</h2>
    <ul>
    <li>Tenant 1 has dependency overrides registered as InstancePerDependency.</li>
    <li>Tenant 2 has dependency overrides registered as SingleInstance.</li>
    <li>Tenants 1 and 2 both have custom controllers/service implementations.</li>
    <li>The default tenant has dependency overrides registered as SingleInstance.</li>
    <li>Tenants that don't have overrides (tenant 3) fall back to the application/base dependency.</li>
    </ul>

    <h2>Current MVC Application Values</h2>
    <ul>
    <li>Tenant ID: <%: this.Model.TenantId.ToString() %></li>
    <li>Controller Type: <%: this.Model.ControllerTypeName %></li>
    <li>Dependency Type: <%: this.Model.DependencyTypeName %></li>
    <li>Dependency Instance ID: <%: this.Model.DependencyInstanceId %></li>
    </ul>

    <h2>Current WCF Service Values</h2>
    <ul>
    <li>Tenant ID: <%: this.Model.ServiceInfo.TenantId %></li>
    <li>Service Implementation Type: <%: this.Model.ServiceInfo.ServiceImplementationTypeName %></li>
    <li>Dependency Type: <%: this.Model.ServiceInfo.DependencyTypeName %></li>
    <li>Dependency Instance ID: <%: this.Model.ServiceInfo.DependencyInstanceId %></li>
    </ul>

    <h2>Switch Tenants</h2>
    <ul>
    <li><%= Html.ActionLink("Tenant 1", "Index", new{tenant=1}) %></li>
    <li><%= Html.ActionLink("Tenant 2", "Index", new{tenant=2}) %></li>
    <li><%= Html.ActionLink("Tenant 3", "Index", new{tenant=3}) %></li>
    <li><%= Html.ActionLink("None/Default Tenant", "Index") %></li>
    </ul>
</asp:Content>
