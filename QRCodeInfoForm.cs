public class QRCodeInfoForm : Form
{
    private PictureBox qrCodePictureBox;
    private Button startButton;
    public event EventHandler? StartButtonClicked;

    public QRCodeInfoForm(Bitmap qrCode)
    {
        qrCodePictureBox = new PictureBox
        {
            Image = qrCode,
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        startButton = new Button
        {
            Text = "開始",
            Dock = DockStyle.Bottom
        };
        startButton.Click += (sender, e) => StartButtonClicked?.Invoke(this, EventArgs.Empty);

        Controls.Add(qrCodePictureBox);
        Controls.Add(startButton);
    }
}
