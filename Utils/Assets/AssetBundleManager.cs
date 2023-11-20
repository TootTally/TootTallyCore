using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TootTallyCore.Graphics;
using UnityEngine.Networking;
using UnityEngine;

namespace TootTallyCore.Utils.Assets
{
    public static class AssetBundleManager
    {
        private static Dictionary<string, GameObject> _prefabDict;
        private static bool _isInitialized;
        public static void LoadAssets(string filePath)
        {
            _prefabDict ??= new Dictionary<string, GameObject>();

            if (File.Exists(filePath))
                Plugin.Instance.StartCoroutine(LoadAssetBundle(filePath, OnAssetBundleLoaded));
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> LoadAssetBundle(string filePath, Action<AssetBundle> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(filePath);
            yield return webRequest.SendWebRequest();

            if (!webRequest.isNetworkError && !webRequest.isHttpError)
                callback(DownloadHandlerAssetBundle.GetContent(webRequest));
            else
                Plugin.LogError("AssetBundle failed to load.");
        }

        public static void OnAssetBundleLoaded(AssetBundle assetBundle)
        {
            if (assetBundle == null)
            {
                Plugin.LogError("AssetBundle was null");
                return;
            }
            assetBundle.GetAllAssetNames().ToList().ForEach(name => _prefabDict.Add(Path.GetFileNameWithoutExtension(name), assetBundle.LoadAsset<GameObject>(name)));
            _isInitialized = true;
        }

        public static GameObject GetPrefab(string name)
        {
            if (!_isInitialized || _prefabDict == null) return _defaultGameObject;

            if (!_prefabDict.ContainsKey(name))
            {
                Plugin.LogError($"Couldn't find asset bundle {name}");
                return _defaultGameObject;
            }

            return _prefabDict[name];
        }

        public static void Dispose()
        {
            _prefabDict.Clear();
            _prefabDict = null;
        }

        private static GameObject _defaultGameObject => GameObjectFactory.CreateImageHolder(null, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("icon.png"), "DefaultGameObject");
    }
}
