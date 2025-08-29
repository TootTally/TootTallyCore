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

        private static void DisplayNotif(string message, Color textColor, float lifespan)
        {
            if (!IsInitialized || !Plugin.Instance.ShouldShowNotifs.Value) return;

            _pendingNotifications.Enqueue(new TootTallyNotifData(message, textColor, lifespan));
        }

        private static TootTallyNotif DisplayNotif(string message, Color textColor)
        {
            if (!IsInitialized || !Plugin.Instance.ShouldShowNotifs.Value) return null;
            var notif = GameObjectFactory.CreateNotif(_notifCanvas.transform, "Notification", message, textColor);
            notif.Initialize(float.MaxValue, new Vector2(695, -400));
            notif.gameObject.SetActive(true);
            _activeNotificationList.Add(notif);
            OnNotifCountChangeSetNewPosition();
            return notif;
        }

        /// <summary>
        /// WARNING: Do not use this function out of unity's main thread or it will cause a crash.
        /// </summary>
        public static TootTallyNotif ManualNotif(string message, Color textColor) => DisplayNotif(message, textColor);

        public static void DisplayNotif(string message, float lifespan = 6f) => DisplayNotif(message, Theme.colors.notification.defaultText, lifespan);
        public static void DisplayWarning(string message, float lifespan = 6f) => DisplayNotif(message, Theme.colors.notification.warningText, lifespan);
        public static void DisplayError(string message, float lifespan = 6f) => DisplayNotif(message, Theme.colors.notification.errorText, lifespan);
        public static void DisplayCustom(string message, Color textColor, float lifespan = 6f) => DisplayNotif(message, textColor, lifespan);

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
                notif.Initialize(notifData.lifespan, new Vector2(695, -400));
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
            public TootTallyNotifData(string message, Color textColor, float lifespan)
            {
                this.message = message;
                this.textColor = textColor;
                this.lifespan = lifespan;
            }

            public string message { get; set; }
            public Color textColor { get; set; }
            public float lifespan { get; set; }
        }
    }
}
