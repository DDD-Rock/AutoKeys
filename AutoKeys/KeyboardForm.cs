using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoKeys
{
    /// <summary>
    /// 虚拟键盘弹窗，仿照标准全键盘布局供用户选择按键
    /// </summary>
    public class KeyboardForm : Form
    {
        /// <summary>用户选中的按键</summary>
        public Keys SelectedKey { get; private set; } = Keys.None;

        private Button _selectedButton = null;
        private readonly Color _normalColor = Color.FromArgb(245, 245, 248);
        private readonly Color _selectedColor = Color.FromArgb(0, 120, 215);
        private readonly Color _hoverColor = Color.FromArgb(225, 225, 230);
        private readonly Color _textColor = Color.FromArgb(40, 40, 45);
        private readonly Font _keyFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        private readonly Font _keyFontSmall = new Font("Segoe UI", 7.5f, FontStyle.Regular);
        private readonly Font _keyFontTiny = new Font("Segoe UI", 6.5f, FontStyle.Regular);

        // 布局常量
        private const int KeyH = 36;        // 按键高度
        private const int KeyW = 42;        // 标准按键宽度
        private const int Gap = 3;          // 按键间距
        private const int SectionGap = 14;  // 区块间距
        private const int MarginLeft = 12;
        private const int MarginTop = 12;

        public KeyboardForm()
        {
            InitializeForm();
            BuildKeyboard();
        }

        private void InitializeForm()
        {
            this.Text = "选择按键";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(255, 255, 255);
            this.ShowInTaskbar = false;
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        }

        private void BuildKeyboard()
        {
            int x, y;

            // ==================== 第1行: 功能键行 ====================
            y = MarginTop;

            // Esc
            x = MarginLeft;
            AddKey("Esc", Keys.Escape, x, y, KeyW, KeyH);

            // F1-F4 (间隔一个键位)
            x = MarginLeft + KeyW + Gap + KeyW + Gap;
            AddKey("F1", Keys.F1, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F2", Keys.F2, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F3", Keys.F3, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F4", Keys.F4, x, y, KeyW, KeyH); x += KeyW + Gap;

            // F5-F8 (间隔半个键位)
            x += KeyW / 2;
            AddKey("F5", Keys.F5, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F6", Keys.F6, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F7", Keys.F7, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F8", Keys.F8, x, y, KeyW, KeyH); x += KeyW + Gap;

            // F9-F12 (间隔半个键位)
            x += KeyW / 2;
            AddKey("F9", Keys.F9, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F10", Keys.F10, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F11", Keys.F11, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("F12", Keys.F12, x, y, KeyW, KeyH);

            // 导航区上方: PrtSc, ScrLk, Pause
            int navX = MarginLeft + 14 * (KeyW + Gap) + KeyW + SectionGap;
            AddKey("PrtSc", Keys.PrintScreen, navX, y, KeyW, KeyH);
            AddKey("ScrLk", Keys.Scroll, navX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("Pause", Keys.Pause, navX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // ==================== 第2行: 数字键行 ====================
            y += KeyH + Gap + 8; // 功能键行和数字行之间留更大间距
            x = MarginLeft;

            // ` 1 2 3 4 5 6 7 8 9 0 - = Backspace
            string[] row1Labels = { "~\n`", "!\n1", "@\n2", "#\n3", "$\n4", "%\n5", "^\n6", "&\n7", "*\n8", "(\n9", ")\n0", "_\n-", "+\n=", "Backspace" };
            Keys[] row1Keys = { Keys.Oemtilde, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0, Keys.OemMinus, Keys.Oemplus, Keys.Back };
            for (int i = 0; i < row1Labels.Length; i++)
            {
                int w = (i == row1Labels.Length - 1) ? KeyW * 2 + Gap : KeyW;
                AddKey(row1Labels[i], row1Keys[i], x, y, w, KeyH);
                x += w + Gap;
            }

            // 导航区: Insert, Home, PageUp
            AddKey("Ins", Keys.Insert, navX, y, KeyW, KeyH);
            AddKey("Home", Keys.Home, navX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("PgUp", Keys.PageUp, navX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // 数字键盘: NumLock, /, *, -
            int numX = navX + 3 * (KeyW + Gap) + SectionGap;
            AddKey("Num", Keys.NumLock, numX, y, KeyW, KeyH);
            AddKey("/", Keys.Divide, numX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("*", Keys.Multiply, numX + 2 * (KeyW + Gap), y, KeyW, KeyH);
            AddKey("-", Keys.Subtract, numX + 3 * (KeyW + Gap), y, KeyW, KeyH);

            // ==================== 第3行: Tab行 ====================
            y += KeyH + Gap;
            x = MarginLeft;
            int tabW = (int)(KeyW * 1.5);
            AddKey("Tab", Keys.Tab, x, y, tabW, KeyH);
            x += tabW + Gap;

            string[] row2Letters = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
            Keys[] row2Keys = { Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.Y, Keys.U, Keys.I, Keys.O, Keys.P };
            for (int i = 0; i < row2Letters.Length; i++)
            {
                AddKey(row2Letters[i], row2Keys[i], x, y, KeyW, KeyH);
                x += KeyW + Gap;
            }
            AddKey("{\n[", Keys.OemOpenBrackets, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("}\n]", Keys.OemCloseBrackets, x, y, KeyW, KeyH); x += KeyW + Gap;
            int bsW = (int)(KeyW * 1.5) + Gap;
            AddKey("|\n\\", Keys.OemPipe, x, y, bsW, KeyH);

            // 导航区: Delete, End, PageDown
            AddKey("Del", Keys.Delete, navX, y, KeyW, KeyH);
            AddKey("End", Keys.End, navX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("PgDn", Keys.PageDown, navX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // 数字键盘: 7, 8, 9, +
            AddKey("7", Keys.NumPad7, numX, y, KeyW, KeyH);
            AddKey("8", Keys.NumPad8, numX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("9", Keys.NumPad9, numX + 2 * (KeyW + Gap), y, KeyW, KeyH);
            AddKey("+", Keys.Add, numX + 3 * (KeyW + Gap), y, KeyW, KeyH * 2 + Gap); // 占2行

            // ==================== 第4行: CapsLock行 ====================
            y += KeyH + Gap;
            x = MarginLeft;
            int capsW = (int)(KeyW * 1.75);
            AddKey("Caps\nLock", Keys.CapsLock, x, y, capsW, KeyH);
            x += capsW + Gap;

            string[] row3Letters = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
            Keys[] row3Keys = { Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.H, Keys.J, Keys.K, Keys.L };
            for (int i = 0; i < row3Letters.Length; i++)
            {
                AddKey(row3Letters[i], row3Keys[i], x, y, KeyW, KeyH);
                x += KeyW + Gap;
            }
            AddKey(":\n;", Keys.OemSemicolon, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("\"\n'", Keys.OemQuotes, x, y, KeyW, KeyH); x += KeyW + Gap;
            int enterW = MarginLeft + 13 * (KeyW + Gap) + KeyW * 2 + Gap - x;
            AddKey("Enter", Keys.Enter, x, y, enterW, KeyH);

            // 数字键盘: 4, 5, 6 (+ 已经占了)
            AddKey("4", Keys.NumPad4, numX, y, KeyW, KeyH);
            AddKey("5", Keys.NumPad5, numX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("6", Keys.NumPad6, numX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // ==================== 第5行: Shift行 ====================
            y += KeyH + Gap;
            x = MarginLeft;
            int lshiftW = (int)(KeyW * 2.25);
            AddKey("Shift", Keys.LShiftKey, x, y, lshiftW, KeyH);
            x += lshiftW + Gap;

            string[] row4Letters = { "Z", "X", "C", "V", "B", "N", "M" };
            Keys[] row4Keys = { Keys.Z, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M };
            for (int i = 0; i < row4Letters.Length; i++)
            {
                AddKey(row4Letters[i], row4Keys[i], x, y, KeyW, KeyH);
                x += KeyW + Gap;
            }
            AddKey("<\n,", Keys.Oemcomma, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey(">\n.", Keys.OemPeriod, x, y, KeyW, KeyH); x += KeyW + Gap;
            AddKey("?\n/", Keys.OemQuestion, x, y, KeyW, KeyH); x += KeyW + Gap;
            int rshiftEnd = MarginLeft + 13 * (KeyW + Gap) + KeyW * 2 + Gap;
            int rshiftW = rshiftEnd - x;
            AddKey("Shift", Keys.RShiftKey, x, y, rshiftW, KeyH);

            // 方向键上
            int arrowCenterX = navX + KeyW + Gap;
            AddKey("↑", Keys.Up, arrowCenterX, y, KeyW, KeyH);

            // 数字键盘: 1, 2, 3, Enter
            AddKey("1", Keys.NumPad1, numX, y, KeyW, KeyH);
            AddKey("2", Keys.NumPad2, numX + KeyW + Gap, y, KeyW, KeyH);
            AddKey("3", Keys.NumPad3, numX + 2 * (KeyW + Gap), y, KeyW, KeyH);
            AddKey("Enter", Keys.Return, numX + 3 * (KeyW + Gap), y, KeyW, KeyH * 2 + Gap); // 占2行

            // ==================== 第6行: Ctrl行 ====================
            y += KeyH + Gap;
            x = MarginLeft;
            int ctrlW = (int)(KeyW * 1.25);
            int winW = (int)(KeyW * 1.25);
            int altW = (int)(KeyW * 1.25);
            AddKey("Ctrl", Keys.LControlKey, x, y, ctrlW, KeyH); x += ctrlW + Gap;
            AddKey("Win", Keys.LWin, x, y, winW, KeyH); x += winW + Gap;
            AddKey("Alt", Keys.LMenu, x, y, altW, KeyH); x += altW + Gap;

            // Space
            int spaceEndX = MarginLeft + (int)(KeyW * 8.5);
            int spaceW = spaceEndX - x;
            AddKey("Space", Keys.Space, x, y, spaceW, KeyH);
            x = spaceEndX + Gap;

            AddKey("Alt", Keys.RMenu, x, y, altW, KeyH); x += altW + Gap;
            AddKey("Win", Keys.RWin, x, y, winW, KeyH); x += winW + Gap;
            AddKey("Menu", Keys.Apps, x, y, winW, KeyH); x += winW + Gap;
            int rctrlW = rshiftEnd - x;
            AddKey("Ctrl", Keys.RControlKey, x, y, rctrlW, KeyH);

            // 方向键: 左 下 右
            AddKey("←", Keys.Left, navX, y, KeyW, KeyH);
            AddKey("↓", Keys.Down, arrowCenterX, y, KeyW, KeyH);
            AddKey("→", Keys.Right, navX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // 数字键盘: 0 (双宽), Del
            AddKey("0", Keys.NumPad0, numX, y, KeyW * 2 + Gap, KeyH);
            AddKey("Del", Keys.Decimal, numX + 2 * (KeyW + Gap), y, KeyW, KeyH);

            // ==================== 确认/取消按钮 ====================
            y += KeyH + Gap + 10;
            int totalW = numX + 4 * (KeyW + Gap) - Gap - MarginLeft;
            int btnW = 100;

            var btnOk = new Button
            {
                Text = "✔ 确定",
                DialogResult = DialogResult.OK,
                Location = new Point(MarginLeft + totalW - btnW * 2 - Gap * 2, y),
                Size = new Size(btnW, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 240);
            this.Controls.Add(btnOk);

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(MarginLeft + totalW - btnW, y),
                Size = new Size(btnW, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 245),
                ForeColor = Color.FromArgb(40, 40, 45),
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 205);
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 225, 230);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            // 计算窗体大小
            int formW = numX + 4 * (KeyW + Gap) - Gap + MarginLeft;
            int formH = y + 36 + MarginTop;
            this.ClientSize = new Size(formW, formH);
        }

        private void AddKey(string label, Keys key, int x, int y, int width, int height)
        {
            var btn = new Button
            {
                Text = label,
                Tag = key,
                Location = new Point(x, y),
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = _normalColor,
                ForeColor = _textColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 根据文本长度选字体
            if (label.Contains("\n"))
                btn.Font = _keyFontTiny;
            else if (label.Length > 4)
                btn.Font = _keyFontSmall;
            else
                btn.Font = _keyFont;

            btn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = _hoverColor;

            btn.Click += (s, e) =>
            {
                // 取消上一个选中
                if (_selectedButton != null)
                {
                    _selectedButton.BackColor = _normalColor;
                    _selectedButton.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
                }

                // 选中当前
                _selectedButton = btn;
                btn.BackColor = _selectedColor;
                btn.FlatAppearance.BorderColor = _selectedColor;
                SelectedKey = key;
            };

            // 如果当前已选中此键，高亮显示
            if (key == SelectedKey && SelectedKey != Keys.None)
            {
                _selectedButton = btn;
                btn.BackColor = _selectedColor;
                btn.FlatAppearance.BorderColor = _selectedColor;
            }

            this.Controls.Add(btn);
        }
    }
}
