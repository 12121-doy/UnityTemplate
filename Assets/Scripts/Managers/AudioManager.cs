using System.Collections;
using System.Collections.Generic;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Managers
{
    public sealed class AudioManager : MonoSingleton<AudioManager>
    {
        private sealed class Voice { public AudioSource Source; public AudioLowPassFilter Filter; public float Volume; }
        [SerializeField] private LayerMask occlusionMask = ~0;
        [SerializeField, Min(1)] private int maxVoices = 32;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 1;
        private ObjectPool<Voice> pool;
        private readonly List<Voice> active = new List<Voice>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private AudioSource[] bgm;
        private int currentBgm;
        private Coroutine fade;
        private AudioListener listener;
        private float nextOcclusion;
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            if (settings != null) { occlusionMask = settings.audioOcclusionMask; maxVoices = settings.audioMaxVoices; fadeSeconds = settings.bgmFadeSeconds; }
            pool = new ObjectPool<Voice>(() =>
            {
                var go = new GameObject("PooledVoice"); go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>(); source.playOnAwake = false;
                source.rolloffMode = AudioRolloffMode.Logarithmic; source.minDistance = 1; source.maxDistance = 25;
                return new Voice { Source = source, Filter = go.AddComponent<AudioLowPassFilter>() };
            }, v => v.Source.gameObject.SetActive(true), v => { v.Source.Stop(); v.Source.clip = null; v.Source.gameObject.SetActive(false); },
            v => { if (v.Source != null) Destroy(v.Source.gameObject); }, maxVoices);
            bgm = new[] { gameObject.AddComponent<AudioSource>(), gameObject.AddComponent<AudioSource>() };
            foreach (var source in bgm) { source.loop = true; source.playOnAwake = false; source.spatialBlend = 0; }
            subscriptions.Add(EventBus.Subscribe<PlaySoundRequested>(Play));
            subscriptions.Add(EventBus.Subscribe<BgmRequested>(e =>
            { if (fade != null) StopCoroutine(fade); fade = StartCoroutine(CrossFade(e.Clip)); }));
        }
        private void Play(PlaySoundRequested e)
        {
            if (e.Clip == null || active.Count >= maxVoices) return;
            var voice = pool.Get(); voice.Source.transform.position = e.Position;
            voice.Source.clip = e.Clip; voice.Source.spatialBlend = Mathf.Clamp01(e.SpatialBlend);
            voice.Volume = Mathf.Clamp01(e.Volume); voice.Source.volume = voice.Volume;
            voice.Source.pitch = 1; voice.Source.loop = false; voice.Filter.cutoffFrequency = 22000;
            voice.Source.Play(); active.Add(voice);
        }
        private void Update()
        {
            for (int i = active.Count - 1; i >= 0; i--)
                if (!active[i].Source.isPlaying) { pool.Return(active[i]); active.RemoveAt(i); }
            if (Time.unscaledTime < nextOcclusion) return;
            nextOcclusion = Time.unscaledTime + 0.15f;
            if (listener == null) listener = FindObjectOfType<AudioListener>();
            if (listener == null) return;
            foreach (var voice in active)
            {
                var delta = listener.transform.position - voice.Source.transform.position;
                bool blocked = Physics.Raycast(voice.Source.transform.position, delta.normalized, delta.magnitude, occlusionMask, QueryTriggerInteraction.Ignore);
                voice.Filter.cutoffFrequency = blocked ? 1500 : 22000;
                voice.Source.volume = voice.Volume * (blocked ? 0.45f : 1);
            }
        }
        private IEnumerator CrossFade(AudioClip clip)
        {
            var from = bgm[currentBgm]; var to = bgm[1 - currentBgm];
            float fromVolume = from.volume;
            to.Stop(); to.clip = clip; to.volume = 0; if (clip != null) to.Play();
            for (float time = 0; time < fadeSeconds; time += Time.unscaledDeltaTime)
            { float t = time / fadeSeconds; from.volume = fromVolume * (1 - t); to.volume = t; yield return null; }
            from.Stop(); to.volume = 1; currentBgm = 1 - currentBgm; fade = null;
        }
        protected override void OnDestroy()
        {
            subscriptions.Dispose();
            if (pool != null) { foreach (var voice in active) pool.Return(voice); active.Clear(); pool.Dispose(); }
            base.OnDestroy();
        }
    }
}
