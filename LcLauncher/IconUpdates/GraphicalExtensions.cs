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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LcLauncher.IconUpdates;

/// <summary>
/// Extension methods and helpers for graphical operations.
/// </summary>
public static class GraphicalExtensions
{

  /// <summary>
  /// Try to repair the transparency of a bitmap icon,
  /// for cases where the first pixel indicates the color
  /// that should have been transparent in the first place.
  /// ((This doesn't work too well))
  /// </summary>
  public static BitmapSource TryFixTransparancy(
    this BitmapSource icon)
  {
    if(icon.Format != PixelFormats.Bgra32)
    {
      // We only support Bgra32.
      return icon;
    }
    var buffer = new uint[icon.PixelHeight * icon.PixelWidth];
    icon.CopyPixels(buffer, icon.PixelWidth * 4, 0);
    var firstPixel = buffer[0];
    var lastPixel = buffer[buffer.Length - 1];
    Trace.TraceInformation(
      $"First pixel: {firstPixel:X8}");
    Trace.TraceInformation(
      $"Last pixel: {lastPixel:X8}");

    if((firstPixel & 0xFF000000U) != 0xFF000000U ||
      (lastPixel & 0xFF000000U) != 0xFF000000U)
    {
      // There already is transparency in first or last pixel, no
      // need to fix anything.
      return icon;
    }
    for(int i = 0; i < buffer.Length; i++)
    {
      var pixel = buffer[i];
      if(pixel == firstPixel)
      {
        // This pixel should be transparent
        buffer[i] = 0x00000000U;
      }
    }
    return BitmapSource.Create(
      icon.PixelWidth, icon.PixelHeight,
      icon.DpiX, icon.DpiY,
      PixelFormats.Bgra32, null,
      buffer, icon.PixelWidth * 4);
  }

}
