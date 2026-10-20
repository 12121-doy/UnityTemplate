using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Managers
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            if (settings != null) { Guisha.Core.Logger.Enabled = settings.enableLogs; Guisha.Core.Logger.WriteFile = settings.writeLogFile; }
            Ensure<ResourceManager>(); Ensure<AudioManager>(); Ensure<UIManager>();
            Ensure<SceneFlowManager>(); Ensure<GameModeManager>(); Ensure<GameManager>();
        }
        private static void Ensure<T>() where T : MonoSingleton<T>
        { if (!MonoSingleton<T>.Exists) new GameObject(typeof(T).Name).AddComponent<T>(); }
    }
}
