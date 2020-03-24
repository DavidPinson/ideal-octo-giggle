using ReactiveUI;
using yawn.Service.Interface;

namespace yawn.ViewModel
{
  public class EditViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;

    public string UrlPathSegment => "Edit view";

    public IScreen HostScreen { get; }

    public EditViewModel(IScreen screen, INoteService noteService)
    {
      HostScreen = screen;
      _noteService = noteService;
    }
  }
}