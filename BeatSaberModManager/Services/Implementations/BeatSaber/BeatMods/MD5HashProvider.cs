﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class MD5HashProvider : IHashProvider
    {
        public async Task<string?> CalculateHashForFile(string path)
        {
            await using Stream? stream = IOUtils.SafeOpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous);
            if (stream is null) return null;
            return await CalculateHashForStream(stream);
        }

        public async Task<string?> CalculateHashForStream(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}