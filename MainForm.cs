public class MainForm : Form
{
    private Button selectFileButton;
    private Button startDisplayButton;
    private QRCodeDisplayForm? qrCodeDisplayForm;
    private List<Bitmap>? qrCodes;

    public MainForm()
    {
        selectFileButton = new Button { Text = "ファイルを選択", Dock = DockStyle.Top };
        startDisplayButton = new Button { Text = "QRコード表示開始", Dock = DockStyle.Top, Enabled = false };

        selectFileButton.Click += SelectFileButton_Click;
        startDisplayButton.Click += StartDisplayButton_Click;

        Controls.Add(startDisplayButton);
        Controls.Add(selectFileButton);
    }

    private void SelectFileButton_Click(object? sender, EventArgs e)
    {
        using (var openFileDialog = new OpenFileDialog())
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] fileData = FileCompressor.GetFileData(openFileDialog.FileName);
                qrCodes = QRCodeGenerator.GenerateQRCodes(fileData, 1000); // 例として1000バイトごとに分割
                startDisplayButton.Enabled = true;
            }
        }
    }

    private void StartDisplayButton_Click(object? sender, EventArgs e)
    {
        if (qrCodes != null)
        {
            qrCodeDisplayForm = new QRCodeDisplayForm(qrCodes);
            qrCodeDisplayForm.Show();
        }
    }
}
