// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Autofac.CodeGen;

/// <summary>
/// Autogenerates generic `Register` delegate resolve methods.
/// </summary>
[Generator]
public class DelegateRegisterGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Change this number to adjust how many generic arguments we support.
    /// </summary>
    private const int NumberOfGenericArgs = 10;

    private const int SpacesPerIndent = 4;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Set up an incremental generator that regenerates when the 'RegistrationExtensions' class changes.
        // Capture the INamedTypeSymbol when it does.
        IncrementalValuesProvider<INamedTypeSymbol> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax classSyn && classSyn.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)),
                transform: static (ctxt, cancelToken) =>
                {
                    var syntax = (ClassDeclarationSyntax)ctxt.Node;

                    if (ctxt.SemanticModel.GetDeclaredSymbol(syntax, cancelToken) is INamedTypeSymbol symbol)
                    {
                        // Looking for the exact name of the class.
                        if (symbol.ToDisplayString() == "Autofac.RegistrationExtensions")
                        {
                            return symbol;
                        }
                    }

                    return null;
                })
                .Where(static m => m is not null)!;

        // Just get our first one (will only be one instance anyway, we just need to convert to a single value provider).
        IncrementalValueProvider<INamedTypeSymbol?> firstSyntax
            = classDeclarations.Collect().Select((all, _) => all.FirstOrDefault());

        context.RegisterSourceOutput(
            firstSyntax,
            static (spc, regExtensionsTypeSymbol) => Execute(spc, regExtensionsTypeSymbol));
    }

    private static void Execute(SourceProductionContext spc, INamedTypeSymbol? regExtensionClass)
    {
        if (regExtensionClass is null)
        {
            return;
        }

        // Add our holding type for delegate invokers.
        GenerateDelegateInvokers(
            spc,
            holdingTypeName: "DelegateInvokers",
            static (int argCount, bool hasComponentContext) => hasComponentContext ? $"DelegateInvoker{argCount}WithComponentContext" : $"DelegateInvoker{argCount}",
            NumberOfGenericArgs);

        // Add our holding type for registration extensions.
        GenerateExtensionMethodClass(
            spc,
            "RegistrationExtensions",
            "Register",
            static (int argCount, bool hasComponentContext) => hasComponentContext ?
                $"DelegateInvokers.DelegateInvoker{argCount}WithComponentContext" :
                $"DelegateInvokers.DelegateInvoker{argCount}",
            NumberOfGenericArgs);
    }

    private static void GenerateExtensionMethodClass(
        SourceProductionContext spc,
        string className,
        string extensionMethodName,
        Func<int, bool, string> getDelegateInvokerName,
        int maxArgs)
    {
        var sb = new StringBuilder();

        sb.Append($@"// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving;

namespace Autofac;

/// <summary>
/// Adds registration syntax to the <see cref=""ContainerBuilder""/> type.
/// </summary>
[SuppressMessage(""Microsoft.Maintainability"", ""CA1506:AvoidExcessiveClassCoupling"")]
public static partial class {className}
{{");
        for (var argCount = 1; argCount <= maxArgs; argCount++)
        {
            GenerateExtensionMethod(sb, extensionMethodName, getDelegateInvokerName, argCount, withComponentContext: false);

            sb.AppendLine();

            GenerateExtensionMethod(sb, extensionMethodName, getDelegateInvokerName, argCount, withComponentContext: true);

            sb.AppendLine();
        }

        sb.AppendLine($@"
}}");

        spc.AddSource($"Autofac.{className}.g.cs", sb.ToString());
    }

    private static void GenerateExtensionMethod(StringBuilder sb, string methodName, Func<int, bool, string> getDelegateInvokerName, int argCount, bool withComponentContext)
    {
        var reusableStrBuilder = new StringBuilder();
        var delegateGenericTypeList = GetTypeParamList(reusableStrBuilder, withComponentContext, argCount);
        var methodGenericTypeList = GetTypeParamList(reusableStrBuilder, withComponentContext: false, argCount);

        sb.AppendLine($@"
    /// <summary>
    /// Register a delegate as a component.
    /// </summary>");
        WriteTypeParamDocs(sb, argCount, 1);
        sb.AppendLine($@"    /// <typeparam name=""TComponent"">The type of the instance.</typeparam>
    /// <param name=""builder"">Container builder.</param>
    /// <param name=""delegate"">The delegate to register.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle>
        {methodName}<{methodGenericTypeList}>(
            this ContainerBuilder builder,
            Func<{delegateGenericTypeList}> @delegate)");
        WriteTypeConstraints(sb, argCount, 2);
        sb.Append($@"    {{
        if (@delegate is null)
        {{
            throw new ArgumentNullException(nameof(@delegate));
        }}

        var delegateInvoker = new {getDelegateInvokerName(argCount, withComponentContext)}<{methodGenericTypeList}>(@delegate);

        return builder.Register(delegateInvoker.ResolveWithDelegate);
    }}");
    }

    private static void GenerateDelegateInvokers(
        SourceProductionContext spc,
        string holdingTypeName,
        Func<int, bool, string> nameGenerator,
        int maxArgs)
    {
        var sb = new StringBuilder();

        sb.Append($@"// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Core.Resolving;

/// <summary>
/// Provides delegate invocation holding classes.
/// </summary>
[SuppressMessage(""Microsoft.Maintainability"", ""CA1506:AvoidExcessiveClassCoupling"")]
internal static partial class {holdingTypeName}
{{");

        for (var argCount = 1; argCount <= maxArgs; argCount++)
        {
            GenerateDelegateInvoker(sb, nameGenerator(argCount, false), argCount);

            sb.AppendLine();

            GenerateDelegateInvokerWithComponentContext(sb, nameGenerator(argCount, true), argCount);

            sb.AppendLine();
        }

        sb.AppendLine($@"
}}");

        spc.AddSource($"Autofac.{holdingTypeName}.g.cs", sb.ToString());
    }

    private static void GenerateDelegateInvokerWithComponentContext(StringBuilder sb, string typeName, int argCount)
    {
        var reusableStrBuilder = new StringBuilder();
        var delegateGenericTypeList = GetTypeParamList(reusableStrBuilder, withComponentContext: true, argCount);

        sb.AppendLine($@"
    public sealed class {typeName}<{GetTypeParamList(reusableStrBuilder, withComponentContext: false, argCount)}> : BaseGenericResolveDelegateInvoker");
        WriteTypeConstraints(sb, argCount, 2);
        sb.Append($@"    {{
        private readonly Func<{delegateGenericTypeList}> _delegate;

        public {typeName}(Func<{delegateGenericTypeList}> @delegate)
        {{
            _delegate = @delegate;
        }}

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {{
            if (AnyParameters(parameters))
            {{
                return _delegate(
                    context,");
        sb.AppendLine();
        WriteResolveWithParametersOrRegistrationCalls(sb, parameterInfoIndexOffset: 1, argCount, indentTimes: 5);
        sb.Append(");");
        sb.Append($@"
            }}

            return _delegate(
                    context,");
        sb.AppendLine();
        WriteResolveCalls(sb, argCount, indentTimes: 5);
        sb.Append($@");
        }}
    }}");
    }

    private static void GenerateDelegateInvoker(StringBuilder sb, string typeName, int argCount)
    {
        var delegateGenericTypeList = GetTypeParamList(withComponentContext: false, argCount);

        sb.AppendLine($@"
    public sealed class {typeName}<{delegateGenericTypeList}> : BaseGenericResolveDelegateInvoker");
        WriteTypeConstraints(sb, argCount, 2);
        sb.Append($@"    {{
        private readonly Func<{delegateGenericTypeList}> _delegate;

        public {typeName}(Func<{delegateGenericTypeList}> @delegate)
        {{
            _delegate = @delegate;
        }}

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {{
            if (AnyParameters(parameters))
            {{
                return _delegate(");
        sb.AppendLine();
        WriteResolveWithParametersOrRegistrationCalls(sb, parameterInfoIndexOffset: 0, argCount, indentTimes: 5);
        sb.Append(");");
        sb.Append($@"
            }}

            return _delegate(");
        sb.AppendLine();
        WriteResolveCalls(sb, argCount, indentTimes: 5);
        sb.Append(@$");
        }}
    }}");
    }

    private static string GetTypeParamList(bool withComponentContext, int argCount)
    {
        var tmpStrBuilder = new StringBuilder();

        GetTypeParamList(tmpStrBuilder, withComponentContext, argCount);

        return tmpStrBuilder.ToString();
    }

    private static string GetTypeParamList(StringBuilder sb, bool withComponentContext, int argCount)
    {
        sb.Clear();

        if (withComponentContext)
        {
            sb.Append("IComponentContext, ");
        }

        for (var argPos = 1; argPos <= argCount; argPos++)
        {
            sb.Append($"TDependency{argPos}, ");
        }

        sb.Append("TComponent");

        return sb.ToString();
    }

    private static void WriteTypeConstraints(StringBuilder sb, int argCount, int indentTimes)
    {
        for (var argPos = 1; argPos <= argCount; argPos++)
        {
            sb.Append(' ', indentTimes * SpacesPerIndent);
            sb.AppendLine($"where TDependency{argPos} : notnull");
        }
    }

    private static void WriteTypeParamDocs(StringBuilder sb, int argCount, int indentTimes)
    {
        for (var argPos = 1; argPos <= argCount; argPos++)
        {
            sb.Append(' ', indentTimes * SpacesPerIndent);
            sb.AppendLine($"/// <typeparam name=\"TDependency{argPos}\">The type of a dependency to inject into the delegate.</typeparam>");
        }
    }

    private static void WriteResolveWithParametersOrRegistrationCalls(StringBuilder sb, int parameterInfoIndexOffset, int argCount, int indentTimes)
    {
        for (var argPos = 1; argPos <= argCount; argPos++)
        {
            sb.Append(' ', indentTimes * SpacesPerIndent);
            sb.Append($"ResolveWithParametersOrRegistration<TDependency{argPos}>(context, parameters, {(argPos - 1) + parameterInfoIndexOffset})");

            if (argPos < argCount)
            {
                sb.AppendLine(",");
            }
        }
    }

    private static void WriteResolveCalls(StringBuilder sb, int argCount, int indentTimes)
    {
        for (var argPos = 1; argPos <= argCount; argPos++)
        {
            sb.Append(' ', indentTimes * SpacesPerIndent);
            sb.Append($"context.Resolve<TDependency{argPos}>()");

            if (argPos < argCount)
            {
                sb.AppendLine(",");
            }
        }
    }
}
