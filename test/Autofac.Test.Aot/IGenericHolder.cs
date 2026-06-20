// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Open generic service contract used to verify open generic resolution under AOT.
/// </summary>
/// <typeparam name="T">The held type.</typeparam>
internal interface IGenericHolder<T>
{
}
