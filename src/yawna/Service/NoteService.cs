using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using yawna.Service.Interface;
using yawna.Util;

namespace yawna.Service
{
  public class NoteService : INoteService
  {
    private AsyncLock _lock = new AsyncLock();
    private IConfigService _configService;
    private ISearchService _searchService;
    private ReplaySubject<string> _message = new ReplaySubject<string>(1);
    private ReplaySubject<string> _currentNote = new ReplaySubject<string>(1);
    private ReplaySubject<bool> _currentEditNoteDirty = new ReplaySubject<bool>(1);

    private List<string> _notes = new List<string>();
    private int _currentNoteIndex = 0;
    private string _notesPath;
    private string _homeNoteFileName;

    private string _currentEditedNote = string.Empty;

    public IObservable<string> Message => _message.AsObservable();
    public IObservable<string> CurrentNote => _currentNote.AsObservable();
    public IObservable<bool> CurrentEditNoteDirty => _currentEditNoteDirty.AsObservable();

    public NoteService(IConfigService configService, ISearchService searchService)
    {
      _configService = configService;
      _searchService = searchService;
      _currentEditNoteDirty.OnNext(false);

      InitNoteServiceAsync();
    }
    private async Task InitNoteServiceAsync()
    {
      _notesPath = await _configService
        .NotesPath
        .FirstAsync();

      _homeNoteFileName = await _configService
        .HomeNoteFileName
        .FirstAsync();

      _configService
        .NotesPath
        .Subscribe(notePath =>
        {
          try
          {
            _notesPath = notePath;
          }
          catch(Exception ex)
          {
            _message.OnNext($"NoteService - NotePath Subscription: {ex.Message}, {ex.StackTrace}");
          }
        });

      _configService
        .HomeNoteFileName
        .Subscribe(homeNoteFileName =>
        {
          try
          {
            _homeNoteFileName = homeNoteFileName;
          }
          catch(Exception ex)
          {
            _message.OnNext($"NoteService - HomeNoteFileName Subscription: {ex.Message}, {ex.StackTrace}");
          }
        });

      await LoadLinkAsync(_homeNoteFileName).ConfigureAwait(false);
    }

    public async Task LoadLinkAsync(string link)
    {
      try
      {
        // web link
        if(Uri.IsWellFormedUriString(link, UriKind.Absolute) == true)
        {
          _message.OnNext($"NoteService - LoadLinkAsync(string link): OpenBrowser({link})");
          Utility.OpenBrowser(link);
        }
        else
        {
          string filePathName = Path.Combine(_notesPath, link);

          // note file
          if(Path.GetExtension(filePathName) == ".md")
          {
            if(File.Exists(filePathName) == false)
            {
              if(Directory.Exists(Path.GetDirectoryName(filePathName)) == false)
              {
                Directory.CreateDirectory(Path.GetDirectoryName(filePathName));
              }

              // create new note
              using(File.Create(filePathName)) { }
            }

            using(await _lock.LockAsync().ConfigureAwait(false))
            {
              _notes.Add(filePathName);
              _currentNoteIndex = _notes.Count - 1;

              _message.OnNext($"NoteService - LoadLinkAsync(string link): Open existing note({link})");
              await LoadNoteAtCurrentIndex().ConfigureAwait(false);
            }
          }
          else
          {
            // doc reference
            _message.OnNext($"NoteService - LoadLinkAsync(string link): Open ref link({filePathName})");
            Utility.OpenBrowser(filePathName);
          }
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
        await LoadLinkAsync(_homeNoteFileName).ConfigureAwait(false);
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
      try
      {
        List<string> results = await _searchService.QueryAsync(search).ConfigureAwait(false);

        StringBuilder sb = new StringBuilder();
        string[] compiledFilter = CompileFilter(search);

        sb.AppendLine($"# Search Results");
        sb.AppendLine();
        foreach(string note in results)
        {
          sb.AppendLine($"* [{Path.GetFileName(note)}]({GetRelativeFilePathName(note, _notesPath)})");

          List<string> fileSearchResult = await GetSearchLines(note, compiledFilter).ConfigureAwait(false);
          foreach(string s in fileSearchResult)
          {
            sb.AppendLine($"  * {s}");
          }
        }

        using(FileStream fs = new FileStream(Path.Combine(_notesPath, "searchResult.md"), FileMode.Create, FileAccess.ReadWrite))
        using(StreamWriter sw = new StreamWriter(fs))
        {
          await sw.WriteAsync(sb.ToString()).ConfigureAwait(false);
        }

        await LoadLinkAsync("searchResult.md").ConfigureAwait(false);
      }
      catch(Exception ex)
      {
        _message.OnNext($"NoteService - LoadSearchResultNoteAsync():  {ex.Message}, {ex.StackTrace}");
      }
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

    private async Task LoadNoteAtCurrentIndex()
    {
      string file = await LoadNoteFromFileAsync(_notes[_currentNoteIndex]).ConfigureAwait(false);
      _message.OnNext($"NoteService - LoadNoteAtCurrentIndex(): index: {_currentNoteIndex}, note: {_notes[_currentNoteIndex]}");
      _currentNote.OnNext(file);
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
    private string[] CompileFilter(string filter)
    {
      string simplifiedFilter = filter
        .Replace("+", "")
        .Replace("-", "")
        .Replace("*", "")
        .Replace("?", "");

      return simplifiedFilter.Split(" ");
    }
    private string GetRelativeFilePathName(string absoluteFilePathName, string notesPath)
    {
      string relativePathFileName = absoluteFilePathName
        .Replace(Path.GetFullPath(notesPath), "")
        .Replace(@"\", "/")
        .Substring(1);

      return relativePathFileName;
    }
    private static async Task<List<string>> GetSearchLines(string noteFilePathName, string[] filter)
    {
      List<string> results = new List<string>();

      string? line;
      using(FileStream fs = new FileStream(noteFilePathName, FileMode.Open, FileAccess.Read))
      using(StreamReader sr = new StreamReader(fs))
      {
        line = await sr.ReadLineAsync().ConfigureAwait(false);
        while(line != null)
        {
          if(filter.Where(i => line.Contains(i, StringComparison.OrdinalIgnoreCase)).Any() == true)
          {
            results.Add(line.Replace("*","").Replace("#", ""));
          }

          line = await sr.ReadLineAsync().ConfigureAwait(false);
        }
      }

      return results;
    }
  }
}