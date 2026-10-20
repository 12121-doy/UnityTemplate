using UnityEngine;
namespace Guisha.Framework
{
    [CreateAssetMenu(menuName = "Guisha/Framework Settings")]
    public sealed class FrameworkSettings : ScriptableObject
    {
        public LayerMask audioOcclusionMask = ~0;
        [Min(1)] public int audioMaxVoices = 32;
        [Min(0.1f)] public float bgmFadeSeconds = 1;
        public bool enableLogs = true;
        public bool writeLogFile;
        public string persistentScenePath;
        public EntityCatalog entityCatalog;
        public GameMode defaultGameMode = GameMode.Story;
        public GameModeCatalog gameModes;
    }
}
