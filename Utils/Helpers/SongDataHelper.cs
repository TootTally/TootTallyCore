﻿using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Internal.BaseGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using TootTallyCore.APIServices;
using TrombLoader.CustomTracks;
using TrombLoader.Helpers;
using UnityEngine;

namespace TootTallyCore.Utils.Helpers
{
    public static class SongDataHelper
    {
        public class DecimalJsonConverter : JsonConverter<float>
        {
            public override bool CanRead => false;
            public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
            {
                if (value == Math.Truncate(value))
                    writer.WriteRawValue(((int)value).ToString());
                else
                {
                    writer.WriteRawValue(((double)value).ToString(CultureInfo.InvariantCulture));
                }
            }

            public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public static string CalcSHA256Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var ret = new StringBuilder();
                byte[] hashArray = sha256.ComputeHash(data);
                foreach (byte b in hashArray)
                {
                    ret.Append(b.ToString("x2"));
                }
                return ret.ToString();
            }
        }

        public static string CalcFileHash(string fileLocation)
        {
            if (!File.Exists(fileLocation) || new FileInfo(fileLocation).Length > 2_000_000)
                return "";
            return CalcSHA256Hash(Encoding.UTF8.GetBytes(File.ReadAllText(fileLocation)));
        }

        public static string GetSongFilePath(TromboneTrack track)
        {
            if (track is CustomTrack ct)
            {
                return Path.Combine(ct.folderPath, Globals.defaultChartName);
            }
            else
            {
                return $"{Application.streamingAssetsPath}/trackassets/{track.trackref}/trackdata.tmb";
            }
        }

        public static string GetChoosenSongHash()
        {
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            var track = TrackLookup.lookup(trackRef);

            return GetSongHash(track);
        }

        public static string GetSongHash(TromboneTrack track)
        {
            if (track is CustomTrack)
                return CalcFileHash(GetSongFilePath(track));
            else if (track is BaseGameTrack)
                return track.trackref;
            Plugin.LogInfo($"TrackType {track.GetType()} is unknown.");
            return null;

        }

        public static string GenerateBaseTmb(TromboneTrack track)
        {
            var singleTrackData = TrackLookup.toTrackData(track);
            var songFilePath = GetSongFilePath(track);

            var tmb = new SerializableClass.TMBData
            {
                name = singleTrackData.trackname_long,
                shortName = singleTrackData.trackname_short,
                trackRef = singleTrackData.trackref,
                author = singleTrackData.artist,
                genre = singleTrackData.genre,
                description = singleTrackData.desc,
                difficulty = singleTrackData.difficulty
            };

            int.TryParse(new string(singleTrackData.year.Where(char.IsDigit).ToArray()), out tmb.year);

            using (FileStream fileStream = File.Open(songFilePath, FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                var savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
                var levelData = new List<float[]>();
                savedLevel.savedleveldata.ForEach(arr =>
                {
                    var noteData = new List<float>();
                    foreach (var note in arr) noteData.Add(note);
                    levelData.Add(noteData.ToArray());
                });
                tmb.savednotespacing = savedLevel.savednotespacing;
                tmb.endpoint = savedLevel.endpoint;
                tmb.timesig = savedLevel.timesig;
                tmb.tempo = savedLevel.tempo;
                tmb.notes = levelData;
            }

            return JsonConvert.SerializeObject(tmb, new DecimalJsonConverter());
        }
    }
}
