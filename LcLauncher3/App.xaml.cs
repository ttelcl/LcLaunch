using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Configuration;

using ControlzEx.Theming;

using LcLauncher.Main;
using Ttelcl.Persistence.API;
//using LcLauncher.Persistence;

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
      defaultTheme = MainViewModel.DefaultDefaultTheme;
    }

    ThemeManager.Current.ChangeTheme(this, "Dark." + defaultTheme);
    var mainWindow = new MainWindow();
    MainModel = new MainViewModel(
      configuration);
    var rackSet = false;
    foreach(var arg in e.Args)
    {
      if(arg.EndsWith(".rack-json"))
      {
        var isSet = MainModel.RackList.SelectFromCommandlineArgument(arg);
        if(isSet)
        {
          Trace.TraceInformation(
            $"Selected rack as specified on command line (as '{arg}')");
          rackSet = true;
          break;
        }
        else
        {
          Trace.TraceWarning(
            $"Rack '{arg}' not found");
        }
      }
    }
    if(!rackSet)
    {
      var defaultRack = configuration["defaultRack"];
      if(String.IsNullOrEmpty(defaultRack))
      {
        defaultRack = "default";
      }
      else
      {
        if(!NamingRules.IsValidStoreName(defaultRack))
        {
          Trace.TraceError(
            $"Default rack name '{defaultRack}' is not valid, using 'default' instead.");
          defaultRack = "default";
        }
      }
      var isSet = MainModel.RackList.SelectFromRackName(defaultRack);
      if(isSet)
      {
        Trace.TraceInformation(
          $"Selecting default rack '{defaultRack}' because none were specified on command line");
        rackSet = true;
      }
      else
      {
        Trace.TraceError(
          $"Not selecting any rack: none specified and '{defaultRack}' not found");
      }
    }
    mainWindow.DataContext = MainModel;

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

