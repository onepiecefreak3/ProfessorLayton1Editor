using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract.Files;

public interface ILayton1NdsParser
{
    Layton1NdsRom Parse(Stream input);
}