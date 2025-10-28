using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;

namespace Logic.Domain.NintendoManagement.Contract.Font;

public interface INftrWriter
{
    void Write(Stream output, NftrData fontData);
}