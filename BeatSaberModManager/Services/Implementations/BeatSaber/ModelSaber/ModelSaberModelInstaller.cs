﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberModelInstaller
    {
        private readonly HttpProgressClient _httpClient;

        public ModelSaberModelInstaller(HttpProgressClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> InstallModelAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            string? folderName = GetFolderName(uri);
            if (folderName is null) return false;
            string folderPath = Path.Combine(installDir, folderName);
            if (!IOUtils.TryCreateDirectory(folderPath)) return false;
            string modelName = WebUtility.UrlDecode(uri.Segments.Last());
            progress?.Report(new ProgressInfo(StatusType.Installing, modelName));
            using HttpResponseMessage response = await _httpClient.GetAsync($"https://modelsaber.com/files/{uri.Host}{uri.AbsolutePath}", progress).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            byte[] body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            string filePath = Path.Combine(folderPath, modelName);
            await File.WriteAllBytesAsync(filePath, body).ConfigureAwait(false);
            return true;
        }

        private static string? GetFolderName(Uri uri) => uri.Host switch
        {
            "avatar" => "CustomAvatars",
            "saber" => "CustomSabers",
            "platform" => "CustomPlatforms",
            "bloq" => "CustomNotes",
            _ => null
        };
    }
}