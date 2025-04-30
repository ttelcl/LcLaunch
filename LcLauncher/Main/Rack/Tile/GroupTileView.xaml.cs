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
/// Interaction logic for GroupTileView.xaml
/// </summary>
public partial class GroupTileView: UserControl
{
  public GroupTileView()
  {
    InitializeComponent();
  }

  private void GroupTileView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    if(e.ChangedButton == MouseButton.Left && DataContext is GroupTileViewModel tile)
    {
      tile.MouseButtonChange(true);
    }
  }

  private void GroupTileView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    if(e.ChangedButton == MouseButton.Left && DataContext is GroupTileViewModel tile)
    {
      tile.MouseButtonChange(false);
    }
  }
}
