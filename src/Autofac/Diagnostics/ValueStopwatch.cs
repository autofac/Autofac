// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Autofac.Diagnostics;

/// <summary>
/// Lightweight stopwatch with no heap allocations.
/// </summary>
internal readonly struct ValueStopwatch
{
    private readonly long _startTimestamp;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    /// <summary>
    /// Gets the number of elapsed ticks.
    /// </summary>
    public long ElapsedTicks
    {
        get
        {
            if (_startTimestamp == 0)
            {
                return 0;
            }

            return Stopwatch.GetTimestamp() - _startTimestamp;
        }
    }

    /// <summary>
    /// Gets the elapsed duration in milliseconds.
    /// </summary>
    public double ElapsedMilliseconds => ElapsedTicks * 1000.0 / Stopwatch.Frequency;

    /// <summary>
    /// Starts a new stopwatch instance.
    /// </summary>
    /// <returns>A running <see cref="ValueStopwatch"/>.</returns>
    public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());
}
