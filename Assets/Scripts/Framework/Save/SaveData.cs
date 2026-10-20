using System;
using UnityEngine;
namespace Guisha.Framework
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public string scenePath = "";
        public int dungeon;
        public string checkpointId = "";
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public float health = 100, san = 100, stamina = 100;
        public string[] inventory = Array.Empty<string>();
        public string[] worldFlags = Array.Empty<string>();
        public bool hasPlayer;
        public GameMode gameMode = GameMode.Story;
        public float modeElapsed;
        public int survivalWave;
    }
}
