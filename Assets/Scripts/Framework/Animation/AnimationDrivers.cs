using System;
using System.Collections.Generic;
using Guisha.Core.Patterns;
using UnityEngine;
namespace Guisha.Framework
{
    public interface IAnimationDriver
    {
        bool Available { get; }
        void Float(string parameter, float value);
        void Bool(string parameter, bool value);
        void Trigger(string parameter);
        void Play(string state, float fade = 0.15f);
    }
    public sealed class NullAnimationDriver : IAnimationDriver
    {
        public static readonly NullAnimationDriver Instance = new NullAnimationDriver();
        private NullAnimationDriver() { }
        public bool Available => false;
        public void Float(string parameter, float value) { }
        public void Bool(string parameter, bool value) { }
        public void Trigger(string parameter) { }
        public void Play(string state, float fade = 0.15f) { }
    }
    // Shared flyweight: integers have no Unity object ownership or disposal requirements.
    public static class AnimationHashes
    {
        private static readonly FlyweightCache<string, int> Hashes = new FlyweightCache<string, int>(Animator.StringToHash);
        public static int Get(string name) => Hashes.Get(name);
    }
    public sealed class AnimatorDriver : IAnimationDriver
    {
        private readonly Animator animator;
        private RuntimeAnimatorController controller;
        private readonly Dictionary<int, AnimatorControllerParameterType> parameters = new Dictionary<int, AnimatorControllerParameterType>();
        public AnimatorDriver(Animator animator) { this.animator = animator; Refresh(); }
        public bool Available => animator != null && animator.runtimeAnimatorController != null && animator.isActiveAndEnabled;
        private void Refresh()
        {
            parameters.Clear(); controller = animator != null ? animator.runtimeAnimatorController : null;
            if (controller == null) return;
            foreach (var parameter in animator.parameters) parameters[parameter.nameHash] = parameter.type;
        }
        private bool Supports(string name, AnimatorControllerParameterType type, out int hash)
        {
            hash = 0; if (!Available || string.IsNullOrEmpty(name)) return false;
            if (controller != animator.runtimeAnimatorController) Refresh();
            hash = AnimationHashes.Get(name); return parameters.TryGetValue(hash, out var found) && found == type;
        }
        public void Float(string parameter, float value) { if (Supports(parameter, AnimatorControllerParameterType.Float, out var hash)) animator.SetFloat(hash, value); }
        public void Bool(string parameter, bool value) { if (Supports(parameter, AnimatorControllerParameterType.Bool, out var hash)) animator.SetBool(hash, value); }
        public void Trigger(string parameter) { if (Supports(parameter, AnimatorControllerParameterType.Trigger, out var hash)) animator.SetTrigger(hash); }
        public void Play(string state, float fade = 0.15f)
        {
            if (!Available || string.IsNullOrEmpty(state)) return;
            int hash = AnimationHashes.Get(state);
            if (animator.HasState(0, hash)) animator.CrossFadeInFixedTime(hash, Mathf.Max(0, fade));
        }
    }
    public sealed class LegacyAnimationDriver : IAnimationDriver
    {
        private readonly Animation animation;
        public LegacyAnimationDriver(Animation animation) { this.animation = animation; }
        public bool Available => animation != null && animation.isActiveAndEnabled;
        public void Float(string parameter, float value) { }
        public void Bool(string parameter, bool value) { }
        public void Trigger(string parameter) { Play(parameter); }
        public void Play(string state, float fade = 0.15f)
        { if (Available && !string.IsNullOrEmpty(state) && animation[state] != null) animation.CrossFade(state, Mathf.Max(0, fade)); }
    }
    public abstract class AnimationDriverFactory : Factory<IAnimationDriver> { }
    public sealed class AnimatorDriverFactory : AnimationDriverFactory
    {
        private readonly Animator animator;
        public AnimatorDriverFactory(Animator animator) { this.animator = animator; }
        protected override IAnimationDriver CreateProduct() => animator != null ? (IAnimationDriver)new AnimatorDriver(animator) : NullAnimationDriver.Instance;
    }
    public sealed class LegacyAnimationDriverFactory : AnimationDriverFactory
    {
        private readonly Animation animation;
        public LegacyAnimationDriverFactory(Animation animation) { this.animation = animation; }
        protected override IAnimationDriver CreateProduct() => animation != null ? (IAnimationDriver)new LegacyAnimationDriver(animation) : NullAnimationDriver.Instance;
    }
    public sealed class AnimationLoggingDecorator : IAnimationDriver
    {
        private readonly IAnimationDriver inner;
        private readonly Action<string> log;
        public AnimationLoggingDecorator(IAnimationDriver inner, Action<string> log)
        { this.inner = inner ?? throw new ArgumentNullException(nameof(inner)); this.log = log ?? throw new ArgumentNullException(nameof(log)); }
        public bool Available => inner.Available;
        public void Float(string parameter, float value) => inner.Float(parameter, value);
        public void Bool(string parameter, bool value) => inner.Bool(parameter, value);
        public void Trigger(string parameter) { log("Trigger: " + parameter); inner.Trigger(parameter); }
        public void Play(string state, float fade = 0.15f) { log("Play: " + state); inner.Play(state, fade); }
    }
}
