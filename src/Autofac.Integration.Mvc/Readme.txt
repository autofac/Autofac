To build this assembly, you will need the ASP.NET MVC techology preview or later installed.

The integration requires that you add:

  <add name="AutofacHttpModule"
       type="Autofac.Integration.Mvc.AutofacHttpModule, Autofac.Integration.Mvc"/>

...to the HTTP module list in your Web.config.
