using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement.Contract;

public interface INdsCompressionDetector
{
    NdsCompressionType Detect(NdsRom rom, NdsFile file);
}