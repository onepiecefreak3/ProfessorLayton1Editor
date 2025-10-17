using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.InternalContract;

public interface IFileHistory
{
    public int Index { get; }
    public int Count { get; }

    void Add(Layton1NdsFile file);

    Layton1NdsFile? Back();
    Layton1NdsFile? Forward();

    void Clear();
}