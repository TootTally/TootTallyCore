﻿using System;
using System.Collections.Generic;

namespace TootTallyCore.APIServices
{
    public static class SerializableClass
    {
        [Serializable]
        public class TMBFile
        {
            public string tmb;
        }

        [Serializable]
        public class APIActiveSpectator
        {
            public int[] active;
        }

        [Serializable]
        public class TMBData
        {
            public string name;
            public string shortName;
            public string trackRef;
            public int year;
            public string author;
            public string genre;
            public string description;
            public int difficulty;
            public int savednotespacing;
            public float endpoint;
            public int timesig;
            public float tempo;
            public List<float[]> notes;
        }


        [Serializable]
        public class ScoreDataFromDB
        {
            public int score;
            public string player;
            public int player_id;
            public string played_on;
            public string grade;
            public int perfect;
            public int nice;
            public int okay;
            public int meh;
            public int nasty;
            public string replay_id;
            public int max_combo;
            public float percentage;
            public string game_version;
            public float tt;
            public bool is_rated;
            public float replay_speed;
            public string[] modifiers;

            public int[] GetTally => new int[] { nasty, meh, okay, nice, perfect };
        }

        [Serializable]
        public class SongDataFromDB
        {
            public int id;
            public float difficulty;
            public float tap;
            public float aim;
            public float base_tt;
            public bool is_rated;
            public float[] speed_diffs;
            public float[] speed_taps;
            public float[] speed_aim;
            public string name;
            public string short_name;
            public string author;
            public string download;
            public string mirror;
            public string charter;
            public string track_ref;
            public float song_length;
        }

        [Serializable]
        public class SongInfoFromDB
        {
            public float count;
            public string next;
            public string previous;
            public SongDataFromDB[] results;
        }


        [Serializable]
        public class LeaderboardInfo
        {
            public int count;
            public string next;
            public string previous;
            public ScoreDataFromDB[] results;
        }

        [Serializable]
        public struct SendableModInfo
        {
            public string name;
            public string version;
            public string hash;
        }

        [Serializable]
        public class ModInfoAPI
        {
            public string apiKey;
            public string steamTicket;
            public SendableModInfo[] mods;
        }



        [Serializable]
        public class Message
        {
            public string author;
            public string message;
            public string sent_on;
        }

        [Serializable]
        public class APIMessages
        {
            public List<Message> results;
        }

        [Serializable]
        public class APIUsers
        {
            public string next;
            public List<User> results;
        }


        [Serializable]
        public class APISubmission
        {
            public string apiKey;
        }

        [Serializable]
        public class APIFriendSubmission
        {
            public string apiKey;
            public int userID;
        }

        [Serializable]
        public struct APIHeartbeat
        {
            public string apiKey;
            public int status;
        }

        [Serializable]
        public struct APISteamConnect
        {
            public string apiKey;
            public string steamTicket;
        }

        [Serializable]
        public class APISignUp
        {
            public string username;
            public string password;
            public string pass_check;
            public string steamTicket;
        }

        [Serializable]
        public class APILogin
        {
            public string username;
            public string password;
        }

        [Serializable]
        public class APISteamLogin
        {
            public string steamTicket;
        }

        [Serializable]
        public class LoginToken
        {
            public string token;
        }

        [Serializable]
        public class ReplayStart
        {
            public string id;
            public string speed;
        }

        [Serializable]
        public class ReplayUUIDSubmission
        {
            public string apiKey;
            public string songHash;
            public float speed;
        }

        [Serializable]
        public class ReplayStopSubmission
        {
            public string apiKey;
            public string replayId;
        }

        [Serializable]
        public class ReplayData
        {
            public string version;
            public string username;
            public string starttime;
            public string endtime;
            public string uuid;
            public string input;
            public string song;
            public float samplerate;
            public float scrollspeed;
            public float gamespeedmultiplier;
            public string gamemodifiers;
            public float audiolatency;
            public int pluginbuilddate;
            public string gameversion;
            public string songhash;
            public int finalscore;
            public int maxcombo;
            public int[] finalnotetallies;
            public List<int[]> framedata;
            public List<int[]> notedata;
            public List<int[]> tootdata;
        }

