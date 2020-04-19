using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using yawna.Service.Interface;

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
      _notesPath.OnNext(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"yawn\notes"));
      _homeNoteFileName.OnNext("index.md");
    }
  }
}