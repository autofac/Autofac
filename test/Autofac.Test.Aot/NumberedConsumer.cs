// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/// <summary>
/// Verifies generated <c>Func</c> factories with value-type and multiple arguments.
/// </summary>
internal sealed class NumberedConsumer
{
    public NumberedConsumer(int number, string text = "none")
    {
        Number = number;
        Text = text;
    }

    public int Number
    {
        get;
    }

    public string Text
    {
        get;
    }
}
