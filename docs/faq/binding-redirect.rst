==============================================================
Why don't all Autofac packages target the latest Autofac core?
==============================================================

Autofac has a lot of :doc:`integration packages <../integration/index>` and extensions. You'll find that not all of these packages directly reference the very latest of Autofac core.

**Unless there's a technical reason to increase the minimum version requirement for one of these packages, we'll keep the version unchanged.**

We do this because, generally speaking, we don't want to force anyone to update their version of Autofac core unless they absolutely must. This is a fairly good practice for any library set - if a person doesn't *have* to take an update, you shouldn't *force* them to do so.

**What this results in is the need to use assembly binding redirects.** `This is the official supported way <https://msdn.microsoft.com/en-us/library/vstudio/2fc472t2.aspx>`_ to tell the .NET runtime that you need to redirect requests for one version of a strong-named assembly to a later version of that assembly. This is common enough that both NuGet and Visual Studio will, in many cases, automatically add these to your configuration files.

Here's an example of what assembly binding redirects look like:

.. sourcecode:: xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <runtime>
            <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
                <dependentAssembly>
                    <assemblyIdentity name="Autofac"
                                      publicKeyToken="17863af14b0044da"
                                      culture="neutral" />
                    <bindingRedirect oldVersion="0.0.0.0-3.5.0.0"
                                     newVersion="3.5.0.0" />
                </dependentAssembly>
                <dependentAssembly>
                    <assemblyIdentity name="Autofac.Extras.CommonServiceLocator"
                                      publicKeyToken="17863af14b0044da"
                                      culture="neutral" />
                    <bindingRedirect oldVersion="0.0.0.0-3.1.0.0"
                                     newVersion="3.1.0.0" />
                </dependentAssembly>
                <dependentAssembly>
                    <assemblyIdentity name="Autofac.Extras.Multitenant"
                                      publicKeyToken="17863af14b0044da"
                                      culture="neutral" />
                    <bindingRedirect oldVersion="0.0.0.0-3.1.0.0"
                                     newVersion="3.1.0.0" />
                </dependentAssembly>
                <dependentAssembly>
                    <assemblyIdentity name="Autofac.Integration.Mvc"
                                      publicKeyToken="17863af14b0044da"
                                      culture="neutral" />
                    <bindingRedirect oldVersion="0.0.0.0-3.3.0.0"
                                     newVersion="3.3.0.0" />
                </dependentAssembly>
        </runtime>
    </configuration>

Assembly binding redirects are an unfortunate side-effect of `assembly strong-naming <https://msdn.microsoft.com/en-us/library/wd40t7ad.aspx>`_. You don't need binding redirects if assemblies aren't strong-named; but there are some environments that require assemblies to be strong-named, so Autofac continues to strong-name assemblies.

**Even if Autofac always kept every reference up to date, you would still not escape assembly binding redirects.** Autofac integration packages, like :doc:`the Web API integration <../integration/webapi>`, rely on other strong-named packages that then have their own dependencies. For example, the `Microsoft Web API packages <http://www.nuget.org/packages/Microsoft.AspNet.WebApi.Client/>`_ rely on `Newtonsoft.Json <http://www.nuget.org/packages/Newtonsoft.Json/>`_ and they don't always keep up with the latest version. They instead specify a minimum compatible version. *If you update your local version of Newtonsoft.Json... you get a binding redirect.*

**Rather than try to fight against binding redirects, it may be better to just accept them as a "cost of doing business" in the .NET world.** They do add a bit of "clutter" to the application configuration file, but until we can remove strong-naming from the equation, it's an inescapable necessity.