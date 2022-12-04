namespace Emily.Compiler.Tokenizer;

public enum TokenType
{
    Unknown = 0,
    Whitespace,
    IntegerLiteral,
    StringLiteral,
    Identifier,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    Assign,
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanEqual,
    LessThan,
    LessThanEqual,
    LeftParenthesis,
    RightParenthesis,
    LeftBracket,
    RightBracket,
    LeftBrace,
    RightBrace,
    Dot
}

public record Token(TokenType Type, TokenValue? Value, string Text, int Start, int End);
public abstract record TokenValue();
public record IntegerTokenValue(long Value): TokenValue();
public record StringTokenValue(string Value): TokenValue();
public record SymbolTokenValue(string Symbol): TokenValue();
