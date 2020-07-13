// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
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
using System.Globalization;
using System.Text;
using System.Web;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Extension methods for building DOT graph format strings.
    /// </summary>
    internal static class DotStringExtensions
    {
        /// <summary>
        /// Starts a DOT graph node where the label is an HTML table.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="id">
        /// The node ID. You can connect to this node by this ID.
        /// </param>
        /// <param name="border">
        /// Size of the table border.
        /// </param>
        /// <param name="cellBorder">
        /// Size of the border around individual cells.
        /// </param>
        /// <param name="cellPadding">
        /// Space between the text in the cell and the cell wall.
        /// </param>
        /// <param name="cellSpacing">
        /// Space between the cells.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder StartTableNode(this StringBuilder stringBuilder, string id, int border = 0, int cellBorder = 0, int cellPadding = 5, int cellSpacing = 0)
        {
            stringBuilder.AppendFormat(
                CultureInfo.CurrentCulture,
                "{0} [shape=plaintext,label=<<table border='{1}' cellborder='{2}' cellpadding='{3}' cellspacing='{4}'>",
                id,
                border,
                cellBorder,
                cellPadding,
                cellSpacing);
            stringBuilder.AppendLine();
            return stringBuilder;
        }

        /// <summary>
        /// Ends a DOT graph node where the label is an HTML table.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> with the node to close.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder EndTableNode(this StringBuilder stringBuilder)
        {
            stringBuilder.Append("</table>>];");
            stringBuilder.AppendLine();
            return stringBuilder;
        }

        /// <summary>
        /// Writes a table row to an HTML table node in a DOT graph.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="format">
        /// A string into which the arguments will be formatted.
        /// </param>
        /// <param name="args">
        /// The arguments to HTML encode and put into the format string.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder AppendTableRow(this StringBuilder stringBuilder, string format, params string[] args)
        {
            stringBuilder.Append("<tr><td align='left'>");
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = HttpUtility.HtmlEncode(args[i]).NewlineReplace("<br/>");
            }

            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, format, args);
            stringBuilder.Append("</td></tr>");
            return stringBuilder;
        }

        /// <summary>
        /// Writes a table row to an HTML table node in a DOT graph.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="bold">
        /// <see langword="true"/> if the text in the cell should be bold; <see langword="false"/> if not.
        /// </param>
        /// <param name="id">
        /// The cell/row ID. You can connect to this cell by this ID.
        /// </param>
        /// <param name="format">
        /// A string into which the arguments will be formatted.
        /// </param>
        /// <param name="args">
        /// The arguments to HTML encode and put into the format string.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder AppendTableRow(this StringBuilder stringBuilder, bool bold, string id, string format, params string[] args)
        {
            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "<tr><td align='left' port='{0}'>", id);
            if (bold)
            {
                stringBuilder.Append("<b>");
            }

            for (var i = 0; i < args.Length; i++)
            {
                args[i] = HttpUtility.HtmlEncode(args[i]).NewlineReplace("<br/>");
            }

            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, format, args);
            if (bold)
            {
                stringBuilder.Append("</b>");
            }

            stringBuilder.Append("</td></tr>");
            stringBuilder.AppendLine();
            return stringBuilder;
        }

        /// <summary>
        /// Writes a DOT graph connection between two named nodes.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="fromId">
        /// The ID of the node where the connection originates.
        /// </param>
        /// <param name="toId">
        /// The ID of the node where the connection ends.
        /// </param>
        /// <param name="bold">
        /// <see langword="true"/> if the text in the cell should be bold; <see langword="false"/> if not.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder ConnectNodes(this StringBuilder stringBuilder, string fromId, string toId, bool bold)
        {
            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} -> {1}", fromId, toId);
            if (bold)
            {
                stringBuilder.Append(" [penwidth=3]");
            }

            stringBuilder.AppendLine();
            return stringBuilder;
        }

        /// <summary>
        /// Replaces the environment newline character with something else.
        /// </summary>
        /// <param name="input">
        /// The string with characters to replace.
        /// </param>
        /// <param name="newlineReplacement">
        /// The content that should replace newlines.
        /// </param>
        /// <returns>
        /// The <paramref name="input"/> with newlines replaced with the specified content.
        /// </returns>
        public static string NewlineReplace(this string input, string newlineReplacement)
        {
            // Pretty stoked the StringComparison overload is only in one of our
            // target frameworks. :(
#if NETSTANDARD2_0
                return input.Replace(Environment.NewLine, newlineReplacement);
#endif
#if !NETSTANDARD2_0
            return input.Replace(Environment.NewLine, newlineReplacement, StringComparison.Ordinal);
#endif
        }
    }
}
