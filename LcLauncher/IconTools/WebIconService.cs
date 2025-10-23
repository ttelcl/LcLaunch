/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// Tool for downloading icons for web sites
/// </summary>
public static class WebIconService
{
  private static readonly HttpClient __httpClient;

  static WebIconService()
  {
    var handler = new SocketsHttpHandler {
      PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    };
    __httpClient = new HttpClient(handler);
  }

  public static Uri ServiceUrl { get; } =
    new Uri("http://www.google.com/s2/favicons");

  public static async Task<byte[]?> GetWebIconBytes(
    Uri site, int size)
  {
    var domain = new Uri(site, "/"); // get the site base (scheme, host, port)
    var uri = 
      ServiceUrl
      + "?domain="
      + UrlEncoder.Default.Encode(domain.ToString())
      + "&sz=" + size;
    try
    {
      var bytes = await __httpClient.GetByteArrayAsync(uri);
      return bytes;
    }
    catch(Exception ex)
    {
      Trace.TraceError($"Error downloading {uri}: {ex}");
      return null;
    }
  }

  //public static async Task<BitmapSource?> GetWebIcon(
  //  Uri site,
  //  int size)
  //{
  //  // We will re-calculate the HashId later anyway, so no need to calculate it here
  //  var bytes = await GetWebIconBytes(site, size);
  //  if(bytes == null)
  //  {
  //    return null;
  //  }
  //  var icon = IconExtraction.BlobToIcon(bytes);
  //  if(icon == null)
  //  {
  //    return null;
  //  }
  //  return icon;
  //}

}
