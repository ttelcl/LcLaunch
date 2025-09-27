using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Interaction logic for TileHostView.xaml
/// </summary>
public partial class TileHostView: UserControl
{
  public TileHostView()
  {
    InitializeComponent();
  }

  private void TileHost_MouseEnter(object sender, MouseEventArgs e)
  {
    if(DataContext is TileHostViewModel vm)
    {
      vm.Hovering = true;
    }
  }

  private void TileHost_MouseLeave(object sender, MouseEventArgs e)
  {
    if(DataContext is TileHostViewModel vm)
    {
      vm.Hovering = false;
    }
  }

  private void TileHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    if(e.ChangedButton == MouseButton.Left && DataContext is TileHostViewModel tile)
    {
      tile.HostMouseButtonChanged(true, Keyboard.Modifiers);
    }
  }

  private void TileHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    if(e.ChangedButton == MouseButton.Left && DataContext is TileHostViewModel tile)
    {
      tile.HostMouseButtonChanged(false, Keyboard.Modifiers);
    }
  }

}
