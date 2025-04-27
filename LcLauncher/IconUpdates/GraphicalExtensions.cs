/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

  public static BitmapSource ScaleDown(
    this BitmapSource icon, int maxSize)
  {
    if(icon.PixelWidth <= maxSize && icon.PixelHeight <= maxSize)
    {
      return icon;
    }
    Trace.TraceInformation(
      $"Scaling down icon from {icon.PixelWidth}x{icon.PixelHeight} to {maxSize}x{maxSize}");
    if(icon.PixelHeight != icon.PixelWidth)
    {
      throw new NotSupportedException(
        "Only square icons are supported");
    }
    var scale = (double)maxSize / icon.PixelWidth;
    var result = new TransformedBitmap(
      icon,
      new ScaleTransform(scale, scale));
    return result;
  }

  public static RenderTargetBitmap? ElementToBitmap(
    this FrameworkElement frameworkElement)
  {
    // Source: https://stackoverflow.com/a/65189263/271323

    var targetWidth = (int)frameworkElement.ActualWidth;
    var targetHeight = (int)frameworkElement.ActualHeight;

    // Exit if there's no 'area' to render
    if(targetWidth == 0 || targetHeight == 0)
      return null;

    // Prepare the rendering target
    var result = new RenderTargetBitmap(
      targetWidth, targetHeight, 96, 96, PixelFormats.Pbgra32);

    DrawingVisual drawingVisual = new DrawingVisual();
    using(var drawingContext = drawingVisual.RenderOpen())
    {
      // Draw the framework element into the DrawingVisual
      var visualBrush = new VisualBrush(frameworkElement);
      drawingContext.DrawRectangle(visualBrush, null,
        new Rect(new Point(), new Size(targetWidth, targetHeight)));
    }

    // Render the framework element into the target
    result.Render(drawingVisual);

    return result;
  }

  public static void SaveToPng(this BitmapSource source, string fileName)
  {
    var frame = BitmapFrame.Create(source);
    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(frame);
    using(var stream = new FileStream(fileName, FileMode.Create))
    {
      encoder.Save(stream);
    }
  }

  public static byte[] SaveToArray(this BitmapSource source)
  {
    var frame = BitmapFrame.Create(source);
    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(frame);
    using(var stream = new MemoryStream())
    {
      encoder.Save(stream);
      return stream.ToArray();
    }
  }

  /// <summary>
  /// Save a FrameworkElement to a PNG file.
  /// </summary>
  /// <param name="element">
  /// The element to save.
  /// </param>
  /// <param name="fileName">
  /// The name of the file to save to. The file will be
  /// overwritten if it exists.
  /// </param>
  /// <returns>
  /// True on success, false if the element could not be
  /// rendered to a bitmap.
  /// </returns>
  public static bool SaveToPng(this FrameworkElement element, string fileName)
  {
    fileName = Path.GetFullPath(fileName);
    var bits = element.ElementToBitmap();
    bits?.SaveToPng(fileName);
    return bits != null;
  }

}
