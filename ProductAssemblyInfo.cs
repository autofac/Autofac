using System;
using System.Security;

/* This file contains attributes that get set on product assemblies
 * but not test assemblies. */

// All code defaults to SecurityTransparent but individual types can be marked differently.
// http://msdn.microsoft.com/en-us/library/dd233102.aspx
[assembly: AllowPartiallyTrustedCallers]
[assembly: CLSCompliant(true)]
