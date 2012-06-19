#if !PORTABLE
using System.Security;

// Use .NET 4.0 security rules.
[assembly: SecurityRules(SecurityRuleSet.Level2)]

// All code defaults to SecurityTransparent but individual types can be marked differently.
// http://msdn.microsoft.com/en-us/library/dd233102.aspx
[assembly: AllowPartiallyTrustedCallers]
#endif
