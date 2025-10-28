using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;

namespace Logic.Domain.NintendoManagement.Contract.Font;

public interface INftrReader
{
    NftrData Read(Stream input);
}