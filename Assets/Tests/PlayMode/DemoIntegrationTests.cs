using System.Collections;
using Guisha.Core;
using Guisha.Framework;
using Guisha.Gameplay;
using Guisha.Managers;
using Guisha.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Guisha.Validation
{
    public sealed class DemoIntegrationTests
    {
        private const string Main = "Assets/GuishaDemo/Scenes/Guisha.unity";
        private const string Dungeon = "Assets/GuishaDemo/Scenes/BloodVilla.unity";
        [UnityTest]
        public IEnumerator GreyboxLoadsRestoresStateAndSwitchesDungeon()
        {
            yield return SceneManager.LoadSceneAsync(Main, LoadSceneMode.Single);
            yield return null; yield return null; yield return null;
            Assert.That(GameManager.Exists && ResourceManager.Exists, Is.True);
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectOfType<HUD>(), Is.Not.Null);
            var player = Object.FindObjectOfType<PlayerEntity>();
            var status = player.GetComponent<PlayerStatus>();
            var inventory = player.GetComponent<Inventory>();
            status.ChangeSan(-30);
            Assert.That(inventory.TryAdd("ghost_artifact"), Is.True);
            var data = new SaveData(); EventBus.Publish(new SaveCapture(data));
            Assert.That(data.hasPlayer, Is.True);
            inventory.Use(0); Assert.That(inventory.Items.Count, Is.EqualTo(0));
            EventBus.Publish(new SaveLoaded(data));
            Assert.That(status.San, Is.EqualTo(data.san).Within(0.01f));
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            EventBus.Publish(new PanelRequested("Inventory", true));
            Assert.That(GameSession.InputEnabled, Is.False);
            EventBus.Publish(new PanelRequested("Inventory", false));
            Assert.That(GameSession.InputEnabled, Is.True);
            bool done = false, success = false;
            EventBus.Publish(new SceneTransitionRequested(Dungeon, 1, ok => { success = ok; done = true; }));
            float timeout = Time.realtimeSinceStartup + 20;
            while (!done && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.That(done && success, Is.True, "Dungeon transition timed out or failed");
            timeout = Time.realtimeSinceStartup + 20;
            while (Object.FindObjectOfType<GhostEntity>() == null && Time.realtimeSinceStartup < timeout) yield return null;
            yield return null; yield return null;
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1));
            var ghost = Object.FindObjectOfType<GhostEntity>();
            Assert.That(ghost, Is.Not.Null, "Addressables ghost not spawned");
            Assert.That(ghost.Definition.id, Is.EqualTo(EntityId.RedGhost));
            Assert.That(ghost.gameObject.scene.path, Is.EqualTo(Dungeon));
            Assert.That(ghost.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh, Is.True);
            done = false; success = false;
            EventBus.Publish(new SceneTransitionRequested(Main, 0, ok => { success = ok; done = true; }));
            timeout = Time.realtimeSinceStartup + 20;
            while (!done && Time.realtimeSinceStartup < timeout) yield return null;
            yield return null;
            Assert.That(done && success, Is.True);
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1));
            Assert.That(SceneManager.GetSceneByPath(Dungeon).isLoaded, Is.False);
            EventBus.Publish(new PanelRequested("Inventory", true));
            Assert.That(GameSession.InputEnabled, Is.False, "Main-scene UI did not re-register");
            EventBus.Publish(new PanelRequested("Inventory", false));
            Assert.That(GameSession.InputEnabled, Is.True);
        }
    }
}
