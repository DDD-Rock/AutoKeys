namespace AutoKeys
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI 控件
        private System.Windows.Forms.ComboBox _cboWindows;
        private System.Windows.Forms.Button _btnRefresh;
        private System.Windows.Forms.Button _btnAddRule;
        private System.Windows.Forms.Panel _panelRules;
        private System.Windows.Forms.Button _btnStartStop;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.Label _lblStartHotkey;
        private System.Windows.Forms.Button _btnSetStartHotkey;
        private System.Windows.Forms.Label _lblStopHotkey;
        private System.Windows.Forms.Button _btnSetStopHotkey;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            var bgColor = System.Drawing.Color.FromArgb(245, 245, 248);
            var panelColor = System.Drawing.Color.FromArgb(255, 255, 255);
            var borderColor = System.Drawing.Color.FromArgb(210, 210, 215);
            var textColor = System.Drawing.Color.FromArgb(40, 40, 45);
            var dimTextColor = System.Drawing.Color.FromArgb(120, 120, 130);
            var accentColor = System.Drawing.Color.FromArgb(0, 120, 215);
            var inputBgColor = System.Drawing.Color.FromArgb(255, 255, 255);
            var btnBgColor = System.Drawing.Color.FromArgb(240, 240, 245);

            // ====== 第1行：窗口选择 ======
            var lblWindow = new System.Windows.Forms.Label();
            lblWindow.Text = "目标窗口:";
            lblWindow.Location = new System.Drawing.Point(10, 14);
            lblWindow.Size = new System.Drawing.Size(65, 20);
            lblWindow.AutoSize = true;
            lblWindow.ForeColor = dimTextColor;
            lblWindow.Font = new System.Drawing.Font("Segoe UI", 9f);
            lblWindow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            _cboWindows = new System.Windows.Forms.ComboBox();
            _cboWindows.Location = new System.Drawing.Point(75, 11);
            _cboWindows.Size = new System.Drawing.Size(220, 26);
            _cboWindows.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _cboWindows.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cboWindows.BackColor = inputBgColor;
            _cboWindows.ForeColor = textColor;
            _cboWindows.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            _cboWindows.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            _btnRefresh = new System.Windows.Forms.Button();
            _btnRefresh.Text = "⟳ 刷新";
            _btnRefresh.Location = new System.Drawing.Point(300, 10);
            _btnRefresh.Size = new System.Drawing.Size(70, 28);
            _btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnRefresh.BackColor = btnBgColor;
            _btnRefresh.ForeColor = textColor;
            _btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9f);
            _btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnRefresh.FlatAppearance.BorderColor = borderColor;
            _btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(225, 225, 230);
            _btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);

            // ====== 第2行：添加规则按钮 ======
            _btnAddRule = new System.Windows.Forms.Button();
            _btnAddRule.Text = "＋ 添加规则";
            _btnAddRule.Location = new System.Drawing.Point(10, 45);
            _btnAddRule.Size = new System.Drawing.Size(360, 32);
            _btnAddRule.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _btnAddRule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnAddRule.BackColor = accentColor;
            _btnAddRule.ForeColor = System.Drawing.Color.White;
            _btnAddRule.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            _btnAddRule.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnAddRule.FlatAppearance.BorderSize = 0;
            _btnAddRule.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(0, 140, 240);
            _btnAddRule.Click += new System.EventHandler(this.BtnAddRule_Click);

            // 分隔线
            var lblSeparator1 = new System.Windows.Forms.Label();
            lblSeparator1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lblSeparator1.BackColor = borderColor;
            lblSeparator1.Location = new System.Drawing.Point(10, 84);
            lblSeparator1.Size = new System.Drawing.Size(360, 1);
            lblSeparator1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // ====== 中间：规则列表滚动面板 ======
            _panelRules = new System.Windows.Forms.Panel();
            _panelRules.Location = new System.Drawing.Point(10, 85);
            _panelRules.Size = new System.Drawing.Size(360, 400);
            _panelRules.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _panelRules.BackColor = panelColor;
            _panelRules.AutoScroll = true;
            _panelRules.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // 分隔线 2
            var lblSeparator2 = new System.Windows.Forms.Label();
            lblSeparator2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lblSeparator2.BackColor = borderColor;
            lblSeparator2.Location = new System.Drawing.Point(10, 490);
            lblSeparator2.Size = new System.Drawing.Size(360, 1);
            lblSeparator2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // ====== 快捷键设置行 ======
            _lblStartHotkey = new System.Windows.Forms.Label();
            _lblStartHotkey.Text = "开始快捷键:";
            _lblStartHotkey.Location = new System.Drawing.Point(10, 502);
            _lblStartHotkey.AutoSize = true;
            _lblStartHotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _lblStartHotkey.ForeColor = dimTextColor;
            _lblStartHotkey.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            _lblStartHotkey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            _btnSetStartHotkey = new System.Windows.Forms.Button();
            _btnSetStartHotkey.Text = "Ctrl+F1";
            _btnSetStartHotkey.Location = new System.Drawing.Point(85, 500);
            _btnSetStartHotkey.Size = new System.Drawing.Size(100, 28);
            _btnSetStartHotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _btnSetStartHotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnSetStartHotkey.BackColor = btnBgColor;
            _btnSetStartHotkey.ForeColor = textColor;
            _btnSetStartHotkey.Font = new System.Drawing.Font("Segoe UI", 9f);
            _btnSetStartHotkey.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnSetStartHotkey.FlatAppearance.BorderColor = borderColor;
            _btnSetStartHotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(225, 225, 230);
            _btnSetStartHotkey.Click += new System.EventHandler(this.BtnSetStartHotkey_Click);

            _lblStopHotkey = new System.Windows.Forms.Label();
            _lblStopHotkey.Text = "停止快捷键:";
            _lblStopHotkey.Location = new System.Drawing.Point(195, 502);
            _lblStopHotkey.AutoSize = true;
            _lblStopHotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _lblStopHotkey.ForeColor = dimTextColor;
            _lblStopHotkey.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            _lblStopHotkey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            _btnSetStopHotkey = new System.Windows.Forms.Button();
            _btnSetStopHotkey.Text = "Ctrl+F2";
            _btnSetStopHotkey.Location = new System.Drawing.Point(270, 500);
            _btnSetStopHotkey.Size = new System.Drawing.Size(100, 28);
            _btnSetStopHotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _btnSetStopHotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnSetStopHotkey.BackColor = btnBgColor;
            _btnSetStopHotkey.ForeColor = textColor;
            _btnSetStopHotkey.Font = new System.Drawing.Font("Segoe UI", 9f);
            _btnSetStopHotkey.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnSetStopHotkey.FlatAppearance.BorderColor = borderColor;
            _btnSetStopHotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(225, 225, 230);
            _btnSetStopHotkey.Click += new System.EventHandler(this.BtnSetStopHotkey_Click);

            // ====== 底部：开始/停止按钮 ======
            _btnStartStop = new System.Windows.Forms.Button();
            _btnStartStop.Text = "▶ 开始";
            _btnStartStop.Location = new System.Drawing.Point(10, 540);
            _btnStartStop.Size = new System.Drawing.Size(360, 40);
            _btnStartStop.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnStartStop.BackColor = accentColor;
            _btnStartStop.ForeColor = System.Drawing.Color.White;
            _btnStartStop.Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            _btnStartStop.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnStartStop.FlatAppearance.BorderSize = 0;
            _btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(0, 140, 240);
            _btnStartStop.Click += new System.EventHandler(this.BtnStartStop_Click);

            // ====== 状态栏 ======
            _statusStrip = new System.Windows.Forms.StatusStrip();
            _statusStrip.BackColor = System.Drawing.Color.FromArgb(240, 240, 245);
            _statusStrip.SizingGrip = false;

            _statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _statusLabel.Text = "○ 已停止";
            _statusLabel.ForeColor = dimTextColor;
            _statusLabel.Font = new System.Drawing.Font("Segoe UI", 9f);
            _statusStrip.Items.Add(_statusLabel);

            // ====== 主窗口设置 ======
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(380, 610);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "兔子按键精灵";
            this.BackColor = bgColor;
            this.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.KeyPreview = true;

            // 添加所有控件
            this.Controls.Add(lblWindow);
            this.Controls.Add(_cboWindows);
            this.Controls.Add(_btnRefresh);
            this.Controls.Add(_btnAddRule);
            this.Controls.Add(lblSeparator1);
            this.Controls.Add(_panelRules);
            this.Controls.Add(lblSeparator2);
            this.Controls.Add(_lblStartHotkey);
            this.Controls.Add(_btnSetStartHotkey);
            this.Controls.Add(_lblStopHotkey);
            this.Controls.Add(_btnSetStopHotkey);
            this.Controls.Add(_btnStartStop);
            this.Controls.Add(_statusStrip);
        }

        #endregion
    }
}
