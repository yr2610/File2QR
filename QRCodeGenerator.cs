using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using System.Drawing;

public class QRCodeGenerator
{
    public static List<Bitmap> GenerateQRCodes(byte[] data, int chunkSize)
    {
        var qrCodes = new List<Bitmap>();
        for (int i = 0; i < data.Length; i += chunkSize)
        {
            byte[] chunk = data.Skip(i).Take(chunkSize).ToArray();
            string base64String = Convert.ToBase64String(chunk);

            var qrWriter = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                },
                Renderer = new BitmapRenderer() // ここでレンダラーを設定
            };

            qrCodes.Add(qrWriter.Write(base64String));
        }
        return qrCodes;
    }
}
