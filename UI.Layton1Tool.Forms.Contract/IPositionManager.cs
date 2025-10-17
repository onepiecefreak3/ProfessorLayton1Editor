using ImGui.Forms.Controls.Text.Editor;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.Enums;

namespace UI.Layton1Tool.Forms.Contract;

public interface IPositionManager
{
    bool Compare(SyntaxLocation coordinate1, Coordinate coordinate2, PositionComparison comparison);
    bool Compare(Coordinate coordinate1, SyntaxLocation coordinate2, PositionComparison comparison);
    bool Compare(SyntaxLocation coordinate1, SyntaxLocation coordinate2, PositionComparison comparison);
    bool Compare(Coordinate coordinate1, Coordinate coordinate2, PositionComparison comparison);
}