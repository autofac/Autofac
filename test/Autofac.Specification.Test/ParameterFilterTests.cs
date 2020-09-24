// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Xunit;

namespace Autofac.Specification.Test
{
    public class ParameterFilterTests
    {
        [Fact]
        public void NamedFindsCorrectParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Equal("named", parameters.Named<string>("value"));
        }

        [Fact]
        public void NamedRequiresName()
        {
            var parameters = new Parameter[0];
            Assert.Throws<ArgumentNullException>(() => parameters.Named<string>(null));
        }

        [Fact]
        public void NamedRequiresParameterEnumerable()
        {
            IEnumerable<Parameter> parameters = null;
            Assert.Throws<ArgumentNullException>(() => parameters.Named<string>("value"));
        }

        [Fact]
        public void NamedUnableToFindParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Throws<InvalidOperationException>(() => parameters.Named<string>("not-found"));
        }

        [Fact]
        public void PositionalFindsCorrectParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Equal("position", parameters.Positional<string>(2));
        }

        [Fact]
        public void PositionalRequiresParameterEnumerable()
        {
            IEnumerable<Parameter> parameters = null;
            Assert.Throws<ArgumentNullException>(() => parameters.Positional<string>(2));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PositionalRequiresPosition(int position)
        {
            var parameters = new Parameter[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => parameters.Positional<string>(position));
        }

        [Fact]
        public void PositionalUnableToFindParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Throws<InvalidOperationException>(() => parameters.Positional<string>(100));
        }

        [Fact]
        public void TypedAsFindsCorrectParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Equal(5, parameters.TypedAs<int>());
        }

        [Fact]
        public void TypedAsRequiresParameterEnumerable()
        {
            IEnumerable<Parameter> parameters = null;
            Assert.Throws<ArgumentNullException>(() => parameters.TypedAs<int>());
        }

        [Fact]
        public void TypedAsUnableToFindParameter()
        {
            var parameters = new Parameter[]
            {
                new TypedParameter(typeof(int), 5),
                new NamedPropertyParameter("prop", "property"),
                new NamedParameter("value", "named"),
                new PositionalParameter(2, "position")
            };

            Assert.Throws<InvalidOperationException>(() => parameters.TypedAs<DivideByZeroException>());
        }
    }
}
