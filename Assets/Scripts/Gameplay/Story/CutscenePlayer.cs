using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.Playables;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class CutscenePlayer : MonoBehaviour
    {
        public GameObject cutsceneCamera;
        private PlayableDirector director;
        private bool playing;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void Awake() { director = GetComponent<PlayableDirector>(); }
        private void OnEnable()
        { director.stopped += Stopped; subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => { if (playing) director.Stop(); Release(); })); }
        private void OnDisable() { subscriptions.Dispose(); director.stopped -= Stopped; if (playing) director.Stop(); Release(); }
        public void Play()
        {
            if (playing || director.playableAsset == null) return;
            playing = true; EventBus.Publish(new InputLockChanged(this, true));
            if (cutsceneCamera != null) cutsceneCamera.SetActive(true); director.Play();
        }
        private void Stopped(PlayableDirector _) { Release(); }
        private void Release()
        { playing = false; if (cutsceneCamera != null) cutsceneCamera.SetActive(false); EventBus.Publish(new InputLockChanged(this, false)); }
    }
}
