using System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace TootTallyCore.Graphics.Animations
{
    public class TootTallyAnimation
    {
        private GameObject _gameObject;
        private Vector3 _targetVector;
        private float _speedMultiplier, _timeSpan;
        private SecondDegreeDynamicsAnimation _secondDegreeAnimation;
        private bool _disposeOnFinish;
        private Action<GameObject> _onFinishCallback;
        private VectorType _vectorType;
        private bool _isAlreadyDisposed;

        public TootTallyAnimation(GameObject gameObject, Vector3 startingVector, Vector3 targetVector, float speedMultiplier,
            float timeSpan, VectorType vectorType, SecondDegreeDynamicsAnimation secondDegreeAnimation, bool disposeOnFinish, Action<GameObject> onFinishCallback = null)
        {
            _gameObject = gameObject;
            _targetVector = targetVector;
            _speedMultiplier = speedMultiplier;
            _timeSpan = timeSpan;
            _vectorType = vectorType;
            _disposeOnFinish = disposeOnFinish;
            _onFinishCallback = onFinishCallback;
            _secondDegreeAnimation = secondDegreeAnimation;
            _secondDegreeAnimation.SetStartVector(startingVector);
        }

        public void SetStartVector(Vector3 startVector) => _secondDegreeAnimation.SetStartVector(startVector);

        public void SetTargetVector(Vector3 targetVector) => _targetVector = targetVector;

        public void SetCallback(Action<GameObject> onFinishCallback) => _onFinishCallback = onFinishCallback;

        public void UpdateVector(bool unscaledDeltaTime)
        {
            if (_gameObject == null)
            {
                Dispose();
                return;
            }

            var delta = unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeSpan -= delta;
            if (_timeSpan <= 0)
            {
                SnapToFinalVector();

                if (_onFinishCallback != null)
                {
                    _onFinishCallback(_gameObject);
                    _onFinishCallback = null;
                }
                if (_disposeOnFinish)
                    Dispose();
            }
            else
            {
                switch (_vectorType)
                {
                    case VectorType.TransformScale:
                        _gameObject.transform.localScale = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.TransformPosition:
                        _gameObject.transform.position = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.TransformLocalPosition:
                        _gameObject.transform.localPosition = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Position:
                        _gameObject.GetComponent<RectTransform>().anchoredPosition = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.SizeDelta:
                        _gameObject.GetComponent<RectTransform>().sizeDelta = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Scale:
                        _gameObject.GetComponent<RectTransform>().localScale = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.EulerAngle:
                        _gameObject.transform.eulerAngles = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Rotation:
                        _gameObject.transform.rotation = Quaternion.Euler(_secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier));
                        break;
                    case VectorType.Alpha:
                        _gameObject.GetComponent<CanvasGroup>().alpha = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier).x;
                        break;

                }
            }
        }

        public void SnapToFinalVector()
        {
            switch (_vectorType)
            {
                case VectorType.TransformScale:
                    _gameObject.transform.localScale = _targetVector;
                    break;
                case VectorType.TransformPosition:
                    _gameObject.transform.position = _targetVector;
                    break;
                case VectorType.TransformLocalPosition:
                    _gameObject.transform.localPosition = _targetVector;
                    break;
                case VectorType.Position:
                    _gameObject.GetComponent<RectTransform>().anchoredPosition = _targetVector;
                    break;
                case VectorType.SizeDelta:
                    _gameObject.GetComponent<RectTransform>().sizeDelta = _targetVector;
                    break;
                case VectorType.Scale:
                    _gameObject.GetComponent<RectTransform>().localScale = _targetVector;
                    break;
                case VectorType.EulerAngle:
                    _gameObject.transform.eulerAngles = _targetVector;
                    break;
                case VectorType.Rotation:
                    _gameObject.transform.rotation = Quaternion.Euler(_targetVector);
                    break;
                case VectorType.Alpha:
                    _gameObject.GetComponent<CanvasGroup>().alpha = _targetVector.x;
                    break;
            }
        }

        public void Dispose(bool snapToFinalVector = false)
        {
            if (_isAlreadyDisposed) return;
            if (snapToFinalVector)
                SnapToFinalVector();
            TootTallyAnimationManager.RemoveFromList(this);
            _isAlreadyDisposed = true;
        }

        public void Dispose() => Dispose(false);

        public enum VectorType
        {
            TransformScale,
            TransformPosition,
            TransformLocalPosition,
            Position,
            SizeDelta,
            Scale,
            EulerAngle,
            Rotation,
            Alpha,
        }
    }
}
