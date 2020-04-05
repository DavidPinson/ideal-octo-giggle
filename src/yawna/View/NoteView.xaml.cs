using ReactiveUI;
using Splat;
using yawna.Service.Interface;

namespace yawna.View
{
  public partial class NoteView
  {    
    public NoteView()
    {
      InitializeComponent();

      this.WhenActivated(dispose =>
      {
        dispose(this.WhenAnyValue(x => x.ViewModel.CurrentNoteMd).BindTo(this, x => x.NoteViewer.Markdown));
      });
    }

    private async void HyperlinkCmd(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      await Locator.Current.GetService<INoteService>()?.LoadLinkAsync(e.Parameter.ToString());
    }

  }
}