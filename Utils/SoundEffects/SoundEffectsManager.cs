using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TootTallyCore.Utils.SoundEffects
{
    public static class SoundEffectsManager
    {
        private static AudioSource _soundPlayer;
        private static AudioClip _btnHover, _btnClick;

        public static void Initialize(HomeController __instance)
        {
            _soundPlayer = GameObject.Instantiate(__instance.sfxhoverobj);
            _soundPlayer.name = "TootTallySoundPlayer";
            _soundPlayer.volume = .5f;
            _btnHover = __instance.allsfx[2];
            _btnClick = __instance.allsfx[4];

            GameObject.DontDestroyOnLoad(_soundPlayer);
        }

        public static void PlayerBtnHover()
        {
            if (_btnHover == null) return;

            _soundPlayer.clip = _btnHover;
            _soundPlayer.Play();
        }

        public static void PlayerBtnClick()
        {
            if (_btnClick == null) return;

            _soundPlayer.clip = _btnClick;
            _soundPlayer.Play();
        }
    }
}
