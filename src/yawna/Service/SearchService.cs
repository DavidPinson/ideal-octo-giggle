using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;
using hOOt;
using RaptorDB;
using yawna.Service.Interface;
using yawna.Util;

namespace yawna.Service
{
  public class SearchService : ISearchService
  {
    private ReplaySubject<string> _message = new ReplaySubject<string>(1);

    private bool _isIndexFolderInitialized = false;
    private string _indexPathName;
    private string _indexBaseFileName = "index";
    private IConfigService _configService;
    private Hoot _hoot;
    private TimeSpan _waitPeriod = new TimeSpan(0, 0, 1);
    private AsyncLock _lockHoot = new AsyncLock();
    private AsyncLock _lockQueue = new AsyncLock();
    private Queue<string> _fileToIndexQueue = new Queue<string>();

    private Task _worker;

    public IObservable<string> Message => _message.AsObservable();

    public SearchService(IConfigService configService)
    {
      Assembly currentAssem = Assembly.GetExecutingAssembly();
      string exePathFullName = Path.GetDirectoryName(currentAssem.Location);
      _indexPathName = Path.Combine(exePathFullName, "searchindex");

      _hoot = new Hoot(_indexPathName, _indexBaseFileName, true);

      _configService = configService;

      _worker = Task.Run(WorkerAsync);
    }

    public Task<List<string>> QueryAsync(string filter)
    {
      return Task.Run(async () =>
      {
        List<string> results;

        try
        {
          using(await _lockHoot.LockAsync().ConfigureAwait(false))
          {
            results = _hoot.FindDocumentFileNames(filter).ToList();
          }
        }
        catch(Exception ex)
        {
          results = new List<string>();
          _message.OnNext($"SearchService - QueryAsync: {ex.Message}, {ex.StackTrace}");
        }

        return results;
      });
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    private async Task WorkerAsync()
    {
      using(FileSystemWatcher watcher = new FileSystemWatcher())
      {
        while(true)
        {
          try
          {
            await InitFolderAsync(watcher).ConfigureAwait(false);
            await IndexQueueAsync().ConfigureAwait(false);

            await Task.Delay(_waitPeriod).ConfigureAwait(false);
          }
          catch(Exception ex)
          {
            _message.OnNext($"SearchService - Worker: {ex.Message}, {ex.StackTrace}");
          }
        }
      }
    }
    private async Task InitFolderAsync(FileSystemWatcher watcher)
    {
      if(_isIndexFolderInitialized == true)
      {
        return;
      }

      if(Directory.Exists(_indexPathName) == false)
      {
        Directory.CreateDirectory(_indexPathName);
      }

      string notesPathName = await _configService.NotesPath.FirstAsync();

      if(File.Exists(Path.Combine(_indexPathName, $"{_indexBaseFileName}.words")) == false)
      {
        string[] notes = Directory.GetFiles(notesPathName, "*.md", SearchOption.AllDirectories);

        using(await _lockQueue.LockAsync().ConfigureAwait(false))
        {
          foreach(string s in notes)
          {
            _fileToIndexQueue.Enqueue(s);
          }
        }
      }

      watcher.Path = notesPathName;
      watcher.IncludeSubdirectories = true;
      watcher.NotifyFilter = NotifyFilters.LastWrite;
      watcher.Filter = "*.md";
      watcher.Changed += OnChanged;
      watcher.EnableRaisingEvents = true;

      _isIndexFolderInitialized = true;
    }
    private void OnChanged(object source, FileSystemEventArgs e)
    {
      try
      {
        using(_lockQueue.Lock())
        {
          _fileToIndexQueue.Enqueue(e.FullPath);
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"SearchService - OnChanged: {ex.Message}, {ex.StackTrace}");
      }
    }
    private async Task IndexQueueAsync()
    {
      int nbFileToIndex;
      using(await _lockQueue.LockAsync().ConfigureAwait(false))
      {
        nbFileToIndex = _fileToIndexQueue.Count;
      }

      if(nbFileToIndex == 0)
      {
        return;
      }

      string fileToIndex;
      string fileContent;
      for(int i = 0; i < nbFileToIndex; i++)
      {
        try
        {
          using(await _lockQueue.LockAsync().ConfigureAwait(false))
          {
            fileToIndex = _fileToIndexQueue.Dequeue();
          }

          FileInfo fileInfo = new FileInfo(fileToIndex);

          using(FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
          using(StreamReader sr = new StreamReader(fs))
          {
            fileContent = await sr.ReadToEndAsync().ConfigureAwait(false);
          }

          using(await _lockHoot.LockAsync().ConfigureAwait(false))
          {
            _hoot.Index(new Document(fileInfo, fileContent), true);
          }
        }
        catch(Exception ex)
        {
          _message.OnNext($"SearchService - IndexQueueAsync: {ex.Message}, {ex.StackTrace}");
        }
      }

      using(await _lockHoot.LockAsync().ConfigureAwait(false))
      {
        _hoot.OptimizeIndex();
        _hoot.Save();
      }
    }

  }
}