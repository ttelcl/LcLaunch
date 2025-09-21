/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

using Newtonsoft.Json;

using LcLauncher.Models;
using LcLauncher.IconUpdates;
using LcLauncher.WpfUtilities;
using System.Text.Unicode;

namespace LcLauncher.Main;

// Temporary helper class

public class TestPaneViewModel: ViewModelBase
{
  public TestPaneViewModel(
    MainViewModel host)
  {
    Host = host;
    OpenIconFileCommand = new DelegateCommand(p => OpenIconFile());
    TestApplicationShellFolderCommand =
      new DelegateCommand(p => TestApplicationShellFolder());
    TestEditorCommand =
      new DelegateCommand(
        p => EditorViewModelBase.ShowTest(host));
    SaveLogosCommand = new DelegateCommand(
      p => SaveLogoPngs());
    TestClipboardCommand = new DelegateCommand(
      p => ClipboardTest());
  }

  public MainViewModel Host { get; }

  public string? IconFile {
    get => _iconFile;
    set {
      if(SetValueProperty(ref _iconFile, value))
      {
        ProbeIconFile();
      }
    }
  }
  private string? _iconFile;

  private void ProbeIconFile()
  {
    try
    {
      using(var iconShell = ShellObject.FromParsingName(IconFile))
      {
        Trace.TraceInformation(
          $"Parsing Name: {iconShell.ParsingName}");
        Trace.TraceInformation(
          $"Display Name: {iconShell.Name}");
        var thumbnail = iconShell.Thumbnail;
        thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
        IconSmall = thumbnail.SmallBitmapSource;
        Trace.TraceInformation(
          $"Small Icon: {IconSmall.Width}"); // 16
        IconMedium = thumbnail.MediumBitmapSource;
        Trace.TraceInformation(
          $"Medium Icon: {IconMedium.Width}"); // 32
        IconLarge = thumbnail.LargeBitmapSource;
        Trace.TraceInformation(
          $"Large Icon: {IconLarge.Width}"); // 48
        IconExtraLarge = thumbnail.ExtraLargeBitmapSource;
        Trace.TraceInformation(
          $"Extra Large Icon: {IconExtraLarge.Width}"); // 256
        thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
        thumbnail.AllowBiggerSize = false;
        IconSmallFixed = thumbnail.SmallBitmapSource;
        IconMediumFixed = thumbnail.MediumBitmapSource;
        IconLargeFixed = thumbnail.LargeBitmapSource;
        //IconExtraLarge = thumbnail.ExtraLargeBitmapSource;
      }
    }
    catch(Exception ex)
    {
      Trace.TraceError(
        $"ProbeIconFile: Error probing icon file: {ex}");
      ClearIcons();
    }
  }

  private void ClearIcons()
  {
    IconSmall = null;
    IconMedium = null;
    IconLarge = null;
    IconExtraLarge = null;
  }

  /// <summary>
  /// Smallest size, 16x16 pixels usually.
  /// Too small for most purposes.
  /// </summary>
  public BitmapSource? IconSmall {
    get => _iconSmall;
    set {
      if(SetNullableInstanceProperty(ref _iconSmall, value))
      {
        //IconSmallFixed = value?.TryFixTransparancy();
      }
    }
  }
  private BitmapSource? _iconSmall;

  /// <summary>
  /// Smaller size but still readable, 32x32 pixels usually.
  /// May be of use for group thumbnails.
  /// </summary>
  public BitmapSource? IconMedium {
    get => _iconMedium;
    set {
      if(SetNullableInstanceProperty(ref _iconMedium, value))
      {
        //IconMediumFixed = value?.TryFixTransparancy();
      }
    }
  }
  private BitmapSource? _iconMedium;

