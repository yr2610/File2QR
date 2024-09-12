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
    private bool[] skipIndexes; // スキップするインデックスを管理する配列
    private int currentIndex = 0;

    public QRCodeDisplayForm(List<Bitmap> qrCodes, bool[] skipIndexes)
    {
        this.qrCodes = qrCodes;
        this.skipIndexes = skipIndexes;
        this.Size = new Size(320, 360); // ウィンドウのサイズを調整

        pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
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

        this.Resize += new EventHandler(QRCodeDisplayForm_Resize); // ウィンドウサイズ変更イベントを追加
    }

    private void QRCodeDisplayForm_Resize(object? sender, EventArgs e)
    {
        int size = Math.Min(this.ClientSize.Width, this.ClientSize.Height);
        pictureBox.Size = new Size(size, size);
    }

    private void ShowNextQRCode()
    {
        while (skipIndexes[currentIndex])
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
