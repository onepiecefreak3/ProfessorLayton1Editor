using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1PathProvider
{
    string GetFullDirectory(string path, GameVersion version, TextLanguage language);
    string GetFullDirectory(string path, GameVersion version);
}