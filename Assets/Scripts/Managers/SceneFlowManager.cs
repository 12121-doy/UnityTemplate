using System;
using System.Threading.Tasks;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Guisha.Managers
{
    public sealed class SceneFlowManager : MonoSingleton<SceneFlowManager>
    {
        [Tooltip("留空时保留首次载入的场景作为诡舍主场景。")]
        [SerializeField] private string persistentScenePath;
        private string currentDungeonScene;
        private bool busy;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            if (settings != null) persistentScenePath = settings.persistentScenePath;
            subscriptions.Add(EventBus.Subscribe<SceneTransitionRequested>(e => AsyncHelper.Forget(Transition(e))));
        }
        private async Task Transition(SceneTransitionRequested request)
        {
            if (busy) { request.Completed?.Invoke(false); return; }
            if (string.IsNullOrWhiteSpace(request.ScenePath)) { request.Completed?.Invoke(false); return; }
            busy = true; bool success = false;
            if (string.IsNullOrEmpty(persistentScenePath)) persistentScenePath = SceneManager.GetActiveScene().path;
            EventBus.Publish(new SceneTransitionStarted());
            Scene loaded = default; bool added = false;
            var previous = SceneManager.GetActiveScene();
            try
            {
                loaded = SceneManager.GetSceneByPath(request.ScenePath);
                if (!loaded.isLoaded)
                {
                    await AsyncHelper.Await(SceneManager.LoadSceneAsync(request.ScenePath, LoadSceneMode.Additive));
                    loaded = SceneManager.GetSceneByPath(request.ScenePath); added = true;
                }
                if (!loaded.IsValid() || !loaded.isLoaded || !SceneManager.SetActiveScene(loaded))
                    throw new InvalidOperationException("Scene did not become active. Use full Assets/...unity paths in Build Settings.");
                // Wait one frame for new scene Start subscriptions before save restoration.
                await Task.Yield();
                if (!string.IsNullOrEmpty(currentDungeonScene) && currentDungeonScene != request.ScenePath && currentDungeonScene != persistentScenePath)
                {
                    var old = SceneManager.GetSceneByPath(currentDungeonScene);
                    if (old.isLoaded) await AsyncHelper.Await(SceneManager.UnloadSceneAsync(old));
                }
                currentDungeonScene = request.ScenePath == persistentScenePath ? null : request.ScenePath;
                success = true;
            }
            catch (Exception error)
            {
                Guisha.Core.Logger.Error(error.ToString());
                if (previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (added && loaded.isLoaded) await AsyncHelper.Await(SceneManager.UnloadSceneAsync(loaded));
            }
            finally
            {
                busy = false;
                EventBus.Publish(new SceneTransitionFinished(request.ScenePath, request.Dungeon, success));
                request.Completed?.Invoke(success);
            }
        }
        protected override void OnDestroy() { subscriptions.Dispose(); base.OnDestroy(); }
    }
}
