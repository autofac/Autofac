<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    About the Multitenant Demo
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>About</h2>
    <p>This demo illustrates usage of Autofac multitenancy in an ASP.NET MVC environment. The same general principles apply in a web forms environment since the setup all occurs at application startup.</p>

    <p>The mechanism for determining tenant in this example is the "tenant" querystring parameter, however, in practice it could be determined by host header, a claim on the authenticated user's principal, a configuration value, or any other method you so choose.</p>

    <p>This application also serves as a client for the example multitenant WCF service. Switching the tenant in the UI will also switch the context in which the WCF request is made.</p>
</asp:Content>
