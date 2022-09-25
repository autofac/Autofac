// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration;

/// <summary>
/// Indicates that a registration source operates per-scope, and should not provide
/// registrations for requests from child scopes.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Need a marker interface of per-scope registration sources")]
public interface IPerScopeRegistrationSource
{
}
