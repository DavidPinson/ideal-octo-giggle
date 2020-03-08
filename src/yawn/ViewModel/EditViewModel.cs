using ReactiveUI;

namespace yawn.ViewModel
{
  public class EditViewModel : ReactiveObject, IRoutableViewModel
  {
    public string UrlPathSegment => "Edit view";

    public IScreen HostScreen { get; }

    public EditViewModel(IScreen screen)
    {
      HostScreen = screen;
    }
  }
}