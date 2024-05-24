// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Autofac.Test.CodeGen.Helpers;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Override the output converter so we can filter out files we're not interested in.
        VerifierSettings.RegisterFileConverter<GeneratorDriverRunResult>(ConvertRunResult);
        VerifierSettings.RegisterFileConverter<GeneratorDriver>(ConvertDriver);

        VerifySourceGenerators.Initialize();
    }

    private static ConversionResult ConvertRunResult(GeneratorDriverRunResult target, IReadOnlyDictionary<string, object> context)
    {
        var exceptions = new List<Exception>();
        var targets = new List<Target>();
        foreach (var result in target.Results)
        {
            if (result.Exception is not null)
            {
                exceptions.Add(result.Exception);
            }

            targets.AddRange(result.GeneratedSources.Where(x => x.HintName.EndsWith(".g.cs", StringComparison.Ordinal)).Select(SourceToTarget));
        }

        if (exceptions.Count == 1)
        {
            throw exceptions.First();
        }

        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }

        if (target.Diagnostics.Any())
        {
            var info = new
            {
                target.Diagnostics,
            };
            return new(info, targets);
        }

        return new(null, targets);
    }

    private static ConversionResult ConvertDriver(GeneratorDriver target, IReadOnlyDictionary<string, object> context)
    {
        return ConvertRunResult(target.GetRunResult(), context);
    }

    private static Target SourceToTarget(GeneratedSourceResult source)
    {
        var data = $@"// HintName: {source.HintName}
{source.SourceText}";
        return new("cs", data);
    }
}
