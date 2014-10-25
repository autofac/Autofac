===========================================================
Why aren't my assemblies getting scanned after IIS restart?
===========================================================

Sometimes you want to use the :doc:`assembly scanning <../register/scanning>` mechanism to load up plugins in IIS hosted applications.

When hosting applications in IIS all assemblies are loaded into the ``AppDomain`` when the application first starts, but **when the AppDomain is recycled by IIS the assemblies are then only loaded on demand.**

To avoid this issue use the `GetReferencedAssemblies() <http://msdn.microsoft.com/en-us/library/system.web.compilation.buildmanager.getreferencedassemblies.aspx>`_ method on `System.Web.Compilation.BuildManager <http://msdn.microsoft.com/en-us/library/system.web.compilation.buildmanager.aspx>`_ to get a list of the referenced assemblies instead:

.. sourcecode:: csharp

    var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>();

That will force the referenced assemblies to be loaded into the ``AppDomain`` immediately making them available for module scanning.

Alternatively, rather than using ``AppDomain.CurrentDomain.GetAssemblies()`` for scanning, **manually load the assemblies** from the filesystem. Doing a manual load forces them into the ``AppDomain`` so you can start scanning.
