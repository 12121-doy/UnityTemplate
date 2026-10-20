using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    [CreateAssetMenu(menuName = "Guisha/Items/Ghost Artifact")]
    public sealed class GhostArtifactItem : Item
    {
        [Min(0)] public float sanRestore = 25;
        public override bool Use(GameObject actor)
        {
            var status = actor.GetComponent<PlayerStatus>(); if (status == null) return false;
            status.ChangeSan(sanRestore); EventBus.Publish(new Notification("使用了 " + displayName)); return true;
        }
    }
}