  /// <summary>
  /// Most relevant size, 48x48 pixels usually.
  /// </summary>
  public BitmapSource? IconLarge {
    get => _iconLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconLarge, value))
      {
        //IconLargeFixed = value?.TryFixTransparancy();
      }
    }
  }
  private BitmapSource? _iconLarge;

  /// <summary>
  /// Huge icon, 256x256 pixels usually. May be of use as source
  /// for scaling down to other sizes.
  /// </summary>
  public BitmapSource? IconExtraLarge {
    get => _iconExtraLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconExtraLarge, value))
      {
      }
    }
  }
  private BitmapSource? _iconExtraLarge;

  public BitmapSource? IconSmallFixed {
    get => _iconSmallFixed;
    set {
      if(SetNullableInstanceProperty(ref _iconSmallFixed, value))
      {
      }
    }
  }
  private BitmapSource? _iconSmallFixed;

  public BitmapSource? IconMediumFixed {
    get => _iconMediumFixed;
    set {
      if(SetNullableInstanceProperty(ref _iconMediumFixed, value))
      {
      }
    }
  }
  private BitmapSource? _iconMediumFixed;

  public BitmapSource? IconLargeFixed {
    get => _iconLargeFixed;
    set {
      if(SetNullableInstanceProperty(ref _iconLargeFixed, value))
      {
      }
    }
  }
  private BitmapSource? _iconLargeFixed;

  public ICommand OpenIconFileCommand { get; }

  public ICommand TestEditorCommand { get; }

  private void OpenIconFile()
  {
    var ofd = new OpenFileDialog() {
      Filter = "Any file (*.*)|*.*",
      Title = "Open Icon File",
      CheckFileExists = true,
      CheckPathExists = true,
      Multiselect = false,
      DereferenceLinks = false,
    };
    var result = ofd.ShowDialog();
    if(result == true)
    {
      IconFile = ofd.FileName;
    }
  }

  public ICommand TestApplicationShellFolderCommand { get; }

  public ICommand SaveLogosCommand { get; }

  public ICommand TestClipboardCommand { get; }

  private readonly Guid AppsFolderId =
    new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

  private void TestApplicationShellFolder()
  {
    Trace.TraceInformation(
      $"Testing Shell Application Folder features");
    using var appsFolder =
      (ShellObject)KnownFolderHelper.FromKnownFolderId(AppsFolderId);
    Trace.TraceInformation(
      $"Parsing Name: {appsFolder.ParsingName}");
    Trace.TraceInformation(
      $"Name: {appsFolder.Name}");
    //Trace.TraceInformation(
    //  $"IsFileSystemObject: {appsFolder.IsFileSystemObject}");
    //Trace.TraceInformation(
    //  $"IsLink: {appsFolder.IsLink}");
    var appsVf = (IKnownFolder)appsFolder;
    Trace.TraceInformation(
      $"VF Category: {appsVf.Category}");
    Trace.TraceInformation(
      $"VF Canonical Name: {appsVf.CanonicalName}");
    var infoDump = new List<ShellAppInfo>();
    ShellObject? selectedSo = null;
    foreach(var app in appsVf)
    {
      var appInfo = ShellAppInfo.FromShellObject(app);
      infoDump.Add(appInfo);
      // "Microsoft.ScreenSketch_8wekyb3d8bbwe!App"
      // "CanonicalGroupLimited.UbuntuonWindows_79rhkp1fndgsc!ubuntu"
      // "Microsoft.AutoGenerated.{736A64EC-D4B0-70BA-2797-1B45B159F1E4}"
      if(app.ParsingName == "Microsoft.ScreenSketch_8wekyb3d8bbwe!App")
      {
        selectedSo = app;
      }
    }
    Trace.TraceInformation(
      $"Found {infoDump.Count} applications.");
    //var store = Host.FileStore;
    //var path = store.SaveData(
    //  "application-dump",
    //  ".json",
    //  infoDump);
    //Trace.TraceInformation(
    //  $"Saved application dump to {path}");
    if(selectedSo != null)
    {
      Trace.TraceInformation(
        $"Found {selectedSo.Name} ({selectedSo.ParsingName}) application");
      var thumbnail = selectedSo.Thumbnail;
      thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
      //IconSmallFixed = thumbnail.SmallBitmapSource.ScaleDown(16);
      //IconMediumFixed = thumbnail.MediumBitmapSource.ScaleDown(32);
      //IconLargeFixed = thumbnail.LargeBitmapSource.ScaleDown(48);

      // 'thumbnails' are larger than 'icons', but we want
      // icon sized thumbnails. The following hacks around that.
      thumbnail.CurrentSize = DefaultIconSize.Small;
      IconSmallFixed = thumbnail.BitmapSource;
      Trace.TraceInformation(
        $"Small Icon fancy: {IconSmallFixed.Width}"); // 16
      thumbnail.CurrentSize = DefaultIconSize.Medium;
      IconMediumFixed = thumbnail.BitmapSource;
      Trace.TraceInformation(
        $"Medium Icon fancy: {IconMediumFixed.Width}"); // 32
      thumbnail.CurrentSize = DefaultIconSize.Large;
      IconLargeFixed = thumbnail.BitmapSource;
      Trace.TraceInformation(
        $"Large Icon fancy: {IconLargeFixed.Width}"); // 48

      thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
      IconSmall = thumbnail.SmallBitmapSource;
      IconMedium = thumbnail.MediumBitmapSource;
      IconLarge = thumbnail.LargeBitmapSource;
    }
  }

  private void SaveLogoPngs()
  {
    var root = App.Current.MainWindow;
    // gather the logo elements
    var logos = root.FindVisualChildren<LcLaunchLogo>().ToList();
    foreach(var logo in logos)
    {
      if(String.IsNullOrEmpty(logo.Name))
      {
        // ignore unnamed logos
        Trace.TraceWarning(
          $"Logo skipped: no name");
        continue;
      }
      var size = logo.LogoSize;
      if(size == logo.Width && size == logo.Height)
      {
        var fileName = Path.GetFullPath(logo.Name.ToLower() + ".png");
        var bits = logo.ElementToBitmap();
        if(bits != null)
        {
          var bytes = bits.SaveToArray();
          bits.SaveToPng(fileName);
          Trace.TraceInformation(
            $"Saving logo {logo.Name} ({size}) ({bits.PixelWidth} x {bits.PixelHeight}): {fileName}");
          Trace.TraceInformation(
            $"Logo {logo.Name} (array of {bytes.Length} bytes)");
        }
        else
        {
          Trace.TraceWarning(
            $"Logo {logo.Name} ({size}) rejected: could not render");
        }
      }
      else
      {
        Trace.TraceWarning(
          $"Logo {logo.Name} ({size}) rejected: does not match size");
      }
    }
  }

  private void ClipboardTest()
  {
    var dataObject = Clipboard.GetDataObject();
    var formats = dataObject.GetFormats();
    var formatsList = String.Join(", ", formats);
    Trace.TraceInformation($"There are {formats.Length} formats on the clipboard: {formatsList}");
    if(dataObject.GetDataPresent("OneNote Link"))
    {
      Trace.TraceInformation("Found a 'OneNote Link' on the clipboard!");
      var onenoteLink = dataObject.GetData("OneNote Link");
      Trace.TraceInformation($"the type of the 'OneNote Link' is '{onenoteLink.GetType().FullName}'");
      if(onenoteLink is Stream stream) // MemoryStream, really
      {
        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes.AsSpan());
        if(Utf8.IsValid(bytes))
        {
          var streamTxt = UTF8Encoding.UTF8.GetString(bytes);
          Trace.TraceInformation($"Stream content: {streamTxt}");
          // the content appears to be HTML, wrapped in a standard clipboard wrapper.
          var startKey = "<!--StartFragment-->";
          var endKey = "<!--EndFragment-->";
          var fragmentStart = streamTxt.IndexOf(startKey);
          var fragmentEnd = streamTxt.LastIndexOf(endKey);
          if(fragmentStart >= 0 && fragmentEnd > fragmentStart)
          {
            var fragment = streamTxt.Substring(
              fragmentStart +  startKey.Length,
              fragmentEnd - (fragmentStart + startKey.Length));
            Trace.TraceInformation("----- Found the following fragment: -----");
            Trace.TraceInformation(fragment);
            Trace.TraceInformation("----- End of fragment: -----");
          }
        }
        else
        {
          Trace.TraceError("The content is not valid UTF8");
        }
      }
      else
      {
        Trace.TraceWarning("Format was not a stream");
      }
    }
    if(Clipboard.ContainsText())
    {
      var text = Clipboard.GetText();
      var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
      Trace.TraceInformation($"Clipboard contains {lines.Count} lines of text");
      var onenoteLine =
        lines.Take(2).FirstOrDefault(s => s.StartsWith("onenote:"));
      if(onenoteLine != null)
      {
        Trace.TraceInformation($"Found a onenote link: {onenoteLine}");
        var fragmentStartIndex = onenoteLine.IndexOf('#');
        if(fragmentStartIndex != -1)
        {
          fragmentStartIndex++;
          var fragmentEndIndex = onenoteLine.IndexOf('&', fragmentStartIndex);
          if(fragmentEndIndex != -1)
          {
            var fragment = onenoteLine.Substring(
              fragmentStartIndex,
              fragmentEndIndex - fragmentStartIndex);
            fragment = Uri.UnescapeDataString(fragment);
            Trace.TraceInformation($"The title is: {fragment}");
          }
        }
      }
      else if(lines.Count == 1 && (lines[0].StartsWith("http://") || lines[0].StartsWith("https://")))
      {
        Trace.TraceInformation($"Found a web link on line 1: {lines[0]}");
      }
      else if(lines.Count == 1)
      {
        Trace.TraceInformation($"Didn't recognize line '{lines[0]}'");
      }
      else
      {
        Trace.TraceInformation($"Didn't recognize text content '{text}'");
      }
    }
    else
    {
      Trace.TraceInformation($"Clipboard does not contain text");
    }
    if(Clipboard.ContainsFileDropList())
    {
      var dropList = Clipboard.GetFileDropList();
      if(dropList != null)
      {
        var count = dropList.Count;
        Trace.TraceInformation($"Clipboard contains a file drop list with {count} files");
        if(count == 1)
        {
          var file = dropList[0];
          Trace.TraceInformation($"the one file is {file}");
        }
      }
    }
    if(dataObject.GetDataPresent("HTML Format"))
    {
      Trace.TraceInformation("HTML content is present");
      var htmlObject = dataObject.GetData("HTML Format");
      if(htmlObject is string htmlTextItem)
      {
        Trace.TraceInformation($"HTML object text is {htmlTextItem}");
        var fragmentStart = htmlTextItem.IndexOf("<!--StartFragment-->");
        if(fragmentStart >= 0)
        {
          fragmentStart += "<!--StartFragment-->".Length;
          var fragmentEnd = htmlTextItem.LastIndexOf("<!--EndFragment-->");
          if(fragmentEnd > fragmentStart)
          {
            //Trace.TraceInformation($"fragment: {fragmentStart} - {fragmentEnd}");
            var fragment = htmlTextItem.Substring(fragmentStart, fragmentEnd - fragmentStart);
            Trace.TraceInformation($"Found HTML fragment: {fragment}");
          }
        }
      }
    }
  }
}

