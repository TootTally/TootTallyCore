using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using static TootTallyCore.APIServices.SerializableClass;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.CodeDom;
using BepInEx;
using Unity.Jobs;

namespace TootTallyCore.Utils.Helpers
{
    public static class FileHelper
    {
        public static readonly string FILE_PATH_TOOTTALLY_APPDATA = Path.Combine(Application.persistentDataPath, "TootTally");

        public static T LoadFromTootTallyAppData<T>(string fileName)
        {
            var path = Path.Combine(FILE_PATH_TOOTTALLY_APPDATA, fileName);

            if (!File.Exists(path))
            {
                Plugin.LogError($"File {path} doesnt exist.");
                return default;
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Couldn't deserialize object: {ex.Message} - {ex.StackTrace}");
            }

            return default;
        }

        public static void SaveToTootTallyAppData<T>(string fileName, T obj, bool saveToBackupIfExists = false)
        {
            var path = Path.Combine(FILE_PATH_TOOTTALLY_APPDATA, fileName);
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                if (File.Exists(path) && saveToBackupIfExists)
                {
                    if (File.Exists(path + ".old")) //For fuck sake give me NetCore3.0 please
                        File.Delete(path + ".old");
                    File.Move(path, path + ".old"); //Backup
                }

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Couldn't serialize object: {ex.Message} - {ex.StackTrace}");
            }
        }

        //Backward compatibility reasons
        public static void TryMigrateFolder(string sourceFolderPath, string targetFolderPath, bool renameToOldIfAlreadyExists) => TryMigrateFolder(sourceFolderPath, targetFolderPath);
        public static void TryMigrateFolder(string sourceFolderPath, string targetFolderPath)
        {
            if (Directory.Exists(sourceFolderPath))
            {
                var folders = Directory.GetDirectories(sourceFolderPath);
                var files = Directory.GetFiles(sourceFolderPath);
                if (!Directory.Exists(targetFolderPath)) Directory.CreateDirectory(targetFolderPath);

                foreach (var f in folders)
                {
                    var targetFolder = Path.Combine(targetFolderPath, Path.GetFileNameWithoutExtension(f));
                    if (Directory.Exists(targetFolder)) continue;
                    Plugin.LogInfo($"Move new folder {targetFolder}");
                    Directory.Move(f, targetFolder);
                }
                foreach (var f in files)
                {
                    var targetFile = Path.Combine(targetFolderPath, Path.GetFileName(f));
                    if (File.Exists(targetFile)) continue;
                    Plugin.LogInfo($"Moving file {targetFile}");
                    File.Move(f, targetFile);
                }

                Plugin.LogInfo($"Deleting source folder {sourceFolderPath}");
                Directory.Delete(sourceFolderPath, true);
            }
        }


        public static void WriteJsonToFile(string dirName, string fileName, string jsonString)
        {
            try
            {
                Plugin.LogDebug("Creating MemoryStream Buffer for replay creation.");
                using (var memoryStream = new MemoryStream())
                {
                    Plugin.LogDebug("Creating ZipArchive for replay creation.");
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        var zipFile = zipArchive.CreateEntry(fileName);

                        Plugin.LogDebug("Writing Zipped replay to MemoryStream Buffer.");
                        using (var entry = zipFile.Open())
                        using (var sw = new StreamWriter(entry))
                        {
                            sw.Write(jsonString);
                        }
                    }

                    Plugin.LogDebug("Writing MemoryStream to File.");
                    using (var fileStream = new FileStream(dirName + fileName, FileMode.CreateNew))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.LogError(e.Message);
            }

        }

        public static string ReadJsonFromFile(string dirName, string fileName)
        {
            try
            {
                string jsonString;
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = new FileStream(dirName + fileName, FileMode.Open))
                    {
                        fileStream.CopyTo(memoryStream);
                    }

                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                    {
                        var zipFile = zipArchive.GetEntry(zipArchive.Entries[0].Name);

                        using (var entry = zipFile.Open())
                        using (var sr = new StreamReader(entry))
                        {
                            jsonString = sr.ReadToEnd();
                        }

                    }
                }
                return jsonString;
            }
            catch (Exception e)
            {
                Plugin.LogError(e.Message);
                return null;
            }
        }

        public static void WriteBytesToFile(string dirName, string fileName, byte[] bytes)
        {
            if (File.Exists(dirName + fileName)) return;

            File.Create(dirName + fileName).Close();

            File.WriteAllBytes(dirName + fileName, bytes);
        }

        public static void ExtractZipToDirectory(string source, string destination)
        {
            if (File.Exists(source))
                ZipFile.ExtractToDirectory(source, destination, true);
        }

        public static void DeleteFile(string dirName, string fileName)
        {
            if (File.Exists(dirName + fileName))
                File.Delete(dirName + fileName);
        }

        //Taken from https://stackoverflow.com/a/14488941
        private static readonly string[] SizeSuffixes =
                   { "b", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} b", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }


        private const string _DOWNLOAD_MIRROR_LINK = "https://sgp1.digitaloceanspaces.com/toottally/chartmirrors/"; //May or may not use this
        private const string _DISCORD_DOWNLOAD_HEADER = "https://cdn.discordapp.com/";
        private const string _GOOGLEDRIVE_LINK_HEADER = "https://drive.google.com/file/d/";
        private const string _GOOGLEDRIVE_DOWNLOAD_HEADER = "https://drive.google.com/uc?export=download&id=";
        private const string _PIXELDRAIN_DOWNLOAD_HEADER = "https://pixeldrain.com/u/";
        private const string _PIXELDRAIN_DIRECT_DOWNLOAD_HEADER = "https://pixeldrain.com/api/file/";
        private const string _TROMBONEDB_DIRECT_DOWNLOAD_HEADER = "https://db.trombone.fyi/";
        public static string GetDownloadLinkFromSongData(SongDataFromDB song)
        {
            if (song.mirror != null && Path.GetExtension(song.mirror).Contains(".zip"))
                return song.mirror;
            else if (song.download != null && song.download != "")
            {
                if (song.download.Contains(_DISCORD_DOWNLOAD_HEADER) && Path.GetExtension(song.download).Contains(".zip"))
                    return song.download;
                else if (song.download.Contains(_GOOGLEDRIVE_LINK_HEADER))
                    return GetGoogleDriveDownloadLink(song.download);
                else if (song.download.Contains(_PIXELDRAIN_DOWNLOAD_HEADER))
                    return song.download.Replace(_PIXELDRAIN_DOWNLOAD_HEADER, _PIXELDRAIN_DIRECT_DOWNLOAD_HEADER);
                else if (song.download.Contains(_PIXELDRAIN_DIRECT_DOWNLOAD_HEADER))
                    return song.download;
                else if (song.download.Contains(_TROMBONEDB_DIRECT_DOWNLOAD_HEADER))
                    return song.download;
            }
            return null;
        }

        private static string GetGoogleDriveDownloadLink(string downloadString) => _GOOGLEDRIVE_DOWNLOAD_HEADER + downloadString.Replace(_GOOGLEDRIVE_LINK_HEADER, "").Split('/')[0];

        public class FileData
        {
            public long size;
            public string extension;
        }
    }
}
