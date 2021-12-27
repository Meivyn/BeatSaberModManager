﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;


namespace BeatSaberModManager.Utilities
{
    public static class IOUtils
    {
        public static void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static void TryCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static void TryDeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static bool TryExtractArchive(ZipArchive archive, string path, bool overrideFiles)
        {
            try
            {
                archive.ExtractToDirectory(path, overrideFiles);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        public static bool TryOpenFile(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, FileOptions options, [MaybeNullWhen(false)] out FileStream fileStream)
        {
            fileStream = null;
            try
            {
                fileStream = new FileStream(path, fileMode, fileAccess, fileShare, 4096, options);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        public static bool TryReadAllText(string path, [MaybeNullWhen(false)] out string text)
        {
            text = null;
            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }
    }
}