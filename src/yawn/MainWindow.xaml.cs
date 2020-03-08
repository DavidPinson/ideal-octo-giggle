using ReactiveUI;
using yawn.ViewModel;

namespace yawn
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : IActivatableView
  {
    public MainViewModel MainViewModel { get; protected set; }

    public MainWindow()
    {
      InitializeComponent();

      this.MainViewModel = new MainViewModel();
      this.DataContext = this.MainViewModel;

      this.WhenActivated(
        dispose =>
        {
          // d(CommonInteractions.CheckToProceed.RegisterHandler(
          //     async interaction =>
          //     {
          //       MessageDialogResult shouldContinue =
          //                         await
          //                             this.ShowMessageAsync(
          //                                 "Please confirm",
          //                                 interaction.Input,
          //                                 MessageDialogStyle.AffirmativeAndNegative);

          //       interaction.SetOutput(shouldContinue == MessageDialogResult.Affirmative);
          //     }));

          // d(CommonInteractions.GetStringResponse.RegisterHandler(
          //     async interaction =>
          //     {
          //       string input = await this.ShowInputAsync("Please confirm", interaction.Input);
          //       interaction.SetOutput(input);
          //     }));

          dispose(this.WhenAnyValue(x => x.MainViewModel).BindTo(this, x => x.DataContext));
        });
    }
  }
}
