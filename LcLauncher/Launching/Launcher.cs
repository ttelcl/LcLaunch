/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LcLauncher.Models;

namespace LcLauncher.Launching;

public static class Launcher
{

  public static void Launch(LaunchData launchData)
  {
    if(launchData.ShellMode)
    {
      LaunchShellMode(launchData);
    }
    else
    {
      LaunchRawMode(launchData);
    }
  }

  private static void LaunchRawMode(LaunchData rawLaunch)
  {
    if(rawLaunch.ShellMode)
    {
      throw new InvalidOperationException(
        "Expecting a raw mode LaunchData here");
    }
    var fileName = rawLaunch.Target;
    if(!File.Exists(fileName))
    {
      throw new InvalidOperationException(
        $"Cannot launch: file '{fileName}' no longer exists");
    }
    var startInfo = new ProcessStartInfo {
      FileName = fileName,
      UseShellExecute = false,
      WindowStyle = rawLaunch.WindowStyle,
    };
    if(!String.IsNullOrEmpty(rawLaunch.WorkingDirectory))
    {
      startInfo.WorkingDirectory = rawLaunch.WorkingDirectory;
    }
    if(rawLaunch.Arguments.Count > 0)
    {
      foreach(var arg in rawLaunch.Arguments)
      {
        startInfo.ArgumentList.Add(arg);
      }
    }
    if(rawLaunch.Environment.Count > 0)
    {
      foreach(var env in rawLaunch.Environment)
      {
        if(String.IsNullOrEmpty(env.Value))
        {
          startInfo.Environment.Remove(env.Key);
        }
        else
        {
          startInfo.Environment[env.Key] = env.Value;
        }
      }
    }
    if(rawLaunch.PathEnvironment.Count > 0)
    {
      MessageBox.Show(
        "Edits to Path-like environment variables are not yet implemented. Ignoring them.",
        "Warning",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);
    }
    var process = Process.Start(startInfo);
    if(process != null)
    {
      //Trace.TraceInformation(
      //  $"Started process {process.Id} ({process.MainModule?.FileName ?? String.Empty})");
      // Accessing MainModule may fail if the process is not yet running
      Trace.TraceInformation(
        $"Started Raw Mode process {process.Id}, target = {rawLaunch.Target}");
    }
    else
    {
      Trace.TraceInformation(
        $"No process information available. Launched Raw Mode target {rawLaunch.Target}");
    }
  }

  private static void LaunchShellMode(LaunchData shellLaunch)
  {
    var startInfo = new ProcessStartInfo {
      FileName = shellLaunch.Target,
      UseShellExecute = true,
      WindowStyle = shellLaunch.WindowStyle,
      ErrorDialog = true,  // only valid if UseShellExecute is true
    };
    if(!string.IsNullOrEmpty(shellLaunch.Verb))
    {
      startInfo.Verb = shellLaunch.Verb;
    }
    //Trace.TraceInformation(
    //  "Available verbs:");
    //foreach(var verb in startInfo.Verbs)
    //{
    //  Trace.TraceInformation(
    //    $"  {verb}");
    //}
    if(shellLaunch.Arguments.Count > 0)
    {
      foreach(var arg in shellLaunch.Arguments)
      {
        startInfo.ArgumentList.Add(arg);
      }
    }
    var process = Process.Start(startInfo);
    if(process != null)
    {
      //Trace.TraceInformation(
      //  $"Started process {process.Id} ({process.MainModule?.FileName ?? String.Empty})");
      // Accessing MainModule may fail if the process is not yet running
      Trace.TraceInformation(
        $"Started Shell Mode process {process.Id}, target = {shellLaunch.Target}");
    }
    else
    {
      Trace.TraceInformation(
        $"No process information available. Launched Shell Mode target {shellLaunch.Target}");
    }
  }

}
