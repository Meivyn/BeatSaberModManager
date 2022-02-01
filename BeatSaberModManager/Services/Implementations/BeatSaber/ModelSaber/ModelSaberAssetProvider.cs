﻿using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberAssetProvider : IAssetProvider
    {
        private readonly ModelSaberModelInstaller _modelSaberModelInstaller;

        public ModelSaberAssetProvider(ModelSaberModelInstaller modelSaberModelInstaller)
        {
            _modelSaberModelInstaller = modelSaberModelInstaller;
        }

        public string Protocol => "modelsaber";

        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => _modelSaberModelInstaller.InstallModelAsync(installDir, uri, progress);
    }
}