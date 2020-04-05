using System;
using System.IO;
using yawna.Service.Interface;

namespace yawna.Service
{
  public class ConfigService : IConfigService
  {
    public string NotesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "yawn/notes");

    public string HomeNoteFileName => "index.md";
  }
}