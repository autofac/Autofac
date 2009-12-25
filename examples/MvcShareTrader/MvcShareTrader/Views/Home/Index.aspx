<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcShareTrader.Models.Portfolio>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

    <h2>Portfolio</h2>
    <p>Value = <%= Model.Value.ToString("C") %></p>

</asp:Content>

