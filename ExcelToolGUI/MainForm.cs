using ExcelTool;
using Newtonsoft.Json;

namespace ExcelToolGUI
{
    public class MainForm : Form
    {
        private TextBox _txtJsonPath = null!;
        private TextBox _txtExcelPath = null!;
        private TextBox _txtScriptOutput = null!;
        private TextBox _txtBytesOutput = null!;
        private TextBox _txtNamespace = null!;
        private ComboBox _cmbEncryptAlgorithm = null!;
        private Button _btnExport = null!;
        private Button _btnExportBytes = null!;
        private TextBox _txtLog = null!;
        private Label _lblStatus = null!;

        // 用户设置保存路径
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Text = "ExcelTool 配置导出工具";
            Size = new Size(650, 550);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            int y = 15;
            int labelWidth = 90;
            int inputWidth = 440;
            int btnX = 545;

            // JSON配置文件
            AddPathRow(ref y, "配置文件:", out _txtJsonPath, "*.json|*.json", labelWidth, inputWidth, btnX);

            // Excel根目录
            AddFolderRow(ref y, "Excel目录:", out _txtExcelPath, labelWidth, inputWidth, btnX);

            // 脚本输出目录
            AddFolderRow(ref y, "脚本输出:", out _txtScriptOutput, labelWidth, inputWidth, btnX);

            // 字节文件输出目录
            AddFolderRow(ref y, "字节输出:", out _txtBytesOutput, labelWidth, inputWidth, btnX);

            // 命名空间
            var lblNs = new Label { Text = "命名空间:", Location = new Point(15, y + 3), AutoSize = true };
            Controls.Add(lblNs);
            _txtNamespace = new TextBox { Location = new Point(15 + labelWidth, y), Size = new Size(200, 23), Text = "GameConfig" };
            Controls.Add(_txtNamespace);

            y += 30;

