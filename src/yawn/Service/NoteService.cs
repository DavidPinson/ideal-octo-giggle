using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using yawn.Service.Interface;

namespace yawn.Service
{
  public class NoteService : INoteService
  {    
    private ReplaySubject<string> _currentMenu = new ReplaySubject<string>(1);
    private ReplaySubject<string> _currentNote = new ReplaySubject<string>(1);

    public IObservable<string> CurrentMenu => _currentMenu.AsObservable();
    public IObservable<string> CurrentNote => _currentNote.AsObservable();

    public NoteService()
    {
      _currentMenu.OnNext("# Menu");
      _currentNote.OnNext("# Test\n\n[Some page](Documents/page.md)");
    }

    public void ChangeNote()
    {
      _currentNote.OnNext("## Une note rafraichie! haha!");
      _currentMenu.OnNext("# Menu\n\n[Some page](Documents/menu.md)");
    }

    public Task LoadLink()
    {
      throw new NotImplementedException();
    }
  }
}