using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.Utils.Helpers;
using UnityEngine.EventSystems;
using UnityEngine;
using TootTallyCore.Graphics.Animations;

namespace TootTallyCore.Graphics
{
    public class BubblePopupHandler : MonoBehaviour
    {
        private GameObject _bubble;
        private EventTrigger _parentTrigger;
        private TootTallyAnimation _positionAnimation;
        private TootTallyAnimation _scaleAnimation;
        private bool _useWorldPosition;
        private bool _isActive;

        public void Initialize(GameObject bubble, bool useWorldPosition = true)
        {
            _bubble = bubble;
            _bubble.transform.SetParent(transform);
            _bubble.transform.position = transform.position;
            _useWorldPosition = useWorldPosition;
        }

        public void Awake()
        {
            var parent = transform.gameObject;
            if (!parent.TryGetComponent(out _parentTrigger))
                _parentTrigger = parent.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnter());

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerExit());

            _parentTrigger.triggers.Add(pointerEnterEvent);
            _parentTrigger.triggers.Add(pointerExitEvent);
        }

        public void Update()
        {

            if (_isActive)
            {
                var v3 = Input.mousePosition;
                v3.z = 10;
                if (_useWorldPosition)
                    _positionAnimation?.SetTargetVector(Camera.main.ScreenToWorldPoint(v3));
                else
                    _positionAnimation?.SetTargetVector(v3);
            }



            if (Input.GetMouseButtonDown(0) && _isActive)
                OnPointerExit();
        }

        private void OnPointerEnter()
        {
            if (_bubble == null) return;

            _isActive = true;
            _positionAnimation?.Dispose();
            _scaleAnimation?.Dispose();
            _bubble.transform.localScale = Vector2.zero;
            _bubble.SetActive(true);
            _bubble.transform.SetAsLastSibling();
            _positionAnimation = TootTallyAnimationManager.AddNewTransformPositionAnimation(_bubble, _useWorldPosition ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Input.mousePosition, 999f, GetSecondDegreeAnimation());
            _scaleAnimation = TootTallyAnimationManager.AddNewTransformScaleAnimation(_bubble, Vector3.one, 0.8f, GetSecondDegreeAnimation());
        }

        private void OnPointerExit()
        {
            if (_bubble == null) return;

            _isActive = false;
            _positionAnimation?.Dispose();
            _positionAnimation = null;
            _scaleAnimation?.Dispose();
            _scaleAnimation = TootTallyAnimationManager.AddNewTransformScaleAnimation(_bubble, Vector2.zero, 0.8f, GetSecondDegreeAnimationExit(), delegate
            {
                _bubble.SetActive(false);
            });

        }

        public SecondDegreeDynamicsAnimation GetSecondDegreeAnimation() => new(2.5f, .85f, 1f);
        public SecondDegreeDynamicsAnimation GetSecondDegreeAnimationExit() => new(2.65f, 1f, 1f);

    }
}
