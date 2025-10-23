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
using System.Windows.Threading;

using LcLauncher.Main.Rack;
using LcLauncher.Main.Rack.Tile;

namespace LcLauncher.IconTools;

/// <summary>
/// Job to look up the web icon (favicon) for a web address and
/// implant it in a target UI element. This lookup is asynchronous.
/// </summary>
public class WebIconJob : IIconJob
{
  private readonly Dispatcher _dispatcher;

  /// <summary>
  /// Create a new WebIconJob
  /// </summary>
  /// <param name="targetRack">
  /// The rack containing the target. If this is not the current rack by the
  /// time the lookup completes, the job result is ignored.
  /// </param>
  /// <param name="target">
  /// The target to fill (most likely a <see cref="LaunchTileViewModel"/>)
  /// </param>
  /// <param name="url">
  /// The uri for which to get the icon. Only the base of this url is
  /// used.
  /// </param>
  public WebIconJob(
    RackViewModel rack,
    IIconJobTarget target,
    Uri url)
  {
    _dispatcher = Dispatcher.CurrentDispatcher;
    Url = new Uri(url, "/");
    Rack = rack;
    Target = target;
    // Initial result: the unmodified input
    IconIdResult = Target.IconIds.Clone();
    IconResult = Target.Icons.Clone();
    Phase = IconJobPhase.Pending;
    Trace.TraceInformation(
      $"Initializing web icon job '{Url}'");
  }

  public static bool FireAndForget(
    RackViewModel rack,
    IIconJobTarget target,
    Uri url)
  {
    var job = new WebIconJob(rack, target, url);
    return job.Start();
  }

  public Uri Url { get; }

  public IIconJobTarget Target { get; }

  public RackViewModel Rack { get; }

  public IconJobPhase Phase { get; private set; }

  public IconSize IconSizes => Target.IconSizes;

  public IconIdSet IconIdResult { get; }

  public IconSet IconResult { get; }

  public bool IsDone => Phase >= IconJobPhase.Completed;

  public bool IsFailed => Phase == IconJobPhase.Failed;

  public bool IsPending => Phase == IconJobPhase.Pending;

  public bool WasStarted => Phase >= IconJobPhase.Running;

  /// <summary>
  /// Start the job, in a fire-and-forget style.
  /// Returns true if started, false if it already was started before.
  /// </summary>
  /// <returns></returns>
  public bool Start()
  {
    if(Phase == IconJobPhase.Pending)
    {
      Phase = IconJobPhase.Running;
      _dispatcher.Invoke(StartAsync, DispatcherPriority.ApplicationIdle);
      return true;
    }
    else
    {
      return false;
    }
  }

  private async void StartAsync()
  {
    try
    {
      await RunAsync().ConfigureAwait(false);
    }
    catch(Exception ex)
    {
      Phase = IconJobPhase.Failed;
      Trace.TraceError(
        $"Error in web icon retrieval for '{Url}': {ex}");
    }
  }

  private async Task RunAsync()
  {
    var icons = new IconBytesSet();
    foreach(var size in IconSizes.Unpack())
    {
      var icon = await GetIconBytes(size).ConfigureAwait(false);
      if(icon != null)
      {
        icons[size] = icon;
      }
    }
    await _dispatcher.InvokeAsync(() => Complete(icons));
    Trace.TraceInformation(
      $"Web icon retrieval completed for '{Url}'");
  }

  private async Task<byte[]?> GetIconBytes(IconSize iconSize)
  {
    var size =
      iconSize switch {
        IconSize.Small => 16,
        IconSize.Medium => 32,
        IconSize.Large => 48,
        IconSize.ExtraLarge => 256,
        _ => throw new ArgumentException(
          $"Invalid icon size code {iconSize}")
      };
    
    return await WebIconService.GetWebIconBytes(Url, size);
  }

  private void Complete(IconBytesSet iconsbytes)
  {
    // This is called on the main UI thread
    if(Rack.IsCurrent)
    {
      var icons = iconsbytes.ConvertToIconSet();
      Phase = IconJobPhase.Completed;
      // PushToJob must be on main thread for now, to ensure
      // icon store can be turned writable
      Rack.IconLoader.PushToJob(this, icons);
      Target.UpdateIcons(IconIdResult, IconResult);
    }
    else
    {
      Phase = IconJobPhase.Failed;
      Trace.TraceWarning(
        $"Web Icon Job fizzled: '{Url}'");
    }
  }

}

public enum IconJobPhase
{
  Pending,
  Running,
  Completed,
  Failed,
}
