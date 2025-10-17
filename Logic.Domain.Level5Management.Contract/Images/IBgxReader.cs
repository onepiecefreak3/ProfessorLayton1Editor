using Logic.Domain.Level5Management.Contract.DataClasses.Images;

namespace Logic.Domain.Level5Management.Contract.Images;

public interface IBgxReader
{
    BgxContainer Read(Stream input);
}