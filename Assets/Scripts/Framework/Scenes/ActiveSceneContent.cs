using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
namespace Guisha.Framework
{
    // Put on a scene root, with all playable content under a separate child.
    public sealed class ActiveSceneContent : MonoBehaviour
    {
        public GameObject content;
        private static readonly List<ActiveSceneContent> Roots = new List<ActiveSceneContent>();
        private void OnEnable()
        {
            if (Roots.Count == 0) SceneManager.activeSceneChanged += Changed;
            Roots.Add(this); Refresh();
        }
        private void OnDisable()
        {
            Roots.Remove(this);
            if (Roots.Count == 0) SceneManager.activeSceneChanged -= Changed;
        }
        private static void Changed(Scene before, Scene after)
        {
            var roots = Roots.ToArray();
            // Deactivate every old scene before registering panels/players in the next one.
            foreach (var root in roots) if (root != null && root.gameObject.scene != after) root.SetContent(false);
            foreach (var root in roots) if (root != null && root.gameObject.scene == after) root.SetContent(true);
        }
        private void SetContent(bool value) { if (content != null && content != gameObject) content.SetActive(value); }
        public void Refresh() { Changed(default, SceneManager.GetActiveScene()); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { SceneManager.activeSceneChanged -= Changed; Roots.Clear(); }
    }
}
