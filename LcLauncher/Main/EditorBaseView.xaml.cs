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

using LcLauncher.Main.Rack;

namespace LcLauncher.Main;

/// <summary>
/// Interaction logic for EditorBaseView.xaml
/// </summary>
public partial class EditorBaseView: UserControl
{
  public EditorBaseView()
  {
    InitializeComponent();
  }

  private void EditorBaseView_DataContextChanged(
    object sender, DependencyPropertyChangedEventArgs e)
  {
    if(e.NewValue != e.OldValue)
    {
      if(e.NewValue == null && e.OldValue is EditorViewModelBase vmold)
      {
        vmold.UpdateHost(null);
      }
      else if(e.NewValue is EditorViewModelBase vmnew)
      {
        vmnew.UpdateHost(this);
      }
    }
  }
}
