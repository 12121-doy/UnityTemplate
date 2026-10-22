using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.IO; // 引入 System.IO 命名空间，使用其中的类型与 API。
using System.Text; // 引入 System.Text 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    // JsonUtility 序列化字段，不直接支持 Dictionary 或接口多态。
    /// <summary>
    /// 使用 JsonUtility 序列化配置与存档，提供临时文件和上一版备份。
    /// </summary>
    public static class SerializeHelper // 使用 JsonUtility 序列化配置与存档，提供临时文件和上一版备份。
    { // 开始类型。
        /// <summary>
        /// 使用 JsonUtility.ToJson 生成带缩进的 JSON。
        /// </summary>
        /// <typeparam name="T">JsonUtility 可处理的数据类型；采用字段序列化。</typeparam>
        /// <param name="value">当前处理值。</param>
        /// <returns>JSON 文本。</returns>
        public static string ToJson<T>(T value) => JsonUtility.ToJson(value, true); // 使用 JsonUtility.ToJson 生成带缩进的 JSON。
        /// <summary>
        /// 使用 JsonUtility.FromJson&lt;T&gt; 从 JSON 读取字段数据。
        /// </summary>
        /// <typeparam name="T">JsonUtility 可处理的数据类型；采用字段序列化。</typeparam>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的数据对象。</returns>
        public static T FromJson<T>(string json) => JsonUtility.FromJson<T>(json); // 使用 JsonUtility.FromJson<T> 从 JSON 读取字段数据。
        /// <summary>
        /// 写临时文件，保留已有文件为 .bak，再复制覆盖目标文件。
        /// </summary>
        /// <typeparam name="T">JsonUtility 可处理的数据类型；采用字段序列化。</typeparam>
        /// <param name="path">文件或资源路径。</param>
        /// <param name="value">当前处理值。</param>
        public static void Save<T>(string path, T value) // 写临时文件，保留已有文件为 .bak，再复制覆盖目标文件。
        { // 开始方法。
            var folder = Path.GetDirectoryName(Path.GetFullPath(path)); // 把 Path.GetDirectoryName 的返回值 保存为 folder。
            Directory.CreateDirectory(folder); // 确保目标目录存在。
            var temp = path + ".tmp"; // 把 path + ".tmp" 保存为 temp。
            File.WriteAllText(temp, ToJson(value), Encoding.UTF8); // 将文本写入指定文件。
            if (File.Exists(path)) // 当条件 File.Exists(path) 成立时，执行下方分支。
                File.Copy(path, path + ".bak", true); // 复制文件；覆盖行为由参数决定。
            // 覆盖中断时仍保留备份；TryLoad 也会尝试从备份恢复。
            File.Copy(temp, path, true); // 复制文件；覆盖行为由参数决定。
            File.Delete(temp); // 删除旧临时/备份日志文件，为本次保存或日志轮换腾出位置。
        } // 结束方法。

        /// <summary>
        /// 依次读取主文件和 .bak；成功返回 true 和对象，读取/解析失败记录警告。
        /// </summary>
        /// <typeparam name="T">JsonUtility 可处理的数据类型；采用字段序列化。</typeparam>
        /// <param name="path">文件或资源路径。</param>
        /// <param name="value">输出反序列化得到的对象；主文件失败时尝试 .bak。</param>
        /// <returns>true 表示主文件或备份已经成功读取。</returns>
        public static bool TryLoad<T>(string path, out T value) // 依次读取主文件和 .bak；成功返回 true 和对象，读取/解析失败记录警告。
            where T : class // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            value = null; // 清空 当前处理值 引用，表示当前不再持有该对象。
            foreach (var candidate in new[] // 遍历 new[] {     path,     path + ".bak" }，每次把一个元素保存为 candidate。
            { // 开始成员/集合初始化。
                path, // 提供初始化/参数元素 path，交给上方的对象或集合使用。
                path + ".bak" // 提供初始化/参数元素 path + ".bak"，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
            { // 开始遍历循环。
                if (!File.Exists(candidate)) // 当条件 File.Exists(candidate) 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                try // 进入异常保护范围；失败时由 catch/finally 配对清理。
                { // 开始受异常保护的操作。
                    value = FromJson<T>(File.ReadAllText(candidate)); // 更新 当前处理值，使用 FromJson<T> 的返回值。
                    if (value != null) // 当当前处理值不为空时，执行下方分支。
                        return true; // 返回 true，将当前分支的布尔结果交给调用方。
                } // 结束受异常保护的操作。
                catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
                { // 开始异常处理。
                    Logger.Warn("Cannot read " + candidate + ": " + error.Message); // 按日志开关记录警告。
                } // 结束异常处理。
            } // 结束遍历循环。

            return false; // 返回 false，将当前分支的布尔结果交给调用方。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
