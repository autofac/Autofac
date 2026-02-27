// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Original version from https://github.com/dotnet/aspnetcore/blob/main/src/Shared/ValueStopwatch/ValueStopwatch.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Diagnostics;

namespace Autofac.Diagnostics;

/// <summary>
/// Lightweight stopwatch for timing without heap allocations.
/// </summary>
[ExcludeFromCodeCoverage]
internal struct ValueStopwatch
{
#if !NET7_0_OR_GREATER
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
#endif

    private readonly long _startTimestamp;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    /// <summary>
    /// Gets a value indicating whether the stopwatch has been started.
    /// </summary>
    public bool IsActive => _startTimestamp != 0;

    /// <summary>
    /// Starts a new stopwatch instance.
    /// </summary>
    /// <returns>A running <see cref="ValueStopwatch"/>.</returns>
    public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

    /// <summary>
    /// Computes elapsed time between two timestamps captured from <see cref="Stopwatch.GetTimestamp"/>.
    /// </summary>
    /// <param name="startingTimestamp">The starting timestamp.</param>
    /// <param name="endingTimestamp">The ending timestamp.</param>
    /// <returns>The elapsed time between the timestamps.</returns>
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
    {
#if !NET7_0_OR_GREATER
        var timestampDelta = endingTimestamp - startingTimestamp;
        var ticks = (long)(TimestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
#else
        return Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#endif
    }

    /// <summary>
    /// Gets the elapsed time since the stopwatch started.
    /// </summary>
    /// <returns>The elapsed time.</returns>
    public TimeSpan GetElapsedTime()
    {
        // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
        // So it being 0 is a clear indication of default(ValueStopwatch)
        if (!IsActive)
        {
            throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
        }

        var end = Stopwatch.GetTimestamp();

        return GetElapsedTime(_startTimestamp, end);
    }
}
