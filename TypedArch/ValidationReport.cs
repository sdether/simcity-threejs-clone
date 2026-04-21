namespace TypedArch;

public class ValidationReport(List<string> errors, List<string> warnings)
{
    public IReadOnlyList<string> Errors   { get; } = errors;
    public IReadOnlyList<string> Warnings { get; } = warnings;
    public bool IsValid => Errors.Count == 0;
}
