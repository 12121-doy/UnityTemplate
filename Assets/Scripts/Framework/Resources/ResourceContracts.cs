using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Guisha.Framework
{
    public sealed class AssetLease<T> : IDisposable where T : UnityEngine.Object
    {
        private Action release;
        public T Asset { get; private set; }
        public AssetLease(T asset, Action release) { Asset = asset; this.release = release; }
        public void Dispose() { var action = release; release = null; Asset = null; action?.Invoke(); }
    }
    public sealed class ResourceInstance : IDisposable
    {
        private Action release;
        public GameObject GameObject { get; private set; }
        public ResourceInstance(GameObject instance, Action release) { GameObject = instance; this.release = release; }
        public void Dispose() { var action = release; release = null; GameObject = null; action?.Invoke(); }
    }
    public interface IResourceProvider
    {
        Task<AssetLease<T>> LoadAsync<T>(string address, CancellationToken token = default) where T : UnityEngine.Object;
        Task<ResourceInstance> InstantiateAsync(string address, Vector3 position, Quaternion rotation,
            Transform parent = null, CancellationToken token = default);
    }
}
