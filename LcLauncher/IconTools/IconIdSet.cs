/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// Stores Icon IDs for each size
/// </summary>
public class IconIdSet
{

  /// <summary>
  /// The ID for the small icon
  /// </summary>
  public HashId? Icon16 { get; set; }

  /// <summary>
  /// The ID for the medium icon
  /// </summary>
  public HashId? Icon32 { get; set; }

  /// <summary>
  /// The ID for the large (normal) icon
  /// </summary>
  public HashId? Icon48 { get; set; }

  /// <summary>
  /// The ID for the extra large icon (not actively used)
  /// </summary>
  public HashId? Icon256 { get; set; }

  /// <summary>
  /// Get or set the ID for the given size
  /// </summary>
  /// <param name="size">
  /// The size to get or set the ID for. Must be one of
  /// <see cref="IconSize.Small"/>, <see cref="IconSize.Medium"/>,
  /// <see cref="IconSize.Large"/> or <see cref="IconSize.ExtraLarge"/>.
  /// </param>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public HashId? this[IconSize size] {
    get {
      return size switch {
        IconSize.Small => Icon16,
        IconSize.Medium => Icon32,
        IconSize.Large => Icon48,
        IconSize.ExtraLarge => Icon256,
        _ => null,
      };
    }
    set {
      switch(size)
      {
        case IconSize.Small:
          Icon16 = value;
          break;
        case IconSize.Medium:
          Icon32 = value;
          break;
        case IconSize.Large:
          Icon48 = value;
          break;
        case IconSize.ExtraLarge:
          Icon256 = value;
          break;
        default:
          throw new NotSupportedException(
            $"Expecting a single icon size as key");
      }
    }
  }

  public IconIdSet Clone()
  {
    return new IconIdSet {
      Icon16 = Icon16,
      Icon32 = Icon32,
      Icon48 = Icon48,
      Icon256 = Icon256,
    };
  }
}
