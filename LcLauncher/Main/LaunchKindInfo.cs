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

namespace LcLauncher.Main;

/// <summary>
/// UI focused interpretation of <see cref="LaunchKind"/> plus
/// a target.
/// </summary>
public class LaunchKindInfo
{
  /// <summary>
  /// Create a new LaunchKindInfo
  /// </summary>
  public LaunchKindInfo(
    LaunchKind kind,
    string target)
  {
    Kind = kind;
    ArgumentsVisible = Visibility.Collapsed;
    EnvironmentVisible = Visibility.Collapsed;
    WorkDirVisible = Visibility.Collapsed;
    VerbVisible = Visibility.Collapsed;
    switch(Kind)
    {
      case LaunchKind.Raw:
        Icon = "ApplicationCogOutline";
        Text = "executable";
        ArgumentsVisible = Visibility.Visible;
        EnvironmentVisible = Visibility.Visible;
        WorkDirVisible = Visibility.Visible;
        break;
      case LaunchKind.ShellApplication:
        Icon = "ApplicationOutline";
        Text = "application";
        break;
      case LaunchKind.Document:
        VerbVisible = Visibility.Visible;
        if(target.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
        {
          Icon = "ShareCircle";
          Text = "shortcut";
        }
        else
        {
          Icon = "FileDocumentOutline";
          Text = "document";
        }
        break;
      case LaunchKind.UriKind:
        if(target.StartsWith("http://") || target.StartsWith("https://"))
        {
          Icon = "Web";
          Text = "web link";
        }
        else if(target.StartsWith("onenote:"))
        {
          Icon = "MicrosoftOnenote";
          Text = "onenote";
        }
        else
        {
          Icon = "LinkBoxVariantOutline";
          Text = "URI";
        }
        break;
      case LaunchKind.Invalid:
      default:
        Icon = "HelpRhombusOutline";
        Text = "Unknown";
        break;
    }
  }

  public LaunchKind Kind { get; }

  public string Text { get; }

  public string Icon { get; }

  public Visibility WorkDirVisible { get; }

  public Visibility ArgumentsVisible { get; }

  public Visibility EnvironmentVisible { get; }

  public Visibility VerbVisible { get; }

}

