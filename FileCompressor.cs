using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

public class FileCompressor
{
    public static byte[] CompressFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        using (var compressedStream = new MemoryStream())
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(fileData, 0, fileData.Length);
            }
            return compressedStream.ToArray();
        }
    }

    public static byte[] GetFileData(string filePath)
    {
        byte[] originalData = File.ReadAllBytes(filePath);
        byte[] compressedData = CompressFile(filePath);

        return compressedData.Length < originalData.Length ? compressedData : originalData;
    }
}
