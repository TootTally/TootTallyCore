using TootTallyCore.Graphics.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TootTallyCore.Graphics
{
    public class SlideTooltip
    {
        private GameObject _hitboxGameObject;
        private TootTallyAnimation _enterAnimation, _exitAnimation;

        public GameObject tooltipGameObject;
        private Vector2 _startPosition, _targetPosition;


        public SlideTooltip(GameObject hitboxGameObject, GameObject tooltipGameObject, Vector2 startPosition, Vector2 targetPosition)
        {
            _hitboxGameObject = hitboxGameObject;

            this.tooltipGameObject = tooltipGameObject;
            CanvasGroup canvasGroup = tooltipGameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            _startPosition = startPosition;
            _targetPosition = targetPosition;

            EventTrigger tooltipHitboxEvents = _hitboxGameObject.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnterHitbox());
            tooltipHitboxEvents.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerExitHitbox());
            tooltipHitboxEvents.triggers.Add(pointerExitEvent);
        }


        private void OnPointerEnterHitbox()
        {
            if (_exitAnimation != null)
                TootTallyAnimationManager.RemoveFromList(_exitAnimation);
            _exitAnimation = null;
            _enterAnimation = TootTallyAnimationManager.AddNewPositionAnimation(tooltipGameObject, _targetPosition, 1.5f, new SecondDegreeDynamicsAnimation(1.75f, 1f, 0f));
        }
        private void OnPointerExitHitbox()
        {
            if (_enterAnimation != null)
                TootTallyAnimationManager.RemoveFromList(_enterAnimation);
            _enterAnimation = null;
            _exitAnimation = TootTallyAnimationManager.AddNewPositionAnimation(tooltipGameObject, _startPosition, 1.2f, new SecondDegreeDynamicsAnimation(1.75f, 1f, 0f));
        }
    }
}
