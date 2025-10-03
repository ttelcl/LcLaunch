/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Editors;

public class EnvironmentEditViewModel: ViewModelBase, ISubEditor
{
  public EnvironmentEditViewModel(
    LaunchEditViewModel owner,
    Dictionary<string, string?> environment)
  {
    Owner = owner;
    Environment = new Dictionary<string, string?>(environment);
    _environmentAsLines = String.Empty;
    EnvironmentToLines();
  }

  public LaunchEditViewModel Owner { get; }

  public Dictionary<string, string?> Environment { get; }

  public bool IsEmpty {
    get =>
      IsEditing
      ? String.IsNullOrEmpty(EnvironmentAsLines.Trim())
      : Environment.Count == 0;
  }

  public string EnvironmentAsLines {
    get => _environmentAsLines;
    set {
      if(SetValueProperty(ref _environmentAsLines, value))
      {
      }
    }
  }
  private string _environmentAsLines;

  private void EnvironmentToLines()
  {
    var pairs = new List<string>();
    foreach(var kvp in Environment)
    {
      var pair = $"{kvp.Key}={kvp.Value??String.Empty}";
      pairs.Add(pair);
    }
    EnvironmentAsLines = String.Join(System.Environment.NewLine, pairs);
  }

  private void LinesToEnvironment()
  {
    var lines = EnvironmentAsLines.Split(System.Environment.NewLine);
    Environment.Clear();
    foreach(var line in lines)
    {
      var parts = line.Split("=", 2);
      if(parts.Length == 2) // else ignore
      {
        var varname = parts[0].Trim();
        string? value = parts[1].Trim();
        if(String.IsNullOrEmpty(value))
        {
          value = null;
        }
        Environment.Add(varname, value);
      }
    }
    EnvironmentToLines(); // keep in sync, since values may have been dropped
  }

  public bool IsEditing {
    get => _isEditing;
    set {
      if(SetValueProperty(ref _isEditing, value))
      {
        if(value)
        {
          Owner.CurrentSubEditor = this;
          EnvironmentToLines();
        }
        else
        {
          if(Owner.CurrentSubEditor == this)
          {
            Owner.CurrentSubEditor = null;
          }
          LinesToEnvironment();
        }
      }
    }
  }
  private bool _isEditing = false;

  public string SubEditorName => "Environment Variables";
}
