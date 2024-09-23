public class MainForm : Form
{
    private Button selectFileButton;
    private Button startDisplayButton;
    private TrackBar speedTrackBar;
    private QRCodeDisplayForm? qrCodeDisplayForm;
    private QRCodeIndexDisplayForm? qrCodeIndexDisplayForm;
    private List<Bitmap>? qrCodes;
    private HashSet<int> skippedIndexes = new HashSet<int>();
    private System.Windows.Forms.Timer sharedTimer;
    private int currentIndex = 0;

    public MainForm()
    {
        selectFileButton = new Button { Text = "ファイルを選択", Dock = DockStyle.Top };
        startDisplayButton = new Button { Text = "QRコード表示開始", Dock = DockStyle.Top, Enabled = false };
        speedTrackBar = new TrackBar { Minimum = 1, Maximum = 60, Value = 30, Dock = DockStyle.Top }; // 1fpsから60fpsに設定

        selectFileButton.Click += SelectFileButton_Click;
        startDisplayButton.Click += StartDisplayButton_Click;
        speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;

        var skipInputTextBox = new TextBox { Dock = DockStyle.Top };
        var skipButton = new Button { Text = "スキップ", Dock = DockStyle.Top };
        var cancelSkipTextBox = new TextBox { Dock = DockStyle.Top };
        var cancelSkipButton = new Button { Text = "スキップキャンセル", Dock = DockStyle.Top };

        skipButton.Click += (sender, e) =>
        {
            string input = skipInputTextBox.Text;
            if (input.Contains(":"))
            {
                // 階層スキップ
                qrCodeDisplayForm?.SkipIndexHierarchy(input);
            }
            else if (input.Contains("-"))
            {
                // 範囲スキップ
                qrCodeDisplayForm?.SkipIndexRange(input);
            }
            else if (int.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out int index))
            {
                // 単一スキップ
                qrCodeDisplayForm?.SkipIndex(index);
            }
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
        Controls.Add(skipButton);
        Controls.Add(skipInputTextBox);
        Controls.Add(speedTrackBar);
        Controls.Add(startDisplayButton);
        Controls.Add(selectFileButton);

        sharedTimer = new System.Windows.Forms.Timer();
        sharedTimer.Interval = 1000 / speedTrackBar.Value; // 初期の切り替え速度
        sharedTimer.Tick += SharedTimer_Tick;
    }

    private void SharedTimer_Tick(object? sender, EventArgs e)
    {
        if (qrCodes == null || qrCodes.Count == 0)
        {
            return;
        }

        while (currentIndex < qrCodes.Count && QRCodeDisplayForm.SkipIndexes![currentIndex])
        {
            currentIndex = (currentIndex + 1) % qrCodes.Count;
        }

        qrCodeDisplayForm?.UpdateQRCode(currentIndex);
        qrCodeIndexDisplayForm?.UpdateQRCode(currentIndex);

        currentIndex = (currentIndex + 1) % qrCodes.Count;
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

            sharedTimer.Start(); // タイマーをスタート
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
            sharedTimer.Interval = interval;
        }
    }
}