/*
 Some noteworthy parsing names:
"Microsoft.ScreenSketch_8wekyb3d8bbwe!App"
"CanonicalGroupLimited.UbuntuonWindows_79rhkp1fndgsc!ubuntu"
"windows.immersivecontrolpanel_cw5n1h2txyewy!microsoft.windows.immersivecontrolpanel"
  (that's the settings app)
"{F38BF404-1D43-42F2-9305-67DE0B28FC23}\\regedit.exe"
  (that GUID is the folder ID for the Windows folder)
"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\VideoLAN\\VLC\\vlc.exe"
  (that GUID is the folder ID for the Program Files x86 folder)
"Apple.iTunes"
"308046B0AF4A39CB"
  (that's ... Firefox ?!)
"Microsoft.AutoGenerated.{BB044BFD-25B7-2FAA-22A8-6371A93E0456}"
  (those seem LNK files imported to some random GUID. In this case Event Viewer)
"bethesdanet://run/31"
  (some custom URL handler (bethesda game, in this case))
"C:\\Users\\ttelcl\\AppData\\Local\\JetBrains\\Installations\\dotPeek221\\dotPeek64.exe"
  (plain file)
 */

public class ShellAppInfo
{
  public string? Name { get; set; }

  public string? ParsingName { get; set; }

  // public bool IsFileSystemObject { get; set; } // always false

  // public bool IsLink { get; set; } // always false

  // Pointless - always "Microsoft.WindowsAPICodePack.Shell.ShellNonFileSystemItem"
  //public string? TypeName { get; set; }

  public static ShellAppInfo FromShellObject(
    ShellObject shell)
  {
    return new ShellAppInfo() {
      Name = shell.Name,
      ParsingName = shell.ParsingName,
      // IsFileSystemObject = shell.IsFileSystemObject,
      // IsLink = shell.IsLink,
      //TypeName = shell.GetType().FullName,
    };
  }
}
