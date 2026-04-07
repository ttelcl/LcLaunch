using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LcLauncher.WpfUtilities;

// Based on https://stackoverflow.com/a/35324858/271323

public class MouseHorizontalWheelEventArgs: MouseEventArgs
{
  public MouseHorizontalWheelEventArgs(MouseDevice mouse, int timestamp, int horizontalDelta)
    : base(mouse, timestamp)
  {
    HorizontalDelta = horizontalDelta;
  }

  public int HorizontalDelta { get; }
}
