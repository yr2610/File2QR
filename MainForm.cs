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

        var skipIndexTextBox = new TextBox { Dock = DockStyle.Top };
        var skipIndexButton = new Button { Text = "スキップ", Dock = DockStyle.Top };
        var skipRangeTextBox = new TextBox { Dock = DockStyle.Top };
        var skipRangeButton = new Button { Text = "範囲スキップ", Dock = DockStyle.Top };
        var skipHierarchyTextBox = new TextBox { Dock = DockStyle.Top };
        var skipHierarchyButton = new Button { Text = "階層スキップ", Dock = DockStyle.Top };
        var cancelSkipTextBox = new TextBox { Dock = DockStyle.Top };
        var cancelSkipButton = new Button { Text = "スキップキャンセル", Dock = DockStyle.Top };

        skipIndexButton.Click += (sender, e) =>
        {
            if (int.TryParse(skipIndexTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int index))
            {
                qrCodeDisplayForm?.SkipIndex(index);
            }
        };

        skipRangeButton.Click += (sender, e) =>
        {
            qrCodeDisplayForm?.SkipIndexRange(skipRangeTextBox.Text);
        };

        skipHierarchyButton.Click += (sender, e) =>
        {
            qrCodeDisplayForm?.SkipIndexHierarchy(skipHierarchyTextBox.Text);
        };

        cancelSkipButton.Click += (sender, e) =>
        {
            if (int.TryParse(cancelSkipTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int index))
            {
                qrCodeDisplayForm?.CancelSkipIndex(index);
            }
        };

        Controls.Add(cancelSkipButton);
        Controls.Add(cancelSkipTextBox);
        Controls.Add(skipHierarchyButton);
        Controls.Add(skipHierarchyTextBox);
        Controls.Add(skipRangeButton);
        Controls.Add(skipRangeTextBox);
        Controls.Add(skipIndexButton);
        Controls.Add(skipIndexTextBox);
        Controls.Add(speedTrackBar);
        Controls.Add(startDisplayButton);
        Controls.Add(selectFileButton);
    }

    private string ComputeSHA256(byte[] data)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    private void StartDisplay()
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
                    IsCompressed = fileData.Length < new FileInfo(openFileDialog.FileName).Length,
                    FileSHA256 = ComputeSHA256(fileData)
                };

                string yamlInfo = info.ToYaml();
                var infoQRCode = QRCodeGenerator.GenerateQRCode(yamlInfo);
                qrCodes.Insert(0, infoQRCode); // 情報QRコードをリストの最初に追加

                var infoForm = new QRCodeInfoForm(infoQRCode);
                infoForm.StartButtonClicked += (s, args) => StartDisplay();
                infoForm.Show();
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
