using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using LabelVoice.Util;
using Serilog;

namespace Labelvoice
{

    public class PathManager : SingletonBase<PathManager>
    {
        public PathManager()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (LabelVoice.OS.IsMacOS())
            {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                DataPath = Path.Combine(userHome, "Library", "OpenUtau");
                CachePath = Path.Combine(userHome, "Library", "Caches", "OpenUtau");
                HomePathIsAscii = true;
                try
                {
                    // Deletes old cache.
                    string oldCache = Path.Combine(DataPath, "Cache");
                    if (Directory.Exists(oldCache))
                    {
                        Directory.Delete(oldCache, true);
                    }
                }
                finally { }
            }
            else if (LabelVoice.OS.IsLinux())
            {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(dataHome))
                {
                    dataHome = Path.Combine(userHome, ".local", "share");
                }
                DataPath = Path.Combine(dataHome, "OpenUtau");
                string cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
                if (string.IsNullOrEmpty(cacheHome))
                {
                    cacheHome = Path.Combine(userHome, ".cache");
                }
                CachePath = Path.Combine(cacheHome, "OpenUtau");
                HomePathIsAscii = true;
            }
            else
            {
                DataPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                CachePath = Path.Combine(DataPath, "Cache");
                HomePathIsAscii = true;
                var etor = StringInfo.GetTextElementEnumerator(DataPath);
                while (etor.MoveNext())
                {
                    string s = etor.GetTextElement();
                    if (s.Length != 1 || s[0] >= 128)
                    {
                        HomePathIsAscii = false;
                        break;
                    }
                }
            }
            Log.Logger.Information($"Data path = {DataPath}");
            Log.Logger.Information($"Cache path = {CachePath}");
        }

        public string RootPath { get; private set; }
        public string DataPath { get; private set; }
        public string CachePath { get; private set; }
        public bool HomePathIsAscii { get; private set; }
        public string PrefsFilePath => Path.Combine(DataPath, "prefs.json");

        public string GetPartSavePath(string projectPath, int partNo)
        {
            var name = Path.GetFileNameWithoutExtension(projectPath);
            var dir = Path.GetDirectoryName(projectPath);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{name}-{partNo:D2}.ust");
        }

        public string GetExportPath(string exportPath, int trackNo)
        {
            var name = Path.GetFileNameWithoutExtension(exportPath);
            var dir = Path.GetDirectoryName(exportPath);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{name}-{trackNo:D2}.wav");
        }

        public void ClearCache()
        {
            var files = Directory.GetFiles(CachePath);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Failed to delete file {file}");
                }
            }
            var dirs = Directory.GetDirectories(CachePath);
            foreach (var dir in dirs)
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Failed to delete dir {dir}");
                }
            }
        }

        readonly static string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        public string GetCacheSize()
        {
            if (!Directory.Exists(CachePath))
            {
                return "0B";
            }
            var dir = new DirectoryInfo(CachePath);
            double size = dir.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##}{sizes[order]}";
        }
    }
}
