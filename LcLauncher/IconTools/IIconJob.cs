/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.IconTools;

/// <summary>
/// Common interface for <see cref="IconJob"/> and
/// <see cref="WebIconJob"/>
/// </summary>
public interface IIconJob
{
  IIconJobTarget Target { get; }

  IconIdSet IconIdResult { get; }

  IconSet IconResult { get; }

  IconSize IconSizes { get; }
}

