// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac;

// AOT/trim WARNING fixture - see the .csproj header for the full rationale.
//
// Each statement below calls an Autofac API that is annotated [RequiresDynamicCode]
// or [RequiresUnreferencedCode]. With IsAotCompatible=true the trim/AOT analyzers
// run and emit the IL codes noted in the comments. The 'VerifyAotWarnings' target in
// default.proj builds this project and asserts those codes are present in the output;
// if an annotation is ever lost, the corresponding warning disappears and the target
// fails. The expected codes are duplicated in that target - keep them in sync.
var builder = new ContainerBuilder();

// IL3050 (RequiresDynamicCode): open generic registration constructs closed types
// at runtime via MakeGenericType.
builder.RegisterGeneric(typeof(GenericFixture<>));

// IL3050 (RequiresDynamicCode): open generic decorator.
builder.RegisterGenericDecorator(typeof(GenericFixture<>), typeof(GenericFixture<>));

// IL2026 (RequiresUnreferencedCode): assembly type scanning.
builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());

// IL2026 (RequiresUnreferencedCode): assembly module scanning.
builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

using var container = builder.Build();
return 0;

/// <summary>
/// Open generic type used purely to trigger the open-generic registration warnings.
/// </summary>
/// <typeparam name="T">Unused type parameter.</typeparam>
internal sealed class GenericFixture<T>
{
}
