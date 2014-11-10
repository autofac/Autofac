===============================================================================
Why are "old versions" of the framework (e.g., System.Core 2.0.5.0) referenced?
===============================================================================

Autofac (as of 3.0) is a **Portable Class Library** that targets multiple platforms.

As a Portable Class Library, if you open up Autofac in Reflector, dotPeek, or other like tools, you'll see references to version 2.0.5.0 of various system libraries. Version 2.0.5.0 is, in fact, the Silverlight version of the .NET framework. *This is expected and is not a problem.* At runtime everything pans out. Autofac will correctly bind to the framework version you're using - be it .NET 4.5, Silverlight, or Windows Phone. `You can read more about Portable Class Libraries on MSDN. <http://msdn.microsoft.com/en-us/library/gg597391.aspx>`_

You may encounter an exception that looks something like this when using Autofac as a Portable Class Library:

.. sourcecode:: csharp

    Test 'MyNamespace.MyFixture.MyTest' failed: System.IO.FileLoadException : Could not load file or assembly 'System.Core, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e, Retargetable=Yes' or one of its dependencies. The given assembly name or codebase was invalid. (Exception from HRESULT: 0x80131047)
        at Autofac.Builder.RegistrationData..ctor(Service defaultService)
        at Autofac.Builder.RegistrationBuilder`3..ctor(Service defaultService, TActivatorData activatorData, TRegistrationStyle style)
        at Autofac.RegistrationExtensions.RegisterInstance[T](ContainerBuilder builder, T instance)
        MyProject\MyFixture.cs(49,0): at MyNamespace.MyFixture.MyTest()

**Make sure your .NET framework is patched.** Microsoft released patches to .NET to allow Portable Class Libraries to properly find the appropriate runtime (`KB2468871 <http://support.microsoft.com/kb/2468871>`_). If you are seeing the above exception (or something like it), it means you're missing the latest .NET framework patches.

`This blog entry has a good overview <http://www.paraesthesia.com/archive/2013/03/29/portable-class-library-answers.aspx>`_ of these and other things you might see when you use Portable Class Libraries.