using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing;
using System.Windows.Forms;

public class QRCodeIndexDisplayForm : Form
{
    private PictureBox pictureBox;
    private System.Windows.Forms.Timer timer;
    private List<Bitmap> qrCodes;
    private bool[] skipIndexes; // スキップするインデックスを管理する配列
    private int currentIndex = 0;

    public QRCodeIndexDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        this.qrCodes = qrCodes;
        this.skipIndexes = skipIndexes;
        this.Size = new Size(300, 150); // ウィンドウの幅を広げ、高さを固定

        pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
        Controls.Add(pictureBox);

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 500; // 初期の切り替え速度（ミリ秒）
        timer.Tick += (sender, e) => ShowNextQRCode();
        timer.Start();

        this.Resize += new EventHandler(QRCodeIndexDisplayForm_Resize); // ウィンドウサイズ変更イベントを追加
    }

    private void QRCodeIndexDisplayForm_Resize(object? sender, EventArgs e)
    {
        int width = this.ClientSize.Width;
        pictureBox.Size = new Size(width, pictureBox.Height); // 幅だけを変更
    }

    private void ShowNextQRCode()
    {
        while (skipIndexes[currentIndex])
        {
            currentIndex = (currentIndex + 1) % qrCodes.Count;
        }

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
