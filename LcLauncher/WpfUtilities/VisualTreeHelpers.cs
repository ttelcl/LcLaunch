using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LcLauncher.WpfUtilities;

// Based on https://stackoverflow.com/a/35324858/271323

public static class VisualTreeHelpers
{

  /// <summary>
  ///   Returns the first ancestor of specified type
  /// </summary>
  public static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
  {
    var dob = GetVisualOrLogicalParent(current);

    while(dob != null)
    {
      if(dob is T t)
      {
        return t;
      }
      dob = GetVisualOrLogicalParent(dob);
    }

    return null;
  }

  private static DependencyObject? GetVisualOrLogicalParent(DependencyObject obj)
  {
    if(obj is Visual || obj is Visual3D)
    {
      return VisualTreeHelper.GetParent(obj);
    }
    return LogicalTreeHelper.GetParent(obj);
  }

}
