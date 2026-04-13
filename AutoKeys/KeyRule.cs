using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoKeys
{
    public enum ActionType
    {
        KeyPress,
        Delay
    }

    /// <summary>
    /// 单个按键动作或延时模型
    /// </summary>
    public class ActionItem
    {
        public ActionType Type { get; set; } = ActionType.KeyPress;

        /// <summary>按键键值（如果是按键类型）</summary>
        public Keys KeyCode { get; set; } = Keys.None;

        /// <summary>延时时间（如果是延时类型，单位：秒）</summary>
        public decimal DelaySec { get; set; } = 1.0000m;

        /// <summary>
        /// 获取动作的显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (Type == ActionType.Delay) return $"等待 {DelaySec} 秒";
                if (KeyCode == Keys.None) return "(未设置)";
                return KeyCode.ToString();
            }
        }
    }

    /// <summary>
    /// 按键规则数据模型
    /// </summary>
    public class KeyRule
    {
        /// <summary>整体触发延迟（每次循环开始前的倒计时等待时间）</summary>
        public decimal TriggerDelaySec { get; set; } = 1.0000m;

        /// <summary>按键与延时动作列表</summary>
        public List<ActionItem> Actions { get; set; } = new List<ActionItem>();

        /// <summary>是否启用</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>循环次数，0表示无限循环</summary>
        public int LoopCount { get; set; } = 0;

        /// <summary>已完成循环次数（运行时状态，不序列化）</summary>
        public int CompletedLoops { get; set; } = 0;
    }
}
