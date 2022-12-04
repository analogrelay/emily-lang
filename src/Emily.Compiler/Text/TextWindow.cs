using System.Diagnostics;

namespace Emily.Compiler.Text;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class TextWindow
{
    int _start;
    int _length;

    internal string DebuggerDisplay => GetDebugDisplay();

    public SourceText Source { get; }
    public string Content => Source[Start .. End];
    public int Start => _start;
    public int Length => _length;
    public int End => _start + _length;
    public TextSpan Span => new(_start, _length);
    public TextLocation Location => new(Source, Span);
    public bool EndOfFile => _start + _length >= Source.Length;

    public TextWindow(SourceText source)
    {
        Source = source;
        _start = 0;
        _length = 0;
    }

    public string PeekWhile(params char[] chars) =>
        PeekWhile(chars.Contains);
    public string PeekWhile(Func<char, bool> predicate)
    {
        var oldLength = _length;
        var val = NextWhile(predicate);
        _length = oldLength;
        return val;
    }

    public string NextWhile(params char[] chars) =>
        NextWhile(chars.Contains);
    public string NextWhile(Func<char, bool> predicate)
    {
        var oldLength = _length;
        while (Peek() is char c && predicate(c))
        {
            Extend(1);
        }

        return Source[(_start + oldLength) .. (_start + _length)];
    }

    public char? Peek()
    {
        return End < Source.Length ? Source[End] : null;
    }

    public char? Next()
    {
        var chr = Peek();
        Extend(1);
        return chr;
    }

    public string Peek(int count)
    {
        var oldLength = _length;
        var result = Next(count);
        _length = oldLength;
        return result;
    }

    public string Next(int count)
    {
        var oldLength = _length;
        Extend(count);
        return Source[(_start + oldLength) .. (_start + _length)];
    }

    public void Extend(int count = 1)
    {
        var newLength = _length + count;
        _length = (_start + newLength) < Source.Length
            ? newLength
            : Source.Length - _start;
    }

    public void Advance()
    {
        _start += _length;
        _length = 0;
    }

    string GetDebugDisplay()
    {
        var displayStart = Math.Max(_start - 10, 0);
        var displayEnd = Math.Min((_start + _length) + 10, Source.Length);
        return
            $"{Source[displayStart.._start]}«{Source[_start .. (_start + _length)]}»{Source[(_start + _length) .. displayEnd]}";
    }

    public void Assert(char c)
    {
        Trace.Assert(Peek() is {} x && x == c);
        Extend();
    }
}