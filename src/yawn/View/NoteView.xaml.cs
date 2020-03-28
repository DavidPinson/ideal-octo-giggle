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
        dispose(this.WhenAnyValue(x => x.ViewModel.CurrentNoteMd).BindTo(this, x => x.NoteViewer.Markdown));

      });
    }

    private void HyperlinkCmd(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      //Process.Start(e.Parameter.ToString());
      //Viewer.Markdown = File.ReadAllText(e.Parameter.ToString());
      string link = e.Parameter.ToString();
    }

  }
}