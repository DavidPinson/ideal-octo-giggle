using System;

namespace yawn.Service.Interface
{
  public interface INoteService
  {
    IObservable<string> CurrentMenu { get; }
    IObservable<string> CurrentNote { get; }
  }
}