using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TootTallyCore.APIServices;
using UnityEngine;

namespace TootTallyCore.Utils.Assets
{
    public static class AssetManager
    {
        public const string DEFAULT_TEXTURE_NAME = "icon.png";

        public static Dictionary<string, Texture2D> textureDictionary;

        public static void LoadAssets(string directory)
        {
            textureDictionary ??= new Dictionary<string, Texture2D>();

            if (!Directory.Exists(directory)) return;

            foreach (string asset in Directory.GetFiles(directory).Where(f => Path.GetExtension(f) == ".png"))
            {
                var assetName = Path.GetFileName(asset);
                if (!textureDictionary.ContainsKey(assetName))
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(asset, texture =>
                    {
                        if (texture != null && !textureDictionary.ContainsKey(assetName))
                            textureDictionary.Add(assetName, texture);
                        else
                            DownloadAssetFromServer("http://cdn.toottally.com/assets/" + assetName, directory, assetName);
                    }));
            }
        }

        public static void DownloadAssetFromServer(string apiLink, string assetDir, string assetName)
        {
            Plugin.LogInfo("Downloading asset " + assetName);
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadTextureFromServer(apiLink, assetPath, success =>
            {
                ReloadTextureLocal(assetDir, assetName);
            }));
        }

        public static void ReloadTextureLocal(string assetDir, string assetName)
        {
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetPath, texture =>
            {
                if (texture != null && !textureDictionary.ContainsKey(assetName))
                {
                    Plugin.LogInfo("Asset " + assetName + " Reloaded");
                    textureDictionary.Add(assetName, texture);
                }
            }));
        }

        public static void GetProfilePictureByID(int userID, Action<Sprite> callback)
        {
            if (!textureDictionary.ContainsKey(userID.ToString()))
            {
                Plugin.Instance.StartCoroutine(TootTallyAPIService.LoadPFPFromServer(userID, (texture) =>
                {
                    if (!textureDictionary.ContainsKey(userID.ToString()))
                        textureDictionary.Add(userID.ToString(), texture);
                    callback(GetSprite(userID.ToString()));
                }));
            }
            else
                callback(GetSprite(userID.ToString()));

        }

        public static Texture2D GetTexture(string assetKey)
        {
            try
            {
                return textureDictionary[assetKey];
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Key {assetKey} not found.");
                Plugin.LogException(ex);
                return textureDictionary[DEFAULT_TEXTURE_NAME];
            }
        }

        public static Sprite GetSprite(string assetKey)
        {
            try
            {
                Texture2D texture = textureDictionary[assetKey];
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Key {assetKey} not found.");
                Plugin.LogException(ex);
                Texture2D texture = textureDictionary[DEFAULT_TEXTURE_NAME];
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
            }
        }
    }
}