            // 加密算法
            var lblEncryptAlgo = new Label { Text = "加密算法:", Location = new Point(15, y + 3), AutoSize = true };
            Controls.Add(lblEncryptAlgo);
            _cmbEncryptAlgorithm = new ComboBox
            {
                Location = new Point(15 + labelWidth, y),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbEncryptAlgorithm.Items.AddRange(new object[] { "无加密", "异或加密(XOR)", "AES加密", "位移加密(Shift)" });
            _cmbEncryptAlgorithm.SelectedIndex = 1; // 默认选择异或加密
            Controls.Add(_cmbEncryptAlgorithm);

            // 导出按钮（生成脚本 + 字节文件）
            _btnExport = new Button
            {
                Text = "导出配置",
                Location = new Point(btnX - 60, y - 2),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnExport.Click += BtnExport_Click;
            Controls.Add(_btnExport);

            // 仅生成字节文件按钮（只生成 config.bytes，不生成脚本）
            _btnExportBytes = new Button
            {
                Text = "仅生成Bytes",
                Location = new Point(btnX - 175, y - 2),
                Size = new Size(105, 28),
                BackColor = Color.FromArgb(60, 150, 110),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnExportBytes.Click += BtnExportBytes_Click;
            Controls.Add(_btnExportBytes);

            y += 35;

            // 日志输出框
            _txtLog = new TextBox
            {
                Location = new Point(15, y),
                Size = new Size(610, 280),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(200, 200, 200)
            };
            Controls.Add(_txtLog);

            y += 285;

            // 状态栏
            _lblStatus = new Label
            {
                Text = "就绪",
                Location = new Point(15, y),
                Size = new Size(610, 20),
                ForeColor = Color.Gray
            };
            Controls.Add(_lblStatus);
        }

        private void AddPathRow(ref int y, string label, out TextBox textBox, string filter, int labelWidth, int inputWidth, int btnX)
        {
            var lbl = new Label { Text = label, Location = new Point(15, y + 3), AutoSize = true };
            Controls.Add(lbl);

            textBox = new TextBox { Location = new Point(15 + labelWidth, y), Size = new Size(inputWidth, 23) };
            textBox.AllowDrop = true;
            var tb = textBox;
            textBox.DragEnter += (s, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
            textBox.DragDrop += (s, e) => { var f = e.Data?.GetData(DataFormats.FileDrop) as string[]; if (f?.Length > 0) tb.Text = f[0]; };
            Controls.Add(textBox);

            var btn = new Button { Text = "...", Location = new Point(btnX, y - 1), Size = new Size(35, 25) };
            string f2 = filter;
            var tb2 = textBox;
            btn.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = f2, Title = $"选择{label.TrimEnd(':')}" };
                if (dlg.ShowDialog() == DialogResult.OK) tb2.Text = dlg.FileName;
            };
            Controls.Add(btn);

            y += 30;
        }

        private void AddFolderRow(ref int y, string label, out TextBox textBox, int labelWidth, int inputWidth, int btnX)
        {
            var lbl = new Label { Text = label, Location = new Point(15, y + 3), AutoSize = true };
            Controls.Add(lbl);

            textBox = new TextBox { Location = new Point(15 + labelWidth, y), Size = new Size(inputWidth, 23) };
            textBox.AllowDrop = true;
            var tb = textBox;
            textBox.DragEnter += (s, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
            textBox.DragDrop += (s, e) => { var f = e.Data?.GetData(DataFormats.FileDrop) as string[]; if (f?.Length > 0) tb.Text = f[0]; };
            Controls.Add(textBox);

            var btn = new Button { Text = "...", Location = new Point(btnX, y - 1), Size = new Size(35, 25) };
            var tb2 = textBox;
            string lbl2 = label;
            btn.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog { Description = $"选择{lbl2.TrimEnd(':')}" };
                if (dlg.ShowDialog() == DialogResult.OK) tb2.Text = dlg.SelectedPath;
            };
            Controls.Add(btn);

            y += 30;
        }

        private async void BtnExport_Click(object? sender, EventArgs e)
        {
            await RunExport(bytesOnly: false);
        }

        private async void BtnExportBytes_Click(object? sender, EventArgs e)
        {
            await RunExport(bytesOnly: true);
        }

        /// <summary>
        /// 执行导出
        /// </summary>
        /// <param name="bytesOnly">为 true 时只生成 config.bytes，不生成脚本（无需脚本输出目录）</param>
        private async Task RunExport(bool bytesOnly)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(_txtJsonPath.Text))
            { MessageBox.Show("请选择配置文件", "提示"); return; }
            if (string.IsNullOrWhiteSpace(_txtExcelPath.Text))
            { MessageBox.Show("请选择Excel目录", "提示"); return; }
            // 仅生成Bytes时不需要脚本输出目录
            if (!bytesOnly && string.IsNullOrWhiteSpace(_txtScriptOutput.Text))
            { MessageBox.Show("请选择脚本输出目录", "提示"); return; }
            if (string.IsNullOrWhiteSpace(_txtBytesOutput.Text))
            { MessageBox.Show("请选择字节文件输出目录", "提示"); return; }

            string jsonPath = _txtJsonPath.Text.Trim();
            if (!File.Exists(jsonPath))
            { MessageBox.Show($"配置文件不存在: {jsonPath}", "错误"); return; }
            if (!Directory.Exists(_txtExcelPath.Text.Trim()))
            { MessageBox.Show($"Excel目录不存在: {_txtExcelPath.Text}", "错误"); return; }

            // 保存设置
            SaveSettings();

            _btnExport.Enabled = false;
            _btnExportBytes.Enabled = false;
            _txtLog.Clear();
            _lblStatus.Text = bytesOnly ? "正在生成字节文件..." : "正在导出...";
            _lblStatus.ForeColor = Color.Orange;

            var logWriter = new TextBoxWriter(_txtLog);
            var originalOut = Console.Out;
            Console.SetOut(logWriter);

