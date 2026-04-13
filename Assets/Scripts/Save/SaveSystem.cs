using System;
using System.IO;
using UnityEngine;

namespace TCG.Save
{
    /// <summary>
    /// Static utility for loading and saving PlayerSaveData to disk as JSON.
    /// On standalone builds uses Application.persistentDataPath.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "player_save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private static PlayerSaveData _cachedSave;

        /// <summary>Loads save data from disk (cached after first call).</summary>
        public static PlayerSaveData Load()
        {
            if (_cachedSave != null) return _cachedSave;

            if (!File.Exists(FilePath))
            {
                _cachedSave = new PlayerSaveData();
                return _cachedSave;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                _cachedSave = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load save data: {ex.Message}");
                _cachedSave = new PlayerSaveData();
            }

            return _cachedSave;
        }

        /// <summary>Saves data to disk and updates the in-memory cache.</summary>
        public static void Save(PlayerSaveData data)
        {
            if (data == null) return;

            _cachedSave = data;
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save data: {ex.Message}");
            }
        }

        /// <summary>Deletes the save file and clears the cache (use for new game / debug).</summary>
        public static void DeleteSave()
        {
            _cachedSave = null;
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        /// <summary>Invalidates the in-memory cache so the next Load() reads from disk.</summary>
        public static void InvalidateCache() => _cachedSave = null;
    }
}
