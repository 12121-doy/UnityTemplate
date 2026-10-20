using UnityEngine;
namespace Guisha.Framework
{
    [CreateAssetMenu(menuName = "Guisha/Entity Definition")]
    public sealed class EntityDefinition : ScriptableObject
    {
        public EntityId id;
        public EntityType type;
        public string prefabAddress;
        [Min(1)] public float maxHealth = 100;
        [Min(0)] public float moveSpeed = 3;
        [Min(0)] public float attackDamage = 10;
    }
}
