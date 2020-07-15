// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Diagnostics;
using Xunit;

namespace Autofac.Test.Core.Diagnostics
{
    public class DotStringExtensionsTests
    {
        [Fact]
        public void Wrap_ShortString()
        {
            var input = "This is short.";
            var actual = DotStringExtensions.Wrap(input);
            Assert.Equal(input, actual);
        }

        [Fact]
        public void Wrap_LongString()
        {
            var input = "This is a very long string. It should line wrap so it isn't this long, but that's why this test is here - to see if it line wraps or not. If it doesn't line wrap, then the function isn't working.";
            var expected = "This is a very long string. It should\nline wrap so it isn't this long, but\nthat's why this test is here - to see if\nit line wraps or not. If it doesn't line\nwrap, then the function isn't working.".Replace("\n", Environment.NewLine);
            var actual = DotStringExtensions.Wrap(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Wrap_LongWord()
        {
            var input = "Thisisunrealisticbutislongerthantheallowedlinelengthwithnowheretotbreaksowehavetoforceit.";
            var expected = "Thisisunrealisticbutislongerthantheallow\nedlinelengthwithnowheretotbreaksowehavet\noforceit.".Replace("\n", Environment.NewLine);
            var actual = DotStringExtensions.Wrap(input);
            Assert.Equal(expected, actual);
        }
    }
}
