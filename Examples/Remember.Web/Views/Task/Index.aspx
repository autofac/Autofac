<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  Inherits="System.Web.Mvc.ViewPage<IEnumerable<Remember.Model.Task>>" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>Tasks</h2>

    <ul id="taskList">
        <li>This list was created <strong>server side</strong></li>
        <% foreach (var task in Model) { %>
            <li><%= Server.HtmlEncode(task.Title) %></li>
        <% } %>
    </ul>

    <p>
       <a href="javascript:reloadTaskListUsingAjax();" >Load Task List Using JSON API</a>
    </p>


    <script type="text/javascript">
        function reloadTaskListUsingAjax() {
            $.getJSON('/api/task', function (result) {

                var html = '<li>This list was created <strong>client side</strong></li>';
                for (var i = 0; i < result.length; i++) {
                    html += '<li>' + result[i].Title + '</li>';
                }
                $('#taskList').html(html);
            });
            

        }
    
    </script>

</asp:Content>
