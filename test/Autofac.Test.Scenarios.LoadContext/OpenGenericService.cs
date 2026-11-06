// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace A;

// The constraint puts a type from this assembly in the cached constraints as well as the interfaces.
public class OpenGenericService<T> : IOpenGenericService<T>
    where T : Service1
{
}
