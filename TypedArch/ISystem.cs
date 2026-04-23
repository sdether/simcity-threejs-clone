using Arch.Core;

namespace TypedArch;

/// <summary>
/// Base interface for all systems. Declare QueryDescription or TypedQueryDescription fields
/// (any visibility, static or instance) — the validator discovers them automatically.
/// </summary>
public interface ISystem
{
    void Run(World world);
}
