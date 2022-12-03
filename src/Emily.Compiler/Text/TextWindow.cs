using System.Diagnostics;

namespace Emily.Compiler.Text;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class TextWindow
{
    readonly string _buffer;
    int _start;
    int _length;

    internal string DebuggerDisplay => GetDebugDisplay();

    public string Content => _buffer[Start .. End];
    public int Start => _start;
    public int Length => _length;
    public int End => _start + _length;
    public bool EndOfFile => _start + _length >= _buffer.Length;

    public TextWindow(string buffer)
    {
        _buffer = buffer;
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

        return _buffer[(_start + oldLength) .. (_start + _length)];
    }

    public char? Peek()
    {
        return End < _buffer.Length ? _buffer[End] : null;
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
        return _buffer[(_start + oldLength) .. (_start + _length)];
    }

    public void Extend(int count)
    {
        var newLength = _length + count;
        _length = (_start + newLength) < _buffer.Length
            ? newLength
            : _buffer.Length - _start;
    }

    public void Advance()
    {
        _start += _length;
        _length = 0;
    }

    string GetDebugDisplay()
    {
        var displayStart = Math.Max(_start - 10, 0);
        var displayEnd = Math.Min((_start + _length) + 10, _buffer.Length);
        return
            $"{_buffer[displayStart.._start]}«{_buffer[_start .. (_start + _length)]}»{_buffer[(_start + _length) .. displayEnd]}";
    }
}