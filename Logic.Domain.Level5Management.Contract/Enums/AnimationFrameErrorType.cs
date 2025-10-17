namespace Logic.Domain.Level5Management.Contract.Enums;

[Flags]
public enum AnimationFrameErrorType
{
    None = 0,
    InconsistentEncoding = 1 << 0,
    InconsistentCoordinates = 1 << 1
}