using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Guisha.Core
{
    public static class AsyncHelper
    {
        public static Coroutine Delay(MonoBehaviour owner, float seconds, Action callback, bool unscaled = false)
            => owner.StartCoroutine(DelayRoutine(Mathf.Max(0, seconds), callback, unscaled));
        private static IEnumerator DelayRoutine(float seconds, Action callback, bool unscaled)
        {
            if (unscaled) yield return new WaitForSecondsRealtime(seconds);
            else yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
        public static async Task Await(AsyncOperation operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (operation.isDone) return;
            var completion = new TaskCompletionSource<bool>();
            operation.completed += _ => completion.TrySetResult(true);
            await completion.Task;
        }
        public static async Task DelayAsync(float seconds, CancellationToken token = default)
        {
            double end = Time.realtimeSinceStartupAsDouble + Math.Max(0, seconds);
            while (Time.realtimeSinceStartupAsDouble < end) { token.ThrowIfCancellationRequested(); await Task.Yield(); }
            token.ThrowIfCancellationRequested();
        }
        // Use at Unity void boundaries; all exceptions are observed.
        public static async void Forget(Task task)
        {
            try { await task; }
            catch (OperationCanceledException) { }
            catch (Exception error) { Debug.LogException(error); }
        }
    }
}
