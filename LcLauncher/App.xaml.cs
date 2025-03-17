using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

using ControlzEx.Theming;

using LcLauncher.Main;

namespace LcLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App: Application
{
  private void App_Startup(object sender, StartupEventArgs e)
  {
    DispatcherUnhandledException += (s, e) =>
      ProcessUnhandledException(e);
    Trace.TraceInformation($"App.App_Startup enter");
    ThemeManager.Current.ChangeTheme(this, "Dark.Olive");
    var mainWindow = new MainWindow();
    MainModel = new MainViewModel();
    mainWindow.DataContext = MainModel;

    // Todo: save and restore window size
    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    mainWindow.Width = 1280;
    mainWindow.Height = 1024;

    Trace.TraceInformation($"App.App_Startup showing main window");
    mainWindow.Show();
    Trace.TraceInformation($"App.App_Startup exit");
  }

  public MainViewModel? MainModel { get; private set; }

  private void Application_Exit(object sender, ExitEventArgs e)
  {
    Trace.TraceInformation("Application_Exit");
  }

  private void ProcessUnhandledException(
  System.Windows.Threading.DispatcherUnhandledExceptionEventArgs evt)
  {
    var ex = evt.Exception;
    Trace.TraceError($"Error: {ex}");
    MessageBox.Show(
      $"{ex.GetType().FullName}\n{ex.Message}",
      "Error",
      MessageBoxButton.OK,
      MessageBoxImage.Error);
    evt.Handled = MainWindow?.IsLoaded ?? false;
  }

}

