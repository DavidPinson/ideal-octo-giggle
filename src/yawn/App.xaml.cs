using System;
using System.Windows;
using MahApps.Metro;

namespace yawn
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      // get the current app style (theme and accent) from the application
      // you can then use the current theme and custom accent instead set a new theme
      // Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectTheme  .DetectAppStyle(Application.Current);

      //ThemeManager.ChangeTheme(Application.Current, "BaseDark");

      // now set the Green accent and dark theme
      // ThemeManager.ChangeAppStyle(Application.Current,
      //                             ThemeManager.GetAccent("Green"),
      //                             ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1

      base.OnStartup(e);
    }
  }
}
