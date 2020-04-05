using ReactiveUI;

namespace yawna.View
{
  public partial class EditView
  {
    public EditView()
    {
      InitializeComponent();

      this.WhenActivated(dispose =>
      {
        dispose(this.Bind(this.ViewModel, x => x.DocumentText, x => x.AvalonTextEditor.Document.Text));
      });
    }
  }
}