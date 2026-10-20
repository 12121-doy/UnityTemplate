using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Guisha.Framework
{
    public sealed class EntitySpawnBuilder
    {
        private readonly EntityFactory factory;
        private EntityDefinition definition;
        private Vector3 position;
        private Quaternion rotation = Quaternion.identity;
        private Transform parent;
        public EntitySpawnBuilder(EntityFactory factory) { this.factory = factory ?? throw new ArgumentNullException(nameof(factory)); }
        public EntitySpawnBuilder Using(EntityDefinition definition) { this.definition = definition; return this; }
        public EntitySpawnBuilder At(Vector3 position, Quaternion rotation) { this.position = position; this.rotation = rotation; return this; }
        public EntitySpawnBuilder Under(Transform parent) { this.parent = parent; return this; }
        public Task<BaseEntity> SpawnAsync(CancellationToken token = default) => factory.SpawnAsync(definition, position, rotation, token, parent);
    }
}
