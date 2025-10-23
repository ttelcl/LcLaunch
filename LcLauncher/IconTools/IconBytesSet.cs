/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.IconTools;

/// <summary>
/// Similar to <see cref="IconSet"/>, but storing byte arrays instead
/// of BitmapSource (since the latter is thread-bound)
/// </summary>
public class IconBytesSet
{

  /// <summary>
  /// The small icon (16x16), if present.
  /// </summary>
  public byte[]? IconSmall { get; set; }

  /// <summary>
  /// The medium icon (32x32), if present.
  /// </summary>
  public byte[]? IconMedium { get; set; }

  /// <summary>
  /// The large (normal) icon (48x48), if present.
  /// </summary>
  public byte[]? IconLarge { get; set; }

  /// <summary>
  /// The extra large icon (256x256), if present. Currently not
  /// used.
  /// </summary>
  public byte[]? IconExtraLarge { get; set; }

  /// <summary>
  /// The <see cref="IconSize"/> flags combination indicating which
  /// icon sizes are present
  /// </summary>
  public IconSize IconsPresent {
    get {
      return
        (IconSmall != null ? IconSize.Small : IconSize.None)
        | (IconMedium != null ? IconSize.Medium : IconSize.None)
        | (IconLarge != null ? IconSize.Large : IconSize.None)
        | (IconExtraLarge != null ? IconSize.ExtraLarge : IconSize.None);
    }
  }

  /// <summary>
  /// Get or set the icon for the given size
  /// </summary>
  /// <param name="size">
  /// The size to get or set the icon for. Must be one of
  /// <see cref="IconSize.Small"/>, <see cref="IconSize.Medium"/>,
  /// <see cref="IconSize.Large"/> or <see cref="IconSize.ExtraLarge"/>.
  /// </param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public byte[]? this[IconSize size] {
    get {
      return size switch {
        IconSize.Small => IconSmall,
        IconSize.Medium => IconMedium,
        IconSize.Large => IconLarge,
        IconSize.ExtraLarge => IconExtraLarge,
        _ => null,
      };
    }
    set {
      switch(size)
      {
        case IconSize.Small:
          IconSmall = value;
          break;
        case IconSize.Medium:
          IconMedium = value;
          break;
        case IconSize.Large:
          IconLarge = value;
          break;
        case IconSize.ExtraLarge:
          IconExtraLarge = value;
          break;
        default:
          throw new NotSupportedException(
            $"Expecting a single icon size as key");
      }
    }
  }

  /// <summary>
  /// Convert to a normal <see cref="IconSet"/>.
  /// This must be called on the UI thread
  /// </summary>
  /// <returns></returns>
  public IconSet ConvertToIconSet()
  {
    var iconSet = new IconSet();
    foreach(var iconSize in IconsPresent.Unpack())
    {
      var iconBytes = this[iconSize];
      var icon = IconExtraction.BlobToIcon(iconBytes);
      iconSet[iconSize] = icon;
    }
    return iconSet;
  }
}
