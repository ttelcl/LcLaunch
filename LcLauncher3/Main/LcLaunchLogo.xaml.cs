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

namespace LcLauncher.Main;

/// <summary>
/// Interaction logic for LcLaunchLogoControl.xaml
/// </summary>
public partial class LcLaunchLogo: UserControl
{
  public LcLaunchLogo()
  {
    InitializeComponent();
  }

  /// <summary>
  /// Size of the logo. Intended to be set in XAML. Default is 48.
  /// </summary>
  public static readonly DependencyProperty LogoSizeProperty =
    DependencyProperty.Register(
      nameof(LogoSize),
      typeof(double),
      typeof(LcLaunchLogo),
      new FrameworkPropertyMetadata(
        48.0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsRender,
        (d, e) => {
          var control = (LcLaunchLogo)d;
          var size = (double)e.NewValue;
          if(size <= 23.0)
          {
            control.InnerSize = size * 3 / 4;
            control.Angle = 270.0;
          }
          else
          {
            control.InnerSize = size *  2 / 3;
            control.Angle = 315.0;
          }
        }));

  /// <summary>
  /// Size of the logo. Intended to be set in XAML. Default is 48.
  /// </summary>
  public double LogoSize {
    get => (double)GetValue(LogoSizeProperty);
    set => SetValue(LogoSizeProperty, value);
  }

  public static readonly DependencyProperty InnerSizeProperty =
    DependencyProperty.Register(
      nameof(InnerSize),
      typeof(double),
      typeof(LcLaunchLogo),
      new FrameworkPropertyMetadata(
        32.0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Size of the inner logo. This is intended to be set indirectly
  /// by setting <see cref="LogoSize"/>. The default is 32.
  /// Setting the logo size sets this to 2/3 of the logo size,
  /// or equal to 3/4 of the logo size if it is below 23.
  /// </summary>
  public double InnerSize {
    get => (double)GetValue(InnerSizeProperty);
    set => SetValue(InnerSizeProperty, value);
  }

  public static readonly DependencyProperty Angleproperty =
    DependencyProperty.Register(
      nameof(Angle),
      typeof(double),
      typeof(LcLaunchLogo),
      new FrameworkPropertyMetadata(
        315.0,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Angle of the logo. This intended to be set indirectly
  /// by setting <see cref="LogoSize"/>. The default is 315;
  /// setting a logo size below 23 will set this to 270
  /// </summary>
  public double Angle {
    get => (double)GetValue(Angleproperty);
    set => SetValue(Angleproperty, value);
  }

  //
}
