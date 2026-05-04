using System.Reflection;
using Arch.Core;

namespace TypedArch;

/// <summary>
/// Non-generic interface used by the validator to inspect any TypedQueryDescription&lt;T&gt;
/// without knowing T at compile time.
/// </summary>
internal interface ITypedQueryDescription
{
    QueryDescription Inner { get; }
    Type             ArchetypeType { get; }
}

/// <summary>
/// A QueryDescription bound to a specific archetype. Declare as a static readonly field
/// on a system — the validator uses the generic argument to check conformance against
/// the archetype's declared component set.
/// </summary>
public readonly struct TypedQueryDescription<TArcheType> : ITypedQueryDescription
    where TArcheType : IAbstractArcheType
{
    public readonly QueryDescription Inner;

    QueryDescription ITypedQueryDescription.Inner        => Inner;
    Type             ITypedQueryDescription.ArchetypeType => typeof(TArcheType);

    internal TypedQueryDescription(QueryDescription inner) => Inner = inner;

    // ── WithAll ───────────────────────────────────────────────────────────────

    public TypedQueryDescription<TArcheType> WithAll<T>()
        where T : struct =>
        new(Inner.WithAll<T>());

    public TypedQueryDescription<TArcheType> WithAll<T0, T1>()
        where T0 : struct where T1 : struct =>
        new(Inner.WithAll<T0, T1>());

    public TypedQueryDescription<TArcheType> WithAll<T0, T1, T2>()
        where T0 : struct where T1 : struct where T2 : struct =>
        new(Inner.WithAll<T0, T1, T2>());

    public TypedQueryDescription<TArcheType> WithAll<T0, T1, T2, T3>()
        where T0 : struct where T1 : struct where T2 : struct where T3 : struct =>
        new(Inner.WithAll<T0, T1, T2, T3>());

    public TypedQueryDescription<TArcheType> WithAll<T0, T1, T2, T3, T4>()
        where T0 : struct where T1 : struct where T2 : struct where T3 : struct where T4 : struct =>
        new(Inner.WithAll<T0, T1, T2, T3, T4>());

    // ── WithAny ───────────────────────────────────────────────────────────────

    public TypedQueryDescription<TArcheType> WithAny<T>()
        where T : struct =>
        new(Inner.WithAny<T>());

    public TypedQueryDescription<TArcheType> WithAny<T0, T1>()
        where T0 : struct where T1 : struct =>
        new(Inner.WithAny<T0, T1>());

    public TypedQueryDescription<TArcheType> WithAny<T0, T1, T2>()
        where T0 : struct where T1 : struct where T2 : struct =>
        new(Inner.WithAny<T0, T1, T2>());

    // ── WithNone ──────────────────────────────────────────────────────────────

    public TypedQueryDescription<TArcheType> WithNone<T>()
        where T : struct =>
        new(Inner.WithNone<T>());

    public TypedQueryDescription<TArcheType> WithNone<T0, T1>()
        where T0 : struct where T1 : struct =>
        new(Inner.WithNone<T0, T1>());

    public TypedQueryDescription<TArcheType> WithNone<T0, T1, T2>()
        where T0 : struct where T1 : struct where T2 : struct =>
        new(Inner.WithNone<T0, T1, T2>());

    // ── Implicit conversion ───────────────────────────────────────────────────

    public static implicit operator QueryDescription(TypedQueryDescription<TArcheType> tq) =>
        tq.Inner;
}

/// <summary>Static factory for TypedQueryDescription&lt;T&gt;.</summary>
public static class TypedQueryDescription
{
    /// <summary>Empty query bound to <typeparamref name="TArcheType"/> for manual composition.</summary>
    public static TypedQueryDescription<TArcheType> Satisfies<TArcheType>()
        where TArcheType : IAbstractArcheType =>
        new(new QueryDescription());

    /// <summary>
    /// Query pre-populated with WithAll for every required component declared on
    /// <typeparamref name="TArcheType"/>, bound to that archetype.
    /// </summary>
    public static TypedQueryDescription<TArcheType> For<TArcheType>()
        where TArcheType : IAbstractArcheType
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(TArcheType));
        var componentTypes = def.Required.Select(GetComponentType).ToArray();
        var inner = new QueryDescription(
            all:  new Signature(componentTypes),
            any:  Signature.Null,
            none: Signature.Null
        );
        return new TypedQueryDescription<TArcheType>(inner);
    }

    private static ComponentType GetComponentType(Type t) =>
        (ComponentType)(typeof(Component<>)
            .MakeGenericType(t)
            .GetField("ComponentType", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Cannot resolve Arch ComponentType for {t.Name}"))
            .GetValue(null)!;
}
