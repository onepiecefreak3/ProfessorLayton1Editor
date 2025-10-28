using ImGui.Forms.Controls.Base;
using Konnect.Contract.DataClasses.Plugin.File.Font;

namespace UI.Layton1Tool.Messages;

public record SelectedCharacterInfoChangedMessage(Component Target, CharacterInfo CharacterInfo);