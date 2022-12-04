namespace Emily.Compiler;

static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidEscapeSequence = new(
        "E0001",
        DiagnosticSeverity.Error,
        "Invalid Escape Sequence",
        "Invalid Escape Sequence '{0}'");

    public static readonly DiagnosticDescriptor UnterminatedStringLiteral = new(
        "E0002",
        DiagnosticSeverity.Error,
        "Unterminated String Literal",
        "Unterminated String Literal");

    public static readonly DiagnosticDescriptor IntegerLiteralTooLarge = new(
        "E0003",
        DiagnosticSeverity.Error,
        "Integer Literal Too Large",
        "The Integer literal {0} is too large");

    public static readonly DiagnosticDescriptor UnexpectedCharacter = new(
        "E0004",
        DiagnosticSeverity.Error,
        "Unexpected Character",
        "Unexpected '{0}'");
}