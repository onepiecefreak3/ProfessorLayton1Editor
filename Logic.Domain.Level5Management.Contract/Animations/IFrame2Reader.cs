using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Contract.Animations;

public interface IFrame2Reader
{
    Frame2Container Read(Stream input);
}