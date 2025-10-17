﻿namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

public class ArrayIndexExpressionSyntax : ExpressionSyntax
{
    public ValueExpressionSyntax Value { get; private set; }
    public IReadOnlyList<ArrayIndexerExpressionSyntax> Indexer { get; private set; }

    public override SyntaxLocation Location => Value.Location;
    public override SyntaxSpan Span => new(Value.Span.Position, Indexer.Count <= 0 ? Value.Span.EndPosition : Indexer[^1].Span.EndPosition);

    public ArrayIndexExpressionSyntax(ValueExpressionSyntax value, IReadOnlyList<ArrayIndexerExpressionSyntax> indexer)
    {
        value.Parent = this;
        foreach (var index in indexer)
            index.Parent = this;

        Value = value;
        Indexer = indexer;

        Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        position = Value.UpdatePosition(position, ref line, ref column);
        foreach (var index in Indexer)
            position = index.UpdatePosition(position, ref line, ref column);

        return position;
    }
}