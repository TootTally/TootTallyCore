using TootTallyCore.Utils.SoundEffects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TootTallyCore.Graphics
{
    public class CustomButton : MonoBehaviour
    {
        public Button button;
        public Text textHolder;

        public void ConstructNewButton(Button button, Text textHolder)
        {
            this.button = button;
            this.textHolder = textHolder;
            textHolder.supportRichText = true;
            textHolder.maskable = true;
            if (textHolder.TryGetComponent(out LocalizeStringEvent locEvent))
                GameObject.DestroyImmediate(locEvent);
            this.name = name;
            RemoveAllOnClickActions();
            var eventTrigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener(data => SoundEffectsManager.PlayerBtnHover());
            eventTrigger.triggers.Add(pointerEnterEvent);
        }

        public void RemoveAllOnClickActions() => button.onClick.RemoveAllListeners();

    }
}
