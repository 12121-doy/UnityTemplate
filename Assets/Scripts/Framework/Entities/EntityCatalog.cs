using System;
using UnityEngine;
namespace Guisha.Framework
{
    [CreateAssetMenu(menuName = "Guisha/Entity Catalog")]
    public sealed class EntityCatalog : ScriptableObject
    {
        public EntityDefinition[] definitions = Array.Empty<EntityDefinition>();
    }
}
