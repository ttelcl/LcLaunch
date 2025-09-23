/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using LcLauncher.DataModel.Utilities;
using LcLauncher.ShellApps;

using Microsoft.WindowsAPICodePack.Shell;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// Description of IconExtraction
/// </summary>
public static class IconExtraction
{
  /// <summary>
  /// Load icons for the given source from the shell.
  /// </summary>
  /// <param name="source">
  /// The icon source: a file name (document or executable),
  /// or an application ID prefixed with "shell:AppsFolder\".
  /// </param>
  /// <param name="sizes">
  /// Bit flags indicating the requested sizes.
  /// Typically <see cref="IconSize.Normal"/> to request all 3 normal
  /// sizes.
  /// </param>
  /// <returns></returns>
  public static BitmapSource?[]? IconsForSource(
    string source, IconSize sizes)
  {
    bool useAppIcon = ShellAppTools.HasShellAppsFolderPrefix(source);
    try
    {
      var icons = new BitmapSource?[4];
      using var iconShell = ShellObject.FromParsingName(source);
      var thumbnail = iconShell.Thumbnail;
      if(useAppIcon)
      {
        thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
        // Beware: 'Default' causes incorrect icon sizes, do not
        // rely on SmallBBitmapSource etc properties in this case,
        // use CurrentSize + BitmapSource instead.
        if(sizes.HasFlag(IconSize.Small))
        {
          thumbnail.CurrentSize = DefaultIconSize.Small;
          icons[0] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.Medium))
        {
          thumbnail.CurrentSize = DefaultIconSize.Medium;
          icons[1] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.Large))
        {
          thumbnail.CurrentSize = DefaultIconSize.Large;
          icons[2] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.ExtraLarge))
        {
          thumbnail.CurrentSize = DefaultIconSize.ExtraLarge;
          icons[3] = thumbnail.BitmapSource;
        }
        return icons;
      }
      else
      {
        thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
        if(sizes.HasFlag(IconSize.Small))
        {
          icons[0] = thumbnail.SmallBitmapSource;
        }
        if(sizes.HasFlag(IconSize.Medium))
        {
          icons[1] = thumbnail.MediumBitmapSource;
        }
        if(sizes.HasFlag(IconSize.Large))
        {
          icons[2] = thumbnail.LargeBitmapSource;
        }
        if(sizes.HasFlag(IconSize.ExtraLarge))
        {
          icons[3] = thumbnail.ExtraLargeBitmapSource;
        }
        return icons;
      }
    }
    catch(ShellException ex)
    {
      Trace.TraceError(
        $"IconForFile: Error probing icon source {source}: {ex}");
      return null;
    }
  }

  /// <summary>
  /// Convert a blob containing icon bytes to an icon
  /// </summary>
  /// <param name="blob">
  /// The byte array containing the binary representation of the icon
  /// </param>
  /// <param name="key">
  /// A description of what the icon was supposed to contain.
  /// Only used in the trace message written if decoding failed
  /// </param>
  /// <returns>
  /// On success: the WPF <see cref="BitmapSource"/> containing the icon.
  /// Null if <paramref name="blob"/> is null or decoding failed.
  /// </returns>
  public static BitmapSource? BlobToIcon(
    byte[]? blob,
    string key = "UNKNOWN")
  {
    if(blob is null)
    {
      return null;
    }
    using var iconStream = new MemoryStream(blob);
    try
    {
      return BitmapFrame.Create(
        iconStream,
        BitmapCreateOptions.None,
        BitmapCacheOption.OnLoad);
    }
    catch(Exception ex)
    {
      Trace.TraceError(
        $"Failed to decode icon {key} from cache: {ex}");
      return null;
    }
  }

  /// <summary>
  /// Convert a WPF icon to a PNG image and return the bytes
  /// of that image.
  /// </summary>
  /// <param name="icon"></param>
  /// <returns></returns>
  public static byte[] IconToBlob(BitmapSource icon)
  {
    var encoder = new PngBitmapEncoder();
    using var stream = new MemoryStream(0x4000);
    encoder.Frames.Add(BitmapFrame.Create(icon));
    encoder.Save(stream);
    var iconBytes = stream.ToArray();
    return iconBytes;
  }

  /// <summary>
  /// Try reading the icon with the id <paramref name="hashId"/> from
  /// <paramref name="blobReader"/> (which was created using
  /// <see cref="IBlobBucket.OpenReader"/>), returning true on success
  /// or false on failure.
  /// </summary>
  /// <param name="blobReader"></param>
  /// <param name="hashId"></param>
  /// <param name="icon"></param>
  /// <returns></returns>
  public static bool TryReadIcon(
    this IBlobBucketReader blobReader,
    HashId hashId,
    [NotNullWhen(true)] out BitmapSource? icon)
  {
    if(blobReader.TryGetBlob(hashId, out var blob))
    {
      icon = BlobToIcon(blob, hashId.ToString());
      return icon != null;
    }
    icon = null;
    return false;
  }

  /// <summary>
  /// Try reading the icon with the id <paramref name="hashId"/> from
  /// <paramref name="blobReader"/> (which was created using
  /// <see cref="IBlobBucket.OpenReader"/>), returning the icon
  /// on success or null on failure.
  /// </summary>
  /// <param name="blobReader"></param>
  /// <param name="hashId"></param>
  /// <returns></returns>
  public static BitmapSource? TryReadIcon(
    this IBlobBucketReader blobReader,
    HashId hashId)
  {
    return blobReader.TryReadIcon(hashId, out var icon) ? icon : null;
  }

  /// <summary>
  /// Try writing the <paramref name="icon"/> to the
  /// <paramref name="blobWriter"/> (as a PNG image).
  /// Returns true if the icon was written, false if the writing was
  /// skipped because it was already stored.
  /// In either case, <paramref name="hashId"/> will contain the icon's
  /// identifier upon return.
  /// </summary>
  /// <param name="blobWriter"></param>
  /// <param name="icon"></param>
  /// <param name="hashId"></param>
  /// <returns></returns>
  public static bool TryWriteIcon(
    this IBlobBucketWriter blobWriter,
    BitmapSource icon,
    out HashId hashId)
  {
    var blob = IconToBlob(icon);
    return blobWriter.TryPutBlob(blob, out hashId);
  }

  /// <summary>
  /// Try writing the <paramref name="icon"/> to the
  /// <paramref name="blobWriter"/> (as a PNG image).
  /// The writing is silently skipped if the icon was already present.
  /// Returns the icon's blob identifier.
  /// </summary>
  public static HashId TryWriteIcon(
    this IBlobBucketWriter blobWriter,
    BitmapSource icon)
  {
    blobWriter.TryWriteIcon(icon, out var hashId);
    return hashId;
  }

}
