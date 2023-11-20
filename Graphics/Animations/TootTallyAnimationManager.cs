﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.Utils.Helpers;
using UnityEngine;

namespace TootTallyCore.Graphics.Animations
{
    public class TootTallyAnimationManager
    {
        private static List<TootTallyAnimation> _animationList;
        private static List<TootTallyAnimation> _animationToAdd;
        private static List<TootTallyAnimation> _animationToRemove;
        private static bool _isInitialized;

        public static TootTallyAnimation AddNewTransformPositionAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.transform.position, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.TransformPosition, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewPositionAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.GetComponent<RectTransform>().anchoredPosition, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.Position, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewSizeDeltaAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.GetComponent<RectTransform>().sizeDelta, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.SizeDelta, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewScaleAnimation(GameObject gameObject, Vector2 targetVector,
           float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.GetComponent<RectTransform>().localScale, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.Scale, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewTransformScaleAnimation(GameObject gameObject, Vector2 targetVector,
           float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.transform.localScale, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.TransformScale, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewEulerAngleAnimation(GameObject gameObject, Vector3 targetVector,
           float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.transform.eulerAngles, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.EulerAngle, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewRotationAnimation(GameObject gameObject, Vector3 targetVector,
           float timeSpan, SecondDegreeDynamicsAnimation secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, gameObject.transform.rotation.eulerAngles, targetVector, 1f, timeSpan, TootTallyAnimation.VectorType.Rotation, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static TootTallyAnimation AddNewAnimation(GameObject gameObject, Vector3 startingVector, Vector3 targetVector, float speedMultiplier,
            float timeSpan, TootTallyAnimation.VectorType vectorType, SecondDegreeDynamicsAnimation secondDegreeAnimation, bool disposeOnFinish, Action<GameObject> onFinishCallback = null)
        {
            TootTallyAnimation anim = new TootTallyAnimation(gameObject, startingVector, targetVector, speedMultiplier, timeSpan, vectorType, secondDegreeAnimation, disposeOnFinish, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        private void Awake()
        {
            if (_isInitialized) return;

            _animationList = new List<TootTallyAnimation>();
            _animationToAdd = new List<TootTallyAnimation>();
            _animationToRemove = new List<TootTallyAnimation>();
            _isInitialized = true;
        }

        public static void AddToList(TootTallyAnimation anim) => _animationToAdd.Add(anim);
        public static void RemoveFromList(TootTallyAnimation anim) => _animationToRemove.Add(anim);

        private void Update()
        {
            if (!_isInitialized) return;

            //add animation the needs to be added
            if (_animationToAdd.Count > 0)
            {
                _animationToAdd.ForEach(anim => _animationList.Add(anim));
                _animationToAdd.Clear();
            }

            //update all animations
            _animationList.ForEach(anim => anim.UpdateVector());

            //remove animations that are done
            if (_animationToRemove.Count > 0)
            {
                _animationToRemove.ForEach(anim => _animationList.Remove(anim));
                _animationToRemove.Clear();
            }

        }

    }
}
