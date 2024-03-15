// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util;

/// <summary>Helper class to generate GUIDs fast way.</summary>
internal static class FastGuid
{
    private static readonly byte[] BasePart = Guid.NewGuid().ToByteArray().Skip(8).ToArray();
    private static long variablePart = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

    /// <summary>Initializes a new instance of the Guid structure, but faster than Guid.NewGuid().</summary>
    /// <returns>A new GUID object.</returns>
    public static Guid NewGuid()
    {
        var v = Interlocked.Increment(ref variablePart);
        return new((int)(v & 0xFFFFFFFF), (short)(v >> 32), (short)(v >> 48), BasePart);
    }
}
