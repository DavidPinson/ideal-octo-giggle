using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using yawna.Service.Interface;
using yawna.Util;

namespace yawna.Service
{
  public class NoteService : INoteService
  {
    private AsyncLock _lock = new AsyncLock();
    private IConfigService _configService;
    private ReplaySubject<string> _message = new ReplaySubject<string>(1);
    private ReplaySubject<string> _currentNote = new ReplaySubject<string>(1);
    private ReplaySubject<bool> _currentEditNoteDirty = new ReplaySubject<bool>(1);

    private List<string> _notes = new List<string>();
    private int _currentNoteIndex = 0;

    private string _currentEditedNote = string.Empty;

    public IObservable<string> Message => _message.AsObservable();
    public IObservable<string> CurrentNote => _currentNote.AsObservable();
    public IObservable<bool> CurrentEditNoteDirty => _currentEditNoteDirty.AsObservable();

    public NoteService(IConfigService configService)
    {
      _configService = configService;
      _currentEditNoteDirty.OnNext(false);
      LoadLinkAsync(_configService.HomeNoteFileName);
    }

    public async Task LoadLinkAsync(string link)
    {
      try
      {
        string filePathName = Path.Combine(_configService.NotesPath, link);

        if(IsLocalFile(filePathName) == true)
        {
          using(await _lock.LockAsync().ConfigureAwait(false))
          {
            _notes.Add(filePathName);
            _currentNoteIndex = _notes.Count - 1;

            await LoadNoteAtCurrentIndex().ConfigureAwait(false);
          }
        }
        else
        {
          _message.OnNext($"NoteService - LoadLinkAsync(string link): OpenBrowser({link})");
          Utility.OpenBrowser(link);
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - LoadLinkAsync(string link): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task ReloadCurrentNoteAsync()
    {
      try
      {
        using(await _lock.LockAsync().ConfigureAwait(false))
        {
          await LoadNoteAtCurrentIndex().ConfigureAwait(false);
          _currentEditedNote = string.Empty;
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - ReloadCurrentNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task LoadHomeNoteAsync()
    {
      try
      {
        _message.OnNext($"NoteService - LoadHomeNoteAsync()");
        await LoadLinkAsync(_configService.HomeNoteFileName).ConfigureAwait(false);
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - LoadHomeNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task LoadPrevNoteAsync()
    {
      try
      {
        using(await _lock.LockAsync().ConfigureAwait(false))
        {
          if(_currentNoteIndex - 1 >= 0)
          {
            _currentNoteIndex--;
            await LoadNoteAtCurrentIndex().ConfigureAwait(false);
          }
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - LoadPrevNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task LoadNextNoteAsync()
    {
      try
      {
        using(await _lock.LockAsync().ConfigureAwait(false))
        {
          if(_currentNoteIndex + 1 < _notes.Count)
          {
            _currentNoteIndex++;
            await LoadNoteAtCurrentIndex().ConfigureAwait(false);
          }
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - LoadNextNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task LoadSearchResultNoteAsync(string search)
    {
      Task.Delay(500).ConfigureAwait(false);
      _message.OnNext($"NoteService - LoadSearchResultNoteAsync(): search text: {search}");
      _currentNote.OnNext($"# Search({search}) not implemented yet! Comming soon! :)");
    }

    public async Task SetCurrentEditedNoteAsync(string note)
    {
      try
      {
        using(await _lock.LockAsync().ConfigureAwait(false))
        {
          _currentEditedNote = note;

          string currentNote = await LoadNoteFromFileAsync(_notes[_currentNoteIndex]).ConfigureAwait(false);
          if(currentNote.Equals(_currentEditedNote) == true)
          {
            _currentEditNoteDirty.OnNext(false);
          }
          else
          {
            _currentEditNoteDirty.OnNext(true);
          }
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - ReloadCurrentNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }
    public async Task SaveCurrentEditedNoteAsync()
    {
      try
      {
        using(await _lock.LockAsync().ConfigureAwait(false))
        {
          using(FileStream fs = new FileStream(_notes[_currentNoteIndex], FileMode.Truncate, FileAccess.ReadWrite))
          using(StreamWriter sw = new StreamWriter(fs))
          {
            await sw.WriteAsync(_currentEditedNote).ConfigureAwait(false);
          }
          _currentEditNoteDirty.OnNext(false);
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - ReloadCurrentNoteAsync(): {ex.Message}, {ex.StackTrace}");
      }
    }

    private static bool IsLocalFile(string filePathName)
    {
      return File.Exists(filePathName);
    }
    private async Task LoadNoteAtCurrentIndex()
    {
      string file = await LoadNoteFromFileAsync(_notes[_currentNoteIndex]).ConfigureAwait(false);
      if(string.IsNullOrWhiteSpace(file) == false)
      {
        _message.OnNext($"NoteService - LoadNoteAtCurrentIndex(): index: {_currentNoteIndex}, note: {_notes[_currentNoteIndex]}");
        _currentNote.OnNext(file);
      }
    }
    private static async Task<string> LoadNoteFromFileAsync(string filePathName)
    {
      string file = string.Empty;

      using(FileStream fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read))
      using(StreamReader sr = new StreamReader(fs))
      {
        file = await sr.ReadToEndAsync().ConfigureAwait(false);
      }

      return file;
    }
  }
}