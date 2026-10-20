using System;
using Guisha.Core;
using UnityEngine;
namespace Guisha.Framework
{
    public static class GameModeRuntime
    {
        public static GameModeRules Rules { get; private set; } = GameModeRules.Default(GameMode.Story);
        public static float Elapsed { get; private set; }
        public static int Wave { get; private set; }
        public static float Remaining => Mathf.Max(0, Rules.TimeLimit - Elapsed);
        public static float EnemySpeed => Rules.EnemySpeed * (Rules.Mode == GameMode.Endless ? 1 + Mathf.Min(0.75f, Wave * 0.05f) : 1);
        public static float EnemyDamage => Rules.Mode == GameMode.Endless ? 1 + Mathf.Min(2, Wave * 0.1f) : 1;
        public static string Label(GameMode mode)
        { switch (mode) { case GameMode.Exploration: return "自由探索"; case GameMode.Challenge: return "限时挑战"; case GameMode.Endless: return "无尽生存"; default: return "剧情模式"; } }
        public static void Apply(GameModeRules rules)
        { Rules = rules; Elapsed = 0; Wave = 0; EventBus.Publish(new GameModeChanged(rules)); }
        public static void SetProgress(float elapsed, int wave)
        { Elapsed = float.IsNaN(elapsed) || float.IsInfinity(elapsed) ? 0 : Mathf.Max(0, elapsed); Wave = Math.Max(0, wave); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Rules = GameModeRules.Default(GameMode.Story); Elapsed = 0; Wave = 0; }
    }
}
