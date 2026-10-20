using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Guisha.Managers
{
    public sealed class ResourceManager : MonoSingleton<ResourceManager>, IResourceProvider
    {
        private sealed class Entry { public AsyncOperationHandle Handle; public int Users; }
        private readonly Dictionary<string, Entry> cache = new Dictionary<string, Entry>();
        private readonly HashSet<ResourceInstance> instances = new HashSet<ResourceInstance>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private EntityFactory factory;
        private bool disposed;
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            factory = new EntityFactory(this, settings != null ? settings.entityCatalog : null);
            subscriptions.Add(EventBus.Subscribe<SpawnEntityRequested>(e => AsyncHelper.Forget(Spawn(e))));
            subscriptions.Add(EventBus.Subscribe<SpawnEntityByIdRequested>(e => AsyncHelper.Forget(factory.SpawnAsync(e.Id, e.Position, e.Rotation, lifetime.Token, e.Parent))));
        }
        private async Task Spawn(SpawnEntityRequested request)
        {
            BaseEntity result = null;
            try { result = await factory.SpawnAsync(request.Definition, request.Position, request.Rotation, lifetime.Token, request.Parent); }
            finally { request.Completed?.Invoke(result); }
        }
        private static async Task Complete(AsyncOperationHandle handle)
        {
            // Completed callback works without AsyncOperationHandle.Task, including WebGL.
            if (handle.IsDone) return;
            var completion = new TaskCompletionSource<bool>();
            handle.Completed += _ => completion.TrySetResult(true);
            await completion.Task;
        }
        public async Task<AssetLease<T>> LoadAsync<T>(string address, CancellationToken token = default) where T : UnityEngine.Object
        {
            if (disposed) throw new ObjectDisposedException(nameof(ResourceManager));
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Empty address.", nameof(address));
            string key = typeof(T).FullName + ":" + address;
            if (!cache.TryGetValue(key, out var entry))
            { entry = new Entry { Handle = Addressables.LoadAssetAsync<T>(address) }; cache.Add(key, entry); }
            entry.Users++;
            void Release()
            {
                entry.Users--;
                if (entry.Users == 0 && cache.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
                { cache.Remove(key); if (entry.Handle.IsValid()) Addressables.Release(entry.Handle); }
            }
            try
            {
                await Complete(entry.Handle);
                token.ThrowIfCancellationRequested();
                if (disposed) throw new ObjectDisposedException(nameof(ResourceManager));
                if (entry.Handle.Status != AsyncOperationStatus.Succeeded)
                    throw entry.Handle.OperationException ?? new InvalidOperationException("Addressables load failed: " + address);
                return new AssetLease<T>((T)entry.Handle.Result, Release);
            }
            catch { Release(); throw; }
        }
        public async Task<ResourceInstance> InstantiateAsync(string address, Vector3 position, Quaternion rotation,
            Transform parent = null, CancellationToken token = default)
        {
            if (disposed) throw new ObjectDisposedException(nameof(ResourceManager));
            token.ThrowIfCancellationRequested();
            var handle = Addressables.InstantiateAsync(address, position, rotation, parent, true);
            try
            {
                await Complete(handle);
                token.ThrowIfCancellationRequested();
                if (disposed) throw new ObjectDisposedException(nameof(ResourceManager));
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw handle.OperationException ?? new InvalidOperationException("Instantiate failed: " + address);
                ResourceInstance lease = null;
                lease = new ResourceInstance(handle.Result, () =>
                { instances.Remove(lease); if (handle.IsValid()) Addressables.ReleaseInstance(handle); });
                instances.Add(lease);
                handle.Result.AddComponent<InstanceLifetime>().Bind(lease);
                return lease;
            }
            catch
            {
                if (handle.IsValid())
                { if (handle.Status == AsyncOperationStatus.Succeeded) Addressables.ReleaseInstance(handle); else Addressables.Release(handle); }
                throw;
            }
        }
        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                disposed = true; lifetime.Cancel(); subscriptions.Dispose();
                foreach (var instance in instances.ToArray()) instance.Dispose();
                // In-flight loads own their handles until completion; existing leases release naturally.
            }
            lifetime.Dispose(); base.OnDestroy();
        }
    }
}
