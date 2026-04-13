using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace AutoKeys
{
    /// <summary>
    /// 配置管理器，负责规则和快捷键设置的持久化
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        /// <summary>
        /// 配置数据结构
        /// </summary>
        public class AppConfig
        {
            public List<KeyRuleData> Rules { get; set; } = new List<KeyRuleData>();
            public string StartHotkey { get; set; } = "Control+F1";
            public string StopHotkey { get; set; } = "Control+F2";
        }

        public class ActionData
        {
            public int Type { get; set; } = 0; // 0 = KeyPress, 1 = Delay
            public string KeyCode { get; set; } = "None";
            public decimal DelaySec { get; set; } = 1.0000m;
        }

        /// <summary>
        /// 可序列化的规则数据
        /// </summary>
        public class KeyRuleData
        {
            public decimal TriggerDelaySec { get; set; } = 1.0000m;
            public List<ActionData> Actions { get; set; } = new List<ActionData>();
            public bool Enabled { get; set; } = true;
            public int LoopCount { get; set; } = 0;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save(List<KeyRule> rules, string startHotkey, string stopHotkey)
        {
            var config = new AppConfig
            {
                StartHotkey = startHotkey,
                StopHotkey = stopHotkey,
                Rules = new List<KeyRuleData>()
            };

            foreach (var rule in rules)
            {
                var ruleData = new KeyRuleData
                {
                    TriggerDelaySec = rule.TriggerDelaySec,
                    Enabled = rule.Enabled,
                    LoopCount = rule.LoopCount
                };
                foreach (var action in rule.Actions)
                {
                    ruleData.Actions.Add(new ActionData
                    {
                        Type = (int)action.Type,
                        KeyCode = action.KeyCode.ToString(),
                        DelaySec = action.DelaySec
                    });
                }
                config.Rules.Add(ruleData);
            }

            string json = Serializer.Serialize(config);
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var config = Serializer.Deserialize<AppConfig>(json);
                return config ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        /// <summary>
        /// 将配置数据转换为 KeyRule 列表
        /// </summary>
        public static List<KeyRule> ToKeyRules(AppConfig config)
        {
            var rules = new List<KeyRule>();
            if (config.Rules == null) return rules;

            foreach (var data in config.Rules)
            {
                var rule = new KeyRule
                {
                    TriggerDelaySec = data.TriggerDelaySec,
                    Enabled = data.Enabled,
                    LoopCount = data.LoopCount
                };
                
                if (data.Actions != null)
                {
                    foreach (var ad in data.Actions)
                    {
                        var action = new ActionItem 
                        { 
                            Type = (ActionType)ad.Type,
                            DelaySec = ad.DelaySec 
                        };
                        if (System.Enum.TryParse(ad.KeyCode, out System.Windows.Forms.Keys result))
                            action.KeyCode = result;
                        else
                            action.KeyCode = System.Windows.Forms.Keys.None;
                        rule.Actions.Add(action);
                    }
                }
                rules.Add(rule);
            }
            return rules;
        }
    }
}
