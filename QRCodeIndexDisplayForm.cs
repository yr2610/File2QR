using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing;
using System.Windows.Forms;

public class QRCodeIndexDisplayForm : Form
{
    private PictureBox pictureBox;
    private List<Bitmap> qrCodes;

    public QRCodeIndexDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        this.qrCodes = qrCodes;
        this.Size = new Size(300, 150); // ウィンドウの幅を広げ、高さを固定

        pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
        Controls.Add(pictureBox);

        this.Resize += new EventHandler(QRCodeIndexDisplayForm_Resize); // ウィンドウサイズ変更イベントを追加
    }

    private void QRCodeIndexDisplayForm_Resize(object? sender, EventArgs e)
    {
        int width = this.ClientSize.Width;
        pictureBox.Size = new Size(width, pictureBox.Height); // 幅だけを変更
    }

    public void UpdateQRCode(int index)
    {
        var qrWriter = new BarcodeWriter<Bitmap>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 100,
                Width = 100,
                Margin = 1
            },
            Renderer = new BitmapRenderer()
        };

        string hexIndex = index.ToString("X"); // 16進数に変換
        pictureBox.Image = qrWriter.Write(hexIndex);
    }

}
