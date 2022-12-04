using System.Collections.Immutable;

namespace Emily.Compiler;

public enum DiagnosticSeverity
{
    Error,
    Warning,
    Lint,
    Note,
}

public record DiagnosticDescriptor(string Id, DiagnosticSeverity Severity, string Title, string MessageFormat);

public class Diagnostic
{
    object[] _messageArgs;
    
    public DiagnosticDescriptor Descriptor { get; }
    public TextLocation Location { get; }
    public IReadOnlyList<object> MessageArgs => _messageArgs;
    public string Message => string.Format(Descriptor.MessageFormat, _messageArgs);
    public string Id => Descriptor.Id;
    public string Title => Descriptor.Title;

    public Diagnostic(DiagnosticDescriptor descriptor, TextLocation location, object[] messageArgs)
    {
        Descriptor = descriptor;
        Location = location;
        _messageArgs = messageArgs;
    }
}