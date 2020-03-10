using ReactiveUI;
using Splat;
using yawn.View;

namespace yawn.ViewModel
{
  public class MainViewModel : ReactiveObject, IScreen
  {
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; }

    public MainViewModel()
    {
      this.Router = new RoutingState();
      IMutableDependencyResolver dependencyResolver = Locator.CurrentMutable;

      this.RegisterParts();

      // Navigate to the opening page of the application
      this.Router.Navigate.Execute(Locator.Current.GetService<NoteViewModel>());
    }

    private void RegisterParts()//(IMutableDependencyResolver dependencyResolver)
    {
      // Make sure Splat and ReactiveUI are already configured in the locator
      // so that our override runs last
      Locator.CurrentMutable.InitializeSplat();
      Locator.CurrentMutable.InitializeReactiveUI();

      Locator.CurrentMutable.RegisterConstant<IScreen>(this);

      Locator.CurrentMutable.Register<EditViewModel>(() => new EditViewModel(Locator.Current.GetService<IScreen>()));
      Locator.CurrentMutable.Register<NoteViewModel>(() => new NoteViewModel(Locator.Current.GetService<IScreen>()));

      //   dependencyResolver.RegisterConstant<IRepositoryViewModelFactory>(new DefaultRepositoryViewModelFactory());
      //   dependencyResolver.RegisterConstant<IRepositoryFactory>(new DefaultRepositoryFactory());
      //   dependencyResolver.RegisterConstant<IWindowLayoutViewModel>(new WindowLayoutViewModel());
      //   dependencyResolver.Register<ILayoutViewModel>(() => new LayoutViewModel());

      //   dependencyResolver.Register<MainViewModel, IMainViewModel>();
      //   dependencyResolver.RegisterConstant<IActivationForViewFetcher>(new DispatcherActivationForViewFetcher());
      Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());
      Locator.CurrentMutable.Register<IViewFor<NoteViewModel>>(() => new NoteView());
      //   dependencyResolver.Register<IViewFor<IBranchViewModel>>(() => new BranchesView());
      //   dependencyResolver.Register<IViewFor<IRefLogViewModel>>(() => new RefLogView());
      //   dependencyResolver.Register<IViewFor<ICommitHistoryViewModel>>(() => new HistoryView());
      //   dependencyResolver.Register<IViewFor<IOutputViewModel>>(() => new OutputView());
      //   dependencyResolver.Register<IViewFor<IRepositoryDocumentViewModel>>(() => new RepositoryView());
      //   dependencyResolver.Register<IViewFor<ITagViewModel>>(() => new TagView());
    }

  }
}