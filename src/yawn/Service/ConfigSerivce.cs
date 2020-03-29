using System;
using System.IO;
using yawn.Service.Interface;

namespace yawn.Service
{
  public class ConfigService : IConfigService
  {
    public string NotesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "yawn/notes");

    public string HomeNoteFileName => "menu.md";
  }
}