﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly IModProvider _modProvider;

        public BeatModsModInstaller(IModProvider modProvider)
        {
            _modProvider = modProvider;
        }

        public async IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            IEnumerable<string> urls = beatModsMods.Select(static x => x.Downloads[0].Url);
            string pendingDirPath = Path.Combine(installDir, Constants.IpaDir, Constants.PendingDir);
            IOUtils.TryCreateDirectory(pendingDirPath);
            int i = 0;
            progress?.Report(new ProgressInfo(StatusType.Installing, beatModsMods[i].Name));
            await foreach (ZipArchive archive in _modProvider.DownloadModsAsync(urls, progress).ConfigureAwait(false))
            {
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                string extractDir = isModLoader ? installDir : pendingDirPath;
                IOUtils.TryExtractArchive(archive, extractDir, true);
                if (isModLoader) await InstallBsipaAsync(installDir).ConfigureAwait(false);
                yield return beatModsMods[i++];
                if (i >= beatModsMods.Length) break;
                progress?.Report(new ProgressInfo(StatusType.Installing, beatModsMods[i].Name));
            }

            progress?.Report(new ProgressInfo(StatusType.Completed, null));
        }

        public async IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            for (int i = 0; i < beatModsMods.Length; i++)
            {
                progress?.Report(new ProgressInfo(StatusType.Uninstalling, beatModsMods[i].Name));
                progress?.Report(((double)i + 1) / beatModsMods.Length);
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                if (isModLoader) await UninstallBsipaAsync(installDir, beatModsMods[i]).ConfigureAwait(false);
                else RemoveModFiles(installDir, beatModsMods[i]);
                yield return beatModsMods[i];
            }

            progress?.Report(new ProgressInfo(StatusType.Completed, null));
        }

        public void RemoveAllMods(string installDir)
        {
            string pluginsDirPath = Path.Combine(installDir, Constants.PluginsDir);
            string libsDirPath = Path.Combine(installDir, Constants.LibsDir);
            string ipaDirPath = Path.Combine(installDir, Constants.IpaDir);
            string winhttpPath = Path.Combine(installDir, Constants.WinHttpDll);
            IOUtils.TryDeleteDirectory(pluginsDirPath, true);
            IOUtils.TryDeleteDirectory(libsDirPath, true);
            IOUtils.TryDeleteDirectory(ipaDirPath, true);
            IOUtils.TryDeleteFile(winhttpPath);
        }

        private static Task InstallBsipaAsync(string installDir) =>
            OperatingSystem.IsWindows()
                ? InstallBsipaWindowsAsync(installDir)
                : OperatingSystem.IsLinux()
                    ? InstallBsipaLinux(installDir)
                    : throw new PlatformNotSupportedException();

        private static ValueTask UninstallBsipaAsync(string installDir, BeatModsMod bsipa) =>
            OperatingSystem.IsWindows()
                ? UninstallBsipaWindowsAsync(installDir, bsipa)
                : OperatingSystem.IsLinux()
                    ? UninstallBsipaLinuxAsync(installDir, bsipa)
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private static async Task InstallBsipaWindowsAsync(string installDir)
        {
            string winhttpPath = Path.Combine(installDir, Constants.WinHttpDll);
            string bsipaPath = Path.Combine(installDir, Constants.IpaExe);
            if (File.Exists(winhttpPath) || !File.Exists(bsipaPath)) return;
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = installDir,
                Arguments = "-n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        [SupportedOSPlatform("linux")]
        private static async Task InstallBsipaLinux(string installDir)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", Constants.BeatSaberExe });
            Directory.SetCurrentDirectory(oldDir);
            string protonRegPath = Path.Combine($"{installDir}/../..", "compatdata/620980/pfx/user.reg");
            await using FileStream? fileStream = IOUtils.TryOpenFile(protonRegPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, FileOptions.Asynchronous);
            if (fileStream is null) return;
            using StreamReader reader = new(fileStream);
            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            await using StreamWriter streamWriter = new(fileStream);
            if (!content.Contains("[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\""))
                await streamWriter.WriteLineAsync("\n[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\"").ConfigureAwait(false);
        }

        [SupportedOSPlatform("windows")]
        private static async ValueTask UninstallBsipaWindowsAsync(string installDir, BeatModsMod bsipa)
        {
            string bsipaPath = Path.Combine(installDir, Constants.IpaExe);
            if (!File.Exists(bsipaPath))
            {
                RemoveBsipaFiles(installDir, bsipa);
                return;
            }

            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = installDir,
                Arguments = "--revert -n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
            RemoveBsipaFiles(installDir, bsipa);
        }

        [SupportedOSPlatform("linux")]
        private static ValueTask UninstallBsipaLinuxAsync(string installDir, BeatModsMod bsipa)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", Constants.BeatSaberExe });
            Directory.SetCurrentDirectory(oldDir);
            RemoveBsipaFiles(installDir, bsipa);
            return ValueTask.CompletedTask;
        }

        private static void RemoveBsipaFiles(string installDir, BeatModsMod bsipa)
        {
            foreach (BeatModsHash hash in bsipa.Downloads[0].Hashes)
            {
                string fileName = hash.File.Replace("IPA/Data", Constants.BeatSaberDataDir, StringComparison.Ordinal).Replace("IPA/", null);
                string path = Path.Combine(installDir, fileName);
                IOUtils.TryDeleteFile(path);
            }
        }

        private static void RemoveModFiles(string installDir, BeatModsMod mod)
        {
            string pendingDirPath = Path.Combine(installDir, Constants.IpaDir, Constants.PendingDir);
            foreach (BeatModsHash hash in mod.Downloads[0].Hashes)
            {
                string pendingPath = Path.Combine(pendingDirPath, hash.File);
                string normalPath = Path.Combine(installDir, hash.File);
                IOUtils.TryDeleteFile(pendingPath);
                IOUtils.TryDeleteFile(normalPath);
            }
        }
    }
}