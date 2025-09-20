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

namespace LcLauncher.ModelConversion;

/// <summary>
/// Keeps track of Maps V2 GUID to V3 TickId mappings
/// </summary>
public class IdConversionMap
{
  private readonly Dictionary<Guid, TickId> _mappings;

  /// <summary>
  /// Create a new IdConversionMap
  /// </summary>
  public IdConversionMap()
  {
    _mappings = new Dictionary<Guid, TickId>();
  }

  /// <summary>
  /// Get the <see cref="TickId"/> for the given <see cref="Guid"/>.
  /// If no mapping is defined, a new <see cref="TickId"/> is generated.
  /// Can also be used to manually define the mapping by setting a value,
  /// but only if that <see cref="Guid"/> has no <see cref="TickId"/>
  /// associated yet.
  /// </summary>
  /// <param name="guid"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public TickId this[Guid guid] {
    get {
      if(!_mappings.TryGetValue(guid, out var tickId))
      {
        tickId = TickId.New();
        _mappings.Add(guid, tickId);
      }
      return tickId;
    }
    set {
      if(_mappings.TryGetValue(guid, out var existing))
      {
        if(existing != value)
        {
          throw new InvalidOperationException(
            $"Attempt to redefine Id mapping for {guid} from {existing} to {value}");
        }
        return;
      }
      _mappings.Add(guid, value);
    }
  }

}
