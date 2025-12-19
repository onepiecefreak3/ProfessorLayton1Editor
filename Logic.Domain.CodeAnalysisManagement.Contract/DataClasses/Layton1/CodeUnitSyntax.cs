namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class CodeUnitSyntax : SyntaxNode
{
    public MethodDeclarationSyntax? MethodDeclaration { get; private set; }

    public override SyntaxLocation Location => MethodDeclaration?.Location ?? new(1, 1);
    public override SyntaxSpan Span => MethodDeclaration?.Span ?? new(0, 0);

    public CodeUnitSyntax(MethodDeclarationSyntax? methodDeclaration)
    {
        MethodDeclaration = methodDeclaration;

        if (methodDeclaration is not null)
            methodDeclaration.Parent = this;

        Root.Update();
    }

    public void SetMethodDeclaration(MethodDeclarationSyntax? methodDeclaration, bool updatePosition = true)
    {
        MethodDeclaration = methodDeclaration;

        if (methodDeclaration is not null)
            methodDeclaration.Parent = this;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        if (MethodDeclaration is not null)
            position = MethodDeclaration.UpdatePosition(position, ref line, ref column);

        return position;
    }
}