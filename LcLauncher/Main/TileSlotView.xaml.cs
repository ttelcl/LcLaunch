﻿using System;
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

namespace LcLauncher.Main;

/// <summary>
/// Interaction logic for TileSlotView.xaml
/// </summary>
public partial class TileSlotView: UserControl
{
  public TileSlotView()
  {
    InitializeComponent();
  }

  private void TileSlot_MouseEnter(object sender, MouseEventArgs e)
  {
    if(DataContext is TileSlotViewModel vm)
    {
      vm.Hovering = true;
    }
  }

  private void TileSlot_MouseLeave(object sender, MouseEventArgs e)
  {
    if(DataContext is TileSlotViewModel vm)
    {
      vm.Hovering = false;
    }
  }
}
