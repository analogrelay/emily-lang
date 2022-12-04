using System.Collections;
using System.Collections.Immutable;

namespace Emily.Compiler;

public class DiagnosticCollection: IEnumerable<Diagnostic>
{
    readonly List<Diagnostic> _list = new();
    public IReadOnlyList<Diagnostic> Diagnostics => _list;

    public IEnumerator<Diagnostic> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _list).GetEnumerator();

    public void Emit(DiagnosticDescriptor descriptor, TextLocation location, params object[] args)
    {
        _list.Add(new(descriptor, location, args));
    }
}