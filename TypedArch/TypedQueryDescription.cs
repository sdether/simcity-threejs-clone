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
    where TArcheType : IArcheType
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
    public static TypedQueryDescription<TArcheType> Create<TArcheType>()
        where TArcheType : IArcheType =>
        new(new QueryDescription());
}
