/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// A file storing blobs (append-only)
/// </summary>
public class FsBlobsFile
{

  /// <summary>
  /// Create a new FsBlobsFile
  /// </summary>
  public FsBlobsFile(
    string fileName)
  {
    FileName = Path.GetFullPath(fileName);
    InitFile();
  }

  /// <summary>
  /// The full path to the blobs file
  /// </summary>
  public string FileName { get; }

  /// <summary>
  /// Open the blobs file for reading. Move the read pointer to
  /// just after the header
  /// </summary>
  /// <returns></returns>
  internal FileStream OpenRead()
  {
    var fs = File.OpenRead(FileName);
    fs.Position = HeaderSize;
    return fs;
  }

  /// <summary>
  /// Open the blobs file for appending, and move the write pointer
  /// to the end
  /// </summary>
  /// <returns></returns>
  internal FileStream OpenAppend()
  {
    var fs = new FileStream(
      FileName,
      FileMode.Append,
      FileAccess.Write);
    fs.Seek(0L, SeekOrigin.End);
    return fs;
  }

  /// <summary>
  /// Read the blob described by <paramref name="descriptor"/> from
  /// the <see cref="Stream"/> opened with <see cref="OpenRead"/>.
  /// </summary>
  /// <param name="blobReader"></param>
  /// <param name="descriptor"></param>
  /// <returns></returns>
  /// <exception cref="InvalidDataException"></exception>
  public byte[] ReadBlob(FileStream blobReader, FsBlobDescriptor descriptor)
  {
    blobReader.Position = descriptor.Offset;
    Span<byte> sizeBuffer = stackalloc byte[4];
    blobReader.ReadExactly(sizeBuffer);
    var size = BinaryPrimitives.ReadInt32LittleEndian(sizeBuffer);
    if(size != descriptor.Size)
    {
      throw new InvalidDataException(
        $"Invalid blob file data at offset 0x{descriptor.Offset:X8} in {FileName}");
    }
    var blob = new byte[size];
    blobReader.ReadExactly(blob);
    return blob;
  }

  /// <summary>
  /// Make sure the file exists and has its header written out
  /// </summary>
  private void InitFile()
  {
    if(!File.Exists(FileName))
    {
      File.WriteAllBytes(FileName, BitConverter.GetBytes(Signature));
    }
  }

  internal const ulong Signature = 0x524F5453424F4C42; // BLOBSTOR
  internal const int HeaderSize = 8;
}
