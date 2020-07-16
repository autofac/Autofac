// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
        /// The set of characters on which a line is allowed to wrap.
        /// </summary>
        private static readonly char[] WrapCharacters = new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' };

        /// <summary>
        /// Starts a DOT graph node where the label is an HTML table.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="id">
        /// The node ID. You can connect to this node by this ID.
        /// </param>
        /// <param name="shape">
        /// The shape the node should take, like <c>component</c>, <c>box3d</c>, or <c>plaintext</c>.
        /// </param>
        /// <param name="success">
        /// <see langword="true"/> to indicate the operation was successful; <see langword="false"/> to
        /// highlight the operation as a failure path.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder StartNode(this StringBuilder stringBuilder, Guid id, string shape, bool success)
        {
            stringBuilder.AppendFormat(
                CultureInfo.CurrentCulture,
                "{0} [shape={1},{2}label=<",
                id.NodeId(),
                shape,
                success ? null : "penwidth=3,color=red,");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("<table border='0' cellborder='0' cellspacing='0'>");
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
        public static StringBuilder EndNode(this StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine(">];");
            return stringBuilder;
        }

        /// <summary>
        /// Writes a table header to an HTML table node in a DOT graph.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="service">
        /// A string that will be displayed as a header in the table, should be the name
        /// of the service being resolved.
        /// </param>
        /// <param name="portId">
        /// A <see cref="Guid"/> that uniquely identifies the service instance being
        /// resolved. Used for linking from one request to a specific service.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder AppendServiceRow(this StringBuilder stringBuilder, string service, Guid portId)
        {
            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "<tr><td port='{0}'>", portId.NodeId());
            stringBuilder.Append(service.Encode());
            stringBuilder.Append("</td></tr>");
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
            stringBuilder.Append("<tr><td><font point-size=\"10\">");
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Encode();
            }

            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, format, args);
            stringBuilder.Append("</font></td></tr>");
            stringBuilder.AppendLine();
            return stringBuilder;
        }

        /// <summary>
        /// Writes a table row to an HTML table node in a DOT graph.
        /// </summary>
        /// <param name="stringBuilder">
        /// The <see cref="StringBuilder" /> to which the node should be written.
        /// </param>
        /// <param name="exceptionType">
        /// The full exception type name to display.
        /// </param>
        /// <param name="exceptionMessage">
        /// The message from the exception.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder AppendTableErrorRow(this StringBuilder stringBuilder, string exceptionType, string exceptionMessage)
        {
            stringBuilder.Append("<tr><td><font point-size=\"10\"><b>");
            stringBuilder.Append(exceptionType.Encode());
            stringBuilder.Append("</b>:<br/>\n");
            stringBuilder.Append(exceptionMessage.Wrap().Encode());
            stringBuilder.Append("</font></td></tr>");
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
        /// <param name="label">
        /// The label on the connecting line.
        /// </param>
        /// <param name="bold">
        /// <see langword="true"/> if the text in the cell should be bold; <see langword="false"/> if not.
        /// </param>
        /// <returns>
        /// The <paramref name="stringBuilder" /> for continued writing.
        /// </returns>
        public static StringBuilder ConnectNodes(this StringBuilder stringBuilder, string fromId, string toId, string label, bool bold)
        {
            stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} -> {1} [label=<{2}>", fromId, toId, label.Encode());
            if (bold)
            {
                stringBuilder.Append(",penwidth=3,color=red");
            }

            stringBuilder.AppendLine("]");
            return stringBuilder;
        }

        /// <summary>
        /// HTML-encodes and converts newlines to line break tags in an input string.
        /// </summary>
        /// <param name="input">
        /// The <see cref="string"/> to encode.
        /// </param>
        /// <returns>
        /// The encoded version of <paramref name="input"/>.
        /// </returns>
        public static string Encode(this string input)
        {
            // Pretty stoked the StringComparison overload is only in one of our
            // target frameworks. :(
#if NETSTANDARD2_0
                return HttpUtility.HtmlEncode(input).Replace(Environment.NewLine, "<br/>");
#endif
#if !NETSTANDARD2_0
            return HttpUtility.HtmlEncode(input).Replace(Environment.NewLine, "<br/>", StringComparison.Ordinal);
#endif
        }

        /// <summary>
        /// Line-wraps a long string for display in a graph.
        /// </summary>
        /// <param name="input">
        /// The string to line wrap.
        /// </param>
        /// <returns>
        /// A line-wrapped version of the input.
        /// </returns>
        public static string Wrap(this string input)
        {
            const int maxLineLength = 40;
            var list = new List<string>();
            var lastWrap = 0;
            int currentIndex;
            do
            {
                currentIndex = lastWrap + maxLineLength > input.Length ? input.Length : (input.LastIndexOfAny(WrapCharacters, Math.Min(input.Length - 1, lastWrap + maxLineLength)) + 1);
                if (currentIndex <= lastWrap)
                {
                    currentIndex = Math.Min(lastWrap + maxLineLength, input.Length);
                }

                list.Add(input.Substring(lastWrap, currentIndex - lastWrap).Trim());
                lastWrap = currentIndex;
            }
            while (currentIndex < input.Length);
            return string.Join(Environment.NewLine, list);
        }

        /// <summary>
        /// Creates a unique string ID from a <see cref="Guid"/>
        /// that can be used to identify a node in a DOT graph.
        /// </summary>
        /// <param name="guid">The ID to serialize.</param>
        /// <returns>A string version of the ID.</returns>
        public static string NodeId(this Guid guid)
        {
            return "n" + guid.ToString("N");
        }
    }
}
