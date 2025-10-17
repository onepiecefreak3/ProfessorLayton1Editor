using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace UI.Layton1Tool.Messages;

public record SelectedAnimationsChangedMessage(Layton1NdsRom Rom, AnimationSequences AnimationSequences);