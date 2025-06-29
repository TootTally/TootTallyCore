using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TootTallyCore.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallyCore.Utils.TootTallyNotifs
{
    public class TootTallyNotifManager : MonoBehaviour
    {
        private static List<TootTallyNotif> _activeNotificationList;
        private static List<TootTallyNotif> _toRemoveNotificationList;
        private static ConcurrentQueue<TootTallyNotifData> _pendingNotifications;
        private static GameObject _notifCanvas;
        private static bool IsInitialized = false;

        private void Awake()
        {
            if (IsInitialized) return;

            _notifCanvas = new GameObject("NotifCanvas");
            Canvas canvas = _notifCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1;
            CanvasScaler scaler = _notifCanvas.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject.DontDestroyOnLoad(_notifCanvas);
            _pendingNotifications = new ConcurrentQueue<TootTallyNotifData>();
            _activeNotificationList = new List<TootTallyNotif>();
            _toRemoveNotificationList = new List<TootTallyNotif>();
            IsInitialized = true;
        }

        private static void DisplayNotif(string message, Color textColor)
        {
            if (!IsInitialized || !Plugin.Instance.ShouldShowNotifs.Value) return;

            _pendingNotifications.Enqueue(new TootTallyNotifData(message, textColor));
        }

        public static void DisplayNotif(string message) => DisplayNotif(message, Theme.colors.notification.defaultText);
        public static void DisplayWarning(string message) => DisplayNotif(message, Theme.colors.notification.warningText);
        public static void DisplayError(string message) => DisplayNotif(message, Theme.colors.notification.errorText);
        public static void DisplayCustom(string message, Color textColor) => DisplayNotif(message, textColor);

        private static void OnNotifCountChangeSetNewPosition()
        {
            int count = 0;
            for (int i = _activeNotificationList.Count - 1; i >= 0; i--)
            {
                _activeNotificationList[i].SetTransitionToNewPosition(new Vector2(695, -400 + (215 * count)));
                count++;
            }
        }

        private void Update()
        {
            if (!IsInitialized) return;

            while (_pendingNotifications != null && _pendingNotifications.Count > 0 && _pendingNotifications.TryDequeue(out TootTallyNotifData notifData))
            {
                var notif = GameObjectFactory.CreateNotif(_notifCanvas.transform, "Notification", notifData.message, notifData.textColor);
                notif.Initialize(new Vector2(695, -400));
                notif.gameObject.SetActive(true);
                _activeNotificationList.Add(notif);
                OnNotifCountChangeSetNewPosition();
            }

            _activeNotificationList?.ForEach(notif => notif.Update());

            if (_toRemoveNotificationList != null && _toRemoveNotificationList.Count > 0)
            {
                foreach (TootTallyNotif notif in _toRemoveNotificationList)
                {
                    _activeNotificationList.Remove(notif);
                    GameObject.Destroy(notif.gameObject);
                }
                OnNotifCountChangeSetNewPosition();
                _toRemoveNotificationList.Clear();
            }

        }

        public static void QueueToRemovedFromList(TootTallyNotif notif) => _toRemoveNotificationList.Add(notif);

        private class TootTallyNotifData
        {
            public TootTallyNotifData(string message, Color textColor)
            {
                this.message = message;
                this.textColor = textColor;
            }

            public string message { get; set; }
            public Color textColor { get; set; }
        }
    }
}
