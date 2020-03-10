using ReactiveUI;

namespace yawn.View
{
  public partial class EditView
  {
    public EditView()
    {
      InitializeComponent();

      this.WhenActivated(dispose =>
      {
        dispose(this.WhenAnyValue(x => x.ViewModel.UrlPathSegment).BindTo(this, x => x.PathTextBlock.Text));
      });
    }
  }
}