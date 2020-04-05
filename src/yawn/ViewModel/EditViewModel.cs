using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using yawn.Service.Interface;

namespace yawn.ViewModel
{
  public class EditViewModel : ReactiveObject, IRoutableViewModel
  {
    private INoteService _noteService;
    private ReactiveCommand<string, Unit> _textChangedCmd;

    public string UrlPathSegment => "Edit view";

    public IScreen HostScreen { get; }

    [Reactive]
    public string DocumentText { get; set; }

    public EditViewModel(IScreen screen, INoteService noteService)
    {
      HostScreen = screen;
      _noteService = noteService;

      _textChangedCmd = ReactiveCommand.CreateFromTask<string>(_noteService.SetCurrentEditedNoteAsync);

      this
        .WhenAnyValue(x => x.DocumentText)
        .Throttle(TimeSpan.FromMilliseconds(800))
        .DistinctUntilChanged()
        .Where(x => string.IsNullOrWhiteSpace(x) == false)
        .InvokeCommand(_textChangedCmd);

      InitDocumentTextAsync();
    }

    private async Task InitDocumentTextAsync()
    {
      DocumentText = await _noteService
        .CurrentNote
        .FirstAsync();
    }

  }
}