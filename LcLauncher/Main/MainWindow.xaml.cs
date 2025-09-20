using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MahApps.Metro.Controls;

namespace LcLauncher.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow: MetroWindow
{
  public MainWindow()
  {
    InitializeComponent();
  }

  private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
  {
    if(DataContext is MainViewModel vm)
    {
      vm.OnWindowClosing();
    }
  }

  private void WindowActivated(object sender, System.EventArgs e)
  {
    if(DataContext is MainViewModel vm)
    {
      vm.OnAppActiveChange(true);
    }
  }

  private void WindowDeactivated(object sender, System.EventArgs e)
  {
    if(DataContext is MainViewModel vm)
    {
      vm.OnAppActiveChange(false);
    }
  }

  private void SplitButton_DropDownOpened(object sender, System.EventArgs e)
  {
    if(DataContext is MainViewModel vm)
    {
      vm.RackList.OnDropDownOpened();
    }
  }
}
