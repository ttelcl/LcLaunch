using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace LcLauncher.Main
{
  /// <summary>
  /// Interaction logic for PageColumn.xaml
  /// </summary>
  public partial class PageColumn: UserControl
  {
    public PageColumn()
    {
      InitializeComponent();
    }

    private void PageColumn_DataContextChanged(
      object sender, DependencyPropertyChangedEventArgs e)
    {
      if(e.NewValue != e.OldValue)
      {
        if(e.NewValue == null && e.OldValue is PageColumnViewModel vmold)
        {
          vmold.UpdateHost(null);
        }
        else if(e.NewValue is PageColumnViewModel vmnew)
        {
          vmnew.UpdateHost(this);
        }
      }
    }
  }
}
