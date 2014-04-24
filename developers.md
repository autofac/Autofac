<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">

<html>
	<head>
		<title>Readme for Autofac Developers</title>
		<style type="text/css">
		<!--
		body
		{
			font-family: Calibri;
			background-color: white;
			color: black;
			font-size: 11pt;
			padding-left: 10pt;
			padding-right: 10pt;
		}
		h1, h2, h3, h4, h5, h6
		{
			font-family: Cambria;
		}
		h1, h2, h3, h4
		{
			font-weight: bold;
		}
		h2, h3, h4
		{
			color: #4F81BD;
		}
		h3, h4, h5, h6
		{
			font-size: 11pt;
		}
		h4, h6
		{
			font-style: italic;
		}
		h5, h6
		{
			color: #17365D;
			font-weight: normal;
		}
		h1
		{
			font-size: 14pt;
			color: #365F91;
			border-bottom: 1px solid black;
			margin-top: 26pt;
		}
		h2
		{
			font-size: 13pt;
		}
		h1.title
		{
			font-size: 26pt;
			font-weight: normal;
			color: #17365D;
			border-bottom: 1px solid #4F81BD;
		}
		table
		{
			border-collapse: collapse;
			border: 1px solid gray;
		}
		table thead th
		{
			padding: 2pt 4pt 2pt 4pt;
			border-bottom: 1px solid gray;
		}
		table tbody td
		{
			padding: 2pt 4pt 2pt 4pt;
			vertical-align: top;
			border-bottom: 1px solid lightgray;
		}
		table tbody tr:last-child td
		{
			border-bottom: 1px solid gray;
		}
		div.callout
		{
			margin: 5pt 20pt 5pt 20pt;
			padding-left: 5pt;
			padding-right: 5pt;
			border: 1px solid black;
			background-color: lightgray;
		}
		.footnote
		{
			font-size: 8pt;
		}
		.footnote p
		{
			text-indent: -10pt;
			padding-left: 10pt;
		}
		-->
		</style>
	</head>
	<body>
		<h1 class='title'>Readme for Autofac Developers</h1>
		<p>This document explains the developer setup and build execution for Autofac.</p>

		<h1>Developer Environment</h1>
		<ul>
			<li>Visual Studio 2013 Premium/Ultimate. (Include the <em>Windows Phone 8.0 SDK</em> feature when installing.) This will give you:
				<ul>
					<li>.NET 4.5</li>
					<li>WCF RIA Services</li>
					<li>Portable Class Library tooling</li>
					<li>FxCop</li>
					<li>SQL Server Express</li>
				</ul>
			</li>
			<li><strong>All</strong> of the latest .NET, VS, and SQL patches through Microsoft Update.</li>
			<li><strong>All</strong> of the latest VS updates (stable/RTM, not RC) through VS Extension Manager.</li>
			<li><a href="http://nunit.org/index.php?p=vsTestAdapter&amp;r=2.6.3">NUnit Test Adapter for VS11</a> (optional - to run unit tests inside Visual Studio)</li>
		</ul>

		<h1>Building the Project</h1>
		<p>Developer build:<br />
			<code>msbuild default.proj</code></p>

		<p>Production/Release build:<br />
			<code>msbuild default.proj /p:Production=true</code></p>

		<p>The <strong>developer build</strong> will...</p>
		<ul>
			<li>Clean all build artifacts.</li>
			<li>Build the solution.</li>
			<li>Execute the unit tests.</li>
			<li>Run code analysis.</li>
		</ul>

		<p>The <strong>production/release build</strong> will do everything in the developer build <em>plus</em>...</p>
		<ul>
			<li>Create zip packages for distribution.</li>
			<li>Create NuGet packages for distribution.</li>
			<li>Build the compiled API help documentation.</li>
		</ul>

		<p><strong>Note for developers:</strong> If you are working on the Autofac core, there is also a project in Core/Tests/Autofac.Tests.AppCert that should be built/run separately to verify changes will pass Windows App Store certification. This build is not chained into the standard developer build since it takes time to run. <a href="Core/Tests/Autofac.Tests.AppCert/readme.html">There is a readme in that folder explaining more about how to run that build and assess results</a>.</p>

		<p>Production package versions are centrally controlled through the <code>PackageVersions.proj</code>. Documentation in that
		file explains how to use it. Before releasing new versions for consumption, be sure to update the appropriate version(s).</p>

		<h1>Updating the API Documentation Site</h1>
		<p>The API docs are viewable at <a href="http://api.autofac.org">http://api.autofac.org</a>. This is hosted on GitHub pages in the <a href="https://github.com/autofac/autofac.github.com">https://github.com/autofac/autofac.github.com</a> repository.</p>
		<ol>
			<li>Build the API documentation.</li>
			<li>Update the contents in the <code>/apidoc</code> folder with the new docs (add/remove/update).</li>
			<li>Make sure the index page in the <code>/apidoc</code> is <code>index.html</code> - lower case, full <code>html</code> extension. (By default, Sandcastle makes it <code>Index.htm</code> which doesn't work.)</li>
		</ol>

		<h1>Updating the User Documentation Site</h1>
		<p>User documentation is viewable at <a href="https://docs.autofac.org">https://docs.autofac.org</a> (a CNAME to <a href="https://autofac.readthedocs.org">https://autofac.readthedocs.org</a>). It is stored in the <code>/docs</code> folder in this source repo.</p>
		<p>To build the docs and see them locally, you need to follow the <a href="https://docs.readthedocs.org/en/latest/getting_started.html">Getting Started</a> docs on Read The Docs so you get Python and Sphinx installed.</p>
		<p>The docs are written in <a href="http://sphinx-doc.org/rest.html">reStructuredText</a>, which is very similar to Markdown but not quite. Check that out for a primer.</p>
		<p>Updates to the documentation checked into the <code>/docs</code> folder will automatically propagate to Read The Docs. No build or separate push is required.</p>
	</body>
</html>