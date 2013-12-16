<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>Multiple File Upload with ExtensibleActionInvoker</h2>

	<p>This view tests/demonstrates the use of multiple file uploads while using the ExtensibleActionInvoker.</p>

	<%
	// This tests issue 430.
    using (Html.BeginForm("ProcessFileUploads", "Invoker", FormMethod.Post, new{ enctype = "multipart/form-data" }))
    { %>
	<fieldset>
		<legend>Upload multiple files</legend>
		<div>
			<label for="file1">File 1:</label>
			<input type="file" name="files" id="file1" />
		</div>
		<div>
			<label for="file2">File 2:</label>
			<input type="file" name="files" id="file2" />
		</div>
		<div>
			<input type="submit" />
		</div>
	</fieldset>
	<% } %>
</asp:Content>
