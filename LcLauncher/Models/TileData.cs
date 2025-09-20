/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LcLauncher.Models;

/// <summary>
/// Polymorphic data for tile content, containing one of
/// a few possible kinds of tile content. Only one of the
/// fields will be non-null (except for the quad field, which
/// can be an empty list as equivalent).
/// A list of these is what gets serialized in tile list files.
/// </summary>
public class TileData
{
  public TileData(
    LaunchData? launch = null,
    TileGroup? group = null,
    IEnumerable<LaunchData?>? quad = null,
    ShellLaunch? shellLaunch = null,
    RawLaunch? rawLaunch = null)
  {
    if(launch != null)
    {
      Launch = launch;
      // ignore the other launch type arguments!
      ShellLaunch = launch.ToShellLaunch();
      RawLaunch = launch.ToRawLaunch();
    }
    else if(shellLaunch != null)
    {
      Launch = shellLaunch.ToLaunch();
      ShellLaunch = shellLaunch;
      RawLaunch = null;
    }
    else if(rawLaunch != null)
    {
      Launch = rawLaunch.ToLaunch();
      RawLaunch = rawLaunch;
      ShellLaunch = null;
    }
    else
    {
      Launch = null;
      ShellLaunch = null;
      RawLaunch = null;
    }
    Group = group;
    Quad = quad == null ? null : quad.ToList();
  }

  public static TileData EmptyTile()
  {
    return new TileData();
  }

  public static TileData ShellTile(ShellLaunch shellLaunch)
  {
    return new TileData(
      launch: shellLaunch.ToLaunch());
  }

  public static TileData RawTile(RawLaunch rawLaunch)
  {
    return new TileData(
      launch: rawLaunch.ToLaunch());
  }

  public static TileData LaunchTile(LaunchData launch)
  {
    return new TileData(
      launch: launch);
  }

  public static TileData GroupTile(TileGroup group)
  {
    return new TileData(group: group);
  }

  public static TileData GroupTile(
    Guid tilelist,
    string? title = null,
    string? tooltip = null)
  {
    return new TileData(group: new TileGroup(tilelist, title, tooltip));
  }

  public static TileData QuadTile(IEnumerable<LaunchData?> quad)
  {
    return new TileData(quad: quad);
  }

  /// <summary>
  /// Shell based launch tile.
  /// </summary>
  [JsonProperty("shellLaunch")]
  public ShellLaunch? ShellLaunch { get; }

  public bool ShouldSerializeShellLaunch()
  {
    return ShellLaunch != null && Launch == null;
  }

  [JsonProperty("rawLaunch")]
  public RawLaunch? RawLaunch { get; }

  public bool ShouldSerializeRawLaunch()
  {
    return RawLaunch != null && Launch == null;
  }

  [JsonProperty("launch", NullValueHandling = NullValueHandling.Ignore)]
  public LaunchData? Launch { get; }

  public bool HasLaunch()
  {
    return ShellLaunch != null || RawLaunch != null || Launch != null;
  }

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public TileGroup? Group { get; }

  [JsonProperty("quad", NullValueHandling = NullValueHandling.Ignore)]
  public List<LaunchData?>? Quad { get; }

  public bool IsEmpty()
  {
    return
      ShellLaunch == null &&
      RawLaunch == null &&
      Launch == null &&
      Group == null &&
      Quad == null;
  }

}
