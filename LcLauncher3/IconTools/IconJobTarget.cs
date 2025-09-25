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
/// Describes what to do with the result of running an
/// <see cref="IconJob"/>.
/// </summary>
public interface IIconJobTarget
{
  /// <summary>
  /// A random id identifying this target. Used to de-duplicate
  /// requests for the same target.
  /// </summary>
  Guid IconTargetId { get; }

  /// <summary>
  /// The source of the icon for this target (an executable file,
  /// a document, an app descriptor, etc.).
  /// </summary>
  string IconSource { get; }

  /// <summary>
  /// The set of icon sizes this target needs
  /// </summary>
  IconSize IconSizes { get; }

  /// <summary>
  /// Accessors for getting the icon IDs.
  /// The setters are not used; instead the load job calls
  /// <see cref="UpdateIcons(IconIdSet, IconSet)"/>
  /// </summary>
  IconIdSet IconIds { get; }

  /// <summary>
  /// Accessors for getting the icons
  /// The setters are not used; instead the load job calls
  /// <see cref="UpdateIcons(IconIdSet, IconSet)"/>
  /// </summary>
  IconSet Icons { get; }

  /// <summary>
  /// Callback from the icon load job to announce results, allowing
  /// the target to set their <see cref="IconIds"/> and <see cref="Icons"/>.
  /// </summary>
  /// <param name="iconIds"></param>
  /// <param name="icons"></param>
  void UpdateIcons(IconIdSet iconIds, IconSet icons);
}
