namespace TypedArch;

/// <summary>
/// Marker interface for systems. Declare QueryDescription or TypedQueryDescription fields
/// (any visibility, static or instance) — the validator discovers them automatically.
/// Systems define their own Run() signature to accept whatever context they need.
/// </summary>
public interface ISystem { }
