using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing;

public class QRCodeIndexDisplayForm : Form
{
    private PictureBox pictureBox;
    private System.Windows.Forms.Timer timer;
    private List<Bitmap> qrCodes;
    private int currentIndex = 0;

    public QRCodeIndexDisplayForm(List<Bitmap> qrCodes)
    {
        this.qrCodes = qrCodes;
        this.Size = new Size(150, 150);

        pictureBox = new PictureBox { Dock = DockStyle.Fill };
        Controls.Add(pictureBox);

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 500; // 初期の切り替え速度（ミリ秒）
        timer.Tick += (sender, e) => ShowNextQRCode();
        timer.Start();
    }

    private void ShowNextQRCode()
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

        pictureBox.Image = qrWriter.Write($"Index: {currentIndex + 1}/{qrCodes.Count}");
        currentIndex = (currentIndex + 1) % qrCodes.Count;
    }

    public void SetInterval(int interval)
    {
        timer.Interval = interval;
    }
}
