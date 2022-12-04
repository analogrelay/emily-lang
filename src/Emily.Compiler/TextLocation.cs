namespace Emily.Compiler;

public record TextSpan(int Start, int Length)
{
    public int End => Start + Length;
    public override string ToString() => $"{Start} .. {End}";
}

public record TextLocation(SourceText Text, TextSpan Span);

