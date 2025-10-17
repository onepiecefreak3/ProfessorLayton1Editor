using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Contract.Animations;

public interface IFrame3Reader
{
    Frame3Container Read(Stream input);
}