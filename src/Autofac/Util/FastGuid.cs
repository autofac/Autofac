// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Autofac.Util;

/// <summary>Helper class to generate GUIDs fast way.</summary>
internal static class FastGuid
{
    private static readonly long BasePart;
    private static long variablePart;

#pragma warning disable CA1810
    static FastGuid()
#pragma warning restore CA1810
    {
        Span<byte> guidData = stackalloc byte[16];
        ref byte reference = ref MemoryMarshal.GetReference(guidData);
        Unsafe.WriteUnaligned(ref reference, Guid.NewGuid());
        variablePart = Unsafe.ReadUnaligned<long>(ref reference);
        BasePart = Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref reference, 8));
    }

    /// <summary>Initializes a new instance of the Guid structure, but faster than Guid.NewGuid().</summary>
    /// <returns>A new GUID object.</returns>
    public static Guid NewGuid()
    {
        return MemoryMarshal.Read<Guid>(MemoryMarshal.Cast<long, byte>([Interlocked.Increment(ref variablePart), BasePart]));
    }
}
