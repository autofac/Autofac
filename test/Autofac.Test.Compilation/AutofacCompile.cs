// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Autofac.Test.Compilation
{
    /// <summary>
    /// Allows a block of (simple) autofac container creation code to be compiled, and assert on the warnings that we get.
    /// </summary>
    public class AutofacCompile
    {
        private string? _body;

        private readonly List<MetadataReference> _references = new List<MetadataReference>
        {
            // Bring in the appropriate SDK package
            MetadataReference.CreateFromFile(Assembly.Load(typeof(ContainerBuilder).Assembly.GetReferencedAssemblies().First()).Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ContainerBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AutofacCompile).Assembly.Location)
        };

        public AutofacCompile Body(string body)
        {
            _body = body;

            return this;
        }

        public AutofacCompile AssertWarningContainsKeywords(params string[] expectedKeyWords)
        {
            var messages = GetMessages();
            var warnings = messages.Where(x => x.Severity == DiagnosticSeverity.Warning).Select(x => x.GetMessage()).ToList();

            Assert.True(warnings.Count > 0);

            var firstWarning = warnings.First();

            foreach (var expected in expectedKeyWords)
            {
                Assert.Contains(expected, firstWarning);
            }

            return this;
        }

        public AutofacCompile AssertNoWarnings()
        {
            var messages = GetMessages();
            var warnings = messages.Where(x => x.Severity == DiagnosticSeverity.Warning || x.Severity == DiagnosticSeverity.Error).ToList();

            Assert.Empty(warnings);

            return this;
        }

        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable);

        protected IEnumerable<Diagnostic> GetMessages()
        {
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(Render(), parseOptions);

            var compilation = CSharpCompilation.Create("test.dll", new[] { syntaxTree }, _references, DefaultCompilationOptions);

            return compilation.GetDiagnostics();
        }

        private string Render()
        {
            return @$"

                using Autofac;

                namespace Autofac.Test.Compilation {{

                    public class TestClass
                    {{
                        public static void Run()
                        {{
                            {_body}
                        }}
                    }}

                }}

            ";
        }
    }
}
