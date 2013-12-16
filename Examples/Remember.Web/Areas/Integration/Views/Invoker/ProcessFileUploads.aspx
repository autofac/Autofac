<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<string>>" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<p>You uploaded:</p>
<ul>
	<% foreach(var name in this.Model) { %>
	<li><%: name %></li>
	<% } %>
</ul>

</asp:Content>