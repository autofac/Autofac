// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

/// <summary>
/// Verifies resolution with an explicit <see cref="TypedParameter"/>.
/// </summary>
internal sealed class ParameterizedConsumer
{
    public ParameterizedConsumer(string value)
    {
        Value = value;
    }

    public string Value
    {
        get;
    }
}
