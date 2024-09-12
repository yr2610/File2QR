using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class QRCodeDisplayForm : Form
{
    private PictureBox pictureBox;
    private Label indexLabel;
    private System.Windows.Forms.Timer timer;
    private List<Bitmap> qrCodes;
    private HashSet<int> skippedIndexes;
    private int currentIndex = 0;

    public QRCodeDisplayForm(List<Bitmap> qrCodes, HashSet<int> skippedIndexes)
    {
        this.qrCodes = qrCodes;
        this.skippedIndexes = skippedIndexes;
        this.Size = new Size(320, 360); // ウィンドウのサイズを調整

        pictureBox = new PictureBox { Dock = DockStyle.Fill };
        Controls.Add(pictureBox);

        indexLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.Red,
            BackColor = Color.Transparent,
            Location = new Point(10, 10),
            Parent = pictureBox // ラベルの親コントロールを設定
        };
        pictureBox.Controls.Add(indexLabel); // ラベルをPictureBoxに追加

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 500; // 初期の切り替え速度（ミリ秒）
        timer.Tick += (sender, e) => ShowNextQRCode();
        timer.Start();
    }

    private void ShowNextQRCode()
    {
        while (skippedIndexes.Contains(currentIndex))
        {
            currentIndex = (currentIndex + 1) % qrCodes.Count;
        }

        pictureBox.Image = qrCodes[currentIndex];
        indexLabel.Text = $"Index: {currentIndex + 1}/{qrCodes.Count}";
        currentIndex = (currentIndex + 1) % qrCodes.Count;
    }

    public void SetInterval(int interval)
    {
        timer.Interval = interval;
    }
}
