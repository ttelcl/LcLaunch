/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Editors;

public class ArgumentEditViewModel: ViewModelBase
{
  public ArgumentEditViewModel(
    LaunchEditViewModel owner,
    IEnumerable<string> arguments)
  {
    Owner = owner;
    Arguments = [.. arguments];
    _argumentsAsLines = String.Empty; // to keep compiler happy
    ArgumentsToLines();
  }

  public LaunchEditViewModel Owner { get; }

  public ObservableCollection<string> Arguments { get; }

  public string ArgumentsAsLines {
    get => _argumentsAsLines;
    set {
      if(SetValueProperty(ref _argumentsAsLines, value))
      {
      }
    }
  }
  private string _argumentsAsLines;

  public void LinesToArguments()
  {
    Arguments.Clear();
    var lines = ArgumentsAsLines;
    if(!String.IsNullOrEmpty(lines))
    {
      foreach(var line in lines.Split(Environment.NewLine))
      {
        Arguments.Add(line);
      }
    }
  }

  public void ArgumentsToLines()
  {
    ArgumentsAsLines = string.Join(Environment.NewLine, Arguments);
  }

  public bool IsEditing {
    get => _isEditing;
    set {
      if(SetValueProperty(ref _isEditing, value))
      {
        if(value)
        {
          ArgumentsToLines();
        }
        else
        {
          LinesToArguments();
        }
      }
    }
  }
  private bool _isEditing = false;
}
