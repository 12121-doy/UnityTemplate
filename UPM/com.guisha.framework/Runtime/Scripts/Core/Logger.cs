using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.IO; // 引入 System.IO 命名空间，使用其中的类型与 API。
using System.Text; // 引入 System.Text 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 统一日志开关、等级、本地文件写入与轮换。
    /// </summary>
    public static class Logger // 统一日志开关、等级、本地文件写入与轮换。
    { // 开始类型。
        /// <summary>
        /// 获取或设置日志总开关。
        /// </summary>
        public static bool Enabled { get; set; } = true; // 获取或设置日志总开关。
        /// <summary>
        /// 获取或设置是否写入本地日志。
        /// </summary>
        public static bool WriteFile { get; set; } // 获取或设置是否写入本地日志。
        /// <summary>
        /// 获取本地日志文件路径。
        /// </summary>
        public static string FilePath => Path.Combine(Application.persistentDataPath, "Logs", "game.log"); // 获取本地日志文件路径。

        /// <summary>
        /// 按 INFO 级别输出；启用 WriteFile 时同步写入本地日志。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public static void Info(string message) => Write("INFO", message, Debug.Log); // 按 INFO 级别输出；启用 WriteFile 时同步写入本地日志。
        /// <summary>
        /// 按 WARN 级别输出警告，并按开关写文件。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public static void Warn(string message) => Write("WARN", message, Debug.LogWarning); // 按 WARN 级别输出警告，并按开关写文件。
        /// <summary>
        /// 按 ERROR 级别输出错误，并按开关写文件。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public static void Error(string message) => Write("ERROR", message, Debug.LogError); // 按 ERROR 级别输出错误，并按开关写文件。
        /// <summary>
        /// 组织 UTC 日志行、执行输出委托、轮换大于 4 MiB 的日志；文件错误时关闭文件日志。
        /// </summary>
        /// <param name="level">日志级别。</param>
        /// <param name="message">消息或事件载荷。</param>
        /// <param name="output">输出委托；由调用方注入具体行为。</param>
        private static void Write(string level, string message, Action<object> output) // 组织 UTC 日志行、执行输出委托、轮换大于 4 MiB 的日志；文件错误时关闭文件日志。
        { // 开始方法。
            if (!Enabled) // 当条件 Enabled 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var line = $"[{DateTime.UtcNow:O}][{level}] {message}"; // 把 $"[{DateTime.UtcNow:O}][{level}] {message}" 保存为 line。
            output(line); // 执行注入的输出委托，使用参数 line。
            if (!WriteFile) // 当条件 WriteFile 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)); // 确保目标目录存在。
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length > 4 * 1024 * 1024) // 当（条件 File.Exists(FilePath) 成立；且条件 new FileInfo(FilePath).Length > 4 * 1024 * 1024 成立）时，执行下方分支。
                { // 开始条件分支。
                    var backup = FilePath + ".1"; // 把 FilePath + ".1" 保存为 backup。
                    if (File.Exists(backup)) // 当条件 File.Exists(backup) 成立时，执行下方分支。
                        File.Delete(backup); // 删除旧临时/备份日志文件，为本次保存或日志轮换腾出位置。
                    File.Move(FilePath, backup); // 把当前日志移为上一份备份文件，开始新的日志文件。
                } // 结束条件分支。

                File.AppendAllText(FilePath, line + Environment.NewLine, Encoding.UTF8); // 把当前文本追加到文件，持久保存结果。
            } // 结束受异常保护的操作。
            catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                WriteFile = false; // 更新 是否写入本地日志，使用 false（关闭/不满足）。
                Debug.LogWarning("File logging disabled: " + error.Message); // 向 Unity Console 输出警告。
            } // 结束异常处理。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
