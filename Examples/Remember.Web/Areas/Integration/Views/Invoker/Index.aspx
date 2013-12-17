<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<h2>ExtensibleActionInvoker Integration Tests</h2>
    <p>This lets you test various aspects of the <code>Autofac.Integration.Mvc.ExtensibleActionInvoker</code> in a runtime environment where it can really work alongside things like the DefaultModelBinder.</p>
    <ul>
        <li><%:Html.ActionLink("Simple Controller Action Parameter Injection", "ParameterInjection", new{ value="StringValue", id= 17}) %></li>
        <li><%:Html.ActionLink("Multiple File Upload", "FileUpload") %></li>
    </ul>
</asp:Content>
