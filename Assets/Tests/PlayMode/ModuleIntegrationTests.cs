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
    public sealed class ModuleIntegrationTests
    {
        [UnityTest] public IEnumerator MonoSingletonKeepsPrimaryAndClearsOnDestroy()
        {
            var go=new GameObject("SingletonPrimary"); var primary=go.AddComponent<SingletonTestManager>();
            Assert.That(SingletonTestManager.TryGetInstance(out var found),Is.True); Assert.That(found,Is.SameAs(primary));
            var duplicate=new GameObject("SingletonDuplicate"); duplicate.AddComponent<SingletonTestManager>(); yield return null;
            Assert.That(duplicate==null,Is.True); Assert.That(SingletonTestManager.RequireInstance(),Is.SameAs(primary));
            var scene=SceneManager.CreateScene("SingletonSceneProbe"); SceneManager.SetActiveScene(scene);
            Assert.That(primary.gameObject.scene.name,Is.EqualTo("DontDestroyOnLoad"));
            Object.Destroy(go); yield return null; Assert.That(SingletonTestManager.Exists,Is.False);
        }
        [UnityTest] public IEnumerator ModesAnimationAndSurvivalUseRealGameplay()
        {
            yield return SceneManager.LoadSceneAsync("Assets/GuishaDemo/Scenes/Guisha.unity",LoadSceneMode.Single);
            yield return null; yield return null;
            EventBus.Publish(new GameModeRequested(GameMode.Story));
            var player=Object.FindObjectOfType<PlayerEntity>(); var animation=player.GetComponent<EntityAnimator>();
            Assert.That(animation,Is.Not.Null); Assert.That(animation.Driver.Available,Is.True);
            animation.Driver.Float("Speed",2); Assert.That(animation.targetAnimator.GetFloat("Speed"),Is.EqualTo(2));
            EventBus.Publish(new EntityAnimationRequested(player,EntityAnimationState.Interact));
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(animation.targetAnimator.GetCurrentAnimatorStateInfo(0).IsName("Interact") || animation.targetAnimator.GetNextAnimatorStateInfo(0).IsName("Interact"),Is.True);
            EventBus.Publish(new GameModeRequested(GameMode.Exploration));
            float health=player.Health; player.TakeDamage(new DamageInfo(100,null,player.transform.position)); Assert.That(player.Health,Is.EqualTo(health));
            var san=player.GetComponent<PlayerStatus>(); float sanity=san.San; yield return new WaitForSecondsRealtime(0.3f); Assert.That(san.San,Is.EqualTo(sanity));
            EventBus.Publish(new PanelRequested("Modes",true)); Assert.That(GameSession.InputEnabled,Is.False); EventBus.Publish(new PanelRequested("Modes",false));
            bool done=false; EventBus.Publish(new SceneTransitionRequested("Assets/GuishaDemo/Scenes/BloodVilla.unity",1,_=>done=true));
            float timeout=Time.realtimeSinceStartup+20; while(!done&&Time.realtimeSinceStartup<timeout)yield return null; Assert.That(done,Is.True);
            EventBus.Publish(new GameModeRequested(GameMode.Endless)); timeout=Time.realtimeSinceStartup+20;
            while((GameModeRuntime.Wave<1||Object.FindObjectsOfType<GhostEntity>().Length<2)&&Time.realtimeSinceStartup<timeout)yield return null;
            Assert.That(GameModeRuntime.Wave,Is.GreaterThanOrEqualTo(1)); Assert.That(Object.FindObjectsOfType<GhostEntity>().Length,Is.GreaterThanOrEqualTo(2));
            EventBus.Publish(new GameModeRequested(GameMode.Story)); yield return null; yield return null;
            Assert.That(Object.FindObjectsOfType<GhostEntity>().Length,Is.LessThanOrEqualTo(1));
            var captured=new SaveData(); EventBus.Publish(new SaveCapture(captured)); Assert.That(captured.gameMode,Is.EqualTo(GameMode.Story));
            EventBus.Publish(new GameModeRequested(GameMode.Exploration)); EventBus.Publish(new SaveLoaded(captured)); Assert.That(GameModeRuntime.Rules.Mode,Is.EqualTo(GameMode.Story));
            EventBus.Publish(new GameModeRequested(GameMode.Challenge));
            var mechanism=Object.FindObjectOfType<Mechanism>(); WorldState.Set(mechanism.requiredClueId);
            mechanism.Interact(Object.FindObjectOfType<PlayerEntity>().gameObject);
            Assert.That(GameModeRuntime.Rules.Mode,Is.EqualTo(GameMode.Story),"Challenge should finish at the configured mechanism");
            var activePlayer=Object.FindObjectOfType<PlayerEntity>(); var saved=new SaveData(); EventBus.Publish(new SaveCapture(saved));
            activePlayer.SetHealth(1); activePlayer.TakeDamage(new DamageInfo(100,null,activePlayer.transform.position));
            Assert.That(GameSession.State,Is.EqualTo(GameState.GameOver));
            yield return new WaitForSecondsRealtime(0.4f);
            var activeAnimation=activePlayer.GetComponent<EntityAnimator>();
            Assert.That(activeAnimation.targetAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dead") || activeAnimation.targetAnimator.GetNextAnimatorStateInfo(0).IsName("Dead"),Is.True,"Death animation should advance while gameplay is frozen");
            Time.timeScale=1; GameSession.SetState(GameState.Playing); EventBus.Publish(new SaveLoaded(saved));
            Assert.That(activeAnimation.targetAnimator.updateMode,Is.EqualTo(AnimatorUpdateMode.Normal));
        }
    }
}
