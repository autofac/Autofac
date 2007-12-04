using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Component.Activation;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text.RegularExpressions;

namespace Autofac.Component.Activation
{
	/// <summary>
	/// An activator that provides a delegate type that is adapted to be implemented
	/// in terms of ComponentActivator.
	/// </summary>
	/// <remarks>
    /// Currently the structure of the generated code requires that 'creator' is
	/// publicly visible from the assembly it is declared in. This could be circumvented
	/// by declaring an equivalent delegate type in the generated assembly then converting
	/// between the types at runtime (I think!)
    /// Better still, switch to DynamicMethod :)
    /// </remarks>
	class InterceptingDelegateActivator : IActivator
	{
		Type _implementor;
		ComponentActivator _activator;
		const string InterceptionMethodName = "Create";
		Type _creator;

		static readonly Regex GenericTypeRegex = new Regex(@"`[0-9]*\[\[([A-z\._0-9]*),[^\[]*\]\]");

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingDelegateActivator"/> class.
        /// </summary>
        /// <param name="creator">The creator.</param>
        /// <param name="activator">The activator.</param>
		public InterceptingDelegateActivator(Type creator, ComponentActivator activator)
		{
            Enforce.ArgumentNotNull(creator, "creator");
            Enforce.ArgumentNotNull(activator, "activator");

			_implementor = BuildImplementor(creator);
			_creator = creator;
			_activator = activator;
		}

		#region IActivator Members

        /// <summary>
        /// Create a component instance, using container
        /// to resolve the instance's dependencies.
        /// </summary>
        /// <param name="context">The context to use
        /// for dependency resolution.</param>
        /// <param name="parameters">Parameters that can be used in the resolution process.</param>
        /// <returns>
        /// A component instance. Note that while the
        /// returned value need not be created on-the-spot, it must
        /// not be returned more than once by consecutive calls. (Throw
        /// an exception if this is attempted. IActivationScope should
        /// manage singleton semantics.)
        /// </returns>
		public object ActivateInstance(IContext context, IActivationParameters parameters)
		{
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

			var implementor = Activator.CreateInstance(_implementor, _activator, context);
			return Delegate.CreateDelegate(_creator, implementor, InterceptionMethodName);
		}

        /// <summary>
        /// A 'new context' is a scope that is self-contained
        /// and that can dispose the components it contains before the parent
        /// container is disposed. If the activator is stateless it should return
        /// true, otherwise false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can support a new context; otherwise, <c>false</c>.
        /// </value>
		public bool CanSupportNewContext
		{
			get { return true; }
		}

		#endregion

		private Type BuildImplementor(Type creator)
		{
            Enforce.ArgumentNotNull(creator, "creator");

			MethodInfo invoke = creator.GetMethod("Invoke");

			var returnType = invoke.ReturnType;
			var returnTypeName = CSharpTypeSyntax(returnType.FullName);
			var parameters = invoke.GetParameters();
			var className = "Interceptor_" + Guid.NewGuid().ToString("N");
			var namespaceName = "Autofac.Extra";

			var source = new StringBuilder();
			source.AppendLine("namespace " + namespaceName + " {");
				source.AppendLine("public class " +  className + " {");
					source.AppendLine("Autofac.Component.ComponentActivator _activator;");
					source.AppendLine("Autofac.IContext _context;");
					source.AppendLine("public " + className + "(Autofac.Component.ComponentActivator activator, Autofac.IContext context) {");
						source.AppendLine("if (activator == null) throw new System.ArgumentNullException(\"activator\");");
						source.AppendLine("if (context == null) throw new System.ArgumentNullException(\"context\");");
						source.AppendLine("_activator = activator; _context = context;");
					source.AppendLine("}");
					source.Append("public " + returnTypeName + " " + InterceptionMethodName + "(");
						bool first = true;
						foreach (var p in parameters)
						{
							if (first)
								first = false;
							else
								source.Append(", ");

							source.AppendFormat("{0} {1}", CSharpTypeSyntax(p.ParameterType.FullName), p.Name);
						}
					source.AppendLine(") {");
						source.AppendLine("Autofac.IActivationParameters parameters = new Autofac.ActivationParameters();");
						foreach (var p in parameters)
							source.AppendLine("parameters[\"" + p.Name + "\"] = " + p.Name + ";");
						source.AppendLine("object result = _activator(_context, parameters);");
						if (returnType != typeof(void))
							source.AppendLine("return (" + returnTypeName + ") result;");
					source.AppendLine("}");
				source.AppendLine("}");
			source.AppendLine("}");

			var refs = new List<string>();
			refs.Add(creator.Assembly.Location);
			refs.AddRange(creator.Assembly.GetReferencedAssemblies().Select(a => Assembly.Load(a.FullName).Location));
			refs.Add(typeof(InterceptingDelegateActivator).Assembly.Location);
			refs.Add(typeof(int).Assembly.Location);
			var asm = BuildAssembly(source.ToString(), refs);

			return asm.GetType(namespaceName + "." + className);
		}

		private string CSharpTypeSyntax(string cliTypeName)
		{
            Enforce.ArgumentNotNull(cliTypeName, "cliTypeName");

			bool found;
			do
			{
				found = false;
				cliTypeName = GenericTypeRegex.Replace(cliTypeName, m =>
				{
					found = true;
					return "<" + m.Groups[1].Value + ">";
				});
			}
			while (found);
			
			return cliTypeName.Replace('+', '.');
		}

		private Assembly BuildAssembly(string source, IEnumerable<string> references)
		{
            Enforce.ArgumentNotNull(source, "source");
            Enforce.ArgumentNotNull(references, "references");

			var provider = new CSharpCodeProvider();

			var compilerparams = new CompilerParameters()
			{
				GenerateExecutable = false,
				GenerateInMemory = true
			};

			compilerparams.ReferencedAssemblies.AddRange(references.ToArray());

			var results = provider.CompileAssemblyFromSource(compilerparams, source);

			if (results.Errors.HasErrors)
			{
				var errors = new StringBuilder();
				errors.AppendLine("Compiler Errors:");
				foreach (CompilerError error in results.Errors)
				{
					errors.AppendFormat("{0},{1}\t: {2}\n",
						   error.Line, error.Column, error.ErrorText);
				}
				throw new Exception(errors.ToString());
			}
			else
			{
				return results.CompiledAssembly;
			}
		}
	}
}