            try
            {
                await Task.Run(() =>
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    CfgModel? model = JsonConvert.DeserializeObject<CfgModel>(jsonContent);

                    if (model == null)
                    {
                        Console.WriteLine("错误：配置数据反序列化失败");
                        return;
                    }

                    // 从GUI填充路径和设置
                    model.excelPath = Path.GetFullPath(_txtExcelPath.Text.Trim());
                    if (!bytesOnly)
                        model.scriptOutputPath = Path.GetFullPath(_txtScriptOutput.Text.Trim());
                    model.bytesOutputPath = Path.GetFullPath(_txtBytesOutput.Text.Trim());
                    model.ns = _txtNamespace.Text.Trim();
                    model.encryptAlgorithm = GetSelectedAlgorithm();

                    Console.WriteLine($"命名空间: {model.Namespace}");
                    Console.WriteLine($"Excel路径: {model.excelPath}");
                    if (!bytesOnly)
                        Console.WriteLine($"脚本输出: {model.scriptOutputPath}");
                    Console.WriteLine($"字节输出: {model.bytesOutputPath}");
                    Console.WriteLine();

                    if (bytesOnly)
                        Generator.GenerateBytesOnly(model);
                    else
                        Generator.Generate(model);
                });

                _lblStatus.Text = bytesOnly ? "字节文件生成完成" : "导出完成";
                _lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                _txtLog.AppendText($"\n错误：{ex.Message}\n");
                _lblStatus.Text = bytesOnly ? "生成失败" : "导出失败";
                _lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                Console.SetOut(originalOut);
                _btnExport.Enabled = true;
                _btnExportBytes.Enabled = true;
            }
        }

        #region 设置持久化

        private void SaveSettings()
        {
            var settings = new UserSettings
            {
                JsonPath = _txtJsonPath.Text,
                ExcelPath = _txtExcelPath.Text,
                ScriptOutputPath = _txtScriptOutput.Text,
                BytesOutputPath = _txtBytesOutput.Text,
                Namespace = _txtNamespace.Text,
                EncryptAlgorithm = GetSelectedAlgorithm()
            };
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath)) return;
            try
            {
                string json = File.ReadAllText(SettingsPath);
                var settings = JsonConvert.DeserializeObject<UserSettings>(json);
                if (settings == null) return;

                _txtJsonPath.Text = settings.JsonPath ?? "";
                _txtExcelPath.Text = settings.ExcelPath ?? "";
                _txtScriptOutput.Text = settings.ScriptOutputPath ?? "";
                _txtBytesOutput.Text = settings.BytesOutputPath ?? "";
                _txtNamespace.Text = settings.Namespace ?? "GameConfig";
                SetSelectedAlgorithm(settings.EncryptAlgorithm);
            }
            catch { /* 设置文件损坏则忽略 */ }
        }

        #endregion

        #region 加密算法辅助方法

        /// <summary>
        /// 获取选中的加密算法字符串
        /// </summary>
        private string GetSelectedAlgorithm()
        {
            return _cmbEncryptAlgorithm.SelectedIndex switch
            {
                0 => "NONE",
                1 => "XOR",
                2 => "AES",
                3 => "SHIFT",
                _ => "XOR"
            };
        }

        /// <summary>
        /// 设置选中的加密算法
        /// </summary>
        private void SetSelectedAlgorithm(string? algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                _cmbEncryptAlgorithm.SelectedIndex = 1; // 默认XOR
                return;
            }

            _cmbEncryptAlgorithm.SelectedIndex = algorithm.ToUpper() switch
            {
                "NONE" => 0,
                "XOR" => 1,
                "AES" => 2,
                "SHIFT" => 3,
                _ => 1 // 默认XOR
            };
        }

        #endregion
    }

    /// <summary>
    /// 用户设置（持久化到settings.json）
    /// </summary>
    internal class UserSettings
    {
        public string? JsonPath { get; set; }
        public string? ExcelPath { get; set; }
        public string? ScriptOutputPath { get; set; }
        public string? BytesOutputPath { get; set; }
        public string? Namespace { get; set; }
        public string? EncryptAlgorithm { get; set; }
    }

    /// <summary>
    /// Console输出重定向到TextBox
    /// </summary>
    internal class TextBoxWriter : System.IO.TextWriter
    {
        private readonly TextBox _textBox;
        public TextBoxWriter(TextBox textBox) => _textBox = textBox;
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value) => AppendText(value.ToString());
        public override void Write(string? value) { if (value != null) AppendText(value); }
        public override void WriteLine(string? value) => AppendText((value ?? "") + Environment.NewLine);

        private void AppendText(string text)
        {
            if (_textBox.InvokeRequired)
                _textBox.Invoke(() => _textBox.AppendText(text));
            else
                _textBox.AppendText(text);
        }
    }
}
