﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Autofac.Test.CodeGen.Helpers;

internal static class CompilationVerifier
{
    public static Task Verify(string source)
    {
        var compilation = Create(source);

        var generator = new DelegateRegisterGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Once through for the generated code diagnostics, where we update the compilation.
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);

        Assert.NotEmpty(outputCompilation.SyntaxTrees);
        Assert.Empty(outputDiagnostics);

        return Verifier.Verify(driver)
                       .UseDirectory(Path.Combine(AttributeReader.GetProjectDirectory(), "Snapshots"));
    }

    private static CSharpCompilation Create(string source)
        => CSharpCompilation.Create(
            "compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ContainerBuilder).GetTypeInfo().Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}
