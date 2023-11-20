using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.Helpers;
using UnityEngine;

namespace TootTallyCore.Graphics
{
    public class LoadingIcon
    {
        public GameObject iconHolder;
        private bool _isActive;
        private bool _recursiveAnimationActive;
        private TootTallyAnimation _currentAnimation;

        public LoadingIcon(GameObject iconHolder, bool isActive)
        {
            this.iconHolder = iconHolder;
            _isActive = isActive;
            this.iconHolder.SetActive(isActive);
        }

        public void StartRecursiveAnimation()
        {
            _recursiveAnimationActive = true;
            RecursiveAnimation();
        }

        public void StopRecursiveAnimation(bool immediate)
        {
            _recursiveAnimationActive = false;
            if (immediate && _currentAnimation != null)
            {
                _currentAnimation.Dispose();
                _currentAnimation = null;
            }
        }

        public void RecursiveAnimation()
        {
            _currentAnimation = TootTallyAnimationManager.AddNewEulerAngleAnimation(iconHolder, new Vector3(0, 0, 359), 0.9f, GetSecondDegreeAnimation(), (sender) =>
            {
                _currentAnimation = TootTallyAnimationManager.AddNewEulerAngleAnimation(iconHolder, new Vector3(0, 0, 0), 0.9f, GetSecondDegreeAnimation(), (sender) =>
                {
                    if (_recursiveAnimationActive)
                        RecursiveAnimation();
                });
            });
        }

        public void Show()
        {
            iconHolder.SetActive(true);
            _isActive = true;
        }

        public void Hide()
        {
            iconHolder.SetActive(false);
            _isActive = false;
        }

        public void ToggleShow()
        {
            _isActive = !_isActive;
            iconHolder.SetActive(_isActive);
        }

        public bool IsVisible() => _isActive;

        public void Dispose()
        {
            _currentAnimation?.Dispose();
            UnityEngine.Object.DestroyImmediate(iconHolder);
        }

        public SecondDegreeDynamicsAnimation GetSecondDegreeAnimation() => new(0.8f, .5f, 1f);
    }
}
