<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Remember.Web.Models.LoginForm>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

    <h2>Login</h2>

    <div>
    <strong>NOTE</strong> Only valid credentials are "test@test.com" with password "test", anything else returns validation error
    
    </div>

    <%=Html.ValidationSummary() %>
    

    <%using (Html.BeginForm())
      { %>

      <div>
      <label for="Email">Email</label>
      <input type="text" name="Email" value="Enter email here" />
      </div>

      <div>
      <label for="Password">Password</label>
      <input type="password" name="Password" value="" />
      </div>

      <div>
      <input type="submit" value="Login" title="Login" />
      </div>

    <%} %>

</asp:Content>
