
# Notes on nullability

 - Anywhere that we have a 'Try....' method, we now apply [NotNullWhen(returnValue: true)] on the out parameter.
   This should probably be enforced via code review in the future.

   ReflectionExtensions
   ```

    public static bool TryGetDeclaringProperty(this ParameterInfo pi, [NotNullWhen(returnValue: true)] out PropertyInfo? prop)

   ```

 - Null annotation attributes only inform the caller if used in a straight if declaration.

    ResolutionExtensions.TryResolve

 - Registration methods where we accept a 'limit' now constrain the limit to where T: notnull.

   The only impact I can think of is if someone was using ``Nullable<int>`` as the limit for a service.

   In addition, TryResolve is now constrained to 'class'.

 - Types that overload == and != need to specify the overload parameters as nullable, otherwise we can't do null checks on those values.

    Service
    ```
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Service? left, Service? right)
        {
            return Equals(left, right);
        }
    ```

 - Need to make 'optional' methods return TService? to indicate to the caller that the response may be null.

 - Upgraded StyleCop analyzers to a beta version (for now), to understand nullable modifiers on arrays.

 - The Roslyn compiler reports a class field as being uninitialized (and potentially null) if it can't tell it's been initialised at the end of the constructor.
   This happens even if the constructor calls a method or property that always instantiates the field. So you can set 'default!' or 'null!' on the field to get round this (not ideal, but explainable).

    ResolveOperation
    ```
        // _successfulActivations can never be null, but the roslyn compiler doesn't look deeper than
        // the initial constructor methods yet.
        private List<InstanceLookup> _successfulActivations = default!;
    ```

    DeferredCallbacks
    ```
        // _callback set to default! to get around initialisation detection problem in rosyln.
        private Action<IComponentRegistry> _callback = default!;
    ```

   There are a couple of open rosyln issues that are related to this, some discussion about adding flow analysis to check that constructors
   do in fact finish having initialised those properties.

   This is also a massive pain whenever I have a class that I want to instantiate using 'Property' notation, e.g. DecoratorContext:

   ```
     private DecoratorContext()  // error on this line
     {

     }

     internal static DecoratorContext Create(Type implementationType, Type serviceType, object implementationInstance)
     {
        var context = new DecoratorContext
        {
            ImplementationType = implementationType,
            ServiceType = serviceType,
            AppliedDecorators = new List<object>(0),
            AppliedDecoratorTypes = new List<Type>(0),
            CurrentInstance = implementationInstance
        };
        return context;
     }


   ```

   https://github.com/dotnet/roslyn/issues/39291
   https://github.com/dotnet/roslyn/issues/39090
   https://github.com/dotnet/roslyn/issues/37975

 - If a method is being passed as an expression for reflection only, it's better to mark the arguments as default! rather than
   change the signature of the type:

    FactoryGenerator
    ```
        // The explicit '!' default is ok because the code is never executed, it's just used by
        // the expression tree.
        private static readonly ConstructorInfo RequestConstructor
            = ReflectionExtensions.GetConstructor(() => new ResolveRequest(default!, default!, default!));
    ```

 - In a couple of specific cases, some generic methods now need to state explicitly 'class' as a type constraint, where they didn't before; if I don't do that
   then duplicate overloads for class and struct need to be defined to remove ambiguity.

   RegistrationExtensions.RegisterDecorator
