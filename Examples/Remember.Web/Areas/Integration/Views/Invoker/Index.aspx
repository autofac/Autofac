<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<h2>ExtensibleActionInvoker Integration Tests</h2>
    <p>This lets you test various aspects of the <code>Autofac.Integration.Mvc.ExtensibleActionInvoker</code> in a runtime environment where it can really work alongside things like the DefaultModelBinder.</p>
    <p>You can enable/disable controller action parameter injection by switching the value in <code>web.config</code>.</p>
    <ul>
        <li><%:Html.ActionLink("Simple Controller Action Parameter Injection", "ParameterInjection", new{ value="StringValue", id= 17}) %></li>
    </ul>
</asp:Content>
