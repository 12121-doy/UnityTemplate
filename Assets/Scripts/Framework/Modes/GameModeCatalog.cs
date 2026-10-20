using System;
using UnityEngine;
namespace Guisha.Framework
{
    [CreateAssetMenu(menuName = "Guisha/Game Mode Catalog")]
    public sealed class GameModeCatalog : ScriptableObject
    {
        public GameModeDefinition[] definitions = Array.Empty<GameModeDefinition>();
        public GameModeRules Resolve(GameMode mode)
        {
            foreach (var value in definitions)
                if (value != null && value.mode == mode)
                    return new GameModeRules(mode, value.playerDamage, value.sanDrain, value.staminaCost, value.enemySpeed, value.hostileGhosts, value.allowSave, value.timeLimit);
            return GameModeRules.Default(mode);
        }
    }
}
