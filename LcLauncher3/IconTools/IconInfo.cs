/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// Combines different aspects of one icon of one size.
/// Those aspects are partially optional.
/// </summary>
public class IconInfo
{
  public IconInfo(
    IconSize size,
    string? iconSource = null,
    HashId? iconId = null,
    BitmapSource? icon = null)
  {
    if(
      size != IconSize.Small
      && size != IconSize.Medium
      && size != IconSize.Large
      && size != IconSize.ExtraLarge)
    {
      throw new ArgumentException(
        "Expecting a single icon size");
    }
    Size = size;
    IconSource = iconSource;
    IconId = iconId;
    Icon = icon;
    if(iconSource == null && !iconId.HasValue && icon == null)
    {
      throw new ArgumentException(
        "Expecting one of 'iconSource' and 'iconId' (or, unusually, 'icon') to be not null");
    }
  }

  /// <summary>
  /// The (single) size of the icon
  /// </summary>
  public IconSize Size { get; }

  /// <summary>
  /// The icon source, if known.
  /// If not known, <see cref="IconId"/> should be known.
  /// </summary>
  public string? IconSource { get; set; }

  /// <summary>
  /// The icon ID, if known.
  /// If known, <see cref="IconSource"/> should be known.
  /// </summary>
  public HashId? IconId { get; set; }

  /// <summary>
  /// The actual icon, if resolved.
  /// The aim of <see cref="IconLoader"/> is to set this field
  /// </summary>
  public BitmapSource? Icon { get; set; }
}
