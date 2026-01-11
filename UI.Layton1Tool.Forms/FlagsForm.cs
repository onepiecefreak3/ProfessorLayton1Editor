using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

internal abstract partial class FlagsForm
{
    private readonly ILocalizationProvider _localizations;

    protected GameState? States { get; set; }

    public FlagsForm(ILocalizationProvider localizations)
    {
        InitializeComponent(localizations);

        _localizations = localizations;
    }

    protected void UpdateStates(GameState states)
    {
        _puzzleChecks.Items.Clear();
        _byteFlags.Items.Clear();
        _bitFlags.Items.Clear();

        foreach (int puzzleId in states.Puzzles.Keys)
        {
            var seenBox = new CheckBox(_localizations.ScriptPuzzleSeenText) { Checked = states.Puzzles[puzzleId].Seen };
            seenBox.CheckChanged += (_, _) => TogglePuzzleSeen(puzzleId, seenBox.Checked);

            var solvedBox = new CheckBox(_localizations.ScriptPuzzleSolvedText) { Checked = states.Puzzles[puzzleId].Solved };
            solvedBox.CheckChanged += (_, _) => TogglePuzzleSolved(puzzleId, solvedBox.Checked);

            var finalSolvedBox = new CheckBox(_localizations.ScriptPuzzleFinalSolvedText) { Checked = states.Puzzles[puzzleId].FinalSolved };
            finalSolvedBox.CheckChanged += (_, _) => TogglePuzzleFinalSolved(puzzleId, finalSolvedBox.Checked);

            _puzzleChecks.Items.Add(new StackLayout
            {
                Alignment = Alignment.Vertical,
                Size = Size.Content,
                ItemSpacing = 5,
                Items =
                {
                    new Label(_localizations.ScriptPuzzleCaption(puzzleId)),
                    new StackLayout
                    {
                        Alignment = Alignment.Horizontal,
                        Size = Size.Content,
                        ItemSpacing = 5,
                        Items =
                        {
                            seenBox,
                            solvedBox,
                            finalSolvedBox
                        }
                    }
                }
            });
        }

        foreach (int byteFlag in states.ByteFlags.Keys)
        {
            var flagBox = new CheckBox(_localizations.ScriptFlagText(byteFlag)) { Checked = states.ByteFlags[byteFlag] };
            flagBox.CheckChanged += (_, _) => ToggleByteFlag(byteFlag, flagBox.Checked);

            _byteFlags.Items.Add(flagBox);
        }

        foreach (int bitFlag in states.BitFlags.Keys)
        {
            var flagBox = new CheckBox(_localizations.ScriptFlagText(bitFlag)) { Checked = states.BitFlags[bitFlag] };
            flagBox.CheckChanged += (_, _) => ToggleBitFlag(bitFlag, flagBox.Checked);

            _bitFlags.Items.Add(flagBox);
        }

        _scriptReturnBox.CheckChanged -= ScriptReturnBox_CheckChanged;
        _scriptReturnBox.Checked = states.IsScriptReturn;
        _scriptReturnBox.CheckChanged += ScriptReturnBox_CheckChanged;

        _scriptSolvedBox.CheckChanged -= ScriptSolvedBox_CheckChanged;
        _scriptSolvedBox.Checked = states.IsScriptSolved;
        _scriptSolvedBox.CheckChanged += ScriptSolvedBox_CheckChanged;

        _scriptReturnBox.Enabled = true;
        _scriptSolvedBox.Enabled = true;

        _solvedCountText.TextChanged -= SolvedCount_TextChanged;
        _solvedCountText.Text = $"{states.State}";
        _solvedCountText.TextChanged += SolvedCount_TextChanged;

        _stateText.TextChanged -= State_TextChanged;
        _stateText.Text = $"{states.DialogIndex}";
        _stateText.TextChanged += State_TextChanged;

        _dialogIndexText.TextChanged -= DialogIndex_TextChanged;
        _dialogIndexText.Text = $"{states.DialogIndex}";
        _dialogIndexText.TextChanged += DialogIndex_TextChanged;

        _solvedCountText.IsReadOnly = false;
        _stateText.IsReadOnly = false;
        _dialogIndexText.IsReadOnly = false;
    }

    protected abstract void OnFlagsChanged();

    private void ScriptReturnBox_CheckChanged(object? sender, EventArgs e)
    {
        if (States is null)
            return;

        States.IsScriptReturn = _scriptReturnBox.Checked;

        OnFlagsChanged();
    }

    private void ScriptSolvedBox_CheckChanged(object? sender, EventArgs e)
    {
        if (States is null)
            return;

        States.IsScriptSolved = _scriptSolvedBox.Checked;

        OnFlagsChanged();
    }

    private void SolvedCount_TextChanged(object? sender, EventArgs e)
    {
        if (States is null)
            return;

        if (!int.TryParse(_solvedCountText.Text, out int state))
            return;

        States.SolvedCount = state;

        OnFlagsChanged();
    }

    private void State_TextChanged(object? sender, EventArgs e)
    {
        if (States is null)
            return;

        if (!int.TryParse(_stateText.Text, out int state))
            return;

        States.State = state;

        OnFlagsChanged();
    }

    private void DialogIndex_TextChanged(object? sender, EventArgs e)
    {
        if (States is null)
            return;

        if (!int.TryParse(_dialogIndexText.Text, out int count))
            return;

        States.DialogIndex = count;

        OnFlagsChanged();
    }

    private void TogglePuzzleSeen(int puzzleId, bool toggle)
    {
        if (States is null)
            return;

        if (States.Puzzles.TryGetValue(puzzleId, out var flags))
            States.Puzzles[puzzleId] = (toggle, flags.Solved, flags.FinalSolved);

        OnFlagsChanged();
    }

    private void TogglePuzzleSolved(int puzzleId, bool toggle)
    {
        if (States is null)
            return;

        if (States.Puzzles.TryGetValue(puzzleId, out var flags))
            States.Puzzles[puzzleId] = (flags.Seen, toggle, flags.FinalSolved);

        OnFlagsChanged();
    }

    private void TogglePuzzleFinalSolved(int puzzleId, bool toggle)
    {
        if (States is null)
            return;


        if (States.Puzzles.TryGetValue(puzzleId, out var flags))
            States.Puzzles[puzzleId] = (flags.Seen, flags.Solved, toggle);

        OnFlagsChanged();
    }

    private void ToggleByteFlag(int flag, bool toggle)
    {
        if (States is null)
            return;

        States.ByteFlags[flag] = toggle;

        OnFlagsChanged();
    }

    private void ToggleBitFlag(int flag, bool toggle)
    {
        if (States is null)
            return;

        States.BitFlags[flag] = toggle;

        OnFlagsChanged();
    }
}