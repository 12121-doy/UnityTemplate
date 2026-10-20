using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Guisha.Core;
using UnityEngine;
namespace Guisha.Framework
{
    public sealed class EntityFactory
    {
        private readonly IResourceProvider resources;
        private readonly Dictionary<EntityId, EntityDefinition> catalog = new Dictionary<EntityId, EntityDefinition>();
        public EntityFactory(IResourceProvider resources, EntityCatalog definitions = null)
        {
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
            if (definitions == null) return;
            foreach (var entry in definitions.definitions)
            {
                if (entry == null) continue;
                if (catalog.ContainsKey(entry.id)) throw new ArgumentException("Duplicate entity ID: " + entry.id);
                catalog.Add(entry.id, entry);
            }
        }
        public Task<BaseEntity> SpawnAsync(EntityId id, Vector3 position, Quaternion rotation,
            CancellationToken token = default, Transform parent = null)
        {
            if (!catalog.TryGetValue(id, out var definition)) throw new KeyNotFoundException("Entity not registered: " + id);
            return SpawnAsync(definition, position, rotation, token, parent);
        }
        public async Task<BaseEntity> SpawnAsync(EntityDefinition definition, Vector3 position, Quaternion rotation,
            CancellationToken token = default, Transform parent = null)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.prefabAddress))
                throw new ArgumentException("Entity definition requires an Addressables address.");
            bool hasParent = parent != null;
            var lease = await resources.InstantiateAsync(definition.prefabAddress, position, rotation, parent, token);
            if (hasParent && parent == null) { lease.Dispose(); throw new OperationCanceledException("Spawn owner scene was unloaded."); }
            var entity = lease.GameObject.GetComponent<BaseEntity>();
            if (entity == null || entity.Type != definition.type)
            { lease.Dispose(); throw new InvalidOperationException("Entity prefab missing BaseEntity or has a mismatched type."); }
            entity.Configure(definition);
            EventBus.Publish(new EntitySpawned(entity));
            return entity;
        }
    }
}
