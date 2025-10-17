using Logic.Business.Layton1ToolManagement.InternalContract;

namespace Logic.Business.Layton1ToolManagement;

class Layton1GameCodeValidator : ILayton1GameCodeValidator
{
    public void Validate(string gameCode)
    {
        if (gameCode.Length < 3)
            throw new InvalidOperationException("Game Code needs to be at least 3 characters long.");

        if (gameCode[..3] is not "A5F" and not "Y49" and not "C5F")
            throw new InvalidOperationException($"Game Code {gameCode} is not associated with Professor Layton 1.");
    }
}