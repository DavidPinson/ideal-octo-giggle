using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using fastJSON;
using yawna.Service.Interface;
using yawna.ViewModel.Config;

namespace yawna.Service
{
  public class ConfigService : IConfigService
  {
    private ReplaySubject<string> _notesPath = new ReplaySubject<string>(1);
    private ReplaySubject<string> _homeNoteFileName = new ReplaySubject<string>(1);

    public IObservable<string> NotesPath => _notesPath.AsObservable();
    public IObservable<string> HomeNoteFileName => _homeNoteFileName.AsObservable();

    public ConfigService()
    {
      InitConfigServiceAsync();
    }

    private async Task InitConfigServiceAsync()
    {
      Assembly currentAssem = Assembly.GetExecutingAssembly();
      string exePathFullName = Path.GetDirectoryName(currentAssem.Location);
      string configPathFileName = Path.Combine(exePathFullName, "config.json");

      YawnaConfig yc =new YawnaConfig()
      {
        NotesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"yawna\notes"),
        HomeNoteFileName = "index.md"
      };

      if(File.Exists(configPathFileName) == true)
      {
        using(FileStream fs = new FileStream(configPathFileName, FileMode.Open, FileAccess.Read))
        using(StreamReader sr = new StreamReader(fs))
        {
          string config = await sr.ReadToEndAsync().ConfigureAwait(false);
          yc = JSON.ToObject<YawnaConfig>(config);
        }
      }
      else
      {
        using(FileStream fs = new FileStream(configPathFileName, FileMode.Create, FileAccess.ReadWrite))
        using(StreamWriter sw = new StreamWriter(fs))
        {
          await sw.WriteAsync(JSON.ToNiceJSON(yc)).ConfigureAwait(false);
        }
      }

      _notesPath.OnNext(yc.NotesPath);
      _homeNoteFileName.OnNext(yc.HomeNoteFileName);
    }
  }
}
