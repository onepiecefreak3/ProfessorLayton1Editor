using System.Globalization;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;

namespace Logic.Domain.CodeAnalysisManagement.Layton1;

internal class Layton1SyntaxFactory : ILayton1SyntaxFactory
{
    public SyntaxToken Create(string text, int rawKind, SyntaxTokenTrivia? leadingTrivia = null, SyntaxTokenTrivia? trailingTrivia = null)
    {
        return new(text, rawKind, leadingTrivia, trailingTrivia);
    }

    public SyntaxToken Token(SyntaxTokenKind kind)
    {
        switch (kind)
        {
            case SyntaxTokenKind.Comma: return new(",", (int)kind);
            case SyntaxTokenKind.Colon: return new(":", (int)kind);
            case SyntaxTokenKind.Semicolon: return new(";", (int)kind);
            case SyntaxTokenKind.Slash: return new("/", (int)kind);

            case SyntaxTokenKind.ParenOpen: return new("(", (int)kind);
            case SyntaxTokenKind.ParenClose: return new(")", (int)kind);
            case SyntaxTokenKind.CurlyOpen: return new("{", (int)kind);
            case SyntaxTokenKind.CurlyClose: return new("}", (int)kind);

            case SyntaxTokenKind.ReturnKeyword: return new("return", (int)kind);
            case SyntaxTokenKind.BreakKeyword: return new("break", (int)kind);
            case SyntaxTokenKind.NotKeyword: return new("not", (int)kind);
            case SyntaxTokenKind.OrKeyword: return new("or", (int)kind);
            case SyntaxTokenKind.AndKeyword: return new("and", (int)kind);
            case SyntaxTokenKind.IfKeyword: return new("if", (int)kind);
            case SyntaxTokenKind.ElseKeyword: return new("else", (int)kind);
            case SyntaxTokenKind.WhileKeyword: return new("while", (int)kind);
            case SyntaxTokenKind.TrueKeyword: return new("true", (int)kind);
            case SyntaxTokenKind.FalseKeyword: return new("false", (int)kind);
            default: throw new InvalidOperationException($"Cannot create simple token from kind {kind}. Use other methods instead.");
        }
    }

    public SyntaxToken NumericLiteral(long value)
    {
        return new($"{value}", (int)SyntaxTokenKind.NumericLiteral);
    }

    public SyntaxToken HashStringLiteral(string text)
    {
        return new($"\"{text}\"h", (int)SyntaxTokenKind.HashStringLiteral);
    }

    public SyntaxToken FloatingNumericLiteral(float value)
    {
        return new($"{value.ToString(CultureInfo.GetCultureInfo("en-gb"))}f", (int)SyntaxTokenKind.FloatingNumericLiteral);
    }

    public SyntaxToken StringLiteral(string text)
    {
        return new($"\"{text.Replace("\"", "\\\"")}\"", (int)SyntaxTokenKind.StringLiteral);
    }

    public SyntaxToken Identifier(string text)
    {
        return new(text, (int)SyntaxTokenKind.Identifier);
    }
}