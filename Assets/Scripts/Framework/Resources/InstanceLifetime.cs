using System;
using UnityEngine;
namespace Guisha.Framework
{
    public sealed class InstanceLifetime : MonoBehaviour
    {
        private IDisposable lease;
        public void Bind(IDisposable value) { lease = value; }
        private void OnDestroy() { lease?.Dispose(); lease = null; }
    }
}
