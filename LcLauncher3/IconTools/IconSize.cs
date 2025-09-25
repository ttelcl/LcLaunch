/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.IconTools;

[Flags]
public enum IconSize
{
  /// <summary>
  /// Small icon (normally 16x16)
  /// </summary>
  Small = 1,

  /// <summary>
  /// Medium icon (normally 32x32)
  /// </summary>
  Medium = 2,

  /// <summary>
  /// Large icon (normally 48x48)
  /// </summary>
  Large = 4,

  /// <summary>
  /// Extra large icon (normally 256x256)
  /// Currently not expected to be used.
  /// </summary>
  ExtraLarge = 8,

  /// <summary>
  /// All icon sizes (small, medium, large, extra large).
  /// Currently not expected to be used.
  /// </summary>
  All = Small | Medium | Large | ExtraLarge,

  /// <summary>
  /// All normal icon sizes: small, medium, large, but not extra large.
  /// </summary>
  Normal = Small | Medium | Large,

  /// <summary>
  /// No icon sizes
  /// </summary>
  None = 0,
}

public static class IconSizes
{
  /// <summary>
  /// Unpack the individual icon sizes in the argument
  /// </summary>
  /// <param name="iconSize"></param>
  /// <returns></returns>
  public static IEnumerable<IconSize> Unpack(
    this IconSize iconSize)
  {
    if((iconSize & IconSize.Small) != 0)
    {
      yield return IconSize.Small;
    }
    if((iconSize & IconSize.Medium) != 0)
    {
      yield return IconSize.Medium;
    }
    if((iconSize & IconSize.Large) != 0)
    {
      yield return IconSize.Large;
    }
    if((iconSize & IconSize.ExtraLarge) != 0)
    {
      yield return IconSize.ExtraLarge;
    }
  }
}

