using System;
using UnityEngine;
namespace Guisha.Framework
{
    public enum EntityType { Player, Human, Ghost }
    public enum EntityId { NingQiuShui, LiangYan, BaiXiaoXiao, LiuChengFeng, TianXun, Survivor, RedGhost, RuanShi, SkinGhost, HeadlessMonk }
    public enum EntityLifeState { Alive, Dead }
    public interface IDamageable { bool IsAlive { get; } void TakeDamage(DamageInfo damage); }
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly GameObject Source;
        public readonly Vector3 Point;
        public DamageInfo(float amount, GameObject source, Vector3 point) { Amount = amount; Source = source; Point = point; }
    }
}
