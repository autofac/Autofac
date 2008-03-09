<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Remember.Web.Views.Home.Index" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>
        Introduction to ASP.NET MVC Application!</h2>
    <p>
        The Model View Controller (MVC) architectural pattern separates an application into
        three main components: the model, the view, and the controller. The ASP.NET MVC
        framework provides an alternative to the ASP.NET Web-forms pattern for creating
        MVC-based Web applications. The ASP.NET MVC framework is a lightweight, highly testable
        presentation framework that (as with Web-forms-based applications) is integrated
        with existing ASP.NET features, such as master pages and membership-based authentication.
        The MVC framework is defined in the System.Web.Mvc namespace and is a fundamental,
        supported part of the System.Web namespace.
    </p>
    <p>
        The MVC pattern helps you to create applications that separate the different aspects
        of the application (input logic, business logic, and UI logic), while providing
        a loose coupling between these elements. The pattern specifies where each kind of
        logic should exist in the application. The UI logic belongs in the view. Input logic
        belongs in the controller. Business logic belongs in the model. This separation
        helps you manage complexity when you build an application, because it enables you
        to focus on one aspect of the implementation at a time. For example, you can focus
        on the view without depending on the business logic. You can read more about ASP.NET
        MVC Application <a href="http://quickstarts.asp.net/3-5-extensions/mvc/MVCOverview.aspx">
        here</a>.
    </p>
</asp:Content>
