using System.Diagnostics;
using System.Text;
using Emily.Compiler.Text;

namespace Emily.Compiler.Tokenizer;

public class Tokenizer
{
    readonly TextWindow _window;
    Token? _next = null;

    public DiagnosticCollection Diagnostics { get; } = new();

    public Tokenizer(SourceText content)
    {
        _window = new TextWindow(content);
    }

    /// <summary>
    /// Peek at the next token in the stream, but does not advance to the next token.
    /// Returns <c>null</c> if end-of-file is reached.
    /// </summary>
    public Token? Peek()
    {
        if (_next is null)
        {
            _next = GetNextToken();
        }

        return _next;
    }

    /// <summary>
    /// Reads the next token in the stream, and advances to the next token.
    /// Returns <c>null</c> if end-of-file is reached.
    /// </summary>
    public Token? Next()
    {
        var token = Peek();

        // Clear out the buffer so that we are forced to parse the next token.
        _next = null;

        return token;
    }

    Token? GetNextToken()
    {
        Token ParsePlusMinus(TokenType operatorType)
        {
            _window.Extend();
            if (_window.Peek() is { } x && char.IsDigit(x))
            {
                return ParseNumericLiteral();
            }

            return CompleteToken(operatorType);
        }

        Token ParseStringLiteral()
        {
            string ParseString()
            {
                var value = new StringBuilder();
                while (true)
                {
                    switch (_window.Next())
                    {
                        case '"':
                            // End of string
                            return value.ToString();
                        case '\\':
                            switch (_window.Peek())
                            {
                                case '"':
                                    _window.Extend();
                                    value.Append('"');
                                    break;
                                case 'n':
                                    _window.Extend();
                                    value.Append('\n');
                                    break;
                                case '\\':
                                    _window.Extend();
                                    value.Append('\\');
                                    break;
                                case { } c:
                                    // Emit an error, but accept the character
                                    _window.Extend();
                                    Diagnostics.Emit(
                                        DiagnosticDescriptors.InvalidEscapeSequence,
                                        new (_window.Source, new(_window.End - 2, 2)),
                                        $"\\{c}");
                                    // Continue, ignoring the escape sequence, to recover.
                                    break;
                                default:
                                    Diagnostics.Emit(
                                        DiagnosticDescriptors.UnterminatedStringLiteral,
                                        _window.Location);
                                    return value.ToString();
                            }

                            break;
                        case { } c:
                            value.Append(c);
                            break;
                        default:
                            Diagnostics.Emit(
                                DiagnosticDescriptors.UnterminatedStringLiteral,
                                _window.Location);
                            return value.ToString();
                    }
                }
            }

            _window.Assert('"');
            var parsed = ParseString();
            return CompleteToken(TokenType.StringLiteral, new StringTokenValue(parsed));
        }

        Token ParseNumericLiteral()
        {
            if (_window.Peek() is '-' or '+')
            {
                _window.Extend(1);
            }

            _window.NextWhile(c => char.IsDigit(c) || c == '_');
            
            // Probably a better way to do this, but 🤷
            try
            {
                var value = long.Parse(_window.Content.Replace("_", ""));
                return CompleteToken(TokenType.IntegerLiteral, new IntegerTokenValue(value));
            }
            catch (OverflowException)
            {
                Diagnostics.Emit(
                    DiagnosticDescriptors.IntegerLiteralTooLarge,
                    _window.Location,
                    _window.Content);
                return CompleteToken(TokenType.IntegerLiteral, new IntegerTokenValue(long.MaxValue));
            }
        }

        Token CompleteToken(TokenType type, TokenValue? value = null)
        {
            var tok = new Token(type, value, _window.Content, _window.Start, _window.End);
            _window.Advance();
            return tok;
        }

        Token ConsumeAndComplete(TokenType type, TokenValue? value = null)
        {
            _window.Extend();
            return CompleteToken(type, value);
        }

        Token OneOrTwoCharOperator(char secondChar, TokenType oneCharType, TokenType twoCharType)
        {
            _window.Extend();
            if (_window.Peek() is { } x && x == secondChar)
            {
                return ConsumeAndComplete(twoCharType);
            }

            return CompleteToken(oneCharType);
        }

        // The Big Switch
        // This is where we decide what kind of token we're parsing, based on the character we see.
        switch (_window.Peek())
        {
            case '"':
                return ParseStringLiteral();

            case '_':
            case { } x when char.IsLetter(x):
                _window.NextWhile(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');
                return CompleteToken(TokenType.Identifier, new SymbolTokenValue(_window.Content));

            case '*': return ConsumeAndComplete(TokenType.Star);
            case '/': return ConsumeAndComplete(TokenType.Slash);
            case '%': return ConsumeAndComplete(TokenType.Percent);
            case '(': return ConsumeAndComplete(TokenType.LeftParenthesis);
            case ')': return ConsumeAndComplete(TokenType.RightParenthesis);
            case '[': return ConsumeAndComplete(TokenType.LeftBracket);
            case ']': return ConsumeAndComplete(TokenType.RightBracket);
            case '{': return ConsumeAndComplete(TokenType.LeftBrace);
            case '}': return ConsumeAndComplete(TokenType.RightBrace);
            case '.': return ConsumeAndComplete(TokenType.Dot);
            case '=': return OneOrTwoCharOperator('=', TokenType.Assign, TokenType.Equal);
            case '<': return OneOrTwoCharOperator('=', TokenType.LessThan, TokenType.LessThanEqual);
            case '>': return OneOrTwoCharOperator('=', TokenType.GreaterThan, TokenType.GreaterThanEqual);
            case '!':
                _window.Extend();
                if (_window.Peek() is '=')
                {
                    return ConsumeAndComplete(TokenType.NotEqual);
                }

                return ConsumeAndComplete(TokenType.Unknown);

            case '-': return ParsePlusMinus(TokenType.Minus);
            case '+': return ParsePlusMinus(TokenType.Plus);
            case { } x when char.IsDigit(x):
                return ParseNumericLiteral();

            case { } x when char.IsWhiteSpace(x):
                _window.NextWhile(char.IsWhiteSpace);
                return CompleteToken(TokenType.Whitespace);

            default:
                _window.Extend();
                Diagnostics.Emit(
                    DiagnosticDescriptors.UnexpectedCharacter, 
                    _window.Location,
                    _window.Content);
                return CompleteToken(TokenType.Unknown);
        }
    }
}