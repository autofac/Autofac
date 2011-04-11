<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Index</title>
</head>
<body>
<p>Select an exception type to throw from the controller action:</p>
<ul>
<%
	foreach (string typeName in this.ViewBag.ExceptionTypes)
	{
%>
<li><%= Html.ActionLink(Type.GetType(typeName).FullName, "ThrowControllerException", new { exceptionTypeName = typeName }) %></li>
<%
	}
%>
</ul>
<p>If Enterprise Library registration/setup works, you'll see a "friendly" error page; if not, you'll get the Yellow Screen of Death.</p>
</body>
</html>
