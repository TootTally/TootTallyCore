﻿using System;
using TMPro;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.Helpers;
using UnityEngine;

namespace TootTallyCore.Utils.TootTallyNotifs
{
    public class TootTallyNotif : MonoBehaviour
    {
        public string GetText { get => _text; }

        private TMP_Text _textHolder;
        private string _text;
        private Color _textColor;
        private RectTransform _rectTransform;
        private Vector2 _endPosition;
        private CanvasGroup _canvasGroup;
        private SecondDegreeDynamicsAnimation _secondOrderDynamic;

        public void SetText(string message) => _text = message;
        public void SetTextSize(int size) => _textHolder.fontSize = size;

        public void SetTextAlign(TextAlignmentOptions textAnchor) => _textHolder.alignment = textAnchor;
        public void UpdateText(string text) => _textHolder.text = _text = text;
        public void SetTextColor(Color color) => _textColor = color;

        public void Initialize(Vector2 endPosition)
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new SecondDegreeDynamicsAnimation(1.4f, 0.85f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            _textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<TMP_Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = _rectTransform.sizeDelta - Vector2.one * 20;
            _textHolder.GetComponent<RectTransform>().anchoredPosition += new Vector2(1, -1) * 10;
            //_textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        public void Initialize(float lifespan, Vector2 endPosition, Vector2 textRectSize)
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new SecondDegreeDynamicsAnimation(1.4f, 0.85f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            _textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<TMP_Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = textRectSize;
            _textHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(.5f, 0) * textRectSize;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            gameObject.SetActive(true);
        }
        public void Initialize(float lifespan, Vector2 endPosition, Vector2 textRectSize, Vector2 textPosition)
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new SecondDegreeDynamicsAnimation(1.4f, 0.85f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            _textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<TMP_Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = textRectSize;
            _textHolder.GetComponent<RectTransform>().anchoredPosition = textPosition;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            gameObject.SetActive(true);
        }

        public void SetTransitionConstants(float f, float z, float r) => _secondOrderDynamic.SetConstants(f, z, r);

        public void SetTransitionToNewPosition(Vector2 endPosition)
        {
            _secondOrderDynamic.SetStartVector(_rectTransform.anchoredPosition);
            _endPosition = endPosition;
        }

        public void Update()
        {
            if (_secondOrderDynamic != null && _rectTransform.anchoredPosition != _endPosition)
                _rectTransform.anchoredPosition = _secondOrderDynamic.GetNewVector(_endPosition, Time.deltaTime);
        }
    }
}
