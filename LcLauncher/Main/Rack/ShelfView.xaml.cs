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

namespace LcLauncher.Main.Rack
{
  /// <summary>
  /// Interaction logic for ShelfView.xaml
  /// </summary>
  public partial class ShelfView: UserControl
  {
    public ShelfView()
    {
      InitializeComponent();
    }

    private void ShelfView_DataContextChanged(
      object sender, DependencyPropertyChangedEventArgs e)
    {
      if(e.NewValue != e.OldValue)
      {
        if(e.NewValue == null && e.OldValue is ShelfViewModel vmold)
        {
          vmold.UpdateHost(null);
        }
        else if(e.NewValue is ShelfViewModel vmnew)
        {
          vmnew.UpdateHost(this);
        }
      }
    }

  }
}
