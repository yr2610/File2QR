public class MainForm : Form
{
    private Button selectFileButton;
    private Button startDisplayButton;
    private TrackBar speedTrackBar;
    private QRCodeDisplayForm? qrCodeDisplayForm;
    private QRCodeIndexDisplayForm? qrCodeIndexDisplayForm;
    private List<Bitmap>? qrCodes;
    private HashSet<int> skippedIndexes = new HashSet<int>();

    public MainForm()
    {
        selectFileButton = new Button { Text = "ファイルを選択", Dock = DockStyle.Top };
        startDisplayButton = new Button { Text = "QRコード表示開始", Dock = DockStyle.Top, Enabled = false };
        speedTrackBar = new TrackBar { Minimum = 1, Maximum = 60, Value = 30, Dock = DockStyle.Top }; // 1fpsから60fpsに設定

        selectFileButton.Click += SelectFileButton_Click;
        startDisplayButton.Click += StartDisplayButton_Click;
        speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;

        Controls.Add(speedTrackBar);
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

                var info = new QRCodeInfo
                {
                    TotalQRCodes = qrCodes.Count,
                    IsCompressed = fileData.Length < new FileInfo(openFileDialog.FileName).Length
                };

                var infoQRCode = QRCodeGenerator.GenerateInfoQRCode(info);
                qrCodes.Insert(0, infoQRCode);

                startDisplayButton.Enabled = true;
            }
        }
    }

    private void StartDisplayButton_Click(object? sender, EventArgs e)
    {
        if (qrCodes != null)
        {
            bool[] skipIndexes = new bool[qrCodes.Count]; // スキップするインデックスを管理する配列を初期化

            qrCodeDisplayForm = new QRCodeDisplayForm(qrCodes, skipIndexes);
            qrCodeDisplayForm.Show();

            qrCodeIndexDisplayForm = new QRCodeIndexDisplayForm(qrCodes, skipIndexes);
            qrCodeIndexDisplayForm.Show();
        }
    }

    private void SpeedTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        if (qrCodeDisplayForm != null)
        {
            int fps = speedTrackBar.Value;
            int interval = 1000 / fps; // fpsをミリ秒に変換
            qrCodeDisplayForm.SetInterval(interval);

            if (qrCodeIndexDisplayForm != null)
            {
                qrCodeIndexDisplayForm.SetInterval(interval);
            }
        }
    }
}
