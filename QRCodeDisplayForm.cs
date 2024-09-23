using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing;

public class QRCodeDisplayForm : Form
{
    private PictureBox qrCodePictureBox = null!;
    private Label indexLabel = null!;
    private List<Bitmap> qrCodes;
    private static bool[]? skipIndexes; // private に変更
    public static int currentIndex = 0; // 静的プロパティに変更

    public QRCodeDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        QRCodeDisplayForm.skipIndexes = skipIndexes; // 静的プロパティに設定
        this.qrCodes = qrCodes;
        InitializeComponents();
        StartDisplay();
    }

    public static bool[]? SkipIndexes => skipIndexes; // プロパティを追加

    private void InitializeComponents()
    {
        this.qrCodePictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        this.indexLabel = new Label
        {
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter
        };

        this.Controls.Add(this.qrCodePictureBox);
        this.Controls.Add(this.indexLabel);
    }

    private void StartDisplay()
    {
        // タイマーのスタートは不要になったため削除
    }

    public void SkipIndex(int index)
    {
        if (index >= 0 && index < skipIndexes!.Length)
        {
            skipIndexes[index] = true;
        }
    }

    public void SkipIndexRange(string hexRange)
    {
        var parts = hexRange.Split('-');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out int start) &&
            int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int end))
        {
            for (int i = start; i <= end; i++)
            {
                if (i >= 0 && i < skipIndexes!.Length)
                {
                    skipIndexes[i] = true;
                }
            }
        }
    }

    public void SkipIndexHierarchy(string hierarchy)
    {
        var parts = hierarchy.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out int level) &&
            int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int bitmask))
        {
            for (int i = 0; i < 8; i++)
            {
                if ((bitmask & (1 << i)) != 0)
                {
                    int index = level * 8 + i;
                    if (index >= 0 && index < skipIndexes!.Length)
                    {
                        skipIndexes[index] = true;
                    }
                }
            }
        }
    }

    public void CancelSkipIndex(int index)
    {
        if (index >= 0 && index < skipIndexes!.Length)
        {
            skipIndexes[index] = false;
        }
    }

    public void UpdateQRCode(int index, Color color)
    {
        if (index < qrCodes.Count)
        {
            var qrWriter = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = qrCodes[index].Height,
                    Width = qrCodes[index].Width,
                    Margin = 1
                },
                Renderer = new BitmapRenderer { Foreground = color } // 色を設定
            };

            // QRコードの内容を取得して新たに生成
            var qrCodeContent = GetQRCodeContent(qrCodes[index]);
            qrCodePictureBox.Image = qrWriter.Write(qrCodeContent);
            indexLabel.Text = $"Index: {index + 1}/{qrCodes.Count}";
        }
    }

    private string GetQRCodeContent(Bitmap qrCode)
    {
        var reader = new BarcodeReader();
        var result = reader.Decode(qrCode);
        return result?.Text ?? string.Empty;
    }
}
