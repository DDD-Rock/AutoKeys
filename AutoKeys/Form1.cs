using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoKeys
{
    public partial class Form1 : Form
    {
        // 规则组面板列表
        private readonly List<RuleGroupPanel> _rulePanels = new List<RuleGroupPanel>();

        // 按键执行引擎
        private KeySender _keySender;

        // 运行状态
        private bool _isRunning = false;

        // 快捷键设置
        private Keys _startHotkey = Keys.Control | Keys.F1;
        private Keys _stopHotkey = Keys.Control | Keys.F2;
        private const int HOTKEY_ID_START = 1;
        private const int HOTKEY_ID_STOP = 2;

        // 快捷键监听状态
        private bool _listeningStartHotkey = false;
        private bool _listeningStopHotkey = false;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            LoadConfig();
            RegisterHotKeys();
        }

        private void InitializeUI()
        {
            // 刷新窗口列表
            RefreshWindowList();

            // 初始化按钮状态
            UpdateRunningState(false);
            
            // 设置程序窗体及任务栏图标（直接从本身打包好的 EXE 中提取内置图标，不用外部文件）
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        }

        #region 窗口列表

        private void RefreshWindowList()
        {
            var selectedHandle = (_cboWindows.SelectedItem as WindowInfo)?.Handle ?? IntPtr.Zero;
            _cboWindows.Items.Clear();

            var windows = NativeHelper.GetVisibleWindows();
            foreach (var w in windows)
            {
                _cboWindows.Items.Add(w);
                if (w.Handle == selectedHandle)
                    _cboWindows.SelectedItem = w;
            }

            if (_cboWindows.SelectedItem == null && _cboWindows.Items.Count > 0)
                _cboWindows.SelectedIndex = 0;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshWindowList();
        }

        #endregion

        #region 规则管理

        private void BtnAddRule_Click(object sender, EventArgs e)
        {
            AddRulePanel(new KeyRule());
            SaveConfig();
        }

        private void AddRulePanel(KeyRule rule)
        {
            var panel = new RuleGroupPanel(rule);
            panel.Width = _panelRules.ClientSize.Width - 5;
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.DeleteClicked += (s, ev) =>
            {
                _rulePanels.Remove(panel);
                _panelRules.Controls.Remove(panel);
                ReorderPanels();
                panel.Dispose();
                SaveConfig();
            };
            panel.RuleChanged += (s, ev) => SaveConfig();
            panel.LayoutChanged += (s, ev) => ReorderPanels();

            _rulePanels.Add(panel);
            _panelRules.Controls.Add(panel);
            ReorderPanels();
        }

        private void ReorderPanels()
        {
            int y = 5;
            foreach (var panel in _rulePanels)
            {
                panel.Location = new Point(5, y);
                y += panel.Height + 2;
            }
        }

        #endregion

        #region 开始/停止

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            if (_isRunning)
                StopExecution();
            else
                StartExecution();
        }

        private void StartExecution()
        {
            var target = _cboWindows.SelectedItem as WindowInfo;
            if (target == null)
            {
                MessageBox.Show("请先选择目标窗口！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!NativeHelper.IsWindow(target.Handle))
            {
                MessageBox.Show("目标窗口已关闭，请刷新窗口列表重新选择！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshWindowList();
                return;
            }

            var enabledPanels = _rulePanels.Where(p => p.Rule.Enabled && p.Rule.Actions.Any(a => a.KeyCode != Keys.None)).ToList();
            if (enabledPanels.Count == 0)
            {
                MessageBox.Show("没有可执行的规则！请添加规则并设置按键。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 重置所有面板
            foreach (var panel in _rulePanels)
                panel.ResetRunState();

            // 创建并启动引擎
            _keySender = new KeySender();
            _keySender.TargetWindowClosed += (s, e) =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() =>
                    {
                        StopExecution();
                        _statusLabel.Text = "目标窗口已关闭，已自动停止";
                    }));
            };
            _keySender.AllRulesCompleted += (s, e) =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() =>
                    {
                        StopExecution();
                        _statusLabel.Text = "所有规则已完成";
                    }));
            };

            _keySender.Start(target.Handle, _rulePanels);
            UpdateRunningState(true);
        }

        private void StopExecution()
        {
            _keySender?.Stop();
            _keySender?.Dispose();
            _keySender = null;
            UpdateRunningState(false);
        }

        private void UpdateRunningState(bool running)
        {
            _isRunning = running;
            _btnStartStop.Text = running ? "⏹ 停止" : "▶ 开始";
            _btnStartStop.BackColor = running
                ? Color.FromArgb(200, 60, 60)
                : Color.FromArgb(0, 120, 215);
            _statusLabel.Text = running ? "● 运行中" : "○ 已停止";
            _statusLabel.ForeColor = running
                ? Color.FromArgb(30, 150, 80)
                : Color.FromArgb(140, 140, 145);

            // 运行时禁止修改规则
            _btnAddRule.Enabled = !running;
            _cboWindows.Enabled = !running;
            _btnRefresh.Enabled = !running;

            foreach (var panel in _rulePanels)
            {
                if (!running)
                    panel.ResetRunState();
            }
        }

        #endregion

        #region 全局热键

        private void RegisterHotKeys()
        {
            UnregisterHotKeys();

            Keys startKey = _startHotkey & Keys.KeyCode;
            Keys startMods = _startHotkey & Keys.Modifiers;
            uint startNativeMods = NativeHelper.KeyModifiersToNative(startMods) | NativeHelper.MOD_NOREPEAT;
            NativeHelper.RegisterHotKey(this.Handle, HOTKEY_ID_START, startNativeMods, (uint)startKey);

            Keys stopKey = _stopHotkey & Keys.KeyCode;
            Keys stopMods = _stopHotkey & Keys.Modifiers;
            uint stopNativeMods = NativeHelper.KeyModifiersToNative(stopMods) | NativeHelper.MOD_NOREPEAT;
            NativeHelper.RegisterHotKey(this.Handle, HOTKEY_ID_STOP, stopNativeMods, (uint)stopKey);
        }

        private void UnregisterHotKeys()
        {
            NativeHelper.UnregisterHotKey(this.Handle, HOTKEY_ID_START);
            NativeHelper.UnregisterHotKey(this.Handle, HOTKEY_ID_STOP);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeHelper.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_ID_START && !_isRunning)
                    StartExecution();
                else if (id == HOTKEY_ID_STOP && _isRunning)
                    StopExecution();
            }
            base.WndProc(ref m);
        }

        #endregion

        #region 快捷键设置（监听键盘输入）

        private void BtnSetStartHotkey_Click(object sender, EventArgs e)
        {
            _listeningStartHotkey = true;
            _listeningStopHotkey = false;
            _btnSetStartHotkey.Text = "按下快捷键...";
            _btnSetStartHotkey.BackColor = Color.FromArgb(255, 180, 0);
            this.KeyPreview = true;
        }

        private void BtnSetStopHotkey_Click(object sender, EventArgs e)
        {
            _listeningStopHotkey = true;
            _listeningStartHotkey = false;
            _btnSetStopHotkey.Text = "按下快捷键...";
            _btnSetStopHotkey.BackColor = Color.FromArgb(255, 180, 0);
            this.KeyPreview = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_listeningStartHotkey || _listeningStopHotkey)
            {
                // 忽略单独的修饰键
                if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey ||
                    e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                {
                    base.OnKeyDown(e);
                    return;
                }

                Keys hotkey = e.KeyCode | e.Modifiers;

                if (_listeningStartHotkey)
                {
                    _startHotkey = hotkey;
                    _listeningStartHotkey = false;
                    _btnSetStartHotkey.Text = FormatHotkey(_startHotkey);
                    _btnSetStartHotkey.BackColor = Color.FromArgb(240, 240, 245);
                }
                else if (_listeningStopHotkey)
                {
                    _stopHotkey = hotkey;
                    _listeningStopHotkey = false;
                    _btnSetStopHotkey.Text = FormatHotkey(_stopHotkey);
                    _btnSetStopHotkey.BackColor = Color.FromArgb(240, 240, 245);
                }

                RegisterHotKeys();
                SaveConfig();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            base.OnKeyDown(e);
        }

        private string FormatHotkey(Keys hotkey)
        {
            var parts = new List<string>();
            if ((hotkey & Keys.Control) == Keys.Control) parts.Add("Ctrl");
            if ((hotkey & Keys.Alt) == Keys.Alt) parts.Add("Alt");
            if ((hotkey & Keys.Shift) == Keys.Shift) parts.Add("Shift");
            parts.Add((hotkey & Keys.KeyCode).ToString());
            return string.Join("+", parts);
        }

        #endregion

        #region 配置加载/保存

        private void SaveConfig()
        {
            var rules = _rulePanels.Select(p => p.Rule).ToList();
            ConfigManager.Save(rules, FormatHotkey(_startHotkey), FormatHotkey(_stopHotkey));
        }

        private void LoadConfig()
        {
            var config = ConfigManager.Load();

            // 加载快捷键
            _startHotkey = ParseHotkey(config.StartHotkey, Keys.Control | Keys.F1);
            _stopHotkey = ParseHotkey(config.StopHotkey, Keys.Control | Keys.F2);
            _btnSetStartHotkey.Text = FormatHotkey(_startHotkey);
            _btnSetStopHotkey.Text = FormatHotkey(_stopHotkey);

            // 加载规则
            var rules = ConfigManager.ToKeyRules(config);
            foreach (var rule in rules)
            {
                AddRulePanel(rule);
            }
        }

        private Keys ParseHotkey(string hotkeyStr, Keys defaultValue)
        {
            if (string.IsNullOrWhiteSpace(hotkeyStr)) return defaultValue;

            try
            {
                Keys result = Keys.None;
                string[] parts = hotkeyStr.Split('+');
                foreach (string part in parts)
                {
                    string trimmed = part.Trim();
                    if (trimmed.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                        result |= Keys.Control;
                    else if (trimmed.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                        result |= Keys.Alt;
                    else if (trimmed.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                        result |= Keys.Shift;
                    else
                    {
                        Keys key;
                        if (Enum.TryParse(trimmed, true, out key))
                            result |= key;
                    }
                }
                return result == Keys.None ? defaultValue : result;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region 窗口关闭

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopExecution();
            UnregisterHotKeys();
            base.OnFormClosing(e);
        }

        #endregion
    }
}
