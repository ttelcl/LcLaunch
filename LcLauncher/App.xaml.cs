﻿using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Configuration;

using ControlzEx.Theming;

using LcLauncher.Main;
using LcLauncher.Persistence;

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
        var pseudofile = Path.GetFileName(arg);
        var rack = MainModel.RackList.FindRackByPseudoFile(pseudofile);
        if(rack != null)
        {
          Trace.TraceInformation(
            $"Selecting rack '{rack}' specified on command line (as '{pseudofile}')");
          MainModel.RackList.SelectedRack = rack;
          rackSet = true;
        }
        else
        {
          Trace.TraceError(
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
        var errorMessage = LcLaunchStore.TestValidRackName(defaultRack);
        if(errorMessage != null)
        {
          Trace.TraceError(
            $"Default rack name '{defaultRack}' is not valid, using 'default' instead: {errorMessage}");
          defaultRack = "default";
        }
      }
      var pseudofile = $"{defaultRack}.rack-json";
      var rack = MainModel.RackList.FindRackByPseudoFile(pseudofile);
      if(rack != null)
      {
        Trace.TraceInformation(
          $"Selecting default rack '{rack}' because none were specified on command line");
        MainModel.RackList.SelectedRack = rack;
        rackSet = true;
      }
      else
      {
        Trace.TraceError(
          $"Not selecting any rack: none specified and '{pseudofile}' does not exist");
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

