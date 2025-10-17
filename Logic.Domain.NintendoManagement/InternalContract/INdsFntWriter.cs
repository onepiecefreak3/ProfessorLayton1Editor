using Logic.Domain.NintendoManagement.Contract.DataClasses;

namespace Logic.Domain.NintendoManagement.InternalContract;

interface INdsFntWriter
{
    void WriteFnt(Stream input, int fntOffset, NdsContentFile[] files, int startFileId);
}