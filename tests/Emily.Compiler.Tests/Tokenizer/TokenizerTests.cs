namespace Emily.Compiler.Tokenizer;

public class TokenizerTests
{
    [Theory]
    [InlineData("    ", "    ")]
    [InlineData("    abc", "    ")]
    [InlineData(" \t  abc", " \t  ")]
    [InlineData(" \n\r\nabc\n", " \n\r\n")]
    public void ParsesWhitespaceTokens(string document, string text)
    {
        RunSingleTokenTest(document, text, TokenType.Whitespace);
    }

    [Theory]
    [InlineData("1234", "1234", 1234)]
    [InlineData("100_000", "100_000", 100_000)]
    public void ParsesUnsignedIntegerToken(string document, string text, long value)
    {
        RunSingleTokenTest(document, text, TokenType.IntegerLiteral, new IntegerTokenValue(value));
    }

    [Theory]
    [InlineData("+1234", "+1234", 1234)]
    [InlineData("-100_000", "-100_000", -100_000)]
    [InlineData("-1234", "-1234", -1234)]
    [InlineData("+100_000", "+100_000", 100_000)]
    public void ParsesSignedIntegerToken(string document, string text, long value)
    {
        RunSingleTokenTest(document, text, TokenType.IntegerLiteral, new IntegerTokenValue(value));
    }

    [Theory]
    [InlineData("\"abc\"def", "\"abc\"", "abc")]
    [InlineData("\"a\\nbc\"def", "\"a\\nbc\"", "a\nbc")]
    [InlineData("\"a\\\\bc\"def", "\"a\\\\bc\"", "a\\bc")]
    public void ParsesStringLiteralToken(string document, string text, string literal)
    {
        RunSingleTokenTest(document, text, TokenType.StringLiteral, new StringTokenValue(literal));
    }

    [Fact]
    public void InvalidEscapeSequence()
    {
        RunSingleTokenDiagnosticTest(
            "\"invalid escape \\y sequence\"abc",
            "\"invalid escape \\y sequence\"",
            TokenType.StringLiteral,
            new StringTokenValue("invalid escape  sequence"),
            DiagnosticDescriptors.InvalidEscapeSequence,
            "Invalid Escape Sequence '\\y'",
            new(16, 2));
    }

    [Fact]
    public void UnterminatedStringLiteral()
    {
        RunSingleTokenDiagnosticTest(
            "\"this doesn't end",
            "\"this doesn't end",
            TokenType.StringLiteral,
            new StringTokenValue("this doesn't end"),
            DiagnosticDescriptors.UnterminatedStringLiteral,
            "Unterminated String Literal",
            new(0, 17));
    }

    [Fact]
    public void IntegerLiteralOverflow()
    {
        RunSingleTokenDiagnosticTest(
            $"{long.MaxValue}0",
            $"{long.MaxValue}0",
            TokenType.IntegerLiteral,
            new IntegerTokenValue(long.MaxValue),
            DiagnosticDescriptors.IntegerLiteralTooLarge,
            $"The Integer literal {long.MaxValue}0 is too large",
            new(0, 20));
    }

    [Fact]
    public void UnexpectedCharacter()
    {
        RunSingleTokenDiagnosticTest(
            "@",
            "@",
            TokenType.Unknown,
            null,
            DiagnosticDescriptors.UnexpectedCharacter,
            $"Unexpected '@'",
            new(0, 1));
    }
    
    [Theory]
    [InlineData("This_42_That  ", "This_42_That", "This_42_That")]
    [InlineData("_UnderscoresCanStart  ", "_UnderscoresCanStart", "_UnderscoresCanStart")]
    public void ParsesIdentifierToken(string document, string text, string symbol)
    {
        RunSingleTokenTest(document, text, TokenType.Identifier, new SymbolTokenValue(symbol));
    }

    [Theory]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Star)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("%", TokenType.Percent)]
    [InlineData("=", TokenType.Assign)]
    [InlineData("==", TokenType.Equal)]
    [InlineData("!=", TokenType.NotEqual)]
    [InlineData(">", TokenType.GreaterThan)]
    [InlineData(">=", TokenType.GreaterThanEqual)]
    [InlineData("<", TokenType.LessThan)]
    [InlineData("<=", TokenType.LessThanEqual)]
    [InlineData("(", TokenType.LeftParenthesis)]
    [InlineData(")", TokenType.RightParenthesis)]
    [InlineData("[", TokenType.LeftBracket)]
    [InlineData("]", TokenType.RightBracket)]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]
    [InlineData(".", TokenType.Dot)]
    public void ParsesOperatorToken(string @operator, TokenType type)
    {
        var document = $"{@operator}abc";
        RunSingleTokenTest(document, @operator, type);
    }

    void RunSingleTokenDiagnosticTest(string document, string text, TokenType expectedType, TokenValue? expectedValue, DiagnosticDescriptor expectedDescriptor, string message, TextSpan span)
    {
        var tokenizer = new Tokenizer(SourceText.From(document));
        var token = tokenizer.Next();
        Assert.NotNull(token);
        Assert.Equal(token, new Token(expectedType, expectedValue, text, 0, text.Length));
        Assert.Collection(tokenizer.Diagnostics,
            d =>
            {
                Assert.Equal(expectedDescriptor, d.Descriptor);
                Assert.Equal(message, d.Message);
                Assert.Equal(span, d.Location.Span);
            });
    }
    
    void RunSingleTokenTest(string document, string text, TokenType expectedType, TokenValue? expectedValue = null)
    {
        var tokenizer = new Tokenizer(SourceText.From(document));
        var token = tokenizer.Next();
        Assert.NotNull(token);
        Assert.Equal(token, new Token(expectedType, expectedValue, text, 0, text.Length));
    }
}