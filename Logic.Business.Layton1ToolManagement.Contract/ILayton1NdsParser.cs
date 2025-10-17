using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1NdsParser
{
    Layton1NdsRom Parse(Stream input);
}