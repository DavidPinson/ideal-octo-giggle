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
            if(Path.GetFileName(s) != "searchResult.md")
            {
              _fileToIndexQueue.Enqueue(s);
            }
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
        if(Path.GetFileName(e.FullPath) != "searchResult.md")
        {
          using(_lockQueue.Lock())
          {
            _fileToIndexQueue.Enqueue(e.FullPath);
          }
        }
      }
      catch(Exception ex)
      {
        _message.OnNext($"SearchService - OnChanged: {ex.Message}, {ex.StackTrace}");
      }
    }
    private async Task IndexQueueAsync()
    {
      HashSet<string> filesToIndex = new HashSet<string>();
      using(await _lockQueue.LockAsync().ConfigureAwait(false))
      {
        int nbFile = _fileToIndexQueue.Count;
        for(int i = 0; i < nbFile; i++)
        {
          filesToIndex.Add(_fileToIndexQueue.Dequeue());
        }
      }

      string fileContent;
      foreach(string file in filesToIndex)
      {
        try
        {
          FileInfo fileInfo = new FileInfo(file);

          using(FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
          using(StreamReader sr = new StreamReader(fs))
          {
            fileContent = await sr.ReadToEndAsync().ConfigureAwait(false);
          }

          using(await _lockHoot.LockAsync().ConfigureAwait(false))
          {
            _hoot.RemoveDocument(fileInfo.FullName);
            _hoot.Index(new Document(fileInfo, fileContent), true);
          }
        }
        catch(Exception ex)
        {
          _message.OnNext($"SearchService - IndexQueueAsync: {ex.Message}, {ex.StackTrace}");
        }
      }

      if(filesToIndex.Count > 0)
      {
        using(await _lockHoot.LockAsync().ConfigureAwait(false))
        {
          _hoot.Save();
          _hoot.Shutdown();
          _hoot = null;
          await Task.Delay(2000).ConfigureAwait(false);
          _hoot = new Hoot(_indexPathName, _indexBaseFileName, true);
        }
      }
    }

  }
}