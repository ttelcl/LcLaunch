/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using LcLauncher.Models;

namespace LcLauncher.Persistence;

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
}

/// <summary>
/// Abstraction for an icon cache. Supports
/// creating icons, caching them, and loading them.
/// Each icon cache is associated with a tile list.
/// </summary>
public interface ILauncherIconCache
{
  /// <summary>
  /// The ID of this icon cache. This is normally equal
  /// to the ID of the tile list it is associated with.
  /// </summary>
  Guid CacheId { get; }

  /// <summary>
  /// Try to find the icon by ID in this cache.
  /// </summary>
  /// <param name="hashPrefix">
  /// The hash (or sufficiently long prefix) of the icon to load.
  /// For ease of use this may be null or empty, which will always
  /// result in a null return value.
  /// </param>
  /// <returns>
  /// The BitmapSource of the icon, if found.
  /// </returns>
  BitmapSource? LoadCachedIcon(string? hashPrefix);

  /// <summary>
  /// Cache icons for the given source in one or more sizes.
  /// Expect this call to be SLOW.
  /// Icons that are already cached are not put in the cache
  /// again (they are temporarily created to be able to calculate
  /// their hash though). 
  /// </summary>
  /// <param name="iconSource">
  /// The source specification for the icon. Normally this is
  /// the name of the document, or an executable file,
  /// or an application identifier.
  /// </param>
  /// <param name="sizes">
  /// The sizes to cache. The sizes are specified as a bitwise
  /// combination of <see cref="IconSize"/> values.
  /// </param>
  /// <returns>
  /// If successful, this contains the hashes of the icons in the
  /// requested sizes. If no icons could be created, this is null.
  /// </returns>
  IconHashes? CacheIcons(
    string iconSource,
    IconSize sizes = IconSize.Normal);

}

public class IconHashes
{
  public string? Small { get; init; }
  public string? Medium { get; init; }
  public string? Large { get; init; }
  public string? ExtraLarge { get; init; }
}

/// <summary>
/// How hard to look for an icon
/// </summary>
public enum IconLoadLevel { 
  /// <summary>
  /// Only get the icon if it already in the cache
  /// </summary>
  FromCache,
  /// <summary>
  /// Load the icon if it is in the cache, put it in the cache
  /// otherwise.
  /// </summary>
  LoadIfMissing,
  /// <summary>
  /// Put the icon in the cache, even if it was already there.
  /// </summary>
  LoadAlways
}
