using ImGui.Forms.Controls.Text.Editor;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.Enums;

namespace UI.Layton1Tool.Forms;

class PositionManager : IPositionManager
{
    public bool Compare(SyntaxLocation coordinate1, Coordinate coordinate2, PositionComparison comparison)
    {
        return Compare(new Coordinate(coordinate1.Line - 1, coordinate1.Column - 1), coordinate2, comparison);
    }

    public bool Compare(Coordinate coordinate1, SyntaxLocation coordinate2, PositionComparison comparison)
    {
        return Compare(coordinate1, new Coordinate(coordinate2.Line - 1, coordinate2.Column - 1), comparison);
    }

    public bool Compare(SyntaxLocation coordinate1, SyntaxLocation coordinate2, PositionComparison comparison)
    {
        return Compare(new Coordinate(coordinate1.Line - 1, coordinate1.Column - 1),
            new Coordinate(coordinate2.Line - 1, coordinate2.Column - 1), comparison);
    }

    public bool Compare(Coordinate coordinate1, Coordinate coordinate2, PositionComparison comparison)
    {
        switch (comparison)
        {
            case PositionComparison.GreaterThan:
                return IsGreaterThan(coordinate1, coordinate2);

            case PositionComparison.GreaterEqual:
                return IsGreaterEquals(coordinate1, coordinate2);

            case PositionComparison.SmallerThan:
                return IsSmallerThan(coordinate1, coordinate2);

            case PositionComparison.SmallerEqual:
                return IsSmallerEquals(coordinate1, coordinate2);

            default:
                throw new InvalidOperationException($"Unsupported comparison operator {comparison}.");
        }
    }

    private static bool IsGreaterThan(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.Line < coordinate2.Line)
            return false;

        if (coordinate1.Line > coordinate2.Line)
            return true;

        return coordinate1.Column > coordinate2.Column;
    }

    private static bool IsGreaterEquals(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.Line < coordinate2.Line)
            return false;

        if (coordinate1.Line > coordinate2.Line)
            return true;

        return coordinate1.Column >= coordinate2.Column;
    }

    private static bool IsSmallerThan(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.Line > coordinate2.Line)
            return false;

        if (coordinate1.Line < coordinate2.Line)
            return true;

        return coordinate1.Column < coordinate2.Column;
    }

    private static bool IsSmallerEquals(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.Line > coordinate2.Line)
            return false;

        if (coordinate1.Line < coordinate2.Line)
            return true;

        return coordinate1.Column <= coordinate2.Column;
    }
}