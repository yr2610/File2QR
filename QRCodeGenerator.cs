using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using System.Drawing;

public class QRCodeGenerator
{
    public static Bitmap GenerateQRCode(string data, Color color)
    {
        var qrWriter = new ZXing.BarcodeWriter<Bitmap>
        {
            Format = ZXing.BarcodeFormat.QR_CODE,
            Options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                Height = 250,
                Width = 250,
                Margin = 1
            },
            Renderer = new ZXing.Windows.Compatibility.BitmapRenderer { Foreground = color }
        };

        return qrWriter.Write(data);
    }

    public static List<Bitmap> GenerateQRCodes(byte[] data, int chunkSize, Color color)
    {
        var qrCodes = new List<Bitmap>();
        for (int i = 0; i < data.Length; i += chunkSize)
        {
            int size = Math.Min(chunkSize, data.Length - i);
            byte[] chunk = new byte[size];
            Array.Copy(data, i, chunk, 0, size);
            string base64Chunk = Convert.ToBase64String(chunk);
            qrCodes.Add(GenerateQRCode(base64Chunk, color));
        }
        return qrCodes;
    }

    public static Bitmap GenerateInfoQRCode(QRCodeInfo info, Color color)
    {
        var qrWriter = new BarcodeWriter<Bitmap>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 300,
                Width = 300,
                Margin = 1
            },
            Renderer = new BitmapRenderer { Foreground = color }
        };

        return qrWriter.Write(info.ToYaml());
    }
}
