using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Guisha.Core
{
    // JsonUtility serializes fields, not Dictionary or interface polymorphism.
    public static class SerializeHelper
    {
        public static string ToJson<T>(T value) => JsonUtility.ToJson(value, true);
        public static T FromJson<T>(string json) => JsonUtility.FromJson<T>(json);
        public static void Save<T>(string path, T value)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(path));
            Directory.CreateDirectory(folder);
            var temp = path + ".tmp";
            File.WriteAllText(temp, ToJson(value), Encoding.UTF8);
            if (File.Exists(path)) File.Copy(path, path + ".bak", true);
            // Backup is retained if replacement is interrupted; TryLoad also reads it.
            File.Copy(temp, path, true);
            File.Delete(temp);
        }
        public static bool TryLoad<T>(string path, out T value) where T : class
        {
            value = null;
            foreach (var candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try { value = FromJson<T>(File.ReadAllText(candidate)); if (value != null) return true; }
                catch (Exception error) { Logger.Warn("Cannot read " + candidate + ": " + error.Message); }
            }
            return false;
        }
    }
}
