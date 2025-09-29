/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;
/// <summary>
/// Carries a set of different size variants of an icon.
/// Each variant is optional: it may be set to null if not used.
/// You can get and set icons through each icon field or use the indexer.
/// </summary>
public class IconSet
{
  // Unusual for this project: properties are publicly read/write

  /// <summary>
  /// The small icon (16x16), if present.
  /// </summary>
  public BitmapSource? IconSmall { get; set; }

  /// <summary>
  /// The medium icon (32x32), if present.
  /// </summary>
  public BitmapSource? IconMedium { get; set; }

  /// <summary>
  /// The large (normal) icon (48x48), if present.
  /// </summary>
  public BitmapSource? IconLarge { get; set; }

  /// <summary>
  /// The extra large icon (256x256), if present. Currently not
  /// used.
  /// </summary>
  public BitmapSource? IconExtraLarge { get; set; }

  /// <summary>
  /// The <see cref="IconSize"/> flags combination indicating which
  /// icon sizes are present
  /// </summary>
  public IconSize IconsPresent {
    get {
      return
        (IconSmall == null ? IconSize.Small : IconSize.None)
        | (IconMedium == null ? IconSize.Medium : IconSize.None)
        | (IconLarge == null ? IconSize.Large : IconSize.None)
        | (IconExtraLarge == null ? IconSize.ExtraLarge : IconSize.None);
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
  public BitmapSource? this[IconSize size] {
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

  public IconSet Clone()
  {
    return new IconSet {
      IconSmall = IconSmall,
      IconMedium = IconMedium,
      IconLarge = IconLarge,
      IconExtraLarge = IconExtraLarge,
    };
  }
}
