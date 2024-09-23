public class QRCodeDisplayForm : Form
{
    private PictureBox qrCodePictureBox = null!;
    private Label indexLabel = null!;
    private System.Windows.Forms.Timer displayTimer = null!;
    private List<Bitmap> qrCodes;
    public static bool[]? skipIndexes; // null 許容に変更
    public static int currentIndex = 0; // 静的プロパティに変更

    public QRCodeDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        QRCodeDisplayForm.skipIndexes = skipIndexes; // 静的プロパティに設定
        this.qrCodes = qrCodes;
        InitializeComponents();
        StartDisplay();
    }

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
        this.displayTimer = new System.Windows.Forms.Timer();

        this.Controls.Add(this.qrCodePictureBox);
        this.Controls.Add(this.indexLabel);

        this.displayTimer.Interval = 1000; // 1秒ごとに表示を更新
        this.displayTimer.Tick += DisplayTimer_Tick;
    }

    private void StartDisplay()
    {
        this.displayTimer.Start();
    }

    private void DisplayTimer_Tick(object? sender, EventArgs e)
    {
        while (currentIndex < qrCodes.Count && skipIndexes![currentIndex])
        {
            currentIndex++;
        }

        if (currentIndex < qrCodes.Count)
        {
            qrCodePictureBox.Image = qrCodes[currentIndex];
            indexLabel.Text = $"Index: {currentIndex + 1}/{qrCodes.Count}";
            currentIndex = (currentIndex + 1) % qrCodes.Count; // 修正
        }
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

    public void SetInterval(int interval)
    {
        displayTimer.Interval = interval;
    }
}
