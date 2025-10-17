using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;

namespace UI.Layton1Tool.Forms;

class FileHistory : IFileHistory
{
    private readonly List<Layton1NdsFile> _history = [];

    public int Index { get; private set; }
    public int Count => _history.Count;

    public void Add(Layton1NdsFile file)
    {
        if (_history.Count > 0)
        {
            if (Index + 1 < _history.Count)
            {
                int truncateCount = _history.Count - Index - 1;
                _history.RemoveRange(Index + 1, truncateCount);
            }

            Index++;
        }

        _history.Add(file);
    }

    public Layton1NdsFile? Back()
    {
        return Index <= 0 ? null : _history[--Index];
    }

    public Layton1NdsFile? Forward()
    {
        return Index + 1 >= _history.Count ? null : _history[++Index];
    }

    public void Clear()
    {
        Index = 0;
        _history.Clear();
    }
}