﻿using System;
using System.IO;
using System.Text.Json;

using BeatSaberModManager.Models.Implementations.JsonSerializerContexts;


namespace BeatSaberModManager.Models.Implementations
{
    public class Settings
    {
        public string? InstallDir { get; set; }
        public string? VRPlatform { get; set; }
        public string? ThemesDir { get; set; }
        public string? ThemeName { get; set; }
        public string? LanguageCode { get; set; }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, SettingsJsonSerializerContext.Default.Settings);
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            File.WriteAllText(_saveFilePath, json);
        }

        private static readonly string _saveDirPath;
        private static readonly string _saveFilePath;

        static Settings()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, nameof(BeatSaberModManager));
            _saveFilePath = Path.Combine(_saveDirPath, "settings.json");
        }

        public static Settings Load()
        {
            Settings? settings = null;
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            if (File.Exists(_saveFilePath)) settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_saveFilePath), SettingsJsonSerializerContext.Default.Settings);
            return settings ?? new Settings();
        }
    }
}