        [Serializable]
        public class ReplaySubmissionReply
        {
            public string replayUrl;
            public float tt;
            public bool isBestPlay;
            public int position;
            public int ranking;
        }

        [Serializable]
        public class User
        {
            public string username;
            public int id;
            public string country;
            public float tt;
            public int rank;
            public bool allowSubmit;
            public bool dev;
            public bool moderator;
            public string email;
            public string api_key;
            public string status;
            public string picture;
            public SongDataFromDB[] currently_playing;
            public string friend_status;
        }

        [Serializable]
        public class TwitchAccessToken
        {
            public string access_token;
        }

        [Serializable]
        public class ThunderstoreLatestData
        {
            public string version_number;
        }

        [Serializable]
        public class ThunderstorePluginData
        {
            public ThunderstoreLatestData latest;
        }

        #region Theme
        [Serializable]
        public struct BackButtonJson
        {
            public string background;
            public string backgroundOver;
            public string outline;
            public string outlineOver;
            public string text;
            public string textOver;
            public string shadow;
            public string shadowOver;
        }

        [Serializable]
        public struct CapsulesJson
        {
            public string year;
            public string yearShadow;
            public string composer;
            public string composerShadow;
            public string genre;
            public string genreShadow;
            public string description;
            public string descriptionShadow;
            public string tempo;
        }

        [Serializable]
        public struct DiffStarJson
        {
            public string gradientStart;
            public string gradientEnd;
        }

        [Serializable]
        public struct LeaderboardJson
        {
            public string panelBody;
            public string scoresBody;
            public string rowEntry;
            public string yourRowEntry;
            public string headerText;
            public string text;
            public string textOutline;
            public SliderJson slider;
            public TabsJson tabs;
        }

        [Serializable]
        public struct NotificationJson
        {
            public string border;
            public string background;
            public string defaultText;
            public string warningText;
            public string errorText;
            public string textOutline;
        }

        [Serializable]
        public struct PlayButtonJson
        {
            public string background;
            public string backgroundOver;
            public string outline;
            public string outlineOver;
            public string text;
            public string textOver;
            public string shadow;
            public string shadowOver;
        }

        [Serializable]
        public struct RandomButtonJson
        {
            public string background;
            public string backgroundOver;
            public string outline;
            public string outlineOver;
            public string text;
            public string textOver;
        }

        [Serializable]
        public struct ReplayButtonJson
        {
            public string text;
            public string normal;
            public string pressed;
            public string highlighted;
        }

        [Serializable]
        public class JsonThemeDeserializer
        {
            public string version;
            public ThemeJson theme;
        }

        [Serializable]
        public struct ScrollSpeedSliderJson
        {
            public string handle;
            public string text;
            public string background;
            public string fill;
        }

        [Serializable]
        public struct SliderJson
        {
            public string handle;
            public string background;
            public string fill;
        }

        [Serializable]
        public struct SongButtonJson
        {
            public string background;
            public string text;
            public string textOver;
            public string outline;
            public string outlineOver;
            public string shadow;
            public string square;
        }

        [Serializable]
        public struct TabsJson
        {
            public string normal;
            public string pressed;
            public string highlighted;
        }
        [Serializable]
        public struct PointerJson
        {
            public string background;
            public string shadow;
            public string outline;
        }

        [Serializable]
        public struct BackgroundJson
        {
            public string waves;
            public string waves2;
            public string dots;
            public string dots2;
            public string shape;
            public string diamond;
            public string background;
        }

        [Serializable]
        public struct TitleJson
        {
            public string songName;
            public string titleBar;
            public string title;
            public string titleShadow;
        }

        [Serializable]
        public struct ThemeJson
        {
            public LeaderboardJson leaderboard;
            public ScrollSpeedSliderJson scrollSpeedSlider;
            public NotificationJson notification;
            public ReplayButtonJson replayButton;
            public CapsulesJson capsules;
            public RandomButtonJson randomButton;
            public BackButtonJson backButton;
            public PlayButtonJson playButton;
            public SongButtonJson songButton;
            public DiffStarJson diffStar;
            public PointerJson pointer;
            public BackgroundJson background;
            public TitleJson title;
        }
        #endregion
    }
}
