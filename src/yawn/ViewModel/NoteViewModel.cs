using System.Reactive;
using ReactiveUI;
using Splat;
using yawn.Service.Interface;

namespace yawn.ViewModel
{
  public class NoteViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;
    private ObservableAsPropertyHelper<string> _currentMenuMd;
    private ObservableAsPropertyHelper<string> _currentNoteMd;

    public ReactiveCommand<Unit, Unit> HyperlinkCmd;

    public string UrlPathSegment => "Note view";

    public string CurrentMenuMd => _currentMenuMd.Value;
    public string CurrentNoteMd => _currentNoteMd.Value;





    // public string CurrentNoteMd => "##Je suis une note";
    // private string _currentMenuMd;
    // public string CurrentMenuMd
    // {
    //   get => _currentMenuMd;
    //   set => this.RaiseAndSetIfChanged(ref this._currentMenuMd, value);
    // }

    public IScreen HostScreen { get; }

    public NoteViewModel(IScreen screen, INoteService noteService)
    {
      HostScreen = screen;
      _noteService = noteService;

      _currentMenuMd = _noteService
        .CurrentMenu
        .ToProperty(this, nameof(CurrentMenuMd));

      _currentNoteMd = _noteService
        .CurrentNote
        .ToProperty(this, nameof(CurrentNoteMd));

      HyperlinkCmd = ReactiveCommand.Create(() =>
      {
        Locator
          .Current
          .GetService<INoteService>()
          .ChangeNote();
      });

      //ChangeText = ReactiveCommand.Create(() => { CurrentMenuMd = "### je suis un crif de menu!"; });
    }
  }
}