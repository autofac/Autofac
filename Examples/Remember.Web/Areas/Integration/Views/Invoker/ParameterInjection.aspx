<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<h2>Controller Action Parameter Injection</h2>
    <p>This checks how controller action parameters are handled/model-bound based on their presence, absence, and whether or not parameter injection is enabled.</p>
    <p>The following inputs (and currently values) are available:</p>
    <ul>
        <li>System.String <code>value</code>: <%:this.ViewData["Value"] %></li>
        <li>System.Int32 <code>id</code>: <%:this.ViewData["Id"] %></li>
        <li>The injected dependency <strong><%:this.ViewData["Resolved"] %></strong>.</li>
        <li>The unregistered/unresolved dependency <strong><%:this.ViewData["NotRegistered"] %></strong>.</li>
    </ul>

</asp:Content>
