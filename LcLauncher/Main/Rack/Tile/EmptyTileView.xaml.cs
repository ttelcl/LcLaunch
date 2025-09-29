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
/// Interaction logic for EmptyTileView.xaml
/// </summary>
public partial class EmptyTileView: UserControl
{
  public EmptyTileView()
  {
    InitializeComponent();
  }

  private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
  {
    if(DataContext is EmptyTileViewModel model)
    {
      model.PrepareFromClipboard();
    }
  }
}
