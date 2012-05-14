using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyCompany("Autofac Project - http://autofac.org")]
[assembly: AssemblyProduct("Autofac")]
[assembly: AssemblyCopyright("Copyright © 2011 Autofac Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// Use .NET 4.0 security rules.
[assembly: SecurityRules(SecurityRuleSet.Level2)]

// All code defaults to SecurityTransparent but individual
// types can be marked differently.
// http://msdn.microsoft.com/en-us/library/dd233102.aspx
[assembly: AllowPartiallyTrustedCallers]

#if !WINDOWS_PHONE
[assembly: ComVisible(false)]
#endif
