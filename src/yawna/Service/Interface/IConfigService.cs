using System;

namespace yawna.Service.Interface
{
  public interface IConfigService
  {
    IObservable<string> NotesPath { get; }
    IObservable<string> HomeNoteFileName { get; }
  }
}