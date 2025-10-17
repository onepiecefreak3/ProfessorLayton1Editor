using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace UI.Layton1Tool.Dialogs.DataClasses;

class SearchResult
{
    public required Layton1NdsFile File { get; init; }
    public PcmFile? SubFile { get; init; }
}