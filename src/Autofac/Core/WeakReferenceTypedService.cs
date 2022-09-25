// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Defines a weak-referenced typed service that does not hold onto
/// the type in question unless it is still in use.
/// </summary>
internal class WeakReferenceTypedService : Service, IServiceWithType
{
    private WeakReference<Type>? _underlyingType;

    private WeakReferenceTypedService()
    {
    }

    public WeakReferenceTypedService(Type serviceType)
    {
        _underlyingType = new WeakReference<Type>(serviceType);
    }

    public static WeakReferenceTypedService Empty { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    public bool IsValid => TryGetTarget(out _);

    /// <inheritdoc />
    public override string Description => _underlyingType is not null &&
                                          _underlyingType.TryGetTarget(out var targetType) ? targetType.FullName! : "empty weak reference";

    public Type ServiceType => TryGetTarget(out var targetType) ? targetType : throw new InvalidOperationException("Attempt to access the type of a service after it has been cleared is not valid");

    public Service ChangeType(Type newType)
    {
        return new WeakReferenceTypedService(newType);
    }

    private bool TryGetTarget([NotNullWhen(true)] out Type? target)
    {
        if (_underlyingType is not null)
        {
            return _underlyingType.TryGetTarget(out target);
        }

        target = null;
        return false;
    }

}
