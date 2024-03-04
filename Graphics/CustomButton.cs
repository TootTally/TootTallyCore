using UnityEngine;
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
        }

        public void RemoveAllOnClickActions() => button.onClick.RemoveAllListeners();

    }
}
