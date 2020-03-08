using ReactiveUI;

namespace yawn.ViewModel
{
  public class NoteViewModel : ReactiveObject, IRoutableViewModel
  {
    public string UrlPathSegment => "Note view";

    public IScreen HostScreen { get; }

    public NoteViewModel(IScreen screen)
    {
      HostScreen = screen;
    }
  }
}