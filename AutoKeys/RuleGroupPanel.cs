using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AutoKeys
{
    public class RuleGroupPanel : UserControl
    {
        public KeyRule Rule { get; private set; }

        // Top bar controls
        private CheckBox _chkEnabled;
        private NumericUpDown _nudLoopCount;
        private NumericUpDown _nudTriggerDelay;
        private Label _lblCountdown;
        private Label _lblLoopStatus;
        private Button _btnAddKey;
        private Button _btnAddDelay;
        private Button _btnDelete;
        
        // List of action blocks
        private List<ActionBlock> _actionBlocks = new List<ActionBlock>();

        // Events
        public event EventHandler DeleteClicked;
        public event EventHandler RuleChanged;
        public event EventHandler LayoutChanged; // 触发外层重新排版

        // Colors
        private readonly Color _bgColor = Color.FromArgb(255, 255, 255);
        private readonly Color _borderColor = Color.FromArgb(225, 225, 230);
        private readonly Color _accentColor = Color.FromArgb(0, 120, 215);
        private readonly Color _textColor = Color.FromArgb(40, 40, 45);
        private readonly Color _dimTextColor = Color.FromArgb(120, 120, 125);
        private readonly Color _deleteColor = Color.FromArgb(200, 60, 60);
        private readonly Color _inputBgColor = Color.FromArgb(245, 245, 248);

        public RuleGroupPanel(KeyRule rule)
        {
            Rule = rule ?? new KeyRule();
            this.Width = 335;
            this.BackColor = _bgColor;
            this.Margin = new Padding(0, 0, 0, 2);
            this.Padding = new Padding(10, 5, 10, 5);
            
            InitializeTopBar();
            LoadActions();
            UpdateLayout();
            UpdateEnabledState();
        }

        private void InitializeTopBar()
        {
            // 启用 复选框
            _chkEnabled = new CheckBox
            {
                Text = "",
                Checked = Rule.Enabled,
                Location = new Point(8, 13),
                Size = new Size(18, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent
            };
            _chkEnabled.CheckedChanged += (s, e) =>
            {
                Rule.Enabled = _chkEnabled.Checked;
                UpdateEnabledState();
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_chkEnabled);

            // 循环次数
            var lblLoopLabel = new Label
            {
                Text = "循环(0无限):",
                Location = new Point(32, 12),
                AutoSize = true,
                ForeColor = _dimTextColor,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblLoopLabel);

            _nudLoopCount = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 999999,
                Value = Rule.LoopCount,
                Location = new Point(110, 9),
                Size = new Size(50, 24),
                BackColor = _inputBgColor,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _nudLoopCount.ValueChanged += (s, e) =>
            {
                Rule.LoopCount = (int)_nudLoopCount.Value;
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_nudLoopCount);

            // 已循环
            _lblLoopStatus = new Label
            {
                Text = "已循环: 0",
                Location = new Point(165, 12),
                Size = new Size(80, 18),
                ForeColor = _dimTextColor,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_lblLoopStatus);

            // 触发延迟
            var lblDelayLabel = new Label
            {
                Text = "触发延迟(s):",
                Location = new Point(32, 40),
                AutoSize = true,
                ForeColor = _dimTextColor,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblDelayLabel);

            _nudTriggerDelay = new NumericUpDown
            {
                Minimum = 0.0001m,
                Maximum = 86400m,
                DecimalPlaces = 4,
                Increment = 0.1m,
                Value = Math.Max(0.0001m, Math.Min(86400m, Rule.TriggerDelaySec)),
                Location = new Point(115, 37),
                Size = new Size(70, 24),
                BackColor = _inputBgColor,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _nudTriggerDelay.ValueChanged += (s, e) =>
            {
                Rule.TriggerDelaySec = _nudTriggerDelay.Value;
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_nudTriggerDelay);

            // 倒计时
            _lblCountdown = new Label
            {
                Text = "倒计时: --",
                Location = new Point(190, 40),
                Size = new Size(110, 18),
                ForeColor = Color.FromArgb(30, 150, 80),
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_lblCountdown);
            
            // 添加按键按钮
            _btnAddKey = new Button
            {
                Text = "＋ 追加按键",
                Location = new Point(32, 68),
                Size = new Size(95, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 245),
                ForeColor = _accentColor,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAddKey.FlatAppearance.BorderColor = _borderColor;
            _btnAddKey.FlatAppearance.BorderSize = 1;
            _btnAddKey.Click += (s, e) =>
            {
                var newAction = new ActionItem { Type = ActionType.KeyPress, KeyCode = Keys.None };
                Rule.Actions.Add(newAction);
                AddActionBlock(newAction);
                UpdateLayout();
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_btnAddKey);

            // 添加延时按钮
            _btnAddDelay = new Button
            {
                Text = "＋ 追加间隔",
                Location = new Point(135, 68),
                Size = new Size(95, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 245),
                ForeColor = Color.FromArgb(220, 130, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAddDelay.FlatAppearance.BorderColor = _borderColor;
            _btnAddDelay.FlatAppearance.BorderSize = 1;
            _btnAddDelay.Click += (s, e) =>
            {
                var newAction = new ActionItem { Type = ActionType.Delay, DelaySec = 1.0m };
                Rule.Actions.Add(newAction);
                AddActionBlock(newAction);
                UpdateLayout();
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_btnAddDelay);

            // 删除规则按钮
            _btnDelete = new Button
            {
                Text = "✕",
                Location = new Point(298, 6),
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = _deleteColor,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnDelete.FlatAppearance.BorderSize = 0;
            _btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 220, 40, 40);
            _btnDelete.Click += (s, e) => DeleteClicked?.Invoke(this, EventArgs.Empty);
            this.Controls.Add(_btnDelete);
        }

        private void LoadActions()
        {
            if (Rule.Actions.Count == 0)
            {
                Rule.Actions.Add(new ActionItem() { Type = ActionType.KeyPress, KeyCode = Keys.None }); // Default empty action
            }

            foreach (var action in Rule.Actions)
            {
                AddActionBlock(action);
            }
        }

        private void AddActionBlock(ActionItem action)
        {
            var block = new ActionBlock(action, _DimTextColor, _AccentColor, _TextColor, _BorderColor, _InputBgColor);
            block.ActionChanged += (s, e) => RuleChanged?.Invoke(this, EventArgs.Empty);
            block.RemoveClicked += (s, e) =>
            {
                Rule.Actions.Remove(action);
                this.Controls.Remove(block);
                _actionBlocks.Remove(block);
                block.Dispose();
                UpdateLayout();
                RuleChanged?.Invoke(this, EventArgs.Empty);
            };
            _actionBlocks.Add(block);
            this.Controls.Add(block);
        }
        
        // Expose some colors for ActionBlock
        private Color _DimTextColor => _dimTextColor;
        private Color _AccentColor => _accentColor;
        private Color _TextColor => _textColor;
        private Color _BorderColor => _borderColor;
        private Color _InputBgColor => _inputBgColor;

        private void UpdateLayout()
        {
            int y = 100; // Top bar takes about 100px now
            int i = 0;
            foreach (var block in _actionBlocks)
            {
                block.Location = new Point(20, y);
                block.UpdateIndex(i + 1);
                y += block.Height + 5;
                i++;
            }
            this.Height = y + 10;
            this.Invalidate();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制底部分隔线
            using (var pen = new Pen(_borderColor, 1))
            {
                e.Graphics.DrawLine(pen, 0, this.Height - 1, this.Width, this.Height - 1);
            }

            // 启用状态下左侧绘制高亮指示条
            if (Rule.Enabled)
            {
                using (var brush = new SolidBrush(_accentColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 5, 3, this.Height - 10);
                }
            }
        }

        private void UpdateEnabledState()
        {
            bool enabled = Rule.Enabled;
            _nudLoopCount.Enabled = enabled;
            _nudTriggerDelay.Enabled = enabled;
            _btnAddKey.Enabled = enabled;
            _btnAddDelay.Enabled = enabled;
            foreach (var block in _actionBlocks)
            {
                block.SetEnabled(enabled);
            }
            this.Invalidate();
        }

        public void UpdateCountdown(double remainingSeconds)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<double>(UpdateCountdown), remainingSeconds);
                return;
            }
            if (remainingSeconds < 0)
                _lblCountdown.Text = "倒计时: --";
            else
                _lblCountdown.Text = string.Format("倒计时: {0:F1}s", remainingSeconds);
        }

        public void UpdateLoopCount(int completed)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(UpdateLoopCount), completed);
                return;
            }
            Rule.CompletedLoops = completed;
            if (Rule.LoopCount > 0)
                _lblLoopStatus.Text = string.Format("已循环: {0}/{1}", completed, Rule.LoopCount);
            else
                _lblLoopStatus.Text = string.Format("已循环: {0}", completed);
        }

        public void ResetRunState()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(ResetRunState));
                return;
            }
            Rule.CompletedLoops = 0;
            _lblCountdown.Text = "倒计时: --";
            _lblLoopStatus.Text = "已循环: 0";
            SetActiveActionIndex(-1);
        }

        public void SetActiveActionIndex(int index)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(SetActiveActionIndex), index);
                return;
            }
            for (int i = 0; i < _actionBlocks.Count; i++)
            {
                _actionBlocks[i].SetIsActive(i == index);
            }
        }

        public void UpdateStepCountdown(int index, double remaining)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int, double>(UpdateStepCountdown), index, remaining);
                return;
            }
            for (int i = 0; i < _actionBlocks.Count; i++)
            {
                if (i == index && _actionBlocks[i].ActionType == ActionType.Delay)
                {
                    _actionBlocks[i].UpdateCountdownDisplay(remaining);
                }
                else
                {
                    _actionBlocks[i].UpdateCountdownDisplay(-1);
                }
            }
        }
    }

    /// <summary>
    /// 代表单行步骤配置的UI面板
    /// </summary>
    public class ActionBlock : Panel
    {
        private ActionItem _action;
        private Label _lblIndex;
        
        // For KeyPress
        private Label _lblKey;
        private Button _btnSelectKey;

        // For Delay
        private Label _lblTargetOrDelay; // Shows text depending on type
        private NumericUpDown _nudDelay;
        private Label _lblSec;
        private Label _lblCountdownWait; // To show active countdown

        private Button _btnRemove;

        private Color _dimText, _accent, _text, _border, _inputBg;

        public event EventHandler ActionChanged;
        public event EventHandler RemoveClicked;

        public ActionBlock(ActionItem action, Color dimText, Color accent, Color text, Color border, Color inputBg)
        {
            _action = action;
            _dimText = dimText;
            _accent = action.Type == ActionType.KeyPress ? accent : Color.FromArgb(220, 130, 0); // Orange for delay
            _text = text;
            _border = border;
            _inputBg = inputBg;

            this.Size = new Size(300, 32);
            this.BackColor = Color.FromArgb(250, 250, 252);
            
            InitializeControls();
        }

        private void InitializeControls()
        {
            // 序号
            _lblIndex = new Label
            {
                Text = "",
                Location = new Point(2, 8),
                Size = new Size(20, 16),
                ForeColor = _dimText,
                Font = new Font("Segoe UI", 7.5f),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(_lblIndex);

            if (_action.Type == ActionType.KeyPress)
            {
                // 按键显示
                _lblKey = new Label
                {
                    Text = _action.DisplayName,
                    Location = new Point(25, 4),
                    Size = new Size(65, 24),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = _inputBg,
                    ForeColor = _accent,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    BorderStyle = BorderStyle.None
                };
                this.Controls.Add(_lblKey);

                // 选择按键
                _btnSelectKey = new Button
                {
                    Text = "选按键",
                    Location = new Point(95, 4),
                    Size = new Size(55, 24),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(240, 240, 245),
                    ForeColor = _text,
                    Font = new Font("Segoe UI", 8.5f),
                    Cursor = Cursors.Hand
                };
                _btnSelectKey.FlatAppearance.BorderColor = _border;
                _btnSelectKey.FlatAppearance.BorderSize = 1;
                _btnSelectKey.Click += BtnSelectKey_Click;
                this.Controls.Add(_btnSelectKey);
            }
            else
            {
                // 间隔显示
                _lblTargetOrDelay = new Label
                {
                    Text = "⏳ 等待时长:",
                    Location = new Point(25, 8),
                    AutoSize = true,
                    ForeColor = _accent,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
                };
                this.Controls.Add(_lblTargetOrDelay);

                _nudDelay = new NumericUpDown
                {
                    Minimum = 0.0001m,
                    Maximum = 86400m,
                    DecimalPlaces = 4,
                    Increment = 0.1m,
                    Value = Math.Max(0.0001m, Math.Min(86400m, _action.DelaySec)),
                    Location = new Point(105, 5),
                    Size = new Size(65, 24),
                    BackColor = _inputBg,
                    ForeColor = _text,
                    Font = new Font("Segoe UI", 9f),
                    BorderStyle = BorderStyle.FixedSingle
                };
                _nudDelay.ValueChanged += (s, e) =>
                {
                    _action.DelaySec = _nudDelay.Value;
                    ActionChanged?.Invoke(this, EventArgs.Empty);
                };
                this.Controls.Add(_nudDelay);

                _lblSec = new Label
                {
                    Text = "s",
                    Location = new Point(175, 9),
                    AutoSize = true,
                    ForeColor = _dimText,
                    Font = new Font("Segoe UI", 8.5f)
                };
                this.Controls.Add(_lblSec);

                // Individual Step Countdown Wait
                _lblCountdownWait = new Label
                {
                    Text = "",
                    Location = new Point(205, 9),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(30, 150, 80),
                    Font = new Font("Consolas", 8.5f, FontStyle.Bold)
                };
                this.Controls.Add(_lblCountdownWait);
            }
            
            // 删除按键
            _btnRemove = new Button
            {
                Text = "－",
                Location = new Point(275, 4),
                Size = new Size(22, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(200, 60, 60),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnRemove.FlatAppearance.BorderSize = 0;
            _btnRemove.Click += (s, e) => RemoveClicked?.Invoke(this, EventArgs.Empty);
            this.Controls.Add(_btnRemove);
        }

        public void UpdateIndex(int index)
        {
            _lblIndex.Text = $"{index}.";
        }

        private void BtnSelectKey_Click(object sender, EventArgs e)
        {
            using (var kbForm = new KeyboardForm())
            {
                if (kbForm.ShowDialog(this) == DialogResult.OK && kbForm.SelectedKey != Keys.None)
                {
                    _action.KeyCode = kbForm.SelectedKey;
                    if (_lblKey != null)
                    {
                        _lblKey.Text = _action.DisplayName;
                        _lblKey.ForeColor = _action.KeyCode == Keys.None ? _dimText : _accent;
                    }
                    ActionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (_btnSelectKey != null) _btnSelectKey.Enabled = enabled;
            if (_nudDelay != null) _nudDelay.Enabled = enabled;
            _btnRemove.Enabled = enabled;
            if (_lblKey != null)
            {
                _lblKey.ForeColor = enabled
                    ? (_action.KeyCode == Keys.None ? _dimText : _accent)
                    : Color.FromArgb(180, 180, 185);
            }
            if (_lblTargetOrDelay != null)
            {
                _lblTargetOrDelay.ForeColor = enabled ? _accent : Color.FromArgb(180, 180, 185);
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(235, 235, 240), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        public void SetIsActive(bool active)
        {
            this.BackColor = active ? Color.FromArgb(220, 240, 255) : Color.FromArgb(250, 250, 252);
            this.Invalidate();
        }

        public void UpdateCountdownDisplay(double remaining)
        {
            if (_lblCountdownWait == null) return;
            if (remaining < 0)
                _lblCountdownWait.Text = "";
            else
                _lblCountdownWait.Text = string.Format("({0:F1}s)", remaining);
        }

        public ActionType ActionType => _action.Type;
    }
}
