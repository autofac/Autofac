﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using Xunit.Abstractions;

namespace Autofac.Specification.Test.Registration;

public class AssemblyScanningPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public AssemblyScanningPerformanceTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void MeasurePerformance()
    {
        var builder = new ContainerBuilder();
        for (var i = 0; i < 10_000; i++)
        {
            // Just to simulate a lot of "scanning" with few (in this case zero) "hits"
            builder.RegisterAssemblyTypes(GetType().Assembly).Where(x => false);
        }

        var stopwatch = Stopwatch.StartNew();
        builder.Build().Dispose();

        // Should take no more than ~500ms
        _output.WriteLine(stopwatch.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }
}
