using System.Text.RegularExpressions;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Business.Layton1ToolManagement.Scripts;

class Layton1ScriptInstructionManager(ILayton1ScriptInstructionDescriptionProvider descriptionProvider) : ILayton1ScriptInstructionManager
{
    private readonly Regex _subPattern = new("^sub[0-9]+$", RegexOptions.Compiled);

    public Layton1ScriptInstruction? GetInstruction(MethodInvocationStatementSyntax invocation, string gameCode)
    {
        int instructionType;

        if (!descriptionProvider.MapsMethodName(gameCode, invocation.Identifier.Text))
        {
            if (!TryGetInvocationType(invocation.Identifier, out instructionType))
                return null;

            if (!descriptionProvider.MapsInstructionType(gameCode, instructionType))
                return null;
        }
        else
            instructionType = descriptionProvider.GetInstructionType(gameCode, invocation.Identifier.Text);

        return descriptionProvider.GetDescription(gameCode, instructionType);
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