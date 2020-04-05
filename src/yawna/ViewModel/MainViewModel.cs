using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using yawna.Service;
using yawna.Service.Interface;
using yawna.View;

namespace yawna.ViewModel
{
  public class MainViewModel : ReactiveObject, IScreen
  {
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; }

    public ReactiveCommand<Unit, Unit> HomeCmd;
    public ReactiveCommand<Unit, Unit> NavigateBeforeCmd;
    public ReactiveCommand<Unit, Unit> NavigateAfterCmd;
    public ReactiveCommand<Unit, Unit> SearchCmd;
    public ReactiveCommand<Unit, Unit> SaveCmd;
    public ReactiveCommand<Unit, IRoutableViewModel> SettingsCmd;
    public ReactiveCommand<Unit, IRoutableViewModel> EditCmd;    
    public ReactiveCommand<Unit, IRoutableViewModel> CancelCmd;

    [Reactive]
    public bool IsInViewMode { get; set; }
    [Reactive]
    public bool IsInEditMode { get; set; }
    [Reactive]
    public string SearchTerm { get; set; }

    public MainViewModel()
    {
      this.Router = new RoutingState();
      IMutableDependencyResolver dependencyResolver = Locator.CurrentMutable;

      this.RegisterParts();

      IsInViewMode = true;
      IsInEditMode = false;

      // Navigate to the opening page of the application
      this.Router.Navigate.Execute(Locator.Current.GetService<NoteViewModel>());

      IObservable<bool> canExecuteViewCmd = this.WhenAnyValue(x => x.IsInViewMode, (isInViewMode) => isInViewMode == true);
      IObservable<bool> canExecuteSearchCmd = this
        .WhenAnyValue(
          x => x.IsInViewMode, x => x.SearchTerm.Length,
          (isInViewMode, searchTermLength) => isInViewMode == true && searchTermLength > 0);

      HomeCmd = ReactiveCommand.CreateFromTask(HomeCmdImpl, canExecuteViewCmd);
      NavigateBeforeCmd = ReactiveCommand.CreateFromTask(NavigateBeforeCmdImpl, canExecuteViewCmd);
      NavigateAfterCmd = ReactiveCommand.CreateFromTask(NavigateAfterCmdImpl, canExecuteViewCmd);
      SearchCmd = ReactiveCommand.CreateFromTask(SearchCmdImpl, canExecuteSearchCmd);
      SaveCmd = ReactiveCommand.CreateFromTask(SaveCmdImpl, Locator.Current.GetService<INoteService>().CurrentEditNoteDirty.ObserveOn(RxApp.MainThreadScheduler));

      SettingsCmd = ReactiveCommand.CreateFromObservable(SettingsCmdImpl, canExecuteViewCmd);
      EditCmd = ReactiveCommand.CreateFromObservable(EditCmdImpl, canExecuteViewCmd);
      CancelCmd = ReactiveCommand.CreateFromObservable(CancelCmdImpl);
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

    private Task HomeCmdImpl()
    {
      return Locator.Current.GetService<INoteService>().LoadHomeNoteAsync();
    }
    private Task NavigateBeforeCmdImpl()
    {
      return Locator.Current.GetService<INoteService>().LoadPrevNoteAsync();
    }
    private Task NavigateAfterCmdImpl()
    {
      return Locator.Current.GetService<INoteService>().LoadNextNoteAsync();
    }
    private Task SearchCmdImpl()
    {
      return Locator.Current.GetService<INoteService>().LoadSearchResultNoteAsync(SearchTerm);
    }
    private async Task SaveCmdImpl()
    {
      await Locator.Current.GetService<INoteService>().SaveCurrentEditedNoteAsync().ConfigureAwait(false);
      await Locator.Current.GetService<INoteService>().ReloadCurrentNoteAsync().ConfigureAwait(false);
    }
    private IObservable<IRoutableViewModel> SettingsCmdImpl()
    {
      IsInViewMode = false;
      IsInEditMode = true;
      return this.Router.Navigate.Execute(Locator.Current.GetService<EditViewModel>());
    }
    private IObservable<IRoutableViewModel> EditCmdImpl()
    {
      IsInViewMode = false;
      IsInEditMode = true;
      return this.Router.Navigate.Execute(Locator.Current.GetService<EditViewModel>());
    }
    private IObservable<IRoutableViewModel> CancelCmdImpl()
    {
      IsInViewMode = true;
      IsInEditMode = false;
      return this.Router.Navigate.Execute(Locator.Current.GetService<NoteViewModel>());
    }

  }
}