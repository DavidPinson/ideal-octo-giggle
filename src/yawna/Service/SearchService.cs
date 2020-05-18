using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using hOOt;
using RaptorDB;
using yawna.Service.Interface;

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
      throw new NotImplementedException();
    }

    private async Task WorkerAsync()
    {
      while(true)
      {
        try
        {
          await InitFolderAndIndexAsync().ConfigureAwait(false);

          IEnumerable<string> result = _hoot.FindDocumentFileNames("powershell");

          string touper;
          foreach(string s in result)
          {
            touper = s.ToUpper();
          }

          await Task.Delay(_waitPeriod).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
          _message.OnNext($"SearchService - Worker: {ex.Message}, {ex.StackTrace}");
        }
      }
    }

    private async Task InitFolderAndIndexAsync()
    {
      if(_isIndexFolderInitialized == true)
      {
        return;
      }

      if(Directory.Exists(_indexPathName) == false)
      {
        Directory.CreateDirectory(_indexPathName);
      }

      if(File.Exists(Path.Combine(_indexPathName, $"{_indexBaseFileName}.words")) == false)
      {
        string notesPathName = await _configService.NotesPath.FirstAsync();
        string fileContent;
        DirectoryInfo directoryInfo = new DirectoryInfo(notesPathName);
        foreach(FileInfo file in directoryInfo.GetFiles("*.md", SearchOption.AllDirectories))
        {
          using(FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
          using(StreamReader sr = new StreamReader(fs))
          {
            fileContent = await sr.ReadToEndAsync().ConfigureAwait(false);
          }

          _hoot.Index(new Document(file, fileContent), true);
        }

        _hoot.OptimizeIndex();
        _hoot.Save();
        _hoot.Shutdown();
        _hoot = new Hoot(_indexPathName, _indexBaseFileName, true);
      }

      _isIndexFolderInitialized = true;
    }

  }
}