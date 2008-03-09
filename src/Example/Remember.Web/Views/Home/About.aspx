<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Remember.Web.Views.Home.About" %>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h1>
        About MVC Framework</h1>
    <p>
        The MVC framework includes the following components:
    </p>
    <h3>
        Models</h3>
    <p>
        Models. Model objects are the parts of the application that implement the domain
        logic. Often, model objects also retrieve and store model state in a database. For
        example, a Product object might retrieve information from a database, operate on
        it, and then write updated information back to a Products table in SQL Server. note
        In small applications, the model is often a conceptual separation rather than a
        physical one. For example, if the application only reads a data set and sends it
        to the view, the application does not have a physical model layer and associated
        classes. In that case, the data set takes on the role of a model object.
    </p>
    <h3>
        Views</h3>
    <p>
        Views are the components that display the application's user interface (UI). Typically,
        this UI is created from the model data. An example would be an edit view of a Products
        table that displays text boxes, drop-down lists, and check boxes based on the current
        state of a Product object.
    </p>
    <h3>
        Controllers</h3>
    <p>
        Controllers are the components that handle user interaction, manipulate the model,
        and ultimately select a view to render that displays UI. In an MVC application,
        the view only displays information; the controller handles and responds to user
        input and interaction. For example, the controller handles query-string values,
        and passes these values to the model, which in turn queries the database by using
        the values.
    </p>
</asp:Content>
