using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

/// <summary>
/// Interaction logic for RackView.xaml
/// </summary>
public partial class RackView: UserControl /*, IScrollInfo */
{
  public RackView()
  {
    InitializeComponent();
  }

  public event RoutedEventHandler MouseHorizontalWheel {
    add { AddHandler(MouseHorizontalWheelEnabler.MouseHorizontalWheelEvent, value); }
    remove { RemoveHandler(MouseHorizontalWheelEnabler.MouseHorizontalWheelEvent, value); }
  }

  public event RoutedEventHandler PreviewMouseHorizontalWheel {
    add { AddHandler(MouseHorizontalWheelEnabler.PreviewMouseHorizontalWheelEvent, value); }
    remove { RemoveHandler(MouseHorizontalWheelEnabler.PreviewMouseHorizontalWheelEvent, value); }
  }
}
