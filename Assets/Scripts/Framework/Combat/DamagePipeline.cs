using Guisha.Core.Patterns;
using UnityEngine;
namespace Guisha.Framework
{
    // Optional local component. No global mutable chain or registration order dependency.
    public sealed class DamagePipeline : MonoBehaviour
    {
        public bool invulnerable;
        [Min(0)] public float flatReduction;
        [Min(0)] public float multiplier = 1;
        private sealed class Context { public float Amount; }
        private RuleChain<Context> chain;
        private void Awake()
        {
            chain = new RuleChain<Context>(
                new DelegateRule<Context>(c => !invulnerable && !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount) && c.Amount > 0),
                new DelegateRule<Context>(c => { c.Amount = Mathf.Max(0, c.Amount - flatReduction); return c.Amount > 0; }),
                new DelegateRule<Context>(c => { c.Amount *= multiplier; return !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount) && c.Amount > 0; }));
        }
        public float Resolve(float amount)
        { if (chain == null) Awake(); var context = new Context { Amount = amount }; return chain.Apply(context) ? context.Amount : 0; }
    }
}
