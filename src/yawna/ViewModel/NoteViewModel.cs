using ReactiveUI;
using yawna.Service.Interface;

namespace yawna.ViewModel
{
  public class NoteViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;
    private ObservableAsPropertyHelper<string> _currentNoteMd;

    public string UrlPathSegment => "Note view";

    public string CurrentNoteMd => _currentNoteMd.Value;

    public IScreen HostScreen { get; }

    public NoteViewModel(IScreen screen, INoteService noteService)
    {
      HostScreen = screen;
      _noteService = noteService;

      _currentNoteMd = _noteService
        .CurrentNote
        .ToProperty(this, nameof(CurrentNoteMd), scheduler: RxApp.MainThreadScheduler);
    }
  }
}