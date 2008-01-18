// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    /// <summary>
    /// Utilities to handle common activation scenarios
    /// </summary>
    public static class ActivatingHandler
    {
        /// <summary>
        /// Inject properties from the context into the newly
        /// activated instance.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Autofac.ActivatingEventArgs"/> instance containing the event data.</param>
        public static void InjectProperties(object sender, ActivatingEventArgs e)
        {
            Enforce.ArgumentNotNull(sender, "sender");
            Enforce.ArgumentNotNull(e, "e");
            e.Context.InjectProperties(e.Instance);
        }

        /// <summary>
        /// Inject properties from the context into the newly
        /// activated instance unless they're non null on the instance.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Autofac.ActivatingEventArgs"/> instance containing the event data.</param>
        public static void InjectUnsetProperties(object sender, ActivatingEventArgs e)
        {
            Enforce.ArgumentNotNull(sender, "sender");
            Enforce.ArgumentNotNull(e, "e");
            e.Context.InjectUnsetProperties(e.Instance);
        }
    }
}
