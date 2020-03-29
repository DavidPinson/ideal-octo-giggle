using System.Reactive;
using ReactiveUI;
using Splat;
using yawn.Service;
using yawn.Service.Interface;
using yawn.View;

namespace yawn.ViewModel
{
  public class MainViewModel : ReactiveObject, IScreen
  {
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; }

    public ReactiveCommand<Unit, Unit> HomeCmd;
    public ReactiveCommand<Unit, Unit> NavigateBeforeCmd;
    public ReactiveCommand<Unit, Unit> NavigateAfterCmd;
    public ReactiveCommand<string, Unit> SearchCmd;
    public ReactiveCommand<Unit, Unit> SettingsCmd;
    public ReactiveCommand<Unit, Unit> EditCmd;

    public MainViewModel()
    {
      this.Router = new RoutingState();
      IMutableDependencyResolver dependencyResolver = Locator.CurrentMutable;

      this.RegisterParts();

      // Navigate to the opening page of the application
      this.Router.Navigate.Execute(Locator.Current.GetService<NoteViewModel>());

      HomeCmd = ReactiveCommand.CreateFromTask(Locator.Current.GetService<INoteService>().LoadHomeNoteAsync);
      NavigateBeforeCmd = ReactiveCommand.CreateFromTask(Locator.Current.GetService<INoteService>().LoadPrevNoteAsync);
      NavigateAfterCmd = ReactiveCommand.CreateFromTask(Locator.Current.GetService<INoteService>().LoadNextNoteAsync);
      SearchCmd = ReactiveCommand.CreateFromTask<string>(Locator.Current.GetService<INoteService>().LoadSearchResultNoteAsync);
    }

    private void RegisterParts()//(IMutableDependencyResolver dependencyResolver)
    {
      // Make sure Splat and ReactiveUI are already configured in the locator
      // so that our override runs last
      Locator.CurrentMutable.InitializeSplat();
      Locator.CurrentMutable.InitializeReactiveUI();

      Locator.CurrentMutable.RegisterConstant<IScreen>(this);
      Locator.CurrentMutable.RegisterConstant<IConfigService>(new ConfigService());
      Locator.CurrentMutable.RegisterConstant<INoteService>(new NoteService(Locator.Current.GetService<IConfigService>()));

      Locator.CurrentMutable.Register<EditViewModel>(() =>
      {
        return new EditViewModel(Locator.Current.GetService<IScreen>(), Locator.Current.GetService<INoteService>());
      });
      Locator.CurrentMutable.Register<NoteViewModel>(() => 
      {
        return new NoteViewModel(Locator.Current.GetService<IScreen>(), Locator.Current.GetService<INoteService>());
      });

      Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());
      Locator.CurrentMutable.Register<IViewFor<NoteViewModel>>(() => new NoteView());

      //   dependencyResolver.RegisterConstant<IRepositoryViewModelFactory>(new DefaultRepositoryViewModelFactory());
      //   dependencyResolver.RegisterConstant<IRepositoryFactory>(new DefaultRepositoryFactory());
      //   dependencyResolver.RegisterConstant<IWindowLayoutViewModel>(new WindowLayoutViewModel());
      //   dependencyResolver.Register<ILayoutViewModel>(() => new LayoutViewModel());

      //   dependencyResolver.Register<MainViewModel, IMainViewModel>();
      //   dependencyResolver.RegisterConstant<IActivationForViewFetcher>(new DispatcherActivationForViewFetcher());
      
      //   dependencyResolver.Register<IViewFor<IBranchViewModel>>(() => new BranchesView());
      //   dependencyResolver.Register<IViewFor<IRefLogViewModel>>(() => new RefLogView());
      //   dependencyResolver.Register<IViewFor<ICommitHistoryViewModel>>(() => new HistoryView());
      //   dependencyResolver.Register<IViewFor<IOutputViewModel>>(() => new OutputView());
      //   dependencyResolver.Register<IViewFor<IRepositoryDocumentViewModel>>(() => new RepositoryView());
      //   dependencyResolver.Register<IViewFor<ITagViewModel>>(() => new TagView());
    }

  }
}