using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace AutoKeys
{
    /// <summary>
    /// 按键执行引擎，管理多组规则的独立倒计时与按键发送
    /// </summary>
    public class KeySender : IDisposable
    {
        private readonly List<RuleTimer> _timers = new List<RuleTimer>();
        private readonly Random _random = new Random();
        private IntPtr _targetWindowHandle;
        private bool _isRunning = false;
        private System.Threading.Timer _watchdogTimer;

        /// <summary>当目标窗口关闭时触发</summary>
        public event EventHandler TargetWindowClosed;

        /// <summary>当所有规则完成时触发</summary>
        public event EventHandler AllRulesCompleted;

        /// <summary>是否正在运行</summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 单组规则的计时器
        /// </summary>
        private class RuleTimer
        {
            public KeyRule Rule { get; set; }
            public RuleGroupPanel Panel { get; set; }
            public System.Threading.Timer Timer { get; set; }
            public DateTime NextFireTime { get; set; }
            public System.Threading.Timer CountdownTimer { get; set; }
            public int CompletedLoops { get; set; }
            public int CurrentActionIndex { get; set; }
            public bool IsWaitingTriggerDelay { get; set; }
            public bool IsCompleted { get; set; }
        }

        /// <summary>
        /// 启动所有规则的按键循环
        /// </summary>
        public void Start(IntPtr targetWindowHandle, List<RuleGroupPanel> panels)
        {
            if (_isRunning) return;

            _targetWindowHandle = targetWindowHandle;
            _isRunning = true;

            // 清理旧的计时器
            Stop();
            _isRunning = true;

            foreach (var panel in panels)
            {
                if (!panel.Rule.Enabled || panel.Rule.Actions == null || panel.Rule.Actions.Count == 0)
                    continue;

                panel.ResetRunState();

                var ruleTimer = new RuleTimer
                {
                    Rule = panel.Rule,
                    Panel = panel,
                    CompletedLoops = 0,
                    CurrentActionIndex = 0,
                    IsWaitingTriggerDelay = false,
                    IsCompleted = false
                };

                // 第1次执行：启动时不等待整体触发延迟，立刻顺流执行动作，仅留防冲突偏移量
                int initialDelayMs = 50 + (panels.IndexOf(panel) * 50);
                ruleTimer.NextFireTime = DateTime.Now.AddMilliseconds(initialDelayMs);

                // 创建按键发送 Timer
                ruleTimer.Timer = new System.Threading.Timer(
                    state => OnTimerTick((RuleTimer)state),
                    ruleTimer,
                    initialDelayMs,
                    Timeout.Infinite // 一次性，此后的延迟由 OnTimerTick 自行调度
                );

                // 创建倒计时更新 Timer (100ms 刷新)
                ruleTimer.CountdownTimer = new System.Threading.Timer(
                    state =>
                    {
                        var rt = (RuleTimer)state;
                        if (rt.IsCompleted)
                        {
                            rt.Panel.UpdateCountdown(-1);
                            rt.Panel.UpdateStepCountdown(-1, 0);
                            return;
                        }
                        
                        double remaining = Math.Max(0, (rt.NextFireTime - DateTime.Now).TotalSeconds);
                        if (rt.IsWaitingTriggerDelay)
                        {
                            rt.Panel.UpdateCountdown(remaining);
                            rt.Panel.UpdateStepCountdown(-1, 0);
                        }
                        else
                        {
                            rt.Panel.UpdateCountdown(0);
                            int activeActionForWait = rt.CurrentActionIndex - 1;
                            if (activeActionForWait >= 0 && activeActionForWait < rt.Rule.Actions.Count)
                            {
                                if (rt.Rule.Actions[activeActionForWait].Type == ActionType.Delay)
                                {
                                    rt.Panel.UpdateStepCountdown(activeActionForWait, remaining);
                                }
                                else
                                {
                                    rt.Panel.UpdateStepCountdown(-1, 0);
                                }
                            }
                        }
                    },
                    ruleTimer,
                    0,
                    100
                );

                _timers.Add(ruleTimer);
            }

            // 启动目标窗口存活检测 (500ms)
            _watchdogTimer = new System.Threading.Timer(
                state => CheckTargetWindow(),
                null, 500, 500
            );
        }

        /// <summary>
        /// 停止所有规则
        /// </summary>
        public void Stop()
        {
            _isRunning = false;

            foreach (var rt in _timers)
            {
                rt.Timer?.Dispose();
                rt.CountdownTimer?.Dispose();
                rt.Panel.UpdateCountdown(-1);
                rt.Panel.UpdateStepCountdown(-1, 0);
            }
            _timers.Clear();

            _watchdogTimer?.Dispose();
            _watchdogTimer = null;
        }

        /// <summary>
        /// Timer 触发：执行序列动作
        /// </summary>
        private void OnTimerTick(RuleTimer rt)
        {
            if (!_isRunning || rt.IsCompleted) return;

            try
            {
                // 检查目标窗口
                if (!NativeHelper.IsWindow(_targetWindowHandle))
                {
                    TargetWindowClosed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                if (rt.IsWaitingTriggerDelay)
                {
                    rt.IsWaitingTriggerDelay = false;
                }
                else if (rt.CurrentActionIndex >= rt.Rule.Actions.Count)
                {
                    // 序列已经彻底执行完毕，这里是结算点
                    rt.CompletedLoops++;
                    rt.Panel.UpdateLoopCount(rt.CompletedLoops);

                    // 检查是否达到指定循环次数
                    if (rt.Rule.LoopCount > 0 && rt.CompletedLoops >= rt.Rule.LoopCount)
                    {
                        rt.IsCompleted = true;
                        rt.Panel.UpdateCountdown(-1);
                        rt.Panel.UpdateStepCountdown(-1, 0);
                        rt.Panel.SetActiveActionIndex(-1);
                        CheckAllCompleted();
                        return;
                    }
                    
                    // 重置下一轮的整体触发延迟
                    rt.CurrentActionIndex = 0;
                    rt.IsWaitingTriggerDelay = true;
                    rt.Panel.SetActiveActionIndex(-1);
                    int triggerWait = (int)(rt.Rule.TriggerDelaySec * 1000m);
                    rt.NextFireTime = DateTime.Now.AddMilliseconds(triggerWait);
                    rt.Timer.Change(triggerWait, Timeout.Infinite);
                    return;
                }

                // 正在顺次执行内部动作
                int processIndex = rt.CurrentActionIndex;
                rt.Panel.SetActiveActionIndex(processIndex);
                var currentAction = rt.Rule.Actions[processIndex];
                decimal appliedDelay = 0.05m; // 默认极小等待间隙，以便顺次触发

                if (currentAction.Type == ActionType.KeyPress)
                {
                    if (currentAction.KeyCode != Keys.None)
                    {
                        // 激活目标窗口
                        NativeHelper.ForceSetForegroundWindow(_targetWindowHandle);
                        Thread.Sleep(30); // 短暂等待窗口获取焦点

                        // 拟人化按键：按下
                        NativeHelper.SendKeyDown(currentAction.KeyCode);

                        // 拟人化按键按住时间 50~150ms
                        int holdTime = _random.Next(50, 151);
                        Thread.Sleep(holdTime);

                        // 拟人化按键：抬起
                        NativeHelper.SendKeyUp(currentAction.KeyCode);
                    }
                }
                else if (currentAction.Type == ActionType.Delay)
                {
                    appliedDelay = currentAction.DelaySec;
                }

                // 推进指针（下次进 TICK 如果越界，就是结算）
                rt.CurrentActionIndex++;

                // 本次动作做完之后往后等待的延迟
                int nextDelay = currentAction.Type == ActionType.Delay 
                    ? GetHumanizedDelay((int)(appliedDelay * 1000m))
                    : (int)(appliedDelay * 1000m);

                rt.NextFireTime = DateTime.Now.AddMilliseconds(nextDelay);
                rt.Timer.Change(nextDelay, Timeout.Infinite);
            }
            catch (Exception)
            {
                // 出错时尝试继续运行
                if (_isRunning && !rt.IsCompleted)
                {
                    int nextDelay = 1000; // 发生异常后退避1秒重试
                    rt.NextFireTime = DateTime.Now.AddMilliseconds(nextDelay);
                    try { rt.Timer.Change(nextDelay, Timeout.Infinite); } catch { }
                }
            }
        }

        /// <summary>
        /// 检查目标窗口是否存活
        /// </summary>
        private void CheckTargetWindow()
        {
            if (!_isRunning) return;
            if (!NativeHelper.IsWindow(_targetWindowHandle))
            {
                TargetWindowClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 检查是否所有规则都已完成
        /// </summary>
        private void CheckAllCompleted()
        {
            foreach (var rt in _timers)
            {
                if (!rt.IsCompleted) return;
            }
            AllRulesCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 生成拟人化延迟（在设定值基础上 ±10% 随机波动）
        /// </summary>
        private int GetHumanizedDelay(int baseDelay)
        {
            double factor = 0.9 + _random.NextDouble() * 0.2; // 0.9 ~ 1.1
            return Math.Max(50, (int)(baseDelay * factor));
        }

        public void Dispose()
        {
            Stop();
        }
    }
}


