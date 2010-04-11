<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  Inherits="System.Web.Mvc.ViewPage<IEnumerable<Remember.Model.Task>>" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>Tasks</h2>
    <ol>
        <% foreach (var task in Model) { %>
            <li><%= Server.HtmlEncode(task.Title) %></li>
        <% } %>
    </ol>
</asp:Content>
