using System;
using System.Threading.Tasks;

namespace yawn.Service.Interface
{
  public interface INoteService
  {
    IObservable<string> CurrentMenu { get; }
    IObservable<string> CurrentNote { get; }

    void ChangeNote();
    Task LoadLink();
  }
}