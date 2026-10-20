using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Guisha.Core
{
    public static class Logger
    {
        public static bool Enabled { get; set; } = true;
        public static bool WriteFile { get; set; }
        public static string FilePath => Path.Combine(Application.persistentDataPath, "Logs", "game.log");
        public static void Info(string message) => Write("INFO", message, Debug.Log);
        public static void Warn(string message) => Write("WARN", message, Debug.LogWarning);
        public static void Error(string message) => Write("ERROR", message, Debug.LogError);
        private static void Write(string level, string message, Action<object> output)
        {
            if (!Enabled) return;
            var line = $"[{DateTime.UtcNow:O}][{level}] {message}";
            output(line);
            if (!WriteFile) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length > 4 * 1024 * 1024)
                {
                    var backup = FilePath + ".1";
                    if (File.Exists(backup)) File.Delete(backup);
                    File.Move(FilePath, backup);
                }
                File.AppendAllText(FilePath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception error) { WriteFile = false; Debug.LogWarning("File logging disabled: " + error.Message); }
        }
    }
}
