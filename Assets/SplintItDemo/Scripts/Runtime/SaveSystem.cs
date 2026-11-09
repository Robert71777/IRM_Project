using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SplintIt
{
    /// <summary>Local JSON persistence: comfort preferences, and per scenario/mode evaluation records.
    /// No accounts, no cloud, no global leaderboard. If the file cannot be read it is left untouched,
    /// the player is told via <see cref="Message"/>, and the session runs temporary (nothing is written).
    /// A <see cref="Temporary"/> flag exists for the (out-of-scope) Demo/guest mode: when set, nothing persists.</summary>
    public static class SaveSystem
    {
        [Serializable]
        public class Record
        {
            public string key;          // scenario|mode
            public bool completed;
            public float bestTime = float.MaxValue;
            public int bestStars;
            public int fewestMistakes = int.MaxValue;
            public int plays;
        }

        [Serializable]
        public class SaveData
        {
            public int version = 1;

            // comfort / preferences
            public float volume = 0.8f;
            public bool seated = false;
            public float heightOffset = 0f;
            public bool subtitles = true;
            public bool dominantRight = true;

            // per scenario/mode evaluation records
            public List<Record> records = new List<Record>();
        }

        public static SaveData Data { get; private set; } = new SaveData();

        /// <summary>Set when a non-fatal save problem happened (shown on the menu). Empty otherwise.</summary>
        public static string Message { get; private set; } = "";

        /// <summary>Guest/Demo mode: when true the data lives only in memory and is never written to disk.</summary>
        public static bool Temporary { get; set; } = false;

        static bool loaded;

        static string FilePath => Path.Combine(Application.persistentDataPath, "splintit_save.json");

        public static void Load()
        {
            if (loaded) return;
            loaded = true;
            Message = "";

            if (Temporary)
            {
                Data = new SaveData();
                return;
            }

            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var parsed = JsonUtility.FromJson<SaveData>(json);
                    if (parsed == null) throw new Exception("empty or invalid JSON");
                    Data = parsed;
                    if (Data.records == null) Data.records = new List<Record>();
                }
                else
                {
                    Data = new SaveData();
                }
            }
            catch (Exception e)
            {
                // Leave the existing file untouched, tell the player, run a temporary session.
                Debug.LogWarning("[SplintIt] Could not read save file; running a temporary session. " + e.Message);
                Data = new SaveData();
                Temporary = true;
                Message = "Saved progress could not be read. Your existing file was left untouched; this session will not be saved.";
            }
        }

        public static void SavePreferences()
        {
            Write();
        }

        /// <summary>Records one finished Evaluation round. Guided/assisted/abandoned rounds must not reach here.
        /// A failed evaluation never erases earlier successful records.</summary>
        public static void RecordEvaluation(EvaluationResult result, string scenarioName)
        {
            if (result == null || result.mode != GameMode.Evaluation) return;
            Load();

            string key = scenarioName + "|" + result.mode;
            Record rec = Data.records.Find(r => r.key == key);
            if (rec == null)
            {
                rec = new Record { key = key };
                Data.records.Add(rec);
            }

            rec.plays++;
            if (result.success)
            {
                rec.completed = true;
                if (result.secondsUsed < rec.bestTime) rec.bestTime = result.secondsUsed;
                if (result.stars > rec.bestStars) rec.bestStars = result.stars;
                if (result.mistakes < rec.fewestMistakes) rec.fewestMistakes = result.mistakes;
            }

            Write();
        }

        public static Record GetRecord(string scenarioName, GameMode mode)
        {
            Load();
            return Data.records.Find(r => r.key == scenarioName + "|" + mode);
        }

        static void Write()
        {
            if (Temporary) return;   // guest mode / unreadable save: never persist
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SplintIt] Could not write save file. " + e.Message);
                Message = "Progress could not be saved to disk this time.";
            }
        }
    }
}
