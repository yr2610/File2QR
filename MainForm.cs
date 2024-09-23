using YamlDotNet.Serialization;

public class MainForm : Form
{
    private Button selectFileButton;
    private Button startDisplayButton;
    private TrackBar speedTrackBar;
    private Button colorSelectButton;
    private Color selectedColor = Color.Black; // デフォルトの色を黒に設定
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
        speedTrackBar = new TrackBar { Minimum = 1, Maximum = 60, Value = 2, Dock = DockStyle.Top }; // 1fpsから60fpsに設定
        colorSelectButton = new Button { Text = "色を選択", Dock = DockStyle.Top };

        selectFileButton.Click += SelectFileButton_Click;
        startDisplayButton.Click += StartDisplayButton_Click;
        speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;
        colorSelectButton.Click += ColorSelectButton_Click;

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
        Controls.Add(colorSelectButton);
        Controls.Add(speedTrackBar);
        Controls.Add(startDisplayButton);
        Controls.Add(selectFileButton);

        sharedTimer = new System.Windows.Forms.Timer();
        sharedTimer.Interval = 1000 / speedTrackBar.Value; // 初期の切り替え速度
        sharedTimer.Tick += SharedTimer_Tick;
    }

    private void ColorSelectButton_Click(object? sender, EventArgs e)
    {
        using (var colorDialog = new ColorDialog())
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                selectedColor = colorDialog.Color;
                // 設定を記憶するために、config.yaml に保存する処理を追加
                SaveColorToConfig(selectedColor);
            }
        }
    }

    private void SaveColorToConfig(Color color)
    {
        // config.yaml に色を保存する処理を実装
        var config = new { Color = ColorTranslator.ToHtml(color) };
        var yaml = new Serializer().Serialize(config);
        File.WriteAllText("config.yaml", yaml);
    }

    private Color LoadColorFromConfig()
    {
        if (File.Exists("config.yaml"))
        {
            var yaml = File.ReadAllText("config.yaml");
            var config = new Deserializer().Deserialize<dynamic>(yaml);
            return ColorTranslator.FromHtml(config["Color"]);
        }
        return Color.Black; // デフォルトの色を黒に設定
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

        qrCodeDisplayForm?.UpdateQRCode(currentIndex, selectedColor);
        qrCodeIndexDisplayForm?.UpdateQRCode(currentIndex, selectedColor);

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
                qrCodes = QRCodeGenerator.GenerateQRCodes(fileData, 1000, selectedColor); // 色を指定してQRコードを生成

                var info = new QRCodeInfo
                {
                    TotalQRCodes = qrCodes.Count,
                    IsCompressed = fileData.Length < new FileInfo(openFileDialog.FileName).Length,
                    FileSHA256 = ComputeSHA256(fileData)
                };

                string yamlInfo = info.ToYaml();
                var infoQRCode = QRCodeGenerator.GenerateInfoQRCode(info, selectedColor); // 色を指定して情報QRコードを生成

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

            sharedTimer.Start(); // タイマーをスタート
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
