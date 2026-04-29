namespace TypedArch;

/// <summary>
/// Marker interface for abstract archetype base types that define shared component sets.
/// Cannot be used directly to create entities — concrete archetypes must also implement
/// <see cref="IArcheType"/>. Enables <see cref="TypedQueryDescription{TArcheType}"/> to
/// be bound to a base archetype covering all concrete subtypes.
/// </summary>
public interface IAbstractArcheType { }
