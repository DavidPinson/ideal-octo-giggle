using System;
using System.Threading.Tasks;

namespace yawn.Service.Interface
{
  public interface INoteService
  {
    IObservable<string> Message { get; }
    IObservable<string> CurrentNote { get; }

    string CurrentNoteName { get; }

    Task LoadLinkAsync(string link);
    Task ReloadCurrentNoteAsync();
    Task LoadHomeNoteAsync();
    Task LoadPrevNoteAsync();
    Task LoadNextNoteAsync();
    Task LoadSearchResultNoteAsync(string search);
  }
}