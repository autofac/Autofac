<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Remember.Web.Views.Task.Index" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>Tasks</h2>
    <ol>
        <% foreach (var task in ViewData.Tasks) { %>
            <li><%= Server.HtmlEncode(task.Title) %></li>
        <% } %>
    </ol>
</asp:Content>
