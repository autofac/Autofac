<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="MvcShareTrader.Views.Home.Index" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

    <h2>Portfolio</h2>
    <p>Value = <%= ViewData.ToString("C") %></p>

</asp:Content>

