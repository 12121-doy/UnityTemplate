using UnityEngine;
namespace Guisha.Gameplay
{
    public abstract class Item : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public bool consumable = true;
        public abstract bool Use(GameObject actor);
    }
}
