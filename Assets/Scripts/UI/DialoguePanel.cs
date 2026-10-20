using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace Guisha.UI
{
    public sealed class DialoguePanel : UIPanelBase
    {
        public Text speaker, body, choiceText;
        private int choices;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        protected override void OnEnable()
        {
            base.OnEnable();
            subscriptions.Add(EventBus.Subscribe<DialogueLineChanged>(e =>
            {
                if (speaker != null) speaker.text = e.Node.speaker;
                if (body != null) body.text = e.Node.text;
                choices = e.Node.choices?.Length ?? 0;
                if (choiceText == null) return;
                choiceText.text = choices == 0 ? "[空格] 继续" : "";
                for (int i = 0; i < choices; i++) choiceText.text += $"[{i + 1}] {e.Node.choices[i].text}\n";
            }));
        }
        private void Update()
        {
            if (!IsOpen || GameSession.State != GameState.Playing) return;
            if (choices == 0 && Input.GetKeyDown(KeyCode.Space)) Choose(0);
            for (int i = 0; i < Mathf.Min(choices, 9); i++) if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) Choose(i);
        }
        public void Choose(int index) { EventBus.Publish(new DialogueChoiceRequested(index)); }
        protected override void OnDisable() { subscriptions.Dispose(); base.OnDisable(); }
    }
}
