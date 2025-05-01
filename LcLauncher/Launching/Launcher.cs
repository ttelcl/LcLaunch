/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    if(launchData is RawLaunch rawLaunch)
    {
      Launch(rawLaunch);
    }
    else if(launchData is ShellLaunch shellLaunch)
    {
      Launch(shellLaunch);
    }
    else
    {
      MessageBox.Show(
        "Unrecognized launch type",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
    }
  }

  public static void Launch(RawLaunch rawLaunch)
  {
    var startInfo = new ProcessStartInfo {
      FileName = rawLaunch.TargetPath,
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
        $"Started process {process.Id}");
    }
    else
    {
      Trace.TraceInformation("No process information available");
    }
  }

  public static void Launch(ShellLaunch shellLaunch)
  {
    var startInfo = new ProcessStartInfo {
      FileName = shellLaunch.TargetPath,
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
        $"Started process {process.Id}");
    }
    else
    {
      Trace.TraceInformation("No process information available");
    }
  }
}
