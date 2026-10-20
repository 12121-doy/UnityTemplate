using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public Camera view;
        public LayerMask interactionMask = ~0;
        [SerializeField] private float range = 5;
        private IInteractable current;
        private string prompt;
        private void Update()
        {
            current = null;
            if (view == null) view = Camera.main;
            if (view != null && GameSession.InputEnabled)
            {
                var ray = view.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                var hits = Physics.RaycastAll(ray, range, interactionMask, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var hit in hits)
                {
                    if (hit.transform.IsChildOf(transform)) continue;
                    current = hit.collider.GetComponentInParent<IInteractable>();
                    if (current != null && !current.CanInteract(gameObject)) current = null;
                    break; // Walls block interaction with objects behind them.
                }
            }
            string next = current != null ? "[E] " + current.Prompt : "";
            if (prompt != next) { prompt = next; EventBus.Publish(new InteractionPromptChanged(prompt)); }
            if (current != null && Input.GetKeyDown(KeyCode.E))
            { current.Interact(gameObject); EventBus.Publish(new EntityAnimationRequested(GetComponent<BaseEntity>(), EntityAnimationState.Interact)); }
        }
        private void OnDisable() { EventBus.Publish(new InteractionPromptChanged("")); }
    }
}
