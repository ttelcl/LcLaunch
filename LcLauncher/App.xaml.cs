using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

using Microsoft.Extensions.Configuration;

using ControlzEx.Theming;

using LcLauncher.Main;
using System;
using System.Linq;
using System.IO;

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

    // Load configuration
    var builder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
      .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
      /*.AddEnvironmentVariables()*/;
    var configuration = builder.Build();

    var defaultTheme = configuration["defaultTheme"];
    if(String.IsNullOrEmpty(defaultTheme))
    {
      defaultTheme = "Olive";
    }

    ThemeManager.Current.ChangeTheme(this, "Dark." + defaultTheme);
    var mainWindow = new MainWindow();
    MainModel = new MainViewModel(
      configuration);
    foreach(var arg in e.Args)
    {
      if(arg.EndsWith(".rack-json"))
      {
        var pseudofile = Path.GetFileName(arg);
        var rack = MainModel.RackList.FindRackByPseudoFile(pseudofile);
        if(rack != null)
        {
          Trace.TraceInformation(
            $"Selecting rack '{rack}' specified on command line (as '{pseudofile}')");
          MainModel.RackList.SelectedRack = rack;
        }
        else
        {
          Trace.TraceError(
            $"Rack '{arg}' not found");
        }
      }
    }
    mainWindow.DataContext = MainModel;

    //mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    //mainWindow.Width = 1280;
    //mainWindow.Height = 1024;

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

