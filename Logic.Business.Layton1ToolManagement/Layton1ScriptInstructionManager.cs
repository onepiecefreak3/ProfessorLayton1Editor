using Logic.Business.Layton1ToolManagement.Contract;
using System.Text.RegularExpressions;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.InternalContract;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement;

class Layton1ScriptInstructionManager(ILayton1ScriptInstructionDescriptionProvider descriptionProvider) : ILayton1ScriptInstructionManager
{
    private readonly Regex _subPattern = new("^sub[0-9]+$", RegexOptions.Compiled);

    public Layton1ScriptInstruction? GetInstruction(MethodInvocationStatementSyntax invocation)
    {
        int instructionType;

        if (!descriptionProvider.MapsMethodName(invocation.Identifier.Text))
        {
            if (!TryGetInvocationType(invocation.Identifier, out instructionType))
                return null;

            if (!descriptionProvider.MapsInstructionType(instructionType))
                return null;
        }
        else
            instructionType = descriptionProvider.GetInstructionType(invocation.Identifier.Text);

        return descriptionProvider.GetDescription(instructionType);
    }

    private bool TryGetInvocationType(SyntaxToken identifier, out int instructionType)
    {
        instructionType = -1;

        if (!_subPattern.IsMatch(identifier.Text))
            return false;

        instructionType = GetNumberFromStringEnd(identifier.Text);
        return true;

    }

    private static int GetNumberFromStringEnd(string text)
    {
        int startIndex = text.Length;
        while (text[startIndex - 1] >= '0' && text[startIndex - 1] <= '9')
            startIndex--;

        return int.Parse(text[startIndex..]);
    }
}