using System.Reactive;
using ReactiveUI;
using yawn.Service.Interface;

namespace yawn.ViewModel
{
  public class NoteViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;

    //public ReactiveCommand<Unit, Unit> ChangeText;

    public string UrlPathSegment => "Note view";
    public string CurrentNoteMd => "##Je suis une note";
    private string _currentMenuMd;
    public string CurrentMenuMd
    {
      get => _currentMenuMd;
      set => this.RaiseAndSetIfChanged(ref this._currentMenuMd, value);
    }

    public IScreen HostScreen { get; }

    public NoteViewModel(IScreen screen, INoteService noteService)
    {
      HostScreen = screen;
      _noteService = noteService;

      CurrentMenuMd = "##je suis un menu";

      //ChangeText = ReactiveCommand.Create(() => { CurrentMenuMd = "### je suis un crif de menu!"; });
    }
  }
}