using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LcLauncher.WpfUtilities;

public static class VisualTreeHelperExtensions
{
  /// <summary>  
  /// Finds all visual children of a specific type in the visual tree of a given parent.  
  /// </summary>  
  /// <typeparam name="T">The type of visual children to find.</typeparam>  
  /// <param name="parent">The parent element to search within.</param>  
  /// <returns>An enumerable of all matching visual children.</returns>  
  public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
  {
    if(parent == null)
    {
      yield break;
    }

    for(int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
    {
      var child = VisualTreeHelper.GetChild(parent, i);
      if(child is T tChild)
      {
        yield return tChild;
      }

      foreach(var descendant in FindVisualChildren<T>(child))
      {
        yield return descendant;
      }
    }
  }
}
