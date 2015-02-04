// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Determines how the parameters of the delegate type are passed on
    /// to the generated Resolve() call as Parameter objects.
    /// </summary>
    public enum ParameterMapping
    {
        /// <summary>
        /// Chooses parameter mapping based on the factory type.
        /// For Func-based factories this is equivalent to ByType, for all
        /// others ByName will be used.
        /// </summary>
        Adaptive,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as NamedParameters based on the parameter
        /// names in the delegate type's formal argument list.
        /// </summary>
        ByName,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as TypedParameters based on the parameter
        /// types in the delegate type's formal argument list.
        /// </summary>
        ByType,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as PositionalParameters based on the parameter
        /// indices in the delegate type's formal argument list.
        /// </summary>
        ByPosition
    };
}