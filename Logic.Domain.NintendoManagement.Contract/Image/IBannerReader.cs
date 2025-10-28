using Logic.Domain.NintendoManagement.Contract.DataClasses.Image;

namespace Logic.Domain.NintendoManagement.Contract.Image;

public interface IBannerReader
{
    BannerData Read(Stream input);
}

