using Markdig;
using Markdig.Wpf;
using ReactiveUI;

namespace yawn.View
{
  public partial class NoteView
  {    
    public NoteView()
    {
      InitializeComponent();

      this.WhenActivated(dispose =>
      {
        dispose(this.WhenAnyValue(x => x.ViewModel.CurrentMenuMd).BindTo(this, x => x.MenuViewer.Markdown));
        //dispose(this.WhenAnyValue(x => x.ViewModel.CurrentNoteMd).BindTo(this, x => x.NoteViewer.Markdown));

        //dispose(this.BindCommand(this.ViewModel, vm=>vm.ChangeText, v => v.GoNextButton));
      });
    }
  }
}