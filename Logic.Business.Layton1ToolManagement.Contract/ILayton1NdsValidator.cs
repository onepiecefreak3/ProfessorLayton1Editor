using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1NdsValidator
{
    Layton1ValidationError? Validate(Layton1NdsFile file);
}