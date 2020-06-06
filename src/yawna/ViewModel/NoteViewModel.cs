using System.Threading.Tasks;
using ReactiveUI;
using yawna.Service.Interface;

namespace yawna.ViewModel
{
  public class NoteViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;
    private IConfigService _configService;
    private ObservableAsPropertyHelper<string> _currentNoteMd;
    private ObservableAsPropertyHelper<string> _baseNotesPath;

    public string UrlPathSegment => "Note view";

    public string CurrentNoteMd => _currentNoteMd.Value;

    public string BaseNotesPath => _baseNotesPath.Value;

    public IScreen HostScreen { get; }

    public NoteViewModel(IScreen screen, INoteService noteService, IConfigService configService)
    {
      HostScreen = screen;
      _noteService = noteService;
      _configService = configService;

      _baseNotesPath = _configService
        .NotesPath
        .ToProperty(this, nameof(BaseNotesPath), scheduler: RxApp.MainThreadScheduler);

      _currentNoteMd = _noteService
        .CurrentNote
        .ToProperty(this, nameof(CurrentNoteMd), scheduler: RxApp.MainThreadScheduler);

      InitNoteViewModelAsync();
    }

    private async Task InitNoteViewModelAsync()
    {
      await _noteService.ReloadCurrentNoteAsync().ConfigureAwait(false);
    }
  }
}