using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using yawn.Service.Interface;

namespace yawn.Service
{
  public class NoteService : INoteService
  {
    private ReplaySubject<string> _currentMenu = new ReplaySubject<string>(1);
    private ReplaySubject<string> _currentNote = new ReplaySubject<string>(1);

    public IObservable<string> CurrentMenu => _currentMenu.AsObservable();
    public IObservable<string> CurrentNote => _currentNote.AsObservable();
  }
}