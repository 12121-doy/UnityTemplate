using UnityEngine;
namespace Guisha.Framework
{
    public enum GameMode { Story, Exploration, Challenge, Endless }
    [CreateAssetMenu(menuName = "Guisha/Game Mode Definition")]
    public sealed class GameModeDefinition : ScriptableObject
    {
        public GameMode mode;
        [Min(0)] public float playerDamage = 1, sanDrain = 1, staminaCost = 1, enemySpeed = 1;
        public bool hostileGhosts = true, allowSave = true;
        [Min(0)] public float timeLimit;
        [Tooltip("留空时，副本内成功启动任意机关即完成限时挑战。")]
        public string objectiveMechanismId;
    }
    public readonly struct GameModeRules
    {
        public readonly GameMode Mode;
        public readonly float PlayerDamage, SanDrain, StaminaCost, EnemySpeed, TimeLimit;
        public readonly bool HostileGhosts, AllowSave;
        public GameModeRules(GameMode mode, float damage, float san, float stamina, float speed, bool hostile, bool save, float limit = 0)
        { Mode = mode; PlayerDamage = Mathf.Max(0, damage); SanDrain = Mathf.Max(0, san); StaminaCost = Mathf.Max(0, stamina); EnemySpeed = Mathf.Max(0, speed); HostileGhosts = hostile; AllowSave = save; TimeLimit = Mathf.Max(0, limit); }
        public static GameModeRules Default(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Exploration: return new GameModeRules(mode, 0, 0, 0, 1, false, true);
                case GameMode.Challenge: return new GameModeRules(mode, 1.25f, 1.5f, 1.2f, 1.15f, true, false, 180);
                case GameMode.Endless: return new GameModeRules(mode, 1, 1, 1, 1, true, false);
                default: return new GameModeRules(GameMode.Story, 0.75f, 0.7f, 0.8f, 0.9f, true, true);
            }
        }
    }
    public readonly struct GameModeRequested { public readonly GameMode Mode; public GameModeRequested(GameMode mode) { Mode = mode; } }
    public readonly struct GameModeChanged { public readonly GameModeRules Rules; public GameModeChanged(GameModeRules rules) { Rules = rules; } }
    public readonly struct GameModeProgress
    {
        public readonly float Elapsed, Remaining; public readonly int Wave;
        public GameModeProgress(float elapsed, float remaining, int wave) { Elapsed = elapsed; Remaining = remaining; Wave = wave; }
    }
    public readonly struct SurvivalWaveStarted { public readonly int Wave; public SurvivalWaveStarted(int wave) { Wave = wave; } }
    public readonly struct ModeRoundEnded { public readonly string Reason; public ModeRoundEnded(string reason) { Reason = reason; } }
    public readonly struct ChallengeCompleted
    {
        public readonly float Seconds;
        public readonly string MechanismId;
        public ChallengeCompleted(float seconds, string mechanismId) { Seconds = seconds; MechanismId = mechanismId; }
    }
}
