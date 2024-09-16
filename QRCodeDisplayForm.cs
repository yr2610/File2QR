public class QRCodeDisplayForm : Form
{
    private PictureBox qrCodePictureBox = null!;
    private Label indexLabel = null!;
    private System.Windows.Forms.Timer displayTimer = null!;
    private List<Bitmap> qrCodes;
    private bool[] skipIndexes;
    private int currentIndex = 0;

    public QRCodeDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        this.qrCodes = qrCodes;
        this.skipIndexes = skipIndexes;
        InitializeComponents();
        StartDisplay();
    }

    private void InitializeComponents()
    {
        this.qrCodePictureBox = new PictureBox();
        this.indexLabel = new Label();
        this.displayTimer = new System.Windows.Forms.Timer();

        this.qrCodePictureBox.Dock = DockStyle.Fill;
        this.indexLabel.Dock = DockStyle.Top;
        this.indexLabel.TextAlign = ContentAlignment.MiddleCenter;

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
        while (currentIndex < qrCodes.Count && skipIndexes[currentIndex])
        {
            currentIndex++;
        }

        if (currentIndex < qrCodes.Count)
        {
            qrCodePictureBox.Image = qrCodes[currentIndex];
            indexLabel.Text = $"Index: {currentIndex + 1}/{qrCodes.Count}";
            currentIndex++;
        }
        else
        {
            this.displayTimer.Stop();
        }
    }

    public void SkipIndex(int index)
    {
        if (index >= 0 && index < skipIndexes.Length)
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
                if (i >= 0 && i < skipIndexes.Length)
                {
                    skipIndexes[i] = true;
                }
            }
        }
    }

    public void SkipIndexHierarchy(string hexHierarchy)
    {
        if (int.TryParse(hexHierarchy, System.Globalization.NumberStyles.HexNumber, null, out int index))
        {
            int start = index * 256;
            int end = start + 255;
            for (int i = start; i <= end; i++)
            {
                if (i >= 0 && i < skipIndexes.Length)
                {
                    skipIndexes[i] = true;
                }
            }
        }
    }

    public void CancelSkipIndex(int index)
    {
        if (index >= 0 && index < skipIndexes.Length)
        {
            skipIndexes[index] = false;
        }
    }

    public void SetInterval(int interval)
    {
        displayTimer.Interval = interval;
    }
}
